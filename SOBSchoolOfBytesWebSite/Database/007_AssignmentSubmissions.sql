SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF NOT EXISTS(SELECT 1 FROM sys.key_constraints WHERE name=N'UQ_CourseChapters_CourseKey')
    ALTER TABLE dbo.CourseChapters ADD CONSTRAINT UQ_CourseChapters_CourseKey UNIQUE(ChapterId,CourseId);
IF NOT EXISTS(SELECT 1 FROM sys.key_constraints WHERE name=N'UQ_CourseEnrolments_CourseKey')
    ALTER TABLE dbo.CourseEnrolments ADD CONSTRAINT UQ_CourseEnrolments_CourseKey UNIQUE(EnrolmentId,CourseId);
IF OBJECT_ID(N'dbo.AssignmentSubmissions',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AssignmentSubmissions (
        SubmissionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AssignmentSubmissions PRIMARY KEY,
        ChapterId INT NOT NULL,
        CourseId INT NOT NULL,
        EnrolmentId INT NOT NULL,
        RequestId UNIQUEIDENTIFIER NOT NULL CONSTRAINT UQ_AssignmentSubmissions_Request UNIQUE,
        FileName NVARCHAR(180) NOT NULL,
        FileData VARBINARY(MAX) NOT NULL,
        SubmittedAt DATETIME2(0) NOT NULL CONSTRAINT DF_AssignmentSubmissions_Submitted DEFAULT SYSUTCDATETIME(),
        Mark DECIMAL(5,2) NULL,
        Feedback NVARCHAR(MAX) NOT NULL CONSTRAINT DF_AssignmentSubmissions_Feedback DEFAULT N'',
        GradedBy INT NULL CONSTRAINT FK_AssignmentSubmissions_Reviewer REFERENCES dbo.userTable(Id),
        GradedAt DATETIME2(0) NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_AssignmentSubmissions_Chapter FOREIGN KEY(ChapterId,CourseId) REFERENCES dbo.CourseChapters(ChapterId,CourseId),
        CONSTRAINT FK_AssignmentSubmissions_Enrolment FOREIGN KEY(EnrolmentId,CourseId) REFERENCES dbo.CourseEnrolments(EnrolmentId,CourseId),
        CONSTRAINT CK_AssignmentSubmissions_File CHECK(LEN(FileName)>0 AND DATALENGTH(FileData) BETWEEN 1 AND 10485760),
        CONSTRAINT CK_AssignmentSubmissions_Feedback CHECK(DATALENGTH(Feedback)<=10000),
        CONSTRAINT CK_AssignmentSubmissions_Grade CHECK((Mark IS NULL AND GradedBy IS NULL AND GradedAt IS NULL) OR
            (Mark IS NOT NULL AND Mark BETWEEN 0 AND 100 AND GradedBy IS NOT NULL AND GradedAt IS NOT NULL))
    );
    CREATE INDEX IX_AssignmentSubmissions_ChapterEnrolment ON dbo.AssignmentSubmissions(ChapterId,EnrolmentId,SubmissionId);
END;
COMMIT TRANSACTION;
