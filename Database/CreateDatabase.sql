-- =============================================
-- App Dashboard Database Setup Script
-- Run this in SQL Server Management Studio
-- =============================================

-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CoreAppwithSSODb')
BEGIN
    CREATE DATABASE CoreAppwithSSODb;
END
GO

USE CoreAppwithSSODb;
GO

-- Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(150) NOT NULL,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'User',
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
END
GO

-- Applications table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Applications')
BEGIN
    CREATE TABLE Applications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        BaseUrl NVARCHAR(500) NOT NULL,
        IconName NVARCHAR(100) NOT NULL DEFAULT 'apps',
        IconColor NVARCHAR(20) NOT NULL DEFAULT '#4285F4',
        IsActive BIT NOT NULL DEFAULT 1,
        DisplayOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE UNIQUE INDEX IX_Applications_Name ON Applications(Name);
END
GO

-- UserApplications (many-to-many)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserApplications')
BEGIN
    CREATE TABLE UserApplications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        ApplicationId INT NOT NULL,
        AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_UserApplications_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserApplications_Applications FOREIGN KEY (ApplicationId) REFERENCES Applications(Id) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX IX_UserApplications_UserId_ApplicationId ON UserApplications(UserId, ApplicationId);
END
GO

-- SSOTokens table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SSOTokens')
BEGIN
    CREATE TABLE SSOTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Token NVARCHAR(450) NOT NULL,
        UserId INT NOT NULL,
        ApplicationId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NOT NULL,
        IsUsed BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_SSOTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT FK_SSOTokens_Applications FOREIGN KEY (ApplicationId) REFERENCES Applications(Id)
    );

    CREATE UNIQUE INDEX IX_SSOTokens_Token ON SSOTokens(Token);
END
GO

-- Seed default applications
IF NOT EXISTS (SELECT * FROM Applications)
BEGIN
    INSERT INTO Applications (Name, Description, BaseUrl, IconName, IconColor, DisplayOrder) VALUES
    ('HR Portal', 'Human Resources Management', 'http://localhost:5301/hr', 'people', '#4285F4', 1),
    ('Finance', 'Financial Management System', 'http://localhost:5302/finance', 'account_balance', '#EA4335', 2),
    ('Project Mgmt', 'Project Management Tool', 'https://projects.yourcompany.com', 'assignment', '#FBBC05', 3),
    ('CRM', 'Customer Relationship Management', 'https://crm.yourcompany.com', 'contacts', '#34A853', 4),
    ('Inventory', 'Inventory Management System', 'http://localhost:5303/inventory', 'inventory_2', '#FF6D01', 5),
    ('Reports', 'Business Intelligence & Reports', 'https://reports.yourcompany.com', 'analytics', '#46BDC6', 6),
    ('Email', 'Corporate Email System', 'https://mail.yourcompany.com', 'email', '#7B1FA2', 7),
    ('Calendar', 'Team Calendar & Scheduling', 'https://calendar.yourcompany.com', 'calendar_month', '#0097A7', 8),
    ('Admin', 'System Administration', 'https://admin.yourcompany.com', 'admin_panel_settings', '#616161', 9);
END
GO

-- Seed admin user (password: Admin@123)
-- BCrypt hash for 'Admin@123'
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@dashboard.com')
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash, Role)
    VALUES ('System Admin', 'admin@dashboard.com', '$2a$11$K3jX3q4q5q6q7q8q9q0qOeR3s4t5u6v7w8x9y0z1a2b3c4d5e6f7g', 'Admin');

    -- Assign all apps to admin
    INSERT INTO UserApplications (UserId, ApplicationId)
    SELECT u.Id, a.Id FROM Users u CROSS JOIN Applications a WHERE u.Email = 'admin@dashboard.com';
END
GO

PRINT 'Database setup completed successfully!';
GO
