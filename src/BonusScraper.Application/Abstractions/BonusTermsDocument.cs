namespace BonusScraper.Application.Abstractions;

/// <summary>What the language model is given: whose offer it is, and the text describing it.</summary>
public sealed record BonusTermsDocument(string Casino, string Text);
