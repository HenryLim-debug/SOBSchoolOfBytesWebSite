<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4" aria-labelledby="chooseLoginTitle">
        <h1 id="chooseLoginTitle">Welcome to School of Bytes</h1>
        <p class="lead">Choose your login to get started.</p>
        <div class="row g-4 mt-1">
            <section class="col-md-6">
                <div class="card h-100 p-4">
                    <h2>Students</h2>
                    <p>Sign in with your username and password.</p>
                    <p><a class="btn btn-primary" runat="server" href="~/StudentLogin">Student Login</a></p>
                    <p class="mb-0">New user? <a runat="server" href="~/StudentRegistration">Register now!</a></p>
                </div>
            </section>
            <section class="col-md-6">
                <div class="card h-100 p-4">
                    <h2>Teachers and President</h2>
                    <p>Sign in with your username, staff ID and password.</p>
                    <p class="mb-0"><a class="btn btn-outline-primary" runat="server" href="~/StaffLogin">Staff Login</a></p>
                </div>
            </section>
        </div>
    </main>
</asp:Content>
