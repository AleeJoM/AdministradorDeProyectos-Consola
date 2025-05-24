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
            IUserRoleService userRoleService
            )
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
            var existingSteps = await _projectApprovalStepQuery.GetStepsByProjectId(projectId);
            if (existingSteps != null && existingSteps.Any())
            {
                Console.WriteLine("Ya existen pasos de aprobación para este proyecto.");
                return existingSteps.ToList();
            }

            var proposal = await _projectProposalQuery.GetProjectById(projectId);
            if (proposal == null)
                throw new Exception("No se encontró la propuesta con el ID proporcionado.");

            var rules = await _approvalRuleQuery.GetAll();
            var users = await _userQuery.GetAllUsers();

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

                var selectedRule =
                    group.FirstOrDefault(r => r.Area == area && r.Type == type) ??
                    group.FirstOrDefault(r => r.Area == area && r.Type == null) ??
                    group.FirstOrDefault(r => r.Type == type && r.Area == null) ??
                    group.FirstOrDefault(r => r.Area == null && r.Type == null);

                if (selectedRule == null)
                    continue;

                var approvers = users
                    .Where(u => u.Role == selectedRule.ApproverRoleId)
                    .ToList();

                Console.WriteLine($"\nReglas de aprobación: {stepOrder}");

                if (selectedRule != null)
                {
                    string areaName = "Todas";
                    string typeName = "Todos";

                    if (selectedRule.Area.HasValue)
                    {
                        var areaEntity = selectedRule.Areas;
                        if (areaEntity != null)
                            areaName = areaEntity.Name;
                        else
                            areaName = selectedRule.Area.Value.ToString();
                    }

                    if (selectedRule.Type.HasValue)
                    {
                        var typeEntity = selectedRule.ProjectType;
                        if (typeEntity != null)
                            typeName = typeEntity.Name;
                        else
                            typeName = selectedRule.Type.Value.ToString();
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Regla aplicada: Id {selectedRule.Id} | Área: {areaName} | Tipo: {typeName} | Rol: {selectedRule.ApproverRoleId}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠️ No se encontró regla válida para este StepOrder.");
                    Console.ResetColor();
                }

                if (approvers.Any())
                {
                    foreach (var user in approvers)
                    {
                        steps.Add(new ProjectApprovalStep
                        {
                            Id = GenerateRandomBigIntId(),
                            ProjectProposalId = proposal.Id,
                            ApproverRoleId = selectedRule.ApproverRoleId,
                            ApproverUserId = user.Id,
                            Status = 1,
                            StepOrder = selectedRule.StepOrder,
                            Observations = "Pendiente"
                        });
                        Console.WriteLine($"Paso agregado: StepOrder {stepOrder}, Usuario asignado: {user?.Name ?? "ninguno"}");
                    }
                }
                else
                {
                    steps.Add(new ProjectApprovalStep
                    {
                        Id = GenerateRandomBigIntId(),
                        ProjectProposalId = proposal.Id,
                        ApproverRoleId = selectedRule.ApproverRoleId,
                        ApproverUserId = null,
                        Status = 1,
                        StepOrder = selectedRule.StepOrder,
                        Observations = "Pendiente (sin usuario asignado)"
                    });
                }
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
            string roleName = (await _userRoleService.GetRoleById(firstStep.ApproverRoleId)).ToString();
            return new ProjectStatusDto
            {
                UserId = assignedUserId ?? 0,
                UserName = userName,
                Decision = "Pendiente de aprobación",
                Message = $"La propuesta requiere aprobación de {roleName}"
            };
        }
        public async Task UpdateObservation(BigInteger stepId, string decision, string userName)
        {
            var step = await _projectApprovalStepCommand.GetProjectStepById(stepId);
            if (step != null)
            {
                step.Observations = $"Paso iniciado el {DateTime.Now:dd/MM/yyyy HH:mm}. Estado: {decision}. Responsable(s): {userName}";
                await _projectApprovalStepCommand.UpdateStep(step);
            }
        }
        public async Task<string> GetObservation(BigInteger stepId)
        {
            var observations = await _projectApprovalStepCommand.GetProjectStepObservationsById(stepId);
            if (observations == null)
                Console.WriteLine("Paso de aprobación no encontrado");

            return observations ?? "Sin observaciones registradas.";
        }
        public async Task ProcessProposalStep(Guid proposalId, int userId, char decision)
        {
            var step = await _projectApprovalStepQuery.GetStepByProposalAndUser(proposalId, userId);
            var allSteps = await _projectApprovalStepQuery.GetStepsByProjectId(proposalId);

            if (step == null)
            {
                step = allSteps.FirstOrDefault(s => s.Status == 4 && s.ApproverUserId == userId);
            }
            if (step == null || (step.Status != 1 && step.Status != 4))
            {
                throw new Exception("No tiene permisos para aprobar esta propuesta o ya fue procesada.");
            }

            switch (char.ToUpper(decision))
            {
                case 'A':
                    step.Status = 2;
                    break;
                case 'R':
                    step.Status = 3;
                    break;
                case 'O':
                    step.Status = 4;
                    break;
                default:
                    throw new ArgumentException("Opción inválida.");
            }

            step.DecisionDate = DateTime.Now;
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
