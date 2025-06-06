using System;

namespace Application.Exceptions
{
    public class InvalidProjectStateException : Exception
    {
        public InvalidProjectStateException() : base("El estado del proyecto no es válido para esta operación.")
        {
        }

        public InvalidProjectStateException(string message) : base(message)
        {
        }

        public InvalidProjectStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidProjectStateException(int projectId, string currentState, string expectedState) 
            : base($"El proyecto {projectId} tiene estado '{currentState}' pero se esperaba '{expectedState}'.")
        {
        }

        public InvalidProjectStateException(int projectId, string operation) 
            : base($"No se puede realizar la operación '{operation}' en el proyecto {projectId} debido a su estado actual.")
        {
        }
    }
}
