using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class StaffLogin : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = AccountAccess.GetFormKey(Context);
            AccountAccess.PreventCaching(Response);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            try
            {
                UserAccount current = AccountAccess.GetCurrentAccount(Context);
                if (current != null)
                {
                    Response.Redirect(AccountAccess.DashboardPath(current.Role), true);
                    return;
                }
                AccountAccess.ClearSignIn(Context);
            }
            catch (SqlException exception)
            {
                Trace.Warn("StaffLogin", "Could not check current access.", exception);
                ShowError("Sign in is unavailable right now. Please try again later.");
            }
            if (Request.QueryString["signedOut"] == "1")
            {
                SuccessPanel.Visible = true;
                SuccessMessage.Text = "You have signed out successfully.";
            }
            if (Request.QueryString["unavailable"] == "1")
                ShowError("Sign in is unavailable right now. Please try again later.");
        }
        protected void SignInButton_Click(object sender, EventArgs e)
        {
            errMsg.Visible = false;
            SuccessPanel.Visible = false;
            username.Text = username.Text.Trim();
            staffId.Text = staffId.Text.Trim();
            Page.Validate("StaffLogin");
            if (!Page.IsValid) return;
            if (username.Text.Length > 50 || staffId.Text.Length > 30 || pwd.Text.Length > 64
                || Encoding.UTF8.GetByteCount(pwd.Text) > 72 || pwd.Text.IndexOf('\0') >= 0)
            {
                ShowError("Username, staff ID or password is incorrect.");
                return;
            }
            try
            {
                UserAccount account;
                string hash, status;
                using (SqlConnection connection = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                using (SqlCommand command = new SqlCommand(@"SELECT Id, fname, username, usertype, staffId, [password], accountStatus
                    FROM dbo.userTable WHERE username = @username AND usertype IN (N'Teacher', N'President')", connection))
                {
                    command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username.Text;
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) { ShowError("Username, staff ID or password is incorrect."); return; }
                        account = new UserAccount { Id = (int)reader["Id"], FirstName = reader["fname"].ToString(),
                            Username = reader["username"].ToString(), Role = reader["usertype"].ToString(), StaffId = reader["staffId"].ToString() };
                        hash = reader["password"].ToString();
                        status = reader["accountStatus"].ToString();
                    }
                }
                if (!AccountAccess.PasswordMatches(pwd.Text, hash))
                { ShowError("Username, staff ID or password is incorrect."); return; }
                if (!string.Equals(staffId.Text, account.StaffId, StringComparison.OrdinalIgnoreCase))
                { ShowError("The staff ID does not match this account."); return; }
                if (status != "Active")
                { ShowError("Your staff account is not active. Please contact the school for help."); return; }
                AccountAccess.SignIn(Context, account);
                Response.Redirect(AccountAccess.DashboardPath(account.Role), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SqlException exception)
            {
                Trace.Warn("StaffLogin", "Could not sign in a staff member.", exception);
                ShowError("Sign in is unavailable right now. Please try again later.");
            }
        }
        private void ShowError(string message) { errMsg.Text = message; errMsg.Visible = true; }
    }
}
