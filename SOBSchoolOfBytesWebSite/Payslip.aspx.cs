using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using SOBSchoolOfBytesWebSite.Learning;
using SOBSchoolOfBytesWebSite.Security;

namespace SOBSchoolOfBytesWebSite
{
    public partial class Payslip : ProtectedPage
    {
        protected override string RequiredRole { get { return "Teacher"; } }
        protected override bool IsRoleAllowed(string role) { return role=="Teacher" || role=="President"; }
        protected void Page_Load(object sender,EventArgs e)
        {
            Sheet.Visible=false;
            try
            {
                int id;
                if(!int.TryParse(Request.QueryString["id"],out id) || id<=0) throw new ClassRuleException("Choose a payslip from Payslip Records.");
                DataRow row=PayslipService.Read(CurrentAccount.Id,id);
                Reference.Text="SOB-"+id.ToString("D6",CultureInfo.InvariantCulture)+" / Revision "+row["Revision"];
                VersionStatus.Text=row["VersionStatus"].ToString();
                TeacherName.Text=row["TeacherName"].ToString(); StaffId.Text=row["StaffId"].ToString();
                SalaryMonth.Text=((DateTime)row["SalaryMonth"]).ToString("MMMM yyyy",CultureInfo.InvariantCulture);
                IssuedDate.Text=((DateTime)row["IssuedLocal"]).ToString("dd MMM yyyy HH:mm",CultureInfo.InvariantCulture);
                IssuedBy.Text=row["IssuedByName"].ToString(); IssueReason.Text=row["IssueReason"].ToString();
                Hours.Text=((decimal)row["ApprovedHours"]).ToString("N2",CultureInfo.InvariantCulture);
                Rate.Text=((decimal)row["HourlyRate"]).ToString("N2",CultureInfo.InvariantCulture);
                Amount.Text=((decimal)row["GrossAmount"]).ToString("N2",CultureInfo.InvariantCulture);
                PreviousLink.Visible=!row.IsNull("SupersedesId");
                if(!row.IsNull("SupersedesId")) PreviousLink.NavigateUrl="~/Payslip?id="+row["SupersedesId"];
                Lessons.DataSource=PayslipService.Lessons(CurrentAccount.Id,id); Lessons.DataBind();
                Sheet.Visible=true;
            }
            catch(ClassRuleException ex) { ErrorPanel.Visible=true; ErrorMessage.Text=ex.Message; }
            catch(SqlException ex) { Trace.Warn("Payslip","Could not load saved payslip.",ex); ErrorPanel.Visible=true; ErrorMessage.Text="This payslip could not be loaded. Please try again."; }
        }
    }
}
