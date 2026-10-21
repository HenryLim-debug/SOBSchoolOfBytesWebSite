<%@ Page Title="My Attendance" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="StudentAttendance.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.StudentAttendance" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <a runat="server" href="~/StudentDashboard" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
    <h1>My Attendance</h1>
    <p>These are the attendance records your teachers have saved for you. Future and unmarked lessons are not counted as absences.</p>
    <asp:Panel ID="ErrorPanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status">Attendance could not be loaded. Please try again.</asp:Panel>
    <asp:Button ID="RefreshButton" runat="server" Text="Refresh Attendance" CssClass="btn btn-outline-primary mb-3" CausesValidation="false" OnClick="RefreshButton_Click" />
    <asp:Panel ID="ResultsPanel" runat="server" Visible="false">
        <h2 class="h4">Attendance Summary</h2>
        <p>Attendance percentage = Present &divide; (Present + Absent) &times; 100. Excused lessons are listed separately and excluded from the percentage.</p>
        <div class="table-responsive">
            <asp:GridView ID="Summary" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered" GridLines="None" EmptyDataText="No course enrolments or attendance records yet.">
                <Columns>
                    <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
                    <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
                    <asp:BoundField DataField="PresentCount" HeaderText="Present" HtmlEncode="true" />
                    <asp:BoundField DataField="AbsentCount" HeaderText="Absent" HtmlEncode="true" />
                    <asp:BoundField DataField="ExcusedCount" HeaderText="Excused" HtmlEncode="true" />
                    <asp:BoundField DataField="AttendancePercent" HeaderText="Attendance (%)" DataFormatString="{0:N1}" NullDisplayText="Not available yet" HtmlEncode="true" />
                </Columns>
            </asp:GridView>
        </div>
        <h2 class="h4 mt-4">Lesson History</h2>
        <p>Lesson dates and saved times are in Malaysia time (UTC+8). If a record looks incorrect, ask your class teacher.</p>
        <div class="table-responsive">
            <asp:GridView ID="History" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered" GridLines="None" EmptyDataText="Your teacher has not recorded any attendance yet.">
                <Columns>
                    <asp:BoundField DataField="SessionStartLocal" HeaderText="Lesson" DataFormatString="{0:dd MMM yyyy HH:mm}" HtmlEncode="true" />
                    <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
                    <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
                    <asp:BoundField DataField="Status" HeaderText="Attendance" HtmlEncode="true" />
                    <asp:BoundField DataField="Note" HeaderText="Teacher note" HtmlEncode="true" />
                    <asp:BoundField DataField="UpdatedLocal" HeaderText="Last saved" DataFormatString="{0:dd MMM yyyy HH:mm}" HtmlEncode="true" />
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>
</main>
</asp:Content>
