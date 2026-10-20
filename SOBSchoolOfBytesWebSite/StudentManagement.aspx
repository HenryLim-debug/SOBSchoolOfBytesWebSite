<%@ Page Title="Student Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="StudentManagement.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.StudentManagement" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4" aria-labelledby="studentManagementTitle">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
            <div>
                <h1 id="studentManagementTitle">Student Management</h1>
                <p>Find student accounts, update their details and manage access.</p>
            </div>
            <a runat="server" href="~/PresidentDashboard" class="btn btn-outline-secondary">Back to Dashboard</a>
        </div>
        <asp:Panel ID="FeedbackPanel" runat="server" Visible="false" role="status">
            <asp:Literal ID="FeedbackMessage" runat="server" Mode="Encode" />
        </asp:Panel>
        <section class="card p-4 my-3" aria-labelledby="findStudentsTitle">
            <h2 id="findStudentsTitle" class="h4">Find Students</h2>
            <div class="row g-3 align-items-end">
                <div class="col-md-5">
                    <asp:Label runat="server" AssociatedControlID="SearchText" CssClass="form-label" Text="Name or username" />
                    <asp:TextBox ID="SearchText" runat="server" CssClass="form-control" MaxLength="100" />
                </div>
                <div class="col-md-3">
                    <asp:Label runat="server" AssociatedControlID="StatusFilter" CssClass="form-label" Text="Account status" />
                    <asp:DropDownList ID="StatusFilter" runat="server" CssClass="form-select">
                        <asp:ListItem Value="">All statuses</asp:ListItem>
                        <asp:ListItem Value="Pending">Pending</asp:ListItem>
                        <asp:ListItem Value="Approved">Approved</asp:ListItem>
                        <asp:ListItem Value="Rejected">Rejected</asp:ListItem>
                        <asp:ListItem Value="Suspended">Suspended</asp:ListItem>
                        <asp:ListItem Value="Inactive">Inactive</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-4 d-flex gap-2">
                    <asp:Button ID="SearchButton" runat="server" Text="Search" CssClass="btn btn-primary" CausesValidation="false" OnClick="SearchButton_Click" />
                    <asp:Button ID="ClearSearchButton" runat="server" Text="Clear" CssClass="btn btn-outline-secondary" CausesValidation="false" OnClick="ClearSearchButton_Click" />
                </div>
            </div>
        </section>
        <asp:Panel ID="EditorPanel" runat="server" Visible="false" CssClass="card p-4 my-3" role="region" aria-labelledby="editStudentTitle">
            <h2 id="editStudentTitle" class="h4">Edit Student</h2>
            <p>Update the student's details here. Use the account actions in the student list to change access.</p>
            <asp:ValidationSummary runat="server" ValidationGroup="StudentDetails" CssClass="alert alert-danger" HeaderText="Please check the following details:" />
            <asp:Label ID="errMsg" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert" />
            <div class="row g-3">
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="fname" CssClass="form-label" Text="First name *" />
                    <asp:TextBox ID="fname" runat="server" CssClass="form-control" MaxLength="50" autocomplete="given-name" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="fname" ValidationGroup="StudentDetails" Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter the student's first name." />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="lname" CssClass="form-label" Text="Last name *" />
                    <asp:TextBox ID="lname" runat="server" CssClass="form-control" MaxLength="50" autocomplete="family-name" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="lname" ValidationGroup="StudentDetails" Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter the student's last name." />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="username" CssClass="form-label" Text="Username *" />
                    <asp:TextBox ID="username" runat="server" CssClass="form-control" MaxLength="30" autocomplete="off" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="username" ValidationGroup="StudentDetails" Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter a username." />
                    <p class="form-text mb-0">Use 3–30 letters, numbers or underscores.</p>
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="email" CssClass="form-label" Text="Email (optional)" />
                    <asp:TextBox ID="email" runat="server" CssClass="form-control" TextMode="Email" MaxLength="254" autocomplete="email" />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="gender" CssClass="form-label" Text="Gender (optional)" />
                    <asp:DropDownList ID="gender" runat="server" CssClass="form-select">
                        <asp:ListItem Value="">Prefer not to say</asp:ListItem>
                        <asp:ListItem Value="Female">Female</asp:ListItem>
                        <asp:ListItem Value="Male">Male</asp:ListItem>
                        <asp:ListItem Value="Other">Other</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="country" CssClass="form-label" Text="Country (optional)" />
                    <asp:TextBox ID="country" runat="server" CssClass="form-control" MaxLength="50" autocomplete="country-name" />
                </div>
            </div>
            <div class="d-flex gap-2 mt-4">
                <asp:Button ID="SaveButton" runat="server" Text="Save Changes" CssClass="btn btn-primary" ValidationGroup="StudentDetails" OnClick="SaveButton_Click" />
                <asp:Button ID="CancelButton" runat="server" Text="Cancel Editing" CssClass="btn btn-outline-secondary" CausesValidation="false" OnClick="CancelButton_Click" />
            </div>
        </asp:Panel>
        <section class="mt-4" aria-labelledby="studentListTitle">
            <h2 id="studentListTitle">Students</h2>
            <p>Suspend access for a temporary restriction, or deactivate an account when a student leaves. Both actions keep the student's records.</p>
            <p><asp:Literal ID="ResultCount" runat="server" Mode="Encode" /></p>
            <asp:Button ID="RefreshButton" runat="server" Text="Refresh List" CssClass="btn btn-outline-primary mb-3" CausesValidation="false" OnClick="RefreshButton_Click" />
            <div class="table-responsive">
                <asp:GridView ID="Students" runat="server" AutoGenerateColumns="false" DataKeyNames="Id,accountStatus"
                    CssClass="table table-bordered align-middle" GridLines="None" UseAccessibleHeader="true" AllowPaging="true" PageSize="10"
                    EmptyDataText="No student accounts match your search." OnPageIndexChanging="Students_PageIndexChanging"
                    OnRowCommand="Students_RowCommand" OnRowDataBound="Students_RowDataBound">
                    <PagerStyle CssClass="student-pager" />
                    <Columns>
                        <asp:BoundField DataField="FullName" HeaderText="Student name" HtmlEncode="true" />
                        <asp:BoundField DataField="username" HeaderText="Username" HtmlEncode="true" />
                        <asp:BoundField DataField="email" HeaderText="Email" HtmlEncode="true" NullDisplayText="Not provided" />
                        <asp:BoundField DataField="accountStatus" HeaderText="Status" HtmlEncode="true" />
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <div class="d-flex flex-wrap gap-2">
                                    <asp:Button ID="EditStudentButton" runat="server" Text="Edit" CommandName="EditStudent" CssClass="btn btn-outline-primary btn-sm" CausesValidation="false" />
                                    <asp:Button ID="PrimaryActionButton" runat="server" CssClass="btn btn-outline-secondary btn-sm" CausesValidation="false" />
                                    <asp:Button ID="SecondaryActionButton" runat="server" CssClass="btn btn-outline-danger btn-sm" CausesValidation="false" />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>
</asp:Content>
