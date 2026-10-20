<%@ Page Title="President Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PresidentDashboard.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.PresidentDashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4" aria-labelledby="presidentTitle">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
            <div>
                <h1 id="presidentTitle">President Dashboard</h1>
                <p>Welcome, <asp:Literal ID="PresidentName" runat="server" Mode="Encode" />. Manage your school and review new student registrations here.</p>
            </div>
            <asp:Button ID="SignOutButton" runat="server" Text="Sign Out" CssClass="btn btn-outline-secondary"
                CausesValidation="false" OnClick="SignOutButton_Click" />
        </div>
        <div class="row g-3 my-2">
            <div class="col-sm-6 col-lg-3"><div class="card p-3"><h2 class="h6">Total students</h2><asp:Literal ID="StudentCount" runat="server" Mode="Encode" /></div></div>
            <div class="col-sm-6 col-lg-3"><div class="card p-3"><h2 class="h6">Pending approval</h2><asp:Literal ID="PendingCount" runat="server" Mode="Encode" /></div></div>
            <div class="col-sm-6 col-lg-3"><div class="card p-3"><h2 class="h6">Approved students</h2><asp:Literal ID="ApprovedCount" runat="server" Mode="Encode" /></div></div>
            <div class="col-sm-6 col-lg-3"><div class="card p-3"><h2 class="h6">Active teachers</h2><asp:Literal ID="TeacherCount" runat="server" Mode="Encode" /></div></div>
        </div>
        <div class="d-flex flex-wrap gap-2 mt-4">
            <a runat="server" href="~/TeacherManagement" class="btn btn-primary">Manage Teachers</a>
            <a runat="server" href="~/StudentManagement" class="btn btn-outline-primary">Manage Students</a>
            <a runat="server" href="~/ClassManagement" class="btn btn-outline-primary">Manage Classes</a>
        </div>
        <section class="mt-4" aria-labelledby="pendingTitle">
            <h2 id="pendingTitle">Student Approval</h2>
            <p>Approve a registration to allow the student to sign in, or reject it to keep the account unavailable.</p>
            <asp:Panel ID="FeedbackPanel" runat="server" Visible="false" role="status">
                <asp:Literal ID="FeedbackMessage" runat="server" Mode="Encode" />
            </asp:Panel>
            <asp:Button ID="RefreshButton" runat="server" Text="Refresh List" CssClass="btn btn-outline-primary mb-3"
                CausesValidation="false" OnClick="RefreshButton_Click" />
            <div class="table-responsive">
                <asp:GridView ID="PendingStudents" runat="server" AutoGenerateColumns="false" DataKeyNames="Id"
                    CssClass="table table-bordered align-middle" GridLines="None" UseAccessibleHeader="true"
                    EmptyDataText="There are no student registrations waiting for approval."
                    OnRowCommand="PendingStudents_RowCommand" OnRowDataBound="PendingStudents_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="FullName" HeaderText="Student name" HtmlEncode="true" />
                        <asp:BoundField DataField="username" HeaderText="Username" HtmlEncode="true" />
                        <asp:BoundField DataField="email" HeaderText="Email" HtmlEncode="true" NullDisplayText="Not provided" />
                        <asp:BoundField DataField="createdAt" HeaderText="Registered (UTC)" DataFormatString="{0:dd MMM yyyy HH:mm}" HtmlEncode="true" />
                        <asp:TemplateField HeaderText="Decision">
                            <ItemTemplate>
                                <asp:Button ID="ApproveButton" runat="server" Text="Approve" CommandName="ApproveStudent"
                                    CssClass="btn btn-success btn-sm me-2" CausesValidation="false" />
                                <asp:Button ID="RejectButton" runat="server" Text="Reject" CommandName="RejectStudent"
                                    CssClass="btn btn-outline-danger btn-sm" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>
</asp:Content>
