using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserQuery
    {
        Task<List<User>> GetAllUsers();
        Task<User?> GetById(int userId);
        Task<int> GetRoleById(int id);
        Task<IEnumerable<User>> GetUsersByRoleId(int roleId);
    }
}
