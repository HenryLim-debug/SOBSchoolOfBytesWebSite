using System;
using System.Data;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class StudentSubmission : ProtectedPage
    {
        protected override string RequiredRole { get { return "Student"; } }
        private int ChapterId()
        { int id; if(!int.TryParse(Request.QueryString["chapter"],out id) || id<=0) throw new ClassRuleException("Open Submit Work from a published lesson."); return id; }
        protected void Page_Load(object sender,EventArgs e)
        {
            Page.Form.Enctype="multipart/form-data";
            try
            {
                LoadHistory();
                if(!IsPostBack) ViewState["SubmissionRequest"]=Guid.NewGuid();
            }
            catch(ClassRuleException ex) { WorkPanel.Visible=false; Show(ex.Message,false); }
            catch(SqlException ex) { WorkPanel.Visible=false; Trace.Warn("StudentSubmission","Could not load work.",ex); Show("Submissions could not be loaded. Please try again.",false); }
        }
        private void LoadHistory()
        {
            int chapterId=ChapterId(); DataRow chapter=SubmissionService.StudentChapter(CurrentAccount.Id,chapterId);
            ChapterHeading.Text=chapter["CourseTitle"]+" — Chapter "+chapter["ChapterNumber"]+": "+chapter["Title"];
            ClassHeading.Text="Class: "+chapter["ClassName"];
            BackToLesson.NavigateUrl="~/CourseLearning?course="+chapter["CourseId"]+"&chapter="+chapterId+"#lesson";
            History.DataSource=SubmissionService.StudentHistory(CurrentAccount.Id,chapterId); History.DataBind();
            WorkPanel.Visible=true;
        }
        protected void SubmitButton_Click(object sender,EventArgs e)
        {
            bool saved=false;
            try
            {
                if(!(ViewState["SubmissionRequest"] is Guid)) throw new ClassRuleException("Reopen the lesson before submitting.");
                SubmissionService.Submit(CurrentAccount.Id,ChapterId(),(Guid)ViewState["SubmissionRequest"],SubmissionFile.Read(WorkUpload.PostedFile));
                saved=true; ViewState["SubmissionRequest"]=Guid.NewGuid();
                LoadHistory(); Show("Work submitted. Your class teacher can now review it.",true);
            }
            catch(ClassRuleException ex) { Show(saved ? "Work submitted. Refresh to see your submission history." : ex.Message+" Select your file again before retrying.",false); }
            catch(System.IO.IOException ex) { Trace.Warn("StudentSubmission","Could not read file.",ex); Show("The file could not be read. Select it again and retry.",false); }
            catch(SqlException ex) { Trace.Warn("StudentSubmission","Could not submit or refresh.",ex); Show(saved ? "Work submitted. Refresh to see your history." : "Work could not be submitted. Select the file again and retry.",false); }
        }
        private void Show(string text,bool success)
        { MessagePanel.Visible=true; MessagePanel.CssClass=success ? "alert alert-success" : "alert alert-warning"; MessageText.Text=text; }
    }
}
