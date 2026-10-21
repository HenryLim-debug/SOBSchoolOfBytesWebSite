using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class Attendance : ProtectedPage
    {
        protected override string RequiredRole { get { return "Teacher"; } }
        protected override bool IsRoleAllowed(string role) { return role=="Teacher" || role=="President"; }
        protected void Page_Load(object sender,EventArgs e)
        {
            BackLink.HRef=ResolveUrl(AccountAccess.DashboardPath(CurrentAccount.Role));
            ReadOnlyPanel.Visible=CurrentAccount.Role=="President";
            if(!IsPostBack) Safely(delegate {
                ClassList.DataSource=AttendanceService.Classes(CurrentAccount.Id); ClassList.DataTextField="Label"; ClassList.DataValueField="ClassId"; ClassList.DataBind();
                LoadSessions();
            });
        }
        private int ClassId()
        { int id; if(!int.TryParse(ClassList.SelectedValue,out id)) throw new ClassRuleException("There are no class groups available for your account."); return id; }
        private DateTime LessonStart()
        {
            DateTime value;
            if(!DateTime.TryParseExact(SessionList.SelectedValue,"yyyy-MM-ddTHH:mm:ss",CultureInfo.InvariantCulture,DateTimeStyles.None,out value))
                throw new ClassRuleException("Choose a completed lesson.");
            return value;
        }
        private void LoadSessions()
        {
            SessionList.Items.Clear(); RosterPanel.Visible=false;
            if(ClassList.Items.Count>0)
            {
                SessionList.DataSource=AttendanceService.Sessions(CurrentAccount.Id,ClassId());
                SessionList.DataTextField="Label"; SessionList.DataValueField="Value"; SessionList.DataBind();
            }
            bool available=SessionList.Items.Count>0;
            NoLessonsPanel.Visible=!available;
            if(available) LoadRoster();
        }
        private void LoadRoster()
        {
            int id=ClassId(); DateTime start=LessonStart();
            DataTable roster=AttendanceService.Roster(CurrentAccount.Id,id,start);
            Students.DataSource=roster; Students.DataBind();
            ViewState["AttendanceClass"]=id; ViewState["AttendanceStart"]=start;
            RosterPanel.Visible=true; NoLessonsPanel.Visible=false;
            SaveButton.Visible=CurrentAccount.Role=="Teacher" && roster.Rows.Count>0;
        }
        protected void ClassList_SelectedIndexChanged(object sender,EventArgs e) { Safely(LoadSessions); }
        protected void SessionList_SelectedIndexChanged(object sender,EventArgs e) { Safely(LoadRoster); }
        protected void ReloadButton_Click(object sender,EventArgs e)
        { Safely(delegate { if(SessionList.Items.Count==0) LoadSessions(); else LoadRoster(); }); }
        protected void Students_RowDataBound(object sender,GridViewRowEventArgs e)
        {
            if(e.Row.RowType!=DataControlRowType.DataRow) return;
            DataRowView row=(DataRowView)e.Row.DataItem;
            var status=(DropDownList)e.Row.FindControl("StatusList");
            var note=(TextBox)e.Row.FindControl("NoteInput");
            status.SelectedValue=row["AttendanceStatus"].ToString(); note.Text=row["Note"].ToString();
            status.Enabled=CurrentAccount.Role=="Teacher"; note.ReadOnly=CurrentAccount.Role!="Teacher";
        }
        protected void SaveButton_Click(object sender,EventArgs e)
        {
            bool saved=false;
            try
            {
                if(CurrentAccount.Role!="Teacher") throw new ClassRuleException("Only the assigned teacher can record attendance.");
                int id=ClassId(); DateTime start=LessonStart();
                if(!(ViewState["AttendanceClass"] is int) || (int)ViewState["AttendanceClass"]!=id ||
                    !(ViewState["AttendanceStart"] is DateTime) || (DateTime)ViewState["AttendanceStart"]!=start)
                    throw new ClassRuleException("Reload the selected lesson before saving attendance.");
                var entries=new List<AttendanceEntry>();
                foreach(GridViewRow row in Students.Rows)
                    entries.Add(new AttendanceEntry { EnrolmentId=(int)Students.DataKeys[row.RowIndex].Values["EnrolmentId"],
                        VersionToken=Students.DataKeys[row.RowIndex].Values["VersionToken"].ToString(),
                        Status=((DropDownList)row.FindControl("StatusList")).SelectedValue,Note=((TextBox)row.FindControl("NoteInput")).Text });
                AttendanceService.Save(CurrentAccount.Id,id,start,entries);
                saved=true; LoadRoster(); Show("Attendance saved. Students can now see their own records.",true);
            }
            catch(ClassRuleException ex) { Show(saved ? "Attendance saved. Reload to see the latest records." : ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("Attendance","Could not save or refresh attendance.",ex); Show(saved ? "Attendance saved. Reload to see the latest records." : "Attendance could not be saved. Reload the lesson and try again.",false); }
        }
        private void Safely(Action action)
        {
            MessagePanel.Visible=false; RosterPanel.Visible=false;
            try { action(); }
            catch(ClassRuleException ex) { Show(ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("Attendance","Could not load attendance.",ex); Show("Attendance could not be loaded. Please try again.",false); }
        }
        private void Show(string message,bool success)
        { MessagePanel.Visible=true; MessagePanel.CssClass=success ? "alert alert-success" : "alert alert-warning"; MessageText.Text=message; }
    }
}
