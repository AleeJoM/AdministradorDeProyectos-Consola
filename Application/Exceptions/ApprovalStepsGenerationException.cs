using System;

namespace Application.Exceptions
{
    public class ApprovalStepsGenerationException : Exception
    {
        public ApprovalStepsGenerationException() : base("Error al generar los pasos de aprobación.")
        {
        }

        public ApprovalStepsGenerationException(string message) : base(message)
        {
        }

        public ApprovalStepsGenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ApprovalStepsGenerationException(int projectId, string reason) 
            : base($"No se pudieron generar los pasos de aprobación para el proyecto {projectId}. Razón: {reason}")
        {
        }
    }
}
