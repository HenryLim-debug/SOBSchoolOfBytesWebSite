using System;
using System.Data.SqlClient;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class StudentAttendance : ProtectedPage
    {
        protected override string RequiredRole { get { return "Student"; } }
        protected void Page_Load(object sender,EventArgs e) { if(!IsPostBack) LoadRecords(); }
        protected void RefreshButton_Click(object sender,EventArgs e) { LoadRecords(); }
        private void LoadRecords()
        {
            ErrorPanel.Visible=false; ResultsPanel.Visible=false;
            try
            {
                Summary.DataSource=AttendanceService.StudentSummary(CurrentAccount.Id); Summary.DataBind();
                History.DataSource=AttendanceService.StudentHistory(CurrentAccount.Id); History.DataBind();
                ResultsPanel.Visible=true;
            }
            catch(SqlException ex) { Trace.Warn("StudentAttendance","Could not load attendance.",ex); ErrorPanel.Visible=true; }
        }
    }
}
