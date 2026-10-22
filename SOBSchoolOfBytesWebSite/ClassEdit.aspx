<%@ Page Title="Edit Class" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ClassEdit.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.ClassEdit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <a runat="server" href="~/ClassManagement" class="btn btn-outline-secondary mb-3">Back to Manage Classes</a>
    <h1>Edit Class</h1><p>Change a class before its first lesson. Teacher and enrolled-student timetable clashes are checked when saving.</p>
    <asp:Label ID="Message" runat="server" Visible="false" CssClass="alert alert-info d-block" role="status" />
    <asp:Panel ID="EditorPanel" runat="server" Visible="false" CssClass="card p-4">
        <h2 class="h4"><asp:Literal ID="CourseTitle" runat="server" Mode="Encode" /></h2>
        <p>The course and 10-student limit stay the same. All enrolled students and pending applications use the updated class details. Students can see the last change date on their timetable. Tell them about changes before the next lesson.</p>
        <p>If you change the teacher, the previous Teams link is removed so the new teacher can add their own.</p>
        <div class="row g-3">
            <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="ClassName" Text="Class name *" CssClass="form-label" /><asp:TextBox ID="ClassName" runat="server" CssClass="form-control" MaxLength="80" /></div>
            <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="TeacherList" Text="Teacher *" CssClass="form-label" /><asp:DropDownList ID="TeacherList" runat="server" CssClass="form-select" /></div>
            <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="FirstMonday" Text="First week starts (Monday) *" CssClass="form-label" /><asp:TextBox ID="FirstMonday" runat="server" CssClass="form-control" TextMode="Date" /></div>
        </div>
        <div class="row g-3 mt-2">
            <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="DayOne" Text="First session day" CssClass="form-label" /><asp:DropDownList ID="DayOne" runat="server" CssClass="form-select" /><asp:Label runat="server" AssociatedControlID="TimeOne" Text="Start time" CssClass="form-label mt-2" /><asp:TextBox ID="TimeOne" runat="server" CssClass="form-control" TextMode="Time" /></div>
            <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="DayTwo" Text="Second session day" CssClass="form-label" /><asp:DropDownList ID="DayTwo" runat="server" CssClass="form-select" /><asp:Label runat="server" AssociatedControlID="TimeTwo" Text="Start time" CssClass="form-label mt-2" /><asp:TextBox ID="TimeTwo" runat="server" CssClass="form-control" TextMode="Time" /></div>
        </div>
        <asp:Label runat="server" AssociatedControlID="Reason" Text="Reason for change *" CssClass="form-label mt-3" />
        <asp:TextBox ID="Reason" runat="server" CssClass="form-control mb-3" TextMode="MultiLine" Rows="2" MaxLength="500" />
        <asp:Button ID="SaveButton" runat="server" Text="Save Class Changes" CssClass="btn btn-primary align-self-start" OnClick="SaveButton_Click" />
    </asp:Panel>
    <h2 class="h4 mt-4">Change History</h2><div class="table-responsive"><asp:GridView ID="History" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered" GridLines="None" EmptyDataText="No changes recorded.">
        <Columns><asp:BoundField DataField="ChangedAt" HeaderText="Changed (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" HtmlEncode="true" /><asp:BoundField DataField="PreviousDetails" HeaderText="Before" HtmlEncode="true" /><asp:BoundField DataField="NewDetails" HeaderText="After" HtmlEncode="true" /><asp:BoundField DataField="Reason" HeaderText="Reason" HtmlEncode="true" /></Columns>
    </asp:GridView></div>
</main></asp:Content>
