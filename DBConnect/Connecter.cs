using System;
using Npgsql;

namespace DBConnect
{
    public abstract class Connecter
    {
        protected NpgsqlConnection connection;
        protected string jsonFile = "../../../JSON/courses.json";
        private string GetConnectionString()
        {
            string databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            Uri databaseUri = new Uri(databaseUrl);
            string[] userInfo = databaseUri.UserInfo.Split(':');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                Pooling = true,
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };

            return builder.ToString();
        }

        protected void connect()
        {
            string connectionString = GetConnectionString();
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
        }

        protected void CloseConnection()
        {
            connection.Close();
        }
    }
}
