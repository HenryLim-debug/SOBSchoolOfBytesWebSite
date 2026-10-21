<%@ Page Title="Review Student Work" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TeacherSubmissions.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.TeacherSubmissions" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4">
        <a runat="server" href="~/TeacherDashboard" class="btn btn-outline-secondary mb-3">Back to Dashboard</a>
        <h1>Review Student Work</h1>
        <p>Submissions from your class groups appear here. Each attempt keeps its own mark and feedback. Work awaiting review appears first.</p>
        <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="alert alert-warning" role="status"><asp:Literal ID="MessageText" runat="server" Mode="Encode" /></asp:Panel>
        <div class="table-responsive">
            <asp:GridView ID="Submissions" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered align-middle" GridLines="None" EmptyDataText="No student work has been submitted to your classes yet." AllowPaging="true" PageSize="20" OnPageIndexChanging="Submissions_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="StudentName" HeaderText="Student" HtmlEncode="true" />
                    <asp:BoundField DataField="StudentUsername" HeaderText="Username" HtmlEncode="true" />
                    <asp:BoundField DataField="ClassName" HeaderText="Class" HtmlEncode="true" />
                    <asp:BoundField DataField="ChapterNumber" HeaderText="Chapter" />
                    <asp:BoundField DataField="ChapterTitle" HeaderText="Title" HtmlEncode="true" />
                    <asp:BoundField DataField="SubmittedLocal" HeaderText="Submitted (Malaysia)" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                    <asp:BoundField DataField="ReviewStatus" HeaderText="Status" HtmlEncode="true" />
                    <asp:BoundField DataField="Mark" HeaderText="Mark / 100" DataFormatString="{0:0.##}" NullDisplayText="—" />
                    <asp:TemplateField HeaderText="Action"><ItemTemplate><a class="btn btn-outline-primary btn-sm" href='<%# ResolveUrl("~/TeacherSubmissions?id="+Eval("SubmissionId")+"#review") %>'>Review</a></ItemTemplate></asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
        <asp:Panel ID="ReviewPanel" runat="server" Visible="false" CssClass="card p-4 mt-4">
            <section id="review">
                <h2 class="h4">Submission Review</h2>
                <p><asp:Literal ID="ReviewHeading" runat="server" Mode="Encode" /></p>
                <asp:HyperLink ID="DownloadWork" runat="server" CssClass="btn btn-outline-primary mb-3" Text="Download Student Work" />
                <div class="mb-3"><asp:Label runat="server" AssociatedControlID="MarkInput" CssClass="form-label" Text="Mark out of 100 *" />
                    <asp:TextBox ID="MarkInput" runat="server" CssClass="form-control" MaxLength="6" inputmode="decimal" /></div>
                <div class="mb-3"><asp:Label runat="server" AssociatedControlID="FeedbackInput" CssClass="form-label" Text="Feedback" />
                    <asp:TextBox ID="FeedbackInput" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="6" ValidateRequestMode="Disabled" />
                    <p class="form-text">Up to 5,000 characters. Students can see the mark and feedback immediately after you save.</p></div>
                <asp:Button ID="SaveReviewButton" runat="server" Text="Save Mark and Feedback" CssClass="btn btn-primary" OnClick="SaveReviewButton_Click" />
            </section>
        </asp:Panel>
    </main>
</asp:Content>
