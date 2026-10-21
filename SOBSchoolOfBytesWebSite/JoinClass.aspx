<%@ Page Title="Join Online Class" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="JoinClass.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.JoinClass" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <h1>Join Online Class</h1>
    <p role="status"><asp:Literal ID="MessageText" runat="server" Mode="Encode" /></p>
    <a ID="BackLink" runat="server" class="btn btn-outline-primary">Back to Dashboard</a>
</main>
</asp:Content>
