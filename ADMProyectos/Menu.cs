using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Request;
using Application.Services;
using Domain.Entities;
using Infrastructure.Command;
using Infrastructure.Queris;
using Microsoft.Identity.Client;

namespace ADMProyectos
{
    public class Menu
    {
        private readonly IProjectProposalService _projectProposalService;
        private readonly IAreaQuery _areaQuery;
        private readonly IProjectTypeQuery _projectTypeQuery;
        private readonly IProjectProposalQuery _projectProposalQuery;
        private readonly IProjectProposalCommand _projectProposalCommand;
        private readonly IProjectApprovalStepCommand _projectApprovalStepCommand;
        private readonly IProjectApprovalStepService _projectApprovalStepService;
        private readonly IUserRoleService _userRoleService;
        private readonly IProjectApprovalStepQuery _projectApprovalStepQuery;
        private readonly IUserQuery _userQuery;
        private int _userActiveId;

        public Menu(
            IProjectProposalService projectProposalService,
            IAreaQuery areaQuery,
            IProjectTypeQuery projectTypeQuery,
            IProjectProposalQuery projectProposalQuery,
            IProjectProposalCommand projectProposalCommand,
            IProjectApprovalStepCommand projectApprovalStepCommand,
            IProjectApprovalStepService projectApprovalStepService,
            IUserRoleService userRoleService,
            IProjectApprovalStepQuery projectApprovalStepQuery,
            IUserQuery userQuery
            )
        {
            _projectProposalService = projectProposalService;
            _areaQuery = areaQuery;
            _projectTypeQuery = projectTypeQuery;
            _projectProposalQuery = projectProposalQuery;
            _projectProposalCommand = projectProposalCommand;
            _projectApprovalStepCommand = projectApprovalStepCommand;
            _projectApprovalStepService = projectApprovalStepService;
            _userRoleService = userRoleService;
            _projectApprovalStepQuery = projectApprovalStepQuery;
            _userQuery = userQuery;
        }
        public void SetUsuarioActivo(int userId)
        {
            _userActiveId = userId;
        }
        public async Task displayMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("========== SISTEMA ADMINISTRADOR DE PROYECTOS ==========");
                Console.WriteLine("Seleccione una opción: ");
                Console.WriteLine("1 - Crear una propuesta de Proyecto");
                Console.WriteLine("2 - Ver solicitud del Proyecto");
                Console.WriteLine("3 - Aprobaciones y rechazos");
                Console.WriteLine("4 - Actualizar Proyecto");
                Console.WriteLine("5 - Eliminar Proyecto");
                Console.WriteLine("0 - Salir");
                Console.WriteLine("========================================================");
                Console.Write("Opción: ");
                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        await CreateProposal(_projectProposalService, 
                            _areaQuery,
                            _projectTypeQuery, 
                            _projectApprovalStepCommand, 
                            _projectProposalCommand,
                            _projectApprovalStepService,
                            _userQuery,
                            _userRoleService);
                        break;
                    case "2":
                        await ViewProposal(_projectProposalService, _projectProposalQuery);
                        break;
                    case "3":
                        await ApprovalSteps(
                            _projectProposalService,
                            _userRoleService,
                            _projectApprovalStepQuery,
                            _projectApprovalStepService,
                            _userQuery,
                            _projectProposalQuery,
                            _projectApprovalStepCommand);
                        break;
                    case "4":
                        await UpdateProject(_projectProposalService, _projectProposalQuery, _areaQuery, _projectTypeQuery);
                        break;
                    case "5":
                        await DeleteProject(_projectProposalCommand, _projectProposalQuery);
                        break;
                    case "0":
                        Console.WriteLine("Saliendo del sistema...");
                        return;
                    default:
                        Console.WriteLine("Opción inválida. Presione una tecla para continuar.");
                        Console.ReadKey();
                        break;
                }
            }
        }
        private async Task CreateProposal(
            IProjectProposalService projectProposalService,
            IAreaQuery areaQuery,
            IProjectTypeQuery projectTypeQuery,
            IProjectApprovalStepCommand projectApprovalStepCommand,
            IProjectProposalCommand projectProposalCommand,
            IProjectApprovalStepService projectApprovalStepService,
            IUserQuery userQuery,
            IUserRoleService userRoleService
            )
        {
            Console.Clear();
            Console.WriteLine("=== CREE UNA PROPUESTA DE PROYECTO ===\n");

            Console.Write("Título: ");
            string title = Console.ReadLine();

            Console.Write("Descripción: ");
            string description = Console.ReadLine();

            Console.Write("Seleccione un Área:");
            var areas = await areaQuery.GetAllAreas();
            foreach (var a in areas)
            {
                Console.Write($"\n {a.Id} - {a.Name} ");
            }
            int area;
            while (true)
            {
                Console.Write("\nIngrese una opción: ");
                var inputArea = Console.ReadLine();
                if (int.TryParse(inputArea, out area) && areas.Any(a => a.Id == area))
                    break;

                Console.WriteLine("Opción inválida. Intente nuevamente.");
            }

            Console.Write("Seleccione el Tipo: ");
            var types = await projectTypeQuery.GetAllType();
            foreach (var t in types)
            {
                Console.Write($"\n {t.Id} - {t.Name} ");
            }
            int type;
            while (true)
            {
                Console.Write("\nIngrese una opción: ");
                var inputType = Console.ReadLine();
                if (int.TryParse(inputType, out type) && types.Any(t => t.Id == type))
                    break;
                Console.WriteLine("Entrada inválida. Intente nuevamente.");
            }

            decimal amount;
            while (true)
            {
                Console.Write("Monto estimado: ");
                if (decimal.TryParse(Console.ReadLine(), out amount))
                    break;
                Console.WriteLine("Ingrese un número válido.");
            }

            int duration;
            while (true)
            {
                Console.Write("Duración (en días): ");
                if (int.TryParse(Console.ReadLine(), out duration))
                    break;

                Console.WriteLine("Ingrese un número entero válido.");
            }

            Console.WriteLine("Seleccione el usuario que desea proponer el Proyecto:");
            var users = await userQuery.GetAllUsers();
            foreach (var user in users)
            {
                Console.WriteLine($"{user.Id} - {user.Name} - Rol: {await userRoleService.GetRoleById(user.Id)}");
            }

            int selectedUserId;
            while (true)
            {
                Console.Write("\nIngrese el ID del usuario: ");
                if (int.TryParse(Console.ReadLine(), out selectedUserId) && users.Any(u => u.Id == selectedUserId))
                    break;

                Console.WriteLine("ID de usuario inválido. Intente nuevamente.");
            }

            var proposal = new ProjectProposalRequest
            {
                Title = title,
                Description = description,
                Area = area,
                Type = type,
                EstimatedAmount = amount,
                EstimatedDuration = duration
            };

            var newProposal = await projectProposalService.InsertProject(proposal, selectedUserId);

            Console.WriteLine($"\nPropuesta creada con éxito. ID del Proyecto: {newProposal.Id}");
            Console.WriteLine($"Asignada al usuario: {users.FirstOrDefault(u => u.Id == selectedUserId)?.Name}");

            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }
        private async Task ViewProposal(IProjectProposalService projectProposalService, IProjectProposalQuery projectProposalQuery)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ESTADO DE LA PROPUESTA ===\n");

                var proposals = await projectProposalService.GetAllProjects(1, 1000);

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
                    Console.WriteLine($"{i + 1} - {p.Title} - {p.CreateAt:dd/MM/yyyy} - ID: {p.Id}");
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
                var status = await projectProposalService.GetProjectStatus(selectedProposal.Id);

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
        private async Task ApprovalSteps(
            IProjectProposalService projectProposalService,
            IUserRoleService userRoleService,
            IProjectApprovalStepQuery projectApprovalStepQuery,
            IProjectApprovalStepService projectApprovalStepService,
            IUserQuery userQuery,
            IProjectProposalQuery projectProposalQuery,
            IProjectApprovalStepCommand projectApprovalStepCommand
            )
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== GESTIÓN DE PROPUESTAS PENDIENTES ===\n");

                var users = await userQuery.GetAllUsers();
                foreach (var user in users)
                    Console.WriteLine($"{user.Id} - {user.Name} - Rol: {await userRoleService.GetRoleById(user.Id)}");

                Console.WriteLine("\n0 - Volver al menú principal");

                Console.Write("\nIngrese el ID del usuario: ");
                if (!int.TryParse(Console.ReadLine(), out int selectedUserId) || selectedUserId <= 0)
                    return;

                var selectedUser = users.FirstOrDefault(u => u.Id == selectedUserId);
                if (selectedUser == null)
                {
                    Console.WriteLine("Usuario no encontrado. Presione una tecla para continuar.");
                    Console.ReadKey();
                    continue;
                }

                int userRoleId = await userRoleService.GetRoleById(selectedUserId);

                var allProposals = await projectProposalQuery.GetAllProjectProposal(1, 1000);
                var pendingProposals = new List<ProjectProposal>();

                foreach (var proposal in allProposals)
                {
                    var steps = await projectApprovalStepQuery.GetStepsByProjectId(proposal.Id);

                    var currentStep = steps
                        .Where(s => (s.Status == 1 || (s.Status == 4 && s.ApproverUserId == selectedUserId)) &&
                            s.ApproverRoleId == userRoleId)
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
                    var steps = await projectApprovalStepQuery.GetStepsByProjectId(proposal.Id);
                    var currentStep = steps
                        .Where(s =>
                            (s.Status == 1 || (s.Status == 4 && s.ApproverUserId == selectedUserId)) &&
                            s.ApproverRoleId == userRoleId)
                        .OrderBy(s => s.StepOrder)
                        .FirstOrDefault();

                    Console.WriteLine($"{i + 1} - {proposal.Title} - {proposal.CreateAt:dd/MM/yyyy} - ID: {proposal.Id}");
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
                var allSteps = await projectApprovalStepQuery.GetStepsByProjectId(selectedProposal.Id);
                var currentActiveStep = allSteps
                    .Where(s =>
                        (s.Status == 1 || (s.Status == 4 && s.ApproverUserId == selectedUserId)) &&
                        s.ApproverRoleId == userRoleId)
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

                    Console.WriteLine("0 - Volver al listado de propuestas");

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

                    try
                    {
                        int newStatus;
                        switch (decision)
                        {
                            case 'A':
                                newStatus = 2;
                                break;
                            case 'R':
                                newStatus = 3;
                                break;
                            case 'O':
                                newStatus = 4;
                                break;
                            default:
                                newStatus = 1;
                                break;
                        }

                        if (newStatus == 2)
                        {
                            currentActiveStep.ApproverUserId = selectedUserId;
                            currentActiveStep.Status = 2;
                            currentActiveStep.Observations = comment;
                            currentActiveStep.DecisionDate = DateTime.Now;

                            await projectApprovalStepCommand.UpdateStep(currentActiveStep);

                            var duplicateSteps = allSteps
                                .Where(s =>
                                    s.StepOrder == currentActiveStep.StepOrder &&
                                    s.ApproverRoleId == currentActiveStep.ApproverRoleId &&
                                    s.Id != currentActiveStep.Id &&
                                    s.Status == 1)
                                .ToList();

                            foreach (var duplicate in duplicateSteps)
                            {
                                duplicate.Status = 2;
                                duplicate.ApproverUserId = null;
                                duplicate.Observations = "Aprobado automáticamente porque otro usuario del mismo rol ya tomó acción.";
                                duplicate.DecisionDate = DateTime.Now;
                                await projectApprovalStepCommand.UpdateStep(duplicate);
                            }
                        }
                        else if (newStatus == 3 || newStatus == 4)
                        {
                            currentActiveStep.ApproverUserId = selectedUserId;
                            currentActiveStep.Status = newStatus;
                            currentActiveStep.Observations = comment;
                            currentActiveStep.DecisionDate = DateTime.Now;
                            await projectApprovalStepCommand.UpdateStep(currentActiveStep);
                        }

                        if (newStatus == 3)
                        {
                            selectedProposal.Status = 3;
                            await projectProposalService.UpdateProject(selectedProposal);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nLa propuesta ha sido RECHAZADA.");
                        }
                        else if (newStatus == 4)
                        {
                            selectedProposal.Status = 4;
                            await projectProposalService.UpdateProject(selectedProposal);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("\nLa propuesta ha sido OBSERVADA y devuelta para revisión.");
                        }
                        else if (newStatus == 2)
                        {
                            var remainingSteps = allSteps.Any(s => s.Status == 1);

                            if (!remainingSteps)
                            {
                                selectedProposal.Status = 2;
                                await projectProposalService.UpdateProject(selectedProposal);
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
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\nError: {ex.Message}");
                        if (ex.InnerException != null)
                            Console.WriteLine($"Detalle interno: {ex.InnerException.Message}");
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
        private async Task UpdateProject(IProjectProposalService projectProposalService,
            IProjectProposalQuery projectProposalQuery,
            IAreaQuery areaQuery,
            IProjectTypeQuery projectTypeQuery)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ACTUALIZAR PROPUESTA ===\n");

                var proposals = await projectProposalService.GetAllProjects(1, 1000);

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
                    Console.WriteLine($"{i + 1} - {p.Title} - {p.CreateAt:dd/MM/yyyy} - ID: {p.Id}");
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
                var project = await projectProposalQuery.GetProjectById(selectedProposal.Id);

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

                Console.Write($"Nuevo título (actual: {project.Title}): ");
                var newTitle = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newTitle)) project.Title = newTitle;

                Console.Write($"Nueva descripción (actual: {project.Description}): ");
                var newDescription = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newDescription)) project.Description = newDescription;

                Console.Write($"Nuevo monto estimado (actual: {project.EstimatedAmount}): ");
                if (decimal.TryParse(Console.ReadLine(), out decimal newAmount)) project.EstimatedAmount = newAmount;

                Console.Write($"Nueva duración (días, actual: {project.EstimatedDuration}): ");
                if (int.TryParse(Console.ReadLine(), out int newDuration)) project.EstimatedDuration = newDuration;

                var areas = await areaQuery.GetAllAreas();
                Console.WriteLine("\nÁreas disponibles:");
                for (int i = 0; i < areas.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {areas[i].Name}");
                }

                int areaSelection;
                while (true)
                {
                    Console.Write("Seleccione un nuevo área (número): ");
                    if (int.TryParse(Console.ReadLine(), out areaSelection) && areaSelection >= 1 && areaSelection <= areas.Count)
                    {
                        project.Area = areas[areaSelection - 1].Id;
                        break;
                    }

                    Console.WriteLine("Área inválida. Intente nuevamente.");
                }

                var types = await projectTypeQuery.GetAllType();
                Console.WriteLine("\nTipos disponibles:");
                for (int i = 0; i < types.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {types[i].Name}");
                }

                int typeSelection;
                while (true)
                {
                    Console.Write("Seleccione un nuevo tipo (número): ");
                    if (int.TryParse(Console.ReadLine(), out typeSelection) && typeSelection >= 1 && typeSelection <= types.Count)
                    {
                        project.Type = types[typeSelection - 1].Id;
                        break;
                    }

                    Console.WriteLine("Tipo inválido. Intente nuevamente.");
                }

                var updated = await projectProposalService.UpdateProject(project);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nProyecto actualizado exitosamente.");
                Console.ResetColor();
                Console.WriteLine("\nPresione una tecla para continuar...");
                Console.ReadKey();
            }
        }
        private async Task DeleteProject(IProjectProposalCommand projectProposalCommand, IProjectProposalQuery projectProposalQuery)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ELIMINAR PROYECTO ===\n");

                var proposals = await projectProposalQuery.GetAllProjectProposal(1, 1000);

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
                    Console.WriteLine($"{i + 1} - {p.Title} - {p.CreateAt:dd/MM/yyyy} - ID: {p.Id}");
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
                    var success = await projectProposalCommand.DeleteProposal(selectedProposal.Id);
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
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
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
