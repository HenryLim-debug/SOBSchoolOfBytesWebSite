using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.SessionState;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public sealed class ChapterDownload : IHttpHandler, IReadOnlySessionState
    {
        public bool IsReusable { get { return false; } }
        public void ProcessRequest(HttpContext context)
        {
            AccountAccess.PreventCaching(context.Response);
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            try
            {
                var account = AccountAccess.GetCurrentAccount(context);
                int chapterId;
                string kind = context.Request.QueryString["kind"];
                if (account == null || !int.TryParse(context.Request.QueryString["chapter"], out chapterId) ||
                    chapterId <= 0 || (kind != "slides" && kind != "tutorial"))
                { Unavailable(context); return; }
                DataRow file = MaterialService.Download(account.Id, account.Role, chapterId, kind == "slides");
                byte[] data = (byte[])file["FileData"];
                string name = file["FileName"].ToString();
                string extension = Path.GetExtension(name).ToLowerInvariant();
                string mime = extension == ".pptx" ? "application/vnd.openxmlformats-officedocument.presentationml.presentation" :
                    extension == ".docx" ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" :
                    extension == ".ppt" ? "application/vnd.ms-powerpoint" : "application/msword";
                context.Response.ContentType = mime;
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + kind + extension + "\"; filename*=UTF-8''" + Uri.EscapeDataString(name));
                context.Response.AddHeader("Content-Length", data.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
                context.Response.BinaryWrite(data);
            }
            catch (ClassRuleException) { Unavailable(context); }
            catch (SqlException ex)
            {
                context.Trace.Warn("ChapterDownload", "Could not load document.", ex);
                context.Response.StatusCode = 503; context.Response.TrySkipIisCustomErrors = true;
                context.Response.ContentType = "text/plain";
                context.Response.Write("The document could not be downloaded. Please try again later.");
            }
        }
        private static void Unavailable(HttpContext context)
        {
            context.Response.StatusCode = 404; context.Response.TrySkipIisCustomErrors = true;
            context.Response.ContentType = "text/plain";
            context.Response.Write("This document is unavailable. Sign in and open it from your course page.");
        }
    }
}
