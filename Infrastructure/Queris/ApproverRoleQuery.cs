using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Queris
{
    public class ApproverRoleQuery : IApproverRoleQuery
    {
        private readonly AppDbContext _context;

        public ApproverRoleQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApproverRole> GetById(int roleId)
        {
            return await _context.ApproverRole
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task<string> GetRoleName(int roleId)
        {
            var role = await GetById(roleId);
            return role?.Name;
        }
    }
}
