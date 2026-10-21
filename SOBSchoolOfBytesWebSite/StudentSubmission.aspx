<%@ Page Title="Submit Your Work" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="StudentSubmission.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.StudentSubmission" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4">
        <a runat="server" href="~/StudentDashboard" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
        <h1>Submit Your Work</h1>
        <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="MessageText" runat="server" Mode="Encode" /></asp:Panel>
        <asp:Panel ID="WorkPanel" runat="server" Visible="false">
            <h2 class="h4"><asp:Literal ID="ChapterHeading" runat="server" Mode="Encode" /></h2>
            <p><asp:Literal ID="ClassHeading" runat="server" Mode="Encode" /></p>
            <asp:HyperLink ID="BackToLesson" runat="server" Text="Back to Lesson" CssClass="btn btn-outline-primary mb-3" />
            <section class="card p-4 mb-4">
                <h3 class="h5">Upload Completed Work</h3>
                <p>Upload one file for this chapter. Each new submission is a separate attempt; your earlier work and its feedback stay in the history below.</p>
                <asp:Label runat="server" AssociatedControlID="WorkUpload" Text="Completed work" CssClass="form-label" />
                <asp:FileUpload ID="WorkUpload" runat="server" CssClass="form-control" accept=".doc,.docx,.ppt,.pptx,.xlsx,.pdf,.py" />
                <p class="form-text">Word, PowerPoint, Excel (.xlsx), PDF or Python (.py). Maximum 10 MB per file.</p>
                <asp:Button ID="SubmitButton" runat="server" Text="Submit Work" CssClass="btn btn-primary align-self-start" OnClick="SubmitButton_Click" />
            </section>
            <h2 class="h4">My Submission History</h2>
            <p>Your latest attempt appears first. Marks are out of 100. Check here for your class teacher's feedback.</p>
            <div class="table-responsive">
                <asp:GridView ID="History" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="You have not submitted work for this chapter yet.">
                    <Columns>
                        <asp:BoundField DataField="SubmissionId" HeaderText="Submission" />
                        <asp:BoundField DataField="SubmittedLocal" HeaderText="Submitted (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                        <asp:TemplateField HeaderText="My file"><ItemTemplate>
                            <a href='<%# ResolveUrl("~/SubmissionDownload.ashx?id="+Eval("SubmissionId")) %>'><asp:Literal runat="server" Mode="Encode" Text='<%# Eval("FileName") %>' /></a>
                        </ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="ReviewStatus" HeaderText="Status" HtmlEncode="true" />
                        <asp:BoundField DataField="Mark" HeaderText="Mark / 100" DataFormatString="{0:0.##}" NullDisplayText="—" />
                        <asp:TemplateField HeaderText="Teacher feedback"><ItemTemplate><div style="white-space:pre-wrap;overflow-wrap:anywhere"><asp:Literal runat="server" Mode="Encode" Text='<%# Eval("Feedback") %>' /></div></ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="GradedLocal" HeaderText="Reviewed (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" NullDisplayText="—" />
                    </Columns>
                </asp:GridView>
            </div>
        </asp:Panel>
    </main>
    <script>
        var workInput=document.getElementById('<%= WorkUpload.ClientID %>');
        if(workInput) workInput.addEventListener('change',function(){workInput.setCustomValidity(workInput.files.length && workInput.files[0].size>10*1024*1024 ? 'Choose a file no larger than 10 MB.' : '');workInput.reportValidity();});
    </script>
</asp:Content>
