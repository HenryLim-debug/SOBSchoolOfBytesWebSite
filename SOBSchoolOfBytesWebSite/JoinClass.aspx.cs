using System;
using System.Data;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class JoinClass : ProtectedPage
    {
        protected override string RequiredRole { get { return "Student"; } }
        protected override bool IsRoleAllowed(string role) { return role=="Student" || role=="Teacher" || role=="President"; }
        protected void Page_Load(object sender,EventArgs e)
        {
            BackLink.HRef=ResolveUrl(AccountAccess.DashboardPath(CurrentAccount.Role));
            Response.Headers["Referrer-Policy"]="no-referrer";
            try
            {
                int id;
                if(!int.TryParse(Request.QueryString["id"],out id) || id<1) throw new ClassRuleException("This class is unavailable or you do not have access to it.");
                DataRow row=MeetingService.Read(CurrentAccount.Id,id,true);
                if(row.IsNull("MeetingUrl")) { MessageText.Text="The meeting link has not been added, or has been removed. Please check with your teacher."; return; }
                string url=MeetingService.ValidateUrl(row["MeetingUrl"].ToString());
                Response.Redirect(url,false); Context.ApplicationInstance.CompleteRequest();
            }
            catch(ClassRuleException ex) { Response.StatusCode=404; Response.TrySkipIisCustomErrors=true; MessageText.Text=ex.Message; }
            catch(SqlException ex) { Trace.Warn("JoinClass","Could not read meeting link.",ex); MessageText.Text="The meeting link could not be loaded. Please try again."; }
        }
    }
}
