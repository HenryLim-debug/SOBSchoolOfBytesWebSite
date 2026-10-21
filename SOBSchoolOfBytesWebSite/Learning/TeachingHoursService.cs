using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public static class TeachingHoursService
    {
        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; } }
        private const string TeacherClassPermission=@"cl.TeacherId=@actor AND EXISTS(SELECT 1 FROM dbo.TeacherCourses tc WITH(HOLDLOCK)
            JOIN dbo.userTable a WITH(HOLDLOCK) ON a.Id=tc.TeacherId WHERE tc.TeacherId=@actor AND tc.CourseId=cl.CourseId
            AND a.usertype=N'Teacher' AND a.accountStatus=N'Active')";
        private const string PresidentPermission=@"EXISTS(SELECT 1 FROM dbo.userTable a WITH(HOLDLOCK) WHERE a.Id=@actor AND a.usertype=N'President' AND a.accountStatus=N'Active')";
        private const string TeacherLogPermission=@"l.TeacherId=@actor AND EXISTS(SELECT 1 FROM dbo.userTable a WHERE a.Id=@actor AND a.usertype=N'Teacher' AND a.accountStatus=N'Active')";
        private const string LogJoins=@" FROM dbo.TeachingHourLogs l JOIN dbo.CourseClasses cl ON cl.ClassId=l.ClassId
            JOIN dbo.Courses c ON c.CourseId=cl.CourseId JOIN dbo.userTable teacher ON teacher.Id=l.TeacherId ";
        private const string LogColumns=@"l.*,cl.ClassName,c.Title AS CourseTitle,teacher.fname+N' '+teacher.lname AS TeacherName,
            CAST(l.Minutes/60.0 AS DECIMAL(5,2)) AS Hours,DATEADD(MINUTE,l.Minutes,l.SessionStartLocal) AS SessionEndLocal,
            DATEADD(HOUR,8,l.SubmittedAt) AS SubmittedLocal,DATEADD(HOUR,8,l.ReviewedAt) AS ReviewedLocal";
        private static SqlParameter Id(string name,int value) { return new SqlParameter(name,SqlDbType.Int){Value=value}; }
        private static SqlParameter Text(string name,string value,int size) { return new SqlParameter(name,SqlDbType.NVarChar,size){Value=value}; }
        private static DataTable Query(SqlConnection connection,SqlTransaction transaction,string sql,params SqlParameter[] parameters)
        {
            using(var command=new SqlCommand(sql,connection,transaction)) using(var adapter=new SqlDataAdapter(command))
            { command.Parameters.AddRange(parameters); var table=new DataTable(); adapter.Fill(table); return table; }
        }
        private static DataTable Query(string sql,params SqlParameter[] parameters)
        { using(var connection=new SqlConnection(ConnectionString)) return Query(connection,null,sql,parameters); }
        private static DataRow One(DataTable table)
        { if(table.Rows.Count!=1) throw new ClassRuleException("This class or teaching record is unavailable, or your access has changed."); return table.Rows[0]; }
        public static DataTable Classes(int teacherId)
        { return Query("SELECT cl.ClassId,cl.ClassName,c.Title+N' — '+cl.ClassName AS Label FROM dbo.CourseClasses cl JOIN dbo.Courses c ON c.CourseId=cl.CourseId WHERE "+TeacherClassPermission+" ORDER BY cl.FirstMonday DESC,cl.ClassId",Id("@actor",teacherId)); }
        public static DataTable Sessions(int teacherId,int classId)
        {
            DataRow cl=One(Query("SELECT cl.* FROM dbo.CourseClasses cl WHERE cl.ClassId=@class AND "+TeacherClassPermission,Id("@actor",teacherId),Id("@class",classId)));
            DataTable recorded=Query("SELECT SessionStartLocal,Status FROM dbo.TeachingHourLogs WHERE ClassId=@class",Id("@class",classId));
            var table=new DataTable(); table.Columns.Add("Value"); table.Columns.Add("Label");
            foreach(DateTime start in ClassSchedule.FromRow(cl).LessonStarts())
            {
                if(start.AddHours(2)>ClassService.SchoolNow) continue;
                DataRow existing=recorded.AsEnumerable().FirstOrDefault(r=>(DateTime)r["SessionStartLocal"]==start);
                if(existing!=null && existing["Status"].ToString()!="Rejected") continue;
                table.Rows.Add(start.ToString("yyyy-MM-ddTHH:mm:ss",CultureInfo.InvariantCulture),
                    start.ToString("dd MMM yyyy HH:mm",CultureInfo.InvariantCulture)+"–"+start.AddHours(2).ToString("HH:mm",CultureInfo.InvariantCulture)+
                    (existing==null ? " (2 hours)" : " (rejected — correct and resubmit)"));
            }
            return table;
        }
        public static DataRow Entry(int teacherId,int classId,DateTime start)
        {
            One(Query("SELECT cl.ClassId FROM dbo.CourseClasses cl WHERE cl.ClassId=@class AND "+TeacherClassPermission,Id("@actor",teacherId),Id("@class",classId)));
            DataTable table=Query("SELECT * FROM dbo.TeachingHourLogs WHERE ClassId=@class AND TeacherId=@actor AND SessionStartLocal=@start",
                Id("@actor",teacherId),Id("@class",classId),new SqlParameter("@start",SqlDbType.DateTime2){Value=start});
            return table.Rows.Count==0 ? null : table.Rows[0];
        }
        public static void Submit(int teacherId,int classId,DateTime start,string notes,byte[] expectedVersion)
        {
            notes=(notes??"").Trim();
            if(notes.Length==0 || notes.Length>2000) throw new ClassRuleException("Describe the lesson taught in 1 to 2,000 characters.");
            using(var connection=new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(var transaction=connection.BeginTransaction())
                {
                    ClassService.LockChanges(connection,transaction);
                    DataRow cl=One(Query(connection,transaction,"SELECT cl.* FROM dbo.CourseClasses cl WITH(HOLDLOCK) WHERE cl.ClassId=@class AND "+TeacherClassPermission,
                        Id("@actor",teacherId),Id("@class",classId)));
                    if(!ClassSchedule.FromRow(cl).LessonStarts().Contains(start) || start.AddHours(2)>ClassService.SchoolNow)
                        throw new ClassRuleException("Select a scheduled lesson that has already finished. Future lessons cannot be recorded as taught.");
                    DataTable overlaps=Query(connection,transaction,@"SELECT LogId FROM dbo.TeachingHourLogs WITH(UPDLOCK,HOLDLOCK)
                        WHERE TeacherId=@actor AND ClassId<>@class AND Status IN(N'Pending',N'Approved')
                        AND SessionStartLocal<@end AND DATEADD(MINUTE,Minutes,SessionStartLocal)>@start",
                        Id("@actor",teacherId),Id("@class",classId),new SqlParameter("@start",SqlDbType.DateTime2){Value=start},
                        new SqlParameter("@end",SqlDbType.DateTime2){Value=start.AddHours(2)});
                    if(overlaps.Rows.Count!=0) throw new ClassRuleException("You already recorded another class during these hours. The same teaching time cannot be counted twice.");
                    DataTable existing=Query(connection,transaction,"SELECT * FROM dbo.TeachingHourLogs WITH(UPDLOCK,HOLDLOCK) WHERE ClassId=@class AND SessionStartLocal=@start",
                        Id("@class",classId),new SqlParameter("@start",SqlDbType.DateTime2){Value=start});
                    if(existing.Rows.Count!=0)
                    {
                        DataRow row=existing.Rows[0];
                        if((int)row["TeacherId"]!=teacherId || row["Status"].ToString()!="Rejected" || expectedVersion==null || !expectedVersion.SequenceEqual((byte[])row["RowVersion"]))
                            throw new ClassRuleException("This lesson is already recorded or its review changed. Refresh the list before continuing.");
                        using(var command=new SqlCommand(@"UPDATE dbo.TeachingHourLogs SET LessonNotes=@notes,Status=N'Pending',SubmittedAt=SYSUTCDATETIME(),
                            ReviewNotes=N'',ReviewedBy=NULL,ReviewedAt=NULL WHERE LogId=@log",connection,transaction))
                        { command.Parameters.AddRange(new[]{Text("@notes",notes,2000),Id("@log",(int)row["LogId"])}); command.ExecuteNonQuery(); }
                    }
                    else
                    {
                        if(expectedVersion!=null) throw new ClassRuleException("This record changed. Refresh the list before continuing.");
                        using(var command=new SqlCommand(@"INSERT dbo.TeachingHourLogs(ClassId,TeacherId,SessionStartLocal,LessonNotes)
                            VALUES(@class,@actor,@start,@notes)",connection,transaction))
                        {
                            command.Parameters.AddRange(new[]{Id("@class",classId),Id("@actor",teacherId),new SqlParameter("@start",SqlDbType.DateTime2){Value=start},Text("@notes",notes,2000)});
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
        }
        public static DataTable Logs(int actorId,bool president)
        { return Query("SELECT "+LogColumns+LogJoins+" WHERE "+(president ? PresidentPermission : TeacherLogPermission)+" ORDER BY CASE WHEN l.Status=N'Pending' THEN 0 ELSE 1 END,l.SessionStartLocal DESC,l.LogId DESC",Id("@actor",actorId)); }
        public static DataTable Monthly(int actorId,bool president)
        {
            return Query(@"SELECT l.TeacherId,teacher.fname+N' '+teacher.lname AS TeacherName,CONVERT(CHAR(7),l.SessionStartLocal,126) AS TeachingMonth,
                CAST(SUM(CASE WHEN l.Status=N'Approved' THEN l.Minutes ELSE 0 END)/60.0 AS DECIMAL(10,2)) AS ApprovedHours,
                CAST(SUM(CASE WHEN l.Status=N'Pending' THEN l.Minutes ELSE 0 END)/60.0 AS DECIMAL(10,2)) AS PendingHours,
                CAST(SUM(CASE WHEN l.Status=N'Rejected' THEN l.Minutes ELSE 0 END)/60.0 AS DECIMAL(10,2)) AS RejectedHours "+LogJoins+" WHERE "+
                (president ? PresidentPermission : TeacherLogPermission)+@" GROUP BY l.TeacherId,teacher.fname,teacher.lname,CONVERT(CHAR(7),l.SessionStartLocal,126)
                ORDER BY TeachingMonth DESC,TeacherName",Id("@actor",actorId));
        }
        public static DataRow ReviewRecord(int presidentId,int logId)
        { return One(Query("SELECT "+LogColumns+LogJoins+" WHERE l.LogId=@log AND "+PresidentPermission,Id("@actor",presidentId),Id("@log",logId))); }
        public static DataTable ReviewHistory(int presidentId,int logId)
        {
            return Query(@"SELECT r.Decision,r.LessonNotes,r.ReviewNotes,DATEADD(HOUR,8,r.ReviewedAt) AS ReviewedLocal,
                p.fname+N' '+p.lname AS ReviewedByName FROM dbo.TeachingHourReviews r JOIN dbo.userTable p ON p.Id=r.PresidentId
                WHERE r.LogId=@log AND "+PresidentPermission+" ORDER BY r.ReviewId DESC",Id("@actor",presidentId),Id("@log",logId));
        }
        public static void Review(int presidentId,int logId,byte[] version,bool approve,string notes)
        {
            notes=(notes??"").Trim();
            if(notes.Length>1000 || (!approve && notes.Length==0)) throw new ClassRuleException("Enter a reason when rejecting a record. Review notes must be no longer than 1,000 characters.");
            if(version==null || version.Length!=8) throw new ClassRuleException("Reopen the teaching record before reviewing it.");
            using(var connection=new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(var transaction=connection.BeginTransaction())
                {
                    ClassService.LockChanges(connection,transaction);
                    DataRow record=One(Query(connection,transaction,@"SELECT l.* FROM dbo.TeachingHourLogs l WITH(UPDLOCK,HOLDLOCK)
                        WHERE l.LogId=@log AND l.Status=N'Pending' AND l.RowVersion=@version AND "+PresidentPermission,
                        Id("@log",logId),Id("@actor",presidentId),new SqlParameter("@version",SqlDbType.Timestamp,8){Value=version}));
                    if(((DateTime)record["SessionStartLocal"]).AddMinutes((short)record["Minutes"])>ClassService.SchoolNow)
                        throw new ClassRuleException("This lesson has not finished yet.");
                    string decision=approve ? "Approved" : "Rejected";
                    using(var command=new SqlCommand(@"UPDATE dbo.TeachingHourLogs SET Status=@status,ReviewNotes=@notes,ReviewedBy=@actor,ReviewedAt=SYSUTCDATETIME() WHERE LogId=@log;
                        INSERT dbo.TeachingHourReviews(LogId,PresidentId,Decision,LessonNotes,ReviewNotes) VALUES(@log,@actor,@status,@lesson,@notes);",connection,transaction))
                    {
                        command.Parameters.AddRange(new[]{Id("@log",logId),Id("@actor",presidentId),Text("@status",decision,10),Text("@notes",notes,1000),Text("@lesson",record["LessonNotes"].ToString(),2000)});
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }
    }
}
