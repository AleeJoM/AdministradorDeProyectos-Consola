using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int Type { get; set; }
        public int CreateBy { get; set; }
    }
}
