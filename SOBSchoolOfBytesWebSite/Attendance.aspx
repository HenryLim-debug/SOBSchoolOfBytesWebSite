<%@ Page Title="Class Attendance" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Attendance.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.Attendance" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <a ID="BackLink" runat="server" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
    <h1>Class Attendance</h1>
    <p>Choose a completed lesson to record or review attendance. Lesson times are Malaysia time (UTC+8).</p>
    <asp:Panel ID="ReadOnlyPanel" runat="server" Visible="false" CssClass="alert alert-info">You are viewing attendance as president. The assigned teacher records and corrects attendance.</asp:Panel>
    <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="MessageText" runat="server" Mode="Encode" /></asp:Panel>
    <asp:Label runat="server" AssociatedControlID="ClassList" Text="Class group" CssClass="form-label" />
    <asp:DropDownList ID="ClassList" runat="server" CssClass="form-select mb-3" AutoPostBack="true" OnSelectedIndexChanged="ClassList_SelectedIndexChanged" />
    <asp:Label runat="server" AssociatedControlID="SessionList" Text="Completed lesson" CssClass="form-label" />
    <asp:DropDownList ID="SessionList" runat="server" CssClass="form-select mb-3" AutoPostBack="true" OnSelectedIndexChanged="SessionList_SelectedIndexChanged" />
    <asp:Button ID="ReloadButton" runat="server" Text="Reload Attendance" CssClass="btn btn-outline-primary mb-3" CausesValidation="false" OnClick="ReloadButton_Click" />
    <asp:Panel ID="NoLessonsPanel" runat="server" Visible="false" CssClass="alert alert-info">There are no completed lessons available for this group yet. Attendance becomes available after each scheduled 2-hour lesson finishes.</asp:Panel>
    <asp:Panel ID="RosterPanel" runat="server" Visible="false">
        <h2 class="h4">Lesson Register</h2>
        <p>Students enrolled when the lesson started are listed. Existing records remain visible if a student later withdraws or re-enrols.</p>
        <p>Choose a status for every student. Optional notes are visible to that student and the president. Saving updates this lesson&rsquo;s records.</p>
        <div class="table-responsive">
            <asp:GridView ID="Students" runat="server" AutoGenerateColumns="false" DataKeyNames="EnrolmentId,VersionToken" CssClass="table table-bordered align-middle" GridLines="None" OnRowDataBound="Students_RowDataBound" EmptyDataText="There were no eligible students enrolled for this lesson.">
                <Columns>
                    <asp:BoundField DataField="StudentName" HeaderText="Student" HtmlEncode="true" />
                    <asp:BoundField DataField="username" HeaderText="Username" HtmlEncode="true" />
                    <asp:TemplateField HeaderText="Attendance"><ItemTemplate>
                        <asp:Label runat="server" AssociatedControlID="StatusList" Text="Attendance status" CssClass="visually-hidden" />
                        <asp:DropDownList ID="StatusList" runat="server" CssClass="form-select">
                            <asp:ListItem Value="">Not marked</asp:ListItem>
                            <asp:ListItem Value="Present">Present</asp:ListItem>
                            <asp:ListItem Value="Absent">Absent</asp:ListItem>
                            <asp:ListItem Value="Excused">Excused</asp:ListItem>
                        </asp:DropDownList>
                    </ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Note (optional)"><ItemTemplate>
                        <asp:Label runat="server" AssociatedControlID="NoteInput" Text="Attendance note" CssClass="visually-hidden" />
                        <asp:TextBox ID="NoteInput" runat="server" TextMode="MultiLine" Rows="2" MaxLength="500" CssClass="form-control" ValidateRequestMode="Disabled" />
                    </ItemTemplate></asp:TemplateField>
                    <asp:BoundField DataField="UpdatedLocal" HeaderText="Last saved (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" HtmlEncode="true" NullDisplayText="Not recorded" />
                </Columns>
            </asp:GridView>
        </div>
        <asp:Button ID="SaveButton" runat="server" Text="Save Attendance" CssClass="btn btn-primary" CausesValidation="false" OnClick="SaveButton_Click" />
    </asp:Panel>
</main>
</asp:Content>
