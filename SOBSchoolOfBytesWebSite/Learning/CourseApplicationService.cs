using System;
using System.Data;
using System.Data.SqlClient;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public static partial class ClassService
    {
        // A teacher may review only requests for a group they teach and a course
        // currently assigned to them. The same rule protects both listing and saving.
        private const string ApplicationTeacherPermission = @"cl.TeacherId=@teacher AND EXISTS(
            SELECT 1 FROM dbo.TeacherCourses tc JOIN dbo.userTable reviewer ON reviewer.Id=tc.TeacherId
            WHERE tc.TeacherId=@teacher AND tc.CourseId=cl.CourseId
                AND reviewer.usertype=N'Teacher' AND reviewer.accountStatus=N'Active')";

        private static DataTable Applications(string filter, params SqlParameter[] parameters)
        {
            string select = ClassSelect.Replace("SELECT cl.*", @"SELECT a.ApplicationId,a.StudentId,a.Status,
                DATEADD(HOUR,8,a.RequestedAt) AS AppliedAtLocal,
                s.fname + N' ' + s.lname AS StudentName,s.username,s.accountStatus AS StudentStatus,
                assigned.ClassName AS AssignedGroup,
                teacher.fname + N' ' + teacher.lname AS AssignedTeacher,cl.*");
            return Display(Query(select + @"JOIN dbo.CourseApplications a ON a.PreferredClassId=cl.ClassId
                JOIN dbo.userTable s ON s.Id=a.StudentId
                LEFT JOIN dbo.CourseClasses assigned ON assigned.ClassId=a.AssignedClassId
                LEFT JOIN dbo.userTable teacher ON teacher.Id=assigned.TeacherId
                WHERE " + filter, parameters));
        }

        public static DataTable MyApplications(int studentId)
        {
            return Applications("a.StudentId=@student ORDER BY a.RequestedAt DESC,a.ApplicationId DESC", Int("@student", studentId));
        }

        public static DataTable PendingApplications(int teacherId)
        {
            return Applications("a.Status=N'Pending' AND " + ApplicationTeacherPermission
                + " ORDER BY a.RequestedAt,a.ApplicationId", Int("@teacher", teacherId));
        }

        public static void ApplyForCourse(int studentId, int preferredClassId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    LockChanges(connection, transaction);
                    DataRow preferred = CheckEnrolment(connection, transaction, studentId, preferredClassId);
                    int courseId = (int)preferred["CourseId"];
                    using (SqlCommand command = Command(connection, transaction, @"SELECT COUNT(*) FROM dbo.CourseApplications
                        WHERE StudentId=@student AND CourseId=@course AND Status=N'Pending'",
                        Int("@student", studentId), Int("@course", courseId)))
                        if ((int)command.ExecuteScalar() != 0)
                            throw new ClassRuleException("You already have a pending application for this course. Check your Student Dashboard.");
                    using (SqlCommand command = Command(connection, transaction, @"INSERT INTO dbo.CourseApplications
                        (StudentId,CourseId,PreferredClassId) VALUES(@student,@course,@class)",
                        Int("@student", studentId), Int("@course", courseId), Int("@class", preferredClassId)))
                        command.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        public static void CancelApplication(int studentId, int applicationId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    LockChanges(connection, transaction);
                    RequireAccount(connection, transaction, studentId, "Student", "Approved");
                    using (SqlCommand command = Command(connection, transaction, @"UPDATE dbo.CourseApplications
                        SET Status=N'Cancelled',CancelledAt=SYSUTCDATETIME()
                        WHERE ApplicationId=@application AND StudentId=@student AND Status=N'Pending'",
                        Int("@application", applicationId), Int("@student", studentId)))
                        if (command.ExecuteNonQuery() != 1)
                            throw new ClassRuleException("This application has already changed or does not belong to you. Refresh your dashboard.");
                    transaction.Commit();
                }
            }
        }

        public static string ReviewApplication(int teacherId, int applicationId, bool approve)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    LockChanges(connection, transaction);
                    RequireAccount(connection, transaction, teacherId, "Teacher", "Active");
                    DataTable applications = Query(connection, transaction, @"SELECT a.StudentId,a.PreferredClassId,cl.ClassName
                        FROM dbo.CourseApplications a WITH(UPDLOCK,HOLDLOCK)
                        JOIN dbo.CourseClasses cl WITH(UPDLOCK,HOLDLOCK) ON cl.ClassId=a.PreferredClassId
                        WHERE a.ApplicationId=@application AND a.Status=N'Pending' AND " + ApplicationTeacherPermission,
                        Int("@application", applicationId), Int("@teacher", teacherId));
                    if (applications.Rows.Count != 1)
                        throw new ClassRuleException("This application is no longer pending or is not for a class you teach. Refresh the list.");

                    DataRow application = applications.Rows[0];
                    int? assignedClass = null;
                    if (approve)
                    {
                        // Keep the student's requested class and teacher. Never allocate a
                        // place in another teacher's group when this group is full or closed.
                        int classId = (int)application["PreferredClassId"];
                        EnrolWithinTransaction(connection, transaction, (int)application["StudentId"], classId);
                        assignedClass = classId;
                    }

                    using (SqlCommand command = Command(connection, transaction, @"UPDATE dbo.CourseApplications
                        SET Status=@status,AssignedClassId=@class,ReviewedAt=SYSUTCDATETIME(),ReviewedBy=@teacher
                        WHERE ApplicationId=@application AND Status=N'Pending'",
                        Text("@status", approve ? "Approved" : "Rejected", 20),
                        new SqlParameter("@class", SqlDbType.Int) { Value = assignedClass.HasValue ? (object)assignedClass.Value : DBNull.Value },
                        Int("@teacher", teacherId), Int("@application", applicationId)))
                        if (command.ExecuteNonQuery() != 1)
                            throw new ClassRuleException("This application has already changed. Refresh the list.");
                    // The teacher decision and seat allocation commit together or both roll back.
                    transaction.Commit();
                    return approve ? "Application approved. Student assigned to " + application["ClassName"] + "." : "Application rejected.";
                }
            }
        }
    }
}
