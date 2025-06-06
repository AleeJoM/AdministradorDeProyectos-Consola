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
    public class ApprovalRuleQuery : IApprovalRuleQuery
    {
        private readonly AppDbContext _context;
        public ApprovalRuleQuery(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<ApprovalRule>> GetAll()
        {
            return await _context.ApprovalRule.ToListAsync();
        }
    }
}
