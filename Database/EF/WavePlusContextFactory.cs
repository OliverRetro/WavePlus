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
                .UseMySQL(builder.ConnectionString)
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
