using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public static class SalaryService
    {
        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; } }
        private const string PresidentPermission = @"EXISTS(SELECT 1 FROM dbo.userTable a WITH(HOLDLOCK)
            WHERE a.Id=@actor AND a.usertype=N'President' AND a.accountStatus=N'Active')";
        private const string ReaderPermission = @"EXISTS(SELECT 1 FROM dbo.userTable a WHERE a.Id=@actor AND
            a.accountStatus=N'Active' AND (a.usertype=N'President' OR (a.usertype=N'Teacher' AND @teacher=@actor)))";
        public static DateTime CurrentMonth { get { DateTime now=ClassService.SchoolNow; return new DateTime(now.Year,now.Month,1); } }
        public static DateTime ParseMonth(string input)
        {
            DateTime month;
            if(!DateTime.TryParseExact(input,"yyyy-MM",CultureInfo.InvariantCulture,DateTimeStyles.None,out month) || month.Year<1900 || month>CurrentMonth)
                throw new ClassRuleException("Choose the current month or a past month (1900 onwards). Future salaries are unavailable.");
            return month;
        }
        private static void ValidateMonth(DateTime month)
        { if(month.Day!=1 || month.TimeOfDay!=TimeSpan.Zero || month.Year<1900 || month>CurrentMonth) throw new ClassRuleException("Choose a current or past salary month."); }
        private static SqlParameter Id(string name,int value) { return new SqlParameter(name,SqlDbType.Int){Value=value}; }
        private static SqlParameter Month(DateTime value) { return new SqlParameter("@month",SqlDbType.Date){Value=value}; }
        private static DataTable Query(SqlConnection connection,SqlTransaction transaction,string sql,params SqlParameter[] parameters)
        {
            using(var command=new SqlCommand(sql,connection,transaction)) using(var adapter=new SqlDataAdapter(command))
            { command.Parameters.AddRange(parameters); var table=new DataTable(); adapter.Fill(table); return table; }
        }
        private static DataTable Query(string sql,params SqlParameter[] parameters)
        { using(var connection=new SqlConnection(ConnectionString)) return Query(connection,null,sql,parameters); }
        public static DataTable Teachers(int presidentId)
        { return Query("SELECT Id,fname+N' '+lname+N' ('+COALESCE(staffId,N'')+N', '+accountStatus+N')' AS Label FROM dbo.userTable WHERE usertype=N'Teacher' AND "+PresidentPermission+" ORDER BY fname,lname,Id",Id("@actor",presidentId)); }
        public static DataTable Summary(int actorId,int teacherId,DateTime month)
        {
            ValidateMonth(month);
            return Query("SELECT * FROM dbo.TeacherSalarySummary WHERE TeacherId=@teacher AND SalaryMonth=@month AND "+ReaderPermission,
                Id("@actor",actorId),Id("@teacher",teacherId),Month(month));
        }
        public static DataTable AllTeachers(int presidentId,DateTime month)
        {
            ValidateMonth(month);
            return Query("SELECT * FROM dbo.TeacherSalarySummary WHERE SalaryMonth=@month AND "+PresidentPermission+" ORDER BY TeacherName,TeacherId",Id("@actor",presidentId),Month(month));
        }
        public static DataTable History(int actorId,int teacherId)
        { return Query("SELECT * FROM dbo.TeacherSalarySummary WHERE TeacherId=@teacher AND "+ReaderPermission+" ORDER BY SalaryMonth DESC",Id("@actor",actorId),Id("@teacher",teacherId)); }
        public static DataTable Lessons(int actorId,int teacherId,DateTime month)
        {
            ValidateMonth(month);
            return Query(@"SELECT l.SessionStartLocal,c.ClassName,co.Title AS CourseTitle,l.Status,l.LessonNotes,
                CAST(l.Minutes/60.0 AS DECIMAL(5,2)) AS Hours FROM dbo.TeachingHourLogs l
                JOIN dbo.CourseClasses c ON c.ClassId=l.ClassId JOIN dbo.Courses co ON co.CourseId=c.CourseId
                WHERE l.TeacherId=@teacher AND l.SessionStartLocal>=@month AND l.SessionStartLocal<DATEADD(MONTH,1,@month)
                AND DATEADD(MINUTE,l.Minutes,l.SessionStartLocal)<=DATEADD(HOUR,8,GETUTCDATE()) AND "+ReaderPermission+" ORDER BY l.SessionStartLocal,l.LogId",
                Id("@actor",actorId),Id("@teacher",teacherId),Month(month));
        }
        public static DataRow Rate(int presidentId,int teacherId,DateTime month)
        {
            ValidateMonth(month);
            DataTable rows=Query("SELECT * FROM dbo.TeacherMonthlyRates WHERE TeacherId=@teacher AND SalaryMonth=@month AND "+PresidentPermission,
                Id("@actor",presidentId),Id("@teacher",teacherId),Month(month));
            return rows.Rows.Count==0 ? null : rows.Rows[0];
        }
        public static DataTable RateHistory(int presidentId,int teacherId,DateTime month)
        {
            ValidateMonth(month);
            return Query(@"SELECT r.PreviousRate,r.NewRate,r.Reason,DATEADD(HOUR,8,r.ChangedAt) AS ChangedLocal,
                p.fname+N' '+p.lname AS ChangedByName FROM dbo.TeacherRateChanges r JOIN dbo.userTable p ON p.Id=r.ChangedBy
                WHERE r.TeacherId=@teacher AND r.SalaryMonth=@month AND "+PresidentPermission+" ORDER BY r.ChangeId DESC",
                Id("@actor",presidentId),Id("@teacher",teacherId),Month(month));
        }
        public static void SaveRate(int presidentId,int teacherId,DateTime month,decimal rate,string reason,byte[] expectedVersion)
        {
            ValidateMonth(month); reason=(reason??"").Trim();
            if(rate<=0 || rate>10000 || decimal.Round(rate,2)!=rate) throw new ClassRuleException("Enter an hourly rate from RM 0.01 to RM 10,000.00, with at most two decimal places.");
            if(reason.Length==0 || reason.Length>500) throw new ClassRuleException("Enter a reason for setting or changing the rate (up to 500 characters).");
            using(var connection=new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(var transaction=connection.BeginTransaction())
                {
                    ClassService.LockChanges(connection,transaction);
                    DataTable allowed=Query(connection,transaction,"SELECT Id FROM dbo.userTable WITH(HOLDLOCK) WHERE Id=@teacher AND usertype=N'Teacher' AND "+PresidentPermission,
                        Id("@actor",presidentId),Id("@teacher",teacherId));
                    if(allowed.Rows.Count!=1) throw new ClassRuleException("This teacher or your president access is unavailable. Refresh the page.");
                    DataTable existing=Query(connection,transaction,"SELECT HourlyRate,RowVersion FROM dbo.TeacherMonthlyRates WITH(UPDLOCK,HOLDLOCK) WHERE TeacherId=@teacher AND SalaryMonth=@month",Id("@teacher",teacherId),Month(month));
                    object previous=DBNull.Value;
                    if(existing.Rows.Count!=0)
                    {
                        if(expectedVersion==null || !expectedVersion.SequenceEqual((byte[])existing.Rows[0]["RowVersion"]))
                            throw new ClassRuleException("This rate changed in another window. Load the month again before saving.");
                        previous=existing.Rows[0]["HourlyRate"];
                        if((decimal)previous==rate) throw new ClassRuleException("The rate is unchanged. No update is needed.");
                    }
                    else if(expectedVersion!=null) throw new ClassRuleException("The rate record changed. Load the month again before saving.");
                    using(var command=new SqlCommand(@"UPDATE dbo.TeacherMonthlyRates SET HourlyRate=@rate,UpdatedBy=@actor,UpdatedAt=SYSUTCDATETIME()
                        WHERE TeacherId=@teacher AND SalaryMonth=@month;
                        IF @@ROWCOUNT=0 INSERT dbo.TeacherMonthlyRates(TeacherId,SalaryMonth,HourlyRate,UpdatedBy) VALUES(@teacher,@month,@rate,@actor);
                        INSERT dbo.TeacherRateChanges(TeacherId,SalaryMonth,PreviousRate,NewRate,Reason,ChangedBy)
                        VALUES(@teacher,@month,@previous,@rate,@reason,@actor);",connection,transaction))
                    {
                        command.Parameters.AddRange(new[]{Id("@actor",presidentId),Id("@teacher",teacherId),Month(month),
                            new SqlParameter("@rate",SqlDbType.Decimal){Precision=8,Scale=2,Value=rate},
                            new SqlParameter("@previous",SqlDbType.Decimal){Precision=8,Scale=2,Value=previous},
                            new SqlParameter("@reason",SqlDbType.NVarChar,500){Value=reason}});
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }
    }
}
