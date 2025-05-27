using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class ValidateUserRoleResponseDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
