using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IApproverRoleQuery
    {
        Task<ApproverRole> GetById(int roleId);
        Task<string> GetRoleName(int roleId);
    }
}
