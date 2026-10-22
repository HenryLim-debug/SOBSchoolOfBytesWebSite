using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SOBSchoolOfBytesWebSite.Security
{
    public sealed class RecoveryProfile
    {
        public string Email { get; set; }
        public bool HasCode { get; set; }
    }

    public static class RecoveryService
    {
        private const string ActiveAccount = @" AND ((usertype=N'Student' AND accountStatus=N'Approved')
            OR (usertype IN(N'Teacher',N'President') AND accountStatus=N'Active'))";
        internal const string GenerateSql = @"UPDATE dbo.userTable SET RecoveryCodeHash=@code,
            RecoveryCodeCreatedAt=SYSUTCDATETIME(), RecoveryEmail=@email
            WHERE Id=@id AND SecurityStamp=@stamp AND [password]=@oldPassword";
        internal const string ResetSql = @"UPDATE dbo.userTable SET [password]=@newPassword, SecurityStamp=NEWID(),
            RecoveryCodeHash=NULL, RecoveryCodeCreatedAt=NULL
            WHERE Id=@id AND SecurityStamp=@stamp AND RecoveryCodeHash=@code";
        internal const string FindStudentSql = @"SELECT Id,SecurityStamp,RecoveryCodeHash,[password] FROM dbo.userTable
            WHERE usertype=N'Student' AND username=@username AND RecoveryEmail=@email AND RecoveryCodeHash IS NOT NULL";
        internal const string FindStaffSql = @"SELECT Id,SecurityStamp,RecoveryCodeHash,[password] FROM dbo.userTable
            WHERE usertype IN(N'Teacher',N'President') AND staffId=@staff AND fname=@first AND lname=@last AND RecoveryCodeHash IS NOT NULL";
        internal const string RateSql = @"DECLARE @now DATETIME2(0)=SYSUTCDATETIME(), @n INT, @start DATETIME2(0);
            SELECT @n=AttemptCount,@start=WindowStartUtc FROM dbo.RecoveryRequestLimits WITH(UPDLOCK,HOLDLOCK) WHERE BucketHash=@bucket;
            IF @n IS NULL
            BEGIN
                INSERT dbo.RecoveryRequestLimits(BucketHash,WindowStartUtc,AttemptCount) VALUES(@bucket,@now,1);
                SELECT 1;
            END
            ELSE IF @start<=DATEADD(MINUTE,-15,@now)
            BEGIN
                UPDATE dbo.RecoveryRequestLimits SET WindowStartUtc=@now,AttemptCount=1 WHERE BucketHash=@bucket;
                SELECT 1;
            END
            ELSE IF @n<@limit
            BEGIN
                UPDATE dbo.RecoveryRequestLimits SET AttemptCount=AttemptCount+1 WHERE BucketHash=@bucket;
                SELECT 1;
            END
            ELSE SELECT 0;";

        private static SqlConnection Open()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            connection.Open();
            using (var command = new SqlCommand(@"SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON;
                SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;", connection))
                command.ExecuteNonQuery();
            return connection;
        }

        public static bool TakeLimit(string key, int maximum)
        {
            using (var connection = Open())
            using (var transaction = connection.BeginTransaction())
            {
                // Serialize small counter updates so concurrent requests cannot bypass the limit.
                using (var command = new SqlCommand(@"DECLARE @result INT;
                    EXEC @result=sys.sp_getapplock @Resource=N'SOB.RecoveryLimits',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=5000;
                    IF @result<0 THROW 50001,'Recovery limit check unavailable.',1;
                    DELETE dbo.RecoveryRequestLimits WHERE WindowStartUtc<DATEADD(DAY,-1,SYSUTCDATETIME());", connection, transaction))
                    command.ExecuteNonQuery();
                using (var command = new SqlCommand(RateSql, connection, transaction))
                {
                    command.Parameters.Add("@bucket", SqlDbType.Char, 64).Value = Hash(key);
                    command.Parameters.Add("@limit", SqlDbType.Int).Value = maximum;
                    bool allowed = Convert.ToInt32(command.ExecuteScalar()) == 1;
                    transaction.Commit();
                    return allowed;
                }
            }
        }

        public static RecoveryProfile ReadProfile(UserAccount account)
        {
            using (var connection = Open())
            using (var command = new SqlCommand(@"SELECT COALESCE(RecoveryEmail,email,N'') AS Email,RecoveryCodeHash
                FROM dbo.userTable WHERE Id=@id AND SecurityStamp=@stamp" + ActiveAccount, connection))
            {
                AddAccount(command, account);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) throw new InvalidOperationException("Please sign in again.");
                    return new RecoveryProfile { Email = reader["Email"].ToString(), HasCode = reader["RecoveryCodeHash"] != DBNull.Value };
                }
            }
        }

        public static string Generate(UserAccount account, string currentPassword, string email, string ip)
        {
            if (!TakeLimit("setup-ip:" + ip, 20) || !TakeLimit("setup-user:" + account.Id, 5))
                throw new InvalidOperationException("Too many attempts. Please wait 15 minutes before trying again.");
            if (!ValidPasswordInput(currentPassword)) throw new InvalidOperationException("The current password is incorrect.");
            email = (email ?? "").Trim();
            if (account.Role == "Student" && !ValidEmail(email))
                throw new InvalidOperationException("Enter a valid recovery email address. You may use a parent's email.");
            string oldHash;
            using (var connection = Open())
            {
                using (var command = new SqlCommand("SELECT [password] FROM dbo.userTable WHERE Id=@id AND SecurityStamp=@stamp" + ActiveAccount, connection))
                {
                    AddAccount(command, account);
                    oldHash = command.ExecuteScalar() as string;
                }
                if (oldHash == null || !AccountAccess.PasswordMatches(currentPassword, oldHash))
                    throw new InvalidOperationException("The current password is incorrect or your session has expired.");
                byte[] bytes = new byte[16];
                using (var random = RandomNumberGenerator.Create()) random.GetBytes(bytes);
                string rawCode = Hex(bytes);
                using (var command = new SqlCommand(GenerateSql + ActiveAccount, connection))
                {
                    AddAccount(command, account);
                    command.Parameters.Add("@oldPassword", SqlDbType.NVarChar, 300).Value = oldHash;
                    command.Parameters.Add("@code", SqlDbType.Char, 64).Value = Hash(rawCode);
                    command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = account.Role == "Student" ? (object)email : DBNull.Value;
                    if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("Your account changed. Please sign in again.");
                }
                return rawCode.Substring(0,8) + "-" + rawCode.Substring(8,8) + "-" + rawCode.Substring(16,8) + "-" + rawCode.Substring(24,8);
            }
        }

        public static bool Reset(bool student, string username, string email, string staffId, string firstName,
            string lastName, string code, string newPassword, string ip)
        {
            if (!ValidNewPassword(newPassword)) throw new InvalidOperationException("Use 8 to 64 characters for the new password (no more than 72 UTF-8 bytes).");
            string identity = (student ? username : staffId) ?? "";
            if (!TakeLimit("reset-ip:" + ip, 20) || !TakeLimit("reset-account:" + (student ? "student:" : "staff:") + identity.Trim().ToUpperInvariant(), 5))
                return false;
            if (student && (string.IsNullOrWhiteSpace(username) || username.Trim().Length>50 || !ValidEmail((email??"").Trim()))) return false;
            if (!student && (string.IsNullOrWhiteSpace(staffId) || staffId.Trim().Length>30 || string.IsNullOrWhiteSpace(firstName)
                || firstName.Trim().Length>50 || string.IsNullOrWhiteSpace(lastName) || lastName.Trim().Length>50)) return false;
            code = NormalizeCode(code);
            if (code == null) return false;
            string codeHash = Hash(code);
            int id; Guid stamp; string savedCodeHash, savedPasswordHash;
            using (var connection = Open())
            {
                using (var command = new SqlCommand(student ? FindStudentSql : FindStaffSql, connection))
                {
                    if (student)
                    {
                        command.Parameters.Add("@username",SqlDbType.NVarChar,50).Value=username.Trim();
                        command.Parameters.Add("@email",SqlDbType.NVarChar,254).Value=email.Trim();
                    }
                    else
                    {
                        command.Parameters.Add("@staff",SqlDbType.NVarChar,30).Value=staffId.Trim();
                        command.Parameters.Add("@first",SqlDbType.NVarChar,50).Value=firstName.Trim();
                        command.Parameters.Add("@last",SqlDbType.NVarChar,50).Value=lastName.Trim();
                    }
                    using (var reader=command.ExecuteReader())
                    {
                        if (!reader.Read()) return false;
                        id=(int)reader["Id"]; stamp=(Guid)reader["SecurityStamp"]; savedCodeHash=reader["RecoveryCodeHash"].ToString();
                        savedPasswordHash=reader["password"].ToString();
                    }
                }
                if (!EqualHash(savedCodeHash,codeHash)) return false;
                if (AccountAccess.PasswordMatches(newPassword,savedPasswordHash))
                    throw new InvalidOperationException("Choose a new password that is different from your current password.");
                string passwordHash=BCrypt.Net.BCrypt.HashPassword(newPassword,12);
                // One atomic conditional UPDATE consumes the code and changes the stamp. A second request cannot reuse it.
                // Account role, approval status, classes, submissions and salary records are deliberately untouched.
                using (var command=new SqlCommand(ResetSql,connection))
                {
                    command.Parameters.Add("@id",SqlDbType.Int).Value=id;
                    command.Parameters.Add("@stamp",SqlDbType.UniqueIdentifier).Value=stamp;
                    command.Parameters.Add("@code",SqlDbType.Char,64).Value=codeHash;
                    command.Parameters.Add("@newPassword",SqlDbType.NVarChar,300).Value=passwordHash;
                    return command.ExecuteNonQuery()==1;
                }
            }
        }

        private static void AddAccount(SqlCommand command, UserAccount account)
        {
            command.Parameters.Add("@id",SqlDbType.Int).Value=account.Id;
            command.Parameters.Add("@stamp",SqlDbType.UniqueIdentifier).Value=account.SecurityStamp;
        }
        public static bool ValidPasswordInput(string password)
        { return !string.IsNullOrEmpty(password) && password.Length<=64 && Encoding.UTF8.GetByteCount(password)<=72 && password.IndexOf('\0')<0; }
        public static bool ValidNewPassword(string password)
        { return ValidPasswordInput(password) && password.Length>=8 && !string.IsNullOrWhiteSpace(password); }
        private static bool ValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length>254 || email.IndexOfAny(new[]{'\r','\n'})>=0) return false;
            try { return new MailAddress(email).Address==email && email.IndexOf('@')>0; }
            catch (FormatException) { return false; }
        }
        private static string NormalizeCode(string code)
        {
            if (code==null || code.Length>80) return null;
            string value=code.Replace("-","").Replace(" ","").Trim().ToUpperInvariant();
            return Regex.IsMatch(value,@"\A[0-9A-F]{32}\z") ? value : null;
        }
        private static bool EqualHash(string left,string right)
        {
            if (left==null || right==null || left.Length!=right.Length) return false;
            int difference=0;
            for(int i=0;i<left.Length;i++) difference|=left[i]^right[i];
            return difference==0;
        }
        private static string Hash(string value)
        { using(var sha=SHA256.Create()) return Hex(sha.ComputeHash(Encoding.UTF8.GetBytes(value))); }
        private static string Hex(byte[] value) { return BitConverter.ToString(value).Replace("-",""); }
    }
}
