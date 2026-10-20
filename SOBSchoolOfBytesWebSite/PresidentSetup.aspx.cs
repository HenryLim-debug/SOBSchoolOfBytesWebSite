using System;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class PresidentSetup : System.Web.UI.Page
    {
        private bool setupAllowed;
        protected void Page_Init(object sender, EventArgs e)
        {
            AccountAccess.PreventCaching(Response);
            ViewStateUserKey = AccountAccess.GetFormKey(Context);
            if (!Request.IsLocal)
            {
                Response.StatusCode = 403;
                Response.TrySkipIisCustomErrors = true;
                CloseSetup("Open this page on the computer running the website to set up the first president.");
                return;
            }
            try
            {
                setupAllowed = PresidentBootstrap.IsAvailable();
                if (!setupAllowed) CloseSetup("A president account already exists. First president setup is closed.");
            }
            catch (SqlException exception)
            {
                Trace.Warn("PresidentSetup", "Could not check first president setup.", exception);
                CloseSetup("Setup is unavailable right now. Please try again later.");
            }
        }
        protected void CreateButton_Click(object sender, EventArgs e)
        {
            if (!setupAllowed) return;
            errMsg.Visible = false;
            fname.Text = fname.Text.Trim(); lname.Text = lname.Text.Trim();
            username.Text = username.Text.Trim(); staffId.Text = staffId.Text.Trim();
            Page.Validate("PresidentSetup");
            if (!Page.IsValid) return;
            if (fname.Text.Length > 50 || lname.Text.Length > 50 || staffId.Text.Length > 30
                || !Regex.IsMatch(username.Text, @"\A[A-Za-z0-9_]{3,30}\z"))
            { ShowError("Check the field lengths and use 3–30 letters, numbers or underscores for your username."); return; }
            if (pwd.Text.Length < 8 || pwd.Text.Length > 64 || string.IsNullOrWhiteSpace(pwd.Text)
                || Encoding.UTF8.GetByteCount(pwd.Text) > 72 || pwd.Text.IndexOf('\0') >= 0)
            { ShowError("Use a password of 8–64 characters, up to 72 UTF-8 bytes."); return; }
            if (pwd.Text != confirmPwd.Text) { ShowError("The passwords do not match."); return; }
            try
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(pwd.Text);
                if (!PresidentBootstrap.TryCreate(fname.Text, lname.Text, username.Text, staffId.Text, hash))
                { CloseSetup("A president account already exists. First president setup is closed."); return; }
                Response.Redirect("~/StaffLogin?setup=1", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SqlException exception)
            {
                if (exception.Number == 2601 || exception.Number == 2627)
                    ShowError("That username or staff ID is already in use. Please choose another.");
                else
                {
                    Trace.Warn("PresidentSetup", "Could not create the first president.", exception);
                    ShowError("The account could not be created right now. Please try again later.");
                }
            }
        }
        private void CloseSetup(string message)
        {
            setupAllowed = false; SetupPanel.Visible = false;
            UnavailablePanel.Visible = true; UnavailableMessage.Text = message;
        }
        private void ShowError(string message) { errMsg.Text = message; errMsg.Visible = true; }
    }
}
