using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace SOBSchoolOfBytesWebSite.Security
{
    public sealed class UserAccount
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string StaffId { get; set; }
    }

    public static class AccountAccess
    {
        public static string GetFormKey(HttpContext context)
        {
            if (context.Session["AccountFormKey"] == null)
                context.Session["AccountFormKey"] = Guid.NewGuid().ToString("N");
            return (string)context.Session["AccountFormKey"];
        }

        public static UserAccount GetCurrentAccount(HttpContext context)
        {
            int userId;
            if (context.User == null || !context.User.Identity.IsAuthenticated
                || !int.TryParse(context.User.Identity.Name, NumberStyles.None, CultureInfo.InvariantCulture, out userId)
                || !(context.Session["UserId"] is int) || (int)context.Session["UserId"] != userId)
                return null;

            // Recheck role and status on every protected request, so revoked access takes effect immediately.
            using (SqlConnection connection = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            using (SqlCommand command = new SqlCommand(@"SELECT Id, fname, username, usertype, staffId
                FROM dbo.userTable WHERE Id = @id AND
                ((usertype = N'Student' AND accountStatus = N'Approved') OR
                 (usertype IN (N'Teacher', N'President') AND accountStatus = N'Active'))", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read() || !string.Equals(reader["usertype"].ToString(),
                        context.Session["UserType"] as string, StringComparison.Ordinal))
                        return null;
                    return new UserAccount
                    {
                        Id = (int)reader["Id"], FirstName = reader["fname"].ToString(),
                        Username = reader["username"].ToString(), Role = reader["usertype"].ToString(),
                        StaffId = reader["staffId"] == DBNull.Value ? null : reader["staffId"].ToString()
                    };
                }
            }
        }

        public static string DashboardPath(string role)
        {
            switch (role)
            {
                case "Student": return "~/StudentDashboard";
                case "Teacher": return "~/TeacherDashboard";
                case "President": return "~/PresidentDashboard";
                default: throw new ArgumentException("Unknown account role.", "role");
            }
        }

        public static void SignIn(HttpContext context, UserAccount account)
        {
            context.Session["UserId"] = account.Id;
            context.Session["UserType"] = account.Role;
            context.Session["FirstName"] = account.FirstName;
            context.Session["Username"] = account.Username;
            context.Session["StaffId"] = account.StaffId;
            context.Session["AccountFormKey"] = Guid.NewGuid().ToString("N");
            HttpCookie cookie = FormsAuthentication.GetAuthCookie(account.Id.ToString(CultureInfo.InvariantCulture), false);
            cookie.HttpOnly = true;
            cookie.Secure = context.Request.IsSecureConnection;
            cookie.SameSite = SameSiteMode.Lax;
            context.Response.Cookies.Add(cookie);
        }

        public static void ClearSignIn(HttpContext context)
        {
            FormsAuthentication.SignOut();
            foreach (string key in new[] { "UserId", "UserType", "FirstName", "Username", "StaffId" })
                context.Session.Remove(key);
        }

        public static void SignOut(HttpContext context, string loginPath)
        {
            ClearSignIn(context);
            context.Session.Clear();
            context.Session.Abandon();
            context.Response.Redirect(loginPath + "?signedOut=1", true);
        }

        public static bool PasswordMatches(string password, string hash)
        {
            try { return BCrypt.Net.BCrypt.Verify(password, hash); }
            catch (BCrypt.Net.SaltParseException) { return false; }
            catch (ArgumentException) { return false; }
        }

        public static void PreventCaching(HttpResponse response)
        {
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        }
    }

    public abstract class ProtectedPage : Page
    {
        protected abstract string RequiredRole { get; }
        protected virtual bool IsRoleAllowed(string role) { return role == RequiredRole; }
        protected UserAccount CurrentAccount { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            AccountAccess.PreventCaching(Response);
            string loginPath = RequiredRole == "Student" ? "~/StudentLogin" : "~/StaffLogin";
            try { CurrentAccount = AccountAccess.GetCurrentAccount(Context); }
            catch (SqlException exception)
            {
                Trace.Warn("AccountAccess", "Could not check account access.", exception);
                AccountAccess.ClearSignIn(Context);
                Response.Redirect(loginPath + "?unavailable=1", true);
                return;
            }
            if (CurrentAccount == null)
            {
                AccountAccess.ClearSignIn(Context);
                Response.Redirect(loginPath, true);
                return;
            }
            if (!IsRoleAllowed(CurrentAccount.Role))
            {
                Response.Redirect(AccountAccess.DashboardPath(CurrentAccount.Role), true);
                return;
            }
            ViewStateUserKey = AccountAccess.GetFormKey(Context);
            base.OnInit(e);
        }
    }
}
