using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request
{
    public class ProjectProposalRequest
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 100 caracteres")]
        public string Title { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El monto estimado es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "La duración estimada es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La duración debe ser mayor que cero")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "El área es obligatoria")]
        public int Area { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        public int User { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public int Type { get; set; }
    }
}
