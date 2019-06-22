using System;
using Hangfire;
using Hangfire.MemoryStorage;

namespace BMS
{
    public class Program
    {
        private static void Main(string[] args)
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();
            using (var server = new BackgroundJobServer())
            {
                Logger.Debug("OK,Server Started");
                using (var jobManager = new BackupJobManager())
                {
                    jobManager.Start();
                }
                DoNotLetMeDie();
            }
            Logger.Debug("Hmm ,Server Stopped");
        }

        private static void DoNotLetMeDie()
        {
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
