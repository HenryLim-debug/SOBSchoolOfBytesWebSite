using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public static class MaterialService
    {
        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; } }
        private const string ChapterColumns = "ch.ChapterId,ch.CourseId,ch.ChapterNumber,ch.Title,ch.LearningObjectives,ch.LessonText,ch.PracticeTask,ch.YouTubeVideoId,ch.IsPublished,ch.RowVersion,ch.SlidesName,ch.TutorialName";
        private const string TeacherPermission = @"EXISTS(SELECT 1 FROM dbo.TeacherCourses tc WITH(HOLDLOCK)
            JOIN dbo.userTable actor WITH(HOLDLOCK) ON actor.Id=tc.TeacherId
            WHERE tc.CourseId=c.CourseId AND tc.TeacherId=@actor AND actor.usertype=N'Teacher' AND actor.accountStatus=N'Active')";
        private const string StudentPermission = @"EXISTS(SELECT 1 FROM dbo.CourseEnrolments e
            JOIN dbo.userTable actor ON actor.Id=e.StudentId WHERE e.CourseId=c.CourseId AND e.StudentId=@actor
            AND e.IsActive=1 AND actor.usertype=N'Student' AND actor.accountStatus=N'Approved')";
        private static SqlParameter Id(string name, int value) { return new SqlParameter(name,SqlDbType.Int){Value=value}; }
        private static SqlParameter Text(string name,string value,int length) { return new SqlParameter(name,SqlDbType.NVarChar,length){Value=value}; }
        private static DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using(var connection=new SqlConnection(ConnectionString))
            using(var command=new SqlCommand(sql,connection))
            using(var adapter=new SqlDataAdapter(command))
            { command.Parameters.AddRange(parameters); var table=new DataTable(); adapter.Fill(table); return table; }
        }
        private static DataRow One(DataTable table)
        {
            if(table.Rows.Count!=1) throw new ClassRuleException("This course or chapter is unavailable, or you do not have access to it.");
            return table.Rows[0];
        }
        public static DataRow TeacherCourse(int teacherId)
        { return One(Query("SELECT c.CourseId,c.Title FROM dbo.Courses c WHERE "+TeacherPermission,Id("@actor",teacherId))); }
        public static DataTable TeacherChapters(int teacherId)
        {
            return Query(@"SELECT ch.ChapterId,ch.ChapterNumber,ch.Title,ch.IsPublished,
                CASE WHEN ch.IsPublished=1 THEN N'Published' ELSE N'Draft' END AS PublicationStatus,
                DATEADD(HOUR,8,ch.UpdatedAt) AS UpdatedAtLocal
                FROM dbo.CourseChapters ch JOIN dbo.Courses c ON c.CourseId=ch.CourseId WHERE "+TeacherPermission+
                " ORDER BY ch.ChapterNumber",Id("@actor",teacherId));
        }
        public static DataRow TeacherChapter(int teacherId,int chapterId)
        { return One(Query("SELECT "+ChapterColumns+" FROM dbo.CourseChapters ch JOIN dbo.Courses c ON c.CourseId=ch.CourseId WHERE ch.ChapterId=@chapter AND "+TeacherPermission,Id("@actor",teacherId),Id("@chapter",chapterId))); }

        public static string VideoId(string link)
        {
            if(string.IsNullOrWhiteSpace(link)) return null;
            Uri uri;
            if(link.Length>2000 || !Uri.TryCreate(link.Trim(),UriKind.Absolute,out uri) || uri.Scheme!=Uri.UriSchemeHttps || !uri.IsDefaultPort || uri.UserInfo.Length!=0)
                throw new ClassRuleException("Enter a valid HTTPS YouTube video link, or leave the video field empty.");
            string host=uri.Host.ToLowerInvariant(), id=null;
            string[] path=uri.AbsolutePath.Trim('/').Split('/');
            if(host=="youtu.be" && path.Length==1) id=path[0];
            else if(host=="youtube.com" || host=="www.youtube.com" || host=="m.youtube.com")
            {
                if(uri.AbsolutePath=="/watch") id=HttpUtility.ParseQueryString(uri.Query)["v"];
                else if(path.Length==2 && (path[0]=="embed" || path[0]=="shorts")) id=path[1];
            }
            else if((host=="youtube-nocookie.com" || host=="www.youtube-nocookie.com") && path.Length==2 && path[0]=="embed") id=path[1];
            if(!ValidVideoId(id)) throw new ClassRuleException("Use a YouTube video link such as https://www.youtube.com/watch?v=... or https://youtu.be/...");
            return id;
        }
        public static bool ValidVideoId(string id) { return id!=null && Regex.IsMatch(id,@"\A[A-Za-z0-9_-]{11}\z"); }
        public static void SaveChapter(int teacherId,int chapterId,byte[] version,string title,string objectives,string videoLink,bool publish,
            ChapterUpload slides,ChapterUpload tutorial,bool removeSlides,bool removeTutorial)
        {
            title=(title??"").Trim(); objectives=(objectives??"").Trim();
            if(title.Length==0 || title.Length>120 || objectives.Length>2000)
                throw new ClassRuleException("Enter a title (up to 120 characters) and objectives up to 2,000 characters.");
            if((removeSlides && slides!=null) || (removeTutorial && tutorial!=null))
                throw new ClassRuleException("Choose either a replacement file or Remove for each document, then save again.");
            if(version==null || version.Length!=8) throw new ClassRuleException("Reopen the chapter before saving it.");
            string videoId=VideoId(videoLink);
            // The version check below ensures this publication check cannot race another teacher's edit.
            DataRow current=TeacherChapter(teacherId,chapterId);
            bool hasSlides=slides!=null || (!removeSlides && current["SlidesName"]!=DBNull.Value);
            bool hasTutorial=tutorial!=null || (!removeTutorial && current["TutorialName"]!=DBNull.Value);
            if(publish && !hasSlides && !hasTutorial && videoId==null && string.IsNullOrWhiteSpace(current["LessonText"].ToString()))
                throw new ClassRuleException("Upload lecture slides, a tutorial document or a YouTube video before publishing. You can save an empty chapter as a draft.");
            using(var connection=new SqlConnection(ConnectionString))
            using(var command=new SqlCommand(@"UPDATE ch SET Title=@title,LearningObjectives=@objectives,
                SlidesName=CASE WHEN @removeSlides=1 THEN NULL ELSE COALESCE(@slidesName,ch.SlidesName) END,
                SlidesData=CASE WHEN @removeSlides=1 THEN NULL ELSE COALESCE(@slidesData,ch.SlidesData) END,
                TutorialName=CASE WHEN @removeTutorial=1 THEN NULL ELSE COALESCE(@tutorialName,ch.TutorialName) END,
                TutorialData=CASE WHEN @removeTutorial=1 THEN NULL ELSE COALESCE(@tutorialData,ch.TutorialData) END,
                YouTubeVideoId=@video,IsPublished=@published,UpdatedAt=SYSUTCDATETIME(),UpdatedBy=@actor
                FROM dbo.CourseChapters ch JOIN dbo.Courses c ON c.CourseId=ch.CourseId
                WHERE ch.ChapterId=@chapter AND ch.RowVersion=@version AND "+TeacherPermission,connection))
            {
                command.Parameters.AddRange(new[]{Id("@actor",teacherId),Id("@chapter",chapterId),Text("@title",title,120),
                    Text("@objectives",objectives,2000),
                    new SqlParameter("@slidesName",SqlDbType.NVarChar,180){Value=slides==null ? (object)DBNull.Value : slides.Name},
                    new SqlParameter("@slidesData",SqlDbType.VarBinary,-1){Value=slides==null ? (object)DBNull.Value : slides.Data},
                    new SqlParameter("@tutorialName",SqlDbType.NVarChar,180){Value=tutorial==null ? (object)DBNull.Value : tutorial.Name},
                    new SqlParameter("@tutorialData",SqlDbType.VarBinary,-1){Value=tutorial==null ? (object)DBNull.Value : tutorial.Data},
                    new SqlParameter("@removeSlides",SqlDbType.Bit){Value=removeSlides},
                    new SqlParameter("@removeTutorial",SqlDbType.Bit){Value=removeTutorial},
                    new SqlParameter("@video",SqlDbType.VarChar,11){Value=(object)videoId??DBNull.Value},
                    new SqlParameter("@published",SqlDbType.Bit){Value=publish},new SqlParameter("@version",SqlDbType.Timestamp,8){Value=version}});
                connection.Open();
                if(command.ExecuteNonQuery()!=1) throw new ClassRuleException("The chapter changed since you opened it, or your course access changed. Copy your unsaved text, then reopen the chapter before saving again.");
            }
        }
        public static DataRow DeleteDocument(int teacherId,int chapterId,byte[] version,bool slides)
        {
            if(version==null || version.Length!=8) throw new ClassRuleException("Reopen the chapter before deleting a file.");
            DataTable result=Query(@"UPDATE ch SET
                SlidesName=CASE WHEN @slides=1 THEN NULL ELSE ch.SlidesName END,
                SlidesData=CASE WHEN @slides=1 THEN NULL ELSE ch.SlidesData END,
                TutorialName=CASE WHEN @slides=0 THEN NULL ELSE ch.TutorialName END,
                TutorialData=CASE WHEN @slides=0 THEN NULL ELSE ch.TutorialData END,
                IsPublished=CASE WHEN LEN(LTRIM(RTRIM(ch.LessonText)))=0 AND ch.YouTubeVideoId IS NULL
                    AND ((@slides=1 AND ch.TutorialData IS NULL) OR (@slides=0 AND ch.SlidesData IS NULL))
                    THEN 0 ELSE ch.IsPublished END,
                UpdatedAt=SYSUTCDATETIME(),UpdatedBy=@actor
                OUTPUT inserted.SlidesName,inserted.TutorialName,inserted.IsPublished,inserted.RowVersion
                FROM dbo.CourseChapters ch JOIN dbo.Courses c ON c.CourseId=ch.CourseId
                WHERE ch.ChapterId=@chapter AND ch.RowVersion=@version
                    AND ((@slides=1 AND ch.SlidesData IS NOT NULL) OR (@slides=0 AND ch.TutorialData IS NOT NULL)) AND "+TeacherPermission,
                Id("@actor",teacherId),Id("@chapter",chapterId),new SqlParameter("@slides",SqlDbType.Bit){Value=slides},
                new SqlParameter("@version",SqlDbType.Timestamp,8){Value=version});
            if(result.Rows.Count!=1) throw new ClassRuleException("The chapter changed, the file was already removed, or your access changed. Reopen the chapter before deleting.");
            return result.Rows[0];
        }
        public static DataRow StudentCourse(int studentId,int courseId)
        { return One(Query("SELECT c.CourseId,c.Title,c.Description FROM dbo.Courses c WHERE c.CourseId=@course AND "+StudentPermission,Id("@actor",studentId),Id("@course",courseId))); }
        public static DataTable PublishedChapters(int studentId,int courseId)
        {
            return Query(@"SELECT ch.ChapterId,ch.CourseId,ch.ChapterNumber,ch.Title FROM dbo.CourseChapters ch
                JOIN dbo.Courses c ON c.CourseId=ch.CourseId WHERE c.CourseId=@course AND ch.IsPublished=1 AND "+StudentPermission+
                " ORDER BY ch.ChapterNumber",Id("@actor",studentId),Id("@course",courseId));
        }
        public static DataRow StudentChapter(int studentId,int courseId,int chapterId)
        {
            return One(Query("SELECT "+ChapterColumns+@" FROM dbo.CourseChapters ch JOIN dbo.Courses c ON c.CourseId=ch.CourseId
                WHERE c.CourseId=@course AND ch.ChapterId=@chapter AND ch.IsPublished=1 AND "+StudentPermission,
                Id("@actor",studentId),Id("@course",courseId),Id("@chapter",chapterId)));
        }
        public static DataRow Download(int actorId,string role,int chapterId,bool slides)
        {
            if(role!="Teacher" && role!="Student") throw new ClassRuleException("Document unavailable.");
            string prefix=slides ? "Slides" : "Tutorial";
            string permission=role=="Teacher" ? TeacherPermission : "ch.IsPublished=1 AND "+StudentPermission;
            return One(Query("SELECT ch."+prefix+"Name AS FileName,ch."+prefix+"Data AS FileData FROM dbo.CourseChapters ch "+
                "JOIN dbo.Courses c ON c.CourseId=ch.CourseId WHERE ch.ChapterId=@chapter AND ch."+prefix+"Data IS NOT NULL AND "+permission,
                Id("@actor",actorId),Id("@chapter",chapterId)));
        }
    }
}
