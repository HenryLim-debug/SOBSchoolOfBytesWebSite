using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class SiteMaster : MasterPage
    {
        protected override void OnPreRender(EventArgs e)
        {
            // Navigation is only a shortcut. Protected pages still validate account access in the database.
            string role=Session["UserType"] as string;
            bool signedIn=Request.IsAuthenticated && Session["UserId"] is int &&
                (role=="Student" || role=="Teacher" || role=="President");
            RegisterNavItem.Visible=!signedIn; LoginNavItem.Visible=!signedIn; DashboardNavItem.Visible=signedIn;
            if(signedIn)
            {
                DashboardNav.HRef=AccountAccess.DashboardPath(role);
                AccountAccess.PreventCaching(Response);
            }
            string page=VirtualPathUtility.GetFileName(Page.AppRelativeVirtualPath).ToLowerInvariant();
            MarkActive(HomeNav,page=="default.aspx"); MarkActive(CoursesNav,page=="courses.aspx" || page=="coursecatalogue.aspx" || page=="coursepreview.aspx");
            MarkActive(AboutNav,page=="about.aspx"); MarkActive(ContactNav,page=="contact.aspx");
            MarkActive(RegisterNav,page=="studentregistration.aspx");
            MarkActive(LoginNav,page=="login.aspx" || page=="studentlogin.aspx" || page=="stafflogin.aspx");
            MarkActive(DashboardNav,page=="studentdashboard.aspx" || page=="teacherdashboard.aspx" || page=="presidentdashboard.aspx");
            base.OnPreRender(e);
        }
        private static void MarkActive(HtmlAnchor link,bool active)
        {
            link.Attributes["class"]=string.Join(" ",(link.Attributes["class"]??"").Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries).Where(c=>c!="active"));
            if(active) { link.Attributes["class"]+=" active"; link.Attributes["aria-current"]="page"; }
            else link.Attributes.Remove("aria-current");
        }
    }
}
