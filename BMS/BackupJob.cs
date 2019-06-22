using System.Collections.Generic;

namespace BMS
{
    public class BackupJob
    {
        public string Cron { get; set; }
        public string Name { get; set; }
        public string DatabaseName { get; set; }
        public string TempDirectory { get; set; }

        public BackupProviderFactory BackupProvider { get; set; }
        public List<StorageFactory> Storages { get; set; }
    }
}