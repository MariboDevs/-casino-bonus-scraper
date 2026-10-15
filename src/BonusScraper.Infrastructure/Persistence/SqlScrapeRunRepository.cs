using BonusScraper.Application.Abstractions;
using BonusScraper.Domain.Runs;
using Dapper;
using ErrorOr;
using Microsoft.Data.SqlClient;

namespace BonusScraper.Infrastructure.Persistence;

/// <summary>
/// Writes a run, its offers and its failures in one transaction. Casinos are upserted by
/// name so the history of one casino's offer can be read across runs.
/// </summary>
internal sealed class SqlScrapeRunRepository(ISqlConnectionFactory connections) : IScrapeRunRepository
{
    private const string InsertRun = """
        INSERT INTO scrape.ScrapeRun (ScrapeRunId, StartedAt, CompletedAt, Outcome)
        VALUES (@Id, @StartedAt, @CompletedAt, @Outcome);
        """;

    private const string UpsertCasino = """
        MERGE scrape.Casino WITH (HOLDLOCK) AS target
        USING (SELECT @Name AS Name) AS source ON target.Name = source.Name
        WHEN NOT MATCHED THEN INSERT (Name) VALUES (source.Name);
        SELECT CasinoId FROM scrape.Casino WHERE Name = @Name;
        """;

    private const string InsertOffer = """
        INSERT INTO scrape.BonusOffer (
            ScrapeRunId, CasinoId, WelcomeBonusSummary, MatchPercent, MaxBonusAmount, FreeSpins,
            WageringMultiplier, MinimumDepositAmount, CurrencyCode, SourceUrl, RawTerms,
            ExtractedBy, ExtractedAt, VerificationStatus, VerificationBasis, VerificationNotes)
        VALUES (
            @RunId, @CasinoId, @Summary, @MatchPercent, @MaxBonusAmount, @FreeSpins,
            @Wagering, @MinimumDeposit, @Currency, @SourceUrl, @RawTerms,
            @ExtractedBy, @ExtractedAt, @Status, @Basis, @Notes);
        """;

    private const string InsertFailure = """
        INSERT INTO scrape.ScrapeFailure (ScrapeRunId, Casino, SourceUrl, ErrorCode, Description)
        VALUES (@RunId, @Casino, @SourceUrl, @Code, @Description);
        """;

    public async Task<ErrorOr<Success>> SaveAsync(ScrapeRun run, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await connections.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            await connection.ExecuteAsync(new CommandDefinition(
                InsertRun,
                new { run.Id, run.StartedAt, run.CompletedAt, Outcome = run.Outcome.ToString() },
                transaction,
                cancellationToken: cancellationToken));

            foreach (var offer in run.Offers)
            {
                var casinoId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                    UpsertCasino, new { Name = offer.Casino.Value }, transaction, cancellationToken: cancellationToken));

                await connection.ExecuteAsync(new CommandDefinition(
                    InsertOffer,
                    new
                    {
                        RunId = run.Id,
                        CasinoId = casinoId,
                        Summary = offer.WelcomeBonus.Summary,
                        offer.WelcomeBonus.MatchPercent,
                        MaxBonusAmount = offer.WelcomeBonus.MaxBonus?.Amount,
                        offer.WelcomeBonus.FreeSpins,
                        Wagering = offer.Wagering?.Multiplier,
                        MinimumDeposit = offer.MinimumDeposit?.Amount,
                        Currency = (offer.MinimumDeposit ?? offer.WelcomeBonus.MaxBonus)?.Currency.Code,
                        SourceUrl = offer.Source.ToString(),
                        RawTerms = offer.Text.Full,
                        offer.ExtractedBy,
                        offer.ExtractedAt,
                        Status = offer.Verification.Status.ToString(),
                        Basis = offer.Verification.Basis.ToString(),
                        Notes = string.Join('\n', offer.Verification.Notes),
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            foreach (var failure in run.Failures)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    InsertFailure,
                    new { RunId = run.Id, failure.Casino, SourceUrl = failure.Source.AbsoluteUri, failure.Code, failure.Description },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            await transaction.CommitAsync(cancellationToken);
            return Result.Success;
        }
        catch (SqlException ex)
        {
            return Error.Unexpected("Database.WriteFailed", $"The run could not be saved (SQL error {ex.Number}).");
        }
    }
}
