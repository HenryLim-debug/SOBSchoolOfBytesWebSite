using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class ClassMeetings : ProtectedPage
    {
        protected override string RequiredRole { get { return "Teacher"; } }
        protected override bool IsRoleAllowed(string role) { return role=="Teacher" || role=="President"; }
        protected void Page_Load(object sender,EventArgs e)
        {
            BackLink.HRef=ResolveUrl(AccountAccess.DashboardPath(CurrentAccount.Role));
            if(!IsPostBack) Safely(delegate {
                ClassList.DataSource=MeetingService.Classes(CurrentAccount.Id); ClassList.DataTextField="Label"; ClassList.DataValueField="ClassId"; ClassList.DataBind();
                if(Request.QueryString["id"]!=null)
                {
                    int requested;
                    if(!int.TryParse(Request.QueryString["id"],out requested) || ClassList.Items.FindByValue(requested.ToString(CultureInfo.InvariantCulture))==null)
                        throw new ClassRuleException("This class is unavailable or you do not have access to it.");
                    ClassList.SelectedValue=requested.ToString(CultureInfo.InvariantCulture);
                }
                LoadLink();
            });
        }
        private int ClassId()
        { int value; if(!int.TryParse(ClassList.SelectedValue,out value)) throw new ClassRuleException("No classes are assigned yet. A class group must be created before adding a meeting link."); return value; }
        private void LoadLink()
        {
            EditorPanel.Visible=false;
            int id=ClassId(); DataRow row=MeetingService.Read(CurrentAccount.Id,id,false);
            ClassTitle.Text=row["CourseTitle"]+" — "+row["ClassName"];
            ScheduleText.Text=ClassSchedule.FromRow(row).WeeklyText+" · Malaysia time (UTC+8)";
            bool saved=!row.IsNull("MeetingUrl");
            MeetingUrl.Text=saved ? row["MeetingUrl"].ToString() : "";
            SavedStatus.Text=saved ? "Saved link updated: "+((DateTime)row["UpdatedLocal"]).ToString("dd MMM yyyy HH:mm",CultureInfo.InvariantCulture)+" (Malaysia time)." : "No meeting link is saved for this class.";
            // A missing LEFT JOIN timestamp can arrive as an empty byte array. Use record existence,
            // not the timestamp alone, to distinguish a first save from editing/removing a saved link.
            ViewState["MeetingVersion"]=row.IsNull("MeetingRecordId") ? null : row["RowVersion"];
            ViewState["MeetingClass"]=id;
            OpenLink.NavigateUrl=ResolveUrl("~/JoinClass?id="+id.ToString(CultureInfo.InvariantCulture));
            OpenLink.Visible=saved; RemoveButton.Visible=saved; EditorPanel.Visible=true;
        }
        protected void ClassList_SelectedIndexChanged(object sender,EventArgs e) { Safely(LoadLink); }
        protected void ReloadButton_Click(object sender,EventArgs e) { Safely(LoadLink); }
        protected void SaveButton_Click(object sender,EventArgs e) { Save(false); }
        protected void RemoveButton_Click(object sender,EventArgs e) { Save(true); }
        private void Save(bool remove)
        {
            bool saved=false;
            try
            {
                int id=ClassId();
                if(!(ViewState["MeetingClass"] is int) || (int)ViewState["MeetingClass"]!=id) throw new ClassRuleException("Reload this class before changing its link.");
                MeetingService.Save(CurrentAccount.Id,id,MeetingUrl.Text,ViewState["MeetingVersion"] as byte[],remove);
                saved=true; LoadLink(); Show(remove ? "Meeting link removed. Students can no longer open it from this website." : "Meeting link saved for this class group.",true);
            }
            catch(ClassRuleException ex) { Show(saved ? "Change saved. Reload to see the latest link." : ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("ClassMeetings","Could not save or refresh meeting link.",ex); Show(saved ? "Change saved. Reload to see the latest link." : "The meeting link could not be saved. Reload and try again.",false); }
        }
        private void Safely(Action action)
        {
            MessagePanel.Visible=false; EditorPanel.Visible=false;
            try { action(); }
            catch(ClassRuleException ex) { Show(ex.Message,false); }
            catch(SqlException ex) { Trace.Warn("ClassMeetings","Could not load meeting link.",ex); Show("Meeting links could not be loaded. Please try again.",false); }
        }
        private void Show(string text,bool success)
        { MessagePanel.Visible=true; MessagePanel.CssClass=success ? "alert alert-success" : "alert alert-warning"; MessageText.Text=text; }
    }
}
