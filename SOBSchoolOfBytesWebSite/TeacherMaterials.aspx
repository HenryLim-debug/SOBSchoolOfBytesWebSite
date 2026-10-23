<%@ Page Title="Teaching Materials" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TeacherMaterials.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.TeacherMaterials" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4">
        <div class="d-flex flex-wrap justify-content-between gap-3">
            <div><h1>Teaching Materials</h1><h2 class="h4"><asp:Literal ID="CourseTitle" runat="server" Mode="Encode" /></h2></div>
            <a runat="server" href="~/TeacherDashboard" class="btn btn-outline-secondary align-self-start">Back to Dashboard</a>
        </div>
        <p>You manage your own 10 chapters for this course. Your published materials are visible to students in your classes.</p>
        <asp:Panel ID="FeedbackPanel" runat="server" Visible="false" role="status"><asp:Literal ID="FeedbackMessage" runat="server" Mode="Encode" /></asp:Panel>
        <div class="table-responsive">
            <asp:GridView ID="Chapters" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="No chapters are available for your assigned course.">
                <Columns>
                    <asp:BoundField DataField="ChapterNumber" HeaderText="Chapter" />
                    <asp:BoundField DataField="Title" HeaderText="Title" HtmlEncode="true" />
                    <asp:BoundField DataField="PublicationStatus" HeaderText="Status" HtmlEncode="true" />
                    <asp:BoundField DataField="UpdatedAtLocal" HeaderText="Updated (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" HtmlEncode="true" />
                    <asp:TemplateField HeaderText="Action"><ItemTemplate>
                        <a class="btn btn-outline-primary btn-sm" href='<%# ResolveUrl("~/TeacherMaterials?chapter=" + Eval("ChapterId") + "#chapterEditor") %>'>Edit Chapter</a>
                    </ItemTemplate></asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
        <asp:Panel ID="EditorPanel" runat="server" Visible="false" CssClass="card p-4 mt-4">
            <section id="chapterEditor" aria-labelledby="editorTitle">
                <h2 id="editorTitle">Edit Chapter <asp:Literal ID="ChapterNumber" runat="server" Mode="Encode" /></h2>
                <p>Upload your lecture slides and the Word document containing the tutorial questions. Files are saved when you select Save Chapter. You can prepare a draft before the materials are ready.</p>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="ChapterTitle" Text="Chapter title *" CssClass="form-label" />
                    <asp:TextBox ID="ChapterTitle" runat="server" CssClass="form-control" MaxLength="120" ValidateRequestMode="Disabled" />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="Objectives" Text="Learning objectives" CssClass="form-label" />
                    <asp:TextBox ID="Objectives" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" />
                    <p class="form-text">What students will learn; up to 2,000 characters.</p>
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="SlidesUpload" Text="PowerPoint lecture slides" CssClass="form-label" />
                    <asp:FileUpload ID="SlidesUpload" runat="server" CssClass="form-control chapter-upload" accept=".ppt,.pptx" />
                    <p class="form-text">One .ppt or .pptx file, up to 10 MB. Choosing a new file replaces the current slides when saved.</p>
                    <asp:HyperLink ID="CurrentSlides" runat="server" Visible="false" CssClass="d-block mb-2" />
                    <asp:Button ID="DeleteSlidesButton" runat="server" Visible="false" Text="Delete Slides" CssClass="btn btn-outline-danger btn-sm"
                        CausesValidation="false" formnovalidate="formnovalidate" OnClick="DeleteSlidesButton_Click"
                        OnClientClick="return confirm('Delete the saved lecture slides?');" />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="TutorialUpload" Text="Word tutorial questions" CssClass="form-label" />
                    <asp:FileUpload ID="TutorialUpload" runat="server" CssClass="form-control chapter-upload" accept=".doc,.docx" />
                    <p class="form-text">Put all tutorial questions in one .doc or .docx file, up to 10 MB. Leave the file picker empty to keep the current document.</p>
                    <asp:HyperLink ID="CurrentTutorial" runat="server" Visible="false" CssClass="d-block mb-2" />
                    <asp:Button ID="DeleteTutorialButton" runat="server" Visible="false" Text="Delete Tutorial" CssClass="btn btn-outline-danger btn-sm"
                        CausesValidation="false" formnovalidate="formnovalidate" OnClick="DeleteTutorialButton_Click"
                        OnClientClick="return confirm('Delete the saved tutorial document?');" />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="VideoLink" Text="YouTube video link (optional)" CssClass="form-label" />
                    <asp:TextBox ID="VideoLink" runat="server" CssClass="form-control" MaxLength="2000" TextMode="Url" />
                    <p class="form-text">One HTTPS YouTube video link per chapter. Leave this empty if no video is needed.</p>
                </div>
                <div class="mb-3"><asp:CheckBox ID="PublishChapter" runat="server" Text=" Publish this chapter for enrolled students" /></div>
                <p class="form-text">Add slides, a tutorial document or a video before publishing. Uncheck the box and save to return a published chapter to draft. Delete buttons remove saved files immediately; deleting the last learning material returns the chapter to draft.</p>
                <asp:Button ID="SaveButton" runat="server" Text="Save Chapter" CssClass="btn btn-primary" OnClick="SaveButton_Click" />
            </section>
        </asp:Panel>
    </main>
    <script>
        document.querySelectorAll('.chapter-upload').forEach(function (input) {
            input.addEventListener('change', function () {
                input.setCustomValidity(input.files.length && input.files[0].size > 10 * 1024 * 1024 ? 'Choose a file no larger than 10 MB.' : '');
                input.reportValidity();
            });
        });
    </script>
</asp:Content>
