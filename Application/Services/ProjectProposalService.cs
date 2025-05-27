using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Application.Mapper;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Application.Exceptions;
using Application.Validation;

namespace Application.Services
{
    public class ProjectProposalService : IProjectProposalService
    {
        private readonly IProjectProposalQuery _projectProposalQuery;
        private readonly IProjectApprovalStepQuery _projectApprovalStepQuery;
        private readonly IProjectApprovalStepService _projectApprovalStepService;
        private readonly IProjectApprovalStepCommand _projectApprovalStepCommand;
        private readonly IProjectProposalCommand _projectProposalCommand;
        private readonly IUserQuery _userQuery;
        private readonly IUserRoleService _userRoleService;
        private readonly IProjectProposalMapper _projectProposalMapper;

        public ProjectProposalService(
            IProjectProposalQuery projectProposalQuery,
            IProjectApprovalStepQuery projectApprovalStepQuery,
            IProjectApprovalStepService projectApprovalStepService,
            IProjectApprovalStepCommand projectApprovalStepCommand,
            IProjectProposalCommand projectProposalCommand,
            IUserQuery userQuery,
            IUserRoleService userRoleService,
            IProjectProposalMapper projectProposalMapper)
        {
            _projectProposalQuery = projectProposalQuery;
            _projectApprovalStepQuery = projectApprovalStepQuery;
            _projectApprovalStepService = projectApprovalStepService;
            _projectApprovalStepCommand = projectApprovalStepCommand;
            _projectProposalCommand = projectProposalCommand;
            _userQuery = userQuery;
            _userRoleService = userRoleService;
            _projectProposalMapper = projectProposalMapper;
        }
        public async Task<ProjectProposalCreateResponseDto> ProcessDecision(Guid projectId, DecisionStepDto request)
        {
            var proposal = await _projectProposalQuery.GetProjectById(projectId);
            if (proposal == null)
                throw new BusinessException("Proyecto no encontrado");

            if (proposal.Status != 4)
                throw new BusinessException("El proyecto solo puede modificarse si está en estado de observación");

            var allSteps = await _projectApprovalStepQuery.GetStepsByProjectId(projectId);
            if (allSteps == null || !allSteps.Any())
                throw new Exception("No hay pasos de aprobación para este proyecto.");

            var currentStep = allSteps.FirstOrDefault(s => s.Id == request.Id);
            if (currentStep == null)
                throw new BusinessException("Paso de aprobación no encontrado");

            var validationRequest = new ValidateUserRoleRequestDto
            {
                UserId = request.User,
                ApproverRoleId = currentStep.ApproverRoleId,
                CurrentApproverUserId = null
            };

            var validationResult = await _userRoleService.ValidateUserRoleForStep(validationRequest);
            if (!validationResult.IsValid)
                throw new BusinessException("No tiene el rol permitido para aprobar este paso");

            if (currentStep.Status != 1 && currentStep.Status != 4)
                throw new BusinessException("El paso ya no se encuentra en un estado que permite modificaciones");

            var previousStep = allSteps
                 .Where(s => s.StepOrder < currentStep.StepOrder && s.Status != 3)
                 .OrderByDescending(s => s.StepOrder)
                 .FirstOrDefault();

            if (previousStep != null && previousStep.Status != 2)
                throw new BusinessException("El paso anterior debe estar aprobado.");

            await UpdateStepStatus(currentStep, request, allSteps);
            var updatedProposal = await UpdateProposalStatus(proposal, currentStep, allSteps);

            return await MapToResponseDto(updatedProposal);
        }
        private async Task UpdateStepStatus(ProjectApprovalStep step, DecisionStepDto request, List<ProjectApprovalStep> allSteps)
        {
            step.Status = request.Status;
            step.Observations = request.Observation;
            step.DecisionDate = DateTime.UtcNow;
            step.ApproverUserId = request.User;

            await _projectApprovalStepCommand.UpdateStep(step);
        }
        private async Task<ProjectProposal> UpdateProposalStatus(
            ProjectProposal proposal, 
            ProjectApprovalStep currentStep, 
            List<ProjectApprovalStep> allSteps
            )
        {
            if (allSteps.Any(s => s.Status == 3))
            {
                proposal.Status = 3; // Rechazado
            }
            else if (allSteps.All(s => s.Status == 2))
            {
                proposal.Status = 2; // Aprobado
            }
            else if (currentStep.Status == 4)
            {
                proposal.Status = 4; // Observado
            }
            else
            {
                proposal.Status = 1; // En proceso
            }

            var updatedProposal = await _projectProposalCommand.UpdateProposal(proposal);

            return updatedProposal;
        }
        private async Task<ProjectProposalCreateResponseDto> MapToResponseDto(ProjectProposal proposal)
        {
            var steps = await _projectApprovalStepQuery.GetStepsByProjectId(proposal.Id);
            var mappedSteps = new List<StepDto>();

            foreach (var s in steps)
            {
                User stepUser = s.User;

                if ((stepUser == null || s.User?.Id != s.ApproverUserId) && s.ApproverUserId.HasValue)
                {
                    stepUser = await _userQuery.GetById(s.ApproverUserId.Value);
                }

                mappedSteps.Add(new StepDto
                {
                    Id = s.Id.ToString(),
                    StepOrder = s.StepOrder,
                    DecisionDate = s.DecisionDate,
                    Observations = s.Observations ?? "",
                    ApproverUser = MapUserToDto(stepUser),
                    ApproverRole = MapRoleToDto(s.ApproverRole),
                    Status = MapStatusToDto(s.ApprovalStatus, s.Status)
                });
            }

            return new ProjectProposalCreateResponseDto
            {
                Id = proposal.Id,
                Title = proposal.Title,
                Description = proposal.Description,
                Amount = proposal.EstimatedAmount,
                Duration = proposal.EstimatedDuration,
                Area = new AreaDto
                {
                    Id = proposal.Area,
                    Name = proposal.Areas?.Name
                },
                Status = new StatusDto
                {
                    Id = proposal.Status,
                    Name = GetStatusName(proposal.Status)
                },
                Type = new TypeDto
                {
                    Id = proposal.Type,
                    Name = proposal.ProjectType?.Name
                },
                User = MapUserToDto(proposal.User),
                Steps = mappedSteps
            };
        }
        private string GetStatusName(int status) => status switch
        {
            1 => "Pendiente",
            2 => "Aprobado",
            3 => "Rechazado",
            4 => "Observado",
            _ => "Desconocido"
        };
        private UserDto MapUserToDto(User user)
        {
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = MapRoleToDto(user.ApproverRoles)
            };
        }
        private RoleDto MapRoleToDto(ApproverRole role)
        {
            if (role == null) return null;

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name
            };
        }
        private StatusDto MapStatusToDto(ApprovalStatus status, int statusId)
        {
            if (status != null)
            {
                return new StatusDto
                {
                    Id = status.Id,
                    Name = status.Name
                };
            }

            return new StatusDto
            {
                Id = statusId,
                Name = GetStatusName(statusId)
            };
        }
        public async Task<List<ProjectProposalDto>> GetFilteredProjects(
            string? title, 
            int? status, 
            int? applicant, 
            int? approvalUser
            )
        {
            var projects = await _projectProposalQuery.GetProjectsByFilters(title, status, applicant, approvalUser);

            return projects.Select(p => new ProjectProposalDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Amount = p.EstimatedAmount,
                Duration = p.EstimatedDuration,
                Area = p.Areas?.Name ?? "Sin área",
                Status = p.ApprovalStatus?.Name ?? GetStatusName(p.Status),
                Type = p.ProjectType?.Name ?? "Sin tipo"
            }).ToList();
        }
        public async Task<ProjectProposalCreateResponseDto> CreateProject(ProjectProposalRequest request)
        {
            ValidationUtils.ThrowIfNullOrEmpty(request.Title, "Título");
            ValidationUtils.ThrowIfNullOrEmpty(request.Description, "Descripción");
            ValidationUtils.ThrowIfNegative(request.Amount, "Monto estimado");
            ValidationUtils.ThrowIfOutOfRange(request.Duration, 1, int.MaxValue, "Duración");

            var existingProjects = await _projectProposalQuery.GetProjectsByFilters(request.Title, null, null, null);
            if (existingProjects.Any(p =>
                p.Title == request.Title &&
                p.Status != 3 // 3 = Rechazado
                ))
            {
                throw new BusinessException("Ya existe un proyecto con el mismo título.");
            }

            var user = await _userQuery.GetById(request.User);
            if (user == null)
                throw new Application.Exceptions.MyInvalidDataException("Usuario no encontrado");

            var proposal = new ProjectProposal
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                EstimatedAmount = request.Amount,
                EstimatedDuration = request.Duration,
                CreateAt = DateTime.UtcNow,
                Area = request.Area,
                Type = request.Type,
                Status = 1, // Pendiente
                CreateBy = request.User
            };

            var savedProposal = await _projectProposalCommand.CreateProposal(proposal);
            if (savedProposal == null)
                throw new Exception("No se pudo crear el proyecto");

            try
            {
                var approvalSteps = await _projectApprovalStepService.GenerateApprovalSteps(savedProposal.Id);
                if (!approvalSteps.Any())
                    throw new Exception("No se pudieron generar los pasos de aprobación");

                var notification = await _projectApprovalStepService.NotifyUsers(savedProposal.Id);

                var firstStep = approvalSteps.OrderBy(s => s.StepOrder).FirstOrDefault();
                if (firstStep != null)
                {
                    await _projectApprovalStepService.UpdateObservation(
                        firstStep.Id,
                        notification.Decision,
                        notification.UserName);
                }

                var updatedProposal = await _projectProposalQuery.GetProjectById(savedProposal.Id);

                return await MapToResponseDto(updatedProposal);
            }
            catch (Exception)
            {
                await _projectProposalCommand.DeleteProposal(savedProposal.Id);
                throw;
            }
        }

        public async Task<ProjectProposalCreateResponseDto> UpdateProject(Guid projectId, ProjectUpdate request)
        {
            var project = await _projectProposalQuery.GetProjectById(projectId);
            if (project == null)
                throw new BusinessException("Proyecto no encontrado");

            if (project.Status == 2 || project.Status == 3)
                throw new BusinessException("El proyecto ya no se encuentra en un estado que permite modificaciones");

            if (!string.IsNullOrEmpty(request.Title) && !project.Title.Equals(request.Title, StringComparison.OrdinalIgnoreCase))
            {
                var existingProjects = await _projectProposalQuery.GetProjectsByFilters(request.Title, null, null, null);
                if (existingProjects.Any(p => p.Title.Equals(request.Title, StringComparison.OrdinalIgnoreCase)))
                    throw new BusinessException("Ya existe un proyecto con el mismo título");
            }

            if (!string.IsNullOrEmpty(request.Title))
                ValidationUtils.ThrowIfNullOrEmpty(request.Title, "Título");
            if (!string.IsNullOrEmpty(request.Description))
                ValidationUtils.ThrowIfNullOrEmpty(request.Description, "Descripción");
            if (request.Duration > 0)
                ValidationUtils.ThrowIfOutOfRange(request.Duration, 1, int.MaxValue, "Duración");

            if (!string.IsNullOrEmpty(request.Title))
                project.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Description))
                project.Description = request.Description;
            if (request.Duration > 0)
                project.EstimatedDuration = request.Duration;

            var updatedProject = await _projectProposalCommand.UpdateProposal(project);
            return await MapToResponseDto(updatedProject);
        }
        public async Task<ProjectProposalResponse> InsertProject(ProjectProposalRequest projectRequest, int userId)
        {
            var proposal = new ProjectProposal
            {
                Id = Guid.NewGuid(),
                Title = projectRequest.Title,
                Description = projectRequest.Description,
                EstimatedAmount = projectRequest.Amount,
                EstimatedDuration = projectRequest.Duration,
                CreateAt = DateTime.UtcNow,
                Area = projectRequest.Area,
                Type = projectRequest.Type,
                Status = 1,
                CreateBy = userId
            };

            var savedProposal = await _projectProposalCommand.CreateProposal(proposal);
            if (savedProposal == null)
                throw new Exception("Error al crear la propuesta");

            return await _projectProposalMapper.GetProjectProposalResponse(savedProposal);
        }
        public async Task<ProjectStatusResponse> GetProjectStatus(Guid projectId)
        {
            var project = await _projectProposalQuery.GetProjectById(projectId);
            if (project == null)
                throw new Exception("Proyecto no encontrado");

            return new ProjectStatusResponse
            {
                ProjectId = project.Id,
                Title = project.Title,
                Description = project.Description,
                Area = project.Areas?.Name ?? "Sin área",
                Type = project.ProjectType?.Name ?? "Sin tipo",
                CurrentStatus = GetStatusName(project.Status),
                CreatedAt = project.CreateAt,
                CreatedBy = project.User?.Name ?? "Usuario desconocido",
                History = await GetProjectHistory(project)
            };
        }
        private async Task<string> GetProjectHistory(ProjectProposal project)
        {
            var steps = await _projectApprovalStepQuery.GetStepsByProjectId(project.Id);
            var history = new StringBuilder();

            foreach (var step in steps.OrderBy(s => s.StepOrder))
            {
                history.AppendLine($"Paso {step.StepOrder}:");
                history.AppendLine($"Estado: {GetStatusName(step.Status)}");
                if (step.ApproverUserId.HasValue)
                {
                    var user = await _userQuery.GetById(step.ApproverUserId.Value);
                    history.AppendLine($"Aprobador: {user?.Name ?? "No asignado"}");
                }
                if (step.DecisionDate.HasValue)
                {
                    history.AppendLine($"Fecha decisión: {step.DecisionDate.Value:dd/MM/yyyy HH:mm}");
                }
                if (!string.IsNullOrEmpty(step.Observations))
                {
                    history.AppendLine($"Observaciones: {step.Observations}");
                }
                history.AppendLine();
            }

            return history.ToString();
        }
        public async Task<List<ProjectProposal>> GetAllProjects(int pageNumber, int pageSize)
        {
            return await _projectProposalQuery.GetAllProjectProposal(pageNumber, pageSize);
        }
        public async Task<int> GetTotalProjectCount()
        {
            return await _projectProposalQuery.GetTotalProjectCount();
        }
        public async Task<ProjectProposalCreateResponseDto> GetProjectDetail(Guid projectId)
        {
            var project = await _projectProposalQuery.GetProjectById(projectId);
            if (project == null)
                throw new Exception("Proyecto no encontrado");

            return await MapToResponseDto(project);
        }
    }
}
