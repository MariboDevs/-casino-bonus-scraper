using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using BonusScraper.Application.Abstractions;
using ErrorOr;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace BonusScraper.Infrastructure.Extraction;

/// <summary>Azure OpenAI chat completions, constrained to <see cref="BonusTermsSchema"/> with strict structured outputs.</summary>
internal sealed class AzureOpenAiBonusTermsExtractor : IBonusTermsExtractor
{
    private static readonly ChatCompletionOptions CompletionOptions = new()
    {
        ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            BonusTermsSchema.Name,
            BinaryData.FromString(BonusTermsSchema.Json),
            jsonSchemaIsStrict: true),
    };

    private readonly ChatClient _chat;

    public AzureOpenAiBonusTermsExtractor(IOptions<ExtractionOptions> options)
    {
        var azure = options.Value.AzureOpenAI;
        if (!azure.IsConfigured)
        {
            throw new InvalidOperationException(
                "Extraction:AzureOpenAI needs an Endpoint and a Deployment. Set them with user secrets or environment variables, or choose another Extraction:Provider.");
        }

        var client = azure.UsesKey
            ? new AzureOpenAIClient(azure.Endpoint!, new AzureKeyCredential(azure.ApiKey!))
            : new AzureOpenAIClient(azure.Endpoint!, new DefaultAzureCredential());
        _chat = client.GetChatClient(azure.Deployment);
    }

    public async Task<ErrorOr<ExtractedBonusTerms>> ExtractAsync(BonusTermsDocument document, CancellationToken cancellationToken)
    {
        ChatCompletion completion;
        try
        {
            completion = (await _chat.CompleteChatAsync(
                [new SystemChatMessage(BonusTermsPrompt.System), new UserChatMessage(BonusTermsPrompt.User(document))],
                CompletionOptions,
                cancellationToken)).Value;
        }
        catch (ClientResultException ex)
        {
            return ExtractorErrors.ModelUnavailable(ex.Status == 0 ? null : ex.Status);
        }

        if (!string.IsNullOrEmpty(completion.Refusal) || completion.FinishReason == ChatFinishReason.ContentFilter)
        {
            return ExtractorErrors.Refused;
        }

        if (completion.FinishReason == ChatFinishReason.Length)
        {
            return ExtractorErrors.Truncated;
        }

        var text = completion.Content.Count > 0 ? completion.Content[0].Text : null;
        return BonusTermsResponseParser.Parse(text, $"azure-openai:{completion.Model}");
    }
}
