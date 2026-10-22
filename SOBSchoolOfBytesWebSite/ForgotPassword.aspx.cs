using System;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        protected void Page_Init(object sender,EventArgs e)
        {
            ViewStateUserKey=AccountAccess.GetFormKey(Context);
            AccountAccess.PreventCaching(Response);
            Response.Headers["Referrer-Policy"]="no-referrer";
        }
        protected void Page_Load(object sender,EventArgs e)
        {
            if(IsPostBack) return;
            AccountType.SelectedValue=Request.QueryString["role"]=="staff" ? "Staff" : "Student";
            SetFields();
            if(Request.QueryString["done"]=="1" && Session["PasswordResetDone"] is bool && (bool)Session["PasswordResetDone"])
            {
                Session.Remove("PasswordResetDone");
                SuccessPanel.Visible=true;
                ResetPanel.Visible=false;
            }
        }
        protected void AccountType_Changed(object sender,EventArgs e)
        {
            SetFields(); Message.Visible=false;
            RecoveryCode.Text=""; NewPassword.Text=""; ConfirmPassword.Text="";
        }
        private void SetFields() { StudentFields.Visible=AccountType.SelectedValue=="Student"; StaffFields.Visible=!StudentFields.Visible; }
        protected void ResetButton_Click(object sender,EventArgs e)
        {
            Message.Visible=false;
            if(!RecoveryService.ValidNewPassword(NewPassword.Text))
            { ShowError("Use 8 to 64 characters for your new password (no more than 72 UTF-8 bytes)."); return; }
            if(!string.Equals(NewPassword.Text,ConfirmPassword.Text,StringComparison.Ordinal))
            { ShowError("The new passwords do not match."); return; }
            try
            {
                if(!RecoveryService.Reset(AccountType.SelectedValue=="Student",Username.Text,Email.Text,StaffId.Text,
                    FirstName.Text,LastName.Text,RecoveryCode.Text,NewPassword.Text,Request.UserHostAddress??"unknown"))
                {
                    ShowError("The account details or recovery code are incorrect, the code has been used, or there have been too many attempts. Check your saved details; wait 15 minutes after repeated attempts.");
                    return;
                }
                AccountAccess.ClearSignIn(Context);
                Session.Clear();
                Session["PasswordResetDone"]=true;
                Response.Redirect("~/ForgotPassword?done=1",false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch(SqlException) { ShowError("Password reset is unavailable. Please try again later."); }
            catch(InvalidOperationException exception) { ShowError(exception.Message); }
        }
        private void ShowError(string text) { Message.Text=Server.HtmlEncode(text); Message.Visible=true; }
    }
}
