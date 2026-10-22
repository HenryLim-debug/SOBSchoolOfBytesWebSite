<%@ Page Title="Course Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CourseManagement.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.CourseManagement" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <a runat="server" href="~/PresidentDashboard" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
    <h1>Course Management</h1>
    <p>Create courses and edit the information guests see. Every new course starts with 10 draft chapters.</p>
    <asp:Label ID="Message" runat="server" Visible="false" CssClass="alert alert-info d-block" role="status" />
    <div class="table-responsive"><asp:GridView ID="CourseGrid" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered" GridLines="None" EmptyDataText="No courses yet.">
        <Columns><asp:BoundField DataField="Title" HeaderText="Course" HtmlEncode="true" /><asp:CheckBoxField DataField="IsAvailable" HeaderText="Accepting new enrolments" />
        <asp:TemplateField HeaderText="Actions"><ItemTemplate><a class="btn btn-outline-primary" href='<%# ResolveUrl("~/CourseManagement?id="+Eval("CourseId")) %>'>Edit</a></ItemTemplate></asp:TemplateField></Columns>
    </asp:GridView></div>
    <a runat="server" href="~/CourseManagement" class="btn btn-outline-primary mb-3">Add New Course</a>
    <section class="card p-4">
        <h2><asp:Literal ID="EditorTitle" runat="server" Mode="Encode" Text="Add Course" /></h2>
        <asp:Label runat="server" AssociatedControlID="CourseTitle" CssClass="form-label" Text="Course title *" />
        <asp:TextBox ID="CourseTitle" runat="server" CssClass="form-control mb-3" MaxLength="100" />
        <asp:Label runat="server" AssociatedControlID="Description" CssClass="form-label" Text="Description *" />
        <asp:TextBox ID="Description" runat="server" CssClass="form-control mb-3" TextMode="MultiLine" Rows="3" MaxLength="1000" />
        <asp:Label runat="server" AssociatedControlID="Outcomes" CssClass="form-label" Text="Learning outcomes *" />
        <asp:TextBox ID="Outcomes" runat="server" CssClass="form-control mb-3" TextMode="MultiLine" Rows="3" MaxLength="2000" />
        <asp:Label runat="server" AssociatedControlID="SampleLesson" CssClass="form-label" Text="Public sample lesson" />
        <asp:TextBox ID="SampleLesson" runat="server" CssClass="form-control mb-3" TextMode="MultiLine" Rows="5" MaxLength="4000" />
        <asp:Label runat="server" AssociatedControlID="SampleActivity" CssClass="form-label" Text="Public sample activity" />
        <asp:TextBox ID="SampleActivity" runat="server" CssClass="form-control mb-3" TextMode="MultiLine" Rows="3" MaxLength="2000" />
        <p class="form-text">Samples are plain text visible to everyone. Private chapter files remain available only to enrolled students.</p>
        <asp:CheckBox ID="Available" runat="server" Checked="true" Text=" Available for new enrolments and public browsing" />
        <p class="form-text">Making a course unavailable stops new applications. Existing enrolled students keep their learning materials and class access.</p>
        <asp:Button ID="SaveButton" runat="server" Text="Save Course" CssClass="btn btn-primary align-self-start" OnClick="SaveButton_Click" />
    </section>
</main></asp:Content>
