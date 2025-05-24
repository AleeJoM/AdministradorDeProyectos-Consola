using Domain.Entities;
using Application.Dtos;
using System.Numerics;
using Application.Request;

namespace Application.Interfaces
{
    public interface IProjectApprovalStepService
    {
        Task<List<ProjectApprovalStep>> GenerateApprovalSteps(Guid projectId);
        Task<ProjectStatusDto> NotifyUsers(Guid projectId);
        Task UpdateObservation(BigInteger stepId, string newObservation, string user);
        Task<string> GetObservation(BigInteger stepId);
        Task ProcessProposalStep(Guid proposalId, int userId, char decision);
    }
}

