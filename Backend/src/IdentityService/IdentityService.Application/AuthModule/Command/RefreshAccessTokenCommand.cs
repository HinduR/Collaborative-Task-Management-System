using FluentValidation;
using IdentityService.Application.AuthenticationModule.Contract.IService;
using IdentityService.Application.AuthenticationModule.Dto;
using MediatR;

namespace IdentityService.Application.AuthenticationModule.Command.Refresh;

/// <summary>Represents a command to rotate a refresh token and generate a new token pair.</summary>
/// <param name="Request">The request containing the refresh token.</param>
public record RefreshAccessTokenCommand(
    RefreshAccessTokenRequest Request) : IRequest<TokenResponseDto>;

/// <summary>Validates the data supplied in a <see cref="RefreshAccessTokenCommand"/>.</summary>
public class RefreshAccessTokenCommandValidator
    : AbstractValidator<RefreshAccessTokenCommand>
{
    /// <summary>Initializes a new instance of the <see cref="RefreshAccessTokenCommandValidator"/> class and configures the validation rules.</summary>
    public RefreshAccessTokenCommandValidator()
    {
        RuleFor(x => x.Request.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}

/// <summary>Handles the <see cref="RefreshAccessTokenCommand"/> by generating a new token pair through the login service.</summary>
public class RefreshAccessTokenCommandHandler
    : IRequestHandler<RefreshAccessTokenCommand, TokenResponseDto>
{
    private readonly ILoginService _loginService;

    /// <summary>Initializes a new instance of the <see cref="RefreshAccessTokenCommandHandler"/> class.</summary>
    /// <param name="loginService">Service used to rotate refresh tokens and generate token pairs.</param>
    public RefreshAccessTokenCommandHandler(
        ILoginService loginService)
    {
        _loginService = loginService;
    }

    /// <summary>Handles the command and rotates the supplied refresh token.</summary>
    /// <param name="request">The command containing the refresh-token request.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The newly generated access-token and refresh-token pair.</returns>
    public Task<TokenResponseDto> Handle(
        RefreshAccessTokenCommand request,
        CancellationToken cancellationToken)
    {
        return _loginService.RefreshAccessTokenAsync(
            request.Request.RefreshToken,
            cancellationToken);
    }
}
