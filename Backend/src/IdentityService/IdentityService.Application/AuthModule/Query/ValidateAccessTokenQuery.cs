using FluentValidation;
using IdentityService.Application.AuthenticationModule.Contract.IService;
using MediatR;

namespace IdentityService.Application.AuthenticationModule.Query.Validate;

/// <summary>Represents a query to validate an access token.</summary>
/// <param name="AccessToken">The access token to validate.</param>
public record ValidateAccessTokenQuery(
    string AccessToken) : IRequest<bool>;

/// <summary>Validates the access token supplied in a <see cref="ValidateAccessTokenQuery"/>.</summary>
public class ValidateAccessTokenQueryValidator
    : AbstractValidator<ValidateAccessTokenQuery>
{
    /// <summary>Initializes a new instance of the <see cref="ValidateAccessTokenQueryValidator"/> class and configures the validation rules.</summary>
    public ValidateAccessTokenQueryValidator()
    {
        RuleFor(item => item.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required.");
    }
}

/// <summary>Handles the <see cref="ValidateAccessTokenQuery"/> by validating the token through the authentication service.</summary>
public class ValidateAccessTokenQueryHandler
    : IRequestHandler<ValidateAccessTokenQuery, bool>
{
    private readonly IAuthenticationService _authenticationService;

    /// <summary>Initializes a new instance of the <see cref="ValidateAccessTokenQueryHandler"/> class.</summary>
    /// <param name="authenticationService">Service used to validate access tokens.</param>
    public ValidateAccessTokenQueryHandler(
        IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>Handles the query and validates the supplied access token.</summary>
    /// <param name="request">The query containing the access token.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> when the access token is valid; otherwise, <see langword="false"/>.</returns>
    public Task<bool> Handle(
        ValidateAccessTokenQuery request,
        CancellationToken cancellationToken)
    {
        return _authenticationService.ValidateAccessTokenAsync(
            request.AccessToken,
            cancellationToken);
    }
}
