using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserRoleService
    {
        Task<UserRoleDto> GetUserRoleDetails(int userId);
        Task<int> GetRoleById(int userId);
        Task<ValidateUserRoleResponseDto> ValidateUserRoleForStep(ValidateUserRoleRequestDto request);
        Task<ApproverRoleDto> GetRoleDetailsById(int roleId);
    }
}
