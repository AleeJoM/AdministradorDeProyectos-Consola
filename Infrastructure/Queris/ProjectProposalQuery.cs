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
                .OrderBy(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<List<ProjectProposal>> GetProjectsByFilters(string? title, int? status, int? applicant, int? approvalUser)
        {
            var query = _context.ProjectProposal.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(p => p.Title.Contains(title));
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (applicant.HasValue)
            {
                query = query.Where(p => p.CreateBy == applicant.Value);
            }

            if (approvalUser.HasValue)
            {
                query = query.Where(p => p.ProjectApprovalSteps.Any(step => step.ApproverUserId == approvalUser.Value));
            }

            return await query
                .Include(p => p.AreaNavigation)
                .Include(p => p.ProjectType)
                .Include(p => p.ApprovalStatus)
                .Include(p => p.User)
                .Include(p => p.ProjectApprovalSteps)
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
        public async Task<List<ProjectProposal>> GetProjectByStatus(int status)
        {
            return await _context.ProjectProposal
            .Where(p => p.Status == status)
            .ToListAsync();
        }
        public async Task<ProjectProposal> GetProjectById(Guid projectId)
        {
            return await _context.ProjectProposal
                .Include(project => project.AreaNavigation)
                .Include(project => project.ProjectType)
                .Include(project => project.ApprovalStatus)
                .Include(project => project.User)
                .Include(project => project.ProjectApprovalSteps)
                .FirstOrDefaultAsync(project => project.Id == projectId);
        }
    }
}
