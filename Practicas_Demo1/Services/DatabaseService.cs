using Microsoft.Data.SqlClient;

namespace Practicas_Demo1.Services
{
    public class DatabaseService
    {
        private readonly IConfiguration _config;

        public DatabaseService(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection GetMainConnection()
        {
            string connectionString = _config.GetConnectionString("MainDB");
            return new SqlConnection(connectionString);
        }

        public SqlConnection GetSecondConnection()
        {
            string connectionString = _config.GetConnectionString("SecondDB");
            return new SqlConnection(connectionString);
        }
    }

}
