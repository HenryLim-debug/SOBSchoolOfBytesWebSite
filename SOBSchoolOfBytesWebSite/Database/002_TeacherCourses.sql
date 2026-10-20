SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.TeacherCourses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeacherCourses
    (
        TeacherId INT NOT NULL CONSTRAINT PK_TeacherCourses PRIMARY KEY,
        CourseId INT NOT NULL,
        AssignedAt DATETIME2(0) NOT NULL CONSTRAINT DF_TeacherCourses_AssignedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_TeacherCourses_Teacher FOREIGN KEY (TeacherId) REFERENCES dbo.userTable(Id),
        CONSTRAINT FK_TeacherCourses_Course FOREIGN KEY (CourseId) REFERENCES dbo.Courses(CourseId)
    );
    CREATE INDEX IX_TeacherCourses_CourseId ON dbo.TeacherCourses(CourseId);
END;
COMMIT TRANSACTION;
