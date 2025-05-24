using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;

namespace Application.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserQuery _userQuery;
        public UserRoleService(IUserQuery userQuery)
        {
            _userQuery = userQuery;
        }
        public async Task<int> GetRoleById(int userId)
        {
            var user = await _userQuery.GetById(userId);
            return user?.Role ?? 0;
        }
    }
}
