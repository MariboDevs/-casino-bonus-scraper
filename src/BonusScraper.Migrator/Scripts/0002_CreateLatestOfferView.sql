-- 0002: each casino's most recent offer, for dashboards and ad-hoc queries.
CREATE VIEW scrape.LatestBonusOffer
AS
SELECT c.Name AS Casino,
       o.WelcomeBonusSummary,
       o.WageringMultiplier,
       o.MinimumDepositAmount,
       o.CurrencyCode,
       o.SourceUrl,
       o.VerificationStatus,
       o.ExtractedAt
FROM scrape.Casino AS c
CROSS APPLY (
    SELECT TOP (1) *
    FROM scrape.BonusOffer AS b
    WHERE b.CasinoId = c.CasinoId
    ORDER BY b.ExtractedAt DESC
) AS o;
GO
