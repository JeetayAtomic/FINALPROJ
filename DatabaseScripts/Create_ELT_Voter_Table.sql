-- =============================================================================
-- Election Tracker — [ELT].[Voter] table
-- Generated from the domain entity CoreAppwithSSO.ElectionTracker.Models.Domain.Voter
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
--  * VoterId is a BIGINT IDENTITY column and is excluded from INSERT (see VoterRepository).
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
    WHERE s.name = 'ELT' AND t.name = 'Voter')
BEGIN
    CREATE TABLE [ELT].[Voter]
    (
        -- Identity / primary key (excluded from INSERT)
        [VoterId]               BIGINT          IDENTITY(1,1) NOT NULL,

        -- ---- Voter business columns -------------------------------------
        [EpicNo]                NVARCHAR(50)    NOT NULL,
        [BoothId]               INT             NOT NULL,
        [ConstituencyId]        INT             NULL,
        [SerialNo]              NVARCHAR(50)    NULL,
        [FullName]              NVARCHAR(200)   NULL,
        [FirstName]             NVARCHAR(100)   NULL,
        [LastName]              NVARCHAR(100)   NULL,
        [FatherName]            NVARCHAR(200)   NULL,
        [MotherName]            NVARCHAR(200)   NULL,
        [RelationName]          NVARCHAR(200)   NULL,
        [RelationType]          NVARCHAR(50)    NULL,
        [DateOfBirth]           DATETIME2(7)    NULL,
        [Gender]                INT             NOT NULL CONSTRAINT [DF_ELT_Voter_Gender] DEFAULT (0),
        [HouseNo]               NVARCHAR(100)   NULL,
        [Address]               NVARCHAR(500)   NULL,
        [Address1]              NVARCHAR(500)   NULL,
        [MobileNo]              NVARCHAR(20)    NULL,
        [Caste]                 NVARCHAR(100)   NULL,
        [SubCaste]              NVARCHAR(100)   NULL,
        [Religion]              NVARCHAR(100)   NULL,
        [VoterPhotoUrl]         NVARCHAR(500)   NULL,
        [VoterIdCardUrl]        NVARCHAR(500)   NULL,
        [IsOutsideConstituency] BIT             NOT NULL CONSTRAINT [DF_ELT_Voter_IsOutsideConstituency] DEFAULT (0),

        -- ---- BaseModel flexfield columns --------------------------------
        [C_Attribute1]          NVARCHAR(500)   NULL,
        [C_Attribute2]          NVARCHAR(500)   NULL,
        [C_Attribute3]          NVARCHAR(500)   NULL,
        [C_Attribute4]          NVARCHAR(500)   NULL,
        [N_Attribute5]          DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Voter_N_Attribute5] DEFAULT (0),
        [N_Attribute6]          DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Voter_N_Attribute6] DEFAULT (0),
        [N_Attribute7]          DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Voter_N_Attribute7] DEFAULT (0),
        [N_Attribute8]          DECIMAL(18, 2)  NOT NULL CONSTRAINT [DF_ELT_Voter_N_Attribute8] DEFAULT (0),
        [D_Attribute9]          DATETIME2(7)    NULL,
        [D_Attribute10]         DATETIME2(7)    NULL,
        [D_Attribute11]         DATETIME2(7)    NULL,
        [D_Attribute12]         DATETIME2(7)    NULL,
        [D_Attribute13]         DATETIME2(7)    NULL,
        [J_Attribute14]         NVARCHAR(MAX)   NULL,           -- JSON
        [J_Attribute15]         NVARCHAR(MAX)   NULL,           -- JSON
        [Record_Info]           NVARCHAR(MAX)   NULL,

        -- ---- Audit columns (IAuditAttributes) ---------------------------
        [CreatedBy]             INT             NOT NULL CONSTRAINT [DF_ELT_Voter_CreatedBy]     DEFAULT (0),
        [LastUpdatedBy]         INT             NOT NULL CONSTRAINT [DF_ELT_Voter_LastUpdatedBy] DEFAULT (0),

        CONSTRAINT [PK_ELT_Voter] PRIMARY KEY CLUSTERED ([VoterId] ASC)
    );

    -- Lookups used by Voter queries / filters
    CREATE INDEX [IX_ELT_Voter_BoothId] ON [ELT].[Voter] ([BoothId]);
    CREATE UNIQUE INDEX [UX_ELT_Voter_EpicNo] ON [ELT].[Voter] ([EpicNo]);
END
GO
