using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace Infrastructure.Queris
{
    public class ProjectProposalQuery : IProjectProposalQuery
    {
        public readonly AppDbContext _context;
        public ProjectProposalQuery(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<ProjectProposal>> GetAllProjectProposal(int pageNumber, int pageSize)
        {
            return await _context.ProjectProposal
                .OrderBy(p => p.CreateAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<int> GetTotalProjectCount()
        {
            return await _context.ProjectProposal.CountAsync();
        }
        public async Task<int> GetStatusById(Guid projectId)
        {
            var status = await _context.ProjectProposal
            .Where(p => p.Id == projectId)
            .Select(p => p.Status)
            .FirstOrDefaultAsync();
            return status;
        }
        public async Task<List<ProjectProposal>> GetProposalsByStatus(int status)
        {
            return await _context.ProjectProposal
            .Where(p => p.Status == status)
            .ToListAsync();
        }
        public async Task<ProjectProposal> GetProjectById(Guid projectId)
        {
            return await _context.ProjectProposal
                .Include(project => project.Areas)
                .Include(project => project.ProjectType)
                .Include(project => project.ApprovalStatus)
                .Include(project => project.User)
                .Include(project => project.ProjectApprovalSteps)
                .FirstOrDefaultAsync(project => project.Id == projectId);
        }
    }
}
