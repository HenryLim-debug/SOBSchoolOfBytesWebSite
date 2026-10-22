<%@ Page Title="Our Courses" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CourseCatalogue.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.CourseCatalogue" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <h1>Explore Our Courses</h1><p class="lead">Try a sample activity and discover what you can learn.</p>
    <p>Each course has 10 chapters over 10 weeks, with two 2-hour lessons each week and up to 10 students per class.</p>
    <asp:Label ID="Message" runat="server" Visible="false" CssClass="alert alert-info d-block" />
    <div class="row g-4"><asp:Repeater ID="CourseCards" runat="server"><ItemTemplate>
        <div class="col-lg-6"><article class="card h-100 sob-course-card sob-course-purple"><div class="card-body">
            <h2 class="h4"><%#: Eval("Title") %></h2><p><%#: Eval("Description") %></p>
            <h3 class="h6">What you will learn</h3><p style="white-space:pre-wrap"><%#: Eval("LearningOutcomes") %></p>
            <a class="btn btn-primary" href='<%# ResolveUrl("~/CoursePreview?id="+Eval("CourseId")) %>'>See Sample Lesson</a>
        </div></article></div>
    </ItemTemplate></asp:Repeater></div>
    <p class="mt-4"><a runat="server" href="~/StudentRegistration">New student? Register here.</a> Already approved? <a runat="server" href="~/Courses">View class groups and apply.</a></p>
</main></asp:Content>
