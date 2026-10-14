using System.Reflection;
using ErrorOr;

namespace BonusScraper.Application.Behaviours;

/// <summary>
/// Builds a failed <c>ErrorOr&lt;T&gt;</c> for a response type only known generically.
/// The factory is resolved once per closed type and cached, so pipeline behaviours can
/// short-circuit without <c>dynamic</c>.
/// </summary>
internal static class ErrorOrResponse<TResponse>
    where TResponse : IErrorOr
{
    private static readonly Func<List<Error>, TResponse> Factory = Resolve();

    public static TResponse From(List<Error> errors) => Factory(errors);

    private static Func<List<Error>, TResponse> Resolve()
    {
        var method = typeof(TResponse).GetMethod(
                         "From",
                         BindingFlags.Public | BindingFlags.Static,
                         [typeof(List<Error>)])
                     ?? throw new InvalidOperationException($"{typeof(TResponse)} has no From(List<Error>) factory.");

        return method.CreateDelegate<Func<List<Error>, TResponse>>();
    }
}
