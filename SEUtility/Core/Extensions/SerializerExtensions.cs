using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace DeeZ.Core.Extensions
{
    public static class SerializerExtensions
    {
        private static JsonSerializerSettings serializerSettings;

        public static string Serialize(this object obj, bool compress = false)
        {
            if (!compress) 
                return JsonConvert.SerializeObject(obj, serializerSettings);

            string str = JsonConvert.SerializeObject(obj, serializerSettings);
            byte[] bytes = Encoding.Unicode.GetBytes(str);
            using (MemoryStream msBytes = new MemoryStream(bytes))
            using (MemoryStream ms = new MemoryStream())
            using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress)) {
                msBytes.CopyTo(gs);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static T Deserialize<T>(this string jsonString, bool compress = false)
        {
            if(!compress)
                return JsonConvert.DeserializeObject<T>(jsonString, serializerSettings);

            byte[] bytes = Convert.FromBase64String(jsonString);
            using (MemoryStream msBytes = new MemoryStream(bytes))
            using (MemoryStream ms = new MemoryStream())
            using (GZipStream gs = new GZipStream(msBytes, CompressionMode.Decompress)) {
                gs.CopyTo(ms);
                return JsonConvert.DeserializeObject<T>(Encoding.Unicode.GetString(ms.ToArray()));
            }
        }

        static SerializerExtensions()
        {
            serializerSettings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

    }

}
