using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class TeachingHours : ProtectedPage
    {
        protected override string RequiredRole { get { return "Teacher"; } }
        protected void Page_Load(object sender,EventArgs e) { if(!IsPostBack) Safely(LoadAll); }
        private int SelectedClass()
        { int id; if(!int.TryParse(ClassList.SelectedValue,out id)) throw new ClassRuleException("Select one of your classes."); return id; }
        private DateTime SelectedStart()
        {
            DateTime value;
            if(!DateTime.TryParseExact(SessionList.SelectedValue,"yyyy-MM-ddTHH:mm:ss",CultureInfo.InvariantCulture,DateTimeStyles.None,out value))
                throw new ClassRuleException("Choose a completed lesson from the list.");
            return value;
        }
        private void LoadAll()
        {
            ClassList.DataSource=TeachingHoursService.Classes(CurrentAccount.Id); ClassList.DataTextField="Label"; ClassList.DataValueField="ClassId"; ClassList.DataBind();
            LoadSessions(); LoadRecords();
        }
        private void LoadRecords()
        {
            MonthlyHours.DataSource=TeachingHoursService.Monthly(CurrentAccount.Id,false); MonthlyHours.DataBind();
            Records.DataSource=TeachingHoursService.Logs(CurrentAccount.Id,false); Records.DataBind();
        }
        private void LoadSessions()
        {
            SessionList.Items.Clear(); ViewState.Remove("EntryVersion"); LessonNotes.Text=""; PreviousReason.Text="";
            if(ClassList.Items.Count>0)
            {
                SessionList.DataSource=TeachingHoursService.Sessions(CurrentAccount.Id,SelectedClass());
                SessionList.DataTextField="Label"; SessionList.DataValueField="Value"; SessionList.DataBind();
            }
            bool available=SessionList.Items.Count>0;
            EntryPanel.Visible=available; NoSessionsPanel.Visible=!available;
            if(available) LoadEntry();
        }
        private void LoadEntry()
        {
            DataRow existing=TeachingHoursService.Entry(CurrentAccount.Id,SelectedClass(),SelectedStart());
            ViewState["EntryVersion"]=existing==null ? null : existing["RowVersion"];
            LessonNotes.Text=existing==null ? "" : existing["LessonNotes"].ToString();
            PreviousReason.Text=existing==null ? "" : "Previous rejection: "+existing["ReviewNotes"];
        }
        protected void RefreshButton_Click(object sender,EventArgs e) { Safely(LoadAll); }
        protected void ClassList_SelectedIndexChanged(object sender,EventArgs e) { Safely(LoadSessions); }
        protected void SessionList_SelectedIndexChanged(object sender,EventArgs e) { Safely(LoadEntry); }
        protected void SubmitButton_Click(object sender,EventArgs e)
        {
            bool saved=false;
            try
            {
                TeachingHoursService.Submit(CurrentAccount.Id,SelectedClass(),SelectedStart(),LessonNotes.Text,ViewState["EntryVersion"] as byte[]);
                saved=true; LoadSessions(); LoadRecords(); Show("Two teaching hours submitted for the president's approval.",true);
            }
            catch(ClassRuleException ex) { Show(saved ? "Record submitted. Refresh to see the latest records." : ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("TeachingHours","Could not submit or refresh.",ex); Show(saved ? "Record submitted. Refresh to see the latest records." : "The record could not be saved. Please try again.",false); }
        }
        private void Safely(Action action)
        {
            try { action(); MessagePanel.Visible=false; }
            catch(ClassRuleException ex) { EntryPanel.Visible=false; Show(ex.Message,false); }
            catch(SqlException ex) { EntryPanel.Visible=false; Trace.Warn("TeachingHours","Could not load records.",ex); Show("Teaching hours could not be loaded. Please try again.",false); }
        }
        private void Show(string text,bool success)
        { MessagePanel.Visible=true; MessagePanel.CssClass=success ? "alert alert-success" : "alert alert-warning"; MessageText.Text=text; }
    }
}
