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
        // "Integrated Security=True" uses your Windows Credentials (e.g. LAP-2158\Adrian) automatically.
        // "Data Source=(localdb)\MSSQLLocalDB" connects to your local SQL Server instance.
        public static string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True";
    }
}
