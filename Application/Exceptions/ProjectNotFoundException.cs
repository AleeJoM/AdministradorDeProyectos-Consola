using System;

namespace Application.Exceptions
{
    public class ProjectNotFoundException : Exception
    {
        public ProjectNotFoundException(int projectId) 
            : base($"Proyecto con ID {projectId} no fue encontrado.")
        {
        }

        public ProjectNotFoundException(string message) 
            : base(message)
        {
        }

        public ProjectNotFoundException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
