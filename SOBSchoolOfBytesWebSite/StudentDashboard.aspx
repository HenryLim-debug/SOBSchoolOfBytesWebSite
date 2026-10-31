<%@ Page Title="Student Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="StudentDashboard.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.StudentDashboard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4 sob-dashboard" aria-labelledby="dashboardTitle">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3 sob-dashboard-welcome">
            <div>
                <h1 id="dashboardTitle">Student Dashboard</h1>
                <p class="lead">Welcome, <asp:Literal ID="StudentName" runat="server" Mode="Encode" />!</p>
            </div>
            <asp:Button ID="SignOutButton" runat="server" Text="Sign Out" CssClass="btn btn-outline-secondary"
                CausesValidation="false" OnClick="SignOutButton_Click" />
        </div>
        <div class="alert alert-success">Your student account is approved.</div>
        <p>Signed in as <strong><asp:Literal ID="StudentUsername" runat="server" Mode="Encode" /></strong>.</p>
        <nav class="sob-dashboard-menu" aria-labelledby="studentOptionsTitle">
            <h2 id="studentOptionsTitle">Explore your learning options</h2>
            <p class="sob-dashboard-menu-hint">Select a category to view its options.</p>
            <div class="sob-dashboard-groups">
                <details class="sob-action-group">
                    <summary><span><span class="sob-action-group-title">Courses &amp; Applications</span><span class="sob-action-group-description">Explore courses and check your class applications.</span></span></summary>
                    <ul class="sob-action-links">
                        <li><a runat="server" href="~/Courses">Browse Our Courses</a></li>
                        <li><a href="#myApplicationsTitle">My Course Applications</a></li>
                    </ul>
                </details>
                <details class="sob-action-group">
                    <summary><span><span class="sob-action-group-title">Classes &amp; Learning</span><span class="sob-action-group-description">Check your timetable, join online classes and open learning materials.</span></span></summary>
                    <ul class="sob-action-links">
                        <li><a href="#myClassesTitle">My Timetable &amp; Learning Materials</a></li>
                    </ul>
                </details>
                <details class="sob-action-group">
                    <summary><span><span class="sob-action-group-title">Attendance &amp; Progress</span><span class="sob-action-group-description">Review your attendance and follow your learning progress.</span></span></summary>
                    <ul class="sob-action-links">
                        <li><a runat="server" href="~/StudentAttendance">My Attendance</a></li>
                        <li><a runat="server" href="~/LearningProgress">My Learning Progress</a></li>
                    </ul>
                </details>
                <details class="sob-action-group">
                    <summary><span><span class="sob-action-group-title">My Account</span><span class="sob-action-group-description">Keep a recovery code to help you reset a forgotten password.</span></span></summary>
                    <ul class="sob-action-links">
                        <li><a runat="server" href="~/RecoverySettings">My Recovery Code</a></li>
                    </ul>
                </details>
            </div>
        </nav>

        <asp:Panel ID="FeedbackPanel" runat="server" Visible="false" role="status"><asp:Literal ID="FeedbackMessage" runat="server" Mode="Encode" /></asp:Panel>
        <section class="mt-4" aria-labelledby="myApplicationsTitle">
            <h2 id="myApplicationsTitle">My Course Applications</h2>
            <p>Pending applications are waiting for the teacher of your requested group to decide. Approval adds the assigned group to your timetable. Rejected or cancelled applications stay here as history; you may apply again.</p>
            <asp:Button ID="RefreshApplicationsButton" runat="server" Text="Refresh Applications and Timetable" CssClass="btn btn-outline-primary mb-3" CausesValidation="false" OnClick="RefreshApplicationsButton_Click" />
            <div class="table-responsive">
                <asp:GridView ID="MyApplications" runat="server" AutoGenerateColumns="false" DataKeyNames="ApplicationId"
                    CssClass="table table-bordered align-middle" GridLines="None" UseAccessibleHeader="true"
                    EmptyDataText="You have not submitted any course applications yet."
                    OnRowCommand="MyApplications_RowCommand" OnRowDataBound="MyApplications_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
                        <asp:BoundField DataField="ClassName" HeaderText="Requested group" HtmlEncode="true" />
                        <asp:TemplateField HeaderText="Requested timetable"><ItemTemplate>
                            <asp:Literal runat="server" Mode="Encode" Text='<%# Eval("WeeklySchedule") %>' /><br />
                            <small><asp:Literal runat="server" Mode="Encode" Text='<%# Eval("CourseDates") %>' /></small>
                        </ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="Status" HeaderText="Application status" HtmlEncode="true" />
                        <asp:TemplateField HeaderText="Assigned group"><ItemTemplate>
                            <asp:Literal runat="server" Mode="Encode" Text='<%# Eval("AssignedGroup") %>' /><br />
                            <small><asp:Literal runat="server" Mode="Encode" Text='<%# Eval("AssignedTeacher") %>' /></small>
                        </ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="AppliedAtLocal" HeaderText="Applied (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" HtmlEncode="true" />
                        <asp:TemplateField HeaderText="Action"><ItemTemplate>
                            <asp:Button ID="CancelButton" runat="server" Text="Cancel Application" CommandName="CancelApplication" CssClass="btn btn-outline-danger btn-sm" CausesValidation="false" />
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>
        <section class="mt-4" aria-labelledby="myClassesTitle">
            <h2 id="myClassesTitle">My Courses and Timetable</h2>
            <p>All times are Malaysia time (UTC+8). Each course runs for 10 weeks with two 2-hour lessons each week. Withdrawing frees your place; the original approval remains in your application history.</p>
            <div class="table-responsive">
                <asp:GridView ID="MyClasses" runat="server" AutoGenerateColumns="false" DataKeyNames="EnrolmentId" CssClass="table table-bordered align-middle" GridLines="None"
                    EmptyDataText="No approved course enrolments yet. Browse Our Courses to apply, or check your application status above." OnRowCommand="MyClasses_RowCommand" OnRowDataBound="MyClasses_RowDataBound">
                    <Columns>
                    <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
                    <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
                    <asp:BoundField DataField="TeacherName" HeaderText="Teacher" HtmlEncode="true" />
                    <asp:BoundField DataField="LastChangedAt" HeaderText="Class last changed (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" HtmlEncode="true" NullDisplayText="No changes" />
                    <asp:BoundField DataField="WeeklySchedule" HeaderText="Weekly sessions" HtmlEncode="true" />
                    <asp:BoundField DataField="CourseDates" HeaderText="Course dates" HtmlEncode="true" />
                        <asp:TemplateField HeaderText="Online class"><ItemTemplate>
                            <asp:HyperLink runat="server" Text="Join Online Class" CssClass="btn btn-primary btn-sm" Target="_blank" rel="noopener noreferrer"
                                NavigateUrl='<%# ResolveUrl("~/JoinClass?id=" + Eval("ClassId")) %>' Visible='<%# Convert.ToBoolean(Eval("HasMeetingLink")) %>' />
                            <asp:Literal runat="server" Text="Meeting link not added yet" Mode="Encode" Visible='<%# !Convert.ToBoolean(Eval("HasMeetingLink")) %>' />
                        </ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Learning"><ItemTemplate>
                            <a class="btn btn-outline-primary btn-sm" href='<%# ResolveUrl("~/CourseLearning?course=" + Eval("CourseId")) %>'>Learning Materials</a>
                        </ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Enrolment"><ItemTemplate>
                            <asp:Button ID="WithdrawButton" runat="server" Text="Withdraw" CommandName="WithdrawClass" CssClass="btn btn-outline-danger btn-sm" CausesValidation="false" />
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>
</asp:Content>
