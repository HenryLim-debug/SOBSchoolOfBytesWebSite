SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.TeacherMonthlyRates',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeacherMonthlyRates(
        TeacherId INT NOT NULL REFERENCES dbo.userTable(Id),
        SalaryMonth DATE NOT NULL,
        HourlyRate DECIMAL(8,2) NOT NULL,
        UpdatedBy INT NOT NULL REFERENCES dbo.userTable(Id),
        UpdatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT PK_TeacherMonthlyRates PRIMARY KEY(TeacherId,SalaryMonth),
        CONSTRAINT CK_TeacherMonthlyRates_Month CHECK(DAY(SalaryMonth)=1),
        CONSTRAINT CK_TeacherMonthlyRates_Rate CHECK(HourlyRate>0 AND HourlyRate<=10000)
    );
END;
IF OBJECT_ID(N'dbo.TeacherRateChanges',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeacherRateChanges(
        ChangeId INT IDENTITY(1,1) PRIMARY KEY,
        TeacherId INT NOT NULL,
        SalaryMonth DATE NOT NULL,
        PreviousRate DECIMAL(8,2) NULL,
        NewRate DECIMAL(8,2) NOT NULL,
        Reason NVARCHAR(500) NOT NULL,
        ChangedBy INT NOT NULL REFERENCES dbo.userTable(Id),
        ChangedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_TeacherRateChanges_Rate FOREIGN KEY(TeacherId,SalaryMonth)
            REFERENCES dbo.TeacherMonthlyRates(TeacherId,SalaryMonth),
        CONSTRAINT CK_TeacherRateChanges_Reason CHECK(LEN(LTRIM(RTRIM(Reason)))>0)
    );
END;
COMMIT TRANSACTION;
GO
IF OBJECT_ID(N'dbo.TeacherSalarySummary',N'V') IS NULL
    EXEC(N'CREATE VIEW dbo.TeacherSalarySummary AS SELECT 1 AS Placeholder');
GO
-- Calculated earnings, not a payment ledger. Late approvals update the lesson's month.
ALTER VIEW dbo.TeacherSalarySummary AS
WITH Clock AS (
    SELECT DATEADD(HOUR,8,GETUTCDATE()) AS SchoolNow,
        CONVERT(DATE,DATEADD(MONTH,DATEDIFF(MONTH,0,DATEADD(HOUR,8,GETUTCDATE())),0)) AS CurrentMonth
), Months AS (
    SELECT u.Id AS TeacherId,c.CurrentMonth AS SalaryMonth FROM dbo.userTable u CROSS JOIN Clock c WHERE u.usertype=N'Teacher'
    UNION SELECT TeacherId,SalaryMonth FROM dbo.TeacherMonthlyRates
    UNION SELECT TeacherId,CONVERT(DATE,DATEADD(MONTH,DATEDIFF(MONTH,0,SessionStartLocal),0)) FROM dbo.TeachingHourLogs
), Hours AS (
    SELECT l.TeacherId,CONVERT(DATE,DATEADD(MONTH,DATEDIFF(MONTH,0,l.SessionStartLocal),0)) AS SalaryMonth,
        SUM(CASE WHEN l.Status=N'Approved' THEN CONVERT(BIGINT,l.Minutes) ELSE 0 END) AS ApprovedMinutes,
        SUM(CASE WHEN l.Status=N'Pending' THEN CONVERT(BIGINT,l.Minutes) ELSE 0 END) AS PendingMinutes,
        SUM(CASE WHEN l.Status=N'Rejected' THEN CONVERT(BIGINT,l.Minutes) ELSE 0 END) AS RejectedMinutes
    FROM dbo.TeachingHourLogs l CROSS JOIN Clock c
    WHERE DATEADD(MINUTE,l.Minutes,l.SessionStartLocal)<=c.SchoolNow
    GROUP BY l.TeacherId,CONVERT(DATE,DATEADD(MONTH,DATEDIFF(MONTH,0,l.SessionStartLocal),0))
)
SELECT m.TeacherId,u.fname+N' '+u.lname AS TeacherName,u.staffId,u.accountStatus,m.SalaryMonth,
    CAST(COALESCE(h.ApprovedMinutes,0)/60.0 AS DECIMAL(12,2)) AS ApprovedHours,
    CAST(COALESCE(h.PendingMinutes,0)/60.0 AS DECIMAL(12,2)) AS PendingHours,
    CAST(COALESCE(h.RejectedMinutes,0)/60.0 AS DECIMAL(12,2)) AS RejectedHours,
    r.HourlyRate,
    CAST(ROUND(COALESCE(h.ApprovedMinutes,0)*r.HourlyRate/60.0,2) AS DECIMAL(18,2)) AS CalculatedSalary,
    CASE WHEN r.HourlyRate IS NULL THEN N'Rate not set'
         WHEN m.SalaryMonth=c.CurrentMonth THEN N'Current month - accumulating'
         ELSE N'Calculated from approved hours' END AS CalculationStatus
FROM Months m JOIN dbo.userTable u ON u.Id=m.TeacherId AND u.usertype=N'Teacher'
CROSS JOIN Clock c
LEFT JOIN Hours h ON h.TeacherId=m.TeacherId AND h.SalaryMonth=m.SalaryMonth
LEFT JOIN dbo.TeacherMonthlyRates r ON r.TeacherId=m.TeacherId AND r.SalaryMonth=m.SalaryMonth
WHERE m.SalaryMonth<=c.CurrentMonth;
GO
