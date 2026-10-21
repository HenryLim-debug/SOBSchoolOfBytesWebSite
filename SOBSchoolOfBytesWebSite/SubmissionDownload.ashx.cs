using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public sealed class SubmissionDownload : IHttpHandler,IReadOnlySessionState
    {
        public bool IsReusable { get { return false; } }
        public void ProcessRequest(HttpContext context)
        {
            AccountAccess.PreventCaching(context.Response);
            context.Response.Headers["X-Content-Type-Options"]="nosniff";
            try
            {
                var account=AccountAccess.GetCurrentAccount(context); int id;
                if(account==null || !int.TryParse(context.Request.QueryString["id"],out id) || id<=0) { Unavailable(context); return; }
                DataRow row=SubmissionService.Download(account.Id,account.Role,id);
                byte[] data=(byte[])row["FileData"];
                context.Response.ContentType="application/octet-stream";
                context.Response.AddHeader("Content-Disposition","attachment; filename=\"submission\"; filename*=UTF-8''"+Uri.EscapeDataString(row["FileName"].ToString()));
                context.Response.AddHeader("Content-Length",data.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
                context.Response.BinaryWrite(data);
            }
            catch(ClassRuleException) { Unavailable(context); }
            catch(SqlException ex) { context.Trace.Warn("SubmissionDownload","Could not load file.",ex); context.Response.StatusCode=503; context.Response.TrySkipIisCustomErrors=true; context.Response.ContentType="text/plain"; context.Response.Write("Download unavailable. Please try again later."); }
        }
        private static void Unavailable(HttpContext context)
        { context.Response.StatusCode=404; context.Response.TrySkipIisCustomErrors=true; context.Response.ContentType="text/plain"; context.Response.Write("This submission is unavailable. Sign in and open it from your dashboard."); }
    }
}
