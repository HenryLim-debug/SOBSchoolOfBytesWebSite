using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class StudentManagement : ProtectedPage
    {
        protected override string RequiredRole { get { return "President"; } }
        private int? EditingStudentId
        {
            get { return ViewState["EditingStudentId"] as int?; }
            set { ViewState["EditingStudentId"] = value; }
        }
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            EditorPanel.Visible = EditingStudentId.HasValue;
            if (!IsPostBack) LoadStudents();
        }
        protected override void OnPreRender(EventArgs e)
        {
            EditorPanel.Visible = EditingStudentId.HasValue;
            base.OnPreRender(e);
        }
        private void LoadStudents()
        {
            string search = SearchText.Text.Trim();
            string status = StatusFilter.SelectedValue;
            if (search.Length > 100 || !IsStudentStatus(status, true))
            { ShowFeedback("Use a search of up to 100 characters and choose a valid status.", false); return; }
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(@"SELECT Id,fname + N' ' + lname AS FullName,username,email,accountStatus
                    FROM dbo.userTable WHERE usertype=N'Student' AND (@status=N'' OR accountStatus=@status)
                    AND (@search=N'' OR fname LIKE @pattern OR lname LIKE @pattern
                        OR fname + N' ' + lname LIKE @pattern OR username LIKE @pattern)
                    ORDER BY fname,lname,Id", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = status;
                    command.Parameters.Add("@search", SqlDbType.NVarChar, 100).Value = search;
                    // Treat SQL wildcard characters as literal search text.
                    string pattern = search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
                    command.Parameters.Add("@pattern", SqlDbType.NVarChar, 302).Value = "%" + pattern + "%";
                    DataTable students = new DataTable(); adapter.Fill(students);
                    int lastPage = students.Rows.Count == 0 ? 0 : (students.Rows.Count - 1) / Students.PageSize;
                    Students.PageIndex = Math.Min(Students.PageIndex, lastPage);
                    Students.DataSource = students; Students.DataBind(); Students.Visible = true;
                    ResultCount.Text = students.Rows.Count.ToString(CultureInfo.InvariantCulture)
                        + (students.Rows.Count == 1 ? " student found." : " students found.");
                }
            }
            catch (SqlException exception)
            {
                Trace.Warn("StudentManagement", "Could not load students.", exception);
                Students.Visible = false; ResultCount.Text = "";
                ShowFeedback("The student list could not be loaded. Please try Refresh List.", false);
            }
        }
        private static bool IsStudentStatus(string status, bool allowEmpty)
        {
            return (allowEmpty && status == "") || status == "Pending" || status == "Approved"
                || status == "Rejected" || status == "Suspended" || status == "Inactive";
        }
        protected void Students_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            string argument = e.Row.RowIndex.ToString(CultureInfo.InvariantCulture);
            string status = System.Web.UI.DataBinder.Eval(e.Row.DataItem, "accountStatus").ToString();
            ((Button)e.Row.FindControl("EditStudentButton")).CommandArgument = argument;
            Button primary = (Button)e.Row.FindControl("PrimaryActionButton");
            Button secondary = (Button)e.Row.FindControl("SecondaryActionButton");
            primary.CommandArgument = argument; secondary.CommandArgument = argument;
            secondary.Visible = false;
            switch (status)
            {
                case "Pending":
                    primary.Text = "Approve"; primary.CommandName = "ApproveStudent";
                    secondary.Text = "Reject"; secondary.CommandName = "RejectStudent"; secondary.Visible = true;
                    break;
                case "Approved":
                    primary.Text = "Suspend"; primary.CommandName = "SuspendStudent";
                    secondary.Text = "Deactivate"; secondary.CommandName = "DeactivateStudent"; secondary.Visible = true;
                    break;
                case "Suspended":
                    primary.Text = "Restore Access"; primary.CommandName = "RestoreStudent";
                    secondary.Text = "Deactivate"; secondary.CommandName = "DeactivateStudent"; secondary.Visible = true;
                    break;
                case "Inactive": primary.Text = "Reactivate"; primary.CommandName = "ReactivateStudent"; break;
                case "Rejected": primary.Text = "Review Again"; primary.CommandName = "ReviewStudent"; break;
                default: primary.Visible = false; break;
            }
        }
        protected void Students_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Page") return;
            int rowIndex;
            if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out rowIndex)
                || rowIndex < 0 || rowIndex >= Students.DataKeys.Count)
            { ShowFeedback("Refresh the student list and try again.", false); return; }
            int studentId = (int)Students.DataKeys[rowIndex].Values["Id"];
            string previousStatus = Students.DataKeys[rowIndex].Values["accountStatus"].ToString();
            errMsg.Visible = false; FeedbackPanel.Visible = false;
            try
            {
                if (e.CommandName == "EditStudent") LoadStudentForEditing(studentId);
                else
                {
                    string nextStatus = NextStatus(e.CommandName, previousStatus);
                    if (nextStatus != null) ChangeStatus(studentId, previousStatus, nextStatus);
                }
            }
            catch (SqlException exception)
            {
                Trace.Warn("StudentManagement", "Could not manage a student.", exception);
                ShowFeedback("The student account could not be updated. Please refresh and try again.", false);
            }
        }
        private static string NextStatus(string action, string previous)
        {
            switch (action)
            {
                case "ApproveStudent": return previous == "Pending" ? "Approved" : null;
                case "RejectStudent": return previous == "Pending" ? "Rejected" : null;
                case "SuspendStudent": return previous == "Approved" ? "Suspended" : null;
                case "RestoreStudent": return previous == "Suspended" ? "Approved" : null;
                case "ReactivateStudent": return previous == "Inactive" ? "Approved" : null;
                case "ReviewStudent": return previous == "Rejected" ? "Pending" : null;
                case "DeactivateStudent": return previous == "Approved" || previous == "Suspended" ? "Inactive" : null;
                default: return null;
            }
        }
        private void ChangeStatus(int studentId, string previousStatus, string nextStatus)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(@"UPDATE dbo.userTable SET accountStatus=@status
                WHERE Id=@id AND usertype=N'Student' AND accountStatus=@previous
                AND EXISTS(SELECT 1 FROM dbo.userTable WHERE Id=@president AND usertype=N'President' AND accountStatus=N'Active')", connection))
            {
                command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = nextStatus;
                command.Parameters.Add("@previous", SqlDbType.NVarChar, 20).Value = previousStatus;
                command.Parameters.Add("@id", SqlDbType.Int).Value = studentId;
                command.Parameters.Add("@president", SqlDbType.Int).Value = CurrentAccount.Id;
                connection.Open();
                int changed = command.ExecuteNonQuery();
                if (changed == 1)
                {
                    switch (nextStatus)
                    {
                        case "Approved": ShowFeedback("Student access approved. The student can sign in.", true); break;
                        case "Pending": ShowFeedback("Registration returned to Pending. Review it before approving access.", true); break;
                        case "Rejected": ShowFeedback("Student registration rejected. The student cannot sign in.", true); break;
                        case "Suspended": ShowFeedback("Student suspended. Sign-in access has stopped.", true); break;
                        case "Inactive": ShowFeedback("Student deactivated. Sign-in access has stopped and records have been kept.", true); break;
                    }
                }
                else ShowFeedback("This account has already changed or is no longer available. The list has been refreshed.", false);
            }
            LoadStudents();
        }
        private void LoadStudentForEditing(int studentId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(@"SELECT fname,lname,username,email,gender,country
                FROM dbo.userTable WHERE Id=@id AND usertype=N'Student'", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = studentId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) { ShowFeedback("This student account is no longer available.", false); return; }
                    EditingStudentId = studentId;
                    fname.Text = reader["fname"].ToString(); lname.Text = reader["lname"].ToString();
                    username.Text = reader["username"].ToString(); email.Text = reader["email"].ToString();
                    country.Text = reader["country"].ToString();
                    string savedGender = reader["gender"].ToString().Trim();
                    gender.SelectedValue = gender.Items.FindByValue(savedGender) != null ? savedGender : "";
                }
            }
            EditorPanel.Visible = EditingStudentId.HasValue;
        }
        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!EditingStudentId.HasValue) return;
            errMsg.Visible = false; FeedbackPanel.Visible = false;
            fname.Text = fname.Text.Trim(); lname.Text = lname.Text.Trim(); username.Text = username.Text.Trim();
            email.Text = email.Text.Trim(); country.Text = country.Text.Trim();
            Page.Validate("StudentDetails");
            if (!Page.IsValid) return;
            if (string.IsNullOrWhiteSpace(fname.Text) || string.IsNullOrWhiteSpace(lname.Text)
                || fname.Text.Length > 50 || lname.Text.Length > 50 || country.Text.Length > 50
                || !Regex.IsMatch(username.Text, @"\A[A-Za-z0-9_]{3,30}\z"))
            { ShowError("Check the name and country lengths. Usernames need 3–30 letters, numbers or underscores."); return; }
            if (email.Text.Length > 254 || (email.Text.Length > 0 && !Regex.IsMatch(email.Text, @"\A[^@\s]+@[^@\s]+\.[^@\s]+\z")))
            { ShowError("Enter a valid email address, or leave email blank."); return; }
            if (gender.SelectedValue != "" && gender.SelectedValue != "Female" && gender.SelectedValue != "Male" && gender.SelectedValue != "Other")
            { ShowError("Choose a valid gender option."); return; }
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(@"UPDATE dbo.userTable SET fname=@fname,lname=@lname,
                    username=@username,email=@email,gender=@gender,country=@country
                    WHERE Id=@id AND usertype=N'Student'
                    AND EXISTS(SELECT 1 FROM dbo.userTable WHERE Id=@president AND usertype=N'President' AND accountStatus=N'Active')", connection))
                {
                    // The selected ID is held in protected ViewState. Role, status and password are never edited here.
                    command.Parameters.Add("@id", SqlDbType.Int).Value = EditingStudentId.Value;
                    command.Parameters.Add("@president", SqlDbType.Int).Value = CurrentAccount.Id;
                    command.Parameters.Add("@fname", SqlDbType.NVarChar, 50).Value = fname.Text;
                    command.Parameters.Add("@lname", SqlDbType.NVarChar, 50).Value = lname.Text;
                    command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username.Text;
                    command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = OptionalValue(email.Text);
                    command.Parameters.Add("@gender", SqlDbType.NChar, 10).Value = OptionalValue(gender.SelectedValue);
                    command.Parameters.Add("@country", SqlDbType.NVarChar, 50).Value = OptionalValue(country.Text);
                    connection.Open();
                    int changed = command.ExecuteNonQuery();
                    if (changed == 1)
                    {
                        CloseEditor(); LoadStudents(); ShowFeedback("Student details saved.", true);
                    }
                    else ShowError("This account is no longer available or your president access has changed. Refresh and try again.");
                }
            }
            catch (SqlException exception)
            {
                if (exception.Number == 2601 || exception.Number == 2627) ShowError("This username is already taken. Choose another.");
                else
                {
                    Trace.Warn("StudentManagement", "Could not save student details.", exception);
                    ShowError("The student details could not be saved right now. Please try again.");
                }
            }
        }
        private static object OptionalValue(string value) { return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value; }
        private void CloseEditor()
        {
            EditingStudentId = null; EditorPanel.Visible = false;
            fname.Text = ""; lname.Text = ""; username.Text = ""; email.Text = ""; country.Text = ""; gender.SelectedIndex = 0;
            errMsg.Visible = false;
        }
        protected void CancelButton_Click(object sender, EventArgs e) { CloseEditor(); FeedbackPanel.Visible = false; }
        protected void SearchButton_Click(object sender, EventArgs e)
        { CloseEditor(); FeedbackPanel.Visible = false; Students.PageIndex = 0; SearchText.Text = SearchText.Text.Trim(); LoadStudents(); }
        protected void ClearSearchButton_Click(object sender, EventArgs e)
        { CloseEditor(); FeedbackPanel.Visible = false; Students.PageIndex = 0; SearchText.Text = ""; StatusFilter.SelectedValue = ""; LoadStudents(); }
        protected void RefreshButton_Click(object sender, EventArgs e) { FeedbackPanel.Visible = false; LoadStudents(); }
        protected void Students_PageIndexChanging(object sender, GridViewPageEventArgs e)
        { CloseEditor(); FeedbackPanel.Visible = false; Students.PageIndex = e.NewPageIndex; LoadStudents(); }
        private void ShowError(string message) { errMsg.Text = message; errMsg.Visible = true; }
        private void ShowFeedback(string message, bool success)
        {
            FeedbackPanel.Visible = true; FeedbackPanel.CssClass = success ? "alert alert-success" : "alert alert-warning";
            FeedbackMessage.Text = message;
        }
    }
}
