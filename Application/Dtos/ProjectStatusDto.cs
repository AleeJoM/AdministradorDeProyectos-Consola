using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class ProjectStatusDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Area { get; set; }
        public string Type { get; set; }
        public string CurrentStatus { get; set; }
        public string CreatedBy { get; set; }
        public string History { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Decision { get; set; }
        public string Message { get; set; }
    }
}
