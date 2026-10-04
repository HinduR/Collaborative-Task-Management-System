using FluentValidation;
using IdentityService.Application.AuthenticationModule.Contract.IService;
using IdentityService.Application.AuthenticationModule.Dto;
using MediatR;
using Shared.Logging.Contracts;

namespace IdentityService.Application.AuthenticationModule.Command;

/// <summary>
/// Exchanges a one-time Google login code for application tokens.
/// </summary>
/// <param name="Request">The exchange request.</param>
public record ExchangeGoogleLoginCodeCommand(
    GoogleLoginCodeExchangeRequest Request) : IRequest<TokenResponseDto>;

public class ExchangeGoogleLoginCodeCommandValidator
    : AbstractValidator<ExchangeGoogleLoginCodeCommand>
{
    public ExchangeGoogleLoginCodeCommandValidator()
    {
        RuleFor(command => command.Request.Code)
            .NotEmpty()
            .WithMessage("Login code is required.");
    }
}

/// <summary>
/// Handles the exchange of a one-time Google login code for application tokens.
/// </summary>
public class ExchangeGoogleLoginCodeCommandHandler
    : IRequestHandler<
        ExchangeGoogleLoginCodeCommand,
        TokenResponseDto>
{
    private readonly ILoginService _loginService;
    private readonly ILoggerManager<
        ExchangeGoogleLoginCodeCommandHandler> _logger;

    public ExchangeGoogleLoginCodeCommandHandler(
         ILoginService loginService,
         ILoggerManager<
             ExchangeGoogleLoginCodeCommandHandler> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }

    public Task<TokenResponseDto> Handle(
        ExchangeGoogleLoginCodeCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Google login-code exchange started.");
        
        Task<TokenResponseDto>? tokenResponse = _loginService.ExchangeGoogleLoginCodeAsync(
           request.Request.Code,
           cancellationToken);

        _logger.LogInformation(
              "Google login-code exchange completed successfully.");

        return tokenResponse;
    }
}
