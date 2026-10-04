using FluentValidation;
using IdentityService.Application.UserModule.Contract.IService;
using IdentityService.Application.UserModule.Dto;
using MediatR;
using Shared.Logging.Contracts;

namespace IdentityService.Application.UserModule.Query.List;

/// <summary>
/// Represents the GetUsersQuery component.
/// </summary>
public record GetUsersQuery(string? Search)
    : IRequest<UserListResponseDto>;

/// <summary>
/// Represents the GetUsersQueryValidator component.
/// </summary>
public class GetUsersQueryValidator
    : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(150)
            .WithMessage("Search cannot exceed 150 characters.");
    }
}

/// <summary>
/// Represents the GetUsersQueryHandler component.
/// </summary>
public class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, UserListResponseDto>
{
    private readonly IUserService _userService;
    private readonly ILoggerManager<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        IUserService userService,
        ILoggerManager<GetUsersQueryHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<UserListResponseDto> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching application users.");

        UserListResponseDto result = await _userService.GetUsers(
            request.Search,
            cancellationToken);

        _logger.LogInformation(
            "Fetched {Count} application users.",
            result.Items.Count);

        return result;
    }
}
