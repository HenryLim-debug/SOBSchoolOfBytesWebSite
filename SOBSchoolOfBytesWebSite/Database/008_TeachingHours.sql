SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.TeachingHourLogs',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeachingHourLogs (
        LogId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TeachingHourLogs PRIMARY KEY,
        ClassId INT NOT NULL CONSTRAINT FK_TeachingHourLogs_Class REFERENCES dbo.CourseClasses(ClassId),
        TeacherId INT NOT NULL CONSTRAINT FK_TeachingHourLogs_Teacher REFERENCES dbo.userTable(Id),
        SessionStartLocal DATETIME2(0) NOT NULL,
        Minutes SMALLINT NOT NULL CONSTRAINT DF_TeachingHourLogs_Minutes DEFAULT 120,
        LessonNotes NVARCHAR(2000) NOT NULL,
        Status NVARCHAR(10) NOT NULL CONSTRAINT DF_TeachingHourLogs_Status DEFAULT N'Pending',
        SubmittedAt DATETIME2(0) NOT NULL CONSTRAINT DF_TeachingHourLogs_Submitted DEFAULT SYSUTCDATETIME(),
        ReviewNotes NVARCHAR(1000) NOT NULL CONSTRAINT DF_TeachingHourLogs_ReviewNotes DEFAULT N'',
        ReviewedBy INT NULL CONSTRAINT FK_TeachingHourLogs_Reviewer REFERENCES dbo.userTable(Id),
        ReviewedAt DATETIME2(0) NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT UQ_TeachingHourLogs_Session UNIQUE(ClassId,SessionStartLocal),
        CONSTRAINT CK_TeachingHourLogs_Minutes CHECK(Minutes=120),
        CONSTRAINT CK_TeachingHourLogs_Notes CHECK(LEN(LTRIM(RTRIM(LessonNotes)))>0),
        CONSTRAINT CK_TeachingHourLogs_Review CHECK(
            (Status=N'Pending' AND ReviewedBy IS NULL AND ReviewedAt IS NULL AND ReviewNotes=N'') OR
            (Status=N'Approved' AND ReviewedBy IS NOT NULL AND ReviewedAt IS NOT NULL) OR
            (Status=N'Rejected' AND ReviewedBy IS NOT NULL AND ReviewedAt IS NOT NULL AND LEN(LTRIM(RTRIM(ReviewNotes)))>0))
    );
    CREATE INDEX IX_TeachingHourLogs_TeacherMonth ON dbo.TeachingHourLogs(TeacherId,SessionStartLocal,Status);
END;
IF OBJECT_ID(N'dbo.TeachingHourReviews',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeachingHourReviews (
        ReviewId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TeachingHourReviews PRIMARY KEY,
        LogId INT NOT NULL CONSTRAINT FK_TeachingHourReviews_Log REFERENCES dbo.TeachingHourLogs(LogId),
        PresidentId INT NOT NULL CONSTRAINT FK_TeachingHourReviews_President REFERENCES dbo.userTable(Id),
        Decision NVARCHAR(10) NOT NULL CONSTRAINT CK_TeachingHourReviews_Decision CHECK(Decision IN(N'Approved',N'Rejected')),
        LessonNotes NVARCHAR(2000) NOT NULL,
        ReviewNotes NVARCHAR(1000) NOT NULL,
        ReviewedAt DATETIME2(0) NOT NULL CONSTRAINT DF_TeachingHourReviews_Date DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_TeachingHourReviews_Log ON dbo.TeachingHourReviews(LogId,ReviewId);
END;
COMMIT TRANSACTION;
