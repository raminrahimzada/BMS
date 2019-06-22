
using System.IO;

namespace BMS
{
    internal class LocalDirectoryStorage : IStorage
    {
        public string Name { get; set; }
        public StorageTypes Type => StorageTypes.LocalDirectory;
        public string SaveDirectory { get; set; }

        public LocalDirectoryStorage(string saveDirectory)
        {
            SaveDirectory = saveDirectory;
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
        }
        public string SaveFile(string fromFileLocation, string fileName)
        {
            var path = Path.Combine(SaveDirectory, fileName);
            File.Copy(fromFileLocation, path);
            return fileName;
        }
    }
}