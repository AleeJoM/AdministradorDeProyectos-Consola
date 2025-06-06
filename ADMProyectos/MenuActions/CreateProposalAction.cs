using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Application.Request;
using Application.Services;
using Domain.Entities;
using Application.Validation;
using Application.Exceptions;

namespace ADMProyectos.MenuActions
{
    public class CreateProposalAction
    {
        private readonly IProjectProposalService _projectProposalService;
        private readonly IAreaQuery _areaQuery;
        private readonly IProjectTypeQuery _projectTypeQuery;
        private readonly IProjectApprovalStepCommand _projectApprovalStepCommand;
        private readonly IProjectProposalCommand _projectProposalCommand;
        private readonly IProjectApprovalStepService _projectApprovalStepService;
        private readonly IUserQuery _userQuery;
        private readonly IUserRoleService _userRoleService;

        public CreateProposalAction(
            IProjectProposalService projectProposalService,
            IAreaQuery areaQuery,
            IProjectTypeQuery projectTypeQuery,
            IProjectApprovalStepCommand projectApprovalStepCommand,
            IProjectProposalCommand projectProposalCommand,
            IProjectApprovalStepService projectApprovalStepService,
            IUserQuery userQuery,
            IUserRoleService userRoleService)
        {
            _projectProposalService = projectProposalService;
            _areaQuery = areaQuery;
            _projectTypeQuery = projectTypeQuery;
            _projectApprovalStepCommand = projectApprovalStepCommand;
            _projectProposalCommand = projectProposalCommand;
            _projectApprovalStepService = projectApprovalStepService;
            _userQuery = userQuery;
            _userRoleService = userRoleService;
        }

        public async Task Execute()
        {
            Console.Clear();
            Console.WriteLine("=== CREE UNA PROPUESTA DE PROYECTO ===\n");
            Console.Write("Título: ");
            string title = Console.ReadLine();
            Console.Write("Descripción: ");
            string description = Console.ReadLine();
            Console.Write("Seleccione un Área:");
            var areas = await _areaQuery.GetAllAreas();
            foreach (var a in areas)
                Console.Write($"\n {a.Id} - {a.Name} ");
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
            var types = await _projectTypeQuery.GetAllType();
            foreach (var t in types)
                Console.Write($"\n {t.Id} - {t.Name} ");
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
            try
            {
                ValidationUtils.ThrowIfNullOrEmpty(title, "Título");
                ValidationUtils.ThrowIfNullOrEmpty(description, "Descripción");
            }
            catch (MyInvalidDataException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError de validación: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nPresione una tecla para continuar...");
                Console.ReadKey();
                return;
            }
            try
            {
                ValidationUtils.ThrowIfNegative(amount, "Monto estimado");
                ValidationUtils.ThrowIfOutOfRange(duration, 1, int.MaxValue, "Duración");
            }
            catch (MyInvalidDataException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError de validación: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nPresione una tecla para continuar...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Seleccione el usuario que desea proponer el Proyecto:");
            var users = await _userQuery.GetAllUsers();
            foreach (var user in users)
                Console.WriteLine($"{user.Id} - {user.Name} - Rol: {await _userRoleService.GetRoleById(user.Id)}");
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
                Amount = amount,
                Duration = duration
            };
            var newProposal = await _projectProposalService.InsertProject(proposal, selectedUserId);
            Console.WriteLine($"\nPropuesta creada con éxito. ID del Proyecto: {newProposal.Id}");
            Console.WriteLine($"Asignada al usuario: {users.FirstOrDefault(u => u.Id == selectedUserId)?.Name}");
            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }
    }
}
