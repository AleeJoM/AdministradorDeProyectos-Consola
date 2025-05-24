using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Request;
using Application.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProjectProposalService
    {
        Task<ProjectProposalResponse> InsertProject(ProjectProposalRequest projectResquest, int userId);
        Task<ProjectProposalResponse> UpdateProject(ProjectProposal projectId);
        Task<ProjectStatusResponse> GetProjectStatus(Guid projectId);
        Task<List<ProjectProposal>> GetAllProjects(int pageNumber, int pageSize);
        Task<int> GetTotalProjectCount();
    }
}
