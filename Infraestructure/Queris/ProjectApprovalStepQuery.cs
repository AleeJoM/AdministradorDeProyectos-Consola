using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queris
{
    public class ProjectApprovalStepQuery : IProjectApprovalStepQuery
    {
        private readonly AppDbContext _context;
        public ProjectApprovalStepQuery(AppDbContext context)
        {
            _context = context;
        }
        public async Task<string?> GetObservationByProjectId(Guid projectProposalId)
        {
            var observation = await _context.ProjectApprovalStep
                .Where(p => p.ProjectProposalId == projectProposalId && !string.IsNullOrEmpty(p.Observations))
                .OrderBy(p => p.StepOrder)
                .Select(p => p.Observations)
                .FirstOrDefaultAsync();
            return observation;
        }
        public async Task<List<ProjectApprovalStep>> GetStepsByProjectId(Guid projectProposalId)
        {
            return await _context.ProjectApprovalStep
                .Include(p => p.User)
                .Include(p => p.ApproverRole)
                .Include(p => p.ApprovalStatus)
                .Include(p => p.ProjectProposal)
                .Where(p => p.ProjectProposalId == projectProposalId)
                .OrderBy(p => p.StepOrder)
                .ToListAsync();
        }

        public async Task<List<ProjectApprovalStep>> GetStepsByRoleAndStatus(int role, int status)
        {
            var pasos = await _context.ProjectApprovalStep
                .Include(p => p.ProjectProposal)
                .Where(p => p.ApproverRoleId == role && p.Status == status)
                .ToListAsync();

            var stepFilters = new List<ProjectApprovalStep>();

            foreach (var paso in pasos)
            {
                if (paso.StepOrder == 1)
                {
                    stepFilters.Add(paso);
                }
                else
                {
                    var pasoAnterior = await _context.ProjectApprovalStep
                        .FirstOrDefaultAsync(p =>
                            p.ProjectProposalId == paso.ProjectProposalId &&
                            p.StepOrder == paso.StepOrder - 1);

                    if (pasoAnterior != null && pasoAnterior.Status == 2)
                    {
                        stepFilters.Add(paso);
                    }
                }
            }
            return stepFilters;
        }
        public async Task<List<ProjectApprovalStep>> GetStepsByProposal(Guid proposalId)
        {
            return await _context.ProjectApprovalStep
                .Where(s => s.ProjectProposalId == proposalId)
                .ToListAsync();
        }
        public async Task<ProjectApprovalStep> GetStepByProposalAndUser(Guid proposalId, int userId)
        {
            var userRole = await _context.User
                .Where(u => u.Id == userId)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();

            var step = await _context.ProjectApprovalStep
                .Include(p => p.User)
                .Include(p => p.ApproverRole)
                .FirstOrDefaultAsync(s => s.ProjectProposalId == proposalId
                    && s.ApproverRoleId == userRole
                    && (s.Status == 1 || s.Status == 4));

            if (step == null)
                return null;

            var otherUserProcessed = await _context.ProjectApprovalStep
                .AnyAsync(s => s.ProjectProposalId == proposalId
                    && s.StepOrder == step.StepOrder
                    && s.ApproverRoleId == step.ApproverRoleId
                    && (s.Status == 2 || s.Status == 3));

            if (otherUserProcessed)
                return null;

            return step;
        }
    }
}
