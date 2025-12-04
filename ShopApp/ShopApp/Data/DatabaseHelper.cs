using System;
using System.Configuration;
using System.Data.SqlClient;

namespace ShoeStoreLLC.Data
{
    public static class DatabaseHelper
    {
        public static string ConnectionString
        {
            get
            {
                // Для работы ConfigurationManager нужно добавить ссылку на System.Configuration
                return "Data Source=ADCLG1;Initial Catalog=ShoeStoreLLC;Integrated Security=True;";
            }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}