using System;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class RecoverySettings : ProtectedPage
    {
        protected override string RequiredRole { get { return "Student"; } }
        protected override bool IsRoleAllowed(string role) { return role=="Student" || role=="Teacher" || role=="President"; }
        protected void Page_Load(object sender,EventArgs e)
        {
            Response.Headers["Referrer-Policy"]="no-referrer";
            DashboardLink.NavigateUrl=AccountAccess.DashboardPath(CurrentAccount.Role);
            StudentEmailPanel.Visible=CurrentAccount.Role=="Student";
            if (IsPostBack) return;
            try
            {
                var profile=RecoveryService.ReadProfile(CurrentAccount);
                RecoveryEmail.Text=profile.Email;
                CodeStatus.Text=profile.HasCode ? "A recovery code is ready. Keep your saved copy safe." : "No recovery code is set. Create one below.";
                GenerateButton.Text=profile.HasCode ? "Replace Recovery Code" : "Create Recovery Code";
                if(Request.QueryString["created"]=="1" && Session["RecoveryCodeOwner"] is int
                    && (int)Session["RecoveryCodeOwner"]==CurrentAccount.Id && Session["PendingRecoveryCode"] is string)
                {
                    NewCode.Text=(string)Session["PendingRecoveryCode"];
                    NewCodePanel.Visible=true;
                }
                Session.Remove("PendingRecoveryCode");
                Session.Remove("RecoveryCodeOwner");
            }
            catch(SqlException) { ShowError("Recovery settings are unavailable. Please try again later."); }
            catch(InvalidOperationException exception) { ShowError(exception.Message); }
        }
        protected void GenerateButton_Click(object sender,EventArgs e)
        {
            Message.Visible=false;
            try
            {
                string code=RecoveryService.Generate(CurrentAccount,CurrentPassword.Text,RecoveryEmail.Text,Request.UserHostAddress??"unknown");
                Session["PendingRecoveryCode"]=code;
                Session["RecoveryCodeOwner"]=CurrentAccount.Id;
                // A redirect prevents refreshing the result from issuing a different code.
                Response.Redirect("~/RecoverySettings?created=1",false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch(SqlException) { ShowError("The recovery code could not be created. Please try again later."); }
            catch(InvalidOperationException exception) { ShowError(exception.Message); }
        }
        private void ShowError(string text) { Message.Text=Server.HtmlEncode(text); Message.Visible=true; }
    }
}
