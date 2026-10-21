<%@ Page Title="Class Meeting Links" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ClassMeetings.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.ClassMeetings" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <a ID="BackLink" runat="server" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
    <h1>Class Meeting Links</h1>
    <p>Create your meeting in Microsoft Teams, then paste its join link here. Each class group has one current link. You can use your recurring meeting link or replace it before the next lesson.</p>
    <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="MessageText" runat="server" Mode="Encode" /></asp:Panel>
    <asp:Label runat="server" AssociatedControlID="ClassList" Text="Class group" CssClass="form-label" />
    <asp:DropDownList ID="ClassList" runat="server" CssClass="form-select mb-3" AutoPostBack="true" OnSelectedIndexChanged="ClassList_SelectedIndexChanged" />
    <asp:Button ID="ReloadButton" runat="server" Text="Reload Saved Link" CssClass="btn btn-outline-primary mb-3" CausesValidation="false" OnClick="ReloadButton_Click" />
    <asp:Panel ID="EditorPanel" runat="server" Visible="false" CssClass="card p-4">
        <h2 class="h4"><asp:Literal ID="ClassTitle" runat="server" Mode="Encode" /></h2>
        <p><asp:Literal ID="ScheduleText" runat="server" Mode="Encode" /></p>
        <asp:Label runat="server" AssociatedControlID="MeetingUrl" Text="Microsoft Teams meeting join link" CssClass="form-label" />
        <asp:TextBox ID="MeetingUrl" runat="server" CssClass="form-control mb-3" MaxLength="2048" ValidateRequestMode="Disabled" style="max-width:100%" />
        <p class="text-muted">Paste the full HTTPS join link, including any meeting passcode in the link. Students in this group can use it as soon as you save.</p>
        <p><asp:Literal ID="SavedStatus" runat="server" Mode="Encode" /></p>
        <div class="d-flex flex-wrap gap-2">
            <asp:Button ID="SaveButton" runat="server" Text="Save Meeting Link" CssClass="btn btn-primary" CausesValidation="false" OnClick="SaveButton_Click" />
            <asp:HyperLink ID="OpenLink" runat="server" Text="Open Saved Meeting" Target="_blank" rel="noopener noreferrer" CssClass="btn btn-outline-primary" Visible="false" />
            <asp:Button ID="RemoveButton" runat="server" Text="Remove Meeting Link" CssClass="btn btn-outline-danger" Visible="false" CausesValidation="false" OnClick="RemoveButton_Click" OnClientClick="return confirm('Remove the saved meeting link from this class group?');" />
        </div>
    </asp:Panel>
</main>
</asp:Content>
