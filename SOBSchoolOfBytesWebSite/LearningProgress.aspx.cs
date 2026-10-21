using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class LearningProgress : ProtectedPage
    {
        protected override string RequiredRole { get { return "Student"; } }
        protected override bool IsRoleAllowed(string role) { return role=="Student" || role=="Teacher" || role=="President"; }
        protected void Page_Load(object sender,EventArgs e)
        { BackLink.HRef=ResolveUrl(AccountAccess.DashboardPath(CurrentAccount.Role)); }
        protected void Summary_RowCommand(object sender,GridViewCommandEventArgs e)
        {
            if(e.CommandName!="ViewChapters") return;
            int id;
            if(int.TryParse(Convert.ToString(e.CommandArgument),out id)) ViewState["ProgressEnrolment"]=id;
        }
        protected override void OnPreRender(EventArgs e)
        {
            ResultsPanel.Visible=false; DetailsPanel.Visible=false; ErrorPanel.Visible=false;
            try
            {
                DataTable rows=ProgressService.Summary(CurrentAccount.Id);
                Summary.DataSource=rows; Summary.DataBind();
                int selected=ViewState["ProgressEnrolment"] is int ? (int)ViewState["ProgressEnrolment"] : 0;
                DataRow chosen=null;
                foreach(DataRow row in rows.Rows) if((int)row["EnrolmentId"]==selected) chosen=row;
                if(selected!=0 && chosen==null) throw new ClassRuleException("This enrolment is no longer available to your account. Return to your dashboard.");
                if(chosen!=null)
                {
                    DetailsTitle.Text=chosen["StudentName"]+" — "+chosen["CourseTitle"]+" — "+chosen["ClassName"];
                    Chapters.Columns[6].Visible=CurrentAccount.Role=="Student";
                    Chapters.DataSource=ProgressService.Chapters(CurrentAccount.Id,selected); Chapters.DataBind();
                    DetailsPanel.Visible=true;
                }
                ResultsPanel.Visible=true;
            }
            catch(ClassRuleException ex) { ErrorPanel.Visible=true; ErrorMessage.Text=ex.Message; ViewState.Remove("ProgressEnrolment"); }
            catch(SqlException ex) { Trace.Warn("LearningProgress","Could not load progress.",ex); ErrorPanel.Visible=true; ErrorMessage.Text="Learning progress could not be loaded. Please try again."; }
            base.OnPreRender(e);
        }
    }
}
