using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class DecisionStepDto
    {
        [Required(ErrorMessage = "El ID del paso es obligatorio")]
        public long Id { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El usuario debe ser mayor a 0")]
        public int User { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [Range(1, 4, ErrorMessage = "El estado debe estar entre 1 y 4")]
        public int Status { get; set; }

        [Required(ErrorMessage = "La observación es obligatoria")]
        [StringLength(500, ErrorMessage = "La observación no puede exceder 500 caracteres")]
        [MinLength(1, ErrorMessage = "La observación no puede estar vacía")]
        public string Observation { get; set; } = string.Empty;
    }
}
