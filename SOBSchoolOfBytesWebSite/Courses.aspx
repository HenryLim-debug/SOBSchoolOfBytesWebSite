<%@ Page Title="Apply for a Course" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Courses.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.Courses" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4" aria-labelledby="pageTitle">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
            <div><h1 id="pageTitle">Apply for a Course</h1><p>Choose a group and send an application to that group's teacher.</p></div>
            <a runat="server" href="~/StudentDashboard" class="btn btn-outline-secondary">Back to Dashboard</a>
        </div>
        <asp:Panel ID="FeedbackPanel" runat="server" Visible="false" role="status"><asp:Literal ID="FeedbackMessage" runat="server" Mode="Encode" /></asp:Panel>
        <p>Each course has 10 chapters over 10 weeks, with two 2-hour lessons each week. Each class has up to 10 students.</p>
        <p>All times are Malaysia time (UTC+8). Applications close when the first lesson starts.</p>
        <p>The teacher of your requested group will review your application. Approval adds you to that group. Pending applications do not reserve a place. Check your Student Dashboard for updates.</p>
        <asp:Button ID="RefreshButton" runat="server" Text="Refresh Classes" CssClass="btn btn-outline-primary mb-3" CausesValidation="false" OnClick="RefreshButton_Click" />
        <div class="table-responsive">
            <asp:GridView ID="AvailableClasses" runat="server" AutoGenerateColumns="false" DataKeyNames="ClassId" CssClass="table table-bordered align-middle" GridLines="None"
                EmptyDataText="No classes are open for enrolment yet. Please check again after the school opens a class."
                OnRowCommand="AvailableClasses_RowCommand" OnRowDataBound="AvailableClasses_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
                    <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
                    <asp:BoundField DataField="TeacherName" HeaderText="Teacher" HtmlEncode="true" />
                    <asp:BoundField DataField="WeeklySchedule" HeaderText="Weekly sessions" HtmlEncode="true" />
                    <asp:BoundField DataField="CourseDates" HeaderText="Course dates" HtmlEncode="true" />
                    <asp:BoundField DataField="Places" HeaderText="Places" HtmlEncode="true" />
                    <asp:TemplateField HeaderText="Application"><ItemTemplate>
                        <asp:Button ID="EnrolButton" runat="server" Text="Apply" CommandName="ApplyForCourse" CssClass="btn btn-primary btn-sm" CausesValidation="false" />
                    </ItemTemplate></asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </main>
</asp:Content>
