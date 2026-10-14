using ErrorOr;
using FluentValidation;
using Mediator;

namespace BonusScraper.Application.Behaviours;

/// <summary>Runs every FluentValidation validator for a message and returns validation errors instead of calling the handler.</summary>
public sealed class ValidationBehaviour<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
    where TResponse : IErrorOr
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(message, cancellationToken);
            errors.AddRange(result.Errors.Select(f => Error.Validation(f.ErrorCode, f.ErrorMessage)));
        }

        return errors.Count > 0 ? ErrorOrResponse<TResponse>.From(errors) : await next(message, cancellationToken);
    }
}
