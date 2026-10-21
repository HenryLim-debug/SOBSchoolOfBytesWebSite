<%@ Page Title="Course Lessons" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CourseLearning.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.CourseLearning" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4">
        <a runat="server" href="~/StudentDashboard" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
        <asp:Panel ID="FeedbackPanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="FeedbackMessage" runat="server" Mode="Encode" /></asp:Panel>
        <asp:Panel ID="CoursePanel" runat="server" Visible="false">
            <h1><asp:Literal ID="CourseTitle" runat="server" Mode="Encode" /></h1>
            <p><asp:Literal ID="CourseDescription" runat="server" Mode="Encode" /></p>
            <h2 class="h4 mt-4">Published Chapters</h2>
            <p>Choose a chapter to download the lecture slides and tutorial questions, or watch its lesson video.</p>
            <div class="table-responsive">
                <asp:GridView ID="Chapters" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="Your teacher has not published any chapters yet. Please check again later.">
                    <Columns>
                        <asp:BoundField DataField="ChapterNumber" HeaderText="Chapter" />
                        <asp:BoundField DataField="Title" HeaderText="Title" HtmlEncode="true" />
                        <asp:TemplateField HeaderText="Lesson"><ItemTemplate>
                            <a class="btn btn-primary btn-sm" href='<%# ResolveUrl("~/CourseLearning?course=" + Eval("CourseId") + "&chapter=" + Eval("ChapterId") + "#lesson") %>'>Open Lesson</a>
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Panel ID="LessonPanel" runat="server" Visible="false" CssClass="card p-4 mt-4">
                <article id="lesson" aria-labelledby="lessonTitle">
                    <p class="text-muted">Chapter <asp:Literal ID="ChapterNumber" runat="server" Mode="Encode" /></p>
                    <h2 id="lessonTitle"><asp:Literal ID="LessonTitle" runat="server" Mode="Encode" /></h2>
                    <asp:HyperLink ID="SubmitWorkLink" runat="server" Text="Submit Work / View Feedback" CssClass="btn btn-success mt-3" />
                    <div class="border rounded p-3 mt-3">
                        <h3 class="h5">My Chapter Checklist</h3>
                        <p>After reviewing the materials, mark this chapter complete. You can undo this later; it does not change your assignment or mark.</p>
                        <p role="status"><asp:Literal ID="CompletionStatus" runat="server" Mode="Encode" /></p>
                        <asp:Button ID="CompleteButton" runat="server" Text="Mark Chapter Complete" CommandArgument="complete" OnCommand="Completion_Command" CausesValidation="false" CssClass="btn btn-primary" />
                        <asp:Button ID="UndoCompleteButton" runat="server" Text="Mark as Not Completed" CommandArgument="undo" OnCommand="Completion_Command" CausesValidation="false" CssClass="btn btn-outline-secondary" />
                        <a runat="server" href="~/LearningProgress" class="btn btn-outline-primary">My Learning Progress</a>
                    </div>
                    <asp:Panel ID="DocumentsPanel" runat="server" Visible="false" CssClass="mt-4">
                        <h3 class="h5">Learning Materials</h3>
                        <p>Download the slides to review the lesson, then open the Word tutorial document for the questions.</p>
                        <div class="d-flex flex-wrap gap-2">
                            <asp:HyperLink ID="DownloadSlides" runat="server" Visible="false" CssClass="btn btn-primary" Text="Download Lecture Slides" />
                            <asp:HyperLink ID="DownloadTutorial" runat="server" Visible="false" CssClass="btn btn-outline-primary" Text="Download Tutorial Questions" />
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="ObjectivesPanel" runat="server" Visible="false" CssClass="mt-4">
                        <h3 class="h5">What You Will Learn</h3>
                        <div style="white-space:pre-wrap;overflow-wrap:anywhere"><asp:Literal ID="Objectives" runat="server" Mode="Encode" /></div>
                    </asp:Panel>
                    <asp:Panel ID="VideoPanel" runat="server" Visible="false" CssClass="mt-4">
                        <h3 class="h5">Watch the Video</h3>
                        <div class="ratio ratio-16x9"><asp:Literal ID="VideoEmbed" runat="server" Mode="PassThrough" /></div>
                        <p class="mt-2"><asp:HyperLink ID="WatchVideo" runat="server" Target="_blank" rel="noopener noreferrer" Text="Open video on YouTube" /></p>
                    </asp:Panel>
                    <asp:Panel ID="TutorialPanel" runat="server" Visible="false" CssClass="mt-4">
                        <h3 class="h5">Step-by-Step Tutorial</h3>
                        <div style="white-space:pre-wrap;overflow-wrap:anywhere"><asp:Literal ID="Tutorial" runat="server" Mode="Encode" /></div>
                    </asp:Panel>
                    <asp:Panel ID="PracticePanel" runat="server" Visible="false" CssClass="mt-4">
                        <h3 class="h5">Try It Yourself</h3>
                        <div style="white-space:pre-wrap;overflow-wrap:anywhere"><asp:Literal ID="Practice" runat="server" Mode="Encode" /></div>
                    </asp:Panel>
                </article>
            </asp:Panel>
        </asp:Panel>
    </main>
</asp:Content>
