using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProjectProposalCommand
    {
        Task<ProjectProposal> CreateProposal(ProjectProposal Project);
        Task<ProjectProposal> UpdateProposal(ProjectProposal Project);
        Task<bool> DeleteProposal(Guid projectId);
        Task<ProjectProposal> FinalizeProposal(Guid projectId);        
        Task<ProjectProposal> UpdateProposalStatus(Guid projectId, string decision);
        Task<ITransaction> BeginTransactionAsync();
    }
}
