<%@ Page Title="Staff Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="StaffLogin.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.StaffLogin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="row justify-content-center py-4" aria-labelledby="staffLoginTitle">
        <div class="col-lg-6">
            <h1 id="staffLoginTitle">Teacher and President Login</h1>
            <p class="lead">Enter your staff account details to continue.</p>
            <asp:Panel ID="SuccessPanel" runat="server" Visible="false" CssClass="alert alert-success" role="status">
                <asp:Literal ID="SuccessMessage" runat="server" Mode="Encode" />
            </asp:Panel>
            <div class="card p-4">
                <asp:ValidationSummary runat="server" ValidationGroup="StaffLogin" CssClass="alert alert-danger" HeaderText="Please enter your login details:" />
                <asp:Label ID="errMsg" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert" />
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="username" CssClass="form-label" Text="Username" />
                    <asp:TextBox ID="username" runat="server" CssClass="form-control" MaxLength="50" autocomplete="username" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="username" ValidationGroup="StaffLogin"
                        Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter your username." />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="staffId" CssClass="form-label" Text="Staff ID" />
                    <asp:TextBox ID="staffId" runat="server" CssClass="form-control" MaxLength="30" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="staffId" ValidationGroup="StaffLogin"
                        Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter your staff id." />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="pwd" CssClass="form-label" Text="Password" />
                    <asp:TextBox ID="pwd" runat="server" CssClass="form-control" MaxLength="64" TextMode="Password" autocomplete="current-password" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="pwd" ValidationGroup="StaffLogin"
                        Display="Dynamic" CssClass="text-danger" ErrorMessage="Enter your password." />
                </div>
                <asp:Button ID="SignInButton" runat="server" Text="Sign In" CssClass="btn btn-primary align-self-start"
                    ValidationGroup="StaffLogin" OnClick="SignInButton_Click" />
                <p class="mt-3 mb-0"><a runat="server" href="~/StudentLogin">Go to Student Login</a></p>
            </div>
        </div>
    </main>
</asp:Content>
