using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence;
using Application.Interfaces;
using Infrastructure.Command;
using ADMProyectos;
using Application.Services;
using Infrastructure.Queris;
using Application.Mapper;
async Task Main()
{
    var dbContext = new AppDbContext();

    var serviceProvider = ConfigureServices(dbContext);

    var menu = serviceProvider.GetRequiredService<Menu>();

    menu.SetUsuarioActivo(1);

    await menu.displayMenu();
}
IServiceProvider ConfigureServices(AppDbContext dbContext)
{
    var services = new ServiceCollection();

    services.AddSingleton(dbContext);

    services.AddSingleton<IProjectProposalQuery, ProjectProposalQuery>();
    services.AddSingleton<IUserQuery, UserQuery>();
    services.AddSingleton<IAreaQuery, AreaQuery>();
    services.AddSingleton<IApprovalRuleQuery, ApprovalRuleQuery>();
    services.AddSingleton<IProjectTypeQuery, ProjectTypeQuery>();
    services.AddSingleton<IProjectApprovalStepQuery, ProjectApprovalStepQuery>();

    services.AddSingleton<IProjectProposalCommand, ProjectProposalCommand>();
    services.AddSingleton<IProjectApprovalStepCommand, ProjectApprovalStepCommand>();

    services.AddSingleton<IProjectProposalMapper, ProjectProposalMapper>();

    services.AddSingleton<IUserRoleService, UserRoleService>();
    services.AddSingleton<IProjectApprovalStepService, ProjectApprovalStepService>();
    services.AddSingleton<IProjectProposalService, ProjectProposalService>();

    services.AddSingleton<Menu>();

    return services.BuildServiceProvider();
}
await Main();

