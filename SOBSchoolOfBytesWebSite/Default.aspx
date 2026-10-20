<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Default.aspx.cs"
    Inherits="SOBSchoolOfBytesWebSite._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent"
    runat="server">

    <main>
        <section class="py-4" aria-labelledby="welcomeTitle">
            <h1 id="welcomeTitle">Welcome to SOB School of Bytes</h1>

            <p class="lead">
                Discover computers, create new things, and start coding!
                Our learning platform helps primary-school students build
                computer skills through videos, step-by-step tutorials,
                online classes, and practical activities.
            </p>

            <a href="#courses" class="btn btn-primary">
                Explore Our Courses
            </a>
        </section>

        <section id="courses" class="py-4"
            aria-labelledby="coursesTitle">

            <h2 id="coursesTitle">Our Courses</h2>
            <p>Explore four learning areas designed for young beginners.</p>
            <p><a runat="server" href="~/Courses" class="btn btn-outline-primary">View Classes and Enrol</a></p>

            <div class="row g-4">

                <div class="col-md-6 col-lg-3">
                    <article class="card h-100">
                        <div class="card-body">
                            <h3 class="h5 card-title">
                                Computer Fundamentals
                            </h3>
                            <p class="card-text">
                                Discover computer parts and learn to use
                                a keyboard and mouse, organise files,
                                and perform everyday computer tasks.
                            </p>
                        </div>
                    </article>
                </div>

                <div class="col-md-6 col-lg-3">
                    <article class="card h-100">
                        <div class="card-body">
                            <h3 class="h5 card-title">
                                Internet and Online Safety
                            </h3>
                            <p class="card-text">
                                Learn to browse and search the internet,
                                protect personal information, recognise
                                suspicious messages, and behave responsibly online.
                            </p>
                        </div>
                    </article>
                </div>

                <div class="col-md-6 col-lg-3">
                    <article class="card h-100">
                        <div class="card-body">
                            <h3 class="h5 card-title">
                                Basic Office Applications
                            </h3>
                            <p class="card-text">
                                Create simple documents in Microsoft Word,
                                presentations in PowerPoint, and
                                tables in Excel.
                            </p>
                        </div>
                    </article>
                </div>

                <div class="col-md-6 col-lg-3">
                    <article class="card h-100">
                        <div class="card-body">
                            <h3 class="h5 card-title">
                                Beginner Python Programming
                            </h3>
                            <p class="card-text">
                                Start coding with Python! Learn about
                                printing messages, variables, input,
                                conditions, and loops through simple activities.
                            </p>
                        </div>
                    </article>
                </div>

            </div>
        </section>
    </main>

</asp:Content>
