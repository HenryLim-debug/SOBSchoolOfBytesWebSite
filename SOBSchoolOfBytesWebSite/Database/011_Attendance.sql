SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF NOT EXISTS(SELECT 1 FROM sys.key_constraints WHERE name=N'UQ_CourseEnrolments_AttendanceClass')
    ALTER TABLE dbo.CourseEnrolments ADD CONSTRAINT UQ_CourseEnrolments_AttendanceClass UNIQUE(EnrolmentId,ClassId);
IF OBJECT_ID(N'dbo.AttendanceRecords',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceRecords(
        AttendanceId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AttendanceRecords PRIMARY KEY,
        ClassId INT NOT NULL,
        EnrolmentId INT NOT NULL,
        SessionStartLocal DATETIME2(0) NOT NULL,
        Status NVARCHAR(10) NOT NULL CONSTRAINT CK_AttendanceRecords_Status CHECK(Status IN(N'Present',N'Absent',N'Excused')),
        Note NVARCHAR(500) NOT NULL CONSTRAINT DF_AttendanceRecords_Note DEFAULT N'',
        RecordedBy INT NOT NULL CONSTRAINT FK_AttendanceRecords_Recorder REFERENCES dbo.userTable(Id),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_AttendanceRecords_Updated DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_AttendanceRecords_Enrolment FOREIGN KEY(EnrolmentId,ClassId) REFERENCES dbo.CourseEnrolments(EnrolmentId,ClassId),
        CONSTRAINT UQ_AttendanceRecords_Lesson UNIQUE(ClassId,SessionStartLocal,EnrolmentId)
    );
    CREATE INDEX IX_AttendanceRecords_Student ON dbo.AttendanceRecords(EnrolmentId,SessionStartLocal);
END;
COMMIT TRANSACTION;
