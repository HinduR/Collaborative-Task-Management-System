using FluentValidation;
using MediatR;
using Shared.Exceptions.Infrastructure;

namespace Shared.Common.Validator;

/// <summary>
/// The `ValidationBehavior` class implements a pipeline behavior for validating requests using a collection of validators.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    /// <summary>
    /// The  constructor in the `ValidationBehavior` class is initializing the `_validators` field with the collection of
    /// validators passed as a parameter to the constructor.
    /// </summary>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// The function asynchronously validates a request using multiple validators and throws a custom
    /// exception if validation fails, otherwise it proceeds to the next handler.
    /// </summary>
    /// <param name="TRequest">TRequest is a generic type representing the request object that is being
    /// handled by this method.</param>
    /// <param name="next">The `next` parameter in the `Handle` method is a delegate that represents the
    /// next handler in the pipeline.</param>
    /// <param name="CancellationToken">The CancellationToken parameter in the Handle method is used to
    /// propagate notification that operations should be canceled.</param>
    /// <returns>
    /// The method `Handle` returns a `Task<TResponse>`.
    /// </returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ValidationContext<TRequest> context = new ValidationContext<TRequest>(request);

        Task<FluentValidation.Results.ValidationResult>[] validationTasks = _validators
            .Select(v => v.ValidateAsync(context, cancellationToken))
            .ToArray();

        FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(validationTasks);

        List<FluentValidation.Results.ValidationFailure> failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessages = string.Join("; ", failures.Select(f => f.ErrorMessage));
            throw new BadRequestCustomException(errorMessages, errorMessages);
        }
        return await next();
    }
}
