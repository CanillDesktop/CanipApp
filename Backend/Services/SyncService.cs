using Microsoft.Data.Sqlite;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Backend.Services
{
    public class SyncService
    {
        private readonly IConfiguration _config;
        public SyncService(IConfiguration config)
        {
            _config = config;
        }

    }
}
