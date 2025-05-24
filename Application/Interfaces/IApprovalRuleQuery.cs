using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IApprovalRuleQuery
    {
        Task<List<ApprovalRule>> GetAll();
    }
}
