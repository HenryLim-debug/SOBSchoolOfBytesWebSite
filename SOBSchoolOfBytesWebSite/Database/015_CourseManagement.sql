SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.Courses','SampleLesson') IS NULL
    ALTER TABLE dbo.Courses ADD SampleLesson NVARCHAR(4000) NOT NULL CONSTRAINT DF_Courses_SampleLesson DEFAULT N'', SampleActivity NVARCHAR(2000) NOT NULL CONSTRAINT DF_Courses_SampleActivity DEFAULT N'';
IF COL_LENGTH('dbo.Courses','RowVersion') IS NULL
    ALTER TABLE dbo.Courses ADD RowVersion ROWVERSION;
IF COL_LENGTH('dbo.CourseClasses','RowVersion') IS NULL
    ALTER TABLE dbo.CourseClasses ADD RowVersion ROWVERSION;
IF OBJECT_ID('dbo.ClassChangeHistory','U') IS NULL
    CREATE TABLE dbo.ClassChangeHistory(
        ChangeId INT IDENTITY PRIMARY KEY,
        ClassId INT NOT NULL REFERENCES dbo.CourseClasses(ClassId),
        ChangedBy INT NOT NULL REFERENCES dbo.userTable(Id),
        ChangedAtUtc DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        PreviousDetails NVARCHAR(1500) NOT NULL,
        NewDetails NVARCHAR(1500) NOT NULL,
        Reason NVARCHAR(500) NOT NULL
    );
COMMIT;
