<%@ Page Title="Student Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="StudentLogin.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.StudentLogin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="row justify-content-center py-4" aria-labelledby="loginTitle">
        <div class="col-lg-6">
            <h1 id="loginTitle">Student Login</h1>
            <p class="lead">Welcome back! Sign in to your School of Bytes account.</p>
            <asp:Panel ID="SignedOutPanel" runat="server" Visible="false" CssClass="alert alert-success" role="status">
                You have signed out successfully.
            </asp:Panel>
            <div class="card p-4">
                <asp:ValidationSummary ID="LoginValidationSummary" runat="server" ValidationGroup="StudentLogin"
                    HeaderText="Please enter your login details:" CssClass="alert alert-danger" />
                <asp:Label ID="errMsg" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert" />

                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="username" CssClass="form-label" Text="Username" />
                    <asp:TextBox ID="username" runat="server" CssClass="form-control" MaxLength="50" autocomplete="username" />
                    <asp:RequiredFieldValidator ID="UsernameRequired" runat="server" ControlToValidate="username"
                        ValidationGroup="StudentLogin" Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter your username." />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="pwd" CssClass="form-label" Text="Password" />
                    <asp:TextBox ID="pwd" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="current-password" />
                    <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="pwd"
                        ValidationGroup="StudentLogin" Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter your password." />
                </div>
                <asp:Button ID="SignInButton" runat="server" Text="Sign In" CssClass="btn btn-primary align-self-start"
                    ValidationGroup="StudentLogin" OnClick="SignInButton_Click" />
                <p class="mt-3 mb-0">New user? <a runat="server" href="~/StudentRegistration">Register now!</a></p>
                <p class="mt-2 mb-0"><a runat="server" href="~/StaffLogin">Teacher or president? Use Staff Login</a></p>
                <p class="form-text mb-0">Your account needs the president's approval before you can sign in.</p>
            </div>
        </div>
    </main>
</asp:Content>
