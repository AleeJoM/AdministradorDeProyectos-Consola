using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class ValidateUserRoleRequestDto
    {
        public int UserId { get; set; }
        public int ApproverRoleId { get; set; }
        public int? CurrentApproverUserId { get; set; }
    }
}
