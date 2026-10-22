using System;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Learning;
namespace SOBSchoolOfBytesWebSite
{
    public partial class CoursePreview:System.Web.UI.Page
    {
        protected void Page_Load(object sender,EventArgs e)
        {
            if(IsPostBack)return;
            try{
                int id;if(!int.TryParse(Request.QueryString["id"],out id)){Unavailable();return;}
                var rows=ClassService.PublicCourses().Select("CourseId="+id);if(rows.Length!=1){Unavailable();return;}var row=rows[0];
                CourseTitle.Text=row["Title"].ToString();Title=CourseTitle.Text;Description.Text=row["Description"].ToString();Outcomes.Text=row["LearningOutcomes"].ToString();
                Sample.Text=string.IsNullOrWhiteSpace(row["SampleLesson"].ToString())?"A sample lesson will be added soon.":row["SampleLesson"].ToString();
                Activity.Text=string.IsNullOrWhiteSpace(row["SampleActivity"].ToString())?"Ask your teacher about the activities in this course.":row["SampleActivity"].ToString();PreviewPanel.Visible=true;
            }catch(SqlException){Message.Text="This preview could not be loaded. Please try again later.";Message.Visible=true;}
        }
        private void Unavailable(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;Message.Text="This course is not currently available for public browsing.";Message.Visible=true;}
    }
}
