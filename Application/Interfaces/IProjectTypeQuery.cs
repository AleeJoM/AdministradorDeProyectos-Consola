using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProjectTypeQuery
    {
        Task<List<ProjectType>> GetAllType();
        Task<ProjectType?> GetById(int typeId);
    }
}
