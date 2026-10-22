<%@ Page Title="My Recovery Code" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RecoverySettings.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.RecoverySettings" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="row justify-content-center py-4 sob-auth" aria-labelledby="recoveryTitle">
        <div class="col-lg-8">
            <asp:HyperLink ID="DashboardLink" runat="server" CssClass="btn btn-outline-secondary mb-3" Text="Back to Dashboard" />
            <h1 id="recoveryTitle">My Recovery Code</h1>
            <p class="lead">Save a private code now so you can reset your password if you forget it.</p>
            <asp:Label ID="Message" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert" />
            <asp:Panel ID="NewCodePanel" runat="server" Visible="false" EnableViewState="false" CssClass="alert alert-success" role="status">
                <h2 class="h4">Keep this code somewhere safe</h2>
                <p>This is the only time it will be shown. Copy it into your password manager or write it down privately. Students can ask a parent to keep it safe.</p>
                <p><code class="fs-5" style="overflow-wrap:anywhere"><asp:Literal ID="NewCode" runat="server" EnableViewState="false" Mode="Encode" /></code></p>
                <p class="mb-0">This code replaces any previous code. Do not share it with other students or include it in screenshots.</p>
            </asp:Panel>
            <div class="card p-4">
                <p><strong>Current status:</strong> <asp:Literal ID="CodeStatus" runat="server" Mode="Encode" /></p>
                <p>You will need this code and your account details on the Forgot Password page. It works once. After resetting your password, sign in and create a new recovery code.</p>
                <asp:Panel ID="StudentEmailPanel" runat="server" CssClass="mb-3">
                    <asp:Label runat="server" AssociatedControlID="RecoveryEmail" CssClass="form-label" Text="Recovery email" />
                    <asp:TextBox ID="RecoveryEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="254" autocomplete="email" />
                    <p class="form-text">Use your email or a parent's email and remember this address. No email will be sent; the private code confirms that the account is yours.</p>
                </asp:Panel>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="CurrentPassword" CssClass="form-label" Text="Current password" />
                    <asp:TextBox ID="CurrentPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="current-password" />
                </div>
                <asp:Button ID="GenerateButton" runat="server" Text="Create / Replace Recovery Code" CssClass="btn btn-primary align-self-start" OnClick="GenerateButton_Click" />
                <p class="form-text mt-3 mb-0">Creating a replacement immediately stops the old code from working. Your password stays the same.</p>
            </div>
        </div>
    </main>
</asp:Content>
