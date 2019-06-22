using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace BMS
{
    public class MssqlBackupProvider : IBackupProvider
    {
        public string ConnectionString { get; }

        public MssqlBackupProvider(string connectionString)
        {
            ConnectionString = connectionString;
        }

        private string ExtractBackup(string databaseName, string backupFileName)
        {
            Logger.Debug("started backup...");
            Logger.Debug("getting default backup location...");
            var backupFileLocation = GetDefaultBackupLocation();
            if (string.IsNullOrEmpty(backupFileLocation))
            {
                throw new Exception("Backup faylinin default unvani tapilmadi");
            }
            Logger.Debug("got default backup location " + backupFileLocation);

            var path = Path.Combine(backupFileLocation, backupFileName);
            Logger.Debug("real backup will be on " + path);
            var query = $@"
BACKUP DATABASE [{databaseName}] TO 
DISK = N'{path}'
WITH NOFORMAT, NOINIT, 
NAME = N'{databaseName}-Full Database Backup',
SKIP, NOREWIND, NOUNLOAD,  STATS = 10
";
            Logger.Debug("backup started");
            Execute(query);
            Logger.Debug("backup completed");
            return path;
        }

        private void DeleteBackupFile(string backupFile)
        {
            var query = $@"
DECLARE @cmd NVARCHAR(MAX) = 
'xp_cmdshell ''del ""{backupFile}""''';
EXEC (@cmd)";
            Execute(query);
        }

        private void CopyBackup(string fromLocation, string tempFileLocation)
        {
            Logger.Debug("Started Copying backup to external location from " + fromLocation + " to " + tempFileLocation);
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $@"
DECLARE @data  VARBINARY(MAX)
SELECT @data=BulkColumn
FROM   OPENROWSET(BULK'{fromLocation}',SINGLE_BLOB) x;
select @data;";
            // Writes the BLOB to a file (*.bmp).  
            // Streams the BLOB to the FileStream object.  

            // Size of the BLOB buffer.  
            const int bufferSize = 1024 * 4;
            Logger.Debug("starting with buffer size ... " + bufferSize);

            // The BLOB byte[] buffer to be filled by GetBytes.  
            var outByte = new byte[bufferSize];
            // The bytes returned from GetBytes.  

            command.CommandTimeout = 0;

            // Open the connection and read data into the DataReader.  
            var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

            while (reader.Read())
            {
                Logger.Debug("next read action...");
                // Create a file to hold the output.  
                Stream stream = new FileStream(
                    tempFileLocation, FileMode.OpenOrCreate, FileAccess.Write);

                var writer = new BinaryWriter(stream);
                // The starting position in the BLOB output.  
                long startIndex = 0;

                // Read bytes into outByte[] and retain the number of bytes returned.  
                var returnValue = reader.GetBytes(0, startIndex, outByte, 0, bufferSize);

                long readCount = 0;
                // Continue while there are bytes beyond the size of the buffer.  
                while (returnValue == bufferSize)
                {
                    readCount++;

                    writer.Write(outByte);
                    writer.Flush();

                    // Reposition start index to end of last buffer and fill buffer.  
                    startIndex += bufferSize;
                    returnValue = reader.GetBytes(0, startIndex, outByte, 0, bufferSize);

                    if (readCount % 1000 == 0)
                        Logger.Debug("read next buffer action... " + readCount);
                }

                // Write the remaining buffer.  
                writer.Write(outByte, 0, (int)returnValue);
                writer.Flush();

                // Close the output file.  
                writer.Close();
                stream.Close();
            }

            // Close the reader and the connection.  
            reader.Close();
            connection.Close();
        }

        private void Execute(string query)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = query;
                cmd.ExecuteReader();
            }
        }
        private string GetDefaultBackupLocation()
        {
            Logger.Debug("getting default backup location");
            const string regUrl = @"Software\Microsoft\MSSQLServer\MSSQLServer";
            var query = $@"EXEC  master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'{regUrl}',N'BackupDirectory';";
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = query;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var path = reader[1];
                    reader.Close();
                    reader.Dispose();
                    return path == DBNull.Value ? null : path?.ToString();
                }
                Logger.Error("default backup location data reader came empty");

                return null;
            }
        }

        private void AllowXpCmd()
        {
            //To allow advanced options to be changed.
            Execute("EXEC sp_configure 'show advanced options', 1");
            //To update the currently configured value for advanced options.
            Execute("RECONFIGURE");
            //To enable the feature.
            Execute("EXEC sp_configure 'xp_cmdshell', 1");
            //To update the currently configured value for advanced options.
            Execute("RECONFIGURE");
        }


        public void Start(string databaseName, string fileName, string tempFileLocation, IStorage storage)
        {
            try
            {
                Execute("SELECT GETDATE();");
            }
            catch (Exception e)
            {
                Logger.Error("Sorry Cannot Connect to server");
                Logger.Error(e.Message);
                throw;
            }
            try
            {
                AllowXpCmd();
            }
            catch (Exception e)
            {
                Logger.Error("Failed To Activate Xp Cmd pls use `sa` account or run as admin");
                Logger.Error(e.Message);
                throw;
            }
            Logger.Debug("Completed .Run BackupJobManager");
            var realBackupPath = ExtractBackup(databaseName, fileName);

            Logger.Debug("copying backup file from server to local " + new { realBackupPath, tempFileLocation });
            CopyBackup(realBackupPath, tempFileLocation);

            Logger.Debug("deleting backup path from server " + realBackupPath);
            DeleteBackupFile(realBackupPath);

            Logger.Debug("Saving temp file to storage " + new { tempFileLocation, fileName });
            storage.SaveFile(tempFileLocation, fileName);

            Logger.Debug("deleting temp folder " + tempFileLocation);
            File.Delete(tempFileLocation);
        }
    }
}