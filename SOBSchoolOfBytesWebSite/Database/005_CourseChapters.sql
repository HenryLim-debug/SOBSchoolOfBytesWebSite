SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.CourseChapters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CourseChapters
    (
        ChapterId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CourseChapters PRIMARY KEY,
        CourseId INT NOT NULL CONSTRAINT FK_CourseChapters_Course REFERENCES dbo.Courses(CourseId),
        ChapterNumber TINYINT NOT NULL,
        Title NVARCHAR(120) NOT NULL,
        LearningObjectives NVARCHAR(2000) NOT NULL CONSTRAINT DF_CourseChapters_Objectives DEFAULT N'',
        LessonText NVARCHAR(MAX) NOT NULL CONSTRAINT DF_CourseChapters_Lesson DEFAULT N'',
        PracticeTask NVARCHAR(MAX) NOT NULL CONSTRAINT DF_CourseChapters_Practice DEFAULT N'',
        YouTubeVideoId VARCHAR(11) NULL,
        IsPublished BIT NOT NULL CONSTRAINT DF_CourseChapters_Published DEFAULT 0,
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_CourseChapters_UpdatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedBy INT NULL CONSTRAINT FK_CourseChapters_Editor REFERENCES dbo.userTable(Id),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT UQ_CourseChapters_Number UNIQUE(CourseId,ChapterNumber),
        CONSTRAINT CK_CourseChapters_Number CHECK (ChapterNumber BETWEEN 1 AND 10),
        CONSTRAINT CK_CourseChapters_Title CHECK (LEN(LTRIM(RTRIM(Title)))>0),
        CONSTRAINT CK_CourseChapters_Lesson CHECK (DATALENGTH(LessonText)<=40000),
        CONSTRAINT CK_CourseChapters_Practice CHECK (DATALENGTH(PracticeTask)<=10000),
        CONSTRAINT CK_CourseChapters_Video CHECK (YouTubeVideoId IS NULL OR
            (DATALENGTH(YouTubeVideoId)=11 AND YouTubeVideoId COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Za-z0-9_-]%')),
        CONSTRAINT CK_CourseChapters_Publication CHECK (IsPublished=0 OR LEN(LTRIM(RTRIM(LessonText)))>0 OR YouTubeVideoId IS NOT NULL)
    );
END;
-- Rerunning this migration only fills missing chapter slots; it never overwrites lessons.
INSERT dbo.CourseChapters(CourseId,ChapterNumber,Title)
SELECT c.CourseId,n.ChapterNumber,N'Chapter '+CONVERT(NVARCHAR(2),n.ChapterNumber)
FROM dbo.Courses c CROSS JOIN (VALUES(1),(2),(3),(4),(5),(6),(7),(8),(9),(10)) n(ChapterNumber)
WHERE NOT EXISTS(SELECT 1 FROM dbo.CourseChapters ch WHERE ch.CourseId=c.CourseId AND ch.ChapterNumber=n.ChapterNumber);
COMMIT TRANSACTION;
