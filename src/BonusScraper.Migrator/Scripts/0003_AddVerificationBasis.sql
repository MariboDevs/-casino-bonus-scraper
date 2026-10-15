-- 0003: record which text each offer was verified against. Rows written before this script
-- were checked against the whole page, which is closest to the article-based check.
ALTER TABLE scrape.BonusOffer
    ADD VerificationBasis VARCHAR(16) NOT NULL
        CONSTRAINT DF_BonusOffer_VerificationBasis DEFAULT 'ReviewArticle'
        CONSTRAINT CK_BonusOffer_VerificationBasis CHECK (VerificationBasis IN ('OperatorTerms', 'ReviewArticle'));
GO
