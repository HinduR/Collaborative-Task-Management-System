using FluentValidation;
using IdentityService.Application.UserModule.Contract.IService;
using IdentityService.Application.UserModule.Dto;
using MediatR;
using Shared.Logging.Contracts;

namespace IdentityService.Application.UserModule.Command.AssignRole;

/// <summary>
/// Represents the AssignUserRoleCommand component.
/// </summary>
public record AssignUserRoleCommand(
    Guid UserId,
    AssignUserRoleRequestDto Request)
    : IRequest<UserRoleMappingResponseDto>;

/// <summary>
/// Represents the AssignUserRoleCommandValidator component.
/// </summary>
public class AssignUserRoleCommandValidator
    : AbstractValidator<AssignUserRoleCommand>
{
    public AssignUserRoleCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(command => command.Request)
            .NotNull()
            .WithMessage("Role assignment request is required.");

        When(
            command => command.Request is not null,
            () =>
            {
                RuleFor(command =>
                        command.Request.RoleId)
                    .NotEmpty()
                    .WithMessage(
                        "Role ID is required.");
            });
    }
}

/// <summary>
/// Represents the AssignUserRoleCommandHandler component.
/// </summary>
public class AssignUserRoleCommandHandler
    : IRequestHandler<
        AssignUserRoleCommand,
        UserRoleMappingResponseDto>
{
    private readonly IUserRoleService
        _userRoleService;

    private readonly ILoggerManager<
        AssignUserRoleCommandHandler> _logger;

    public AssignUserRoleCommandHandler(
        IUserRoleService userRoleService,
        ILoggerManager<
            AssignUserRoleCommandHandler> logger)
    {
        _userRoleService = userRoleService;
        _logger = logger;
    }

    public async Task<UserRoleMappingResponseDto>
        Handle(
            AssignUserRoleCommand command,
            CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Assigning role {RoleId} to user {UserId}.",
            command.Request.RoleId,
            command.UserId);

        return await _userRoleService.AssignRoleAsync(
            command.UserId,
            command.Request.RoleId,
            cancellationToken);
    }
}
