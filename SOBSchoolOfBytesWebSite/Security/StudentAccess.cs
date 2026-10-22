using System;
using System.Web;

namespace SOBSchoolOfBytesWebSite.Security
{
    public sealed class StudentAccount
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Username { get; set; }
        public Guid SecurityStamp { get; set; }
    }

    // The student pages use the same session and protected cookie as the staff pages.
    public static class StudentAccess
    {
        public static string GetFormKey(HttpContext context) { return AccountAccess.GetFormKey(context); }
        public static void SignIn(HttpContext context, StudentAccount student)
        {
            AccountAccess.SignIn(context, new UserAccount
            {
                Id = student.Id, FirstName = student.FirstName, Username = student.Username, Role = "Student", SecurityStamp = student.SecurityStamp
            });
        }
        public static void ClearSignIn(HttpContext context) { AccountAccess.ClearSignIn(context); }
        public static void PreventCaching(HttpResponse response) { AccountAccess.PreventCaching(response); }
    }
}
