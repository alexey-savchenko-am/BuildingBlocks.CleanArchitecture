using FluentValidation;
using MediatR;

namespace BuildingBlocks.CleanArchitecture.Application.CQRS.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
           .Where(validationResult => !validationResult.IsValid)
           .SelectMany(validationResult => validationResult.Errors)
           .Select(validationFailure => new ValidationError(
               validationFailure.PropertyName,
               validationFailure.ErrorMessage))
           .ToList();

        if (errors.Any())
        {
            throw new ValidationException(errors);
        }

        var response = await next();

        return response;
    }
}
