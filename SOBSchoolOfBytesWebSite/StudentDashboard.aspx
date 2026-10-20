<%@ Page Title="Student Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="StudentDashboard.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.StudentDashboard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4" aria-labelledby="dashboardTitle">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
            <div>
                <h1 id="dashboardTitle">Student Dashboard</h1>
                <p class="lead">Welcome, <asp:Literal ID="StudentName" runat="server" Mode="Encode" />!</p>
            </div>
            <asp:Button ID="SignOutButton" runat="server" Text="Sign Out" CssClass="btn btn-outline-secondary"
                CausesValidation="false" OnClick="SignOutButton_Click" />
        </div>
        <div class="alert alert-success">Your student account is approved.</div>
        <p>Signed in as <strong><asp:Literal ID="StudentUsername" runat="server" Mode="Encode" /></strong>.</p>
        <h2>Explore your learning options</h2>
        <p>Discover computer fundamentals, internet safety, office applications and beginner Python.</p>
        <a class="btn btn-primary" runat="server" href="~/Courses">Browse Our Courses</a>

        <asp:Panel ID="FeedbackPanel" runat="server" Visible="false" role="status"><asp:Literal ID="FeedbackMessage" runat="server" Mode="Encode" /></asp:Panel>
        <section class="mt-4" aria-labelledby="myClassesTitle">
            <h2 id="myClassesTitle">My Courses and Timetable</h2>
            <p>All times are Malaysia time (UTC+8). Each course runs for 10 weeks with two 2-hour lessons each week.</p>
            <div class="table-responsive">
                <asp:GridView ID="MyClasses" runat="server" AutoGenerateColumns="false" DataKeyNames="EnrolmentId" CssClass="table table-bordered align-middle" GridLines="None"
                    EmptyDataText="You have not enrolled in a class yet. Browse Our Courses to choose one." OnRowCommand="MyClasses_RowCommand" OnRowDataBound="MyClasses_RowDataBound">
                    <Columns>
                    <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
                    <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
                    <asp:BoundField DataField="TeacherName" HeaderText="Teacher" HtmlEncode="true" />
                    <asp:BoundField DataField="WeeklySchedule" HeaderText="Weekly sessions" HtmlEncode="true" />
                    <asp:BoundField DataField="CourseDates" HeaderText="Course dates" HtmlEncode="true" />
                        <asp:TemplateField HeaderText="Enrolment"><ItemTemplate>
                            <asp:Button ID="WithdrawButton" runat="server" Text="Withdraw" CommandName="WithdrawClass" CssClass="btn btn-outline-danger btn-sm" CausesValidation="false" />
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>
</asp:Content>
