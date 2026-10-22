using System;
using System.Data;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Security;
using SOBSchoolOfBytesWebSite.Learning;
namespace SOBSchoolOfBytesWebSite
{
    public partial class CourseManagement:ProtectedPage
    {
        protected override string RequiredRole{get{return "President";}}
        protected void Page_Load(object sender,EventArgs e)
        {
            if(IsPostBack)return;
            try{
                var courses=ClassService.ManageCourses(CurrentAccount.Id);CourseGrid.DataSource=courses;CourseGrid.DataBind();
                if(Request.QueryString["saved"]=="1")Show("Course saved. Its public information is now updated.");
                if(Request.QueryString["id"]!=null){
                    int id;if(!int.TryParse(Request.QueryString["id"],out id))throw new ClassRuleException("Choose a course from the list.");
                    var rows=courses.Select("CourseId="+id);if(rows.Length!=1)throw new ClassRuleException("Course not found.");DataRow r=rows[0];
                    ViewState["CourseId"]=id;ViewState["Version"]=r["VersionKey"].ToString();EditorTitle.Text="Edit Course";
                    CourseTitle.Text=r["Title"].ToString();Description.Text=r["Description"].ToString();Outcomes.Text=r["LearningOutcomes"].ToString();
                    SampleLesson.Text=r["SampleLesson"].ToString();SampleActivity.Text=r["SampleActivity"].ToString();Available.Checked=(bool)r["IsAvailable"];
                }
            }catch(ClassRuleException ex){SaveButton.Enabled=false;Show(ex.Message);}catch(SqlException){SaveButton.Enabled=false;Show("Courses could not be loaded. Please try again.");}
        }
        protected void SaveButton_Click(object sender,EventArgs e)
        {
            try{
                int? id=ViewState["CourseId"] as int?;
                int saved=ClassService.SaveCourse(CurrentAccount.Id,id,ViewState["Version"] as string,CourseTitle.Text,Description.Text,Outcomes.Text,SampleLesson.Text,SampleActivity.Text,Available.Checked);
                Response.Redirect("~/CourseManagement?id="+saved+"&saved=1",false);Context.ApplicationInstance.CompleteRequest();
            }catch(ClassRuleException ex){Show(ex.Message);}catch(SqlException ex){Show(ex.Number==2601||ex.Number==2627?"A course with that title already exists.":"The course could not be saved. Please try again.");}
        }
        private void Show(string text){Message.Text=Server.HtmlEncode(text);Message.Visible=true;}
    }
}
