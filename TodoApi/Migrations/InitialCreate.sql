-- ============================================================
-- Initial migration SQL (generated from EF Core Code-First)
-- Run this manually if you prefer not to use dotnet ef commands
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TodoDb')
BEGIN
    CREATE DATABASE TodoDb;
END
GO

USE TodoDb;
GO

-- EF Core migrations history table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_NAME = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId]    NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32)  NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

-- TodoItems table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_NAME = 'TodoItems')
BEGIN
    CREATE TABLE [TodoItems] (
        [Id]          INT            NOT NULL IDENTITY(1,1),
        [Title]       NVARCHAR(200)  NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [Status]      NVARCHAR(20)   NOT NULL DEFAULT 'Incomplete',
        [CreatedAt]   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt]   DATETIME2      NULL,
        CONSTRAINT [PK_TodoItems] PRIMARY KEY ([Id])
    );
END
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES ('20240101000000_InitialCreate', '8.0.0');
GO
