-- =============================================================================
-- Election Tracker — [ELT].[Sector] table
-- Generated from the domain entity CoreAppwithSSO.ElectionTracker.Models.Domain.Sector
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
--  * SectorId is an IDENTITY column and is excluded from INSERT (see SectorRepository).
--  * J_Attribute14/15 columns hold JSON text (stored as NVARCHAR(MAX)).
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
    WHERE s.name = 'ELT' AND t.name = 'Sector')
BEGIN
    CREATE TABLE [ELT].[Sector]
    (
        -- Identity / primary key (excluded from INSERT)
        [SectorId]           INT             IDENTITY(1,1) NOT NULL,

        -- ---- Sector business columns ------------------------------------
        [BoothId]            INT             NOT NULL,
        [Code]               NVARCHAR(50)    NOT NULL,
        [Name]               NVARCHAR(150)   NOT NULL,
        [Description]        NVARCHAR(500)   NULL,
        [Latitude]           DECIMAL(10, 7)  NULL,
        [Longitude]          DECIMAL(10, 7)  NULL,

        -- ---- BaseModel flexfield columns --------------------------------
        [C_Attribute1]       NVARCHAR(500)   NULL,
        [C_Attribute2]       NVARCHAR(500)   NULL,
        [C_Attribute3]       NVARCHAR(500)   NULL,
        [C_Attribute4]       NVARCHAR(500)   NULL,
        [N_Attribute5]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Sector_N_Attribute5] DEFAULT (0),
        [N_Attribute6]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Sector_N_Attribute6] DEFAULT (0),
        [N_Attribute7]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Sector_N_Attribute7] DEFAULT (0),
        [N_Attribute8]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Sector_N_Attribute8] DEFAULT (0),
        [D_Attribute9]       DATETIME2(7)    NULL,
        [D_Attribute10]      DATETIME2(7)    NULL,
        [D_Attribute11]      DATETIME2(7)    NULL,
        [D_Attribute12]      DATETIME2(7)    NULL,
        [D_Attribute13]      DATETIME2(7)    NULL,
        [J_Attribute14]      NVARCHAR(MAX)   NULL,           -- JSON
        [J_Attribute15]      NVARCHAR(MAX)   NULL,           -- JSON
        [Record_Info]        NVARCHAR(MAX)   NULL,

        -- ---- Audit columns (IAuditAttributes) ---------------------------
        [CreatedBy]          INT             NOT NULL CONSTRAINT [DF_ELT_Sector_CreatedBy]     DEFAULT (0),
        [LastUpdatedBy]      INT             NOT NULL CONSTRAINT [DF_ELT_Sector_LastUpdatedBy] DEFAULT (0),

        CONSTRAINT [PK_ELT_Sector] PRIMARY KEY CLUSTERED ([SectorId] ASC)
    );

    -- Lookups used by Sector queries / filters
    CREATE INDEX [IX_ELT_Sector_BoothId] ON [ELT].[Sector] ([BoothId]);
    CREATE UNIQUE INDEX [UX_ELT_Sector_Code] ON [ELT].[Sector] ([Code]);
END
GO
