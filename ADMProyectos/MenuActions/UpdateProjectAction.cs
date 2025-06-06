using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Application.Interfaces;
using Application.Request;
using Application.Exceptions;

namespace ADMProyectos.MenuActions
{
    public class UpdateProjectAction
    {
        private readonly IProjectProposalService _projectProposalService;
        private readonly IProjectProposalQuery _projectProposalQuery;
        private readonly IAreaQuery _areaQuery;
        private readonly IProjectTypeQuery _projectTypeQuery;
        public UpdateProjectAction(
            IProjectProposalService projectProposalService,
            IProjectProposalQuery projectProposalQuery,
            IAreaQuery areaQuery,
            IProjectTypeQuery projectTypeQuery)
        {
            _projectProposalService = projectProposalService;
            _projectProposalQuery = projectProposalQuery;
            _areaQuery = areaQuery;
            _projectTypeQuery = projectTypeQuery;
        }
        public async Task Execute()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ACTUALIZAR PROPUESTA ===\n");
                var proposals = await _projectProposalService.GetAllProjects(1, 1000);
                if (!proposals.Any())
                {
                    Console.WriteLine("No hay propuestas disponibles.");
                    Console.WriteLine("\nPresione una tecla para continuar...");
                    Console.ReadKey();
                    return;
                }
                for (int i = 0; i < proposals.Count; i++)
                {
                    var p = proposals[i];
                    Console.WriteLine($"{i + 1} - {p.Title} - {p.CreatedAt:dd/MM/yyyy} - ID: {p.Id}");
                }
                Console.WriteLine("\n0 - Volver al menú principal");
                int selection;
                while (true)
                {
                    Console.Write("\nSeleccione una propuesta para actualizar (número): ");
                    var input = Console.ReadLine();
                    if (input == "0") return;
                    if (int.TryParse(input, out selection) && selection >= 1 && selection <= proposals.Count)
                        break;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Número inválido. Intente nuevamente.");
                    Console.ResetColor();
                }
                var selectedProposal = proposals[selection - 1];
                var project = await _projectProposalQuery.GetProjectById(selectedProposal.Id);
                if (project.Status == 3)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nNo se puede actualizar este proyecto porque ha sido RECHAZADO.");
                    Console.ResetColor();
                    Console.WriteLine("\nPresione una tecla para continuar...");
                    Console.ReadKey();
                    continue;
                }
                if (project.Status == 2)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nNo se puede actualizar este proyecto porque ya ha sido APROBADO.");
                    Console.ResetColor();
                    Console.WriteLine("\nPresione una tecla para continuar...");
                    Console.ReadKey();
                    continue;
                }
                var updateRequest = new ProjectUpdate();
                Console.Write($"Nuevo título (actual: {project.Title}, presione Enter para mantener): ");
                var newTitle = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newTitle)) updateRequest.Title = newTitle;
                Console.Write($"Nueva descripción (actual: {project.Description}, presione Enter para mantener): ");
                var newDescription = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newDescription)) updateRequest.Description = newDescription;
                Console.Write($"Nueva duración en días (actual: {project.EstimatedDuration}, presione Enter para mantener): ");
                var durationInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(durationInput) && int.TryParse(durationInput, out int newDuration))
                {
                    updateRequest.Duration = newDuration;
                }
                try
                {
                    var updated = await _projectProposalService.UpdateProject(selectedProposal.Id, updateRequest);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nProyecto actualizado exitosamente.");
                    if (!string.IsNullOrWhiteSpace(updateRequest.Title))
                        Console.WriteLine($"Título actualizado a: {updated.Title}");
                    if (!string.IsNullOrWhiteSpace(updateRequest.Description))
                        Console.WriteLine($"Descripción actualizada a: {updated.Description}");
                    if (updateRequest.Duration > 0)
                        Console.WriteLine($"Duración actualizada a: {updated.Duration} días");
                }
                catch (MyInvalidDataException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nError de validación: {ex.Message}");
                }
                catch (BusinessException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\nError de negocio: {ex.Message}");
                }
                catch (ValidationException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nError de validación: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nError inesperado: {ex.Message}");
                }
                finally
                {
                    Console.ResetColor();
                    Console.WriteLine("\nPresione una tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }
    }
}
