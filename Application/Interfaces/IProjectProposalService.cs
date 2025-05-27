using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Request;
using Application.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProjectProposalService
    {
        Task<List<ProjectProposalDto>> GetFilteredProjects(string? title, int? status, int? applicant, int? approvalUser);
        Task<ProjectProposalCreateResponseDto> CreateProject(ProjectProposalRequest request);
        Task<ProjectProposalCreateResponseDto> ProcessDecision(Guid projectId, DecisionStepDto request);
        Task<ProjectProposalCreateResponseDto> UpdateProject(Guid projectId, ProjectUpdate request);
        Task<ProjectProposalCreateResponseDto> GetProjectDetail(Guid projectId);
        Task<ProjectProposalResponse> InsertProject(ProjectProposalRequest projectRequest, int userId);
        Task<ProjectStatusResponse> GetProjectStatus(Guid projectId);
        Task<List<ProjectProposal>> GetAllProjects(int pageNumber, int pageSize);
        Task<int> GetTotalProjectCount();
    }
}
