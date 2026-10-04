using FluentValidation;
using IdentityService.Application.AuthenticationModule.Contract.IService;
using IdentityService.Application.AuthenticationModule.Dto;
using MediatR;

namespace IdentityService.Application.AuthenticationModule.Command;

/// <summary>
/// Creates a one-time login code after Google authentication succeeds.
/// </summary>
/// <param name="Request">The Google-authenticated user.</param>
public record CreateGoogleLoginCodeCommand(
    UserDto Request) : IRequest<string>;

public class CreateGoogleLoginCodeCommandValidator
    : AbstractValidator<CreateGoogleLoginCodeCommand>
{
    public CreateGoogleLoginCodeCommandValidator()
    {
        RuleFor(command => command.Request.GoogleSubjectId)
            .NotEmpty()
            .WithMessage("Google subject identifier is required.");

        RuleFor(command => command.Request.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");
    }
}

public class CreateGoogleLoginCodeCommandHandler
    : IRequestHandler<CreateGoogleLoginCodeCommand, string>
{
    private readonly ILoginService _loginService;

    public CreateGoogleLoginCodeCommandHandler(
        ILoginService loginService)
    {
        _loginService = loginService;
    }

    public Task<string> Handle(
        CreateGoogleLoginCodeCommand request,
        CancellationToken cancellationToken)
    {
        return _loginService.CreateGoogleLoginCodeAsync(
            request.Request,
            cancellationToken);
    }
}
