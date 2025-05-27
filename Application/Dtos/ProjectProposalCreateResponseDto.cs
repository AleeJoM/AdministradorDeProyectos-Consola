using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class ProjectProposalCreateResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int Duration { get; set; }
        public AreaDto Area { get; set; }
        public StatusDto Status { get; set; }
        public TypeDto Type { get; set; }
        public UserDto User { get; set; }
        public List<StepDto> Steps { get; set; }
    }
}
