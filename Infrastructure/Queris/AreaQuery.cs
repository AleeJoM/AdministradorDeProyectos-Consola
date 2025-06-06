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
    public class AreaQuery : IAreaQuery
    {
        public readonly AppDbContext _context;
        public AreaQuery(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Area>> GetAllAreas() =>
            await _context.Area.ToListAsync();
        public async Task<Area?> GetById(int areaId)
        {
            return await _context.Area.FirstOrDefaultAsync(u => u.Id == areaId);
        }
        public async Task<string> GetAreaNameById(int areaId)
        {
            var area = await GetById(areaId);
            return area?.Name ?? "Desconocido";
        }
    }
}
