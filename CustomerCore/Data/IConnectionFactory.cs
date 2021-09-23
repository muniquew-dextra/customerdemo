using System.Data;

namespace CustomerCore.Data
{
    public interface IConnectionFactory
    {
        IDbConnection Connection(string connectionString);
    }
}
