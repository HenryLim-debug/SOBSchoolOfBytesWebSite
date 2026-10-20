using System;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AccountAccess.PreventCaching(Response);
            try
            {
                UserAccount account = AccountAccess.GetCurrentAccount(Context);
                if (account != null) Response.Redirect(AccountAccess.DashboardPath(account.Role), true);
            }
            catch (SqlException exception) { Trace.Warn("Login", "Could not check current access.", exception); }
        }
    }
}
