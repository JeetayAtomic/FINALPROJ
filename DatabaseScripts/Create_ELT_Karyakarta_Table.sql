-- =============================================================================
-- Election Tracker — [ELT].[Karyakarta] table
-- Generated from the domain entity CoreAppwithSSO.ElectionTracker.Models.Domain.Karyakarta
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
--  * KaryakartaId is an IDENTITY column and is excluded from INSERT (see KaryakartaRepository).
--  * Gender is the int-backed CoreAppwithSSO.ElectionTracker.Common.Gender enum.
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
    WHERE s.name = 'ELT' AND t.name = 'Karyakarta')
BEGIN
    CREATE TABLE [ELT].[Karyakarta]
    (
        -- Identity / primary key (excluded from INSERT)
        [KaryakartaId]       INT             IDENTITY(1,1) NOT NULL,

        -- ---- Karyakarta business columns --------------------------------
        [KaryakartaCode]     NVARCHAR(50)    NOT NULL,
        [Name]               NVARCHAR(200)   NULL,
        [Initials]           NVARCHAR(20)    NULL,
        [Role]               NVARCHAR(100)   NULL,
        [ConstituencyId]     INT             NULL,
        [BoothId]            INT             NULL,
        [WardId]             INT             NULL,
        [DateOfBirth]        DATETIME2(7)    NULL,
        [Gender]             INT             NOT NULL CONSTRAINT [DF_ELT_Karyakarta_Gender] DEFAULT (0),
        [Phone]              NVARCHAR(20)    NULL,
        [Mobile]             NVARCHAR(20)    NULL,
        [JoinedDate]         DATETIME2(7)    NULL,
        [Rating]             NVARCHAR(50)    NULL,
        [Status]             NVARCHAR(50)    NULL,
        [KaryakartaPhotoUrl] NVARCHAR(500)   NULL,
        [HouseNo]            NVARCHAR(100)   NULL,
        [Address]            NVARCHAR(500)   NULL,
        [Address1]           NVARCHAR(500)   NULL,
        [Caste]              NVARCHAR(100)   NULL,

        -- ---- BaseModel flexfield columns --------------------------------
        [C_Attribute1]       NVARCHAR(500)   NULL,
        [C_Attribute2]       NVARCHAR(500)   NULL,
        [C_Attribute3]       NVARCHAR(500)   NULL,
        [C_Attribute4]       NVARCHAR(500)   NULL,
        [N_Attribute5]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Karyakarta_N_Attribute5] DEFAULT (0),
        [N_Attribute6]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Karyakarta_N_Attribute6] DEFAULT (0),
        [N_Attribute7]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Karyakarta_N_Attribute7] DEFAULT (0),
        [N_Attribute8]       DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Karyakarta_N_Attribute8] DEFAULT (0),
        [D_Attribute9]       DATETIME2(7)    NULL,
        [D_Attribute10]      DATETIME2(7)    NULL,
        [D_Attribute11]      DATETIME2(7)    NULL,
        [D_Attribute12]      DATETIME2(7)    NULL,
        [D_Attribute13]      DATETIME2(7)    NULL,
        [J_Attribute14]      NVARCHAR(MAX)   NULL,           -- JSON
        [J_Attribute15]      NVARCHAR(MAX)   NULL,           -- JSON
        [Record_Info]        NVARCHAR(MAX)   NULL,

        -- ---- Audit columns (IAuditAttributes) ---------------------------
        [CreatedBy]          INT             NOT NULL CONSTRAINT [DF_ELT_Karyakarta_CreatedBy]     DEFAULT (0),
        [LastUpdatedBy]      INT             NOT NULL CONSTRAINT [DF_ELT_Karyakarta_LastUpdatedBy] DEFAULT (0),

        CONSTRAINT [PK_ELT_Karyakarta] PRIMARY KEY CLUSTERED ([KaryakartaId] ASC)
    );

    -- Lookups used by Karyakarta queries / filters
    CREATE INDEX [IX_ELT_Karyakarta_BoothId]        ON [ELT].[Karyakarta] ([BoothId]);
    CREATE INDEX [IX_ELT_Karyakarta_WardId]         ON [ELT].[Karyakarta] ([WardId]);
    CREATE INDEX [IX_ELT_Karyakarta_ConstituencyId] ON [ELT].[Karyakarta] ([ConstituencyId]);
    CREATE UNIQUE INDEX [UX_ELT_Karyakarta_KaryakartaCode] ON [ELT].[Karyakarta] ([KaryakartaCode]);
END
GO
