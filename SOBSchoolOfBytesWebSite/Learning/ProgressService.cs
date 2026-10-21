using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public static class ProgressService
    {
        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; } }
        private const string Access = @"e.IsActive=1 AND c.IsAvailable=1 AND EXISTS(SELECT 1 FROM dbo.userTable actor WHERE actor.Id=@actor AND
            ((actor.usertype=N'Student' AND actor.accountStatus=N'Approved' AND e.StudentId=actor.Id)
            OR (actor.usertype=N'Teacher' AND actor.accountStatus=N'Active' AND cl.TeacherId=actor.Id
                AND EXISTS(SELECT 1 FROM dbo.TeacherCourses tc WHERE tc.TeacherId=actor.Id AND tc.CourseId=e.CourseId))
            OR (actor.usertype=N'President' AND actor.accountStatus=N'Active')))";
        private const string Joins = @" FROM dbo.CourseEnrolments e JOIN dbo.CourseClasses cl ON cl.ClassId=e.ClassId
            JOIN dbo.Courses c ON c.CourseId=e.CourseId JOIN dbo.userTable s ON s.Id=e.StudentId ";
        private const string SummarySql = @"SELECT e.EnrolmentId,c.Title AS CourseTitle,cl.ClassName,s.fname+N' '+s.lname AS StudentName,s.username,
            COUNT(ch.ChapterId) AS TotalChapters,
            SUM(CASE WHEN ch.IsPublished=1 THEN 1 ELSE 0 END) AS PublishedChapters,
            SUM(CASE WHEN ch.IsPublished=1 AND cp.ChapterId IS NOT NULL THEN 1 ELSE 0 END) AS CompletedChapters,
            CAST(100.0*SUM(CASE WHEN ch.IsPublished=1 AND cp.ChapterId IS NOT NULL THEN 1 ELSE 0 END)/NULLIF(COUNT(ch.ChapterId),0) AS DECIMAL(5,1)) AS CompletionPercent,
            SUM(CASE WHEN ch.IsPublished=1 AND latest.SubmissionId IS NOT NULL THEN 1 ELSE 0 END) AS SubmittedChapters,
            SUM(CASE WHEN ch.IsPublished=1 AND latest.Mark IS NOT NULL THEN 1 ELSE 0 END) AS MarkedChapters";
        private const string ChapterJoins = @" LEFT JOIN dbo.CourseChapters ch ON ch.CourseId=e.CourseId
            LEFT JOIN dbo.ChapterCompletions cp ON cp.EnrolmentId=e.EnrolmentId AND cp.ChapterId=ch.ChapterId
            OUTER APPLY(SELECT TOP(1) a.SubmissionId,a.Mark FROM dbo.AssignmentSubmissions a
                WHERE a.EnrolmentId=e.EnrolmentId AND a.ChapterId=ch.ChapterId ORDER BY a.SubmissionId DESC) latest ";
        private const string DetailSql = @"SELECT ch.CourseId,ch.ChapterId,ch.ChapterNumber,ch.Title,
            CASE WHEN cp.ChapterId IS NULL THEN N'Not completed' ELSE N'Completed' END AS CompletionStatus,
            DATEADD(HOUR,8,cp.CompletedAt) AS CompletedLocal,
            CASE WHEN latest.SubmissionId IS NULL THEN N'Not submitted' WHEN latest.Mark IS NULL THEN N'Awaiting marking' ELSE N'Marked' END AS SubmissionStatus,
            latest.Mark";
        private const string EligibleSql = @"SELECT e.EnrolmentId,e.CourseId FROM dbo.CourseEnrolments e WITH(UPDLOCK,HOLDLOCK)
            JOIN dbo.Courses c WITH(HOLDLOCK) ON c.CourseId=e.CourseId
            JOIN dbo.CourseChapters ch WITH(HOLDLOCK) ON ch.CourseId=e.CourseId
            JOIN dbo.userTable s WITH(HOLDLOCK) ON s.Id=e.StudentId
            WHERE e.StudentId=@actor AND e.IsActive=1 AND c.IsAvailable=1 AND ch.ChapterId=@chapter AND ch.IsPublished=1
                AND s.usertype=N'Student' AND s.accountStatus=N'Approved'";
        private const string SetSql = @"IF @complete=1
            BEGIN
                IF NOT EXISTS(SELECT 1 FROM dbo.ChapterCompletions WHERE EnrolmentId=@enrolment AND ChapterId=@chapter)
                    INSERT dbo.ChapterCompletions(EnrolmentId,ChapterId,CourseId) VALUES(@enrolment,@chapter,@course);
            END
            ELSE DELETE dbo.ChapterCompletions WHERE EnrolmentId=@enrolment AND ChapterId=@chapter;";
        private static SqlParameter Id(string name,int value) { return new SqlParameter(name,SqlDbType.Int){Value=value}; }
        private static DataTable Query(string sql,params SqlParameter[] parameters)
        {
            using(var connection=new SqlConnection(ConnectionString))
            using(var command=new SqlCommand(sql,connection))
            using(var adapter=new SqlDataAdapter(command))
            { command.Parameters.AddRange(parameters); var table=new DataTable(); adapter.Fill(table); return table; }
        }
        public static DataTable Summary(int actor)
        { return Query(SummarySql+Joins+ChapterJoins+" WHERE "+Access+" GROUP BY e.EnrolmentId,c.Title,cl.ClassName,s.fname,s.lname,s.username ORDER BY c.Title,cl.ClassName,s.fname,s.lname",Id("@actor",actor)); }
        public static DataTable Chapters(int actor,int enrolment)
        { return Query(DetailSql+Joins+ChapterJoins+" WHERE e.EnrolmentId=@enrolment AND ch.IsPublished=1 AND "+Access+" ORDER BY ch.ChapterNumber",Id("@actor",actor),Id("@enrolment",enrolment)); }
        public static bool IsCompleted(int student,int chapter)
        {
            DataTable rows=Query(@"SELECT cp.ChapterId FROM dbo.ChapterCompletions cp JOIN dbo.CourseEnrolments e ON e.EnrolmentId=cp.EnrolmentId
                JOIN dbo.CourseClasses cl ON cl.ClassId=e.ClassId JOIN dbo.Courses c ON c.CourseId=e.CourseId
                JOIN dbo.CourseChapters ch ON ch.ChapterId=cp.ChapterId
                WHERE e.StudentId=@actor AND cp.ChapterId=@chapter AND ch.IsPublished=1 AND "+Access,Id("@actor",student),Id("@chapter",chapter));
            return rows.Rows.Count==1;
        }
        public static void SetCompleted(int student,int chapter,bool completed)
        {
            using(var connection=new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(var transaction=connection.BeginTransaction())
                {
                    ClassService.LockChanges(connection,transaction);
                    int enrolment,course;
                    using(var command=new SqlCommand(EligibleSql,connection,transaction))
                    {
                        command.Parameters.AddRange(new[]{Id("@actor",student),Id("@chapter",chapter)});
                        using(var reader=command.ExecuteReader())
                        {
                            if(!reader.Read()) throw new ClassRuleException("This chapter is unavailable or you are no longer enrolled. Reload your dashboard.");
                            enrolment=reader.GetInt32(0); course=reader.GetInt32(1);
                        }
                    }
                    using(var command=new SqlCommand(SetSql,connection,transaction))
                    {
                        command.Parameters.AddRange(new[]{Id("@enrolment",enrolment),Id("@chapter",chapter),Id("@course",course),new SqlParameter("@complete",SqlDbType.Bit){Value=completed}});
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }
    }
}
