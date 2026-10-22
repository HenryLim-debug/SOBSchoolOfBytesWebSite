using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Security;
using SOBSchoolOfBytesWebSite.Learning;
namespace SOBSchoolOfBytesWebSite
{
    public partial class ClassEdit:ProtectedPage
    {
        protected override string RequiredRole{get{return "President";}}
        protected void Page_Load(object sender,EventArgs e)
        {
            if(IsPostBack)return;
            try{
                int id;if(!int.TryParse(Request.QueryString["id"],out id))throw new ClassRuleException("Choose Edit Class from Manage Classes.");
                var row=ClassService.StaffClass(CurrentAccount.Id,id);ViewState["ClassId"]=id;
                ViewState["Version"]=BitConverter.ToString((byte[])row["RowVersion"]).Replace("-","");
                CourseTitle.Text=row["CourseTitle"].ToString();ClassName.Text=row["ClassName"].ToString();
                TeacherList.DataSource=ClassService.ClassEditTeachers((int)row["CourseId"]);TeacherList.DataTextField="Label";TeacherList.DataValueField="Id";TeacherList.DataBind();
                TeacherList.Items.Insert(0,new ListItem("Choose an active teacher for this course",""));
                if(TeacherList.Items.FindByValue(row["TeacherId"].ToString())!=null)TeacherList.SelectedValue=row["TeacherId"].ToString();
                for(int d=1;d<=7;d++){string label=CultureInfo.InvariantCulture.DateTimeFormat.GetDayName((DayOfWeek)(d%7));DayOne.Items.Add(new ListItem(label,d.ToString()));DayTwo.Items.Add(new ListItem(label,d.ToString()));}
                var schedule=ClassSchedule.FromRow(row);FirstMonday.Text=schedule.FirstMonday.ToString("yyyy-MM-dd");DayOne.SelectedValue=schedule.DayOne.ToString();DayTwo.SelectedValue=schedule.DayTwo.ToString();
                TimeOne.Text=schedule.TimeOne.ToString(@"hh\:mm");TimeTwo.Text=schedule.TimeTwo.ToString(@"hh\:mm");EditorPanel.Visible=true;
                SaveButton.Enabled=schedule.FirstLesson>ClassService.SchoolNow;
                if(!SaveButton.Enabled)Show("This class has already started. Its teacher and timetable are protected to preserve lesson history.");
                else if(Request.QueryString["saved"]=="1")Show("Class updated. Student and teacher timetables now use these details.");
                History.DataSource=ClassService.ClassHistory(CurrentAccount.Id,id);History.DataBind();
            }catch(ClassRuleException ex){Show(ex.Message);}catch(SqlException){Show("Class details could not be loaded. Please try again.");}
        }
        protected void SaveButton_Click(object sender,EventArgs e)
        {
            try{
                int teacher,d1,d2;DateTime monday;TimeSpan t1,t2;
                if(!(ViewState["ClassId"] is int)||!int.TryParse(TeacherList.SelectedValue,out teacher)||!int.TryParse(DayOne.SelectedValue,out d1)||!int.TryParse(DayTwo.SelectedValue,out d2)
                    ||!DateTime.TryParseExact(FirstMonday.Text,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out monday)
                    ||!TimeSpan.TryParseExact(TimeOne.Text,@"hh\:mm",CultureInfo.InvariantCulture,out t1)||!TimeSpan.TryParseExact(TimeTwo.Text,@"hh\:mm",CultureInfo.InvariantCulture,out t2))throw new ClassRuleException("Choose a valid teacher, Monday date, days and times.");
                int id=(int)ViewState["ClassId"];
                ClassService.EditClass(CurrentAccount.Id,id,ViewState["Version"] as string,teacher,ClassName.Text,new ClassSchedule{FirstMonday=monday,DayOne=d1,TimeOne=t1,DayTwo=d2,TimeTwo=t2},Reason.Text);
                Response.Redirect("~/ClassEdit?id="+id+"&saved=1",false);Context.ApplicationInstance.CompleteRequest();
            }catch(ClassRuleException ex){Show(ex.Message);}catch(SqlException ex){Show(ex.Number==2601||ex.Number==2627?"A class with this name already exists for that course and starting week.":"The class could not be changed. Please try again.");}
        }
        private void Show(string text){Message.Text=Server.HtmlEncode(text);Message.Visible=true;}
    }
}
