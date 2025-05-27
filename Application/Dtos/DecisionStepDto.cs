using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class DecisionStepDto
    {
        public long Id { get; set; }
        public int User { get; set; }
        public int Status { get; set; }
        public string Observation { get; set; }
    }
}
