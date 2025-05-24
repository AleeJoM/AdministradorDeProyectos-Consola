using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRoleService
    {
        Task<int> GetRoleById(int userId);
    }
}
