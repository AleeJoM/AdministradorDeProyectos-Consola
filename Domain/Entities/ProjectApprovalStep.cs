using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ProjectApprovalStep
    {
        public long Id { get; set; }
        public int StepOrder { get; set; }

        public DateTime? DecisionDate { get; set; }
        public string? Observations { get; set; }

        public Guid ProjectProposalId { get; set; }
        public ProjectProposal? ProjectProposal { get; set; }

        public int? ApproverUserId { get; set; }
        public User? User { get; set; }

        public int ApproverRoleId { get; set; }
        public ApproverRole? ApproverRole { get; set; }
        public int Status { get; set; }
        public ApprovalStatus? ApprovalStatus { get; set; }
    }
}
