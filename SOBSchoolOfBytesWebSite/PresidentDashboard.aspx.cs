using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class PresidentDashboard : ProtectedPage
    {
        protected override string RequiredRole { get { return "President"; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            PresidentName.Text = CurrentAccount.FirstName;
            if (!IsPostBack) LoadOverview();
        }
        private void LoadOverview()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(@"SELECT
                        COUNT(CASE WHEN usertype=N'Student' THEN 1 END) AS Students,
                        COUNT(CASE WHEN usertype=N'Student' AND accountStatus=N'Pending' THEN 1 END) AS Pending,
                        COUNT(CASE WHEN usertype=N'Student' AND accountStatus=N'Approved' THEN 1 END) AS Approved,
                        COUNT(CASE WHEN usertype=N'Teacher' AND accountStatus=N'Active' THEN 1 END) AS Teachers
                        FROM dbo.userTable", connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        StudentCount.Text = reader["Students"].ToString(); PendingCount.Text = reader["Pending"].ToString();
                        ApprovedCount.Text = reader["Approved"].ToString(); TeacherCount.Text = reader["Teachers"].ToString();
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(@"SELECT Id, fname + N' ' + lname AS FullName,
                        username, email, createdAt FROM dbo.userTable
                        WHERE usertype=N'Student' AND accountStatus=N'Pending' ORDER BY createdAt, Id", connection))
                    {
                        DataTable students = new DataTable();
                        adapter.Fill(students);
                        PendingStudents.DataSource = students;
                        PendingStudents.DataBind();
                    }
                }
            }
            catch (SqlException exception)
            {
                Trace.Warn("PresidentDashboard", "Could not load student registrations.", exception);
                PendingStudents.Visible = false;
                ShowFeedback("The student list could not be loaded. Please try Refresh List.", false);
            }
        }
        protected void PendingStudents_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            string argument = e.Row.RowIndex.ToString(CultureInfo.InvariantCulture);
            ((Button)e.Row.FindControl("ApproveButton")).CommandArgument = argument;
            ((Button)e.Row.FindControl("RejectButton")).CommandArgument = argument;
        }
        protected void PendingStudents_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ApproveStudent" && e.CommandName != "RejectStudent") return;
            int rowIndex;
            if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out rowIndex)
                || rowIndex < 0 || rowIndex >= PendingStudents.DataKeys.Count)
            { ShowFeedback("Please refresh the list and try again.", false); return; }
            // DataKeys come from protected ViewState, rather than a student ID supplied by the browser.
            int studentId = (int)PendingStudents.DataKeys[rowIndex].Value;
            string status = e.CommandName == "ApproveStudent" ? "Approved" : "Rejected";
            try
            {
                using (SqlConnection connection = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                using (SqlCommand command = new SqlCommand(@"UPDATE dbo.userTable SET accountStatus=@status
                    WHERE Id=@studentId AND usertype=N'Student' AND accountStatus=N'Pending'
                    AND EXISTS (SELECT 1 FROM dbo.userTable WHERE Id=@presidentId
                        AND usertype=N'President' AND accountStatus=N'Active')", connection))
                {
                    command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = status;
                    command.Parameters.Add("@studentId", SqlDbType.Int).Value = studentId;
                    command.Parameters.Add("@presidentId", SqlDbType.Int).Value = CurrentAccount.Id;
                    connection.Open();
                    int changed = command.ExecuteNonQuery();
                    ShowFeedback(changed == 1 ? (status == "Approved" ? "Student approved. The student can now sign in."
                        : "Student registration rejected. The student cannot sign in.")
                        : "This registration has already been reviewed or is no longer available.", changed == 1);
                }
                LoadOverview();
            }
            catch (SqlException exception)
            {
                Trace.Warn("PresidentDashboard", "Could not review a student registration.", exception);
                ShowFeedback("The decision could not be saved. Please refresh the list and try again.", false);
            }
        }
        protected void RefreshButton_Click(object sender, EventArgs e)
        {
            FeedbackPanel.Visible = false; PendingStudents.Visible = true; LoadOverview();
        }
        protected void SignOutButton_Click(object sender, EventArgs e) { AccountAccess.SignOut(Context, "~/StaffLogin"); }
        private void ShowFeedback(string message, bool success)
        {
            FeedbackPanel.Visible = true;
            FeedbackPanel.CssClass = success ? "alert alert-success" : "alert alert-warning";
            FeedbackMessage.Text = message;
        }
    }
}
