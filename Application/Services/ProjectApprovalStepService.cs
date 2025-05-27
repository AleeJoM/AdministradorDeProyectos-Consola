using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Application.Exceptions;
using Application.Validation;

namespace Application.Services
{
    public class ProjectApprovalStepService : IProjectApprovalStepService
    {
        private readonly IProjectProposalQuery _projectProposalQuery;
        private readonly IProjectProposalCommand _projectProposalCommand;
        private readonly IProjectApprovalStepCommand _projectApprovalStepCommand;
        private readonly IUserQuery _userQuery;
        private readonly IApprovalRuleQuery _approvalRuleQuery;
        private readonly IProjectApprovalStepQuery _projectApprovalStepQuery;
        private readonly IUserRoleService _userRoleService;
        public ProjectApprovalStepService(
            IProjectProposalQuery projectProposalQuery,
            IProjectApprovalStepCommand projectApprovalStepCommand,
            IUserQuery userQuery,
            IApprovalRuleQuery approvalRuleQuery,
            IProjectApprovalStepQuery projectApprovalStepQuery,
            IProjectProposalCommand projectProposalCommand,
            IUserRoleService userRoleService)
        {
            _projectProposalQuery = projectProposalQuery;
            _projectApprovalStepCommand = projectApprovalStepCommand;
            _userQuery = userQuery;
            _approvalRuleQuery = approvalRuleQuery;
            _projectApprovalStepQuery = projectApprovalStepQuery;
            _projectProposalCommand = projectProposalCommand;
            _userRoleService = userRoleService;
        }
        public async Task<List<ProjectApprovalStep>> GenerateApprovalSteps(Guid projectId)
        {
            var proposal = await _projectProposalQuery.GetProjectById(projectId);
            if (proposal == null)
                throw new BusinessException("No se encontró la propuesta con el ID proporcionado.");

            var existingSteps = await _projectApprovalStepQuery.GetStepsByProjectId(projectId);
            if (existingSteps != null && existingSteps.Any())
            {
                return existingSteps.ToList();
            }

            var rules = await _approvalRuleQuery.GetAll();
            var statusPendiente = await _projectApprovalStepCommand.GetApprovalStatusById(1);

            decimal amount = proposal.EstimatedAmount;
            int? area = proposal.Area;
            int? type = proposal.Type;

            bool InRange(decimal value, decimal min, decimal max) =>
                value >= min && (max <= 0 || value <= max);

            var steps = new List<ProjectApprovalStep>();
            var rulesGrouped = rules
                .Where(r => InRange(amount, r.MinAmount, r.MaxAmount))
                .GroupBy(r => r.StepOrder)
                .OrderBy(g => g.Key);

            foreach (var group in rulesGrouped)
            {
                var stepOrder = group.Key;

                var selectedRule = group.FirstOrDefault(r => r.Area == area && r.Type == type) ??
                                   group.FirstOrDefault(r => r.Area == area && r.Type == null) ??
                                   group.FirstOrDefault(r => r.Type == type && r.Area == null) ??
                                   group.FirstOrDefault(r => r.Area == null && r.Type == null);

                if (selectedRule == null) continue;

                var usersWithRole = await _userQuery.GetUsersByRoleId(selectedRule.ApproverRoleId);
                int? approverUserId = null;
                User? approverUser = null;

                if (usersWithRole != null)
                {
                    var userList = usersWithRole.ToList();
                    if (userList.Count == 1)
                    {
                        approverUserId = userList[0].Id;
                        approverUser = userList[0];
                    }
                }

                var mainStep = new ProjectApprovalStep
                {
                    Id = GenerateRandomBigIntId(),
                    ProjectProposalId = proposal.Id,
                    ProjectProposal = proposal,
                    ApproverRoleId = selectedRule.ApproverRoleId,
                    ApproverRole = selectedRule.ApproverRole,
                    ApproverUserId = approverUserId,
                    User = approverUser,
                    Status = 1,
                    ApprovalStatus = statusPendiente,
                    StepOrder = stepOrder,
                    Observations = "Pendiente de revisión",
                    DecisionDate = null
                };
                steps.Add(mainStep);
            }

            await _projectApprovalStepCommand.SaveSteps(steps);
            return steps.OrderBy(s => s.StepOrder).ToList();
        }
        private long GenerateRandomBigIntId()
        {
            var buffer = new byte[8];
            RandomNumberGenerator.Fill(buffer);
            long value = BitConverter.ToInt64(buffer, 0);
            return Math.Abs(value);
        }
        public async Task<ProjectStatusDto> NotifyUsers(Guid projectId)
        {
            var project = await _projectProposalQuery.GetProjectById(projectId);
            if (project == null)
            {
                return new ProjectStatusDto
                {
                    UserId = 0,
                    UserName = "N/A",
                    Decision = "Sin decisión",
                    Message = "Proyecto no encontrado"
                };
            }

            var steps = await _projectApprovalStepQuery.GetStepsByProjectId(projectId);
            var firstStep = steps.FirstOrDefault();
            if (firstStep == null)
            {
                return new ProjectStatusDto
                {
                    UserId = 0,
                    UserName = "N/A",
                    Decision = "Sin decisión",
                    Message = "No se encontraron pasos de aprobación"
                };
            }

            var assignedUserId = firstStep.ApproverUserId;
            string userName = "No asignado";

            if (assignedUserId.HasValue)
            {
                var user = await _userQuery.GetById(assignedUserId.Value);
                if (user != null)
                {
                    userName = user.Name;
                }
            }
            else
            {
                var usersWithRole = await _userQuery.GetUsersByRoleId(firstStep.ApproverRoleId);
                if (usersWithRole != null && usersWithRole.Any())
                {
                    userName = string.Join(", ", usersWithRole.Select(u => u.Name));
                }
            }

            var roleDetails = await _userRoleService.GetRoleDetailsById(firstStep.ApproverRoleId);

            return new ProjectStatusDto
            {
                UserId = assignedUserId ?? 0,
                UserName = userName,
                Decision = "Pendiente de aprobación",
                Message = $"La propuesta requiere aprobación de {roleDetails.Name}"
            };
        }
        public async Task UpdateObservation(BigInteger stepId, string decision, string userName)
        {
            var step = await _projectApprovalStepCommand.GetProjectStepById(stepId);
            if (step != null)
            {
                var assignedUser = await _userQuery.GetById(step.ApproverUserId ?? 0);
                var userRoleId = await _userRoleService.GetRoleById(assignedUser?.Id ?? 0);
                var roleDetails = await _userRoleService.GetRoleDetailsById(userRoleId);

                step.Observations = $"Paso iniciado el {DateTime.Now:dd/MM/yyyy HH:mm}. " +
                                  $"Estado: {decision}. " +
                                  $"Responsable(s): {assignedUser?.Name ?? "No asignado"} " +
                                  $"(Rol: {roleDetails?.Name ?? "Desconocido"})";

                await _projectApprovalStepCommand.UpdateStep(step);
            }
        }
        public async Task<string> GetObservation(BigInteger stepId)
        {
            var observations = await _projectApprovalStepCommand.GetProjectStepObservationsById(stepId);
            if (observations == null)
                throw new BusinessException("Paso de aprobación no encontrado");

            return observations ?? "Sin observaciones registradas.";
        }
        public async Task ProcessProposalStep(Guid proposalId, int userId, char decision)
        {
            var step = await _projectApprovalStepQuery.GetStepByProposalAndUser(proposalId, userId);
            var allSteps = await _projectApprovalStepQuery.GetStepsByProjectId(proposalId);
            var currentUser = await _userQuery.GetById(userId);

            if (step == null)
            {
                var processedStep = allSteps.FirstOrDefault(s =>
                    s.ApproverRoleId == currentUser?.Role &&
                    (s.Status == 2 || s.Status == 3));

                if (processedStep != null)
                    throw new BusinessException($"Este paso ya fue procesado por {processedStep.User?.Name ?? "otro usuario"} con el mismo rol.");

                throw new BusinessException("No tiene permisos para procesar este paso o ya fue procesado.");
            }

            switch (char.ToUpper(decision))
            {
                case 'A':
                    step.Status = 2; // Aprobado
                    step.DecisionDate = DateTime.Now;
                    step.ApproverUserId = userId;
                    step.Observations = $"Aprobado por {currentUser?.Name}";

                    var relatedSteps = allSteps.Where(s =>
                        s.StepOrder == step.StepOrder &&
                        s.ApproverRoleId == step.ApproverRoleId &&
                        s.Id != step.Id).ToList();

                    foreach (var relatedStep in relatedSteps)
                    {
                        relatedStep.Status = 2;
                        relatedStep.DecisionDate = DateTime.Now;
                        relatedStep.Observations = $"Este paso fue aprobado automáticamente porque {step.User?.Name} del mismo rol ('{step.ApproverRole?.Name}') lo aprobó.";
                        await _projectApprovalStepCommand.UpdateStep(relatedStep);
                    }
                    break;

                case 'R':
                    step.Status = 3; // Rechazado
                    step.DecisionDate = DateTime.Now;
                    step.ApproverUserId = userId;
                    step.Observations = $"Rechazado por {currentUser?.Name}";

                    var rejectedRelatedSteps = allSteps.Where(s =>
                        s.StepOrder == step.StepOrder &&
                        s.ApproverRoleId == step.ApproverRoleId &&
                        s.Id != step.Id).ToList();

                    foreach (var relatedStep in rejectedRelatedSteps)
                    {
                        relatedStep.Status = 3;
                        relatedStep.DecisionDate = DateTime.Now;
                        relatedStep.Observations = $"Rechazado automáticamente. El paso fue rechazado por {step.User?.Name ?? "otro usuario"} del mismo rol.";
                        await _projectApprovalStepCommand.UpdateStep(relatedStep);
                    }
                    break;

                case 'O':
                    step.Status = 4; // Observado
                    step.DecisionDate = DateTime.Now;
                    step.ApproverUserId = userId;
                    step.Observations = $"Observado por {currentUser?.Name}";

                    var observedRelatedSteps = allSteps.Where(s =>
                        s.StepOrder == step.StepOrder &&
                        s.ApproverRoleId == step.ApproverRoleId &&
                        s.Id != step.Id).ToList();

                    foreach (var relatedStep in observedRelatedSteps)
                    {
                        relatedStep.Status = 4;
                        relatedStep.DecisionDate = DateTime.Now;
                        relatedStep.Observations = $"Observado automáticamente. El paso fue observado por {step.User?.Name ?? "otro usuario"} del mismo rol.";
                        await _projectApprovalStepCommand.UpdateStep(relatedStep);
                    }
                    break;

                default:
                    throw new MyInvalidDataException("Opción inválida.");
            }

            await _projectApprovalStepCommand.UpdateStep(step);

            if (allSteps.Any(s => s.Status == 3))
            {
                await _projectProposalCommand.UpdateProposalStatus(proposalId, "Rechazado");
            }
            else if (allSteps.All(s => s.Status == 2))
            {
                await _projectProposalCommand.UpdateProposalStatus(proposalId, "Aprobado");
            }
            else if (step.Status == 4)
            {
                await _projectProposalCommand.UpdateProposalStatus(proposalId, "Revisión");
            }
        }
    }
}
