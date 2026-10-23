-- Preserve existing course chapters and student chapter references. Each teacher
-- receives an independent copy of the currently saved course materials once.
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
DECLARE @lockResult INT;
EXEC @lockResult=sys.sp_getapplock @Resource=N'SOB.ClassEnrolment',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
IF @lockResult<0 THROW 51000,'Another class change is in progress. Try again.',1;
IF OBJECT_ID(N'dbo.TeacherChapterMaterials',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeacherChapterMaterials(
        TeacherId INT NOT NULL CONSTRAINT FK_TeacherChapterMaterials_Teacher REFERENCES dbo.userTable(Id),
        ChapterId INT NOT NULL,
        CourseId INT NOT NULL,
        ChapterNumber TINYINT NOT NULL,
        Title NVARCHAR(120) NOT NULL,
        LearningObjectives NVARCHAR(2000) NOT NULL CONSTRAINT DF_TeacherChapterMaterials_Objectives DEFAULT N'',
        LessonText NVARCHAR(MAX) NOT NULL CONSTRAINT DF_TeacherChapterMaterials_Lesson DEFAULT N'',
        PracticeTask NVARCHAR(MAX) NOT NULL CONSTRAINT DF_TeacherChapterMaterials_Practice DEFAULT N'',
        YouTubeVideoId VARCHAR(11) NULL,
        IsPublished BIT NOT NULL CONSTRAINT DF_TeacherChapterMaterials_Published DEFAULT 0,
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_TeacherChapterMaterials_Updated DEFAULT SYSUTCDATETIME(),
        UpdatedBy INT NULL CONSTRAINT FK_TeacherChapterMaterials_Editor REFERENCES dbo.userTable(Id),
        SlidesName NVARCHAR(180) NULL,
        SlidesData VARBINARY(MAX) NULL,
        TutorialName NVARCHAR(180) NULL,
        TutorialData VARBINARY(MAX) NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT PK_TeacherChapterMaterials PRIMARY KEY(TeacherId,ChapterId),
        CONSTRAINT UQ_TeacherChapterMaterials_Number UNIQUE(TeacherId,CourseId,ChapterNumber),
        CONSTRAINT FK_TeacherChapterMaterials_Chapter FOREIGN KEY(ChapterId,CourseId) REFERENCES dbo.CourseChapters(ChapterId,CourseId),
        CONSTRAINT CK_TeacherChapterMaterials_Number CHECK(ChapterNumber BETWEEN 1 AND 10),
        CONSTRAINT CK_TeacherChapterMaterials_Title CHECK(LEN(LTRIM(RTRIM(Title)))>0),
        CONSTRAINT CK_TeacherChapterMaterials_Lesson CHECK(DATALENGTH(LessonText)<=40000),
        CONSTRAINT CK_TeacherChapterMaterials_Practice CHECK(DATALENGTH(PracticeTask)<=10000),
        CONSTRAINT CK_TeacherChapterMaterials_Video CHECK(YouTubeVideoId IS NULL OR
            (DATALENGTH(YouTubeVideoId)=11 AND YouTubeVideoId COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Za-z0-9_-]%')),
        CONSTRAINT CK_TeacherChapterMaterials_Slides CHECK(
            (SlidesName IS NULL AND SlidesData IS NULL) OR
            (SlidesName IS NOT NULL AND LEN(SlidesName)>0 AND SlidesData IS NOT NULL AND DATALENGTH(SlidesData) BETWEEN 1 AND 10485760)),
        CONSTRAINT CK_TeacherChapterMaterials_Tutorial CHECK(
            (TutorialName IS NULL AND TutorialData IS NULL) OR
            (TutorialName IS NOT NULL AND LEN(TutorialName)>0 AND TutorialData IS NOT NULL AND DATALENGTH(TutorialData) BETWEEN 1 AND 10485760)),
        CONSTRAINT CK_TeacherChapterMaterials_Publication CHECK(IsPublished=0 OR
            LEN(LTRIM(RTRIM(LessonText)))>0 OR YouTubeVideoId IS NOT NULL OR SlidesData IS NOT NULL OR TutorialData IS NOT NULL)
    );
    -- Only the initial migration copies existing materials. Rerunning this file
    -- must never repopulate or overwrite any teacher's independent chapters.
    INSERT dbo.TeacherChapterMaterials(TeacherId,ChapterId,CourseId,ChapterNumber,Title,LearningObjectives,
        LessonText,PracticeTask,YouTubeVideoId,IsPublished,UpdatedAt,UpdatedBy,SlidesName,SlidesData,TutorialName,TutorialData)
    SELECT tc.TeacherId,ch.ChapterId,ch.CourseId,ch.ChapterNumber,ch.Title,ch.LearningObjectives,
        ch.LessonText,ch.PracticeTask,ch.YouTubeVideoId,ch.IsPublished,ch.UpdatedAt,ch.UpdatedBy,
        ch.SlidesName,ch.SlidesData,ch.TutorialName,ch.TutorialData
    FROM dbo.TeacherCourses tc JOIN dbo.CourseChapters ch ON ch.CourseId=tc.CourseId;
END;
COMMIT TRANSACTION;
