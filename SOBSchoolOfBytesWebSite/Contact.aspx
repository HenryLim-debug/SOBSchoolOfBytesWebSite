<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="SOBSchoolOfBytesWebSite.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="sob-info py-4" aria-labelledby="contactTitle">
        <section class="sob-info-hero" aria-labelledby="contactTitle">
            <p class="sob-info-eyebrow">SOB School of Bytes</p>
            <h1 id="contactTitle">We&rsquo;re here to help.</h1>
            <p class="lead">Have a question about joining, your account, or your classes? Get in touch or check the answers below.</p>
        </section>
        <div class="row g-4 my-3">
            <div class="col-lg-6">
                <section class="sob-info-card h-100" aria-labelledby="schoolContactTitle">
                    <h2 id="schoolContactTitle" class="h4">Contact the School</h2>
                    <p class="h5 mt-3">Henry Lim</p>
                    <address class="sob-info-contact">
                        <p><strong>Phone:</strong> <a href="tel:+60138077421">0138077421</a></p>
                        <p><strong>Email:</strong> <a href="mailto:henry0416@SOBmail.com">henry0416@SOBmail.com</a></p>
                    </address>
                    <div class="d-flex flex-wrap gap-2">
                        <a href="mailto:henry0416@SOBmail.com?subject=SOB%20School%20of%20Bytes%20Enquiry" class="btn btn-primary">Email Henry</a>
                        <a href="tel:+60138077421" class="btn btn-outline-primary">Call Henry</a>
                    </div>
                    <hr class="my-4" />
                    <p class="h5">Shalott</p>
                    <address class="sob-info-contact">
                        <p><strong>Phone:</strong> <a href="tel:+60162299315">0162299315</a></p>
                        <p><strong>Email:</strong> <a href="mailto:shalott@SOBmail.com">shalott@SOBmail.com</a></p>
                    </address>
                    <div class="d-flex flex-wrap gap-2">
                        <a href="mailto:shalott@SOBmail.com?subject=SOB%20School%20of%20Bytes%20Enquiry" class="btn btn-primary">Email Shalott</a>
                        <a href="tel:+60162299315" class="btn btn-outline-primary">Call Shalott</a>
                    </div>
                    <p class="small text-muted mt-3 mb-0">Email opens your email app. Calling depends on the calling apps available on your device.</p>
                </section>
            </div>
            <div class="col-lg-6">
                <section class="sob-info-card h-100" aria-labelledby="helpDetailsTitle">
                    <h2 id="helpDetailsTitle" class="h4">Help Us Understand Your Question</h2>
                    <p>Include your username, course or class group, and a short description of what happened. If you saw an error, include its message.</p>
                    <p>Please do not send your password. Younger students can ask a parent or guardian to help them contact the school.</p>
                    <p>For questions about a lesson or your marked work, ask your class teacher during your Teams lesson.</p>
                    <a runat="server" href="~/Login" class="btn btn-outline-primary">Go to Login</a>
                </section>
            </div>
        </div>
        <section class="my-5" aria-labelledby="helpTitle">
            <h2 id="helpTitle">Quick Help</h2>
            <p>Open a question to see what to do next.</p>
            <details class="sob-info-faq">
                <summary>How do I join as a new student?</summary>
                <p>Use <a runat="server" href="~/StudentRegistration">Student Registration</a> to create your account. The president must approve your account before you can sign in. You can then browse class groups and apply for a course.</p>
            </details>
            <details class="sob-info-faq">
                <summary>Why is my course application still pending?</summary>
                <p>The teacher of your requested class group needs to review it. Check My Course Applications on your student dashboard. Account approval and course approval are separate steps.</p>
            </details>
            <details class="sob-info-faq">
                <summary>Where do I find my online class?</summary>
                <p>Sign in and look under My Courses and Timetable. Select Join Online Class for your approved group. If the page says the meeting link has not been added, refresh the timetable or check with your teacher.</p>
            </details>
            <details class="sob-info-faq">
                <summary>Why can&rsquo;t I see all the chapters?</summary>
                <p>You need an active enrolment in that course, and your teacher must publish each chapter before it appears. Draft chapters are not shown to students.</p>
            </details>
            <details class="sob-info-faq">
                <summary>How do I submit my tutorial work?</summary>
                <p>Open Learning Materials for your course, select the published chapter, and open its submission page. Follow the allowed file types shown there and upload one file of up to 10 MB. After submitting, check that the new attempt appears in your submission history.</p>
            </details>
            <details class="sob-info-faq">
                <summary>Where can I see my marks and feedback?</summary>
                <p>Return to the chapter&rsquo;s submission page to check your submitted attempts. Your mark and feedback appear after your class teacher has reviewed the work.</p>
            </details>
            <details class="sob-info-faq">
                <summary>What should I do if I cannot sign in?</summary>
                <p>Check your username and password. Teachers and presidents must also enter their staff ID. New students need account approval first. If you still cannot sign in, contact Henry or Shalott using the details above.</p>
            </details>
        </section>
    </main>
</asp:Content>
