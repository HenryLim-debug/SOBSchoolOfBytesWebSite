<%@ Page Title="First President Setup" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PresidentSetup.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.PresidentSetup" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="row justify-content-center py-4" aria-labelledby="setupTitle">
        <div class="col-lg-7">
            <h1 id="setupTitle">First President Setup</h1>
            <asp:Panel ID="UnavailablePanel" runat="server" Visible="false" CssClass="alert alert-info" role="status">
                <asp:Literal ID="UnavailableMessage" runat="server" Mode="Encode" />
                <p class="mt-2 mb-0"><a runat="server" href="~/StaffLogin">Go to Staff Login</a></p>
            </asp:Panel>
            <asp:Panel ID="SetupPanel" runat="server" CssClass="card p-4">
                <p>Create the president account for your school. This page is available only on the computer running the website and closes after the first president is created.</p>
                <asp:ValidationSummary runat="server" ValidationGroup="PresidentSetup" CssClass="alert alert-danger" HeaderText="Please check your details:" />
                <asp:Label ID="errMsg" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert" />
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="fname" CssClass="form-label" Text="First name" />
                    <asp:TextBox ID="fname" runat="server" CssClass="form-control" MaxLength="50" autocomplete="given-name" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="fname" ValidationGroup="PresidentSetup"
                        Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter your first name." />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="lname" CssClass="form-label" Text="Last name" />
                    <asp:TextBox ID="lname" runat="server" CssClass="form-control" MaxLength="50" autocomplete="family-name" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="lname" ValidationGroup="PresidentSetup"
                        Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter your last name." />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="username" CssClass="form-label" Text="Username" />
                    <asp:TextBox ID="username" runat="server" CssClass="form-control" MaxLength="30" autocomplete="username" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="username" ValidationGroup="PresidentSetup"
                        Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter your username." />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="staffId" CssClass="form-label" Text="Staff ID" />
                    <asp:TextBox ID="staffId" runat="server" CssClass="form-control" MaxLength="30" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="staffId" ValidationGroup="PresidentSetup"
                        Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter your staff id." />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="pwd" CssClass="form-label" Text="Password" />
                    <asp:TextBox ID="pwd" runat="server" CssClass="form-control" MaxLength="64" TextMode="Password" autocomplete="new-password" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="pwd" ValidationGroup="PresidentSetup"
                        Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter your password." />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="confirmPwd" CssClass="form-label" Text="Password confirmation" />
                    <asp:TextBox ID="confirmPwd" runat="server" CssClass="form-control" MaxLength="64" TextMode="Password" autocomplete="new-password" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="confirmPwd" ValidationGroup="PresidentSetup"
                        Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter your password confirmation." />
                </div>
                <p class="form-text">Username: 3–30 letters, numbers or underscores. Password: 8–64 characters (up to 72 UTF-8 bytes).</p>
                <asp:Button ID="CreateButton" runat="server" Text="Create President Account" CssClass="btn btn-primary align-self-start"
                    ValidationGroup="PresidentSetup" OnClick="CreateButton_Click" />
            </asp:Panel>
        </div>
    </main>
</asp:Content>
