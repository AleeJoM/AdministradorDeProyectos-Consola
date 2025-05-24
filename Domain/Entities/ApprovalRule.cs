using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ApprovalRule
    {
        public BigInteger Id { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int StepOrder { get; set; }

        public int? Area { get; set; }
        public Area Areas { get; set; }

        public int? Type { get; set; }
        public ProjectType ProjectType { get; set; }

        public int ApproverRoleId { get; set; }
        public ApproverRole ApproverRole { get; set; }
    }
}
