SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.ClassMeetingLinks',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClassMeetingLinks(
        ClassId INT NOT NULL CONSTRAINT PK_ClassMeetingLinks PRIMARY KEY
            CONSTRAINT FK_ClassMeetingLinks_Class REFERENCES dbo.CourseClasses(ClassId),
        MeetingUrl NVARCHAR(2048) NULL,
        UpdatedBy INT NOT NULL CONSTRAINT FK_ClassMeetingLinks_Editor REFERENCES dbo.userTable(Id),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ClassMeetingLinks_Updated DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT CK_ClassMeetingLinks_NotEmpty CHECK(MeetingUrl IS NULL OR LEN(MeetingUrl)>0)
    );
END;
COMMIT TRANSACTION;
