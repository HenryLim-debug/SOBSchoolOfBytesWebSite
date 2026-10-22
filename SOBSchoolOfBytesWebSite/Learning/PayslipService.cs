using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public sealed class PayslipPreview
    {
        public DataRow Teacher { get; set; }
        public DataTable Lessons { get; set; }
        public DateTime Month { get; set; }
        public decimal Rate { get; set; }
        public long Minutes { get; set; }
        public decimal Amount { get { return decimal.Round(Minutes*Rate/60m,2,MidpointRounding.AwayFromZero); } }
        public string Fingerprint { get; set; }
        public int PreviousId { get; set; }
        public int NextRevision { get; set; }
        public string PreviousFingerprint { get; set; }
    }
    public static class PayslipService
    {
        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; } }
        private const string PresidentPermission = @"EXISTS(SELECT 1 FROM dbo.userTable a WITH(HOLDLOCK) WHERE a.Id=@actor AND a.usertype=N'President' AND a.accountStatus=N'Active')";
        private const string ReaderPermission = @"EXISTS(SELECT 1 FROM dbo.userTable a WHERE a.Id=@actor AND a.accountStatus=N'Active'
            AND (a.usertype=N'President' OR (a.usertype=N'Teacher' AND p.TeacherId=a.Id)))
            AND p.SalaryMonth<=CONVERT(DATE,DATEADD(MONTH,DATEDIFF(MONTH,0,DATEADD(HOUR,8,GETUTCDATE())),0))";
        private const string SourceTeacherSql = @"SELECT t.Id,t.fname+N' '+t.lname AS TeacherName,COALESCE(t.staffId,N'') AS StaffId,
            r.HourlyRate FROM dbo.userTable t WITH(HOLDLOCK)
            LEFT JOIN dbo.TeacherMonthlyRates r WITH(HOLDLOCK) ON r.TeacherId=t.Id AND r.SalaryMonth=@month
            WHERE t.Id=@teacher AND t.usertype=N'Teacher' AND ";
        private const string SourceLessonsSql = @"SELECT l.LogId,c.Title AS CourseTitle,cl.ClassName,l.SessionStartLocal,l.Minutes,
            CAST(l.Minutes/60.0 AS DECIMAL(5,2)) AS Hours
            FROM dbo.TeachingHourLogs l WITH(HOLDLOCK) JOIN dbo.CourseClasses cl WITH(HOLDLOCK) ON cl.ClassId=l.ClassId
            JOIN dbo.Courses c WITH(HOLDLOCK) ON c.CourseId=cl.CourseId
            WHERE l.TeacherId=@teacher AND l.Status=N'Approved' AND l.SessionStartLocal>=@month AND l.SessionStartLocal<DATEADD(MONTH,1,@month)
            AND DATEADD(MINUTE,l.Minutes,l.SessionStartLocal)<=DATEADD(HOUR,8,GETUTCDATE()) ORDER BY l.LogId";
        private const string LatestSql = @"SELECT TOP(1) PayslipId,Revision,SourceFingerprint FROM dbo.TeacherPayslips WITH(UPDLOCK,HOLDLOCK)
            WHERE TeacherId=@teacher AND SalaryMonth=@month ORDER BY Revision DESC";
        private const string InsertSql = @"INSERT dbo.TeacherPayslips(TeacherId,SalaryMonth,Revision,SupersedesId,TeacherName,StaffId,HourlyRate,ApprovedMinutes,SourceFingerprint,IssueReason,IssuedBy,IssuedByName)
            SELECT @teacher,@month,@revision,@previous,@name,@staff,@rate,@minutes,@fingerprint,@reason,@actor,a.fname+N' '+a.lname
            FROM dbo.userTable a WHERE a.Id=@actor AND a.usertype=N'President' AND a.accountStatus=N'Active';
            SELECT CONVERT(INT,SCOPE_IDENTITY());";
        private const string InsertLessonSql = @"INSERT dbo.TeacherPayslipLessons(PayslipId,LogId,CourseTitle,ClassName,SessionStartLocal,Minutes)
            VALUES(@payslip,@log,@courseTitle,@className,@start,@minutes)";
        private const string DisplayColumns = @"p.*,CAST(p.ApprovedMinutes/60.0 AS DECIMAL(12,2)) AS ApprovedHours,
            DATEADD(HOUR,8,p.IssuedAt) AS IssuedLocal,
            CASE WHEN EXISTS(SELECT 1 FROM dbo.TeacherPayslips newer WHERE newer.SupersedesId=p.PayslipId)
                THEN N'Replaced by a newer revision' ELSE N'Latest issued revision' END AS VersionStatus";
        private static SqlParameter Id(string name,int value) { return new SqlParameter(name,SqlDbType.Int){Value=value}; }
        private static SqlParameter Month(DateTime value) { return new SqlParameter("@month",SqlDbType.Date){Value=value}; }
        private static SqlParameter Text(string name,string value,int length) { return new SqlParameter(name,SqlDbType.NVarChar,length){Value=value}; }
        private static DataTable Query(SqlConnection connection,SqlTransaction transaction,string sql,params SqlParameter[] parameters)
        {
            using(var command=new SqlCommand(sql,connection,transaction)) using(var adapter=new SqlDataAdapter(command))
            { command.Parameters.AddRange(parameters); var table=new DataTable(); adapter.Fill(table); return table; }
        }
        private static DataTable Query(string sql,params SqlParameter[] parameters)
        { using(var connection=new SqlConnection(ConnectionString)) return Query(connection,null,sql,parameters); }
        private static void ValidateMonth(DateTime month)
        { if(month.Year<1900 || month.Day!=1 || month.TimeOfDay!=TimeSpan.Zero || month>SalaryService.CurrentMonth) throw new ClassRuleException("Choose the current month or a past month. Future payslips are unavailable."); }
        private static PayslipPreview ReadPreview(SqlConnection connection,SqlTransaction transaction,int actor,int teacher,DateTime month)
        {
            ValidateMonth(month);
            DataTable rows=Query(connection,transaction,SourceTeacherSql+PresidentPermission,Id("@actor",actor),Id("@teacher",teacher),Month(month));
            if(rows.Rows.Count!=1) throw new ClassRuleException("This teacher or your president access is unavailable.");
            if(rows.Rows[0].IsNull("HourlyRate")) throw new ClassRuleException("Set the teacher's hourly rate for this month in Teacher Salaries before preparing a payslip.");
            var preview=new PayslipPreview { Teacher=rows.Rows[0],Month=month,Rate=(decimal)rows.Rows[0]["HourlyRate"],NextRevision=1 };
            preview.Lessons=Query(connection,transaction,SourceLessonsSql,Id("@teacher",teacher),Month(month));
            foreach(DataRow row in preview.Lessons.Rows) preview.Minutes+=Convert.ToInt64(row["Minutes"],CultureInfo.InvariantCulture);
            // Serialize typed, length-prefixed fields so names or delimiters cannot create ambiguous hashes.
            using(var stream=new MemoryStream())
            {
                using(var writer=new BinaryWriter(stream,Encoding.UTF8,true))
                {
                    writer.Write(teacher); writer.Write(month.Ticks); writer.Write(preview.Teacher["TeacherName"].ToString());
                    writer.Write(preview.Teacher["StaffId"].ToString()); writer.Write(preview.Rate); writer.Write(preview.Minutes);
                    foreach(DataRow row in preview.Lessons.Rows)
                    {
                        writer.Write((int)row["LogId"]); writer.Write(row["CourseTitle"].ToString()); writer.Write(row["ClassName"].ToString());
                        writer.Write(((DateTime)row["SessionStartLocal"]).Ticks); writer.Write(Convert.ToInt32(row["Minutes"],CultureInfo.InvariantCulture));
                    }
                }
                using(var sha=SHA256.Create()) preview.Fingerprint=BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-","");
            }
            DataTable previous=Query(connection,transaction,LatestSql,Id("@teacher",teacher),Month(month));
            if(previous.Rows.Count==1)
            { preview.PreviousId=(int)previous.Rows[0]["PayslipId"]; preview.NextRevision=(int)previous.Rows[0]["Revision"]+1; preview.PreviousFingerprint=previous.Rows[0]["SourceFingerprint"].ToString(); }
            return preview;
        }
        public static PayslipPreview Preview(int actor,int teacher,DateTime month)
        {
            using(var connection=new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(var transaction=connection.BeginTransaction())
                { ClassService.LockChanges(connection,transaction); var preview=ReadPreview(connection,transaction,actor,teacher,month); transaction.Commit(); return preview; }
            }
        }
        public static int Issue(int actor,int teacher,DateTime month,string expectedFingerprint,int expectedPreviousId,string reason)
        {
            reason=(reason??"").Trim();
            if(reason.Length==0 || reason.Length>500) throw new ClassRuleException("Enter a reason for issuing this payslip or revision, up to 500 characters.");
            using(var connection=new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(var transaction=connection.BeginTransaction())
                {
                    ClassService.LockChanges(connection,transaction);
                    PayslipPreview preview=ReadPreview(connection,transaction,actor,teacher,month);
                    if(!string.Equals(preview.Fingerprint,expectedFingerprint,StringComparison.Ordinal))
                        throw new ClassRuleException("The approved hours, rate or teacher details changed. Preview the payslip again before issuing it.");
                    // A repeated click or an unchanged preview returns the existing record instead of duplicating it.
                    if(preview.PreviousId!=0 && preview.PreviousFingerprint==preview.Fingerprint)
                    { transaction.Commit(); return preview.PreviousId; }
                    if(preview.PreviousId!=expectedPreviousId) throw new ClassRuleException("Another president issued a revision. Preview the payslip again.");
                    int payslipId;
                    using(var command=new SqlCommand(InsertSql,connection,transaction))
                    {
                        command.Parameters.AddRange(new[]{Id("@teacher",teacher),Month(month),Id("@revision",preview.NextRevision),
                            new SqlParameter("@previous",SqlDbType.Int){Value=preview.PreviousId==0 ? (object)DBNull.Value : preview.PreviousId},
                            Text("@name",preview.Teacher["TeacherName"].ToString(),101),Text("@staff",preview.Teacher["StaffId"].ToString(),30),
                            new SqlParameter("@rate",SqlDbType.Decimal){Precision=8,Scale=2,Value=preview.Rate},
                            new SqlParameter("@minutes",SqlDbType.BigInt){Value=preview.Minutes},
                            new SqlParameter("@fingerprint",SqlDbType.VarChar,64){Value=preview.Fingerprint},Text("@reason",reason,500),Id("@actor",actor)});
                        object result=command.ExecuteScalar();
                        if(result==null || result==DBNull.Value) throw new ClassRuleException("Your president access changed. The payslip was not issued.");
                        payslipId=Convert.ToInt32(result,CultureInfo.InvariantCulture);
                    }
                    foreach(DataRow row in preview.Lessons.Rows)
                    {
                        using(var command=new SqlCommand(InsertLessonSql,connection,transaction))
                        {
                            command.Parameters.AddRange(new[]{Id("@payslip",payslipId),Id("@log",(int)row["LogId"]),
                                Text("@courseTitle",row["CourseTitle"].ToString(),200),Text("@className",row["ClassName"].ToString(),200),
                                new SqlParameter("@start",SqlDbType.DateTime2){Value=row["SessionStartLocal"]},new SqlParameter("@minutes",SqlDbType.SmallInt){Value=row["Minutes"]}});
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit(); return payslipId;
                }
            }
        }
        public static DataTable History(int actor)
        { return Query("SELECT "+DisplayColumns+" FROM dbo.TeacherPayslips p WHERE "+ReaderPermission+" ORDER BY p.SalaryMonth DESC,p.TeacherName,p.Revision DESC",Id("@actor",actor)); }
        public static DataRow Read(int actor,int id)
        {
            DataTable rows=Query("SELECT "+DisplayColumns+" FROM dbo.TeacherPayslips p WHERE p.PayslipId=@payslip AND "+ReaderPermission,Id("@actor",actor),Id("@payslip",id));
            if(rows.Rows.Count!=1) throw new ClassRuleException("This payslip is unavailable or belongs to another account.");
            return rows.Rows[0];
        }
        public static DataTable Lessons(int actor,int id)
        {
            return Query(@"SELECT l.CourseTitle,l.ClassName,l.SessionStartLocal,CAST(l.Minutes/60.0 AS DECIMAL(5,2)) AS Hours
                FROM dbo.TeacherPayslipLessons l JOIN dbo.TeacherPayslips p ON p.PayslipId=l.PayslipId
                WHERE p.PayslipId=@payslip AND "+ReaderPermission+" ORDER BY l.SessionStartLocal,l.LogId",Id("@actor",actor),Id("@payslip",id));
        }
    }
}
