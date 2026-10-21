<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="sob-info py-4" aria-labelledby="aboutTitle">
        <section class="sob-info-hero" aria-labelledby="aboutTitle">
            <p class="sob-info-eyebrow">SOB School of Bytes</p>
            <h1 id="aboutTitle">Small steps. Big computer discoveries.</h1>
            <p class="lead">A place for primary-school students to explore computers, create useful things, and take their first steps with Python.</p>
            <p>Learn with your teacher in a small online class, practise with lesson materials, and build your confidence one chapter at a time.</p>
            <div class="d-flex flex-wrap gap-2 mt-3">
                <a runat="server" href="~/Default#courses" class="btn btn-primary">Explore Our Courses</a>
                <a runat="server" href="~/StudentRegistration" class="btn btn-outline-primary">Register as a Student</a>
            </div>
        </section>
        <section class="my-5" aria-labelledby="missionTitle">
            <h2 id="missionTitle">Our Mission</h2>
            <p>This website provides an organised and accessible learning platform that helps primary students understand the internet technology knowledge through videos, tutorials, online classes, practical assignments, and teacher feedback.</p>
            <div class="row g-3 mt-1">
                <div class="col-md-4"><article class="sob-info-card h-100">
                    <span class="sob-info-number" aria-hidden="true">1</span>
                    <h3 class="h5">Build computer skills</h3>
                    <p>Learn everyday computer tasks, Microsoft Office tools, and beginner Python through guided practice.</p>
                </article></div>
                <div class="col-md-4"><article class="sob-info-card h-100">
                    <span class="sob-info-number" aria-hidden="true">2</span>
                    <h3 class="h5">Learn with support</h3>
                    <p>Join a small class, submit your work, and use your teacher&rsquo;s marks and feedback to improve.</p>
                </article></div>
                <div class="col-md-4"><article class="sob-info-card h-100">
                    <span class="sob-info-number" aria-hidden="true">3</span>
                    <h3 class="h5">Keep learning organised</h3>
                    <p>Choose a suitable class group and see your approved classes together in your personal timetable.</p>
                </article></div>
            </div>
        </section>
        <section class="my-5" aria-labelledby="subjectsTitle">
            <h2 id="subjectsTitle">Four Courses to Explore</h2>
            <p>Each course has 10 chapters, with learning materials published by your teachers.</p>
            <div class="row g-3">
                <div class="col-md-6"><article class="sob-info-card h-100">
                    <h3 class="h5">Computer Fundamentals</h3>
                    <p>Discover computer parts, practise using a keyboard and mouse, and learn to organise files and folders.</p>
                </article></div>
                <div class="col-md-6"><article class="sob-info-card h-100">
                    <h3 class="h5">Internet and Online Safety</h3>
                    <p>Learn to search and browse, protect personal information, recognise suspicious messages, and behave responsibly online.</p>
                </article></div>
                <div class="col-md-6"><article class="sob-info-card h-100">
                    <h3 class="h5">Basic Office Applications</h3>
                    <p>Create documents in Word, presentations in PowerPoint, and simple tables in Excel.</p>
                </article></div>
                <div class="col-md-6"><article class="sob-info-card h-100">
                    <h3 class="h5">Beginner Python Programming</h3>
                    <p>Start with messages, variables and input, then explore conditions and loops through simple programming activities.</p>
                </article></div>
            </div>
        </section>
        <section class="sob-info-band my-5" aria-labelledby="classesTitle">
            <h2 id="classesTitle">How Our Classes Work</h2>
            <ul class="mb-0">
                <li>One teacher and a maximum of 10 students in each class group.</li>
                <li>Two 2-hour lessons each week, over 10 weeks: 40 scheduled teaching hours per course.</li>
                <li>Timetables shown in Malaysia time (UTC+8).</li>
                <li>Online classes joined through the Teams meeting link saved for your group.</li>
                <li>Published PowerPoint slides, Word tutorial questions, and optional YouTube lesson videos.</li>
            </ul>
        </section>
        <section class="my-5" aria-labelledby="startTitle">
            <h2 id="startTitle">Your Learning Journey</h2>
            <ol class="sob-info-steps">
                <li><strong>Register.</strong> Create a student account and wait for the president to approve it.</li>
                <li><strong>Choose a class.</strong> Sign in, browse available groups, and apply for a suitable timetable.</li>
                <li><strong>Get approved.</strong> The teacher responsible for your requested group reviews your application.</li>
                <li><strong>Learn and practise.</strong> Open your timetable, join your class, use published materials, and submit your work.</li>
                <li><strong>Review your feedback.</strong> Check your marks and your teacher&rsquo;s comments after marking.</li>
            </ol>
            <a runat="server" href="~/Contact" class="btn btn-outline-primary">Questions? Contact Us</a>
        </section>
    </main>
</asp:Content>
