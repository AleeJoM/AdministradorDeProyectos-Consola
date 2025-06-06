using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queris
{
    public class UserQuery : IUserQuery
    {
        public readonly AppDbContext _context;
        public UserQuery(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<User>> GetAllUsers() =>
            await _context.User.Include(u => u.ApproverRole).ToListAsync();
        public async Task<User?> GetById(int userId)
        {
            return await _context.User
                .Include(u => u.ApproverRole)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<int> GetRoleById(int id)
        {
            return await _context.User
                .Where(u => u.Id == id)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<User>> GetUsersByRoleId(int roleId)
        {
            var usersWithRole = await _context.User
                .Where(u => u.Role == roleId)
                .ToListAsync();
            return usersWithRole;
        }
    }
}
