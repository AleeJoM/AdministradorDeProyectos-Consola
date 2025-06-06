using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ProjectProposal
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedAmount { get; set; }
        public int EstimatedDuration { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? Area { get; set; }
        public Area? AreaNavigation { get; set; }

        public int? Type { get; set; }
        public ProjectType? ProjectType { get; set; }

        public int Status { get; set; }
        public ApprovalStatus? ApprovalStatus { get; set; }

        public int CreateBy { get; set; }
        public User User { get; set; }

        public ICollection<ProjectApprovalStep> ProjectApprovalSteps { get; set; } = new List<ProjectApprovalStep>();
    }
}
