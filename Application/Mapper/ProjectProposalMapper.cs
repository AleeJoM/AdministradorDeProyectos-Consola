using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Application.Response;
using Domain.Entities;

namespace Application.Mapper
{
    public class ProjectProposalMapper : IProjectProposalMapper
    {
        private readonly IUserRoleService _userRoleService;
        private readonly IUserQuery _userQuery;
        private readonly IAreaQuery _areaQuery;
        private readonly IProjectTypeQuery _projectTypeQuery;
        public ProjectProposalMapper(
            IUserRoleService userRoleService,
            IUserQuery userQuery,
            IAreaQuery areaQuery,
            IProjectTypeQuery projectTypeQuery)
        {
            _userRoleService = userRoleService;
            _userQuery = userQuery;
            _areaQuery = areaQuery;
            _projectTypeQuery = projectTypeQuery;
        }
        public async Task<ProjectProposalResponse> GetProjectProposalResponse(ProjectProposal project)
        {
            var area = await _areaQuery.GetById(project.Area ?? 0);
            var type = await _projectTypeQuery.GetById(project.Type ?? 0);
            var user = await _userQuery.GetById(project.CreateBy);
            var userRole = user != null ? await _userRoleService.GetRoleById(user.Id) : 0;

            return new ProjectProposalResponse
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                EstimatedAmount = project.EstimatedAmount,
                EstimatedDuration = project.EstimatedDuration,
                Area = project.Area ?? 0,
                AreaName = area?.Name,
                Type = project.Type ?? 0,
                TypeName = type?.Name,
                Status = project.ApprovalStatus?.Id ?? project.Status,
                StatusName = project.ApprovalStatus?.Name,
                CreateBy = project.CreateBy,
                UserName = user?.Name,
                UserEmail = user?.Email,
                UserRoleId = userRole,
                UserRoleName = project.User.ApproverRole?.Name,
                Steps = project.ProjectApprovalSteps?.Select(s =>
                    new ProjectApprovalStep
                    {
                        Id = s.Id,
                        StepOrder = s.StepOrder,
                        DecisionDate = s.DecisionDate,
                        Observations = s.Observations,
                        ProjectProposalId = s.ProjectProposalId,
                        ApproverUserId = s.ApproverUserId,
                        User = s.User,
                        ApproverRoleId = s.ApproverRoleId,
                        ApproverRole = s.ApproverRole,
                        Status = s.ApprovalStatus?.Id ?? s.Status,
                        ApprovalStatus = s.ApprovalStatus 
                    }
                ).ToList()
            };
        }
        public async Task<ProjectProposalCreateResponseDto> MapToResponseDto(ProjectProposal proposal)
        {
            var steps = proposal.ProjectApprovalSteps?.ToList() ?? new List<ProjectApprovalStep>();
            var mappedSteps = new List<StepDto>();

            foreach (var step in steps)
            {
                var stepUser = step.User;
                if ((stepUser == null || step.User?.Id != step.ApproverUserId) && step.ApproverUserId.HasValue)
                {
                    stepUser = await _userQuery.GetById(step.ApproverUserId.Value);
                }

                mappedSteps.Add(new StepDto
                {
                    Id = step.Id,
                    StepOrder = step.StepOrder,
                    DecisionDate = step.DecisionDate,
                    Observations = step.Observations ?? "",
                    ApproverUser = MapUserToDto(stepUser),
                    ApproverRole = new RoleDto
                    {
                        Id = step.ApproverRole?.Id ?? 0,
                        Name = step.ApproverRole?.Name ?? "Sin rol"
                    },
                    Status = new StatusDto
                    {
                        Id = step.ApprovalStatus?.Id ?? step.Status,
                        Name = step.ApprovalStatus?.Name ?? GetStatusName(step.Status)
                    }
                });
            }

            var area = await _areaQuery.GetById(proposal.Area ?? 0);
            var type = await _projectTypeQuery.GetById(proposal.Type ?? 0);

            return new ProjectProposalCreateResponseDto
            {
                Id = proposal.Id,
                Title = proposal.Title,
                Description = proposal.Description,
                Amount = proposal.EstimatedAmount,
                Duration = proposal.EstimatedDuration,
                Area = new AreaDto
                {
                    Id = proposal.Area ?? 0,
                    Name = area?.Name ?? "Sin área"
                },
                Status = new StatusDto
                {
                    Id = proposal.Status,
                    Name = proposal.ApprovalStatus?.Name ?? GetStatusName(proposal.Status)
                },
                Type = new TypeDto
                {
                    Id = proposal.Type ?? 0,
                    Name = type?.Name ?? "Sin tipo"
                },
                User = MapUserToDto(proposal.User),
                Steps = mappedSteps
            };
        }
        private UserDto MapUserToDto(User user)
        {
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = new RoleDto
                {
                    Id = user.Role,
                    Name = user.ApproverRole?.Name ?? "Sin rol"
                }
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
    }
}
