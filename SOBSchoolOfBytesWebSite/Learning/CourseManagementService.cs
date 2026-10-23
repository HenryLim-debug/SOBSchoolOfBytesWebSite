using System;
using System.Data;
using System.Data.SqlClient;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public static partial class ClassService
    {
        public static DataTable PublicCourses()
        { return Query("SELECT CourseId,Title,Description,LearningOutcomes,SampleLesson,SampleActivity FROM dbo.Courses WHERE IsAvailable=1 ORDER BY CourseId"); }
        public static DataTable ManageCourses(int actor)
        {
            return Query(@"SELECT c.*,CONVERT(VARCHAR(16),CONVERT(VARBINARY(8),c.RowVersion),2) AS VersionKey FROM dbo.Courses c
                WHERE EXISTS(SELECT 1 FROM dbo.userTable WHERE Id=@actor AND usertype=N'President' AND accountStatus=N'Active') ORDER BY c.CourseId",Int("@actor",actor));
        }
        public static byte[] ParseVersion(string value)
        {
            if(string.IsNullOrEmpty(value) || value.Length!=16) throw new ClassRuleException("Reload this page before saving.");
            try { var result=new byte[8];for(int i=0;i<8;i++)result[i]=Convert.ToByte(value.Substring(i*2,2),16);return result; }
            catch(FormatException){throw new ClassRuleException("Reload this page before saving.");}
        }
        private static string CheckedText(string value,int max,string label,bool required)
        {
            value=(value??"").Trim();
            if(value.Length>max || (required && value.Length==0))throw new ClassRuleException(label+" must contain "+(required?"1 to ":"no more than ")+max+" characters.");
            return value;
        }
        public static int SaveCourse(int actor,int? id,string version,string title,string description,string outcomes,string sample,string activity,bool available)
        {
            title=CheckedText(title,100,"Course title",true);description=CheckedText(description,1000,"Description",true);
            outcomes=CheckedText(outcomes,2000,"Learning outcomes",true);sample=CheckedText(sample,4000,"Sample lesson",false);
            activity=CheckedText(activity,2000,"Sample activity",false);
            using(var connection=new SqlConnection(ConnectionString))
            {
                connection.Open();using(var transaction=connection.BeginTransaction())
                {
                    LockChanges(connection,transaction);RequireAccount(connection,transaction,actor,"President","Active");
                    using(var command=Command(connection,transaction,id.HasValue ? @"UPDATE dbo.Courses SET Title=@title,Description=@description,LearningOutcomes=@outcomes,
                        SampleLesson=@sample,SampleActivity=@activity,IsAvailable=@available WHERE CourseId=@id AND RowVersion=@version; SELECT @@ROWCOUNT;" :
                        @"INSERT dbo.Courses(Title,Description,LearningOutcomes,SampleLesson,SampleActivity,IsAvailable)
                        VALUES(@title,@description,@outcomes,@sample,@activity,@available); SELECT CONVERT(INT,SCOPE_IDENTITY());",
                        Text("@title",title,100),Text("@description",description,1000),Text("@outcomes",outcomes,2000),Text("@sample",sample,4000),Text("@activity",activity,2000),new SqlParameter("@available",SqlDbType.Bit){Value=available}))
                    {
                        if(id.HasValue){command.Parameters.Add(Int("@id",id.Value));command.Parameters.Add("@version",SqlDbType.Binary,8).Value=ParseVersion(version);}
                        int result=(int)command.ExecuteScalar();
                        if(id.HasValue && result!=1)throw new ClassRuleException("This course changed in another window. Reload it before saving.");
                        if(!id.HasValue)id=result;
                    }
                    using(var command=Command(connection,transaction,@"INSERT dbo.CourseChapters(CourseId,ChapterNumber,Title)
                        SELECT @id,n,N'Chapter '+CONVERT(NVARCHAR(2),n) FROM (VALUES(1),(2),(3),(4),(5),(6),(7),(8),(9),(10)) numbers(n)
                        WHERE NOT EXISTS(SELECT 1 FROM dbo.CourseChapters WHERE CourseId=@id AND ChapterNumber=n)",Int("@id",id.Value)))command.ExecuteNonQuery();
                    DataTable assignedTeachers=Query(connection,transaction,"SELECT TeacherId FROM dbo.TeacherCourses WHERE CourseId=@course",Int("@course",id.Value));
                    foreach(DataRow teacher in assignedTeachers.Rows)
                        MaterialService.EnsureTeacherChapters(connection,transaction,(int)teacher["TeacherId"]);
                    transaction.Commit();return id.Value;
                }
            }
        }
        public static DataTable ClassEditTeachers(int courseId)
        {return Query(@"SELECT u.Id,u.fname+N' '+u.lname+N' ('+u.staffId+N')' AS Label FROM dbo.userTable u JOIN dbo.TeacherCourses tc ON tc.TeacherId=u.Id
            WHERE tc.CourseId=@course AND u.usertype=N'Teacher' AND u.accountStatus=N'Active' ORDER BY u.fname,u.Id",Int("@course",courseId));}
        public static DataTable ClassHistory(int actor,int classId)
        {return Query(@"SELECT DATEADD(HOUR,8,h.ChangedAtUtc) AS ChangedAt,h.PreviousDetails,h.NewDetails,h.Reason FROM dbo.ClassChangeHistory h
            JOIN dbo.CourseClasses cl ON cl.ClassId=h.ClassId WHERE h.ClassId=@class AND "+StaffPermission+" ORDER BY h.ChangeId DESC",Int("@actor",actor),Int("@class",classId));}
        private static string ScheduleDescription(string name,int teacher,ClassSchedule schedule)
        {return name+"; teacher ID "+teacher+"; week starts "+schedule.FirstMonday.ToString("yyyy-MM-dd")+"; "+schedule.WeeklyText;}
        public static void EditClass(int actor,int classId,string version,int teacher,string name,ClassSchedule schedule,string reason)
        {
            name=CheckedText(name,80,"Class name",true);reason=CheckedText(reason,500,"Reason for change",true);schedule.Validate();
            using(var connection=new SqlConnection(ConnectionString))
            {
                connection.Open();using(var transaction=connection.BeginTransaction())
                {
                    LockChanges(connection,transaction);RequireAccount(connection,transaction,actor,"President","Active");
                    var rows=Query(connection,transaction,"SELECT * FROM dbo.CourseClasses WHERE ClassId=@id AND RowVersion=@version",Int("@id",classId),new SqlParameter("@version",SqlDbType.Binary,8){Value=ParseVersion(version)});
                    if(rows.Rows.Count!=1)throw new ClassRuleException("This class changed. Reload it before saving.");
                    DataRow old=rows.Rows[0];var original=ClassSchedule.FromRow(old);
                    if(original.FirstLesson<=SchoolNow)throw new ClassRuleException("This class has already started. Its teacher and timetable are kept to preserve lesson history. Create a new class for a new schedule.");
                    using(var command=Command(connection,transaction,@"SELECT (SELECT COUNT(*) FROM dbo.TeachingHourLogs WHERE ClassId=@id)+(SELECT COUNT(*) FROM dbo.AttendanceRecords WHERE ClassId=@id)",Int("@id",classId)))
                        if((int)command.ExecuteScalar()!=0)throw new ClassRuleException("This class has teaching hours or attendance records and cannot be rescheduled.");
                    CheckTeacher(connection,transaction,teacher,(int)old["CourseId"]);CheckTeacherClashes(connection,transaction,teacher,classId,schedule);
                    var other=Query(connection,transaction,@"SELECT DISTINCT cl.* FROM dbo.CourseEnrolments own JOIN dbo.CourseEnrolments e ON e.StudentId=own.StudentId
                        JOIN dbo.CourseClasses cl ON cl.ClassId=e.ClassId WHERE own.ClassId=@id AND own.IsActive=1 AND e.IsActive=1 AND e.ClassId<>@id",Int("@id",classId));
                    foreach(DataRow row in other.Rows)if(schedule.Overlaps(ClassSchedule.FromRow(row)))throw new ClassRuleException("The new timetable clashes with another course for an enrolled student. Choose different times.");
                    string before=ScheduleDescription(old["ClassName"].ToString(),(int)old["TeacherId"],original);
                    string after=ScheduleDescription(name,teacher,schedule);
                    if(before==after)throw new ClassRuleException("No class details have changed.");
                    using(var command=Command(connection,transaction,@"UPDATE dbo.CourseClasses SET TeacherId=@teacher,ClassName=@name,FirstMonday=@monday,
                        DayOne=@d1,TimeOne=@t1,DayTwo=@d2,TimeTwo=@t2 WHERE ClassId=@id",Int("@id",classId),Int("@teacher",teacher),Text("@name",name,80),
                        new SqlParameter("@monday",SqlDbType.Date){Value=schedule.FirstMonday.Date},Int("@d1",schedule.DayOne),Int("@d2",schedule.DayTwo),
                        new SqlParameter("@t1",SqlDbType.Time){Value=schedule.TimeOne},new SqlParameter("@t2",SqlDbType.Time){Value=schedule.TimeTwo}))command.ExecuteNonQuery();
                    using(var command=Command(connection,transaction,@"INSERT dbo.ClassChangeHistory(ClassId,ChangedBy,PreviousDetails,NewDetails,Reason) VALUES(@id,@actor,@before,@after,@reason)",
                        Int("@id",classId),Int("@actor",actor),Text("@before",before,1500),Text("@after",after,1500),Text("@reason",reason,500)))command.ExecuteNonQuery();
                    // A former teacher's meeting must not be handed to the replacement teacher.
                    if(teacher!=(int)old["TeacherId"])
                        using(var command=Command(connection,transaction,"UPDATE dbo.ClassMeetingLinks SET MeetingUrl=NULL,UpdatedBy=@actor,UpdatedAt=SYSUTCDATETIME() WHERE ClassId=@id",Int("@id",classId),Int("@actor",actor)))command.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }
    }
}
