using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class TeacherSubmissions : ProtectedPage
    {
        protected override string RequiredRole { get { return "Teacher"; } }
        private int SubmissionId()
        { int id; if(!int.TryParse(Request.QueryString["id"],out id) || id<=0) throw new ClassRuleException("Choose Review beside a student submission."); return id; }
        protected void Page_Load(object sender,EventArgs e)
        {
            if(IsPostBack) return;
            try { LoadList(); if(Request.QueryString["id"]!=null) LoadReview(); }
            catch(ClassRuleException ex) { ReviewPanel.Visible=false; Show(ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("TeacherSubmissions","Could not load reviews.",ex); Show("Submissions could not be loaded. Please try again.",false); }
        }
        private void LoadList() { Submissions.DataSource=SubmissionService.TeacherList(CurrentAccount.Id); Submissions.DataBind(); }
        private void LoadReview()
        {
            int id=SubmissionId(); DataRow row=SubmissionService.TeacherSubmission(CurrentAccount.Id,id);
            ReviewHeading.Text=row["StudentName"]+" ("+row["StudentUsername"]+") — "+row["ClassName"]+" — Chapter "+row["ChapterNumber"]+" — Submission #"+id;
            DownloadWork.NavigateUrl="~/SubmissionDownload.ashx?id="+id;
            MarkInput.Text=row["Mark"]==DBNull.Value ? "" : ((decimal)row["Mark"]).ToString("0.##",CultureInfo.InvariantCulture);
            FeedbackInput.Text=row["Feedback"].ToString();
            ViewState["ReviewId"]=id; ViewState["ReviewVersion"]=(byte[])row["RowVersion"]; ReviewPanel.Visible=true;
        }
        protected void SaveReviewButton_Click(object sender,EventArgs e)
        {
            bool saved=false;
            try
            {
                int id=SubmissionId(); decimal mark;
                if(!(ViewState["ReviewId"] is int) || (int)ViewState["ReviewId"]!=id) throw new ClassRuleException("Reopen the submission before saving.");
                if(!decimal.TryParse(MarkInput.Text,NumberStyles.AllowDecimalPoint|NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out mark))
                    throw new ClassRuleException("Enter a number from 0 to 100 for the mark.");
                SubmissionService.Grade(CurrentAccount.Id,id,ViewState["ReviewVersion"] as byte[],mark,FeedbackInput.Text);
                saved=true; LoadList(); LoadReview(); Show("Mark and feedback saved. The student can now see the result.",true);
            }
            catch(ClassRuleException ex) { Show(saved ? "Review saved. Reopen the page to refresh it." : ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("TeacherSubmissions","Could not save review.",ex); Show(saved ? "Review saved. Reopen the page to refresh it." : "The review could not be saved. Your text is still here; please try again.",false); }
        }
        protected void Submissions_PageIndexChanging(object sender,GridViewPageEventArgs e)
        {
            try { Submissions.PageIndex=e.NewPageIndex; LoadList(); }
            catch(SqlException ex) { Trace.Warn("TeacherSubmissions","Could not load list.",ex); Show("The list could not be loaded. Please try again.",false); }
        }
        private void Show(string text,bool success)
        { MessagePanel.Visible=true; MessagePanel.CssClass=success ? "alert alert-success" : "alert alert-warning"; MessageText.Text=text; }
    }
}
