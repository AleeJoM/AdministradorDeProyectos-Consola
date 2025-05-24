using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IAreaQuery
    {
        Task<List<Area>> GetAllAreas();
        Task<Area?> GetById(int areaId);
        Task<string> GetAreaNameById(int areaId);
    }
}
