using FluentFTP;

namespace BMS
{
    internal class FtpStorage : IStorage
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public short Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Directory { get; set; }

        public FtpStorage(string host, short port = 21, string username = "anonymous", string password = "", string directory = "/")
        {
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            Directory = directory;
            Directory = Directory.Trim('/');
        }

        public StorageTypes Type => StorageTypes.FTP;

        public string SaveFile(string fromFileLocation, string fileName)
        {
            var path = $"/{Directory}/{fileName}";
            using (var client = new FtpClient(Host, Port, Username, Password))
            {
                client.UploadFile(fromFileLocation, path, FtpExists.NoCheck, createRemoteDir: true);
                client.Disconnect();
            }
            return path;
        }

    }
}