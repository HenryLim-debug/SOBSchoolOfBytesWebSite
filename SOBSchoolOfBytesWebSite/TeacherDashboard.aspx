<%@ Page Title="Teacher Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TeacherDashboard.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.TeacherDashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4" aria-labelledby="teacherTitle">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
            <div><h1 id="teacherTitle">Teacher Dashboard</h1>
                <p>Welcome, <asp:Literal ID="TeacherName" runat="server" Mode="Encode" />.</p></div>
            <asp:Button ID="SignOutButton" runat="server" Text="Sign Out" CssClass="btn btn-outline-secondary"
                CausesValidation="false" OnClick="SignOutButton_Click" />
        </div>
        <p class="mt-3 d-flex flex-wrap gap-2">
            <a runat="server" href="~/CourseApplications" class="btn btn-primary">Review Course Applications</a>
            <a runat="server" href="~/TeacherMaterials" class="btn btn-outline-primary">Manage Teaching Materials</a>
            <a runat="server" href="~/TeacherSubmissions" class="btn btn-outline-primary">Review Student Work</a>
            <a runat="server" href="~/TeachingHours" class="btn btn-outline-primary">My Teaching Hours</a>
        </p>
        <section class="card p-4 mt-3" aria-labelledby="teacherAccountTitle">
            <h2 id="teacherAccountTitle" class="h4">Your Account</h2>
            <p>Username: <asp:Literal ID="TeacherUsername" runat="server" Mode="Encode" /></p>
            <p>Staff ID: <asp:Literal ID="TeacherStaffId" runat="server" Mode="Encode" /></p>
            <p class="mb-0">Your teacher account is active.</p>
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
                        <asp:TemplateField HeaderText="Students"><ItemTemplate><a class="btn btn-outline-primary btn-sm" href='<%# ResolveUrl("~/ClassRoster?id=" + Eval("ClassId")) %>'>View Students</a></ItemTemplate></asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>
</asp:Content>
