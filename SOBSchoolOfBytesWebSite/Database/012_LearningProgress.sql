SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.ChapterCompletions',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChapterCompletions(
        EnrolmentId INT NOT NULL,
        ChapterId INT NOT NULL,
        CourseId INT NOT NULL,
        CompletedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ChapterCompletions_Date DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ChapterCompletions PRIMARY KEY(EnrolmentId,ChapterId),
        CONSTRAINT FK_ChapterCompletions_Enrolment FOREIGN KEY(EnrolmentId,CourseId) REFERENCES dbo.CourseEnrolments(EnrolmentId,CourseId),
        CONSTRAINT FK_ChapterCompletions_Chapter FOREIGN KEY(ChapterId,CourseId) REFERENCES dbo.CourseChapters(ChapterId,CourseId)
    );
END;
COMMIT TRANSACTION;
