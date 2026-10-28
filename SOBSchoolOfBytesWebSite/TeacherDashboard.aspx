<%@ Page Title="Teacher Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TeacherDashboard.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.TeacherDashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4 sob-dashboard" aria-labelledby="teacherTitle">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3 sob-dashboard-welcome">
            <div><h1 id="teacherTitle">Teacher Dashboard</h1>
                <p>Welcome, <asp:Literal ID="TeacherName" runat="server" Mode="Encode" />.</p></div>
            <asp:Button ID="SignOutButton" runat="server" Text="Sign Out" CssClass="btn btn-outline-secondary"
                CausesValidation="false" OnClick="SignOutButton_Click" />
        </div>
        <nav class="sob-dashboard-menu" aria-labelledby="teacherToolsTitle">
            <h2 id="teacherToolsTitle">Teaching tools</h2>
            <p class="sob-dashboard-menu-hint">Select a category to view its options.</p>
            <div class="sob-dashboard-groups">
                <details class="sob-action-group">
                    <summary><span><span class="sob-action-group-title">Classes &amp; Enrolment</span><span class="sob-action-group-description">Review applications, your timetable and meeting links.</span></span></summary>
                    <ul class="sob-action-links">
                        <li><a runat="server" href="~/CourseApplications">Review Course Applications</a></li>
                        <li><a href="#teachingClassesTitle">My Classes and Timetable</a></li>
                        <li><a runat="server" href="~/ClassMeetings">Class Meeting Links</a></li>
                    </ul>
                </details>
                <details class="sob-action-group">
                    <summary><span><span class="sob-action-group-title">Teaching &amp; Assessment</span><span class="sob-action-group-description">Prepare learning materials and assess student work.</span></span></summary>
                    <ul class="sob-action-links">
                        <li><a runat="server" href="~/TeacherMaterials">Manage Teaching Materials</a></li>
                        <li><a runat="server" href="~/TeacherSubmissions">Review Student Work</a></li>
                    </ul>
                </details>
                <details class="sob-action-group">
                    <summary><span><span class="sob-action-group-title">Attendance &amp; Progress</span><span class="sob-action-group-description">Record attendance and follow your students' learning.</span></span></summary>
                    <ul class="sob-action-links">
                        <li><a runat="server" href="~/Attendance">Record Attendance</a></li>
                        <li><a runat="server" href="~/LearningProgress">Review Learning Progress</a></li>
                    </ul>
                </details>
                <details class="sob-action-group">
                    <summary><span><span class="sob-action-group-title">Hours &amp; Pay</span><span class="sob-action-group-description">Submit teaching hours and view your salary and payslips.</span></span></summary>
                    <ul class="sob-action-links">
                        <li><a runat="server" href="~/TeachingHours">My Teaching Hours</a></li>
                        <li><a runat="server" href="~/TeacherSalary">My Salary</a></li>
                        <li><a runat="server" href="~/Payslips">My Payslips</a></li>
                    </ul>
                </details>
            </div>
        </nav>
        <section class="card p-4 mt-3" aria-labelledby="teacherAccountTitle">
            <h2 id="teacherAccountTitle" class="h4">Your Account</h2>
            <p>Username: <asp:Literal ID="TeacherUsername" runat="server" Mode="Encode" /></p>
            <p>Staff ID: <asp:Literal ID="TeacherStaffId" runat="server" Mode="Encode" /></p>
            <p class="mb-0">Your teacher account is active.</p>
            <p class="mt-3 mb-0"><a runat="server" href="~/RecoverySettings">My Recovery Code</a></p>
        </section>
        <section class="mt-4" aria-labelledby="myCourseTitle">
            <h2 id="myCourseTitle">My Course</h2>
            <asp:Panel ID="NoCoursePanel" runat="server" Visible="false" CssClass="alert alert-info" role="status">
                Your course has not been assigned yet. Please contact the president.
            </asp:Panel>
            <asp:Panel ID="CourseErrorPanel" runat="server" Visible="false" CssClass="alert alert-warning" role="alert">
                Your course could not be loaded. Please reload the page and try again.
            </asp:Panel>
            <asp:Panel ID="AssignedCoursePanel" runat="server" Visible="false" CssClass="card p-4">
                <h3 class="h4"><asp:Literal ID="CourseTitle" runat="server" Mode="Encode" /></h3>
                <p><asp:Literal ID="CourseDescription" runat="server" Mode="Encode" /></p>
                <h4 class="h6">Learning Outcomes</h4>
                <p><asp:Literal ID="CourseOutcomes" runat="server" Mode="Encode" /></p>
                <p class="mb-1">10 chapters · 10 weeks · 4 teaching hours per week</p>
                <p class="mb-0">Course status: <asp:Literal ID="CourseStatus" runat="server" Mode="Encode" /></p>
            </asp:Panel>
        </section>
        <section class="mt-4" aria-labelledby="teachingClassesTitle">
            <h2 id="teachingClassesTitle">My Classes and Timetable</h2>
            <p>All times are Malaysia time (UTC+8). Open a class to view its enrolled students.</p>
            <asp:Panel ID="ClassesErrorPanel" runat="server" Visible="false" CssClass="alert alert-warning">Your classes could not be loaded. Please reload and try again.</asp:Panel>
            <div class="table-responsive">
                <asp:GridView ID="TeachingClasses" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="No classes have been created for you yet.">
                    <Columns>
                    <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
                    <asp:BoundField DataField="WeeklySchedule" HeaderText="Weekly sessions" HtmlEncode="true" />
                    <asp:BoundField DataField="CourseDates" HeaderText="Course dates" HtmlEncode="true" />
                    <asp:BoundField DataField="Places" HeaderText="Places" HtmlEncode="true" />
                        <asp:TemplateField HeaderText="Online class"><ItemTemplate><a class="btn btn-outline-primary btn-sm" href='<%# ResolveUrl("~/ClassMeetings?id=" + Eval("ClassId")) %>'>Meeting Link</a></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Students"><ItemTemplate><a class="btn btn-outline-primary btn-sm" href='<%# ResolveUrl("~/ClassRoster?id=" + Eval("ClassId")) %>'>View Students</a></ItemTemplate></asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>
</asp:Content>
