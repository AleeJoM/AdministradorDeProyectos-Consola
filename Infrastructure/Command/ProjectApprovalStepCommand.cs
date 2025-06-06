using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Command
{
    public class ProjectApprovalStepCommand : IProjectApprovalStepCommand
    {
        private readonly AppDbContext _context;
        public ProjectApprovalStepCommand(AppDbContext context) 
        {
            _context = context;
        }
        public async Task<ProjectApprovalStep> CreateProjectStep(ProjectApprovalStep projectApprovalStep)
        {
            await _context.AddAsync(projectApprovalStep);
            await _context.SaveChangesAsync();
            return projectApprovalStep;
        }
        public async Task SaveSteps(ICollection<ProjectApprovalStep> steps)
        {
            _context.ProjectApprovalStep.AddRange(steps);
            await _context.SaveChangesAsync();
        }
        public async Task<List<ProjectApprovalStep>> GetProjectStepForUser(Guid projectId)
        {
            return await _context.ProjectApprovalStep
                .Where(s => s.ProjectProposalId == projectId)
                .Include(s => s.User)
                .ToListAsync();
        }
        public async Task<ProjectApprovalStep> GetProjectStepById(BigInteger stepId)
        {
            return await _context.ProjectApprovalStep.FirstOrDefaultAsync(p => p.Id == stepId);
        }
        public async Task<string> GetProjectStepObservationsById(BigInteger stepId)
        {
            var projectStep = await _context.ProjectApprovalStep
                .Where(p => p.Id == stepId)
                .Select(p => p.Observations)
                .FirstOrDefaultAsync();

            return projectStep;
        }
        public async Task UpdateStep(ProjectApprovalStep step)
        {
            _context.ProjectApprovalStep.Update(step);
            await _context.SaveChangesAsync();
        }
        public async Task<ApprovalStatus> GetApprovalStatusById(int id)
        {
            return await _context.ApprovalStatus.FindAsync(id);
        }
    }
}
