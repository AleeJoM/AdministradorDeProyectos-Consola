using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;

namespace Application.Services
{
    public class ProjectProposalService : IProjectProposalService
    {
        private readonly IProjectProposalCommand _projectProposalCommand;
        private readonly IProjectProposalMapper _mapper;
        private readonly IProjectApprovalStepService _projectApprovalStepService;
        private readonly IProjectApprovalStepQuery _projectApprovalStepQuery;
        private readonly IProjectProposalQuery _projectProposalQuery;
        private readonly IAreaQuery _areaQuery;
        private readonly IProjectTypeQuery _projectTypeQuery;
        private readonly IUserQuery _userQuery;
        private readonly IUserRoleService _userRoleService;
        public ProjectProposalService(IProjectProposalCommand projectProposalCommand,
            IProjectProposalMapper mapper,
            IProjectApprovalStepService projectApprovalStepService,
            IProjectApprovalStepQuery projectApprovalStepQuery,
            IAreaQuery areaQuery,
            IProjectTypeQuery projectTypeQuery,
            IProjectProposalQuery projectProposalQuery,
            IUserQuery userQuery,
            IUserRoleService userRoleService
            )
        {
            _projectProposalCommand = projectProposalCommand;
            _mapper = mapper;
            _projectApprovalStepService = projectApprovalStepService;
            _projectApprovalStepQuery = projectApprovalStepQuery;
            _areaQuery = areaQuery;
            _projectTypeQuery = projectTypeQuery;
            _projectProposalQuery = projectProposalQuery;
            _userQuery = userQuery;
            _userRoleService = userRoleService;
        }
        public async Task<ProjectProposalResponse> InsertProject(ProjectProposalRequest projectResquest, int userId)
        {
            var projectProposal = new ProjectProposal
            {
                Id = Guid.NewGuid(),
                Title = projectResquest.Title,
                Description = projectResquest.Description,
                EstimatedAmount = projectResquest.EstimatedAmount,
                EstimatedDuration = projectResquest.EstimatedDuration,
                CreateAt = DateTime.UtcNow,
                Area = projectResquest.Area,
                Type = projectResquest.Type,
                Status = 1,
                CreateBy = userId
            };

            await _projectProposalCommand.CreateProposal(projectProposal);
            var check = await _projectProposalQuery.GetProjectById(projectProposal.Id);
            if (check == null)
            {
                throw new Exception("No se pudo recuperar la propuesta recién creada. Verificá que se haya guardado correctamente.");
            }

            var approvalSteps = await _projectApprovalStepService.GenerateApprovalSteps(projectProposal.Id);
            var firstStep = approvalSteps.FirstOrDefault();

            var observations = await _projectApprovalStepService.NotifyUsers(projectProposal.Id);

            await _projectApprovalStepService.UpdateObservation(firstStep.Id, observations.Decision, observations.UserName);
            await _projectProposalCommand.FinalizeProposal(projectProposal.Id);

            var response = await _mapper.GetProjectProposalResponse(projectProposal);

            return response;
        }
        public async Task<ProjectProposalResponse> UpdateProject(ProjectProposal projectId)
        {
            var newProject = await _projectProposalCommand.UpdateProposal(projectId);
            var response = await _mapper.GetProjectProposalResponse(newProject);
            return response;
        }
        public async Task<ProjectStatusResponse> GetProjectStatus(Guid projectId)
        {
            var project = await _projectProposalQuery.GetProjectById(projectId);
            if (project == null)
            {
                throw new Exception("No se encontró la propuesta solicitada.");
            }

            var createdBy = await _userQuery.GetById(project.CreateBy);

            var steps = await _projectApprovalStepQuery.GetStepsByProjectId(projectId);

            var orderedSteps = steps.OrderBy(s => s.StepOrder).ThenBy(s => s.DecisionDate).ToList();

            var historyBuilder = new StringBuilder();

            var stepsGroupedByOrder = orderedSteps
                .GroupBy(s => s.StepOrder)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var group in stepsGroupedByOrder.OrderBy(g => g.Key))
            {
                var stepOrder = group.Key;
                var stepsInGroup = group.Value;

                var completedSteps = stepsInGroup.Where(s => s.Status != 1).ToList();

                if (completedSteps.Any())
                {
                    foreach (var step in completedSteps)
                    {
                        string userName = "No asignado";
                        if (step.ApproverUserId.HasValue)
                        {
                            var user = await _userQuery.GetById(step.ApproverUserId.Value);
                            if (user != null)
                            {
                                userName = user.Name;
                            }
                        }

                        string statusText;
                        switch (step.Status)
                        {
                            case 1:
                                statusText = "Pendiente";
                                break;
                            case 2:
                                statusText = "Aprobado";
                                break;
                            case 3:
                                statusText = "Rechazado";
                                break;
                            case 4:
                                statusText = "Observado";
                                break;
                            default:
                                statusText = "Desconocido";
                                break;
                        }

                        historyBuilder.AppendLine($"Paso {step.StepOrder}: {statusText}");
                        historyBuilder.AppendLine($"  - Decisión por: {userName}");

                        if (step.DecisionDate.HasValue)
                        {
                            historyBuilder.AppendLine($"  - Fecha: {step.DecisionDate.Value:dd/MM/yyyy HH:mm}");
                        }

                        if (!string.IsNullOrEmpty(step.Observations))
                        {
                            historyBuilder.AppendLine($"  - Observaciones: {step.Observations}");
                        }

                        historyBuilder.AppendLine();
                    }
                }
                else
                {
                    var pendingStep = stepsInGroup.First();
                    int approversCount = stepsInGroup.Count;
                    string statusText = "Pendiente";
                    historyBuilder.AppendLine($"Paso {pendingStep.StepOrder} ({approversCount}): {statusText}");
                }
            }

            string currentStatus = DetermineCurrentStatus(orderedSteps);

            return new ProjectStatusResponse
            {
                Title = project.Title,
                Description = project.Description,
                Area = await _areaQuery.GetAreaNameById(project.Area),
                Type = (await _projectTypeQuery.GetById(project.Type))?.Name ?? "Desconocido",
                CurrentStatus = currentStatus,
                CreatedBy = createdBy?.Name ?? "Desconocido",
                History = historyBuilder.ToString()
            };
        }
        private string DetermineCurrentStatus(List<ProjectApprovalStep> steps)
        {
            if (steps.Any(s => s.Status == 3))
            {
                return "Rechazado";
            }
            if (steps.Any(s => s.Status == 1 || s.Status == 4))
            {
                return "En revisión";
            }
            if (steps.All(s => s.Status == 2))
            {
                return "Aprobado";
            }
            return "En revisión";
        }
        private string GetStatusText(int status)
        {
            switch (status)
            {
                case 1: return "Borrador";
                case 2: return "En revisión";
                case 3: return "Aprobado";
                case 4: return "Rechazado";
                case 5: return "Observado";
                default: return "Desconocido";
            }
        }
        public async Task<List<ProjectProposal>> GetAllProjects(int pageNumber, int pageSize)
        {
            return await _projectProposalQuery.GetAllProjectProposal(pageNumber, pageSize);
        }
        public async Task<int> GetTotalProjectCount()
        {
            return await _projectProposalQuery.GetTotalProjectCount();
        }
    }
}
