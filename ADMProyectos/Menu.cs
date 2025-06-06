using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Application.Request;
using Application.Services;
using Domain.Entities;
using Infrastructure.Command;
using Infrastructure.Queris;
using Microsoft.Identity.Client;
using Application.Validation;
using Application.Exceptions;
using ADMProyectos.MenuActions;

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
            var createProposalAction = new CreateProposalAction(_projectProposalService, _areaQuery, _projectTypeQuery, _projectApprovalStepCommand, _projectProposalCommand, _projectApprovalStepService, _userQuery, _userRoleService);
            var viewProposalAction = new ViewProposalAction(_projectProposalService, _projectProposalQuery);
            var approvalStepsAction = new ApprovalStepsAction(_projectProposalService, _userRoleService, _projectApprovalStepQuery, _projectApprovalStepService, _userQuery, _projectProposalQuery, _projectApprovalStepCommand);
            var updateProjectAction = new UpdateProjectAction(_projectProposalService, _projectProposalQuery, _areaQuery, _projectTypeQuery);
            var deleteProjectAction = new DeleteProjectAction(_projectProposalCommand, _projectProposalQuery);
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
                        await createProposalAction.Execute();
                        break;
                    case "2":
                        await viewProposalAction.Execute();
                        break;
                    case "3":
                        await approvalStepsAction.Execute();
                        break;
                    case "4":
                        await updateProjectAction.Execute();
                        break;
                    case "5":
                        await deleteProjectAction.Execute();
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
    }
}
