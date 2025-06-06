using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Application.Exceptions;

namespace ADMProyectos.MenuActions
{
    public class ApprovalStepsAction
    {
        private readonly IProjectProposalService _projectProposalService;
        private readonly IUserRoleService _userRoleService;
        private readonly IProjectApprovalStepQuery _projectApprovalStepQuery;
        private readonly IProjectApprovalStepService _projectApprovalStepService;
        private readonly IUserQuery _userQuery;
        private readonly IProjectProposalQuery _projectProposalQuery;
        private readonly IProjectApprovalStepCommand _projectApprovalStepCommand;

        public ApprovalStepsAction(
            IProjectProposalService projectProposalService,
            IUserRoleService userRoleService,
            IProjectApprovalStepQuery projectApprovalStepQuery,
            IProjectApprovalStepService projectApprovalStepService,
            IUserQuery userQuery,
            IProjectProposalQuery projectProposalQuery,
            IProjectApprovalStepCommand projectApprovalStepCommand)
        {
            _projectProposalService = projectProposalService;
            _userRoleService = userRoleService;
            _projectApprovalStepQuery = projectApprovalStepQuery;
            _projectApprovalStepService = projectApprovalStepService;
            _userQuery = userQuery;
            _projectProposalQuery = projectProposalQuery;
            _projectApprovalStepCommand = projectApprovalStepCommand;
        }

