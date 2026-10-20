using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Security;
using SOBSchoolOfBytesWebSite.Learning;

namespace SOBSchoolOfBytesWebSite
{
    public partial class StudentDashboard : ProtectedPage
    {
        protected override string RequiredRole {get{return "Student";}}
        protected void Page_Load(object sender,EventArgs e)
        {
            StudentName.Text=CurrentAccount.FirstName;StudentUsername.Text=CurrentAccount.Username;
            if(!IsPostBack)LoadClasses();
        }
        private void LoadClasses()
        {
            try{MyClasses.DataSource=ClassService.MyEnrolments(CurrentAccount.Id);MyClasses.DataBind();}
            catch(SqlException exception){Trace.Warn("StudentDashboard","Could not load the timetable.",exception);Show("Your timetable could not be loaded. Please reload and try again.",false);}
        }
        protected void MyClasses_RowDataBound(object sender,GridViewRowEventArgs e)
        {
            if(e.Row.RowType==DataControlRowType.DataRow)((Button)e.Row.FindControl("WithdrawButton")).CommandArgument=e.Row.RowIndex.ToString(CultureInfo.InvariantCulture);
        }
        protected void MyClasses_RowCommand(object sender,GridViewCommandEventArgs e)
        {
            int index;if(e.CommandName!="WithdrawClass"||!int.TryParse(e.CommandArgument.ToString(),out index)||index<0||index>=MyClasses.DataKeys.Count)return;
            try{ClassService.Withdraw(CurrentAccount.Id,(int)MyClasses.DataKeys[index].Value);LoadClasses();Show("You have withdrawn from this class. Your place is now available to another student.",true);}
            catch(ClassRuleException exception){Show(exception.Message,false);LoadClasses();}
            catch(SqlException exception){Trace.Warn("StudentDashboard","Could not withdraw enrolment.",exception);Show("The enrolment could not be updated. Please try again.",false);}
        }
        private void Show(string message,bool success){FeedbackPanel.Visible=true;FeedbackPanel.CssClass=success?"alert alert-success":"alert alert-warning";FeedbackMessage.Text=message;}
        protected void SignOutButton_Click(object sender,EventArgs e){AccountAccess.SignOut(Context,"~/StudentLogin");}
    }
}
