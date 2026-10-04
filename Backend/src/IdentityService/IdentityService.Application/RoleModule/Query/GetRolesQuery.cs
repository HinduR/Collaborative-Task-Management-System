using IdentityService.Application.RoleModule.Contract.IService;
using IdentityService.Application.RoleModule.Dto;
using IdentityService.Domain.Models;
using MediatR;
using Shared.Logging.Contracts;

namespace IdentityService.Application.RoleModule.Query.Get;

/// <summary>
/// Represents the GetRolesQuery component.
/// </summary>
public record GetRolesQuery : IRequest<List<RoleDto>>;

/// <summary>
/// Represents the GetRolesQueryHandler component.
/// </summary>
public class GetRolesQueryHandler
    : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly IRoleService _roleService;
    private readonly ILoggerManager<GetRolesQueryHandler> _logger;

    public GetRolesQueryHandler(
        IRoleService roleService,
        ILoggerManager<GetRolesQueryHandler> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    public async Task<List<RoleDto>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching active roles.");

        List<Role> roles = await _roleService.GetActiveRolesAsync(
            cancellationToken);

        List<RoleDto> response = roles
            .Select(role => new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description
            })
            .ToList();

        _logger.LogInformation(
            "Returned {RoleCount} active roles.",
            response.Count);

        return response;
    }
}
