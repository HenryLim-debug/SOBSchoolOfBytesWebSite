using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Security;
using SOBSchoolOfBytesWebSite.Learning;

namespace SOBSchoolOfBytesWebSite
{
    public partial class TeacherManagement : ProtectedPage
    {
        protected override string RequiredRole { get { return "President"; } }
        private int? EditingTeacherId
        {
            get { return ViewState["EditingTeacherId"] as int?; }
            set { ViewState["EditingTeacherId"] = value; }
        }
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) { LoadCourses(); LoadTeachers(); }
            SetFormMode();
        }
        protected override void OnPreRender(EventArgs e)
        {
            SetFormMode();
            base.OnPreRender(e);
        }
        private void SetFormMode()
        {
            bool creating = !EditingTeacherId.HasValue;
            FormTitle.Text = creating ? "Add Teacher" : "Edit Teacher";
            SaveButton.Text = creating ? "Create Teacher Account" : "Save Changes";
            InitialPasswordPanel.Visible = creating;
            PasswordRequired.Enabled = creating;
            ConfirmationRequired.Enabled = creating;
            CancelButton.Visible = !creating;
        }
        private void LoadCourses()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlDataAdapter adapter = new SqlDataAdapter(
                    "SELECT CourseId, Title FROM dbo.Courses WHERE IsAvailable=1 ORDER BY CourseId", connection))
                {
                    DataTable courses = new DataTable(); adapter.Fill(courses);
                    CourseList.DataSource = courses; CourseList.DataTextField = "Title";
                    CourseList.DataValueField = "CourseId"; CourseList.DataBind();
                    CourseList.Items.Insert(0, new ListItem("Choose a course", ""));
                }
            }
            catch (SqlException exception)
            {
                Trace.Warn("TeacherManagement", "Could not load available courses.", exception);
                CourseList.Items.Clear();
                ShowError("Courses could not be loaded. Reload this page and try again.");
            }
        }
        private void LoadTeachers()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlDataAdapter adapter = new SqlDataAdapter(@"SELECT u.Id, u.fname + N' ' + u.lname AS FullName,
                    u.username, u.staffId, u.email, u.accountStatus, c.Title AS CourseTitle
                    FROM dbo.userTable u LEFT JOIN dbo.TeacherCourses tc ON tc.TeacherId=u.Id
                    LEFT JOIN dbo.Courses c ON c.CourseId=tc.CourseId
                    WHERE u.usertype=N'Teacher' ORDER BY u.fname, u.lname, u.Id", connection))
                {
                    DataTable teachers = new DataTable(); adapter.Fill(teachers);
                    Teachers.DataSource = teachers; Teachers.DataBind(); Teachers.Visible = true;
                }
            }
            catch (SqlException exception)
            {
                Trace.Warn("TeacherManagement", "Could not load teachers.", exception);
                Teachers.Visible = false;
                ShowFeedback("The teacher list could not be loaded. Please try Refresh List.", false);
            }
        }
        protected void SaveButton_Click(object sender, EventArgs e)
        {
            errMsg.Visible = false; FeedbackPanel.Visible = false;
            fname.Text = fname.Text.Trim(); lname.Text = lname.Text.Trim();
            username.Text = username.Text.Trim(); staffId.Text = staffId.Text.Trim(); email.Text = email.Text.Trim();
            Page.Validate("TeacherDetails");
            if (!Page.IsValid) return;
            int courseId;
            if (string.IsNullOrWhiteSpace(fname.Text) || string.IsNullOrWhiteSpace(lname.Text)
                || string.IsNullOrWhiteSpace(staffId.Text) || fname.Text.Length > 50 || lname.Text.Length > 50
                || staffId.Text.Length > 30 || !Regex.IsMatch(username.Text, @"\A[A-Za-z0-9_]{3,30}\z"))
            { ShowError("Check the name and staff ID lengths. Usernames need 3–30 letters, numbers or underscores."); return; }
            if (email.Text.Length > 254 || (email.Text.Length > 0 && !Regex.IsMatch(email.Text, @"\A[^@\s]+@[^@\s]+\.[^@\s]+\z")))
            { ShowError("Enter a valid email address, or leave email blank."); return; }
            if (!int.TryParse(CourseList.SelectedValue, out courseId) || courseId < 1
                || (StatusList.SelectedValue != "Active" && StatusList.SelectedValue != "Inactive"))
            { ShowError("Choose an available course and a valid account status."); return; }
            bool creating = !EditingTeacherId.HasValue;
            if (creating)
            {
                if (pwd.Text.Length < 8 || pwd.Text.Length > 64 || string.IsNullOrWhiteSpace(pwd.Text)
                    || Encoding.UTF8.GetByteCount(pwd.Text) > 72 || pwd.Text.IndexOf('\0') >= 0)
                { ShowError("Use a password of 8–64 characters. A password with many emoji or non-English characters may need to be shorter."); return; }
                if (pwd.Text != confirmPwd.Text) { ShowError("Both passwords must match."); return; }
            }
            try
            {
                // Passwords are created only for new accounts; editing never reads or changes a saved hash.
                string hash = creating ? BCrypt.Net.BCrypt.HashPassword(pwd.Text) : null;
                SaveTeacher(courseId, hash);
                ResetForm(); LoadTeachers();
                ShowFeedback(creating ? "Teacher account created. The teacher can sign in using the details you entered."
                    : "Teacher details and course assignment saved.", true);
            }
            catch (SqlException exception)
            {
                if (exception.Number == 2601 || exception.Number == 2627)
                    ShowError("That username or staff ID is already in use. Choose another.");
                else
                {
                    Trace.Warn("TeacherManagement", "Could not save a teacher.", exception);
                    ShowError("The teacher could not be saved right now. Please try again.");
                }
            }
            catch (InvalidOperationException exception) { ShowError(exception.Message); }
            catch (ClassRuleException exception) { ShowError(exception.Message); }
        }
        private void SaveTeacher(int courseId, string hash)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                using (SqlCommand command = connection.CreateCommand())
                {
                    ClassService.LockChanges(connection, transaction);
                    if (EditingTeacherId.HasValue)
                        ClassService.EnsureTeacherCourseChangeAllowed(connection, transaction, EditingTeacherId.Value, courseId);
                    command.Transaction = transaction;
                    command.CommandText = @"SELECT COUNT(*) FROM dbo.userTable WITH (HOLDLOCK)
                        WHERE Id=@president AND usertype=N'President' AND accountStatus=N'Active'";
                    command.Parameters.Add("@president", SqlDbType.Int).Value = CurrentAccount.Id;
                    if ((int)command.ExecuteScalar() != 1)
                        throw new InvalidOperationException("Your president account is no longer active. Please sign in again.");
                    command.CommandText = "SELECT COUNT(*) FROM dbo.Courses WITH (HOLDLOCK) WHERE CourseId=@course AND IsAvailable=1";
                    command.Parameters.Add("@course", SqlDbType.Int).Value = courseId;
                    if ((int)command.ExecuteScalar() != 1)
                        throw new InvalidOperationException("This course is no longer available. Reload the page and choose another course.");
                    command.Parameters.Add("@fname", SqlDbType.NVarChar, 50).Value = fname.Text;
                    command.Parameters.Add("@lname", SqlDbType.NVarChar, 50).Value = lname.Text;
                    command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username.Text;
                    command.Parameters.Add("@staffId", SqlDbType.NVarChar, 30).Value = staffId.Text;
                    command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email.Text.Length == 0 ? (object)DBNull.Value : email.Text;
                    command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = StatusList.SelectedValue;
                    int teacherId;
                    if (!EditingTeacherId.HasValue)
                    {
                        command.Parameters.Add("@hash", SqlDbType.NVarChar, 300).Value = hash;
                        command.CommandText = @"INSERT INTO dbo.userTable (fname,lname,username,staffId,email,[password],usertype,accountStatus)
                            VALUES (@fname,@lname,@username,@staffId,@email,@hash,N'Teacher',@status);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        teacherId = (int)command.ExecuteScalar();
                    }
                    else
                    {
                        teacherId = EditingTeacherId.Value;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = teacherId;
                        command.CommandText = @"UPDATE dbo.userTable WITH (UPDLOCK) SET fname=@fname,lname=@lname,
                            username=@username,staffId=@staffId,email=@email,accountStatus=@status
                            WHERE Id=@id AND usertype=N'Teacher'";
                        if (command.ExecuteNonQuery() != 1)
                            throw new InvalidOperationException("This teacher account is no longer available. Refresh the list.");
                    }
                    if (!command.Parameters.Contains("@id")) command.Parameters.Add("@id", SqlDbType.Int).Value = teacherId;
                    command.CommandText = @"UPDATE dbo.TeacherCourses SET CourseId=@course,AssignedAt=SYSUTCDATETIME() WHERE TeacherId=@id;
                        IF @@ROWCOUNT=0 INSERT INTO dbo.TeacherCourses(TeacherId,CourseId) VALUES (@id,@course);";
                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }
        protected void Teachers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            string argument = e.Row.RowIndex.ToString(CultureInfo.InvariantCulture);
            ((Button)e.Row.FindControl("EditTeacherButton")).CommandArgument = argument;
            Button toggle = (Button)e.Row.FindControl("ToggleTeacherButton");
            toggle.CommandArgument = argument;
            toggle.Text = DataBinderStatus(e.Row) == "Active" ? "Deactivate" : "Activate";
        }
        private static string DataBinderStatus(GridViewRow row)
        {
            return System.Web.UI.DataBinder.Eval(row.DataItem, "accountStatus").ToString();
        }
        protected void Teachers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditTeacher" && e.CommandName != "ToggleTeacher") return;
            int rowIndex;
            if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out rowIndex)
                || rowIndex < 0 || rowIndex >= Teachers.DataKeys.Count)
            { ShowFeedback("Refresh the teacher list and try again.", false); return; }
            int teacherId = (int)Teachers.DataKeys[rowIndex].Values["Id"];
            errMsg.Visible = false; FeedbackPanel.Visible = false;
            try
            {
                if (e.CommandName == "EditTeacher") LoadTeacherForEditing(teacherId);
                else ToggleTeacher(teacherId, Teachers.DataKeys[rowIndex].Values["accountStatus"].ToString());
            }
            catch (SqlException exception)
            {
                Trace.Warn("TeacherManagement", "Could not manage a teacher.", exception);
                ShowFeedback("The teacher account could not be updated. Please refresh and try again.", false);
            }
        }
        private void LoadTeacherForEditing(int teacherId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(@"SELECT u.fname,u.lname,u.username,u.staffId,u.email,u.accountStatus,tc.CourseId
                FROM dbo.userTable u LEFT JOIN dbo.TeacherCourses tc ON tc.TeacherId=u.Id
                WHERE u.Id=@id AND u.usertype=N'Teacher'", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = teacherId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) { ShowFeedback("This teacher account is no longer available.", false); LoadTeachers(); return; }
                    EditingTeacherId = teacherId;
                    fname.Text = reader["fname"].ToString(); lname.Text = reader["lname"].ToString();
                    username.Text = reader["username"].ToString(); staffId.Text = reader["staffId"].ToString();
                    email.Text = reader["email"].ToString(); StatusList.SelectedValue = reader["accountStatus"].ToString();
                    CourseList.ClearSelection();
                    string selected = reader["CourseId"].ToString();
                    if (CourseList.Items.FindByValue(selected) != null) CourseList.SelectedValue = selected;
                    else if (CourseList.Items.Count > 0) CourseList.SelectedIndex = 0;
                    pwd.Text = ""; confirmPwd.Text = "";
                }
            }
        }
        private void ToggleTeacher(int teacherId, string previousStatus)
        {
            if (previousStatus != "Active" && previousStatus != "Inactive") return;
            string status = previousStatus == "Active" ? "Inactive" : "Active";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(@"UPDATE dbo.userTable SET accountStatus=@status
                WHERE Id=@id AND usertype=N'Teacher' AND accountStatus=@previousStatus
                AND EXISTS(SELECT 1 FROM dbo.userTable WHERE Id=@president AND usertype=N'President' AND accountStatus=N'Active')", connection))
            {
                command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = status;
                command.Parameters.Add("@previousStatus", SqlDbType.NVarChar, 20).Value = previousStatus;
                command.Parameters.Add("@id", SqlDbType.Int).Value = teacherId;
                command.Parameters.Add("@president", SqlDbType.Int).Value = CurrentAccount.Id;
                connection.Open();
                int changed = command.ExecuteNonQuery();
                if (changed == 1)
                {
                    if (EditingTeacherId == teacherId) ResetForm();
                    ShowFeedback(status == "Active" ? "Teacher account activated." : "Teacher account deactivated. Teacher access has stopped.", true);
                }
                else ShowFeedback("The account status has already changed or is no longer available. The list has been refreshed.", false);
            }
            LoadTeachers();
        }
        private void ResetForm()
        {
            EditingTeacherId = null;
            fname.Text = ""; lname.Text = ""; username.Text = ""; staffId.Text = "";
            email.Text = ""; pwd.Text = ""; confirmPwd.Text = "";
            StatusList.SelectedValue = "Active";
            if (CourseList.Items.Count > 0) CourseList.SelectedIndex = 0;
        }
        protected void CancelButton_Click(object sender, EventArgs e)
        { ResetForm(); errMsg.Visible = false; FeedbackPanel.Visible = false; }
        protected void RefreshButton_Click(object sender, EventArgs e)
        { FeedbackPanel.Visible = false; LoadTeachers(); }
        private void ShowError(string message) { errMsg.Text = message; errMsg.Visible = true; }
        private void ShowFeedback(string message, bool success)
        {
            FeedbackPanel.Visible = true;
            FeedbackPanel.CssClass = success ? "alert alert-success" : "alert alert-warning";
            FeedbackMessage.Text = message;
        }
    }
}
