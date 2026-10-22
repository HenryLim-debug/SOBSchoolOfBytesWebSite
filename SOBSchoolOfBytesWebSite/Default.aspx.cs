using System;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Learning;
namespace SOBSchoolOfBytesWebSite {
 public partial class _Default:System.Web.UI.Page {
  protected void Page_Load(object sender,EventArgs e){
   if(IsPostBack)return;
   try{var rows=ClassService.PublicCourses();CourseCount.Text=rows.Rows.Count.ToString();CourseCards.DataSource=rows;CourseCards.DataBind();
    if(rows.Rows.Count==0){CourseMessage.Text="New courses will appear here when available.";CourseMessage.Visible=true;}}
   catch(SqlException){CourseCount.Text="Our";CourseMessage.Text="The course list is temporarily unavailable. Please try again later.";CourseMessage.Visible=true;}
  }
 }
}
