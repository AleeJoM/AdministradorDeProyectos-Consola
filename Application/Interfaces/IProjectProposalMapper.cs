using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProjectProposalMapper
    {
        Task<ProjectProposalResponse> GetProjectProposalResponse(ProjectProposal Project);
        Task<ProjectProposalCreateResponseDto> MapToResponseDto(ProjectProposal proposal);
    }
}
