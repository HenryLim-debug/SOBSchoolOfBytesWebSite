<%@ Page Title="Student Registration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="StudentRegistration.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.StudentRegistration" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="row justify-content-center py-4 sob-auth" aria-labelledby="registrationTitle">
        <div class="col-lg-8">
            <h1 id="registrationTitle">Student Registration</h1>
            <p class="lead">Create your account and get ready to explore computers and Python.</p>

            <asp:Panel ID="SuccessPanel" runat="server" Visible="false" CssClass="alert alert-success" role="status">
                <h2 class="h4">Account created</h2>
                <p>Your student account is waiting for the president's approval. You can sign in after it is approved.</p>
                <a class="btn btn-success" runat="server" href="~/StudentLogin">Go to Student Login</a>
                <a class="btn btn-outline-success" runat="server" href="~/">Back to Home</a>
            </asp:Panel>

            <asp:Panel ID="RegistrationPanel" runat="server" CssClass="card p-4">
                <p>Fields marked with * are required. New student accounts need the president's approval before signing in.</p>

                <asp:ValidationSummary ID="RegistrationValidationSummary" runat="server"
                    ValidationGroup="StudentRegistration" CssClass="alert alert-danger"
                    HeaderText="Please check the following details:" />
                <asp:CustomValidator ID="DetailsValidator" runat="server" ValidationGroup="StudentRegistration"
                    Display="None" ErrorMessage="One or more details are too long. Please shorten them." />
                <asp:Label ID="errMsg" runat="server" Visible="false" CssClass="alert alert-danger" role="alert" />

                <div class="row g-3">
                    <div class="col-md-6">
                        <asp:Label runat="server" AssociatedControlID="fname" CssClass="form-label" Text="First name *" />
                        <asp:TextBox ID="fname" runat="server" CssClass="form-control" MaxLength="50" autocomplete="given-name" />
                        <asp:RequiredFieldValidator ID="FirstNameRequired" runat="server" ControlToValidate="fname"
                            ValidationGroup="StudentRegistration" Display="Dynamic" CssClass="text-danger"
                            ErrorMessage="Enter your first name." />
                    </div>
                    <div class="col-md-6">
                        <asp:Label runat="server" AssociatedControlID="lname" CssClass="form-label" Text="Last name *" />
                        <asp:TextBox ID="lname" runat="server" CssClass="form-control" MaxLength="50" autocomplete="family-name" />
                        <asp:RequiredFieldValidator ID="LastNameRequired" runat="server" ControlToValidate="lname"
                            ValidationGroup="StudentRegistration" Display="Dynamic" CssClass="text-danger"
                            ErrorMessage="Enter your last name." />
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
                    <div class="col-12">
                        <asp:Label runat="server" AssociatedControlID="email" CssClass="form-label" Text="Email address (optional)" />
                        <asp:TextBox ID="email" runat="server" CssClass="form-control" TextMode="Email" MaxLength="254" autocomplete="email" />
                        <asp:RegularExpressionValidator ID="EmailFormatValidator" runat="server" ControlToValidate="email"
                            ValidationExpression="[^@\s]+@[^@\s]+\.[^@\s]+" ValidationGroup="StudentRegistration"
                            Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter a valid email address, or leave this field empty." />
                    </div>
                    <div class="col-12">
                        <asp:Label runat="server" AssociatedControlID="username" CssClass="form-label" Text="Username *" />
                        <asp:TextBox ID="username" runat="server" CssClass="form-control" MaxLength="30"
                            autocomplete="username" aria-describedby="usernameHelp" />
                        <div id="usernameHelp" class="form-text">Use 3-30 letters, numbers or underscores. You will use this to sign in.</div>
                        <asp:RequiredFieldValidator ID="UsernameRequired" runat="server" ControlToValidate="username"
                            ValidationGroup="StudentRegistration" Display="Dynamic" CssClass="text-danger"
                            ErrorMessage="Choose a username." />
                        <asp:RegularExpressionValidator ID="UsernameFormatValidator" runat="server" ControlToValidate="username"
                            ValidationExpression="[A-Za-z0-9_]{3,30}" ValidationGroup="StudentRegistration"
                            Display="Dynamic" CssClass="text-danger" ErrorMessage="Your username must have 3-30 letters, numbers or underscores." />
                    </div>
                    <div class="col-md-6">
                        <asp:Label runat="server" AssociatedControlID="pwd" CssClass="form-label" Text="Password *" />
                        <asp:TextBox ID="pwd" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64"
                            autocomplete="new-password" aria-describedby="passwordHelp" />
                        <div id="passwordHelp" class="form-text">Choose a password with 8-64 characters.</div>
                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="pwd"
                            ValidationGroup="StudentRegistration" Display="Dynamic" CssClass="text-danger"
                            ErrorMessage="Choose a password." />
                        <asp:CustomValidator ID="PasswordRulesValidator" runat="server" ControlToValidate="pwd"
                            ValidationGroup="StudentRegistration" Display="Dynamic" CssClass="text-danger"
                            OnServerValidate="PasswordRulesValidator_ServerValidate" EnableClientScript="false"
                            ErrorMessage="Choose a password with 8-64 characters." />
                    </div>
                    <div class="col-md-6">
                        <asp:Label runat="server" AssociatedControlID="confirmPwd" CssClass="form-label" Text="Confirm password *" />
                        <asp:TextBox ID="confirmPwd" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" />
                        <asp:RequiredFieldValidator ID="ConfirmPasswordRequired" runat="server" ControlToValidate="confirmPwd"
                            ValidationGroup="StudentRegistration" Display="Dynamic" CssClass="text-danger"
                            ErrorMessage="Type your password again." />
                        <asp:CompareValidator ID="PasswordsMatchValidator" runat="server" ControlToValidate="confirmPwd"
                            ControlToCompare="pwd" Operator="Equal" Type="String" ValidationGroup="StudentRegistration"
                            Display="Dynamic" CssClass="text-danger" ErrorMessage="Both passwords must match." />
                    </div>
                    <div class="col-12 mt-4">
                        <asp:Button ID="RegisterButton" runat="server" Text="Create Student Account" CssClass="btn btn-primary"
                            ValidationGroup="StudentRegistration" OnClick="RegisterButton_Click" />
                        <a class="btn btn-link" runat="server" href="~/">Back to Home</a>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </main>
</asp:Content>
