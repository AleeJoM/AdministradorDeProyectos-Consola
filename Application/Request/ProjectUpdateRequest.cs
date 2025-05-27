using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request
{
    public class ProjectUpdate
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 100 caracteres")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La duración debe ser mayor que cero")]
        public int Duration { get; set; }
    }
}
