using System;
using System.Data.SqlClient;
using System.Globalization;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class TeacherSalary : ProtectedPage
    {
        protected override string RequiredRole { get { return "Teacher"; } }
        protected void Page_Load(object sender,EventArgs e)
        {
            MonthInput.Attributes["max"]=SalaryService.CurrentMonth.ToString("yyyy-MM",CultureInfo.InvariantCulture);
            if(!IsPostBack) { MonthInput.Text=SalaryService.CurrentMonth.ToString("yyyy-MM",CultureInfo.InvariantCulture); LoadSalary(); }
        }
        protected void LoadButton_Click(object sender,EventArgs e) { LoadSalary(); }
        private void LoadSalary()
        {
            ResultsPanel.Visible=false; MessagePanel.Visible=false;
            try
            {
                DateTime month=SalaryService.ParseMonth(MonthInput.Text);
                SelectedMonth.Text=month.ToString("MMMM yyyy",CultureInfo.InvariantCulture);
                SalarySummary.DataSource=SalaryService.Summary(CurrentAccount.Id,CurrentAccount.Id,month); SalarySummary.DataBind();
                LessonRecords.DataSource=SalaryService.Lessons(CurrentAccount.Id,CurrentAccount.Id,month); LessonRecords.DataBind();
                SalaryHistory.DataSource=SalaryService.History(CurrentAccount.Id,CurrentAccount.Id); SalaryHistory.DataBind();
                ResultsPanel.Visible=true;
            }
            catch(ClassRuleException ex) { Show(ex.Message); }
            catch(SqlException ex) { Trace.Warn("TeacherSalary","Could not load salary.",ex); Show("Salary information could not be loaded. Please try again."); }
        }
        private void Show(string message) { MessagePanel.Visible=true; MessageText.Text=message; }
    }
}
