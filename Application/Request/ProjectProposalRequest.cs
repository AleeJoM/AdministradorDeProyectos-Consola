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
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        [MinLength(3, ErrorMessage = "El título debe tener al menos 3 caracteres")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 10000000, ErrorMessage = "El monto debe estar entre 0.01 y 10,000,000")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "La duración es obligatoria")]
        [Range(1, 3650, ErrorMessage = "La duración debe estar entre 1 y 3650 días")]
        public int Duration { get; set; }

        [Range(1, 4, ErrorMessage = "El área debe estar entre 1 y 4")]
        public int? Area { get; set; }

        [Range(1, 4, ErrorMessage = "El tipo debe estar entre 1 y 4")]
        public int? Type { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El usuario debe ser mayor a 0")]
        public int User { get; set; }
    }
}
