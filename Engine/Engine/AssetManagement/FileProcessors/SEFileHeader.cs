using System;
using System.IO;
using System.Text;

namespace SE.AssetManagement.FileProcessors
{
    public readonly struct SEFileHeader
    {
        public readonly bool Loaded;

        public readonly SEFileHeaderFlags HeaderFlags;
        public readonly ushort Version;
        public readonly string OriginalExtension;
        public readonly byte[] AdditionalHeaderData;
        public readonly uint FileSize;
        public readonly uint HeaderSize;

        public SEFileHeader(SEFileHeaderFlags flags, ushort version, string extension, byte[] additionalData, uint fileSize)
        {
            Loaded = true;
            HeaderFlags = flags;
            Version = version;
            OriginalExtension = extension;
            AdditionalHeaderData = additionalData;
            FileSize = fileSize;
            HeaderSize = (uint) (
                  sizeof(SEFileHeaderFlags)                // Flags.
                + sizeof(ushort) * 2                       // Version + AdditionalHeaderData length.
                + sizeof(byte)                             // OriginalExtension length.
                + Encoding.UTF8.GetBytes(extension).Length // OriginalExtension.
                + sizeof(byte) * additionalData.Length     // AdditionalHeaderData.
                + sizeof(uint));                           // HeaderSize.
        }

        public SEFileHeader(BinaryReader reader)
        {
            Loaded = true;
            HeaderFlags = (SEFileHeaderFlags) reader.ReadByte();
            Version = reader.ReadUInt16();

            byte[] originalExtension = new byte[reader.ReadByte()];
            originalExtension = reader.ReadBytes(originalExtension.Length);
            OriginalExtension = Encoding.UTF8.GetString(originalExtension);
            
            byte[] additionalData = new byte[reader.ReadUInt16()];
            AdditionalHeaderData = reader.ReadBytes(additionalData.Length);

            FileSize = reader.ReadUInt32();
            HeaderSize = (uint) (
                  sizeof(SEFileHeaderFlags)            // Flags.
                + sizeof(ushort) * 2                   // Version + AdditionalHeaderData length.
                + sizeof(byte)                         // OriginalExtension length.
                + originalExtension.Length             // OriginalExtension.
                + sizeof(byte) * additionalData.Length // AdditionalHeaderData.
                + sizeof(uint));                       // HeaderSize.
        }

        public void WriteToStream(BinaryWriter writer)
        {
            writer.Write((byte)HeaderFlags);
            writer.Write(Version);
            writer.Write((byte) OriginalExtension.Length);
            writer.Write(Encoding.UTF8.GetBytes(OriginalExtension));
            writer.Write((ushort) AdditionalHeaderData.Length);
            writer.Write(AdditionalHeaderData);
            writer.Write(FileSize);
        }

        public static SEFileHeader FromStream(BinaryReader reader) 
            => new SEFileHeader(reader);
    }

    [Flags]
    public enum SEFileHeaderFlags : byte
    {
        None         = 0b_0000_0000,
        IsCompressed = 0b_0000_0001, // 0
        Reserved1    = 0b_0000_0010, // 1
        Reserved2    = 0b_0000_0100, // 2
        Reserved3    = 0b_0000_1000, // 4
        Reserved4    = 0b_0001_0000, // 8
        Reserved5    = 0b_0010_0000, // 16
        Reserved6    = 0b_0100_0000, // 32
        Reserved7    = 0b_1000_0000  // 64
    }
}
