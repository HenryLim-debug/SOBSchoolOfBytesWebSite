<%@ Page Title="Review Teaching Hours" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReviewTeachingHours.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.ReviewTeachingHours" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4">
        <a runat="server" href="~/PresidentDashboard" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
        <h1>Review Teaching Hours</h1>
        <p>Check the teacher's lesson notes before approving two hours of completed teaching. Only approved hours count toward salary calculations. Times are shown in Malaysia time (UTC+8).</p>
        <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="MessageText" runat="server" Mode="Encode" /></asp:Panel>
        <asp:Button ID="RefreshButton" runat="server" Text="Refresh Records" CssClass="btn btn-outline-primary mb-3" CausesValidation="false" OnClick="RefreshButton_Click" />
        <h2 class="h4">Monthly Hours by Teacher</h2>
        <div class="table-responsive"><asp:GridView ID="MonthlyHours" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered" GridLines="None" EmptyDataText="No teaching hours recorded yet."><Columns>
            <asp:BoundField DataField="TeacherName" HeaderText="Teacher" HtmlEncode="true" />
            <asp:BoundField DataField="TeachingMonth" HeaderText="Teaching month" />
            <asp:BoundField DataField="ApprovedHours" HeaderText="Approved hours" DataFormatString="{0:0.##}" />
            <asp:BoundField DataField="PendingHours" HeaderText="Pending hours" DataFormatString="{0:0.##}" />
            <asp:BoundField DataField="RejectedHours" HeaderText="Rejected hours" DataFormatString="{0:0.##}" />
        </Columns></asp:GridView></div>
        <h2 class="h4 mt-4">Teaching Records</h2>
        <div class="table-responsive"><asp:GridView ID="Records" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="No teaching records are waiting for review."><Columns>
            <asp:BoundField DataField="TeacherName" HeaderText="Teacher" HtmlEncode="true" />
            <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
            <asp:BoundField DataField="SessionStartLocal" HeaderText="Lesson start" DataFormatString="{0:dd MMM yyyy HH:mm}" />
            <asp:BoundField DataField="Hours" HeaderText="Hours" DataFormatString="{0:0.##}" />
            <asp:BoundField DataField="Status" HeaderText="Status" HtmlEncode="true" />
            <asp:TemplateField HeaderText="Action"><ItemTemplate><a class="btn btn-outline-primary btn-sm" href='<%# ResolveUrl("~/ReviewTeachingHours?id="+Eval("LogId")+"#review") %>'>View / Review</a></ItemTemplate></asp:TemplateField>
        </Columns></asp:GridView></div>
        <asp:Panel ID="ReviewPanel" runat="server" Visible="false" CssClass="card p-4 mt-4">
            <section id="review">
                <h2 class="h4">Teaching Record</h2>
                <p><asp:Literal ID="RecordHeading" runat="server" Mode="Encode" /></p>
                <h3 class="h6">Lesson notes</h3>
                <div class="mb-3" style="white-space:pre-wrap;overflow-wrap:anywhere"><asp:Literal ID="LessonSummary" runat="server" Mode="Encode" /></div>
                <asp:Panel ID="DecisionPanel" runat="server" Visible="false">
                    <asp:Label runat="server" AssociatedControlID="ReviewNotes" Text="Review notes (required when rejecting)" CssClass="form-label" />
                    <asp:TextBox ID="ReviewNotes" runat="server" CssClass="form-control mb-3" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" />
                    <asp:Button ID="ApproveButton" runat="server" Text="Approve 2 Hours" CssClass="btn btn-success me-2" OnClick="ApproveButton_Click" />
                    <asp:Button ID="RejectButton" runat="server" Text="Reject Record" CssClass="btn btn-outline-danger" OnClick="RejectButton_Click" />
                </asp:Panel>
                <h3 class="h5 mt-4">Review History</h3>
                <div class="table-responsive"><asp:GridView ID="ReviewHistory" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered" GridLines="None" EmptyDataText="This record has not been reviewed yet."><Columns>
                    <asp:BoundField DataField="Decision" HeaderText="Decision" HtmlEncode="true" />
                    <asp:BoundField DataField="LessonNotes" HeaderText="Lesson notes at review" HtmlEncode="true" />
                    <asp:BoundField DataField="ReviewNotes" HeaderText="Review notes" HtmlEncode="true" />
                    <asp:BoundField DataField="ReviewedByName" HeaderText="Reviewed by" HtmlEncode="true" />
                    <asp:BoundField DataField="ReviewedLocal" HeaderText="Reviewed (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                </Columns></asp:GridView></div>
            </section>
        </asp:Panel>
    </main>
</asp:Content>
