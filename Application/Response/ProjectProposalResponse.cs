using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Response
{
    public class ProjectProposalResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal EstimatedAmount { get; set; }
        public int EstimatedDuration { get; set; }
        public int Area { get; set; }
        public string AreaName { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public int CreateBy { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int UserRoleId { get; set; }
        public string UserRoleName { get; set; }
        public ICollection<ProjectApprovalStep> Steps { get; set; }
    }
}
