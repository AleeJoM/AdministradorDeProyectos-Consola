using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.Request;
using Application.Dtos;
using Application.Response;
using Application.Exceptions;
using Application.Validation;

namespace ADMProyectos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectProposalService _projectProposalService;

        public ProjectController(IProjectProposalService projectProposalService)
        {
            _projectProposalService = projectProposalService;
        }

        /// <summary>
        /// Obtiene el listado de proyectos con filtros
        /// </summary>
        /// <param name="title">Filtro por título del proyecto</param>
        /// <param name="status">Filtro por ID del estado del proyecto</param>
        /// <param name="applicant">Filtro por ID del solicitante</param>
        /// <param name="approvalUser">Filtro por ID del usuario aprobador</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProjectProposalDto>), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> GetProjects(
            [FromQuery] string? title,
            [FromQuery] int? status,
            [FromQuery] int? applicant,
            [FromQuery] int? approvalUser)
        {
            try
            {
                var projectDtos = await _projectProposalService.GetFilteredProjects(title, status, applicant, approvalUser);
                return Ok(projectDtos);
            }
            catch (MyInvalidDataException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message, ErrorCode = "INVALID_DATA" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message, ErrorCode = "VALIDATION_ERROR" });
            }
            catch (BusinessException ex)
            {
                return Conflict(new ApiError { Message = ex.Message, ErrorCode = "BUSINESS_ERROR" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva propuesta de proyecto
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ProjectProposalCreateResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectProposalCreateResponseDto>> CreateProject(
            [FromBody] ProjectProposalRequest request
            )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiError
                    {
                        Message = "Datos de entrada inválidos",
                        Errors = errors,
                        ErrorCode = "VALIDATION_ERROR"
                    });
                }

                ValidationUtils.ThrowIfNullOrEmpty(request.Title, "Título");
                ValidationUtils.ThrowIfNullOrEmpty(request.Description, "Descripción");
                ValidationUtils.ThrowIfNegative(request.Amount, "Monto estimado");
                ValidationUtils.ThrowIfOutOfRange(request.Duration, 1, int.MaxValue, "Duración");

                var response = await _projectProposalService.CreateProject(request);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (MyInvalidDataException ex)
            {
                return BadRequest(new ApiError
                {
                    Message = ex.Message,
                    ErrorCode = "INVALID_DATA"
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiError
                {
                    Message = ex.Message,
                    ErrorCode = "VALIDATION_ERROR"
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ApiError
                {
                    Message = ex.Message,
                    ErrorCode = "BUSINESS_ERROR"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiError { Message = ex.Message, ErrorCode = "INTERNAL_ERROR" });
            }
        }

        /// <summary>
        /// Aprobar, rechazar u observar un proyecto
        /// </summary>
        [HttpPatch("{id}/decision")]
        [ProducesResponseType(typeof(ProjectProposalCreateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> MakeDecision(Guid id, [FromBody] DecisionStepDto request)
        {
            try
            {
                var response = await _projectProposalService.ProcessDecision(id, request);
                return Ok(response);
            }
            catch (MyInvalidDataException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message, ErrorCode = "INVALID_DATA" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message, ErrorCode = "VALIDATION_ERROR" });
            }
            catch (BusinessException ex)
            {
                return Conflict(new ApiError { Message = ex.Message, ErrorCode = "BUSINESS_ERROR" });
            }
            catch (Exception ex) when (ex.Message.Contains("no encontrado"))
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (Exception ex) when (ex.Message.Contains("no se encuentra en un estado"))
            {
                return StatusCode(StatusCodes.Status409Conflict, new ApiError { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        /// <summary>
        /// Modificar un proyecto existente
        /// </summary>
        /// <param name="id">ID único del proyecto</param>
        /// <param name="request">Datos de actualización del proyecto</param>
        /// <response code="200">Proyecto actualizado exitosamente</response>
        /// <response code="404">Si el proyecto no existe</response>
        /// <response code="400">Si los datos de actualización son inválidos</response>
        /// <response code="409">Si el proyecto no puede ser modificado en su estado actual</response>
        [HttpPatch("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ProjectProposalCreateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] ProjectUpdate request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return BadRequest(new ApiError { Message = $"Datos de actualización inválidos: {errors}" });
                }

                var result = await _projectProposalService.UpdateProject(id, request);
                return Ok(result);
            }
            catch (MyInvalidDataException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message, ErrorCode = "INVALID_DATA" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message, ErrorCode = "VALIDATION_ERROR" });
            }
            catch (BusinessException ex)
            {
                return Conflict(new ApiError { Message = ex.Message, ErrorCode = "BUSINESS_ERROR" });
            }
            catch (Exception ex) when (ex.Message.Contains("no encontrado"))
            {
                return NotFound(new ApiError { Message = "Proyecto no encontrado" });
            }
            catch (Exception ex) when (ex.Message.Contains("no se encuentra en un estado"))
            {
                return StatusCode(StatusCodes.Status409Conflict,
                    new ApiError { Message = "El proyecto ya no se encuentra en un estado que permite modificaciones" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = "Datos de actualización inválidos" });
            }
        }

        /// <summary>
        /// Ver el detalle del proyecto
        /// </summary>
        /// <param name="id">ID único del proyecto</param>
        /// <response code="200">Retorna el detalle del proyecto</response>
        /// <response code="404">Si el proyecto no existe</response>
        /// <response code="400">Si hay un error en la solicitud</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProjectProposalCreateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetProjectDetail(Guid id)
        {
            try
            {
                var result = await _projectProposalService.GetProjectDetail(id);
                return Ok(result);
            }
            catch (MyInvalidDataException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message, ErrorCode = "INVALID_DATA" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message, ErrorCode = "VALIDATION_ERROR" });
            }
            catch (BusinessException ex)
            {
                return Conflict(new ApiError { Message = ex.Message, ErrorCode = "BUSINESS_ERROR" });
            }
            catch (Exception ex) when (ex.Message.Contains("no encontrado"))
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }
    }
}