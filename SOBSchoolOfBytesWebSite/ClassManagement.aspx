<%@ Page Title="Class Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ClassManagement.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.ClassManagement" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4" aria-labelledby="pageTitle">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
            <div><h1 id="pageTitle">Class Management</h1><p>Create small online classes and manage enrolment.</p></div>
            <a runat="server" href="~/PresidentDashboard" class="btn btn-outline-secondary">Back to Dashboard</a>
        </div>
        <asp:Panel ID="FeedbackPanel" runat="server" Visible="false" role="status"><asp:Literal ID="FeedbackMessage" runat="server" Mode="Encode" /></asp:Panel>
        <section class="card p-4 my-3" aria-labelledby="createClassTitle">
            <h2 id="createClassTitle" class="h4">Create Class</h2>
            <p>Each class has one teacher, up to 10 students and two 2-hour sessions each week for 10 weeks. All times are Malaysia time (UTC+8).</p>
            <asp:Panel ID="NoTeachersPanel" runat="server" Visible="false" CssClass="alert alert-info">
                Create an active teacher account and assign a course in <a runat="server" href="~/TeacherManagement">Manage Teachers</a> first.
            </asp:Panel>
            <asp:ValidationSummary runat="server" ValidationGroup="CreateClass" CssClass="alert alert-danger" HeaderText="Please check the class details:" />
            <div class="row g-3">
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="ClassName" Text="Class name *" CssClass="form-label" />
                    <asp:TextBox ID="ClassName" runat="server" CssClass="form-control" MaxLength="80" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ClassName" ValidationGroup="CreateClass" ErrorMessage="Enter a class name." CssClass="text-danger" Display="Dynamic" />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="TeacherList" Text="Teacher and course *" CssClass="form-label" />
                    <asp:DropDownList ID="TeacherList" runat="server" CssClass="form-select" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="TeacherList" InitialValue="" ValidationGroup="CreateClass" ErrorMessage="Choose a teacher and course." CssClass="text-danger" Display="Dynamic" />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="FirstMonday" Text="First week starts (Monday) *" CssClass="form-label" />
                    <asp:TextBox ID="FirstMonday" runat="server" TextMode="Date" CssClass="form-control" />
                </div>
            </div>
            <div class="row g-3 mt-1">
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="DayOne" Text="First session day" CssClass="form-label" />
                    <asp:DropDownList ID="DayOne" runat="server" CssClass="form-select" />
                    <asp:Label runat="server" AssociatedControlID="TimeOne" Text="Start time" CssClass="form-label mt-2" />
                    <asp:TextBox ID="TimeOne" runat="server" TextMode="Time" Text="15:00" CssClass="form-control" />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="DayTwo" Text="Second session day (later in the week)" CssClass="form-label" />
                    <asp:DropDownList ID="DayTwo" runat="server" CssClass="form-select" />
                    <asp:Label runat="server" AssociatedControlID="TimeTwo" Text="Start time" CssClass="form-label mt-2" />
                    <asp:TextBox ID="TimeTwo" runat="server" TextMode="Time" Text="15:00" CssClass="form-control" />
                </div>
            </div>
            <asp:Button ID="CreateButton" runat="server" Text="Create Class" CssClass="btn btn-primary align-self-start mt-4" ValidationGroup="CreateClass" OnClick="CreateButton_Click" />
        </section>
        <h2 class="mt-4">Classes</h2>
        <p>Closing enrolment stops new applications and new assignments to this group. Existing students keep their places and timetable.</p>
        <asp:Button ID="RefreshButton" runat="server" Text="Refresh List" CssClass="btn btn-outline-primary mb-3" CausesValidation="false" OnClick="RefreshButton_Click" />
        <div class="table-responsive">
            <asp:GridView ID="Classes" runat="server" AutoGenerateColumns="false" DataKeyNames="ClassId,IsOpen" CssClass="table table-bordered align-middle" GridLines="None"
                EmptyDataText="No classes have been created yet." OnRowCommand="Classes_RowCommand" OnRowDataBound="Classes_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
                    <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
                    <asp:BoundField DataField="TeacherName" HeaderText="Teacher" HtmlEncode="true" />
                    <asp:BoundField DataField="WeeklySchedule" HeaderText="Weekly sessions" HtmlEncode="true" />
                    <asp:BoundField DataField="CourseDates" HeaderText="Course dates" HtmlEncode="true" />
                    <asp:BoundField DataField="Places" HeaderText="Places" HtmlEncode="true" />
                    <asp:BoundField DataField="EnrolmentState" HeaderText="Enrolment" HtmlEncode="true" />
                    <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                        <a class="btn btn-outline-primary btn-sm mb-2" href='<%# ResolveUrl("~/ClassRoster?id=" + Eval("ClassId")) %>'>View Students</a>
                        <asp:Button ID="ToggleButton" runat="server" CssClass="btn btn-outline-secondary btn-sm" CommandName="ToggleClass" CausesValidation="false" />
                    </ItemTemplate></asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </main>
</asp:Content>
