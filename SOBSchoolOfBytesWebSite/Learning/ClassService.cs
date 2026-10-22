using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public sealed class ClassRuleException : Exception
    {
        public ClassRuleException(string message) : base(message) { }
    }

    public sealed class ClassSchedule
    {
        public DateTime FirstMonday { get; set; }
        public int DayOne { get; set; }
        public TimeSpan TimeOne { get; set; }
        public int DayTwo { get; set; }
        public TimeSpan TimeTwo { get; set; }
        public DateTime FirstLesson { get { return FirstMonday.AddDays(DayOne - 1).Add(TimeOne); } }
        public DateTime LastLessonEnd { get { return FirstMonday.AddDays(63 + DayTwo - 1).Add(TimeTwo).AddHours(2); } }

        public void Validate()
        {
            if (FirstMonday > DateTime.MaxValue.Date.AddDays(-70) || FirstMonday.DayOfWeek != DayOfWeek.Monday || DayOne < 1 || DayOne > 6
                || DayTwo <= DayOne || DayTwo > 7 || TimeOne < TimeSpan.Zero || TimeTwo < TimeSpan.Zero
                || TimeOne > TimeSpan.FromHours(22) || TimeTwo > TimeSpan.FromHours(22)
                || TimeOne.Seconds != 0 || TimeTwo.Seconds != 0)
                throw new ClassRuleException("Choose a Monday for the first week, two different days in weekly order, and valid start times no later than 22:00.");
            if (FirstMonday < ClassService.SchoolNow.Date || FirstLesson <= ClassService.SchoolNow)
                throw new ClassRuleException("Choose a class schedule that starts in the future.");
        }
        public IEnumerable<DateTime> LessonStarts()
        {
            for (int week = 0; week < 10; week++)
            {
                yield return FirstMonday.AddDays(week * 7 + DayOne - 1).Add(TimeOne);
                yield return FirstMonday.AddDays(week * 7 + DayTwo - 1).Add(TimeTwo);
            }
        }
        public bool Overlaps(ClassSchedule other)
        {
            foreach (DateTime left in LessonStarts())
                foreach (DateTime right in other.LessonStarts())
                    if (left < right.AddHours(2) && right < left.AddHours(2)) return true;
            return false;
        }
        public string WeeklyText
        {
            get { return DayName(DayOne) + " " + TimeText(TimeOne) + "–" + TimeText(TimeOne.Add(TimeSpan.FromHours(2)))
                + "; " + DayName(DayTwo) + " " + TimeText(TimeTwo) + "–" + TimeText(TimeTwo.Add(TimeSpan.FromHours(2))); }
        }
        private static string DayName(int day) { return CultureInfo.InvariantCulture.DateTimeFormat.GetDayName((DayOfWeek)(day % 7)); }
        private static string TimeText(TimeSpan time) { return time.TotalHours == 24 ? "24:00" : time.ToString(@"hh\:mm", CultureInfo.InvariantCulture); }
        public static ClassSchedule FromRow(DataRow row)
        {
            return new ClassSchedule { FirstMonday = ((DateTime)row["FirstMonday"]).Date,
                DayOne = Convert.ToInt32(row["DayOne"]), TimeOne = (TimeSpan)row["TimeOne"],
                DayTwo = Convert.ToInt32(row["DayTwo"]), TimeTwo = (TimeSpan)row["TimeTwo"] };
        }
    }

    public static partial class ClassService
    {
        // School dates and times are always Malaysia time, independent of the server's timezone.
        public static DateTime SchoolNow { get { return DateTime.UtcNow.AddHours(8); } }
        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; } }
        private const string ClassSelect = @"SELECT cl.*,c.Title AS CourseTitle,c.Description,
            u.fname + N' ' + u.lname AS TeacherName,u.accountStatus AS TeacherStatus,c.IsAvailable,
            (SELECT MAX(DATEADD(HOUR,8,h.ChangedAtUtc)) FROM dbo.ClassChangeHistory h WHERE h.ClassId=cl.ClassId) AS LastChangedAt,
            (SELECT COUNT(*) FROM dbo.CourseEnrolments e WHERE e.ClassId=cl.ClassId AND e.IsActive=1) AS Seats
            FROM dbo.CourseClasses cl JOIN dbo.Courses c ON c.CourseId=cl.CourseId JOIN dbo.userTable u ON u.Id=cl.TeacherId ";
        private const string StaffPermission = @" EXISTS(SELECT 1 FROM dbo.userTable actor WHERE actor.Id=@actor
            AND actor.accountStatus=N'Active' AND (actor.usertype=N'President' OR (actor.usertype=N'Teacher'
            AND actor.Id=cl.TeacherId AND EXISTS(SELECT 1 FROM dbo.TeacherCourses tc WHERE tc.TeacherId=actor.Id AND tc.CourseId=cl.CourseId)))) ";

        public static SqlParameter Int(string name, int value) { return new SqlParameter(name, SqlDbType.Int) { Value = value }; }
        private static SqlParameter Text(string name, string value, int size) { return new SqlParameter(name, SqlDbType.NVarChar, size) { Value = value }; }
        private static SqlCommand Command(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(sql, connection, transaction); command.Parameters.AddRange(parameters); return command;
        }
        private static DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = Command(connection, null, sql, parameters))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            { DataTable table = new DataTable(); adapter.Fill(table); return table; }
        }
        private static DataTable Query(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
        {
            using (SqlCommand command = Command(connection, transaction, sql, parameters))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            { DataTable table = new DataTable(); adapter.Fill(table); return table; }
        }
        public static void LockChanges(SqlConnection connection, SqlTransaction transaction)
        {
            // SQL holds this lock until commit/rollback. Concurrent requests cannot take an eleventh place
            // or pass timetable checks using an out-of-date count. This also works across web processes.
            using (SqlCommand command = Command(connection, transaction, @"DECLARE @result INT;
                EXEC @result=sys.sp_getapplock @Resource=N'SOB.ClassEnrolment',@LockMode=N'Exclusive',
                    @LockOwner=N'Transaction',@LockTimeout=10000; SELECT @result;"))
                if ((int)command.ExecuteScalar() < 0) throw new ClassRuleException("Another class change is being saved. Please try again.");
        }
        private static void RequireAccount(SqlConnection connection, SqlTransaction transaction, int id, string role, string status)
        {
            using (SqlCommand command = Command(connection, transaction, @"SELECT COUNT(*) FROM dbo.userTable WITH(HOLDLOCK)
                WHERE Id=@id AND usertype=@role AND accountStatus=@status", Int("@id", id), Text("@role", role, 20), Text("@status", status, 20)))
                if ((int)command.ExecuteScalar() != 1) throw new ClassRuleException("Your account no longer has permission for this action. Please sign in again.");
        }
        private static DataTable Display(DataTable table)
        {
            table.Columns.Add("WeeklySchedule", typeof(string)); table.Columns.Add("CourseDates", typeof(string));
            table.Columns.Add("Places", typeof(string)); table.Columns.Add("EnrolmentState", typeof(string));
            foreach (DataRow row in table.Rows)
            {
                ClassSchedule schedule = ClassSchedule.FromRow(row);
                row["WeeklySchedule"] = schedule.WeeklyText;
                row["CourseDates"] = schedule.FirstLesson.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + " – "
                    + schedule.LastLessonEnd.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
                row["Places"] = row["Seats"].ToString() + "/10";
                row["EnrolmentState"] = schedule.FirstLesson <= SchoolNow ? "Started"
                    : !(bool)row["IsOpen"] ? "Closed" : (int)row["Seats"] >= 10 ? "Full" : "Open";
            }
            return table;
        }
        public static DataTable AvailableTeachers()
        {
            return Query(@"SELECT u.Id,u.fname + N' ' + u.lname + N' — ' + c.Title AS Label
                FROM dbo.userTable u JOIN dbo.TeacherCourses tc ON tc.TeacherId=u.Id
                JOIN dbo.Courses c ON c.CourseId=tc.CourseId WHERE u.usertype=N'Teacher'
                AND u.accountStatus=N'Active' AND c.IsAvailable=1 ORDER BY c.CourseId,u.fname,u.Id");
        }
        public static DataTable AllClasses() { return Display(Query(ClassSelect + "ORDER BY cl.FirstMonday DESC,cl.ClassId DESC")); }
        public static DataTable Catalogue(int studentId)
        {
            DataTable classes = Display(Query(ClassSelect + @"WHERE cl.IsOpen=1 AND c.IsAvailable=1 AND u.usertype=N'Teacher'
                AND u.accountStatus=N'Active' AND EXISTS(SELECT 1 FROM dbo.TeacherCourses tc
                    WHERE tc.TeacherId=cl.TeacherId AND tc.CourseId=cl.CourseId) ORDER BY c.CourseId,cl.FirstMonday,cl.ClassId"));
            for (int i = classes.Rows.Count - 1; i >= 0; i--)
                if (ClassSchedule.FromRow(classes.Rows[i]).FirstLesson <= SchoolNow) classes.Rows.RemoveAt(i);
            classes.Columns.Add("AlreadyEnrolled", typeof(bool));
            classes.Columns.Add("ApplicationPending", typeof(bool));
            DataTable enrolled = Query("SELECT CourseId FROM dbo.CourseEnrolments WHERE StudentId=@student AND IsActive=1", Int("@student", studentId));
            DataTable pending = Query("SELECT CourseId FROM dbo.CourseApplications WHERE StudentId=@student AND Status=N'Pending'", Int("@student", studentId));
            foreach (DataRow row in classes.Rows)
            {
                bool found = false;
                foreach (DataRow course in enrolled.Rows) if ((int)course["CourseId"] == (int)row["CourseId"]) found = true;
                row["AlreadyEnrolled"] = found;
                bool waiting = false;
                foreach (DataRow course in pending.Rows) if ((int)course["CourseId"] == (int)row["CourseId"]) waiting = true;
                row["ApplicationPending"] = waiting;
            }
            return classes;
        }
        public static DataTable MyEnrolments(int studentId)
        {
            return Display(Query(ClassSelect.Replace("SELECT cl.*", "SELECT own.EnrolmentId,CAST(CASE WHEN EXISTS(SELECT 1 FROM dbo.ClassMeetingLinks ml WHERE ml.ClassId=cl.ClassId AND ml.MeetingUrl IS NOT NULL) THEN 1 ELSE 0 END AS BIT) AS HasMeetingLink,cl.*") + @"JOIN dbo.CourseEnrolments own ON own.ClassId=cl.ClassId
                WHERE own.StudentId=@student AND own.IsActive=1 ORDER BY cl.FirstMonday,cl.ClassId", Int("@student", studentId)));
        }
        public static DataTable MyTeachingClasses(int teacherId)
        {
            return Display(Query(ClassSelect + @"WHERE cl.TeacherId=@teacher AND EXISTS(SELECT 1 FROM dbo.TeacherCourses tc
                WHERE tc.TeacherId=cl.TeacherId AND tc.CourseId=cl.CourseId) ORDER BY cl.FirstMonday,cl.ClassId", Int("@teacher", teacherId)));
        }
        public static DataRow StaffClass(int actorId, int classId)
        {
            DataTable table = Display(Query(ClassSelect + "WHERE cl.ClassId=@class AND " + StaffPermission, Int("@class", classId), Int("@actor", actorId)));
            if (table.Rows.Count != 1) throw new ClassRuleException("This class was not found or you do not have access to it.");
            return table.Rows[0];
        }
        public static DataTable Roster(int actorId, int classId)
        {
            return Query(@"SELECT e.EnrolmentId,s.fname + N' ' + s.lname AS StudentName,s.username,s.accountStatus,e.EnrolledAt
                FROM dbo.CourseEnrolments e JOIN dbo.userTable s ON s.Id=e.StudentId
                JOIN dbo.CourseClasses cl ON cl.ClassId=e.ClassId WHERE e.ClassId=@class AND e.IsActive=1 AND "
                + StaffPermission + " ORDER BY s.fname,s.lname,s.Id", Int("@class", classId), Int("@actor", actorId));
        }
        private static void CheckTeacher(SqlConnection connection, SqlTransaction transaction, int teacherId, int courseId)
        {
            using (SqlCommand command = Command(connection, transaction, @"SELECT COUNT(*) FROM dbo.userTable u WITH(HOLDLOCK)
                JOIN dbo.TeacherCourses tc WITH(HOLDLOCK) ON tc.TeacherId=u.Id JOIN dbo.Courses c WITH(HOLDLOCK) ON c.CourseId=tc.CourseId
                WHERE u.Id=@teacher AND u.usertype=N'Teacher' AND u.accountStatus=N'Active' AND c.IsAvailable=1 AND tc.CourseId=@course",
                Int("@teacher", teacherId), Int("@course", courseId)))
                if ((int)command.ExecuteScalar() != 1) throw new ClassRuleException("The teacher or course is no longer available. Please refresh the list.");
        }
        private static void CheckTeacherClashes(SqlConnection connection, SqlTransaction transaction, int teacherId, int exceptClass, ClassSchedule schedule)
        {
            DataTable other = Query(connection, transaction, @"SELECT cl.* FROM dbo.CourseClasses cl WHERE TeacherId=@teacher AND ClassId<>@except
                AND (IsOpen=1 OR EXISTS(SELECT 1 FROM dbo.CourseEnrolments e WHERE e.ClassId=cl.ClassId AND e.IsActive=1))",
                Int("@teacher", teacherId), Int("@except", exceptClass));
            foreach (DataRow row in other.Rows) if (schedule.Overlaps(ClassSchedule.FromRow(row)))
                throw new ClassRuleException("This teacher already has a class at one of these times. Choose a different timetable.");
        }
        public static void EnsureTeacherCourseChangeAllowed(SqlConnection connection, SqlTransaction transaction, int teacherId, int newCourse)
        {
            DataTable classes = Query(connection, transaction, @"SELECT cl.* FROM dbo.CourseClasses cl WHERE TeacherId=@teacher AND CourseId<>@course
                AND (IsOpen=1 OR EXISTS(SELECT 1 FROM dbo.CourseEnrolments e WHERE e.ClassId=cl.ClassId AND e.IsActive=1))",
                Int("@teacher", teacherId), Int("@course", newCourse));
            foreach (DataRow row in classes.Rows) if (ClassSchedule.FromRow(row).LastLessonEnd > SchoolNow)
                throw new ClassRuleException("This teacher has unfinished classes for another course. Close any empty classes or finish the enrolled classes before changing the assigned course.");
        }
        public static void CreateClass(int presidentId, int teacherId, string name, ClassSchedule schedule)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 80) throw new ClassRuleException("Enter a class name of up to 80 characters.");
            schedule.Validate();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    LockChanges(connection, transaction); RequireAccount(connection, transaction, presidentId, "President", "Active");
                    int courseId;
                    using (SqlCommand command = Command(connection, transaction, "SELECT CourseId FROM dbo.TeacherCourses WITH(HOLDLOCK) WHERE TeacherId=@teacher", Int("@teacher", teacherId)))
                    {
                        object value = command.ExecuteScalar();
                        if (value == null) throw new ClassRuleException("Assign this teacher to a course before creating a class.");
                        courseId = (int)value;
                    }
                    CheckTeacher(connection, transaction, teacherId, courseId); CheckTeacherClashes(connection, transaction, teacherId, 0, schedule);
                    using (SqlCommand command = Command(connection, transaction, @"INSERT INTO dbo.CourseClasses
                        (CourseId,TeacherId,ClassName,FirstMonday,DayOne,TimeOne,DayTwo,TimeTwo)
                        VALUES(@course,@teacher,@name,@monday,@day1,@time1,@day2,@time2)", Int("@course", courseId), Int("@teacher", teacherId),
                        Text("@name", name.Trim(), 80), new SqlParameter("@monday", SqlDbType.Date) { Value = schedule.FirstMonday.Date },
                        Int("@day1", schedule.DayOne), new SqlParameter("@time1", SqlDbType.Time) { Value = schedule.TimeOne },
                        Int("@day2", schedule.DayTwo), new SqlParameter("@time2", SqlDbType.Time) { Value = schedule.TimeTwo })) command.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }
        public static void SetOpen(int presidentId, int classId, bool previousOpen)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    LockChanges(connection, transaction); RequireAccount(connection, transaction, presidentId, "President", "Active");
                    DataTable table = Query(connection, transaction, "SELECT * FROM dbo.CourseClasses WHERE ClassId=@class AND IsOpen=@open",
                        Int("@class", classId), new SqlParameter("@open", SqlDbType.Bit) { Value = previousOpen });
                    if (table.Rows.Count != 1) throw new ClassRuleException("This class has already changed. Refresh the list.");
                    DataRow row = table.Rows[0];
                    if (!previousOpen)
                    {
                        ClassSchedule schedule = ClassSchedule.FromRow(row);
                        if (schedule.FirstLesson <= SchoolNow) throw new ClassRuleException("Enrolment cannot reopen after the first lesson has started.");
                        CheckTeacher(connection, transaction, (int)row["TeacherId"], (int)row["CourseId"]);
                        CheckTeacherClashes(connection, transaction, (int)row["TeacherId"], classId, schedule);
                    }
                    using (SqlCommand command = Command(connection, transaction, "UPDATE dbo.CourseClasses SET IsOpen=@open WHERE ClassId=@class",
                        Int("@class", classId), new SqlParameter("@open", SqlDbType.Bit) { Value = !previousOpen })) command.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }
        // The caller must hold LockChanges for the entire transaction. Only an
        // approved application may call EnrolWithinTransaction to allocate a place.
        private static DataRow CheckEnrolment(SqlConnection connection, SqlTransaction transaction, int studentId, int classId)
        {
            RequireAccount(connection, transaction, studentId, "Student", "Approved");
            DataTable table = Query(connection, transaction, "SELECT * FROM dbo.CourseClasses WHERE ClassId=@class", Int("@class", classId));
            if (table.Rows.Count != 1) throw new ClassRuleException("This class is no longer available.");
            DataRow row = table.Rows[0]; ClassSchedule schedule = ClassSchedule.FromRow(row);
            if (!(bool)row["IsOpen"] || schedule.FirstLesson <= SchoolNow) throw new ClassRuleException("Enrolment is closed for this class.");
            CheckTeacher(connection, transaction, (int)row["TeacherId"], (int)row["CourseId"]);
            using (SqlCommand command = Command(connection, transaction, "SELECT COUNT(*) FROM dbo.CourseEnrolments WHERE StudentId=@student AND CourseId=@course AND IsActive=1",
                Int("@student", studentId), Int("@course", (int)row["CourseId"])))
                if ((int)command.ExecuteScalar() != 0) throw new ClassRuleException("You are already enrolled in this course. Withdraw from your current class before choosing another group.");
            using (SqlCommand command = Command(connection, transaction, "SELECT COUNT(*) FROM dbo.CourseEnrolments WHERE ClassId=@class AND IsActive=1", Int("@class", classId)))
                if ((int)command.ExecuteScalar() >= 10) throw new ClassRuleException("This class is full. Please choose another group.");
            DataTable current = Query(connection, transaction, @"SELECT cl.* FROM dbo.CourseEnrolments e JOIN dbo.CourseClasses cl ON cl.ClassId=e.ClassId
                WHERE e.StudentId=@student AND e.IsActive=1", Int("@student", studentId));
            foreach (DataRow other in current.Rows) if (schedule.Overlaps(ClassSchedule.FromRow(other)))
                throw new ClassRuleException("This class clashes with your timetable. Choose another group.");
            return row;
        }
        private static void EnrolWithinTransaction(SqlConnection connection, SqlTransaction transaction, int studentId, int classId)
        {
            DataRow row = CheckEnrolment(connection, transaction, studentId, classId);
            using (SqlCommand command = Command(connection, transaction, @"UPDATE dbo.CourseEnrolments SET IsActive=1,EnrolledAt=SYSUTCDATETIME(),WithdrawnAt=NULL
                WHERE StudentId=@student AND ClassId=@class;
                IF @@ROWCOUNT=0 INSERT INTO dbo.CourseEnrolments(StudentId,ClassId,CourseId) VALUES(@student,@class,@course)",
                Int("@student", studentId), Int("@class", classId), Int("@course", (int)row["CourseId"]))) command.ExecuteNonQuery();
        }
        public static void Withdraw(int actorId, int enrolmentId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    LockChanges(connection, transaction);
                    using (SqlCommand command = Command(connection, transaction, @"UPDATE e SET IsActive=0,WithdrawnAt=SYSUTCDATETIME()
                        FROM dbo.CourseEnrolments e WHERE e.EnrolmentId=@enrolment AND e.IsActive=1
                        AND EXISTS(SELECT 1 FROM dbo.userTable actor WITH(HOLDLOCK) WHERE actor.Id=@actor
                            AND ((actor.usertype=N'President' AND actor.accountStatus=N'Active') OR
                                 (actor.usertype=N'Student' AND actor.accountStatus=N'Approved' AND e.StudentId=actor.Id)))",
                        Int("@enrolment", enrolmentId), Int("@actor", actorId)))
                        if (command.ExecuteNonQuery() != 1) throw new ClassRuleException("This enrolment has already changed or you do not have permission to withdraw it.");
                    transaction.Commit();
                }
            }
        }
    }
}
