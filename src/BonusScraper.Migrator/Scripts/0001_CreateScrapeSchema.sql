-- 0001: the scrape schema. Scripts are immutable once merged; change the schema with a new script.
IF SCHEMA_ID(N'scrape') IS NULL
    EXEC (N'CREATE SCHEMA scrape AUTHORIZATION dbo;');
GO

CREATE TABLE scrape.ScrapeRun
(
    ScrapeRunId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ScrapeRun PRIMARY KEY NONCLUSTERED,
    StartedAt   DATETIMEOFFSET(3) NOT NULL,
    CompletedAt DATETIMEOFFSET(3) NULL,
    Outcome     VARCHAR(32) NOT NULL
        CONSTRAINT CK_ScrapeRun_Outcome CHECK (Outcome IN ('Succeeded', 'PartiallySucceeded', 'Failed'))
);

CREATE CLUSTERED INDEX CIX_ScrapeRun_StartedAt ON scrape.ScrapeRun (StartedAt);
GO

CREATE TABLE scrape.Casino
(
    CasinoId  INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Casino PRIMARY KEY,
    Name      NVARCHAR(100) NOT NULL CONSTRAINT UQ_Casino_Name UNIQUE,
    CreatedAt DATETIMEOFFSET(3) NOT NULL CONSTRAINT DF_Casino_CreatedAt DEFAULT SYSDATETIMEOFFSET()
);
GO

CREATE TABLE scrape.BonusOffer
(
    BonusOfferId         BIGINT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_BonusOffer PRIMARY KEY,
    ScrapeRunId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_BonusOffer_ScrapeRun REFERENCES scrape.ScrapeRun (ScrapeRunId),
    CasinoId             INT NOT NULL CONSTRAINT FK_BonusOffer_Casino REFERENCES scrape.Casino (CasinoId),
    WelcomeBonusSummary  NVARCHAR(200) NOT NULL,
    MatchPercent         INT NULL,
    MaxBonusAmount       DECIMAL(12, 2) NULL,
    FreeSpins            INT NULL,
    WageringMultiplier   DECIMAL(6, 2) NULL,
    MinimumDepositAmount DECIMAL(12, 2) NULL,
    CurrencyCode         CHAR(3) NULL,
    SourceUrl            NVARCHAR(2048) NOT NULL,
    RawTerms             NVARCHAR(MAX) NOT NULL,
    ExtractedBy          NVARCHAR(200) NOT NULL,
    ExtractedAt          DATETIMEOFFSET(3) NOT NULL,
    VerificationStatus   VARCHAR(16) NOT NULL
        CONSTRAINT CK_BonusOffer_VerificationStatus CHECK (VerificationStatus IN ('Confirmed', 'Provisional')),
    VerificationNotes    NVARCHAR(MAX) NOT NULL CONSTRAINT DF_BonusOffer_VerificationNotes DEFAULT N'',
    CONSTRAINT UQ_BonusOffer_Run_Casino UNIQUE (ScrapeRunId, CasinoId)
);

-- "What did this casino offer over time" is the history query the table exists for.
CREATE INDEX IX_BonusOffer_Casino_ExtractedAt ON scrape.BonusOffer (CasinoId, ExtractedAt DESC)
    INCLUDE (WelcomeBonusSummary, WageringMultiplier, MinimumDepositAmount, CurrencyCode, VerificationStatus);
GO

CREATE TABLE scrape.ScrapeFailure
(
    ScrapeFailureId BIGINT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_ScrapeFailure PRIMARY KEY,
    ScrapeRunId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ScrapeFailure_ScrapeRun REFERENCES scrape.ScrapeRun (ScrapeRunId),
    Casino          NVARCHAR(100) NOT NULL,
    SourceUrl       NVARCHAR(2048) NOT NULL,
    ErrorCode       VARCHAR(100) NOT NULL,
    Description     NVARCHAR(1000) NOT NULL
);

CREATE INDEX IX_ScrapeFailure_ScrapeRunId ON scrape.ScrapeFailure (ScrapeRunId);
GO
