<%@ Page Title="My Teaching Hours" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TeachingHours.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.TeachingHours" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4">
        <a runat="server" href="~/TeacherDashboard" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
        <h1>My Teaching Hours</h1>
        <p>Record lessons after you have taught them. Each scheduled lesson is 2 hours. The president reviews your records; only approved hours count toward salary calculations. All lesson times are Malaysia time (UTC+8).</p>
        <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="MessageText" runat="server" Mode="Encode" /></asp:Panel>
        <asp:Button ID="RefreshButton" runat="server" Text="Refresh Records" CssClass="btn btn-outline-primary mb-3" CausesValidation="false" OnClick="RefreshButton_Click" />
        <section class="card p-4 mb-4">
            <h2 class="h4">Record a Completed Lesson</h2>
            <asp:Label runat="server" AssociatedControlID="ClassList" Text="Your class" CssClass="form-label" />
            <asp:DropDownList ID="ClassList" runat="server" CssClass="form-select mb-3" AutoPostBack="true" OnSelectedIndexChanged="ClassList_SelectedIndexChanged" />
            <asp:Panel ID="NoSessionsPanel" runat="server" Visible="false" CssClass="alert alert-info">No completed lessons are available to record for this class. Future lessons appear only after their scheduled end time. Lessons already pending or approved are listed in your records below.</asp:Panel>
            <asp:Panel ID="EntryPanel" runat="server" Visible="false">
                <asp:Label runat="server" AssociatedControlID="SessionList" Text="Completed lesson" CssClass="form-label" />
                <asp:DropDownList ID="SessionList" runat="server" CssClass="form-select mb-3" AutoPostBack="true" OnSelectedIndexChanged="SessionList_SelectedIndexChanged" />
                <p class="text-muted"><asp:Literal ID="PreviousReason" runat="server" Mode="Encode" /></p>
                <asp:Label runat="server" AssociatedControlID="LessonNotes" Text="What did you teach? *" CssClass="form-label" />
                <asp:TextBox ID="LessonNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" ValidateRequestMode="Disabled" />
                <p class="form-text">Briefly describe the topics or activities covered, up to 2,000 characters. Rejected records can be corrected and resubmitted.</p>
                <asp:Button ID="SubmitButton" runat="server" Text="Submit Teaching Hours" CssClass="btn btn-primary" OnClick="SubmitButton_Click" />
            </asp:Panel>
        </section>
        <h2 class="h4">Monthly Hours</h2>
        <p>The month is based on when you taught the lesson. Pending and rejected hours are shown separately from approved hours.</p>
        <div class="table-responsive"><asp:GridView ID="MonthlyHours" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered" GridLines="None" EmptyDataText="No teaching hours recorded yet."><Columns>
            <asp:BoundField DataField="TeachingMonth" HeaderText="Month" />
            <asp:BoundField DataField="ApprovedHours" HeaderText="Approved hours" DataFormatString="{0:0.##}" />
            <asp:BoundField DataField="PendingHours" HeaderText="Pending hours" DataFormatString="{0:0.##}" />
            <asp:BoundField DataField="RejectedHours" HeaderText="Rejected hours" DataFormatString="{0:0.##}" />
        </Columns></asp:GridView></div>
        <h2 class="h4 mt-4">My Teaching Records</h2>
        <div class="table-responsive"><asp:GridView ID="Records" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="No teaching records yet."><Columns>
            <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
            <asp:BoundField DataField="SessionStartLocal" HeaderText="Lesson start" DataFormatString="{0:dd MMM yyyy HH:mm}" />
            <asp:BoundField DataField="SessionEndLocal" HeaderText="Lesson end" DataFormatString="{0:dd MMM yyyy HH:mm}" />
            <asp:BoundField DataField="Hours" HeaderText="Hours" DataFormatString="{0:0.##}" />
            <asp:BoundField DataField="Status" HeaderText="Status" HtmlEncode="true" />
            <asp:BoundField DataField="LessonNotes" HeaderText="Lesson notes" HtmlEncode="true" />
            <asp:BoundField DataField="ReviewNotes" HeaderText="President's feedback" HtmlEncode="true" />
        </Columns></asp:GridView></div>
    </main>
</asp:Content>
