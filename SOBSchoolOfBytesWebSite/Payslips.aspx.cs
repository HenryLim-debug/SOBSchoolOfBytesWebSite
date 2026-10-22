using System;
using System.Data.SqlClient;
using System.Globalization;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class Payslips : ProtectedPage
    {
        protected override string RequiredRole { get { return "Teacher"; } }
        protected override bool IsRoleAllowed(string role) { return role=="Teacher" || role=="President"; }
        protected void Page_Load(object sender,EventArgs e)
        {
            BackLink.HRef=ResolveUrl(AccountAccess.DashboardPath(CurrentAccount.Role));
            IssuePanel.Visible=CurrentAccount.Role=="President";
            MonthInput.Attributes["max"]=SalaryService.CurrentMonth.ToString("yyyy-MM",CultureInfo.InvariantCulture);
            if(!IsPostBack && CurrentAccount.Role=="President")
            {
                MonthInput.Text=SalaryService.CurrentMonth.ToString("yyyy-MM",CultureInfo.InvariantCulture);
                try { TeacherList.DataSource=SalaryService.Teachers(CurrentAccount.Id); TeacherList.DataTextField="Label"; TeacherList.DataValueField="Id"; TeacherList.DataBind(); }
                catch(SqlException ex) { Trace.Warn("Payslips","Could not load teachers.",ex); Show("Teachers could not be loaded. Please reload this page."); }
            }
        }
        private int TeacherId()
        { int id; if(!int.TryParse(TeacherList.SelectedValue,out id)) throw new ClassRuleException("Choose a teacher before preparing a payslip."); return id; }
        protected void PreviewButton_Click(object sender,EventArgs e)
        {
            PreviewPanel.Visible=false; MessagePanel.Visible=false;
            ViewState.Remove("PayslipFingerprint");
            try
            {
                if(CurrentAccount.Role!="President") throw new ClassRuleException("Only presidents can issue payslips.");
                int teacher=TeacherId(); DateTime month=SalaryService.ParseMonth(MonthInput.Text);
                PayslipPreview preview=PayslipService.Preview(CurrentAccount.Id,teacher,month);
                ViewState["PayslipTeacher"]=teacher; ViewState["PayslipMonth"]=month;
                ViewState["PayslipFingerprint"]=preview.Fingerprint; ViewState["PayslipPrevious"]=preview.PreviousId;
                PreviewHeading.Text=preview.Teacher["TeacherName"]+" — "+month.ToString("MMMM yyyy",CultureInfo.InvariantCulture);
                PreviewTotals.Text=string.Format(CultureInfo.InvariantCulture,"Approved hours: {0:N2} × RM {1:N2} = RM {2:N2} gross teaching earnings.",preview.Minutes/60m,preview.Rate,preview.Amount);
                bool unchanged=preview.PreviousId!=0 && preview.PreviousFingerprint==preview.Fingerprint;
                PreviewNotice.Text=unchanged ? "These details match the latest issued payslip. Issuing again will open that record without creating a duplicate." :
                    "This will issue revision "+preview.NextRevision+". "+(preview.PreviousId==0 ? "" : "The previous version will remain available in the history. ");
                if(month==SalaryService.CurrentMonth) PreviewNotice.Text+=" This month is still in progress; later approvals may require another revision.";
                PreviewLessons.DataSource=preview.Lessons; PreviewLessons.DataBind();
                ReasonInput.Text=""; IssueButton.Text=unchanged ? "Open Existing Payslip" : (preview.PreviousId==0 ? "Issue Payslip" : "Issue Revised Payslip");
                // Store a reason for normal issue; opening an unchanged record needs no new reason.
                ViewState["PayslipUnchanged"]=unchanged;
                ReasonInput.Enabled=!unchanged;
                PreviewPanel.Visible=true;
            }
            catch(ClassRuleException ex) { Show(ex.Message); }
            catch(SqlException ex) { Trace.Warn("Payslips","Could not prepare payslip.",ex); Show("The payslip preview could not be loaded. Please try again."); }
        }
        protected void IssueButton_Click(object sender,EventArgs e)
        {
            try
            {
                if(CurrentAccount.Role!="President") throw new ClassRuleException("Only presidents can issue payslips.");
                int teacher=TeacherId(); DateTime month=SalaryService.ParseMonth(MonthInput.Text);
                if(!(ViewState["PayslipTeacher"] is int) || (int)ViewState["PayslipTeacher"]!=teacher ||
                    !(ViewState["PayslipMonth"] is DateTime) || (DateTime)ViewState["PayslipMonth"]!=month || !(ViewState["PayslipPrevious"] is int))
                    throw new ClassRuleException("Preview the selected teacher and month before issuing a payslip.");
                string reason=ViewState["PayslipUnchanged"] is bool && (bool)ViewState["PayslipUnchanged"] ? "Open unchanged payslip" : ReasonInput.Text;
                int id=PayslipService.Issue(CurrentAccount.Id,teacher,month,ViewState["PayslipFingerprint"] as string,(int)ViewState["PayslipPrevious"],reason);
                Response.Redirect(ResolveUrl("~/Payslip?id="+id),false); Context.ApplicationInstance.CompleteRequest();
            }
            catch(ClassRuleException ex) { Show(ex.Message); }
            catch(SqlException ex) { Trace.Warn("Payslips","Could not issue payslip.",ex); Show("The payslip could not be issued. Preview it again and retry."); }
        }
        protected override void OnPreRender(EventArgs e)
        {
            try { Records.DataSource=PayslipService.History(CurrentAccount.Id); Records.DataBind(); }
            catch(SqlException ex) { Records.Visible=false; Trace.Warn("Payslips","Could not load history.",ex); Show("Payslip records could not be loaded. Please reload this page."); }
            base.OnPreRender(e);
        }
        private void Show(string message) { MessagePanel.Visible=true; MessageText.Text=message; }
    }
}
