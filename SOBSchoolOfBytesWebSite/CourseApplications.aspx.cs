using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class CourseApplications : ProtectedPage
    {
        protected override string RequiredRole { get { return "Teacher"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadApplications();
        }

        private void LoadApplications()
        {
            try
            {
                PendingRequests.DataSource = ClassService.PendingApplications(CurrentAccount.Id);
                PendingRequests.DataBind();
                PendingRequests.Visible = true;
            }
            catch (SqlException exception)
            {
                Trace.Warn("CourseApplications", "Could not load applications.", exception);
                PendingRequests.Visible = false;
                Show("Applications could not be loaded. Please refresh and try again.", false);
            }
        }

        protected void PendingRequests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            string argument = e.Row.RowIndex.ToString(CultureInfo.InvariantCulture);
            ((Button)e.Row.FindControl("ApproveButton")).CommandArgument = argument;
            ((Button)e.Row.FindControl("RejectButton")).CommandArgument = argument;
        }

        protected void PendingRequests_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ApproveApplication" && e.CommandName != "RejectApplication") return;
            int index;
            if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out index)
                || index < 0 || index >= PendingRequests.DataKeys.Count) return;
            try
            {
                string message = ClassService.ReviewApplication(CurrentAccount.Id,
                    (int)PendingRequests.DataKeys[index].Value, e.CommandName == "ApproveApplication");
                LoadApplications();
                Show(message, true);
            }
            catch (ClassRuleException exception) { LoadApplications(); Show(exception.Message, false); }
            catch (SqlException exception)
            {
                Trace.Warn("CourseApplications", "Could not review application.", exception);
                Show("The decision could not be saved. Refresh the list and try again.", false);
            }
        }

        protected void RefreshButton_Click(object sender, EventArgs e)
        {
            FeedbackPanel.Visible = false;
            LoadApplications();
        }

        private void Show(string message, bool success)
        {
            FeedbackPanel.Visible = true;
            FeedbackPanel.CssClass = success ? "alert alert-success" : "alert alert-warning";
            FeedbackMessage.Text = message;
        }
    }
}