        public async Task Execute()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== GESTIÓN DE PROPUESTAS PENDIENTES ===\n");
                var users = await _userQuery.GetAllUsers();
                foreach (var user in users)
                    Console.WriteLine($"{user.Id} - {user.Name} - Rol: {await _userRoleService.GetRoleById(user.Id)}");
                Console.WriteLine("\n0 - Volver al menú principal");
                Console.Write("\nSeleccione una opción: ");
                if (!int.TryParse(Console.ReadLine(), out int selectedUserId) || selectedUserId <= 0)
                    return;
                var selectedUser = users.FirstOrDefault(u => u.Id == selectedUserId);
                if (selectedUser == null)
                {
                    Console.WriteLine("Usuario no encontrado. Presione una tecla para continuar.");
                    Console.ReadKey();
                    continue;
                }
                int userRoleId = await _userRoleService.GetRoleById(selectedUserId);
                var allProposals = await _projectProposalQuery.GetAllProjectProposal(1, 1000);
                var pendingProposals = new List<ProjectProposal>();
                foreach (var proposal in allProposals)
                {
                    var steps = await _projectApprovalStepQuery.GetStepsByProjectId(proposal.Id);
                    var currentStep = steps
                        .Where(s => (s.Status == 1 || s.Status == 4) && s.ApproverRoleId == userRoleId)
                        .OrderBy(s => s.StepOrder)
                        .FirstOrDefault();
                    if (currentStep == null)
                    {
                        continue;
                    }
                    else
                    {
                        var userObservedStep = steps
                            .FirstOrDefault(s => s.Status == 4 && s.ApproverUserId == selectedUserId);
                        if (userObservedStep != null)
                        {
                            bool previousStepsApproved = steps
                                .Where(s => s.StepOrder < userObservedStep.StepOrder)
                                .All(s => s.Status == 2);
                            if (previousStepsApproved)
                            {
                                pendingProposals.Add(proposal);
                            }
                        }
                        else
                        {
                            bool previousStepsApproved = steps
                                .Where(s => s.StepOrder < currentStep.StepOrder)
                                .All(s => s.Status == 2);
                            if (previousStepsApproved)
                            {
                                pendingProposals.Add(proposal);
                            }
                        }
                    }
                }
                if (!pendingProposals.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nEste usuario no tiene propuestas pendientes para aprobar.");
                    Console.ResetColor();
                    Console.Write("\n¿Desea seleccionar otro usuario? (S/N): ");
                    var input = Console.ReadLine()?.Trim().ToLower();
                    if (input != "s") return;
                    continue;
                }
                Console.Clear();
                Console.WriteLine($"=== PROPUESTAS PENDIENTES PARA {selectedUser.Name.ToUpper()} (ROL: {userRoleId}) ===\n");
                for (int i = 0; i < pendingProposals.Count; i++)
                {
                    var proposal = pendingProposals[i];
                    var steps = await _projectApprovalStepQuery.GetStepsByProjectId(proposal.Id);
                    var currentStep = steps
                        .Where(s => (s.Status == 1 || s.Status == 4) && s.ApproverRoleId == userRoleId)
                        .OrderBy(s => s.StepOrder)
                        .FirstOrDefault();
                    Console.WriteLine($"{i + 1} - {proposal.Title} - {proposal.CreatedAt:dd/MM/yyyy} - ID: {proposal.Id}");
                    string stepStatus = currentStep?.Status switch
                    {
                        1 => "Pendiente",
                        2 => "Aprobado",
                        3 => "Rechazado",
                        4 => "Observado",
                        _ => "Desconocido"
                    };
                    Console.WriteLine($"   Paso actual: {currentStep?.StepOrder} - Estado: {stepStatus}");
                }
                Console.WriteLine("\n0 - Volver al menú anterior");
                int selection = 0;
                while (true)
                {
                    Console.Write("\nSeleccione una propuesta (número): ");
                    var input = Console.ReadLine();
                    if (input == "0") break;
                    if (int.TryParse(input, out selection) && selection >= 1 && selection <= pendingProposals.Count)
                        break;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Número inválido. Intente nuevamente.");
                    Console.ResetColor();
                }
                if (selection == 0 || selection > pendingProposals.Count)
                    continue;
                var selectedProposal = pendingProposals[selection - 1];
                var allSteps = await _projectApprovalStepQuery.GetStepsByProjectId(selectedProposal.Id);
                var currentActiveStep = allSteps
                    .Where(s => (s.Status == 1 || s.Status == 4) && s.ApproverRoleId == userRoleId)
                    .OrderBy(s => s.StepOrder)
                    .FirstOrDefault();
                if (currentActiveStep != null)
                {
                    Console.WriteLine($"\nDetalles de la propuesta: {selectedProposal.Title}");
                    Console.WriteLine($"Descripción: {selectedProposal.Description}");
                    Console.WriteLine($"Área: {selectedProposal.Area}");
                    Console.WriteLine($"Tipo: {selectedProposal.Type}");
                    Console.WriteLine($"Monto estimado: ${selectedProposal.EstimatedAmount}");
                    Console.WriteLine($"Duración: {selectedProposal.EstimatedDuration} días");
                    Console.Write("\nIngrese una opción: (A = Aprobar, R = Rechazar, O = Observar): ");
                    char decision = char.ToUpper(Console.ReadKey().KeyChar);
                    Console.WriteLine();
                    if (decision != 'A' && decision != 'R' && decision != 'O')
                    {
                        Console.WriteLine("Opción inválida.");
                        Console.WriteLine("Presione una tecla para continuar.");
                        Console.ReadKey();
                        continue;
                    }
                    Console.Write("\nIngrese un comentario (opcional): ");
                    string comment = Console.ReadLine() ?? "";
                    int newStatus = decision switch
                    {
                        'A' => 2,
                        'R' => 3,
                        'O' => 4,
                        _ => 1
                    };
                    var decisionStep = new DecisionStepDto
                    {
                        Id = (long)currentActiveStep.Id,
                        User = selectedUserId,
                        Status = newStatus,
                        Observation = comment
                    };
                    try
                    {
                        await _projectProposalService.ProcessDecision(selectedProposal.Id, decisionStep);
                        if (newStatus == 3)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nLa propuesta ha sido RECHAZADA.");
                        }
                        else if (newStatus == 4)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("\nLa propuesta ha sido OBSERVADA y devuelta para revisión.");
                        }
                        else if (newStatus == 2)
                        {
                            var remainingSteps = allSteps.Any(s => s.Status == 1);
                            if (!remainingSteps)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("\n¡La propuesta ha sido APROBADA!");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("\nPaso aprobado correctamente. La propuesta continúa al siguiente paso.");
                            }
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
                        Console.WriteLine("\nPresione una tecla para continuar.");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nError: No se encontró el paso activo de la propuesta para su rol.");
                    Console.ResetColor();
                    Console.WriteLine("\nPresione una tecla para continuar.");
                    Console.ReadKey();
                }
            }
        }
    }
}
