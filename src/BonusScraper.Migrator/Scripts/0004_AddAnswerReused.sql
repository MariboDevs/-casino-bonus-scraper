-- 0004: record whether the model's answer was reused from an earlier run for the identical
-- page text (change detection). Rows written before this script were all answered on their run.
ALTER TABLE scrape.BonusOffer
    ADD AnswerReused BIT NOT NULL
        CONSTRAINT DF_BonusOffer_AnswerReused DEFAULT 0;
GO
