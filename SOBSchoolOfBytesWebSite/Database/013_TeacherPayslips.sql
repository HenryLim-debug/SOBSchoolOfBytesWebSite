SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.TeacherPayslips',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeacherPayslips(
        PayslipId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TeacherPayslips PRIMARY KEY,
        TeacherId INT NOT NULL CONSTRAINT FK_TeacherPayslips_Teacher REFERENCES dbo.userTable(Id),
        SalaryMonth DATE NOT NULL,
        Revision INT NOT NULL,
        SupersedesId INT NULL,
        TeacherName NVARCHAR(101) NOT NULL,
        StaffId NVARCHAR(30) NOT NULL,
        HourlyRate DECIMAL(8,2) NOT NULL,
        ApprovedMinutes BIGINT NOT NULL,
        GrossAmount AS CAST(ROUND(CAST(ApprovedMinutes AS DECIMAL(18,2))*HourlyRate/60.0,2) AS DECIMAL(18,2)) PERSISTED,
        SourceFingerprint VARCHAR(64) NOT NULL,
        IssueReason NVARCHAR(500) NOT NULL,
        IssuedBy INT NOT NULL CONSTRAINT FK_TeacherPayslips_Issuer REFERENCES dbo.userTable(Id),
        IssuedByName NVARCHAR(101) NOT NULL,
        IssuedAt DATETIME2(0) NOT NULL CONSTRAINT DF_TeacherPayslips_Date DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_TeacherPayslips_Revision UNIQUE(TeacherId,SalaryMonth,Revision),
        CONSTRAINT UQ_TeacherPayslips_Owner UNIQUE(PayslipId,TeacherId,SalaryMonth),
        CONSTRAINT FK_TeacherPayslips_Previous FOREIGN KEY(SupersedesId,TeacherId,SalaryMonth) REFERENCES dbo.TeacherPayslips(PayslipId,TeacherId,SalaryMonth),
        CONSTRAINT CK_TeacherPayslips_Values CHECK(DAY(SalaryMonth)=1 AND Revision>0 AND HourlyRate>0 AND HourlyRate<=10000 AND ApprovedMinutes>=0),
        CONSTRAINT CK_TeacherPayslips_Reason CHECK(LEN(LTRIM(RTRIM(IssueReason)))>0),
        CONSTRAINT CK_TeacherPayslips_Revision CHECK((Revision=1 AND SupersedesId IS NULL) OR (Revision>1 AND SupersedesId IS NOT NULL))
    );
    CREATE UNIQUE INDEX UX_TeacherPayslips_Replacement ON dbo.TeacherPayslips(SupersedesId) WHERE SupersedesId IS NOT NULL;
END;
IF OBJECT_ID(N'dbo.TeacherPayslipLessons',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeacherPayslipLessons(
        PayslipId INT NOT NULL CONSTRAINT FK_TeacherPayslipLessons_Payslip REFERENCES dbo.TeacherPayslips(PayslipId),
        LogId INT NOT NULL CONSTRAINT FK_TeacherPayslipLessons_Log REFERENCES dbo.TeachingHourLogs(LogId),
        CourseTitle NVARCHAR(200) NOT NULL,
        ClassName NVARCHAR(200) NOT NULL,
        SessionStartLocal DATETIME2(0) NOT NULL,
        Minutes SMALLINT NOT NULL CONSTRAINT CK_TeacherPayslipLessons_Minutes CHECK(Minutes=120),
        CONSTRAINT PK_TeacherPayslipLessons PRIMARY KEY(PayslipId,LogId)
    );
END;
COMMIT TRANSACTION;
