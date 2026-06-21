-- =============================================================================
-- Election Tracker — [ELT].[Ward] table
-- Generated from the domain entity CoreAppwithSSO.ElectionTracker.Models.Domain.Ward
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
--  * WardId is an IDENTITY column and is excluded from INSERT (see WardRepository).
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
    WHERE s.name = 'ELT' AND t.name = 'Ward')
BEGIN
    CREATE TABLE [ELT].[Ward]
    (
        -- Identity / primary key (excluded from INSERT)
        [WardId]             INT             IDENTITY(1,1) NOT NULL,

        -- ---- Ward business columns --------------------------------------
        [WardCode]           NVARCHAR(50)    NOT NULL,
        [Name]               NVARCHAR(150)   NOT NULL,
        [ShortName]          NVARCHAR(100)   NULL,
        [Neighborhood]       NVARCHAR(200)   NULL,
        [ConstituencyId]     INT             NULL,
        [Households]         INT             NULL,
        [LeadId]             NVARCHAR(100)   NULL,
        [Coverage]           INT             NULL,
        [SupportLevel]       INT             NULL,
        [LastCanvass]        INT             NULL,
        [Flags]              NVARCHAR(500)   NULL,
        [Status]             INT             NULL,

        -- ---- BaseModel flexfield columns --------------------------------
        [C_Attribute1]       NVARCHAR(500)   NULL,
        [C_Attribute2]       NVARCHAR(500)   NULL,
        [C_Attribute3]       NVARCHAR(500)   NULL,
        [C_Attribute4]       NVARCHAR(500)   NULL,
        [N_Attribute5]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Ward_N_Attribute5] DEFAULT (0),
        [N_Attribute6]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Ward_N_Attribute6] DEFAULT (0),
        [N_Attribute7]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Ward_N_Attribute7] DEFAULT (0),
        [N_Attribute8]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Ward_N_Attribute8] DEFAULT (0),
        [D_Attribute9]       DATETIME2(7)    NULL,
        [D_Attribute10]      DATETIME2(7)    NULL,
        [D_Attribute11]      DATETIME2(7)    NULL,
        [D_Attribute12]      DATETIME2(7)    NULL,
        [D_Attribute13]      DATETIME2(7)    NULL,
        [J_Attribute14]      NVARCHAR(MAX)   NULL,           -- JSON
        [J_Attribute15]      NVARCHAR(MAX)   NULL,           -- JSON
        [Record_Info]        NVARCHAR(MAX)   NULL,

        -- ---- Audit columns (IAuditAttributes) ---------------------------
        [CreatedBy]          INT             NOT NULL CONSTRAINT [DF_ELT_Ward_CreatedBy]     DEFAULT (0),
        [LastUpdatedBy]      INT             NOT NULL CONSTRAINT [DF_ELT_Ward_LastUpdatedBy] DEFAULT (0),

        CONSTRAINT [PK_ELT_Ward] PRIMARY KEY CLUSTERED ([WardId] ASC)
    );

    -- Lookups used by Ward queries / filters
    CREATE INDEX [IX_ELT_Ward_ConstituencyId] ON [ELT].[Ward] ([ConstituencyId]);
    CREATE UNIQUE INDEX [UX_ELT_Ward_WardCode] ON [ELT].[Ward] ([WardCode]);
END
GO
