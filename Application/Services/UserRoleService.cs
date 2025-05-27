using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Application.Exceptions;
using Application.Validation;

namespace Application.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserQuery _userQuery;
        private readonly IApproverRoleQuery _approverRoleQuery;

        public UserRoleService(
            IUserQuery userQuery,
            IApproverRoleQuery approverRoleQuery)
        {
            _userQuery = userQuery;
            _approverRoleQuery = approverRoleQuery;
        }

        public async Task<int> GetRoleById(int userId)
        {
            var user = await _userQuery.GetById(userId);
            if (user == null)
                throw new BusinessException("Usuario no encontrado");
            return user?.Role ?? 0;
        }

        public async Task<UserRoleDto> GetUserRoleDetails(int userId)
        {
            var user = await _userQuery.GetById(userId);
            if (user == null)
            {
                throw new BusinessException("Usuario no encontrado");
            }

            var roleName = await _approverRoleQuery.GetRoleName(user.Role);

            return new UserRoleDto
            {
                UserId = user.Id,
                UserName = user.Name,
                RoleId = user.Role,
                RoleName = roleName ?? "Rol desconocido"
            };
        }

        public async Task<ApproverRoleDto> GetRoleDetailsById(int roleId)
        {
            var role = await _approverRoleQuery.GetById(roleId);
            if (role == null)
                throw new BusinessException("Rol no encontrado");
            return new ApproverRoleDto
            {
                Id = role?.Id ?? 0,
                Name = role?.Name ?? "Rol no encontrado"
            };
        }

        public async Task<ValidateUserRoleResponseDto> ValidateUserRoleForStep(ValidateUserRoleRequestDto request)
        {
            var userRole = await GetRoleById(request.UserId);

            if (userRole != request.ApproverRoleId)
            {
                return new ValidateUserRoleResponseDto
                {
                    IsValid = false,
                    Message = "El usuario no tiene el rol requerido para este paso"
                };
            }

            return new ValidateUserRoleResponseDto
            {
                IsValid = true,
                Message = "Usuario validado correctamente"
            };
        }
    }
}
