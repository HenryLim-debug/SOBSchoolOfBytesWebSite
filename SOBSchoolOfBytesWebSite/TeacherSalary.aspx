<%@ Page Title="My Salary" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TeacherSalary.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.TeacherSalary" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <a runat="server" href="~/TeacherDashboard" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
    <h1>My Salary</h1>
    <p>Calculated salary = approved teaching hours × the hourly rate set for that month. All amounts are in Malaysian Ringgit (RM).</p>
    <p>Pending and rejected hours are excluded. Current-month earnings grow as hours are approved. Past months also update if a late approval or rate correction is made. These are gross teaching earnings before deductions; they do not confirm a payment.</p>
    <p>You can view your own current and past months. Contact the president if your hourly rate is not set.</p>
    <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="MessageText" runat="server" Mode="Encode" /></asp:Panel>
    <asp:Label runat="server" AssociatedControlID="MonthInput" Text="Salary month" CssClass="form-label" />
    <asp:TextBox ID="MonthInput" runat="server" TextMode="Month" CssClass="form-control mb-3" />
    <asp:Button ID="LoadButton" runat="server" Text="Load Month" CssClass="btn btn-primary mb-3" OnClick="LoadButton_Click" CausesValidation="false" />
    <asp:Panel ID="ResultsPanel" runat="server" Visible="false">
    <h2 class="h4">Salary for <asp:Literal ID="SelectedMonth" runat="server" Mode="Encode" /></h2>
    <div class="table-responsive">
        <asp:GridView ID="SalarySummary" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="No rate or teaching hours have been recorded for this month.">
        <Columns>
            <asp:BoundField DataField="ApprovedHours" HeaderText="Approved hours" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="PendingHours" HeaderText="Pending hours" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="RejectedHours" HeaderText="Rejected hours" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="HourlyRate" HeaderText="Hourly rate (RM)" HtmlEncode="true" DataFormatString="{0:N2}" NullDisplayText="Rate not set" />
            <asp:BoundField DataField="CalculatedSalary" HeaderText="Calculated salary (RM)" HtmlEncode="true" DataFormatString="{0:N2}" NullDisplayText="Rate not set" />
            <asp:BoundField DataField="CalculationStatus" HeaderText="Status" HtmlEncode="true" />
        </Columns>
        </asp:GridView>
    </div>
    <h2 class="h4 mt-4">Teaching Hours for This Month</h2>
    <div class="table-responsive">
        <asp:GridView ID="LessonRecords" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="No completed teaching-hour records for this month.">
        <Columns>
            <asp:BoundField DataField="SessionStartLocal" HeaderText="Lesson (Malaysia time)" HtmlEncode="true" DataFormatString="{0:dd MMM yyyy HH:mm}" />
            <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
            <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
            <asp:BoundField DataField="Hours" HeaderText="Hours" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="Status" HeaderText="Review status" HtmlEncode="true" />
            <asp:BoundField DataField="LessonNotes" HeaderText="Lesson notes" HtmlEncode="true" />
        </Columns>
        </asp:GridView>
    </div>
    <h2 class="h4 mt-4">Salary History</h2>
    <div class="table-responsive">
        <asp:GridView ID="SalaryHistory" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="No salary history yet.">
        <Columns>
            <asp:BoundField DataField="SalaryMonth" HeaderText="Month" HtmlEncode="true" DataFormatString="{0:MMM yyyy}" />
            <asp:BoundField DataField="ApprovedHours" HeaderText="Approved hours" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="PendingHours" HeaderText="Pending hours" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="RejectedHours" HeaderText="Rejected hours" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="HourlyRate" HeaderText="Hourly rate (RM)" HtmlEncode="true" DataFormatString="{0:N2}" NullDisplayText="Rate not set" />
            <asp:BoundField DataField="CalculatedSalary" HeaderText="Calculated salary (RM)" HtmlEncode="true" DataFormatString="{0:N2}" NullDisplayText="Rate not set" />
            <asp:BoundField DataField="CalculationStatus" HeaderText="Status" HtmlEncode="true" />
        </Columns>
        </asp:GridView>
    </div>
    </asp:Panel>
</main>
</asp:Content>
