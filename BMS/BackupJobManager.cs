using Hangfire;
using Hangfire.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BMS
{
    public class BackupJobManager : IDisposable
    {
        private const string FileLocation = "config.json";
        public List<BackupJob> AllJobs { get; set; }
        public BackupJobManager()
        {
            Load();
        }

        private void Load()
        {
            try
            {
                var json = File.ReadAllText(FileLocation);
                AllJobs = JsonConvert.DeserializeObject<List<BackupJob>>(json);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to load jobs from config" + e.Message);
                AllJobs = new List<BackupJob>();
                Save();
            }
            Save();
            
        }
        private void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(AllJobs, Formatting.Indented);
                File.WriteAllText(FileLocation, json);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to save jobs to config" + e.Message);
            }
        }

        private static string GenerateFileName(BackupJob job)
        {
            const string format = "yyyy-MM-dd HH:mm:ss.fff";
            var now = DateTime.Now.ToString(format);
            now = now.Replace(":", "_");
            return job.DatabaseName + "__" + now + ".bak";
        }

        public void Run(BackupJob job)
        {
            Logger.Debug("Started .Run BackupJobManager");

            var fileName = GenerateFileName(job);
            Logger.Debug("BackupProvider.Create .Run BackupJobManager");

            var backupProvider = job.BackupProvider.Create();
            var storage = new Storage();
            foreach (var storageFactory in job.Storages)
            {
                storage.Add(storageFactory.Create());
            }
            var tempFileLocation = Path.Combine(job.TempDirectory, fileName);
            backupProvider.Start(job.DatabaseName, fileName, tempFileLocation, storage);
            Logger.Debug("Completed .Run BackupJobManager");
        }

        public void Start()
        {
            Logger.Debug("Started .Start BackupJobManager");
            var api = JobStorage.Current.GetMonitoringApi();
            Logger.Debug("JobStorage.Current.Stats=" + JsonConvert.SerializeObject(api.GetStatistics()));
            var recurringJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
            foreach (var recurringJob in recurringJobs)
            {
                Logger.Debug("Removing old jobs.Id..." + recurringJob.Id);
                if (!string.IsNullOrEmpty(recurringJob.Id)) RecurringJob.RemoveIfExists(recurringJob.Id);
                Logger.Debug("Removing old jobs.LastJobId..." + recurringJob.LastJobId);
                if (!string.IsNullOrEmpty(recurringJob.LastJobId)) RecurringJob.RemoveIfExists(recurringJob.LastJobId);
            }

            foreach (var backupJob in AllJobs)
            {
                
                Logger.Debug("Adding new jobs..." + backupJob.Name);

                RecurringJob.AddOrUpdate<BackupJobManager>(backupJob.Name, x => x.Run(backupJob), backupJob.Cron);
            }
            Logger.Debug("completed BackupJobManager");
            Logger.Debug("listing all recurring jobs BackupJobManager");
            recurringJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
            foreach (var recurringJob in recurringJobs)
            {
                Logger.Debug("BackupJobManager.Rec.Job:" +
                             Newtonsoft.Json.JsonConvert.SerializeObject(recurringJob.Id));
            }
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        public void Dispose()
        {
            Save();
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
    }
}