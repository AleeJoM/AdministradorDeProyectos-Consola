using System;

namespace Application.Exceptions
{
    public class ApprovalStepNotFoundException : Exception
    {
        public ApprovalStepNotFoundException() : base("No se encontró el paso de aprobación.")
        {
        }

        public ApprovalStepNotFoundException(string message) : base(message)
        {
        }

        public ApprovalStepNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ApprovalStepNotFoundException(int stepId) 
            : base($"No se encontró el paso de aprobación con ID {stepId}.")
        {
        }

        public ApprovalStepNotFoundException(int projectId, int step) 
            : base($"No se encontró el paso {step} para el proyecto {projectId}.")
        {
        }
    }
}
