using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMS
{
    public class BackupProviderFactory 
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public DbTypes DbType { get; set; }
        public string ConnectionString { get; set; }

        public IBackupProvider Create()
        {
            switch (DbType)
            {
                case DbTypes.MsSql:
                    return new MssqlBackupProvider(ConnectionString);
                default:
                    throw new Exception("invalid DbType");
            }
        }
    }
}