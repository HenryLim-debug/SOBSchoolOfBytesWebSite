using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Security;
using SOBSchoolOfBytesWebSite.Learning;

namespace SOBSchoolOfBytesWebSite
{
    public partial class TeacherDashboard : ProtectedPage
    {
        protected override string RequiredRole { get { return "Teacher"; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            TeacherName.Text = CurrentAccount.FirstName;
            TeacherUsername.Text = CurrentAccount.Username;
            TeacherStaffId.Text = CurrentAccount.StaffId;
            LoadAssignedCourse();
            if (!IsPostBack)
            {
                try { TeachingClasses.DataSource = ClassService.MyTeachingClasses(CurrentAccount.Id); TeachingClasses.DataBind(); }
                catch (SqlException exception) { Trace.Warn("TeacherDashboard", "Could not load teaching classes.", exception); ClassesErrorPanel.Visible = true; }
            }
        }
        private void LoadAssignedCourse()
        {
            // Use the authenticated teacher ID only; a URL parameter cannot select another teacher's course.
            try
            {
                using (SqlConnection connection = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                using (SqlCommand command = new SqlCommand(@"SELECT c.Title,c.Description,c.LearningOutcomes,c.IsAvailable
                    FROM dbo.TeacherCourses tc JOIN dbo.Courses c ON c.CourseId=tc.CourseId
                    JOIN dbo.userTable u ON u.Id=tc.TeacherId
                    WHERE tc.TeacherId=@teacher AND u.usertype=N'Teacher' AND u.accountStatus=N'Active'", connection))
                {
                    command.Parameters.Add("@teacher", SqlDbType.Int).Value = CurrentAccount.Id;
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        bool assigned = reader.Read();
                        AssignedCoursePanel.Visible = assigned; NoCoursePanel.Visible = !assigned;
                        if (assigned)
                        {
                            CourseTitle.Text = reader["Title"].ToString(); CourseDescription.Text = reader["Description"].ToString();
                            CourseOutcomes.Text = reader["LearningOutcomes"].ToString();
                            CourseStatus.Text = (bool)reader["IsAvailable"] ? "Available" : "Currently unavailable";
                        }
                    }
                }
            }
            catch (SqlException exception)
            {
                Trace.Warn("TeacherDashboard", "Could not load the assigned course.", exception);
                AssignedCoursePanel.Visible = false; NoCoursePanel.Visible = false; CourseErrorPanel.Visible = true;
            }
        }
        protected void SignOutButton_Click(object sender, EventArgs e) { AccountAccess.SignOut(Context, "~/StaffLogin"); }
    }
}
