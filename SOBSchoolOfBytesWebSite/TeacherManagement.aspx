<%@ Page Title="Teacher Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TeacherManagement.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.TeacherManagement" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4" aria-labelledby="managementTitle">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
            <div>
                <h1 id="managementTitle">Teacher Management</h1>
                <p>Create teacher accounts, assign courses and keep staff details up to date.</p>
            </div>
            <a runat="server" href="~/PresidentDashboard" class="btn btn-outline-secondary">Back to Dashboard</a>
        </div>
        <asp:Panel ID="FeedbackPanel" runat="server" Visible="false" role="status">
            <asp:Literal ID="FeedbackMessage" runat="server" Mode="Encode" />
        </asp:Panel>
        <section class="card p-4 my-3" aria-labelledby="teacherFormTitle">
            <h2 id="teacherFormTitle" class="h4"><asp:Literal ID="FormTitle" runat="server" Mode="Encode" Text="Add Teacher" /></h2>
            <p>Each teacher has one assigned course. Several teachers can teach the same course.</p>
            <asp:ValidationSummary runat="server" ValidationGroup="TeacherDetails" CssClass="alert alert-danger" HeaderText="Please check the following details:" />
            <asp:Label ID="errMsg" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert" />
            <div class="row g-3">
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="fname" CssClass="form-label" Text="First name *" />
                    <asp:TextBox ID="fname" runat="server" CssClass="form-control" MaxLength="50" autocomplete="given-name" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="fname" ValidationGroup="TeacherDetails" Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter the teacher's first name." />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="lname" CssClass="form-label" Text="Last name *" />
                    <asp:TextBox ID="lname" runat="server" CssClass="form-control" MaxLength="50" autocomplete="family-name" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="lname" ValidationGroup="TeacherDetails" Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter the teacher's last name." />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="username" CssClass="form-label" Text="Username *" />
                    <asp:TextBox ID="username" runat="server" CssClass="form-control" MaxLength="30" autocomplete="off" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="username" ValidationGroup="TeacherDetails" Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter a username." />
                    <p class="form-text mb-0">Use 3–30 letters, numbers or underscores.</p>
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="staffId" CssClass="form-label" Text="Staff ID *" />
                    <asp:TextBox ID="staffId" runat="server" CssClass="form-control" MaxLength="30" autocomplete="off" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="staffId" ValidationGroup="TeacherDetails" Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter a staff ID." />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="email" CssClass="form-label" Text="Email (optional)" />
                    <asp:TextBox ID="email" runat="server" CssClass="form-control" MaxLength="254" TextMode="Email" autocomplete="email" />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="CourseList" CssClass="form-label" Text="Assigned course *" />
                    <asp:DropDownList ID="CourseList" runat="server" CssClass="form-select" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="CourseList" InitialValue="" ValidationGroup="TeacherDetails" Display="Dynamic" CssClass="text-danger" ErrorMessage="Choose an available course." />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="StatusList" CssClass="form-label" Text="Account status" />
                    <asp:DropDownList ID="StatusList" runat="server" CssClass="form-select">
                        <asp:ListItem Value="Active">Active</asp:ListItem>
                        <asp:ListItem Value="Inactive">Inactive</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <asp:Panel ID="InitialPasswordPanel" runat="server" CssClass="row g-3 mt-1">
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="pwd" CssClass="form-label" Text="Password *" />
                    <asp:TextBox ID="pwd" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" />
                    <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="pwd" ValidationGroup="TeacherDetails" Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter a password." />
                    <p class="form-text mb-0">Use 8–64 characters.</p>
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="confirmPwd" CssClass="form-label" Text="Confirm password *" />
                    <asp:TextBox ID="confirmPwd" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" />
                    <asp:RequiredFieldValidator ID="ConfirmationRequired" runat="server" ControlToValidate="confirmPwd" ValidationGroup="TeacherDetails" Display="Dynamic" CssClass="text-danger" ErrorMessage="Confirm the password." />
                </div>
            </asp:Panel>
            <p class="form-text mt-3">Teachers sign in using their username, staff ID and password.</p>
            <div class="d-flex gap-2">
                <asp:Button ID="SaveButton" runat="server" Text="Create Teacher Account" CssClass="btn btn-primary" ValidationGroup="TeacherDetails" OnClick="SaveButton_Click" />
                <asp:Button ID="CancelButton" runat="server" Text="Cancel Editing" Visible="false" CssClass="btn btn-outline-secondary" CausesValidation="false" OnClick="CancelButton_Click" />
            </div>
        </section>
        <section class="mt-4" aria-labelledby="teachersTitle">
            <h2 id="teachersTitle">Teachers</h2>
            <p>Deactivate an account to stop teacher access while keeping its records.</p>
            <asp:Button ID="RefreshButton" runat="server" Text="Refresh List" CssClass="btn btn-outline-primary mb-3" CausesValidation="false" OnClick="RefreshButton_Click" />
            <div class="table-responsive">
                <asp:GridView ID="Teachers" runat="server" AutoGenerateColumns="false" DataKeyNames="Id,accountStatus" CssClass="table table-bordered align-middle"
                    GridLines="None" UseAccessibleHeader="true" EmptyDataText="No teachers have been registered yet."
                    OnRowCommand="Teachers_RowCommand" OnRowDataBound="Teachers_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="FullName" HeaderText="Teacher" HtmlEncode="true" />
                        <asp:BoundField DataField="username" HeaderText="Username" HtmlEncode="true" />
                        <asp:BoundField DataField="staffId" HeaderText="Staff ID" HtmlEncode="true" />
                        <asp:BoundField DataField="email" HeaderText="Email" HtmlEncode="true" NullDisplayText="Not provided" />
                        <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" NullDisplayText="Not assigned" />
                        <asp:BoundField DataField="accountStatus" HeaderText="Status" HtmlEncode="true" />
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:Button ID="EditTeacherButton" runat="server" Text="Edit" CommandName="EditTeacher" CssClass="btn btn-outline-primary btn-sm me-2" CausesValidation="false" />
                                <asp:Button ID="ToggleTeacherButton" runat="server" CommandName="ToggleTeacher" CssClass="btn btn-outline-secondary btn-sm" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>
</asp:Content>
