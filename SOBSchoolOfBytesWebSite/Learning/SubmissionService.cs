using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public static class SubmissionService
    {
        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; } }
        private const string Joins=@" FROM dbo.AssignmentSubmissions s
            JOIN dbo.CourseChapters ch ON ch.ChapterId=s.ChapterId AND ch.CourseId=s.CourseId
            JOIN dbo.Courses c ON c.CourseId=s.CourseId
            JOIN dbo.CourseEnrolments e ON e.EnrolmentId=s.EnrolmentId AND e.CourseId=s.CourseId
            JOIN dbo.CourseClasses cl ON cl.ClassId=e.ClassId
            JOIN dbo.userTable student ON student.Id=e.StudentId ";
        private const string StudentPermission=@"e.StudentId=@actor AND e.IsActive=1 AND ch.IsPublished=1
            AND student.usertype=N'Student' AND student.accountStatus=N'Approved'";
        private const string TeacherPermission=@"cl.TeacherId=@actor AND EXISTS(SELECT 1 FROM dbo.TeacherCourses tc WITH(HOLDLOCK)
            JOIN dbo.userTable teacher WITH(HOLDLOCK) ON teacher.Id=tc.TeacherId
            WHERE tc.TeacherId=@actor AND tc.CourseId=s.CourseId AND teacher.usertype=N'Teacher' AND teacher.accountStatus=N'Active')";
        private const string Columns=@"s.SubmissionId,s.FileName,DATEADD(HOUR,8,s.SubmittedAt) AS SubmittedLocal,
            s.Mark,s.Feedback,DATEADD(HOUR,8,s.GradedAt) AS GradedLocal,s.RowVersion,
            CASE WHEN s.Mark IS NULL THEN N'Awaiting review' ELSE N'Graded' END AS ReviewStatus,
            c.Title AS CourseTitle,ch.ChapterNumber,ch.Title AS ChapterTitle,cl.ClassName,
            student.fname+N' '+student.lname AS StudentName,student.username AS StudentUsername";
        private static SqlParameter Id(string name,int value) { return new SqlParameter(name,SqlDbType.Int){Value=value}; }
        private static DataTable Query(string sql,params SqlParameter[] parameters)
        {
            using(var connection=new SqlConnection(ConnectionString)) using(var command=new SqlCommand(sql,connection))
            using(var adapter=new SqlDataAdapter(command))
            { command.Parameters.AddRange(parameters); var table=new DataTable(); adapter.Fill(table); return table; }
        }
        private static DataRow One(DataTable table)
        { if(table.Rows.Count!=1) throw new ClassRuleException("This submission or chapter is unavailable, or you do not have access."); return table.Rows[0]; }
        public static DataRow StudentChapter(int studentId,int chapterId)
        {
            return One(Query(@"SELECT ch.ChapterId,ch.CourseId,ch.ChapterNumber,ch.Title,c.Title AS CourseTitle,cl.ClassName
                FROM dbo.CourseChapters ch JOIN dbo.Courses c ON c.CourseId=ch.CourseId
                JOIN dbo.CourseEnrolments e ON e.CourseId=ch.CourseId JOIN dbo.CourseClasses cl ON cl.ClassId=e.ClassId
                JOIN dbo.userTable student ON student.Id=e.StudentId WHERE ch.ChapterId=@chapter AND "+StudentPermission,
                Id("@actor",studentId),Id("@chapter",chapterId)));
        }
        public static DataTable StudentHistory(int studentId,int chapterId)
        { return Query("SELECT "+Columns+Joins+" WHERE s.ChapterId=@chapter AND "+StudentPermission+" ORDER BY s.SubmissionId DESC",Id("@actor",studentId),Id("@chapter",chapterId)); }
        public static void Submit(int studentId,int chapterId,Guid requestId,SubmissionFile file)
        {
            if(file==null || requestId==Guid.Empty) throw new ClassRuleException("Reopen the chapter and choose your completed work.");
            using(var connection=new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(var transaction=connection.BeginTransaction(IsolationLevel.Serializable))
                using(var command=new SqlCommand(@"IF NOT EXISTS(SELECT 1 FROM dbo.AssignmentSubmissions WITH(UPDLOCK,HOLDLOCK) WHERE RequestId=@request)
                    INSERT dbo.AssignmentSubmissions(ChapterId,CourseId,EnrolmentId,RequestId,FileName,FileData)
                    SELECT ch.ChapterId,ch.CourseId,e.EnrolmentId,@request,@name,@data
                    FROM dbo.CourseChapters ch JOIN dbo.Courses c ON c.CourseId=ch.CourseId
                    JOIN dbo.CourseEnrolments e ON e.CourseId=ch.CourseId JOIN dbo.userTable student ON student.Id=e.StudentId
                    WHERE ch.ChapterId=@chapter AND "+StudentPermission+@";
                    SELECT COUNT(*) FROM dbo.AssignmentSubmissions s JOIN dbo.CourseEnrolments e ON e.EnrolmentId=s.EnrolmentId
                    WHERE s.RequestId=@request AND e.StudentId=@actor AND s.ChapterId=@chapter;",connection,transaction))
                {
                    command.Parameters.AddRange(new[]{Id("@actor",studentId),Id("@chapter",chapterId),
                        new SqlParameter("@request",SqlDbType.UniqueIdentifier){Value=requestId},
                        new SqlParameter("@name",SqlDbType.NVarChar,180){Value=file.Name},new SqlParameter("@data",SqlDbType.VarBinary,-1){Value=file.Data}});
                    if(Convert.ToInt32(command.ExecuteScalar())!=1) throw new ClassRuleException("Your enrolment or chapter access changed. Reopen the chapter before submitting.");
                    transaction.Commit();
                }
            }
        }
        public static DataTable TeacherList(int teacherId)
        { return Query("SELECT "+Columns+Joins+" WHERE "+TeacherPermission+" ORDER BY CASE WHEN s.Mark IS NULL THEN 0 ELSE 1 END,s.SubmissionId DESC",Id("@actor",teacherId)); }
        public static DataRow TeacherSubmission(int teacherId,int submissionId)
        { return One(Query("SELECT "+Columns+Joins+" WHERE s.SubmissionId=@submission AND "+TeacherPermission,Id("@actor",teacherId),Id("@submission",submissionId))); }
        public static void Grade(int teacherId,int submissionId,byte[] version,decimal mark,string feedback)
        {
            feedback=(feedback??"").Trim();
            if(mark<0 || mark>100 || decimal.Round(mark,2)!=mark || feedback.Length>5000)
                throw new ClassRuleException("Enter a mark from 0 to 100 (up to two decimal places) and feedback up to 5,000 characters.");
            if(version==null || version.Length!=8) throw new ClassRuleException("Reopen the submission before saving feedback.");
            using(var connection=new SqlConnection(ConnectionString))
            using(var command=new SqlCommand(@"UPDATE s SET Mark=@mark,Feedback=@feedback,GradedBy=@actor,GradedAt=SYSUTCDATETIME() "+Joins+
                " WHERE s.SubmissionId=@submission AND s.RowVersion=@version AND "+TeacherPermission,connection))
            {
                command.Parameters.AddRange(new[]{Id("@actor",teacherId),Id("@submission",submissionId),
                    new SqlParameter("@mark",SqlDbType.Decimal){Precision=5,Scale=2,Value=mark},
                    new SqlParameter("@feedback",SqlDbType.NVarChar,-1){Value=feedback},new SqlParameter("@version",SqlDbType.Timestamp,8){Value=version}});
                connection.Open();
                if(command.ExecuteNonQuery()!=1) throw new ClassRuleException("This review or your class assignment changed. Reopen the submission before saving again.");
            }
        }
        public static DataRow Download(int actor,string role,int submissionId)
        {
            if(role!="Student" && role!="Teacher") throw new ClassRuleException("Submission unavailable.");
            return One(Query("SELECT s.FileName,s.FileData"+Joins+" WHERE s.SubmissionId=@submission AND "+(role=="Student" ? StudentPermission : TeacherPermission),
                Id("@actor",actor),Id("@submission",submissionId)));
        }
    }
}
