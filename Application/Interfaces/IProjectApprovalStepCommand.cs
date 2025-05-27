using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProjectApprovalStepCommand
    {
        Task<ProjectApprovalStep> CreateProjectStep(ProjectApprovalStep projectApprovalStep);
        Task SaveSteps(ICollection<ProjectApprovalStep> steps);
        Task<List<ProjectApprovalStep>> GetProjectStepForUser(Guid projectId);
        Task<ProjectApprovalStep> GetProjectStepById(BigInteger stepId);
        Task<string> GetProjectStepObservationsById(BigInteger stepId);
        Task UpdateStep(ProjectApprovalStep step);
        Task<ApprovalStatus> GetApprovalStatusById(int id);
    }
}
