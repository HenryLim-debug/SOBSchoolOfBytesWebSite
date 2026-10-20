SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.CourseClasses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CourseClasses
    (
        ClassId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CourseClasses PRIMARY KEY,
        CourseId INT NOT NULL CONSTRAINT FK_CourseClasses_Course REFERENCES dbo.Courses(CourseId),
        TeacherId INT NOT NULL CONSTRAINT FK_CourseClasses_Teacher REFERENCES dbo.userTable(Id),
        ClassName NVARCHAR(80) NOT NULL,
        FirstMonday DATE NOT NULL,
        DayOne TINYINT NOT NULL,
        TimeOne TIME(0) NOT NULL,
        DayTwo TINYINT NOT NULL,
        TimeTwo TIME(0) NOT NULL,
        Capacity TINYINT NOT NULL CONSTRAINT DF_CourseClasses_Capacity DEFAULT 10,
        IsOpen BIT NOT NULL CONSTRAINT DF_CourseClasses_IsOpen DEFAULT 1,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_CourseClasses_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_CourseClasses_CourseKey UNIQUE (ClassId,CourseId),
        CONSTRAINT UQ_CourseClasses_Name UNIQUE (CourseId,ClassName,FirstMonday),
        CONSTRAINT CK_CourseClasses_Name CHECK (LEN(LTRIM(RTRIM(ClassName)))>0),
        CONSTRAINT CK_CourseClasses_Capacity CHECK (Capacity=10),
        CONSTRAINT CK_CourseClasses_Days CHECK (DayOne BETWEEN 1 AND 6 AND DayTwo>DayOne AND DayTwo<=7),
        CONSTRAINT CK_CourseClasses_Monday CHECK (DATEDIFF(DAY,CONVERT(DATE,'19000101',112),FirstMonday)%7=0),
        CONSTRAINT CK_CourseClasses_Times CHECK (TimeOne<='22:00:00' AND TimeTwo<='22:00:00')
    );
    CREATE INDEX IX_CourseClasses_Teacher ON dbo.CourseClasses(TeacherId);
END;
IF OBJECT_ID(N'dbo.CourseEnrolments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CourseEnrolments
    (
        EnrolmentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CourseEnrolments PRIMARY KEY,
        ClassId INT NOT NULL,
        CourseId INT NOT NULL,
        StudentId INT NOT NULL CONSTRAINT FK_CourseEnrolments_Student REFERENCES dbo.userTable(Id),
        IsActive BIT NOT NULL CONSTRAINT DF_CourseEnrolments_IsActive DEFAULT 1,
        EnrolledAt DATETIME2(0) NOT NULL CONSTRAINT DF_CourseEnrolments_EnrolledAt DEFAULT SYSUTCDATETIME(),
        WithdrawnAt DATETIME2(0) NULL,
        CONSTRAINT FK_CourseEnrolments_Class FOREIGN KEY (ClassId,CourseId) REFERENCES dbo.CourseClasses(ClassId,CourseId),
        CONSTRAINT UQ_CourseEnrolments_StudentClass UNIQUE (StudentId,ClassId),
        CONSTRAINT CK_CourseEnrolments_Withdrawal CHECK ((IsActive=1 AND WithdrawnAt IS NULL) OR (IsActive=0 AND WithdrawnAt IS NOT NULL))
    );
    CREATE UNIQUE INDEX UQ_CourseEnrolments_ActiveCourse ON dbo.CourseEnrolments(StudentId,CourseId) WHERE IsActive=1;
    CREATE INDEX IX_CourseEnrolments_Class ON dbo.CourseEnrolments(ClassId,IsActive);
END;
COMMIT TRANSACTION;
