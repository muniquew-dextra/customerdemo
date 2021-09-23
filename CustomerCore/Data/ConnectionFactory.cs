using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerCore.Data
{
    public class ConnectionFactory : IConnectionFactory
    {
        public IDbConnection Connection(string conectionString)
        {
            return new SqlConnection(conectionString);
        }
    }
}
