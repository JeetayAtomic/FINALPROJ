-- =============================================================================
-- Election Tracker — [ELT].[Booth] table
-- Generated from the domain entity CoreAppwithSSO.ElectionTracker.Models.Domain.Booth
-- (which inherits CoreAppwithSSO.ElectionTracker.Models.Domain.BaseModel).
--
-- Target: each TENANT database (the per-client DB resolved at runtime). Run this
--         script against every tenant DB that needs the Election Tracker module.
-- Engine: SQL Server.
--
-- Notes
--  * Column names match the entity property names 1:1 — Dapper maps by name and the
--    dynamic INSERT/UPDATE (CommonUtl.BuildInsertQuery/BuildUpdateQuery) emits one
--    column per writable property, so the names must stay in sync.
--  * BoothId is an IDENTITY column and is excluded from INSERT (see BoothRepository).
--  * J_/Agent/Backup/Materials columns hold JSON text (stored as NVARCHAR(MAX)).
--  * D_Attribute9..13 are written as parsed DateTime values (CommonUtl.ParseFlexDate).
-- =============================================================================

-- Schema --------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ELT')
BEGIN
    EXEC('CREATE SCHEMA [ELT];');
END
GO

-- Table ---------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = 'ELT' AND t.name = 'Booth')
BEGIN
    CREATE TABLE [ELT].[Booth]
    (
        -- Identity / primary key (excluded from INSERT)
        [BoothId]            INT             IDENTITY(1,1) NOT NULL,

        -- ---- Booth business columns -------------------------------------
        [BoothCode]          NVARCHAR(50)    NOT NULL,          -- business identity (was "ShortId")
        [WardId]             INT             NOT NULL,
        [WardName]           NVARCHAR(200)   NULL,
        [WardNeighborhood]   NVARCHAR(200)   NULL,
        [ConstituencyId]     NVARCHAR(50)    NULL,
        [ConstituencyName]   NVARCHAR(200)   NULL,
        [Venue]              NVARCHAR(300)   NULL,
        [Address]            NVARCHAR(500)   NULL,
        [VoterCount]         INT             NOT NULL CONSTRAINT [DF_ELT_Booth_VoterCount]    DEFAULT (0),

        -- Nested objects persisted as JSON (BoothPerson / BoothMaterials shapes)
        [Agent]              NVARCHAR(MAX)   NULL,
        [Backup]             NVARCHAR(MAX)   NULL,
        [Materials]          NVARCHAR(MAX)   NULL,

        [Accessibility]      BIT             NOT NULL CONSTRAINT [DF_ELT_Booth_Accessibility] DEFAULT (0),
        [Status]             NVARCHAR(50)    NULL,
        [StatusColor]        NVARCHAR(50)    NULL,

        -- ---- BaseModel flexfield columns --------------------------------
        [C_Attribute1]       NVARCHAR(500)   NULL,
        [C_Attribute2]       NVARCHAR(500)   NULL,
        [C_Attribute3]       NVARCHAR(500)   NULL,
        [C_Attribute4]       NVARCHAR(500)   NULL,
        [N_Attribute5]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Booth_N_Attribute5] DEFAULT (0),
        [N_Attribute6]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Booth_N_Attribute6] DEFAULT (0),
        [N_Attribute7]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Booth_N_Attribute7] DEFAULT (0),
        [N_Attribute8]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Booth_N_Attribute8] DEFAULT (0),
        [D_Attribute9]       DATETIME2(7)    NULL,
        [D_Attribute10]      DATETIME2(7)    NULL,
        [D_Attribute11]      DATETIME2(7)    NULL,
        [D_Attribute12]      DATETIME2(7)    NULL,
        [D_Attribute13]      DATETIME2(7)    NULL,
        [J_Attribute14]      NVARCHAR(MAX)   NULL,           -- JSON
        [J_Attribute15]      NVARCHAR(MAX)   NULL,           -- JSON
        [Record_Info]        NVARCHAR(MAX)   NULL,

        -- ---- Audit columns (IAuditAttributes) ---------------------------
        [CreatedBy]          INT             NOT NULL CONSTRAINT [DF_ELT_Booth_CreatedBy]     DEFAULT (0),
        [LastUpdatedBy]      INT             NOT NULL CONSTRAINT [DF_ELT_Booth_LastUpdatedBy] DEFAULT (0),

        CONSTRAINT [PK_ELT_Booth] PRIMARY KEY CLUSTERED ([BoothId] ASC)
    );

    -- Lookups used by Booth queries / filters
    CREATE INDEX [IX_ELT_Booth_WardId]    ON [ELT].[Booth] ([WardId]);
    CREATE UNIQUE INDEX [UX_ELT_Booth_BoothCode] ON [ELT].[Booth] ([BoothCode]);
END
GO
