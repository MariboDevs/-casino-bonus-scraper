using System.Net;

namespace BonusScraper.Infrastructure.UnitTests.Http;

/// <summary>An in-memory site: maps absolute URLs to responses and records what was requested.</summary>
internal sealed class StubHttp : HttpMessageHandler, IHttpClientFactory
{
    private readonly Dictionary<string, Func<HttpResponseMessage>> _routes = new(StringComparer.Ordinal);

    public List<Uri> Requests { get; } = [];

    public StubHttp Respond(string url, HttpStatusCode status, string body = "")
    {
        _routes[url] = () => new HttpResponseMessage(status) { Content = new StringContent(body) };
        return this;
    }

    public StubHttp Fail(string url)
    {
        _routes[url] = () => throw new HttpRequestException("Connection refused.");
        return this;
    }

    public HttpClient CreateClient(string name) => new(this, disposeHandler: false);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request.RequestUri!);
        var route = _routes.GetValueOrDefault(request.RequestUri!.AbsoluteUri);
        return Task.FromResult(route is null ? new HttpResponseMessage(HttpStatusCode.NotFound) : route());
    }
}
