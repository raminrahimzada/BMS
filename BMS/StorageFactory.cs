using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMS
{
    public class StorageFactory
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public short Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Directory { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StorageTypes Type { get; set; }

        public IStorage Create()
        {
            switch (Type)
            {
                case StorageTypes.LocalDirectory:
                    return new LocalDirectoryStorage(Directory)
                    {
                        Name = Name
                    };
                case StorageTypes.FTP:
                    return new FtpStorage(Host,Port,Username,Password,Directory)
                    {
                        Name = Name
                    };
                default:
                    throw new Exception("Invalid StorageType");
            }
        }

    }
}