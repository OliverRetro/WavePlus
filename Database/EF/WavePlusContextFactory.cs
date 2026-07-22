using System;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace Plus.Database.EF
{
    public static class WavePlusContextFactory
    {
        private static DbContextOptions<WavePlusContext> _options;

        public static void Init(string hostname, uint port, string database, string username, string password, uint minPool, uint maxPool)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = hostname,
                Port = port,
                Database = database,
                UserID = username,
                Password = password,
                Pooling = true,
                MinimumPoolSize = minPool,
                MaximumPoolSize = maxPool,
                SslMode = MySqlSslMode.Disabled,
                ConnectionTimeout = 10,
                DefaultCommandTimeout = 30
            };

            _options = new DbContextOptionsBuilder<WavePlusContext>()
                // MaxBatchSize(1): Oracle's MySql.EntityFrameworkCore provider throws
                // "Could not save changes. Please configure your entity type accordingly."
                // when a single SaveChanges batches a multi-row INSERT into a table whose key
                // is AUTO_INCREMENT (it can't read the generated ids back across the batch).
                // Every append-only buffer here — chatlogs, LogBuffer, RoomVisitBuffer — hits
                // this. Emitting one INSERT per row (each with its own LAST_INSERT_ID) is the
                // path the provider handles correctly. ExecuteUpdate/ExecuteDelete, which the
                // app's hot writes use, are not affected by batch sizing.
                .UseMySQL(builder.ConnectionString, o => o.MaxBatchSize(1))
                .Options;
        }

        public static WavePlusContext Create()
        {
            if (_options == null)
                throw new InvalidOperationException("Database Connection has not been called.");

            return new WavePlusContext(_options);
        }
    }
}