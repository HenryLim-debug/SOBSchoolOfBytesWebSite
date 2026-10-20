using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Security;
using SOBSchoolOfBytesWebSite.Learning;

namespace SOBSchoolOfBytesWebSite
{
    public partial class ClassRoster : ProtectedPage
    {
        private int classId;
        protected override string RequiredRole { get { return "President"; } }
        protected override bool IsRoleAllowed(string role){return role=="President"||role=="Teacher";}
        protected void Page_Load(object sender,EventArgs e)
        {
            BackLink.HRef=ResolveUrl(AccountAccess.DashboardPath(CurrentAccount.Role));
            if(!int.TryParse(Request.QueryString["id"],out classId)||classId<1){Deny();return;}
            LoadRoster();
        }
        private void LoadRoster()
        {
            try
            {
                DataRow row=ClassService.StaffClass(CurrentAccount.Id,classId);
                ClassTitle.Text=row["ClassName"].ToString();
                ClassDetails.Text=row["CourseTitle"]+" · "+row["TeacherName"]+" · "+row["Places"]+" places";
                ClassTimes.Text=row["WeeklySchedule"]+" · "+row["CourseDates"]+" · Malaysia time (UTC+8)";
                if(!IsPostBack){Students.DataSource=ClassService.Roster(CurrentAccount.Id,classId);Students.DataBind();}
                Students.Columns[3].Visible=CurrentAccount.Role=="President";
            }
            catch(ClassRuleException){Deny();}
            catch(SqlException exception){Trace.Warn("ClassRoster","Could not load class students.",exception);ClassPanel.Visible=false;Students.Visible=false;Show("Class students could not be loaded. Please try again.",false);}
        }
        private void Deny(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;ClassPanel.Visible=false;Students.Visible=false;Show("This class was not found or you do not have access to it.",false);}
        protected void Students_RowDataBound(object sender,GridViewRowEventArgs e)
        {
            if(e.Row.RowType!=DataControlRowType.DataRow)return;
            Button button=(Button)e.Row.FindControl("WithdrawButton");button.CommandArgument=e.Row.RowIndex.ToString(CultureInfo.InvariantCulture);button.Visible=CurrentAccount.Role=="President";
        }
        protected void Students_RowCommand(object sender,GridViewCommandEventArgs e)
        {
            int index;if(CurrentAccount.Role!="President"||!ClassPanel.Visible||e.CommandName!="WithdrawStudent"
                ||!int.TryParse(e.CommandArgument.ToString(),out index)||index<0||index>=Students.DataKeys.Count)return;
            try
            {
                ClassService.Withdraw(CurrentAccount.Id,(int)Students.DataKeys[index].Value);
                Students.DataSource=ClassService.Roster(CurrentAccount.Id,classId);Students.DataBind();LoadRoster();Show("Enrolment withdrawn. This class now has a free place.",true);
            }
            catch(ClassRuleException exception){Show(exception.Message,false);}
            catch(SqlException exception){Trace.Warn("ClassRoster","Could not withdraw enrolment.",exception);Show("The enrolment could not be updated. Please try again.",false);}
        }
        private void Show(string message,bool success){FeedbackPanel.Visible=true;FeedbackPanel.CssClass=success?"alert alert-success":"alert alert-warning";FeedbackMessage.Text=message;}
    }
}
