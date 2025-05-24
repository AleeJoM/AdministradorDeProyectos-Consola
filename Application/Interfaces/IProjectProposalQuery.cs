using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProjectProposalQuery
    {
        Task<List<ProjectProposal>> GetAllProjectProposal(int pageNumber, int pageSize);
        Task<int> GetTotalProjectCount();
        Task<int> GetStatusById(Guid projectId);
        Task<List<ProjectProposal>> GetProposalsByStatus(int status);
        Task<ProjectProposal> GetProjectById(Guid projectId);
    }
}
