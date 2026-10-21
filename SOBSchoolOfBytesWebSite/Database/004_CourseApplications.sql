SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.CourseApplications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CourseApplications
    (
        ApplicationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CourseApplications PRIMARY KEY,
        StudentId INT NOT NULL CONSTRAINT FK_CourseApplications_Student REFERENCES dbo.userTable(Id),
        CourseId INT NOT NULL,
        PreferredClassId INT NOT NULL,
        AssignedClassId INT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_CourseApplications_Status DEFAULT N'Pending',
        RequestedAt DATETIME2(0) NOT NULL CONSTRAINT DF_CourseApplications_RequestedAt DEFAULT SYSUTCDATETIME(),
        ReviewedAt DATETIME2(0) NULL,
        ReviewedBy INT NULL CONSTRAINT FK_CourseApplications_Reviewer REFERENCES dbo.userTable(Id),
        CancelledAt DATETIME2(0) NULL,
        CONSTRAINT FK_CourseApplications_PreferredClass FOREIGN KEY (PreferredClassId,CourseId)
            REFERENCES dbo.CourseClasses(ClassId,CourseId),
        CONSTRAINT FK_CourseApplications_AssignedClass FOREIGN KEY (AssignedClassId,CourseId)
            REFERENCES dbo.CourseClasses(ClassId,CourseId),
        CONSTRAINT CK_CourseApplications_State CHECK
        (
            (Status=N'Pending' AND AssignedClassId IS NULL AND ReviewedAt IS NULL AND ReviewedBy IS NULL AND CancelledAt IS NULL)
            OR (Status=N'Approved' AND AssignedClassId IS NOT NULL AND ReviewedAt IS NOT NULL AND ReviewedBy IS NOT NULL AND CancelledAt IS NULL)
            OR (Status=N'Rejected' AND AssignedClassId IS NULL AND ReviewedAt IS NOT NULL AND ReviewedBy IS NOT NULL AND CancelledAt IS NULL)
            OR (Status=N'Cancelled' AND AssignedClassId IS NULL AND ReviewedAt IS NULL AND ReviewedBy IS NULL AND CancelledAt IS NOT NULL)
        )
    );
    CREATE UNIQUE INDEX UQ_CourseApplications_PendingCourse ON dbo.CourseApplications(StudentId,CourseId) WHERE Status=N'Pending';
    CREATE INDEX IX_CourseApplications_Status ON dbo.CourseApplications(Status,RequestedAt,ApplicationId);
END;
COMMIT TRANSACTION;
