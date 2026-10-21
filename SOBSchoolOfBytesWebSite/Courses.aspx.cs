using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Security;
using SOBSchoolOfBytesWebSite.Learning;

namespace SOBSchoolOfBytesWebSite
{
    public partial class Courses : ProtectedPage
    {
        protected override string RequiredRole { get { return "Student"; } }
        protected void Page_Load(object sender,EventArgs e){if(!IsPostBack)LoadClasses();}
        private void LoadClasses()
        {
            try {AvailableClasses.DataSource=ClassService.Catalogue(CurrentAccount.Id);AvailableClasses.DataBind();}
            catch(SqlException exception){Trace.Warn("Courses","Could not load classes.",exception);Show("Classes could not be loaded. Please try again.",false);}
        }
        protected void AvailableClasses_RowDataBound(object sender,GridViewRowEventArgs e)
        {
            if(e.Row.RowType!=DataControlRowType.DataRow)return;
            DataRowView row=(DataRowView)e.Row.DataItem;Button button=(Button)e.Row.FindControl("EnrolButton");
            button.CommandArgument=e.Row.RowIndex.ToString(CultureInfo.InvariantCulture);
            if((bool)row["AlreadyEnrolled"]){button.Text="Already enrolled";button.Enabled=false;}
            else if((bool)row["ApplicationPending"]){button.Text="Application pending";button.Enabled=false;}
            else if((int)row["Seats"]>=10){button.Text="Class full";button.Enabled=false;}
        }
        protected void AvailableClasses_RowCommand(object sender,GridViewCommandEventArgs e)
        {
            int index;if(e.CommandName!="ApplyForCourse"||!int.TryParse(e.CommandArgument.ToString(),out index)||index<0||index>=AvailableClasses.DataKeys.Count)return;
            try {ClassService.ApplyForCourse(CurrentAccount.Id,(int)AvailableClasses.DataKeys[index].Value);LoadClasses();Show("Application submitted. The teacher of your requested class will review it. Check your Student Dashboard for its status.",true);}
            catch(ClassRuleException exception){Show(exception.Message,false);LoadClasses();}
            catch(SqlException exception){Trace.Warn("Courses","Could not apply for a course.",exception);Show("Your application could not be saved. Please refresh and try again.",false);}
        }
        protected void RefreshButton_Click(object sender,EventArgs e){FeedbackPanel.Visible=false;LoadClasses();}
        private void Show(string message,bool success){FeedbackPanel.Visible=true;FeedbackPanel.CssClass=success?"alert alert-success":"alert alert-warning";FeedbackMessage.Text=message;}
    }
}
