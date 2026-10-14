using System.Text.Json;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Beta.Messages;
using BonusScraper.Application.Abstractions;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure.Extraction;

/// <summary>
/// The Claude API with structured outputs, constrained to <see cref="BonusTermsSchema"/>.
/// Server-side refusal fallbacks are on, so a safety decline is re-served by another
/// model inside the same call instead of failing the casino.
/// </summary>
internal sealed class ClaudeBonusTermsExtractor : IBonusTermsExtractor
{
    /// <summary>The answer is a few hundred tokens of JSON; this is headroom, not a target.</summary>
    private const int MaxTokens = 4096;

    private static readonly Dictionary<string, JsonElement> Schema =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(BonusTermsSchema.Json)!;

    private readonly AnthropicClient _client;
    private readonly string _model;

    public ClaudeBonusTermsExtractor(IOptions<ExtractionOptions> options)
    {
        var anthropic = options.Value.Anthropic;
        if (!anthropic.IsConfigured)
        {
            throw new InvalidOperationException(
                "Extraction:Anthropic needs an ApiKey. Set it with user secrets or environment variables, or choose another Extraction:Provider.");
        }

        _client = new AnthropicClient { ApiKey = anthropic.ApiKey };
        _model = anthropic.Model;
    }

    public async Task<ErrorOr<ExtractedBonusTerms>> ExtractAsync(BonusTermsDocument document, CancellationToken cancellationToken)
    {
        BetaMessage message;
        try
        {
            message = await _client.Beta.Messages.Create(
                new MessageCreateParams
                {
                    Model = _model,
                    MaxTokens = MaxTokens,
                    System = BonusTermsPrompt.System,
                    Messages = [new() { Role = Role.User, Content = BonusTermsPrompt.User(document) }],
                    OutputConfig = new BetaOutputConfig
                    {
                        Effort = Effort.Low,
                        Format = new BetaJsonOutputFormat { Schema = Schema },
                    },
                    Fallbacks = new Default(),
                    Betas = ["server-side-fallback-2026-07-01"],
                },
                cancellationToken);
        }
        catch (AnthropicRateLimitException)
        {
            return ExtractorErrors.ModelUnavailable(429);
        }
        catch (Anthropic5xxException)
        {
            return ExtractorErrors.ModelUnavailable(500);
        }
        catch (AnthropicIOException)
        {
            return ExtractorErrors.ModelUnavailable(null);
        }
        catch (AnthropicApiException)
        {
            return ExtractorErrors.ModelUnavailable(400);
        }

        if (message.StopReason == "refusal")
        {
            return ExtractorErrors.Refused;
        }

        if (message.StopReason == "max_tokens")
        {
            return ExtractorErrors.Truncated;
        }

        var text = message.Content
            .Select(block => block.TryPickText(out var t) ? t.Text : null)
            .FirstOrDefault(t => t is not null);
        return BonusTermsResponseParser.Parse(text, $"anthropic:{message.Model}");
    }
}
