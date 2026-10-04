using FluentValidation;
using IdentityService.Application.UserModule.Contract.IService;
using IdentityService.Application.UserModule.Dto;
using MediatR;
using Shared.Logging.Contracts;

namespace IdentityService.Application.UserModule.Query.Get;

/// <summary>
/// Represents the GetUserByIdQuery component.
/// </summary>
public record GetUserByIdQuery(Guid? UserId)
    : IRequest<AuthenticatedUserDto>;

/// <summary>
/// Represents the GetUserByIdQueryValidator component.
/// </summary>
public class GetUserByIdQueryValidator
    : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEqual(Guid.Empty)
            .When(query => query.UserId.HasValue)
            .WithMessage("UserId cannot be empty.");
    }
}

/// <summary>
/// Represents the GetUserByIdQueryHandler component.
/// </summary>
public class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, AuthenticatedUserDto>
{
    private readonly IUserService _userService;
    private readonly ILoggerManager<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IUserService userService,
        ILoggerManager<GetUserByIdQueryHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<AuthenticatedUserDto> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching user. Requested user ID: {UserId}.",
            request.UserId);

        return await _userService.GetUserById(
            request.UserId,
            cancellationToken);
    }
}
