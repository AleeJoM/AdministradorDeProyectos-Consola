using Application.Interfaces;
using Application.Services;
using Infrastructure.Queris;
using Infrastructure.Command;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Mapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Solicitud de Proyecto",
        Version = "1.0",
    });

    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
            return new[] { api.GroupName };
        return new[] { api.ActionDescriptor.RouteValues["controller"] };
    });

    c.UseAllOfToExtendReferenceSchemas();

    c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}");

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = null;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(@"Server=DESKTOP-ALEJO;Database=ADMProyectos;Trusted_Connection=True;TrustServerCertificate=true;"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IApproverRoleQuery, ApproverRoleQuery>();
builder.Services.AddScoped<IProjectProposalMapper, ProjectProposalMapper>();
builder.Services.AddScoped<IApprovalRuleQuery, ApprovalRuleQuery>();
builder.Services.AddScoped<IProjectProposalCommand, ProjectProposalCommand>();
builder.Services.AddScoped<IProjectProposalService, ProjectProposalService>();
builder.Services.AddScoped<IProjectApprovalStepService, ProjectApprovalStepService>();
builder.Services.AddScoped<IProjectProposalQuery, ProjectProposalQuery>();
builder.Services.AddScoped<IProjectApprovalStepQuery, ProjectApprovalStepQuery>();
builder.Services.AddScoped<IProjectApprovalStepCommand, ProjectApprovalStepCommand>();
builder.Services.AddScoped<IAreaQuery, AreaQuery>();
builder.Services.AddScoped<IProjectTypeQuery, ProjectTypeQuery>();
builder.Services.AddScoped<IUserQuery, UserQuery>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
