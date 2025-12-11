using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace medicheck_group5
{
    public static class DatabaseConfig
    {
        // Centralized Connection String
        // Change this path to update the database connection for the entire application
        public static string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MediCheck_Login;Integrated Security=True;TrustServerCertificate=True";
    }
}
