using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SOBSchoolOfBytesWebSite.Security
{
    public static class PresidentBootstrap
    {
        public static bool IsAvailable()
        {
            using (SqlConnection connection = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM dbo.userTable WHERE usertype = N'President'", connection))
            {
                connection.Open();
                return (int)command.ExecuteScalar() == 0;
            }
        }

        public static bool TryCreate(string firstName, string lastName, string username, string staffId, string passwordHash)
        {
            using (SqlConnection connection = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                connection.Open();
                // Serialize the check and insert, so two setup requests cannot create two first presidents.
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = @"SELECT COUNT(*) FROM dbo.userTable WITH (UPDLOCK, HOLDLOCK)
                        WHERE usertype = N'President'";
                    if ((int)command.ExecuteScalar() != 0) { transaction.Rollback(); return false; }
                    command.CommandText = @"INSERT INTO dbo.userTable
                        (fname, lname, username, staffId, [password], usertype, accountStatus)
                        VALUES (@firstName, @lastName, @username, @staffId, @password, N'President', N'Active')";
                    command.Parameters.Add("@firstName", SqlDbType.NVarChar, 50).Value = firstName;
                    command.Parameters.Add("@lastName", SqlDbType.NVarChar, 50).Value = lastName;
                    command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username;
                    command.Parameters.Add("@staffId", SqlDbType.NVarChar, 30).Value = staffId;
                    command.Parameters.Add("@password", SqlDbType.NVarChar, 300).Value = passwordHash;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    return true;
                }
            }
        }
    }
}
