using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class ReviewTeachingHours : ProtectedPage
    {
        protected override string RequiredRole { get { return "President"; } }
        private int LogId()
        { int id; if(!int.TryParse(Request.QueryString["id"],out id) || id<=0) throw new ClassRuleException("Choose a teaching record from the list."); return id; }
        protected void Page_Load(object sender,EventArgs e) { if(!IsPostBack) Refresh(); }
        private void LoadList()
        {
            MonthlyHours.DataSource=TeachingHoursService.Monthly(CurrentAccount.Id,true); MonthlyHours.DataBind();
            Records.DataSource=TeachingHoursService.Logs(CurrentAccount.Id,true); Records.DataBind();
        }
        private void LoadReview()
        {
            int id=LogId(); DataRow record=TeachingHoursService.ReviewRecord(CurrentAccount.Id,id);
            RecordHeading.Text=record["TeacherName"]+" — "+record["ClassName"]+" — "+((DateTime)record["SessionStartLocal"]).ToString("dd MMM yyyy HH:mm",CultureInfo.InvariantCulture)+" — "+record["Status"];
            LessonSummary.Text=record["LessonNotes"].ToString(); ReviewNotes.Text="";
            ViewState["ReviewId"]=id; ViewState["ReviewVersion"]=(byte[])record["RowVersion"];
            DecisionPanel.Visible=record["Status"].ToString()=="Pending";
            ReviewHistory.DataSource=TeachingHoursService.ReviewHistory(CurrentAccount.Id,id); ReviewHistory.DataBind();
            ReviewPanel.Visible=true;
        }
        private void Refresh()
        {
            try { LoadList(); if(Request.QueryString["id"]!=null) LoadReview(); MessagePanel.Visible=false; }
            catch(ClassRuleException ex) { ReviewPanel.Visible=false; Show(ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("ReviewTeachingHours","Could not load records.",ex); Show("Teaching records could not be loaded. Please try again.",false); }
        }
        protected void RefreshButton_Click(object sender,EventArgs e) { Refresh(); }
        protected void ApproveButton_Click(object sender,EventArgs e) { Decide(true); }
        protected void RejectButton_Click(object sender,EventArgs e) { Decide(false); }
        private void Decide(bool approve)
        {
            bool saved=false;
            try
            {
                int id=LogId();
                if(!(ViewState["ReviewId"] is int) || (int)ViewState["ReviewId"]!=id) throw new ClassRuleException("Reopen the record before reviewing it.");
                TeachingHoursService.Review(CurrentAccount.Id,id,ViewState["ReviewVersion"] as byte[],approve,ReviewNotes.Text);
                saved=true; LoadList(); LoadReview(); Show(approve ? "Two teaching hours approved." : "Record rejected. The teacher can correct the lesson notes and resubmit.",true);
            }
            catch(ClassRuleException ex) { Show(saved ? "Decision saved. Refresh to see the latest records." : ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("ReviewTeachingHours","Could not review record.",ex); Show(saved ? "Decision saved. Refresh to see the latest records." : "The decision could not be saved. Please try again.",false); }
        }
        private void Show(string text,bool success)
        { MessagePanel.Visible=true; MessagePanel.CssClass=success ? "alert alert-success" : "alert alert-warning"; MessageText.Text=text; }
    }
}
