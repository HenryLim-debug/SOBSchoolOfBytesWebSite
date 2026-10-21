using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public static class MeetingService
    {
        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; } }
        private const string StaffPermission = @"EXISTS(SELECT 1 FROM dbo.userTable a WITH(HOLDLOCK) WHERE a.Id=@actor AND a.accountStatus=N'Active'
            AND (a.usertype=N'President' OR (a.usertype=N'Teacher' AND cl.TeacherId=a.Id AND EXISTS(
                SELECT 1 FROM dbo.TeacherCourses tc WITH(HOLDLOCK) WHERE tc.TeacherId=a.Id AND tc.CourseId=cl.CourseId))))";
        private const string StudentPermission = @"EXISTS(SELECT 1 FROM dbo.CourseEnrolments e
            JOIN dbo.userTable a ON a.Id=e.StudentId JOIN dbo.Courses co ON co.CourseId=e.CourseId
            JOIN dbo.userTable t ON t.Id=cl.TeacherId
            WHERE e.ClassId=cl.ClassId AND e.StudentId=@actor AND e.IsActive=1
            AND a.usertype=N'Student' AND a.accountStatus=N'Approved' AND co.IsAvailable=1
            AND t.usertype=N'Teacher' AND t.accountStatus=N'Active'
            AND EXISTS(SELECT 1 FROM dbo.TeacherCourses tc WHERE tc.TeacherId=t.Id AND tc.CourseId=cl.CourseId))";
        private static SqlParameter Id(string name,int value) { return new SqlParameter(name,SqlDbType.Int){Value=value}; }
        private static DataTable Query(SqlConnection connection,SqlTransaction transaction,string sql,params SqlParameter[] parameters)
        {
            using(var command=new SqlCommand(sql,connection,transaction)) using(var adapter=new SqlDataAdapter(command))
            { command.Parameters.AddRange(parameters); var table=new DataTable(); adapter.Fill(table); return table; }
        }
        private static DataTable Query(string sql,params SqlParameter[] parameters)
        { using(var connection=new SqlConnection(ConnectionString)) return Query(connection,null,sql,parameters); }
        public static string ValidateUrl(string input)
        {
            string value=(input??"").Trim(); Uri url;
            if(value.Length==0 || value.Length>2048 || value.Any(char.IsWhiteSpace) || value.Any(char.IsControl) || value.Contains("\\")
                || value.IndexOf("%0d",StringComparison.OrdinalIgnoreCase)>=0 || value.IndexOf("%0a",StringComparison.OrdinalIgnoreCase)>=0
                || !Uri.TryCreate(value,UriKind.Absolute,out url) || url.Scheme!=Uri.UriSchemeHttps || !url.IsDefaultPort
                || url.UserInfo.Length!=0 || url.Fragment.Length!=0
                || !(url.Host.Equals("teams.microsoft.com",StringComparison.OrdinalIgnoreCase)
                    || url.Host.Equals("teams.live.com",StringComparison.OrdinalIgnoreCase)
                    || url.Host.Equals("teams.cloud.microsoft",StringComparison.OrdinalIgnoreCase))
                || !((url.AbsolutePath.StartsWith("/meet/",StringComparison.OrdinalIgnoreCase) && url.AbsolutePath.Length>6)
                    || (url.AbsolutePath.StartsWith("/l/meetup-join/",StringComparison.OrdinalIgnoreCase) && url.AbsolutePath.Length>15)))
                throw new ClassRuleException("Paste the full HTTPS Teams meeting join link (teams.microsoft.com, teams.live.com or teams.cloud.microsoft). Copy the meeting link, not the invitation text or a shortened link.");
            if(url.AbsoluteUri.Length>2048) throw new ClassRuleException("This meeting link is too long. Copy the direct Teams meeting join link.");
            return url.AbsoluteUri;
        }
        public static DataTable Classes(int actorId)
        { return Query("SELECT cl.ClassId,c.Title+N' — '+cl.ClassName AS Label FROM dbo.CourseClasses cl JOIN dbo.Courses c ON c.CourseId=cl.CourseId WHERE "+StaffPermission+" ORDER BY cl.FirstMonday DESC,cl.ClassId",Id("@actor",actorId)); }
        public static DataRow Read(int actorId,int classId,bool allowStudent)
        {
            DataTable rows=Query(@"SELECT cl.ClassName,cl.FirstMonday,cl.DayOne,cl.TimeOne,cl.DayTwo,cl.TimeTwo,
                c.Title AS CourseTitle,m.ClassId AS MeetingRecordId,m.MeetingUrl,
                CONVERT(VARBINARY(8),m.RowVersion) AS RowVersion,DATEADD(HOUR,8,m.UpdatedAt) AS UpdatedLocal
                FROM dbo.CourseClasses cl JOIN dbo.Courses c ON c.CourseId=cl.CourseId
                LEFT JOIN dbo.ClassMeetingLinks m ON m.ClassId=cl.ClassId WHERE cl.ClassId=@class AND ("+
                StaffPermission+(allowStudent ? " OR "+StudentPermission : "")+")",Id("@actor",actorId),Id("@class",classId));
            if(rows.Rows.Count!=1) throw new ClassRuleException("This class is unavailable or you do not have access to it.");
            return rows.Rows[0];
        }
        public static void Save(int actorId,int classId,string input,byte[] expectedVersion,bool remove)
        {
            string url=remove ? null : ValidateUrl(input);
            using(var connection=new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(var transaction=connection.BeginTransaction())
                {
                    ClassService.LockChanges(connection,transaction);
                    if(Query(connection,transaction,"SELECT cl.ClassId FROM dbo.CourseClasses cl WITH(HOLDLOCK) WHERE cl.ClassId=@class AND "+StaffPermission,
                        Id("@class",classId),Id("@actor",actorId)).Rows.Count!=1)
                        throw new ClassRuleException("You can only change meeting links for classes you manage. Refresh the page.");
                    DataTable existing=Query(connection,transaction,"SELECT MeetingUrl,RowVersion FROM dbo.ClassMeetingLinks WITH(UPDLOCK,HOLDLOCK) WHERE ClassId=@class",Id("@class",classId));
                    if(existing.Rows.Count==0 ? expectedVersion!=null : expectedVersion==null || !expectedVersion.SequenceEqual((byte[])existing.Rows[0]["RowVersion"]))
                        throw new ClassRuleException("The meeting link changed in another window. Reload it before saving.");
                    if(remove && (existing.Rows.Count==0 || existing.Rows[0].IsNull("MeetingUrl")))
                        throw new ClassRuleException("This class has no saved meeting link to remove.");
                    // Keep an empty row after removal so stale editor tabs cannot restore a removed link.
                    using(var command=new SqlCommand(@"UPDATE dbo.ClassMeetingLinks SET MeetingUrl=@url,UpdatedBy=@actor,UpdatedAt=SYSUTCDATETIME() WHERE ClassId=@class;
                        IF @@ROWCOUNT=0 INSERT dbo.ClassMeetingLinks(ClassId,MeetingUrl,UpdatedBy) VALUES(@class,@url,@actor);",connection,transaction))
                    {
                        command.Parameters.AddRange(new[]{Id("@class",classId),Id("@actor",actorId),new SqlParameter("@url",SqlDbType.NVarChar,2048){Value=(object)url??DBNull.Value}});
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }
    }
}
