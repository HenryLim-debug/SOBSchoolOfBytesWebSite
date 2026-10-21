using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Security;
using SOBSchoolOfBytesWebSite.Learning;

namespace SOBSchoolOfBytesWebSite
{
    public partial class ClassManagement : ProtectedPage
    {
        protected override string RequiredRole { get { return "President"; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            for (int day=1; day<=7; day++)
            {
                string label = CultureInfo.InvariantCulture.DateTimeFormat.GetDayName((DayOfWeek)(day%7));
                DayOne.Items.Add(new ListItem(label, day.ToString(CultureInfo.InvariantCulture)));
                DayTwo.Items.Add(new ListItem(label, day.ToString(CultureInfo.InvariantCulture)));
            }
            DayOne.SelectedValue="1"; DayTwo.SelectedValue="3";
            DateTime today=ClassService.SchoolNow.Date;
            int offset=((int)DayOfWeek.Monday-(int)today.DayOfWeek+7)%7;
            FirstMonday.Text=today.AddDays(offset==0?7:offset).ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);
            LoadTeachers(); LoadClasses();
        }
        private void LoadTeachers()
        {
            try
            {
                TeacherList.DataSource=ClassService.AvailableTeachers(); TeacherList.DataTextField="Label";
                TeacherList.DataValueField="Id"; TeacherList.DataBind(); TeacherList.Items.Insert(0,new ListItem("Choose a teacher and course",""));
                NoTeachersPanel.Visible=TeacherList.Items.Count==1; CreateButton.Enabled=!NoTeachersPanel.Visible;
            }
            catch(SqlException exception){Trace.Warn("ClassManagement","Could not load teachers.",exception);Show("Teachers could not be loaded. Reload and try again.",false);CreateButton.Enabled=false;}
        }
        private void LoadClasses()
        {
            try {Classes.DataSource=ClassService.AllClasses();Classes.DataBind();}
            catch(SqlException exception){Trace.Warn("ClassManagement","Could not load classes.",exception);Show("The class list could not be loaded. Please try again.",false);}
        }
        protected void CreateButton_Click(object sender,EventArgs e)
        {
            FeedbackPanel.Visible=false; Page.Validate("CreateClass"); if(!Page.IsValid)return;
            int teacher,day1,day2; DateTime monday; TimeSpan time1,time2;
            if(!int.TryParse(TeacherList.SelectedValue,out teacher)||!int.TryParse(DayOne.SelectedValue,out day1)||!int.TryParse(DayTwo.SelectedValue,out day2)
                ||!DateTime.TryParseExact(FirstMonday.Text,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out monday)
                ||!TimeSpan.TryParseExact(TimeOne.Text,@"hh\:mm",CultureInfo.InvariantCulture,out time1)
                ||!TimeSpan.TryParseExact(TimeTwo.Text,@"hh\:mm",CultureInfo.InvariantCulture,out time2))
            {Show("Choose valid class dates, session days, times and a teacher.",false);return;}
            try
            {
                ClassService.CreateClass(CurrentAccount.Id,teacher,ClassName.Text,new ClassSchedule{FirstMonday=monday,DayOne=day1,TimeOne=time1,DayTwo=day2,TimeTwo=time2});
                ClassName.Text="";LoadClasses();Show("Class created. Students can now apply for this class.",true);
            }
            catch(ClassRuleException exception){Show(exception.Message,false);}
            catch(SqlException exception)
            {
                Trace.Warn("ClassManagement","Could not create a class.",exception);
                Show(exception.Number==2601||exception.Number==2627?"A class with this name and course already exists for that starting week.":"The class could not be saved. Please try again.",false);
            }
        }
        protected void Classes_RowDataBound(object sender,GridViewRowEventArgs e)
        {
            if(e.Row.RowType!=DataControlRowType.DataRow)return;
            Button button=(Button)e.Row.FindControl("ToggleButton");button.CommandArgument=e.Row.RowIndex.ToString(CultureInfo.InvariantCulture);
            button.Text=(bool)System.Web.UI.DataBinder.Eval(e.Row.DataItem,"IsOpen")?"Close Enrolment":"Open Enrolment";
        }
        protected void Classes_RowCommand(object sender,GridViewCommandEventArgs e)
        {
            int index;if(e.CommandName!="ToggleClass"||!int.TryParse(e.CommandArgument.ToString(),out index)||index<0||index>=Classes.DataKeys.Count)return;
            try
            {
                bool wasOpen=(bool)Classes.DataKeys[index].Values["IsOpen"];
                ClassService.SetOpen(CurrentAccount.Id,(int)Classes.DataKeys[index].Values["ClassId"],wasOpen);
                LoadClasses();Show(wasOpen?"Enrolment closed. Existing students keep their places.":"Enrolment opened.",true);
            }
            catch(ClassRuleException exception){Show(exception.Message,false);LoadClasses();}
            catch(SqlException exception){Trace.Warn("ClassManagement","Could not update enrolment.",exception);Show("The class could not be updated. Please try again.",false);}
        }
        protected void RefreshButton_Click(object sender,EventArgs e){FeedbackPanel.Visible=false;LoadClasses();}
        private void Show(string message,bool success){FeedbackPanel.Visible=true;FeedbackPanel.CssClass=success?"alert alert-success":"alert alert-warning";FeedbackMessage.Text=message;}
    }
}
