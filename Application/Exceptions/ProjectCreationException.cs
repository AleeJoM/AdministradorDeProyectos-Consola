using System;

namespace Application.Exceptions
{
    public class ProjectCreationException : Exception
    {
        public ProjectCreationException() : base("Error al crear el proyecto.")
        {
        }

        public ProjectCreationException(string message) : base(message)
        {
        }

        public ProjectCreationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ProjectCreationException(string projectName, string reason) 
            : base($"No se pudo crear el proyecto '{projectName}'. Razón: {reason}")
        {
        }
    }
}
