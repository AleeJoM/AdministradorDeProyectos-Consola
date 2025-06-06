using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Application.Interfaces;

namespace ADMProyectos.MenuActions
{
    public class ViewProposalAction
    {
        private readonly IProjectProposalService _projectProposalService;
        private readonly IProjectProposalQuery _projectProposalQuery;
        public ViewProposalAction(IProjectProposalService projectProposalService, IProjectProposalQuery projectProposalQuery)
        {
            _projectProposalService = projectProposalService;
            _projectProposalQuery = projectProposalQuery;
        }
        public async Task Execute()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ESTADO DE LA PROPUESTA ===\n");
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
                    Console.Write("\nSeleccione una propuesta (número): ");
                    var input = Console.ReadLine();
                    if (input == "0") return;
                    if (int.TryParse(input, out selection) && selection >= 1 && selection <= proposals.Count)
                        break;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: debe ingresar un número válido del listado.");
                    Console.ResetColor();
                }
                var selectedProposal = proposals[selection - 1];
                var status = await _projectProposalService.GetProjectStatus(selectedProposal.Id);
                Console.Clear();
                Console.WriteLine($"=== DETALLE DE PROPUESTA ID: {selectedProposal.Id} ===\n");
                Console.WriteLine($"Título: {status.Title}");
                Console.WriteLine($"Descripción: {status.Description}");
                Console.WriteLine($"Área: {status.Area}");
                Console.WriteLine($"Tipo: {status.Type}");
                Console.Write($"Estado actual: ");
                switch (status.CurrentStatus)
                {
                    case "Pendiente":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case "Aprobado":
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case "Rechazado":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case "Observado":
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                }
                Console.WriteLine(status.CurrentStatus);
                Console.ResetColor();
                Console.WriteLine($"Creado por: {status.CreatedBy}");
                Console.WriteLine("\n=== HISTORIAL DE ACCIONES ===\n");
                Console.WriteLine(status.History);
                Console.Write("\nPresione una tecla para continuar...");
                Console.ReadKey();
            }
        }
    }
}
