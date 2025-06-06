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
    public class ProjectTypeQuery : IProjectTypeQuery
    {
        public readonly AppDbContext _context;
        public ProjectTypeQuery(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<ProjectType>> GetAllType() =>
            await _context.ProjectType.ToListAsync();
        public async Task<ProjectType?> GetById(int typeId)
        {
            return await _context.ProjectType.FirstOrDefaultAsync(u => u.Id == typeId);
        }
    }
}
