using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMS
{
    public interface IStorage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        StorageTypes Type { get; }
        string SaveFile(string fromFileLocation,string fileName);
        string Name { get; set; }
    }
}