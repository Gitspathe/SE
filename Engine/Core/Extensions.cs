using System;
using System.Numerics;
using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SE.Core
{
    public static class MonoGameExtensions
    {
        public static Vector2 ToMonoGameVector2(this System.Numerics.Vector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static Point ToPoint(this System.Numerics.Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        public static Matrix ToMonoGameMatrix(this Matrix4x4 tM)
        {
            return new Matrix(tM.M11, tM.M12, tM.M13, tM.M14, tM.M21, tM.M22, tM.M23, tM.M24, tM.M31, tM.M32, tM.M33, tM.M34, tM.M41, tM.M42, tM.M43, tM.M44);
        }

        public static Matrix? ToMonoGameMatrix(this Matrix4x4? mat)
        {
            if (!mat.HasValue) 
                return null;

            Matrix4x4 tM = mat.Value;
            return new Matrix(tM.M11, tM.M12, tM.M13, tM.M14, tM.M21, tM.M22, tM.M23, tM.M24, tM.M31, tM.M32, tM.M33, tM.M34, tM.M41, tM.M42, tM.M43, tM.M44);
        }

        public static float ToRotation(this Vector2 vector)
        {
        #if NETSTANDARD2_1
            float angle = MathF.Atan2(vector.X, -vector.Y);
        #else
            float angle = (float)Math.Atan2(vector.X, -vector.Y);
        #endif
            return angle;
        }

        public static System.Numerics.Vector2 GetCenter(this Rectangle rectangle)
        {
            return new System.Numerics.Vector2(rectangle.X + (rectangle.Width / 2), rectangle.Y + (rectangle.Height / 2));
        }

        public static System.Numerics.Vector2 ToNumericsVector2(this Vector2 vector)
        {
            return new System.Numerics.Vector2(vector.X, vector.Y);
        }

        public static System.Numerics.Vector2 ToNumericsVector2(this Point point)
        {
            return new System.Numerics.Vector2(point.X, point.Y);
        }

        public static Matrix4x4 ToNumericsMatrix(this Matrix tM)
        {
            return new Matrix4x4(tM.M11, tM.M12, tM.M13, tM.M14, tM.M21, tM.M22, tM.M23, tM.M24, tM.M31, tM.M32, tM.M33, tM.M34, tM.M41, tM.M42, tM.M43, tM.M44);
        }

        public static Matrix4x4? ToNumericsMatrix(this Matrix? mat)
        {
            if (!mat.HasValue) 
                return null;

            Matrix tM = mat.Value;
            return new Matrix4x4(tM.M11, tM.M12, tM.M13, tM.M14, tM.M21, tM.M22, tM.M23, tM.M24, tM.M31, tM.M32, tM.M33, tM.M34, tM.M41, tM.M42, tM.M43, tM.M44);
        }
    }

    public static class SerializerExtensions
    {
        private static JsonSerializerSettings serializerOptions;

        public static string Serialize<T>(this T obj, bool compress = false, JsonSerializerSettings options = null)
        {
            JsonSerializerSettings o = options ?? serializerOptions;
            if (!compress)
                return JsonConvert.SerializeObject(obj, o);

            string str = JsonConvert.SerializeObject(obj, o);
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            using (MemoryStream msBytes = new MemoryStream(bytes))
            using (MemoryStream ms = new MemoryStream())
            using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress)) {
                msBytes.CopyTo(gs);
                return str;
            }
        }

        public static T Deserialize<T>(this string jsonString, bool compress = false, JsonSerializerSettings options = null)
        {
            JsonSerializerSettings o = options ?? serializerOptions;
            if (!compress)
                return JsonConvert.DeserializeObject<T>(jsonString, o);

            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
            using (MemoryStream msBytes = new MemoryStream(bytes))
            using (MemoryStream ms = new MemoryStream())
            using (GZipStream gs = new GZipStream(msBytes, CompressionMode.Decompress)) {
                gs.CopyTo(ms);
                return JsonConvert.DeserializeObject<T>(jsonString, o);
            }
        }

        static SerializerExtensions()
        {
            serializerOptions = new JsonSerializerSettings {
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
