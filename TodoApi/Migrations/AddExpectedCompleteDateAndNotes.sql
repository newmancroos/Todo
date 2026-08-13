-- ============================================================
-- Migration: Add ExpectedCompleteDate and Notes columns
-- Run against an existing TodoDb that already has TodoItems
-- ============================================================

USE TodoDb;
GO

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TodoItems' AND COLUMN_NAME = 'ExpectedCompleteDate'
)
BEGIN
    ALTER TABLE [TodoItems]
    ADD [ExpectedCompleteDate] DATETIME2 NULL;
END
GO

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TodoItems' AND COLUMN_NAME = 'Notes'
)
BEGIN
    ALTER TABLE [TodoItems]
    ADD [Notes] NVARCHAR(2000) NULL;
END
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES ('20240102000000_AddExpectedCompleteDateAndNotes', '8.0.0');
GO
