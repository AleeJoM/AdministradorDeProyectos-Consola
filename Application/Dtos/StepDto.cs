using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class StepDto
    {
        public long Id { get; set; }
        public int StepOrder { get; set; }
        public DateTime? DecisionDate { get; set; }
        public string Observations { get; set; } = string.Empty;
        public UserDto? ApproverUser { get; set; }
        public RoleDto? ApproverRole { get; set; }
        public StatusDto? Status { get; set; }
    }
}
