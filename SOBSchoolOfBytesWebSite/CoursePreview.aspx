<%@ Page Title="Course Preview" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CoursePreview.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.CoursePreview" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <a runat="server" href="~/CourseCatalogue" class="btn btn-outline-secondary mb-3">All Courses</a>
    <asp:Label ID="Message" runat="server" Visible="false" CssClass="alert alert-info d-block" />
    <asp:Panel ID="PreviewPanel" runat="server" Visible="false">
        <h1><asp:Literal ID="CourseTitle" runat="server" Mode="Encode" /></h1><p class="lead"><asp:Literal ID="Description" runat="server" Mode="Encode" /></p>
        <h2 class="h4">What You Will Learn</h2><p style="white-space:pre-wrap"><asp:Literal ID="Outcomes" runat="server" Mode="Encode" /></p>
        <section class="card p-4 my-4"><h2 class="h4">Sample Lesson</h2><div style="white-space:pre-wrap"><asp:Literal ID="Sample" runat="server" Mode="Encode" /></div>
            <h3 class="h5 mt-4">Try It Yourself</h3><div style="white-space:pre-wrap"><asp:Literal ID="Activity" runat="server" Mode="Encode" /></div></section>
        <p>This is a public preview. Full chapters, slides, tutorial files and assignments are available after your course application is approved.</p>
        <a runat="server" href="~/StudentRegistration" class="btn btn-primary">Register as a Student</a>
        <a runat="server" href="~/Courses" class="btn btn-outline-primary">View Class Groups and Apply</a>
    </asp:Panel>
</main></asp:Content>
