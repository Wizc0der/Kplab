using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Task_1.Services
{
    public static class JsonService
    {
        public static void Save<T>(string filePath, T data)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public static T Load<T>(string filePath)
        {
            if (!File.Exists(filePath)) return default;
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
        }
    }
}