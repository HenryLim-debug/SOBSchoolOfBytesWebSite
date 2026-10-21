<%@ Page Title="Course Applications" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CourseApplications.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.CourseApplications" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4" aria-labelledby="pageTitle">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
            <div><h1 id="pageTitle">Course Applications</h1><p>Approve or reject applications for the classes you teach.</p></div>
            <a runat="server" href="~/TeacherDashboard" class="btn btn-outline-secondary">Back to Dashboard</a>
        </div>
        <p>You can review only applications for your own assigned classes. Approval adds the student to the requested group, with a maximum of 10 students.</p>
        <p>If the requested group is full, closed or has started, approval is blocked and the application stays pending. Pending applications do not reserve places. All times shown are Malaysia time (UTC+8).</p>
        <asp:Panel ID="FeedbackPanel" runat="server" Visible="false" role="status">
            <asp:Literal ID="FeedbackMessage" runat="server" Mode="Encode" />
        </asp:Panel>
        <asp:Button ID="RefreshButton" runat="server" Text="Refresh Applications" CssClass="btn btn-outline-primary mb-3"
            CausesValidation="false" OnClick="RefreshButton_Click" />
        <div class="table-responsive">
            <asp:GridView ID="PendingRequests" runat="server" AutoGenerateColumns="false" DataKeyNames="ApplicationId"
                CssClass="table table-bordered align-middle" GridLines="None" UseAccessibleHeader="true"
                EmptyDataText="There are no course applications waiting for approval."
                OnRowCommand="PendingRequests_RowCommand" OnRowDataBound="PendingRequests_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="Student"><ItemTemplate>
                        <asp:Literal runat="server" Mode="Encode" Text='<%# Eval("StudentName") %>' /><br />
                        <small><asp:Literal runat="server" Mode="Encode" Text='<%# Eval("username") %>' /></small><br />
                        <small>Account: <asp:Literal runat="server" Mode="Encode" Text='<%# Eval("StudentStatus") %>' /></small>
                    </ItemTemplate></asp:TemplateField>
                    <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
                    <asp:BoundField DataField="ClassName" HeaderText="Requested group" HtmlEncode="true" />
                    <asp:TemplateField HeaderText="Requested timetable"><ItemTemplate>
                        <asp:Literal runat="server" Mode="Encode" Text='<%# Eval("WeeklySchedule") %>' /><br />
                        <small><asp:Literal runat="server" Mode="Encode" Text='<%# Eval("CourseDates") %>' /></small>
                    </ItemTemplate></asp:TemplateField>
                    <asp:BoundField DataField="AppliedAtLocal" HeaderText="Applied" DataFormatString="{0:dd MMM yyyy HH:mm}" HtmlEncode="true" />
                    <asp:TemplateField HeaderText="Decision"><ItemTemplate>
                        <asp:Button ID="ApproveButton" runat="server" Text="Approve" CommandName="ApproveApplication" CssClass="btn btn-success btn-sm mb-2" CausesValidation="false" />
                        <asp:Button ID="RejectButton" runat="server" Text="Reject" CommandName="RejectApplication" CssClass="btn btn-outline-danger btn-sm" CausesValidation="false" />
                    </ItemTemplate></asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </main>
</asp:Content>
