using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Response;
using Domain.Entities;

namespace Application.Mapper
{
    public class ProjectProposalMapper : IProjectProposalMapper
    {
        public ProjectProposalMapper() { }
        public async Task<ProjectProposalResponse> GetProjectProposalResponse(ProjectProposal Project)
        {
            return new ProjectProposalResponse
            {
                Id = Project.Id,
                Title = Project.Title,
                Description = Project.Description,
                EstimatedAmount = Project.EstimatedAmount,
                EstimatedDuration = Project.EstimatedDuration,
                Area = Project.Area,
                Type = Project.Type,
                CreateBy = Project.CreateBy
            };
        }
    }
}
