<%@ Page Title="Payslip Records" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Payslips.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.Payslips" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <a ID="BackLink" runat="server" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
    <h1>Payslip Records</h1>
    <p>Issued payslips keep a saved copy of the approved teaching hours and monthly hourly rate. All amounts are in Malaysian Ringgit (RM).</p>
    <p>A current-month payslip includes only hours approved when it is issued. Later approvals or rate changes can be included in a new revision. Previous versions remain available; use the latest revision for that teacher and month.</p>
    <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="MessageText" runat="server" Mode="Encode" /></asp:Panel>
    <asp:Panel ID="IssuePanel" runat="server" Visible="false" CssClass="card p-4 mb-4">
        <h2 class="h4">Prepare a Payslip</h2>
        <p>Set the monthly rate and review teaching hours first. Pending and rejected hours are excluded.</p>
        <div class="d-flex flex-wrap gap-2 mb-3">
            <a runat="server" href="~/SalaryManagement" class="btn btn-outline-primary">Teacher Salaries</a>
            <a runat="server" href="~/ReviewTeachingHours" class="btn btn-outline-primary">Review Teaching Hours</a>
        </div>
        <asp:Label runat="server" AssociatedControlID="TeacherList" Text="Teacher" CssClass="form-label" />
        <asp:DropDownList ID="TeacherList" runat="server" CssClass="form-select mb-3" />
        <asp:Label runat="server" AssociatedControlID="MonthInput" Text="Salary month" CssClass="form-label" />
        <asp:TextBox ID="MonthInput" runat="server" TextMode="Month" CssClass="form-control mb-3" />
        <asp:Button ID="PreviewButton" runat="server" Text="Preview Payslip" OnClick="PreviewButton_Click" CausesValidation="false" CssClass="btn btn-primary align-self-start" />
        <asp:Panel ID="PreviewPanel" runat="server" Visible="false" CssClass="border-top mt-4 pt-3">
            <h3 class="h5"><asp:Literal ID="PreviewHeading" runat="server" Mode="Encode" /></h3>
            <p><asp:Literal ID="PreviewTotals" runat="server" Mode="Encode" /></p>
            <p role="status"><asp:Literal ID="PreviewNotice" runat="server" Mode="Encode" /></p>
            <div class="table-responsive">
                <asp:GridView ID="PreviewLessons" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered" GridLines="None" EmptyDataText="No approved completed teaching lessons for this month. This payslip will show RM 0.00.">
                    <Columns>
                        <asp:BoundField DataField="SessionStartLocal" HeaderText="Lesson (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                        <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
                        <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
                        <asp:BoundField DataField="Hours" HeaderText="Approved hours" DataFormatString="{0:N2}" />
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Label runat="server" AssociatedControlID="ReasonInput" Text="Issue or revision reason" CssClass="form-label" />
            <asp:TextBox ID="ReasonInput" runat="server" TextMode="MultiLine" Rows="2" MaxLength="500" CssClass="form-control mb-3" ValidateRequestMode="Disabled" />
            <p class="text-muted">Explain why you are issuing this record, for example: monthly earnings, late approved hours, or a corrected rate. The teacher can read this reason.</p>
            <asp:Button ID="IssueButton" runat="server" Text="Issue Payslip" OnClick="IssueButton_Click" CausesValidation="false" CssClass="btn btn-success" />
        </asp:Panel>
    </asp:Panel>
    <h2 class="h4">Issued Payslips</h2>
    <asp:Button ID="RefreshButton" runat="server" Text="Refresh Records" CausesValidation="false" CssClass="btn btn-outline-primary mb-3" />
    <div class="table-responsive">
        <asp:GridView ID="Records" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="No payslips have been issued yet.">
            <Columns>
                <asp:BoundField DataField="TeacherName" HeaderText="Teacher" HtmlEncode="true" />
                <asp:BoundField DataField="StaffId" HeaderText="Staff ID" HtmlEncode="true" />
                <asp:BoundField DataField="SalaryMonth" HeaderText="Month" DataFormatString="{0:MMM yyyy}" />
                <asp:BoundField DataField="Revision" HeaderText="Revision" />
                <asp:BoundField DataField="ApprovedHours" HeaderText="Hours" DataFormatString="{0:N2}" />
                <asp:BoundField DataField="HourlyRate" HeaderText="Rate (RM)" DataFormatString="{0:N2}" />
                <asp:BoundField DataField="GrossAmount" HeaderText="Gross (RM)" DataFormatString="{0:N2}" />
                <asp:BoundField DataField="IssuedLocal" HeaderText="Issued (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                <asp:BoundField DataField="VersionStatus" HeaderText="Version" HtmlEncode="true" />
                <asp:HyperLinkField Text="View / Print" HeaderText="Payslip" DataNavigateUrlFields="PayslipId" DataNavigateUrlFormatString="~/Payslip?id={0}" />
            </Columns>
        </asp:GridView>
    </div>
</main>
</asp:Content>
