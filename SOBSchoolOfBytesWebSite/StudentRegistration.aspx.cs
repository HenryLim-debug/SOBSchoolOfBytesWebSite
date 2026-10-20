using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;

namespace SOBSchoolOfBytesWebSite
{
    public partial class StudentRegistration : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            // Store the key so ASP.NET keeps the same anonymous session between showing and submitting the form.
            if (Session["RegistrationViewStateKey"] == null)
            {
                Session["RegistrationViewStateKey"] = Guid.NewGuid().ToString("N");
            }
            ViewStateUserKey = (string)Session["RegistrationViewStateKey"];
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Redirect after creating an account so refreshing does not submit the form again.
            if (!IsPostBack && Request.QueryString["registered"] == "1"
                && Session["StudentRegistrationComplete"] is bool
                && (bool)Session["StudentRegistrationComplete"])
            {
                Session.Remove("StudentRegistrationComplete");
                RegistrationPanel.Visible = false;
                SuccessPanel.Visible = true;
            }
        }

        protected void PasswordRulesValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrWhiteSpace(args.Value)
                && args.Value.Length >= 8 && args.Value.Length <= 64
                && args.Value.IndexOf('\0') < 0;

            // BCrypt accepts at most 72 UTF-8 bytes. Reject longer input instead of truncating it.
            if (args.IsValid && Encoding.UTF8.GetByteCount(args.Value) > 72)
            {
                args.IsValid = false;
                PasswordRulesValidator.ErrorMessage = "Your password is too long. Please shorten it.";
            }
        }

        protected void RegisterButton_Click(object sender, EventArgs e)
        {
            errMsg.Visible = false;
            fname.Text = fname.Text.Trim();
            lname.Text = lname.Text.Trim();
            username.Text = username.Text.Trim();
            email.Text = email.Text.Trim();
            country.Text = country.Text.Trim();

            Page.Validate("StudentRegistration");

            // Browser limits can be bypassed, so check SQL column limits on the server too.
            DetailsValidator.IsValid = fname.Text.Length <= 50 && lname.Text.Length <= 50
                && country.Text.Length <= 50 && email.Text.Length <= 254;
            PasswordsMatchValidator.IsValid = PasswordsMatchValidator.IsValid
                && string.Equals(pwd.Text, confirmPwd.Text, StringComparison.Ordinal);
            if (!Page.IsValid)
            {
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand checkUsername = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.userTable WHERE username = @username", connection))
                    {
                        checkUsername.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username.Text;
                        if ((int)checkUsername.ExecuteScalar() > 0)
                        {
                            ShowError("This username is already taken. Please choose another one.");
                            return;
                        }
                    }

                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(pwd.Text);
                    const string insertQuery = @"INSERT INTO dbo.userTable
                        (fname, lname, gender, country, email, username, [password], usertype, staffId, accountStatus)
                        VALUES (@fname, @lname, @gender, @country, @email, @username, @password,
                            N'Student', NULL, N'Pending')";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.Add("@fname", SqlDbType.NVarChar, 50).Value = fname.Text;
                        command.Parameters.Add("@lname", SqlDbType.NVarChar, 50).Value = lname.Text;
                        command.Parameters.Add("@gender", SqlDbType.NChar, 10).Value = OptionalValue(gender.SelectedValue);
                        command.Parameters.Add("@country", SqlDbType.NVarChar, 50).Value = OptionalValue(country.Text);
                        command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = OptionalValue(email.Text);
                        command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username.Text;
                        command.Parameters.Add("@password", SqlDbType.NVarChar, 300).Value = passwordHash;
                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException("Registration did not insert one student record.");
                        }
                    }
                }

                // The SQL statement fixes the role and status; the visitor cannot choose a staff role.
                Session["StudentRegistrationComplete"] = true;
                Response.Redirect("~/StudentRegistration.aspx?registered=1", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SqlException exception)
            {
                // The unique SQL constraint handles simultaneous requests for the same username too.
                if (exception.Number == 2601 || exception.Number == 2627)
                {
                    ShowError("This username is already taken. Please choose another one.");
                }
                else
                {
                    Trace.Warn("StudentRegistration", "Could not create a student account.", exception);
                    ShowError("We could not create your account right now. Please try again later.");
                }
            }
            catch (Exception exception)
            {
                Trace.Warn("StudentRegistration", "Could not create a student account.", exception);
                ShowError("We could not create your account right now. Please try again later.");
            }
        }

        private static object OptionalValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;
        }

        private void ShowError(string message)
        {
            errMsg.Text = message;
            errMsg.Visible = true;
        }
    }
}
