<%@ Page Title="Learning Progress" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LearningProgress.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.LearningProgress" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<main class="py-4">
    <a ID="BackLink" runat="server" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
    <h1>Learning Progress</h1>
    <p>See progress in active course enrolments. Students mark a chapter complete after reviewing its materials; they can undo this at any time.</p>
    <p>Completion is measured against all 10 chapters in the course, including chapters not yet published. Only completed chapters that are currently published count towards the percentage. This is a student checklist, not a grade or proof that a video was watched.</p>
    <p>Submission and marking totals use the latest attempt for each published chapter. A new attempt waits for marking even if an earlier attempt was marked.</p>
    <asp:Panel ID="ErrorPanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="ErrorMessage" runat="server" Mode="Encode" /></asp:Panel>
    <asp:Button ID="RefreshButton" runat="server" Text="Refresh Progress" CausesValidation="false" CssClass="btn btn-outline-primary mb-3" />
    <asp:Panel ID="ResultsPanel" runat="server" Visible="false">
        <div class="table-responsive">
            <asp:GridView ID="Summary" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" OnRowCommand="Summary_RowCommand" EmptyDataText="No active enrolments are available to view.">
                <Columns>
                    <asp:BoundField DataField="StudentName" HeaderText="Student" HtmlEncode="true" />
                    <asp:BoundField DataField="username" HeaderText="Username" HtmlEncode="true" />
                    <asp:BoundField DataField="CourseTitle" HeaderText="Course" HtmlEncode="true" />
                    <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
                    <asp:BoundField DataField="TotalChapters" HeaderText="Total chapters" />
                    <asp:BoundField DataField="PublishedChapters" HeaderText="Published" />
                    <asp:BoundField DataField="CompletedChapters" HeaderText="Completed" />
                    <asp:BoundField DataField="CompletionPercent" HeaderText="Progress (%)" DataFormatString="{0:N1}" NullDisplayText="Not available" />
                    <asp:BoundField DataField="SubmittedChapters" HeaderText="Submitted" />
                    <asp:BoundField DataField="MarkedChapters" HeaderText="Marked" />
                    <asp:TemplateField HeaderText="Details"><ItemTemplate>
                        <asp:Button runat="server" Text="View Chapters" CommandName="ViewChapters" CommandArgument='<%# Eval("EnrolmentId") %>' CausesValidation="false" CssClass="btn btn-outline-primary btn-sm" />
                    </ItemTemplate></asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
        <asp:Panel ID="DetailsPanel" runat="server" Visible="false" CssClass="mt-4">
            <h2 class="h4"><asp:Literal ID="DetailsTitle" runat="server" Mode="Encode" /></h2>
            <p>Only published chapters appear below. Completion dates use Malaysia time (UTC+8).</p>
            <div class="table-responsive">
                <asp:GridView ID="Chapters" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered" GridLines="None" EmptyDataText="No chapters have been published for this course yet.">
                    <Columns>
                        <asp:BoundField DataField="ChapterNumber" HeaderText="Chapter" />
                        <asp:BoundField DataField="Title" HeaderText="Title" HtmlEncode="true" />
                        <asp:BoundField DataField="CompletionStatus" HeaderText="Student checklist" HtmlEncode="true" />
                        <asp:BoundField DataField="CompletedLocal" HeaderText="Completed (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" NullDisplayText="—" />
                        <asp:BoundField DataField="SubmissionStatus" HeaderText="Latest submission" HtmlEncode="true" />
                        <asp:BoundField DataField="Mark" HeaderText="Mark / 100" DataFormatString="{0:0.##}" NullDisplayText="Not marked" />
                        <asp:HyperLinkField HeaderText="Lesson" Text="Open Lesson" DataNavigateUrlFields="CourseId,ChapterId" DataNavigateUrlFormatString="~/CourseLearning?course={0}&amp;chapter={1}#lesson" />
                    </Columns>
                </asp:GridView>
            </div>
        </asp:Panel>
    </asp:Panel>
</main>
</asp:Content>
