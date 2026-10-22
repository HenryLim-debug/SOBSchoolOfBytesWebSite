using System;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Learning;
namespace SOBSchoolOfBytesWebSite
{
    public partial class CourseCatalogue:System.Web.UI.Page
    {
        protected void Page_Load(object sender,EventArgs e)
        {
            if(IsPostBack)return;
            try{var rows=ClassService.PublicCourses();CourseCards.DataSource=rows;CourseCards.DataBind();if(rows.Rows.Count==0){Message.Text="New courses will appear here when available.";Message.Visible=true;}}
            catch(SqlException){Message.Text="Courses could not be loaded. Please try again later.";Message.Visible=true;}
        }
    }
}
