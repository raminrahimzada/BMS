using System.Collections.Generic;

namespace BMS
{
    public class Storage : IStorage
    {
        public string Name { get; set; }
        public StorageTypes Type => 0;
        public List<IStorage> InnerStorages { get; set; }

        public Storage( List<IStorage> innerStorages)
        {
            InnerStorages = innerStorages;
        }
        public Storage()
        {
            InnerStorages = new List<IStorage>();
        }

        public Storage Ftp(string host, short port = 21, string username = "anonymous", string password = "", string directory = "/")
        {
            Add(new FtpStorage(host, port , username, password, directory));
            return this;
        }
        public Storage Add(IStorage storage)
        {
            InnerStorages.Add(storage);
            return this;
        }
        public string SaveFile(string fromFileLocation, string fileName)
        {
            var result = new List<string>(InnerStorages.Count);
            foreach (var storage in InnerStorages)
            {
                var singleResult=storage.SaveFile(fromFileLocation, fileName);
                result.Add(singleResult);
            }

            return string.Join("\r\n", result);
        }

        public Storage FileSystem(string directory)
        {
            Add(new LocalDirectoryStorage(directory));
            return this;
        }
    }
}