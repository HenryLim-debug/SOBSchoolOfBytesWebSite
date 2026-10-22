<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Default.aspx.cs"
    Inherits="SOBSchoolOfBytesWebSite._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="sob-home">
        <section class="sob-hero" aria-labelledby="welcomeTitle">
            <div class="sob-hero-copy">
            <p class="sob-eyebrow">Big ideas start with little bytes</p>
            <h1 id="welcomeTitle">Welcome to<br />SOB School of Bytes</h1>

            <p class="lead">
                Discover computers, create new things, and start coding!
                Our learning platform helps primary-school students build
                computer skills through videos, step-by-step tutorials,
                online classes, and practical activities.
            </p>

            <div class="sob-hero-actions">
            <a href="#courses" class="btn btn-primary">
                Explore Our Courses
            </a>
            <a runat="server" href="~/StudentLogin" class="btn btn-outline-primary">Student Login</a>
            </div>
            <p class="sob-hero-note">Made for primary-school learners. Guided by teachers.</p>
            </div>
            <div class="sob-hero-art">
                <span class="sob-art-spark sob-spark-one" aria-hidden="true">+</span>
                <span class="sob-art-spark sob-spark-two" aria-hidden="true">&lt;/&gt;</span>
                <img runat="server" src="~/Images/sob-logo.png" alt="A cheerful computer beneath a rainbow, with the SOB School of Bytes name" width="1782" height="864" fetchpriority="high" />
                <div class="sob-art-caption"><span aria-hidden="true">&#9733;</span> A bright start to your digital adventure</div>
            </div>
        </section>
        <div class="sob-learning-facts" aria-label="How our courses work">
            <div><strong><asp:Literal ID="CourseCount" runat="server" Mode="Encode" /> courses</strong><span>Explore something new</span></div>
            <div><strong>10 chapters each</strong><span>Learn one step at a time</span></div>
            <div><strong>Up to 10 students</strong><span>In each class group</span></div>
            <div><strong>Teacher guidance</strong><span>Practice with feedback</span></div>
        </div>

        <section id="courses" class="sob-course-section py-4"
            aria-labelledby="coursesTitle">

            <h2 id="coursesTitle">Our Courses</h2>
            <p>Explore learning areas designed for young beginners.</p>
            <p><a runat="server" href="~/CourseCatalogue" class="btn btn-outline-primary">Browse Courses and Samples</a></p>

            <asp:Label ID="CourseMessage" runat="server" Visible="false" CssClass="alert alert-info d-block" />
            <div class="row g-4"><asp:Repeater ID="CourseCards" runat="server"><ItemTemplate>
                <div class="col-md-6 col-lg-3"><article class='<%# "card h-100 sob-course-card "+new[]{"sob-course-blue","sob-course-green","sob-course-gold","sob-course-purple"}[Container.ItemIndex%4] %>'><div class="card-body">
                    <span class="sob-course-symbol" aria-hidden="true"><%# (Container.ItemIndex+1).ToString("00") %></span>
                    <h3 class="h5 card-title"><%#: Eval("Title") %></h3><p class="card-text"><%#: Eval("Description") %></p>
                    <a class="btn btn-outline-primary" href='<%# ResolveUrl("~/CoursePreview?id="+Eval("CourseId")) %>'>Preview Course</a>
                </div></article></div>
            </ItemTemplate></asp:Repeater></div>
        </section>
    </main>

</asp:Content>
