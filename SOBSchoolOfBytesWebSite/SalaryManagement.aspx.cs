using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class SalaryManagement : ProtectedPage
    {
        protected override string RequiredRole { get { return "President"; } }
        protected void Page_Load(object sender,EventArgs e)
        {
            MonthInput.Attributes["max"]=SalaryService.CurrentMonth.ToString("yyyy-MM",CultureInfo.InvariantCulture);
            if(!IsPostBack)
            {
                MonthInput.Text=SalaryService.CurrentMonth.ToString("yyyy-MM",CultureInfo.InvariantCulture);
                Safely(delegate {
                    TeacherList.DataSource=SalaryService.Teachers(CurrentAccount.Id); TeacherList.DataTextField="Label"; TeacherList.DataValueField="Id"; TeacherList.DataBind();
                    LoadSalary();
                });
            }
        }
        private int TeacherId()
        { int value; if(!int.TryParse(TeacherList.SelectedValue,out value)) throw new ClassRuleException("Create a teacher account first, then return to this page."); return value; }
        protected void LoadButton_Click(object sender,EventArgs e) { Safely(LoadSalary); }
        protected void TeacherList_SelectedIndexChanged(object sender,EventArgs e) { Safely(LoadSalary); }
        private void LoadSalary()
        {
            ResultsPanel.Visible=false;
            DateTime month=SalaryService.ParseMonth(MonthInput.Text); int teacher=TeacherId();
            SelectedMonth.Text=month.ToString("MMMM yyyy",CultureInfo.InvariantCulture);
            SelectedTeacher.Text=TeacherList.SelectedItem.Text;
            AllSalaries.DataSource=SalaryService.AllTeachers(CurrentAccount.Id,month); AllSalaries.DataBind();
            SalaryHistory.DataSource=SalaryService.History(CurrentAccount.Id,teacher); SalaryHistory.DataBind();
            LessonRecords.DataSource=SalaryService.Lessons(CurrentAccount.Id,teacher,month); LessonRecords.DataBind();
            RateChanges.DataSource=SalaryService.RateHistory(CurrentAccount.Id,teacher,month); RateChanges.DataBind();
            DataRow rate=SalaryService.Rate(CurrentAccount.Id,teacher,month);
            RateInput.Text=rate==null ? "" : ((decimal)rate["HourlyRate"]).ToString("0.00",CultureInfo.InvariantCulture);
            RateReason.Text="";
            ViewState["RateVersion"]=rate==null ? null : rate["RowVersion"];
            ViewState["RateTeacher"]=teacher; ViewState["RateMonth"]=month;
            ResultsPanel.Visible=true;
        }
        protected void SaveRateButton_Click(object sender,EventArgs e)
        {
            bool saved=false;
            try
            {
                int teacher=TeacherId(); DateTime month=SalaryService.ParseMonth(MonthInput.Text);
                if(!(ViewState["RateTeacher"] is int) || (int)ViewState["RateTeacher"]!=teacher ||
                    !(ViewState["RateMonth"] is DateTime) || (DateTime)ViewState["RateMonth"]!=month)
                    throw new ClassRuleException("Select Load Month before setting the rate for a different month or teacher.");
                decimal rate;
                if(!decimal.TryParse(RateInput.Text.Trim(),NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out rate))
                    throw new ClassRuleException("Enter a numeric hourly rate, for example 50.00.");
                SalaryService.SaveRate(CurrentAccount.Id,teacher,month,rate,RateReason.Text,ViewState["RateVersion"] as byte[]);
                saved=true; LoadSalary(); Show("Hourly rate saved for the selected teacher and month. The salary calculation has been updated.",true);
            }
            catch(ClassRuleException ex) { Show(saved ? "Rate saved. Load the month again to refresh the totals." : ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("SalaryManagement","Could not save or refresh rate.",ex); Show(saved ? "Rate saved. Load the month again to refresh the totals." : "The rate could not be saved. Load the month again and retry.",false); }
        }
        private void Safely(Action action)
        {
            MessagePanel.Visible=false; ResultsPanel.Visible=false;
            try { action(); }
            catch(ClassRuleException ex) { Show(ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("SalaryManagement","Could not load salary.",ex); Show("Salary information could not be loaded. Please try again.",false); }
        }
        private void Show(string message,bool success)
        { MessagePanel.Visible=true; MessagePanel.CssClass=success ? "alert alert-success" : "alert alert-warning"; MessageText.Text=message; }
    }
}
