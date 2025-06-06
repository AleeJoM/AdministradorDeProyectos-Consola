using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;

namespace ADMProyectos.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class InformationController : ControllerBase
    {
        private readonly IAreaQuery _areaQuery;
        private readonly IProjectTypeQuery _projectTypeQuery;
        private readonly IUserQuery _userQuery;
        private readonly IProjectApprovalStepQuery _approvalStepQuery;

        public InformationController(
            IAreaQuery areaQuery,
            IProjectTypeQuery projectTypeQuery,
            IUserQuery userQuery,
            IProjectApprovalStepQuery approvalStepQuery)
        {
            _areaQuery = areaQuery;
            _projectTypeQuery = projectTypeQuery;
            _userQuery = userQuery;
            _approvalStepQuery = approvalStepQuery;
        }

        /// <summary>
        /// Listado de áreas
        /// </summary>
        [HttpGet("Area")]
        public async Task<IActionResult> GetAreas()
        {
            var areas = await _areaQuery.GetAllAreas();
            var response = areas.Select(a => new { id = a.Id, name = a.Name });
            return Ok(response);
        }

        /// <summary>
        /// Listado de tipos de proyectos
        /// </summary>
        [HttpGet("ProjectType")]
        public async Task<IActionResult> GetProjectTypes()
        {
            var types = await _projectTypeQuery.GetAllType();
            var response = types.Select(t => new { id = t.Id, name = t.Name });
            return Ok(response);
        }

        /// <summary>
        /// Listado de roles de usuario
        /// </summary>
        [HttpGet("Role")]
        public async Task<IActionResult> GetRoles()
        {
            var users = await _userQuery.GetAllUsers();
            var roles = users
                .Where(u => u.ApproverRole != null)
                .Select(u => u.ApproverRole)
                .Distinct()
                .Select(r => new { id = r.Id, name = r.Name });
            return Ok(roles);
        }

        /// <summary>
        /// Listado de estados para una solicitud de proyectos y pasos de aprobación
        /// </summary>
        [HttpGet("ApprovalStatus")]
        public async Task<IActionResult> GetApprovalStatus()
        {
            var statuses = new[]
            {
                new { id = 1, name = "Pendiente" },
                new { id = 2, name = "Aprobado" },
                new { id = 3, name = "Rechazado" },
                new { id = 4, name = "En Revisión" }
            };
            return Ok(statuses);
        }

        /// <summary>
        /// Listado de usuarios
        /// </summary>
        [HttpGet("User")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userQuery.GetAllUsers();
            var response = users.Select(u => new { id = u.Id, name = u.Name });
            return Ok(response);
        }
    }
}
