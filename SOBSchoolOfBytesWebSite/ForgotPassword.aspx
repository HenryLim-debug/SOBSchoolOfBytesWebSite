<%@ Page Title="Forgot Password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.ForgotPassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="row justify-content-center py-4 sob-auth" aria-labelledby="resetTitle">
        <div class="col-lg-7">
            <h1 id="resetTitle">Forgot Password</h1>
            <p class="lead">Enter your account details and the private recovery code you saved earlier.</p>
            <asp:Label ID="Message" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert" />
            <asp:Panel ID="SuccessPanel" runat="server" Visible="false" CssClass="alert alert-success" role="status">
                <h2 class="h4">Your password has been reset</h2>
                <p>Your old password and recovery code no longer work. Existing sessions have been signed out.</p>
                <p>Sign in with your new password, then open <strong>My Recovery Code</strong> to create a new code. Your account approval status stays the same.</p>
                <a runat="server" href="~/StudentLogin" class="btn btn-primary">Student Login</a>
                <a runat="server" href="~/StaffLogin" class="btn btn-outline-primary">Teacher / President Login</a>
            </asp:Panel>
            <asp:Panel ID="ResetPanel" runat="server" CssClass="card p-4">
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="AccountType" CssClass="form-label" Text="Account type" />
                    <asp:DropDownList ID="AccountType" runat="server" CssClass="form-select" AutoPostBack="true" CausesValidation="false" OnSelectedIndexChanged="AccountType_Changed">
                        <asp:ListItem Value="Student">Student</asp:ListItem>
                        <asp:ListItem Value="Staff">Teacher or President</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <asp:Panel ID="StudentFields" runat="server">
                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="Username" CssClass="form-label" Text="Username" />
                        <asp:TextBox ID="Username" runat="server" CssClass="form-control" MaxLength="50" autocomplete="username" />
                    </div>
                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="Email" CssClass="form-label" Text="Recovery email" />
                        <asp:TextBox ID="Email" runat="server" CssClass="form-control" MaxLength="254" TextMode="Email" autocomplete="email" />
                        <p class="form-text">The email address you entered when creating your recovery code.</p>
                    </div>
                </asp:Panel>
                <asp:Panel ID="StaffFields" runat="server" Visible="false">
                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="StaffId" CssClass="form-label" Text="Staff ID" />
                        <asp:TextBox ID="StaffId" runat="server" CssClass="form-control" MaxLength="30" />
                    </div>
                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="FirstName" CssClass="form-label" Text="First name" />
                        <asp:TextBox ID="FirstName" runat="server" CssClass="form-control" MaxLength="50" autocomplete="given-name" />
                    </div>
                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="LastName" CssClass="form-label" Text="Last name" />
                        <asp:TextBox ID="LastName" runat="server" CssClass="form-control" MaxLength="50" autocomplete="family-name" />
                        <p class="form-text">Use your first and last names as saved in your school account.</p>
                    </div>
                </asp:Panel>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="RecoveryCode" CssClass="form-label" Text="Private recovery code" />
                    <asp:TextBox ID="RecoveryCode" runat="server" CssClass="form-control" TextMode="Password" MaxLength="80" autocomplete="off" />
                    <p class="form-text">Paste your saved code, including or excluding the dashes.</p>
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="NewPassword" CssClass="form-label" Text="New password" />
                    <asp:TextBox ID="NewPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" />
                    <p class="form-text">Use 8 to 64 characters. A longer password is harder to guess.</p>
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="ConfirmPassword" CssClass="form-label" Text="Confirm new password" />
                    <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" />
                </div>
                <asp:Button ID="ResetButton" runat="server" Text="Reset Password" CssClass="btn btn-primary align-self-start" OnClick="ResetButton_Click" />
                <p class="form-text mt-3 mb-0">If the details or code do not match, check them carefully. Repeated attempts are limited for 15 minutes.</p>
            </asp:Panel>
            <p class="mt-3">Still signed in? <a runat="server" href="~/RecoverySettings">Create a recovery code first.</a></p>
            <p>Lost both your password and code? <a runat="server" href="~/Contact">Contact the school</a> for help verifying your identity. Names and IDs alone cannot reset a password.</p>
            <a runat="server" href="~/Login">Back to Login</a>
        </div>
    </main>
</asp:Content>
