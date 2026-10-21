SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH(N'dbo.CourseChapters', N'SlidesName') IS NULL
BEGIN
    ALTER TABLE dbo.CourseChapters ADD SlidesName NVARCHAR(180) NULL, SlidesData VARBINARY(MAX) NULL,
        TutorialName NVARCHAR(180) NULL, TutorialData VARBINARY(MAX) NULL;
END;
GO
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_CourseChapters_Slides')
    ALTER TABLE dbo.CourseChapters ADD CONSTRAINT CK_CourseChapters_Slides CHECK (
        (SlidesName IS NULL AND SlidesData IS NULL) OR
        (SlidesName IS NOT NULL AND LEN(SlidesName)>0 AND SlidesData IS NOT NULL AND DATALENGTH(SlidesData) BETWEEN 1 AND 10485760));
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_CourseChapters_TutorialFile')
    ALTER TABLE dbo.CourseChapters ADD CONSTRAINT CK_CourseChapters_TutorialFile CHECK (
        (TutorialName IS NULL AND TutorialData IS NULL) OR
        (TutorialName IS NOT NULL AND LEN(TutorialName)>0 AND TutorialData IS NOT NULL AND DATALENGTH(TutorialData) BETWEEN 1 AND 10485760));
IF EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_CourseChapters_Publication')
    ALTER TABLE dbo.CourseChapters DROP CONSTRAINT CK_CourseChapters_Publication;
ALTER TABLE dbo.CourseChapters ADD CONSTRAINT CK_CourseChapters_Publication CHECK (
    IsPublished=0 OR LEN(LTRIM(RTRIM(LessonText)))>0 OR YouTubeVideoId IS NOT NULL OR SlidesData IS NOT NULL OR TutorialData IS NOT NULL);
COMMIT TRANSACTION;
