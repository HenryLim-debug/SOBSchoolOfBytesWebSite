using System;
using System.Data;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class TeacherMaterials : ProtectedPage
    {
        protected override string RequiredRole { get { return "Teacher"; } }
        private int RequestedChapter()
        {
            int chapter;
            if(!int.TryParse(Request.QueryString["chapter"],out chapter) || chapter<=0)
                throw new ClassRuleException("Choose a chapter using Edit Chapter in the list.");
            return chapter;
        }
        protected void Page_Load(object sender,EventArgs e)
        {
            Page.Form.Enctype="multipart/form-data";
            if(IsPostBack) return;
            try
            {
                LoadList();
                if(Request.QueryString["chapter"]!=null) LoadEditor(RequestedChapter());
            }
            catch(ClassRuleException ex) { EditorPanel.Visible=false; Show(ex.Message,false); }
            catch(SqlException ex) { EditorPanel.Visible=false; Trace.Warn("TeacherMaterials","Could not load materials.",ex); Show("Teaching materials could not be loaded. Please try again.",false); }
        }
        private void LoadList()
        {
            DataRow course=MaterialService.TeacherCourse(CurrentAccount.Id);
            CourseTitle.Text=course["Title"].ToString();
            Chapters.DataSource=MaterialService.TeacherChapters(CurrentAccount.Id); Chapters.DataBind();
        }
        private void LoadEditor(int chapterId)
        {
            DataRow chapter=MaterialService.TeacherChapter(CurrentAccount.Id,chapterId);
            ChapterNumber.Text=chapter["ChapterNumber"].ToString(); ChapterTitle.Text=chapter["Title"].ToString();
            Objectives.Text=chapter["LearningObjectives"].ToString();
            LoadDocumentState(chapter,chapterId);
            string video=chapter["YouTubeVideoId"].ToString();
            VideoLink.Text=MaterialService.ValidVideoId(video) ? "https://www.youtube.com/watch?v="+video : "";
            EditorPanel.Visible=true;
        }
        private void LoadDocumentState(DataRow chapter,int chapterId)
        {
            // EditorPanel is still hidden on initial load; derive visibility from data.
            bool hasSlides=chapter["SlidesName"]!=DBNull.Value;
            bool hasTutorial=chapter["TutorialName"]!=DBNull.Value;
            CurrentSlides.Visible=hasSlides;
            CurrentSlides.Text=Server.HtmlEncode(chapter["SlidesName"].ToString());
            CurrentSlides.NavigateUrl="~/ChapterDownload.ashx?chapter="+chapterId+"&kind=slides";
            CurrentTutorial.Visible=hasTutorial;
            CurrentTutorial.Text=Server.HtmlEncode(chapter["TutorialName"].ToString());
            CurrentTutorial.NavigateUrl="~/ChapterDownload.ashx?chapter="+chapterId+"&kind=tutorial";
            DeleteSlidesButton.Visible=hasSlides;
            DeleteTutorialButton.Visible=hasTutorial;
            PublishChapter.Checked=(bool)chapter["IsPublished"];
            ViewState["ChapterVersion"]=(byte[])chapter["RowVersion"];
            ViewState["EditingChapterId"]=chapterId;
        }
        protected void SaveButton_Click(object sender,EventArgs e)
        {
            bool saved=false;
            try
            {
                int chapterId=RequestedChapter();
                if(!(ViewState["EditingChapterId"] is int) || (int)ViewState["EditingChapterId"]!=chapterId)
                    throw new ClassRuleException("Reopen the chapter before saving it.");
                var slides=ChapterUpload.Read(SlidesUpload.PostedFile,true);
                var tutorial=ChapterUpload.Read(TutorialUpload.PostedFile,false);
                MaterialService.SaveChapter(CurrentAccount.Id,chapterId,ViewState["ChapterVersion"] as byte[],ChapterTitle.Text,Objectives.Text,VideoLink.Text,PublishChapter.Checked,
                    slides,tutorial,false,false);
                saved=true;
                LoadList(); LoadEditor(chapterId);
                Show(PublishChapter.Checked ? "Chapter saved and published. Enrolled students can now open it." : "Draft saved. This chapter is hidden from students.",true);
            }
            catch(ClassRuleException ex) { Show(saved ? "Chapter saved, but the editor could not refresh. Reopen the page." : ex.Message+" If you selected files, select them again before retrying.",false); }
            catch(System.IO.IOException ex) { Trace.Warn("TeacherMaterials","Could not read upload.",ex); Show("The document could not be read. Select the file again and retry.",false); }
            catch(SqlException ex) { Trace.Warn("TeacherMaterials","Could not save or refresh chapter.",ex); Show(saved ? "Chapter saved, but the editor could not refresh. Reopen the page." : "The chapter could not be saved. Your text is still here; select any upload files again and retry.",false); }
        }
        protected void DeleteSlidesButton_Click(object sender,EventArgs e) { DeleteDocument(true); }
        protected void DeleteTutorialButton_Click(object sender,EventArgs e) { DeleteDocument(false); }
        private void DeleteDocument(bool slides)
        {
            bool deleted=false;
            bool selectedFiles=SlidesUpload.HasFile || TutorialUpload.HasFile;
            try
            {
                int chapterId=RequestedChapter();
                if(!(ViewState["EditingChapterId"] is int) || (int)ViewState["EditingChapterId"]!=chapterId)
                    throw new ClassRuleException("Reopen the chapter before deleting a file.");
                DataRow result=MaterialService.DeleteDocument(CurrentAccount.Id,chapterId,ViewState["ChapterVersion"] as byte[],slides);
                deleted=true;
                LoadDocumentState(result,chapterId);
                LoadList();
                string message=slides ? "Lecture slides deleted." : "Tutorial document deleted.";
                if(!(bool)result["IsPublished"]) message+=" The chapter is currently a draft.";
                message+=" Other edits on this form have not been saved.";
                if(selectedFiles) message+=" Select any new upload files again before saving.";
                Show(message,true);
            }
            catch(ClassRuleException ex) { Show(deleted ? "File deleted. Reopen the chapter to refresh the list." : ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("TeacherMaterials","Could not delete document or refresh list.",ex); Show(deleted ? "File deleted. Reopen the chapter to refresh the list." : "The file could not be deleted. Please try again.",false); }
        }
        private void Show(string message,bool success)
        { FeedbackPanel.Visible=true; FeedbackPanel.CssClass=success ? "alert alert-success" : "alert alert-warning"; FeedbackMessage.Text=message; }
    }
}
