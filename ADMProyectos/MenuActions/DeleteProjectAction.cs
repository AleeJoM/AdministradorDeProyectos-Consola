using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Exceptions;

namespace ADMProyectos.MenuActions
{
    public class DeleteProjectAction
    {
        private readonly IProjectProposalCommand _projectProposalCommand;
        private readonly IProjectProposalQuery _projectProposalQuery;
        public DeleteProjectAction(IProjectProposalCommand projectProposalCommand, IProjectProposalQuery projectProposalQuery)
        {
            _projectProposalCommand = projectProposalCommand;
            _projectProposalQuery = projectProposalQuery;
        }
        public async Task Execute()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ELIMINAR PROYECTO ===\n");
                var proposals = await _projectProposalQuery.GetAllProjectProposal(1, 1000);
                if (!proposals.Any())
                {
                    Console.WriteLine("No hay proyectos disponibles para eliminar.");
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
                    Console.Write("\nSeleccione un proyecto para eliminar (número): ");
                    var input = Console.ReadLine();
                    if (input == "0") return;
                    if (int.TryParse(input, out selection) && selection >= 1 && selection <= proposals.Count)
                        break;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Número inválido. Intente nuevamente.");
                    Console.ResetColor();
                }
                var selectedProposal = proposals[selection - 1];
                Console.Write($"\n¿Está seguro que desea eliminar el proyecto '{selectedProposal.Title}'? (s/n): ");
                var confirm = Console.ReadLine()?.ToLower();
                if (confirm != "s")
                {
                    Console.WriteLine("\nOperación cancelada.");
                    Console.WriteLine("\nPresione una tecla para continuar...");
                    Console.ReadKey();
                    continue;
                }
                try
                {
                    var success = await _projectProposalCommand.DeleteProposal(selectedProposal.Id);
                    if (success)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("El proyecto ha sido eliminado con éxito.");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("No se pudo eliminar el proyecto.");
                    }
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
