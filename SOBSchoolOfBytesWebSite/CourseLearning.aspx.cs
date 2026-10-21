using System;
using System.Data;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class CourseLearning : ProtectedPage
    {
        protected override string RequiredRole { get { return "Student"; } }
        protected void Page_Load(object sender,EventArgs e)
        {
            // Recheck enrolment and publication on every request, including a stale bookmarked link.
            CoursePanel.Visible=false; LessonPanel.Visible=false;
            try
            {
                int courseId;
                if(!int.TryParse(Request.QueryString["course"],out courseId) || courseId<=0)
                    throw new ClassRuleException("Choose Learning Materials from a course on your dashboard.");
                DataRow course=MaterialService.StudentCourse(CurrentAccount.Id,courseId);
                CourseTitle.Text=course["Title"].ToString(); CourseDescription.Text=course["Description"].ToString();
                Chapters.DataSource=MaterialService.PublishedChapters(CurrentAccount.Id,courseId); Chapters.DataBind();
                CoursePanel.Visible=true;
                if(Request.QueryString["chapter"]==null) return;
                int chapterId;
                if(!int.TryParse(Request.QueryString["chapter"],out chapterId) || chapterId<=0)
                    throw new ClassRuleException("Choose a published chapter from the list.");
                DataRow chapter=MaterialService.StudentChapter(CurrentAccount.Id,courseId,chapterId);
                ChapterNumber.Text=chapter["ChapterNumber"].ToString(); LessonTitle.Text=chapter["Title"].ToString();
                SubmitWorkLink.NavigateUrl="~/StudentSubmission?chapter="+chapterId;
                Objectives.Text=chapter["LearningObjectives"].ToString(); Tutorial.Text=chapter["LessonText"].ToString(); Practice.Text=chapter["PracticeTask"].ToString();
                ObjectivesPanel.Visible=!string.IsNullOrWhiteSpace(Objectives.Text);
                TutorialPanel.Visible=!string.IsNullOrWhiteSpace(Tutorial.Text);
                PracticePanel.Visible=!string.IsNullOrWhiteSpace(Practice.Text);
                // A control's Visible getter is false while its parent panel is hidden.
                // Use the saved data to decide what to render before showing the lesson.
                bool hasSlides=chapter["SlidesName"]!=DBNull.Value;
                bool hasTutorial=chapter["TutorialName"]!=DBNull.Value;
                DownloadSlides.Visible=hasSlides;
                DownloadTutorial.Visible=hasTutorial;
                DocumentsPanel.Visible=hasSlides || hasTutorial;
                DownloadSlides.NavigateUrl="~/ChapterDownload.ashx?chapter="+chapterId+"&kind=slides";
                DownloadTutorial.NavigateUrl="~/ChapterDownload.ashx?chapter="+chapterId+"&kind=tutorial";
                string videoId=chapter["YouTubeVideoId"].ToString();
                bool hasVideo=MaterialService.ValidVideoId(videoId);
                VideoPanel.Visible=hasVideo;
                if(hasVideo)
                {
                    // Only an 11-character YouTube ID can reach this fixed iframe markup.
                    VideoEmbed.Text="<iframe title=\"Lesson video\" src=\"https://www.youtube-nocookie.com/embed/"+videoId+"\" loading=\"lazy\" referrerpolicy=\"strict-origin-when-cross-origin\" allowfullscreen></iframe>";
                    WatchVideo.NavigateUrl="https://www.youtube.com/watch?v="+videoId;
                }
                LessonPanel.Visible=true;
                bool completed=ProgressService.IsCompleted(CurrentAccount.Id,chapterId);
                CompletionStatus.Text=completed ? "You have marked this chapter complete." : "You have not marked this chapter complete yet.";
                CompleteButton.Visible=!completed; UndoCompleteButton.Visible=completed;
            }
            catch(ClassRuleException ex) { FeedbackPanel.Visible=true; FeedbackMessage.Text=ex.Message; }
            catch(SqlException ex) { CoursePanel.Visible=false; Trace.Warn("CourseLearning","Could not load learning materials.",ex); FeedbackPanel.Visible=true; FeedbackMessage.Text="Learning materials could not be loaded. Please try again later."; }
        }
        protected void Completion_Command(object sender,System.Web.UI.WebControls.CommandEventArgs e)
        {
            try
            {
                int courseId,chapterId;
                if(!int.TryParse(Request.QueryString["course"],out courseId) || !int.TryParse(Request.QueryString["chapter"],out chapterId))
                    throw new ClassRuleException("Reopen the chapter from your dashboard.");
                string action=Convert.ToString(e.CommandArgument);
                if(action!="complete" && action!="undo") throw new ClassRuleException("Reopen the chapter before changing progress.");
                MaterialService.StudentChapter(CurrentAccount.Id,courseId,chapterId);
                ProgressService.SetCompleted(CurrentAccount.Id,chapterId,action=="complete");
                Response.Redirect(ResolveUrl("~/CourseLearning?course="+courseId+"&chapter="+chapterId+"#lesson"),false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch(ClassRuleException ex) { FeedbackPanel.Visible=true; FeedbackMessage.Text=ex.Message; }
            catch(SqlException ex) { Trace.Warn("CourseLearning","Could not update progress.",ex); FeedbackPanel.Visible=true; FeedbackMessage.Text="Progress could not be saved. Please reload the lesson and try again."; }
        }
    }
}
