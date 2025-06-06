using Application.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Command
{
    public class ProjectProposalCommand : IProjectProposalCommand
    {
        private readonly AppDbContext _context;
        public ProjectProposalCommand(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ProjectProposal> CreateProposal(ProjectProposal project)
        {
            try
            {
                await _context.ProjectProposal.AddAsync(project);
                await _context.SaveChangesAsync();
                return project;
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No hay detalles adicionales";
                throw new ProjectCreationException($"No se pudo generar la propuesta. Error: {innerMessage}");
            }
        }
        public async Task<ProjectProposal> UpdateProposal(ProjectProposal Project)
        {
            try
            {
                _context.ProjectProposal.Update(Project);
                await _context.SaveChangesAsync();
                return Project;
            }
            catch (DbUpdateException)
            {
                throw new ProjectCreationException("No se pudo actualizar la propuesta.");
            }
        }
        public async Task<bool> DeleteProposal(Guid projectId)
        {
            var proposal = await _context.ProjectProposal
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (proposal == null)
            {
                throw new ProjectNotFoundException("No se encontró la propuesta para eliminar.");
            }
            var approvalSteps = await _context.ProjectApprovalStep
                .Where(pas => pas.ProjectProposalId == projectId)
                .ToListAsync();
            if (approvalSteps.Any())
            {
                _context.ProjectApprovalStep.RemoveRange(approvalSteps);
            }
            _context.ProjectProposal.Remove(proposal);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ProjectProposal> FinalizeProposal(Guid projectId)
        {
            var proposal = await _context.ProjectProposal
            .FirstOrDefaultAsync(p => p.Id == projectId);

            if (proposal == null)
            {
                throw new ProjectNotFoundException("No se encontró la propuesta para finalizar.");
            }

            var approvedStatusId = proposal.Status;
            proposal.Status = approvedStatusId;
            await _context.SaveChangesAsync();
            return proposal;
        }

        public async Task<ITransaction> BeginTransactionAsync()
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            return new Transaction(transaction);
        }

        public async Task<ProjectProposal> UpdateProposalStatus(Guid projectId, string decision)
        {
            var proposal = await _context.ProjectProposal
            .FirstOrDefaultAsync(p => p.Id == projectId);

            if (proposal == null)
            {
                throw new ProjectNotFoundException("No se encontró la propuesta para actualizar el estado.");
            }

            switch (decision)
            {
                case "Rechazado":
                    proposal.Status = 3;
                    break;
                case "Revisión":
                    proposal.Status = 1;
                    break;
                case "Aprobado":
                    proposal.Status = 2;
                    break;
                default:
                    throw new InvalidProjectStateException($"Decisión '{decision}' no válida. Use: Rechazado, Revisión, o Aprobado.");
            }

            _context.ProjectProposal.Update(proposal);
            await _context.SaveChangesAsync();
            return proposal;
        }
    }
}
