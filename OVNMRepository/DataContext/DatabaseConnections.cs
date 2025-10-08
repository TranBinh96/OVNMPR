using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVNMRepository.DataContext
{
    public static class DatabaseConnections
    {
        public static readonly IDatabaseHelper dbMSSQL =
            DatabaseFactory.Create("MSSQL", "Data Source=10.216.28.12;Initial Catalog=OKIVNPE;User ID=okipe;Password=oki2024$;");

        public static readonly IDatabaseHelper dbMySQL =
            DatabaseFactory.Create("MySQL", "Server=10.216.28.11;Database=db_pas_prod;User ID=ovnm;Password=P@ssw0rd;");

        public static readonly IDatabaseHelper dbPg =
            DatabaseFactory.Create("Postgres", "Server=10.216.28.11;Port=5432;Database=optas;UserId=ovnm;SearchPath=ovnm;Password=ovnm;Pooling=True");
    }
}
