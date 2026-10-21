using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public sealed class AttendanceEntry
    {
        public int EnrolmentId { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public string VersionToken { get; set; }
    }
    public static class AttendanceService
    {
        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; } }
        private const string TeacherPermission = @"EXISTS(SELECT 1 FROM dbo.userTable a WITH(HOLDLOCK) JOIN dbo.TeacherCourses tc WITH(HOLDLOCK) ON tc.TeacherId=a.Id
            WHERE a.Id=@actor AND a.usertype=N'Teacher' AND a.accountStatus=N'Active' AND cl.TeacherId=a.Id AND tc.CourseId=cl.CourseId)";
        private const string PresidentPermission = @"EXISTS(SELECT 1 FROM dbo.userTable a WITH(HOLDLOCK) WHERE a.Id=@actor AND a.usertype=N'President' AND a.accountStatus=N'Active')";
        private const string StudentPermission = @"e.StudentId=@actor AND EXISTS(SELECT 1 FROM dbo.userTable a WHERE a.Id=@actor AND a.usertype=N'Student' AND a.accountStatus=N'Approved')";
        // Enrolment dates are stored in UTC; lesson times are Malaysia time. Keep existing records
        // visible even if a student subsequently withdraws or re-enrols in the same class.
        private const string RosterSql = @"SELECT e.EnrolmentId,s.fname+N' '+s.lname AS StudentName,s.username,
            COALESCE(ar.Status,N'') AS AttendanceStatus,COALESCE(ar.Note,N'') AS Note,
            COALESCE(CONVERT(VARCHAR(16),CONVERT(VARBINARY(8),ar.RowVersion),2),'') AS VersionToken,
            DATEADD(HOUR,8,ar.UpdatedAt) AS UpdatedLocal
            FROM dbo.CourseEnrolments e JOIN dbo.userTable s ON s.Id=e.StudentId
            LEFT JOIN dbo.AttendanceRecords ar ON ar.EnrolmentId=e.EnrolmentId AND ar.ClassId=e.ClassId AND ar.SessionStartLocal=@start
            WHERE e.ClassId=@class AND (ar.AttendanceId IS NOT NULL OR
                (e.EnrolledAt<=DATEADD(HOUR,-8,@start) AND (e.WithdrawnAt IS NULL OR e.WithdrawnAt>DATEADD(HOUR,-8,@start))))
            ORDER BY s.fname,s.lname,e.EnrolmentId";
        private const string SaveSql = @"UPDATE dbo.AttendanceRecords SET Status=@status,Note=@note,RecordedBy=@actor,UpdatedAt=SYSUTCDATETIME()
            WHERE ClassId=@class AND EnrolmentId=@enrolment AND SessionStartLocal=@start;
            IF @@ROWCOUNT=0 INSERT dbo.AttendanceRecords(ClassId,EnrolmentId,SessionStartLocal,Status,Note,RecordedBy)
                VALUES(@class,@enrolment,@start,@status,@note,@actor);";
        private static SqlParameter Id(string name,int value) { return new SqlParameter(name,SqlDbType.Int){Value=value}; }
        private static SqlParameter Start(DateTime value) { return new SqlParameter("@start",SqlDbType.DateTime2){Value=value}; }
        private static DataTable Query(SqlConnection connection,SqlTransaction transaction,string sql,params SqlParameter[] parameters)
        {
            using(var command=new SqlCommand(sql,connection,transaction)) using(var adapter=new SqlDataAdapter(command))
            { command.Parameters.AddRange(parameters); var table=new DataTable(); adapter.Fill(table); return table; }
        }
        private static DataTable Query(string sql,params SqlParameter[] parameters)
        { using(var connection=new SqlConnection(ConnectionString)) return Query(connection,null,sql,parameters); }
        private static DataRow Class(SqlConnection connection,SqlTransaction transaction,int actor,int id,bool editing)
        {
            DataTable rows=Query(connection,transaction,"SELECT cl.* FROM dbo.CourseClasses cl WITH(HOLDLOCK) WHERE cl.ClassId=@class AND ("+
                TeacherPermission+(editing ? "" : " OR "+PresidentPermission)+")",Id("@class",id),Id("@actor",actor));
            if(rows.Rows.Count!=1) throw new ClassRuleException("This class is unavailable or you do not have permission to manage its attendance.");
            return rows.Rows[0];
        }
        private static void RequireCompletedLesson(DataRow cl,DateTime start)
        {
            if(!ClassSchedule.FromRow(cl).LessonStarts().Contains(start) || start.AddHours(2)>ClassService.SchoolNow)
                throw new ClassRuleException("Choose a scheduled lesson that has already finished. Future attendance cannot be recorded.");
        }
        public static DataTable Classes(int actor)
        { return Query("SELECT cl.ClassId,c.Title+N' — '+cl.ClassName AS Label FROM dbo.CourseClasses cl JOIN dbo.Courses c ON c.CourseId=cl.CourseId WHERE ("+TeacherPermission+" OR "+PresidentPermission+") ORDER BY cl.FirstMonday DESC,cl.ClassId",Id("@actor",actor)); }
        public static DataTable Sessions(int actor,int id)
        {
            using(var connection=new SqlConnection(ConnectionString))
            {
                DataRow cl=Class(connection,null,actor,id,false);
                var table=new DataTable(); table.Columns.Add("Value"); table.Columns.Add("Label");
                foreach(DateTime start in ClassSchedule.FromRow(cl).LessonStarts())
                    if(start.AddHours(2)<=ClassService.SchoolNow)
                        table.Rows.Add(start.ToString("yyyy-MM-ddTHH:mm:ss",CultureInfo.InvariantCulture),start.ToString("dd MMM yyyy HH:mm",CultureInfo.InvariantCulture)+"–"+start.AddHours(2).ToString("HH:mm",CultureInfo.InvariantCulture));
                return table;
            }
        }
        public static DataTable Roster(int actor,int id,DateTime start)
        {
            using(var connection=new SqlConnection(ConnectionString))
            {
                RequireCompletedLesson(Class(connection,null,actor,id,false),start);
                return Query(connection,null,RosterSql,Id("@class",id),Start(start));
            }
        }
        public static void Save(int actor,int id,DateTime start,IList<AttendanceEntry> entries)
        {
            if(entries==null || entries.Count==0 || entries.Select(x=>x.EnrolmentId).Distinct().Count()!=entries.Count)
                throw new ClassRuleException("Load a lesson with students before saving attendance.");
            foreach(AttendanceEntry entry in entries)
            {
                entry.Note=(entry.Note??"").Trim();
                if(entry.Status!="Present" && entry.Status!="Absent" && entry.Status!="Excused") throw new ClassRuleException("Choose Present, Absent or Excused for every student before saving.");
                if(entry.Note.Length>500) throw new ClassRuleException("Attendance notes must be no longer than 500 characters.");
            }
            using(var connection=new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(var transaction=connection.BeginTransaction())
                {
                    ClassService.LockChanges(connection,transaction);
                    RequireCompletedLesson(Class(connection,transaction,actor,id,true),start);
                    DataTable roster=Query(connection,transaction,RosterSql,Id("@class",id),Start(start));
                    if(roster.Rows.Count!=entries.Count) throw new ClassRuleException("The class roster changed. Reload the lesson before saving.");
                    foreach(AttendanceEntry entry in entries)
                    {
                        DataRow row=roster.AsEnumerable().SingleOrDefault(r=>(int)r["EnrolmentId"]==entry.EnrolmentId);
                        if(row==null || !string.Equals(row["VersionToken"].ToString(),entry.VersionToken??"",StringComparison.Ordinal))
                            throw new ClassRuleException("Attendance changed in another window. Reload the lesson before saving.");
                    }
                    // Validate the entire roster before writing any records; commit all students together.
                    foreach(AttendanceEntry entry in entries)
                    {
                        using(var command=new SqlCommand(SaveSql,connection,transaction))
                        {
                            command.Parameters.AddRange(new[]{Id("@actor",actor),Id("@class",id),Id("@enrolment",entry.EnrolmentId),Start(start),
                                new SqlParameter("@status",SqlDbType.NVarChar,10){Value=entry.Status},new SqlParameter("@note",SqlDbType.NVarChar,500){Value=entry.Note}});
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
        }
        public static DataTable StudentHistory(int actor)
        {
            return Query(@"SELECT c.Title AS CourseTitle,cl.ClassName,ar.SessionStartLocal,ar.Status,ar.Note,DATEADD(HOUR,8,ar.UpdatedAt) AS UpdatedLocal
                FROM dbo.AttendanceRecords ar JOIN dbo.CourseEnrolments e ON e.EnrolmentId=ar.EnrolmentId
                JOIN dbo.CourseClasses cl ON cl.ClassId=ar.ClassId JOIN dbo.Courses c ON c.CourseId=cl.CourseId
                WHERE "+StudentPermission+" ORDER BY ar.SessionStartLocal DESC,c.Title,ar.AttendanceId",Id("@actor",actor));
        }
        public static DataTable StudentSummary(int actor)
        {
            return Query(@"SELECT c.Title AS CourseTitle,cl.ClassName,
                SUM(CASE WHEN ar.Status=N'Present' THEN 1 ELSE 0 END) AS PresentCount,
                SUM(CASE WHEN ar.Status=N'Absent' THEN 1 ELSE 0 END) AS AbsentCount,
                SUM(CASE WHEN ar.Status=N'Excused' THEN 1 ELSE 0 END) AS ExcusedCount,
                CAST(100.0*SUM(CASE WHEN ar.Status=N'Present' THEN 1 ELSE 0 END)/NULLIF(SUM(CASE WHEN ar.Status IN(N'Present',N'Absent') THEN 1 ELSE 0 END),0) AS DECIMAL(5,1)) AS AttendancePercent
                FROM dbo.CourseEnrolments e JOIN dbo.CourseClasses cl ON cl.ClassId=e.ClassId
                JOIN dbo.Courses c ON c.CourseId=cl.CourseId LEFT JOIN dbo.AttendanceRecords ar ON ar.EnrolmentId=e.EnrolmentId
                WHERE "+StudentPermission+@" AND (e.IsActive=1 OR ar.AttendanceId IS NOT NULL)
                GROUP BY cl.ClassId,c.Title,cl.ClassName ORDER BY c.Title,cl.ClassName",Id("@actor",actor));
        }
    }
}
