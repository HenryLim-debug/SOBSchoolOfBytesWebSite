using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class StudentLogin : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = StudentAccess.GetFormKey(Context);
            StudentAccess.PreventCaching(Response);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }
            try
            {
                UserAccount current = AccountAccess.GetCurrentAccount(Context);
                if (current != null)
                {
                    Response.Redirect(AccountAccess.DashboardPath(current.Role), false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
                StudentAccess.ClearSignIn(Context);
            }
            catch (SqlException exception)
            {
                Trace.Warn("StudentLogin", "Could not check the current student's access.", exception);
                ShowError("Sign in is unavailable right now. Please try again later.");
            }
            SignedOutPanel.Visible = Request.QueryString["signedOut"] == "1";
            if (Request.QueryString["unavailable"] == "1")
            {
                ShowError("Sign in is unavailable right now. Please try again later.");
            }
        }

        protected void SignInButton_Click(object sender, EventArgs e)
        {
            errMsg.Visible = false;
            SignedOutPanel.Visible = false;
            username.Text = username.Text.Trim();
            Page.Validate("StudentLogin");
            if (!Page.IsValid)
            {
                return;
            }
            if (username.Text.Length > 50 || pwd.Text.Length > 64
                || Encoding.UTF8.GetByteCount(pwd.Text) > 72 || pwd.Text.IndexOf('\0') >= 0)
            {
                ShowError("Username or password is incorrect.");
                return;
            }

            try
            {
                StudentAccount student;
                string passwordHash;
                string accountStatus;
                using (SqlConnection connection = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                using (SqlCommand command = new SqlCommand(@"SELECT Id, fname, username, [password], accountStatus, SecurityStamp
                    FROM dbo.userTable WHERE username = @username AND usertype = N'Student'", connection))
                {
                    command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username.Text;
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            ShowError("Username or password is incorrect.");
                            return;
                        }
                        student = new StudentAccount
                        {
                            Id = (int)reader["Id"],
                            FirstName = reader["fname"].ToString(),
                            Username = reader["username"].ToString(),
                            SecurityStamp = (Guid)reader["SecurityStamp"]
                        };
                        passwordHash = reader["password"].ToString();
                        accountStatus = reader["accountStatus"].ToString().Trim();
                    }
                }

                if (!PasswordMatches(pwd.Text, passwordHash))
                {
                    ShowError("Username or password is incorrect.");
                    return;
                }
                if (accountStatus == "Pending")
                {
                    ShowError("Your account is waiting for the president's approval. Please try again after it is approved.");
                    return;
                }
                if (accountStatus != "Approved")
                {
                    ShowError("Your student account is not active. Please contact the school for help.");
                    return;
                }

                StudentAccess.SignIn(Context, student);
                Response.Redirect("~/StudentDashboard", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SqlException exception)
            {
                Trace.Warn("StudentLogin", "Could not sign in a student.", exception);
                ShowError("Sign in is unavailable right now. Please try again later.");
            }
        }

        private static bool PasswordMatches(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private void ShowError(string message)
        {
            errMsg.Text = message;
            errMsg.Visible = true;
        }
    }
}
