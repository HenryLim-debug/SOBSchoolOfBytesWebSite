<%@ Page Title="Class Students" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ClassRoster.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.ClassRoster" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4" aria-labelledby="pageTitle">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
            <div><h1 id="pageTitle">Class Students</h1><p>View the students enrolled in this class.</p></div>
            <a runat="server" ID="BackLink" href="~/PresidentDashboard" class="btn btn-outline-secondary">Back to Dashboard</a>
        </div>
        <asp:Panel ID="FeedbackPanel" runat="server" Visible="false" role="status"><asp:Literal ID="FeedbackMessage" runat="server" Mode="Encode" /></asp:Panel>
        <asp:Panel ID="ClassPanel" runat="server" CssClass="card p-4 my-3">
            <h2 class="h4"><asp:Literal ID="ClassTitle" runat="server" Mode="Encode" /></h2>
            <p><asp:Literal ID="ClassDetails" runat="server" Mode="Encode" /></p>
            <p class="mb-0"><asp:Literal ID="ClassTimes" runat="server" Mode="Encode" /></p>
        </asp:Panel>
        <p>Inactive or suspended students keep their reserved place until an enrolment is withdrawn.</p>
        <div class="table-responsive">
            <asp:GridView ID="Students" runat="server" AutoGenerateColumns="false" DataKeyNames="EnrolmentId" CssClass="table table-bordered align-middle" GridLines="None"
                EmptyDataText="No students are enrolled in this class." OnRowCommand="Students_RowCommand" OnRowDataBound="Students_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="StudentName" HeaderText="Student" HtmlEncode="true" />
                    <asp:BoundField DataField="username" HeaderText="Username" HtmlEncode="true" />
                    <asp:BoundField DataField="accountStatus" HeaderText="Account status" HtmlEncode="true" />
                    <asp:TemplateField HeaderText="Enrolment"><ItemTemplate>
                        <asp:Button ID="WithdrawButton" runat="server" Text="Withdraw Enrolment" CommandName="WithdrawStudent" CssClass="btn btn-outline-danger btn-sm" CausesValidation="false" />
                    </ItemTemplate></asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </main>
</asp:Content>
