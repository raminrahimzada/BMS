namespace BMS
{
    public interface IBackupProvider
    {
        void Start(string databaseName, string fileName, string tempFileLocation, IStorage storage);
    }
}