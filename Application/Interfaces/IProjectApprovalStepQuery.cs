using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProjectApprovalStepQuery
    {
        Task<string?> GetObservationByProjectId(Guid projectProposalId);
        Task<List<ProjectApprovalStep>> GetStepsByRoleAndStatus(int role, int status);
        Task<ProjectApprovalStep> GetStepByProposalAndUser(Guid proposalId, int userId);
        Task<List<ProjectApprovalStep>> GetStepsByProjectId(Guid projectProposalId);
    }
}
