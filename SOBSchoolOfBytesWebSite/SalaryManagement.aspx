<%@ Page Title="Teacher Salaries" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SalaryManagement.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.SalaryManagement" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <a runat="server" href="~/PresidentDashboard" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
    <h1>Teacher Salaries</h1>
    <p>Calculated salary = approved teaching hours × the hourly rate set for that month. All amounts are in Malaysian Ringgit (RM).</p>
    <p>Pending and rejected hours are excluded. Current-month earnings grow as hours are approved. Past months also update if a late approval or rate correction is made. These are gross teaching earnings before deductions; they do not confirm a payment.</p>
    <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="MessageText" runat="server" Mode="Encode" /></asp:Panel>
    <asp:Label runat="server" AssociatedControlID="TeacherList" Text="Teacher" CssClass="form-label" />
    <asp:DropDownList ID="TeacherList" runat="server" CssClass="form-select mb-3" AutoPostBack="true" OnSelectedIndexChanged="TeacherList_SelectedIndexChanged" />
    <asp:Label runat="server" AssociatedControlID="MonthInput" Text="Salary month" CssClass="form-label" />
    <asp:TextBox ID="MonthInput" runat="server" TextMode="Month" CssClass="form-control mb-3" />
    <asp:Button ID="LoadButton" runat="server" Text="Load Month" CssClass="btn btn-primary mb-3" OnClick="LoadButton_Click" CausesValidation="false" />
    <asp:Panel ID="ResultsPanel" runat="server" Visible="false">
    <h2 class="h4">All Teachers — <asp:Literal ID="SelectedMonth" runat="server" Mode="Encode" /></h2>
    <div class="table-responsive">
        <asp:GridView ID="AllSalaries" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="No rates or teaching hours have been recorded for this month.">
        <Columns>
            <asp:BoundField DataField="TeacherName" HeaderText="Teacher" HtmlEncode="true" />
            <asp:BoundField DataField="staffId" HeaderText="Staff ID" HtmlEncode="true" />
            <asp:BoundField DataField="ApprovedHours" HeaderText="Approved hours" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="PendingHours" HeaderText="Pending hours" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="RejectedHours" HeaderText="Rejected hours" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="HourlyRate" HeaderText="Hourly rate (RM)" HtmlEncode="true" DataFormatString="{0:N2}" NullDisplayText="Rate not set" />
            <asp:BoundField DataField="CalculatedSalary" HeaderText="Calculated salary (RM)" HtmlEncode="true" DataFormatString="{0:N2}" NullDisplayText="Rate not set" />
            <asp:BoundField DataField="CalculationStatus" HeaderText="Status" HtmlEncode="true" />
        </Columns>
        </asp:GridView>
    </div>
    <section class="card p-4 my-4">
        <h2 class="h4">Monthly Rate — <asp:Literal ID="SelectedTeacher" runat="server" Mode="Encode" /></h2>
        <p>This rate applies to all approved hours for this teacher in the loaded month only. Set a rate separately for each month; other months keep their own rates.</p>
        <asp:Label runat="server" AssociatedControlID="RateInput" Text="Hourly rate (RM)" CssClass="form-label" />
        <asp:TextBox ID="RateInput" runat="server" CssClass="form-control mb-3" MaxLength="8" inputmode="decimal" />
        <asp:Label runat="server" AssociatedControlID="RateReason" Text="Reason for setting or changing the rate" CssClass="form-label" />
        <asp:TextBox ID="RateReason" runat="server" CssClass="form-control mb-3" TextMode="MultiLine" Rows="3" MaxLength="500" ValidateRequestMode="Disabled" />
        <asp:Button ID="SaveRateButton" runat="server" Text="Save Monthly Rate" CssClass="btn btn-primary align-self-start" OnClick="SaveRateButton_Click" CausesValidation="false" />
    </section>
    <h2 class="h4">Rate Change History for the Selected Month</h2>
    <div class="table-responsive">
        <asp:GridView ID="RateChanges" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="No rate changes for this month.">
        <Columns>
            <asp:BoundField DataField="ChangedLocal" HeaderText="Changed (Malaysia time)" HtmlEncode="true" DataFormatString="{0:dd MMM yyyy HH:mm}" />
            <asp:BoundField DataField="PreviousRate" HeaderText="Previous rate (RM)" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="NewRate" HeaderText="New rate (RM)" HtmlEncode="true" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="Reason" HeaderText="Reason" HtmlEncode="true" />
            <asp:BoundField DataField="ChangedByName" HeaderText="Changed by" HtmlEncode="true" />
        </Columns>
        </asp:GridView>
    </div>
    <h2 class="h4 mt-4">Selected Teacher’s Lessons for This Month</h2>
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
    <h2 class="h4 mt-4">Selected Teacher’s Salary History</h2>
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
