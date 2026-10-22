<%@ Page Title="Teaching Earnings Payslip" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Payslip.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.Payslip" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<style>
    .sob-payslip { max-width: 960px; margin: 1.5rem auto; }
    .sob-payslip-sheet { border: 1px solid #d5dce3; border-radius: .5rem; padding: 2rem; }
    .sob-payslip-sheet dt { font-weight: 600; }
    .sob-payslip-sheet dd { overflow-wrap: anywhere; }
    @media print {
        @page { margin: 15mm; }
        .navbar, footer, .payslip-controls, .body-content > hr { display: none !important; }
        .container, .body-content, .sob-payslip { width: 100% !important; max-width: none !important; margin: 0 !important; padding: 0 !important; }
        .sob-payslip-sheet { border: 0; padding: 0; }
        .sob-payslip-sheet .table-responsive { overflow: visible !important; }
        .sob-payslip-sheet { font-size: 10pt; color: #000; }
        .sob-payslip-sheet tr { break-inside: avoid; }
        .sob-payslip-sheet thead { display: table-header-group; }
    }
</style>
<main class="sob-payslip">
    <div class="payslip-controls mb-3"><a runat="server" href="~/Payslips" class="btn btn-outline-secondary">Back to Payslip Records</a></div>
    <asp:Panel ID="ErrorPanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="ErrorMessage" runat="server" Mode="Encode" /></asp:Panel>
    <asp:Panel ID="Sheet" runat="server" Visible="false" CssClass="sob-payslip-sheet">
        <div class="payslip-controls mb-3"><button type="button" class="btn btn-primary" onclick="window.print()">Print / Save as PDF</button><p class="mt-2">In the print window, choose a printer or Save as PDF.</p></div>
        <p class="text-muted">SOB SCHOOL OF BYTES</p>
        <h1 class="h2">Teaching Earnings Payslip</h1>
        <p><strong><asp:Literal ID="Reference" runat="server" Mode="Encode" /></strong></p>
        <p><asp:Literal ID="VersionStatus" runat="server" Mode="Encode" /></p>
        <dl class="row">
            <dt class="col-sm-4">Teacher</dt><dd class="col-sm-8"><asp:Literal ID="TeacherName" runat="server" Mode="Encode" /></dd>
            <dt class="col-sm-4">Staff ID</dt><dd class="col-sm-8"><asp:Literal ID="StaffId" runat="server" Mode="Encode" /></dd>
            <dt class="col-sm-4">Salary month</dt><dd class="col-sm-8"><asp:Literal ID="SalaryMonth" runat="server" Mode="Encode" /></dd>
            <dt class="col-sm-4">Issued (Malaysia time)</dt><dd class="col-sm-8"><asp:Literal ID="IssuedDate" runat="server" Mode="Encode" /></dd>
            <dt class="col-sm-4">Issued by</dt><dd class="col-sm-8"><asp:Literal ID="IssuedBy" runat="server" Mode="Encode" /></dd>
            <dt class="col-sm-4">Approved teaching hours</dt><dd class="col-sm-8"><asp:Literal ID="Hours" runat="server" Mode="Encode" /></dd>
            <dt class="col-sm-4">Hourly rate (RM)</dt><dd class="col-sm-8"><asp:Literal ID="Rate" runat="server" Mode="Encode" /></dd>
            <dt class="col-sm-4">Gross teaching earnings (RM)</dt><dd class="col-sm-8"><strong><asp:Literal ID="Amount" runat="server" Mode="Encode" /></strong></dd>
        </dl>
        <p><strong>Issue / revision reason:</strong> <asp:Literal ID="IssueReason" runat="server" Mode="Encode" /></p>
        <asp:HyperLink ID="PreviousLink" runat="server" Visible="false" Text="View previous revision" CssClass="payslip-controls" />
        <h2 class="h4 mt-4">Included Approved Lessons</h2>
        <div class="table-responsive">
            <asp:GridView ID="Lessons" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered" GridLines="None" EmptyDataText="No approved lessons were included. Gross teaching earnings are RM 0.00.">
                <Columns>
                    <asp:BoundField DataField="SessionStartLocal" HeaderText="Lesson (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                    <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
                    <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
                    <asp:BoundField DataField="Hours" HeaderText="Hours" DataFormatString="{0:N2}" />
                </Columns>
            </asp:GridView>
        </div>
        <p class="mt-3">Gross teaching earnings = included approved hours × the saved hourly rate. Pending and rejected hours are excluded. This record shows earnings as of its issue time; deductions and payment status are not tracked.</p>
        <p>Changes to teaching hours or rates do not alter this saved record. If a correction is needed, ask the president to issue a revised payslip.</p>
    </asp:Panel>
</main>
</asp:Content>
