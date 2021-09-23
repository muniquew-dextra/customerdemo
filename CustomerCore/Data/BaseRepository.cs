using CustomerCore.Exception;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerCore.Data
{
    public class BaseRepository : IBaseRepository
    {
        private readonly IConnectionFactory _connection;
        private readonly ILogger<BaseRepository> _logger;

        public BaseRepository(IConnectionFactory connection, ILogger<BaseRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<IEnumerable<T>> QueryAll<T>(string connectionString, string query, object parameters = null)
        {
            try
            {
                using (var conn = _connection.Connection(connectionString))
                {
                    return await conn.QueryAsync<T>(query, parameters);
                }
            }
            catch (DataRepositoryException ex)
            {
                _logger.LogError(ex, "Exception {0} on command {1} params {2}", ex.Message, query, parameters);
                throw new DataRepositoryException($"QueryAll results failed.");
            }
        }

        public async Task<T> QuerySingleOrDefault<T>(string connectionString, string query, object parameters = null)
        {
            try
            {
                using (var conn = _connection.Connection(connectionString))
                {
                    return await conn.QueryFirstOrDefaultAsync<T>(query, parameters);
                }
            }
            catch (DataRepositoryException ex)
            {
                _logger.LogError(ex, "Exception {0} on command {1} params {2}", ex.Message, query, parameters);
                throw new DataRepositoryException($"QuerySingleOrDefault results failed.");
            }
        }

        public async Task<bool> Execute(string connectionString, string query, object parameters = null)
        {
            try
            {
                using (var conn = _connection.Connection(connectionString))
                {
                    var id = await conn.ExecuteAsync(query, parameters);

                    if (id > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (DataRepositoryException ex)
            {
                _logger.LogError(ex, "Exception {0} on command {1} params {2}", ex.Message, query, parameters);
                throw new DataRepositoryException($"Execute results failed.");
            }
        }
    }
}
