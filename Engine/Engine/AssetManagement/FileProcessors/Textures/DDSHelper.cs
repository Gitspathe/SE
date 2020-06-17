using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace SE.AssetManagement.FileProcessors.Textures
{
    internal static class DDSHelper
    {
        private const int DDSD_CAPS = 0x00000001;
        private const int DDSD_HEIGHT = 0x00000002;
        private const int DDSD_WIDTH = 0x00000004;
        private const int DDSD_PITCH = 0x00000008;
        private const int DDSD_PIXELFORMAT = 0x00001000;
        private const int DDSD_MIPMAPCOUNT = 0x00020000;
        private const int DDSD_LINEARSIZE = 0x00080000;
        private const int DDSD_DEPTH = 0x00800000;

        private const int DDPF_ALPHAPIXELS = 0x00000001;
        private const int DDPF_FOURCC = 0x00000004;
        private const int DDPF_RGB = 0x00000040;
        private const int DDPF_LUMINANCE = 0x00020000;

        // caps1
        private const int DDSCAPS_COMPLEX = 0x00000008;
        private const int DDSCAPS_TEXTURE = 0x00001000;
        private const int DDSCAPS_MIPMAP = 0x00400000;
        // caps2
        private const int DDSCAPS2_CUBEMAP = 0x00000200;
        private const int DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400;
        private const int DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800;
        private const int DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000;
        private const int DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000;
        private const int DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000;
        private const int DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000;
        private const int DDSCAPS2_VOLUME = 0x00200000;

        private const uint FOURCC_DXT1 = 0x31545844;
        private const uint FOURCC_DXT5 = 0x35545844;
        private const uint FOURCC_ATI1 = 0x31495441;
        private const uint FOURCC_ATI2 = 0x32495441;
        private const uint FOURCC_RXGB = 0x42475852;
        private const uint FOURCC_DOLLARNULL = 0x24;
        private const uint FOURCC_oNULL = 0x6f;
        private const uint FOURCC_pNULL = 0x70;
        private const uint FOURCC_qNULL = 0x71;
        private const uint FOURCC_rNULL = 0x72;
        private const uint FOURCC_sNULL = 0x73;
        private const uint FOURCC_tNULL = 0x74;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DDSStruct
        {
            public uint size;		// equals size of struct (which is part of the data file!)
            public uint flags;
            public uint height;
            public uint width;
            public uint sizeorpitch;
            public uint depth;
            public uint mipmapcount;
            public uint alphabitdepth;
            public uint[] reserved;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct pixelformatstruct
            {
                public uint size;	// equals size of struct (which is part of the data file!)
                public uint flags;
                public uint fourcc;
                public uint rgbbitcount;
                public uint rbitmask;
                public uint gbitmask;
                public uint bbitmask;
                public uint alphabitmask;
            }
            public pixelformatstruct pixelformat;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct ddscapsstruct
            {
                public uint caps1;
                public uint caps2;
                public uint caps3;
                public uint caps4;
            }
            public ddscapsstruct ddscaps;
            public uint texturestage;

            public static DDSStruct Create(BinaryReader reader)
            {
                DDSStruct header = new DDSStruct();
                if (ReadHeader(reader, ref header)) {
                    return header;
                }
                throw new System.Exception("DDS header is invalid.");
            }

            public static bool ReadHeader(BinaryReader reader, ref DDSStruct header)
            {
                byte[] signature = reader.ReadBytes(4);
                if (!(signature[0] == 'D' && signature[1] == 'D' && signature[2] == 'S' && signature[3] == ' '))
                    return false;

                header.size = reader.ReadUInt32();
                if (header.size != 124)
                    return false;

                //convert the data
                header.flags = reader.ReadUInt32();
                header.height = reader.ReadUInt32();
                header.width = reader.ReadUInt32();
                header.sizeorpitch = reader.ReadUInt32();
                header.depth = reader.ReadUInt32();
                header.mipmapcount = reader.ReadUInt32();
                header.alphabitdepth = reader.ReadUInt32();

                header.reserved = new uint[10];
                for (int i = 0; i < 10; i++) {
                    header.reserved[i] = reader.ReadUInt32();
                }

                //pixelfromat
                header.pixelformat.size = reader.ReadUInt32();
                header.pixelformat.flags = reader.ReadUInt32();
                header.pixelformat.fourcc = reader.ReadUInt32();
                header.pixelformat.rgbbitcount = reader.ReadUInt32();
                header.pixelformat.rbitmask = reader.ReadUInt32();
                header.pixelformat.gbitmask = reader.ReadUInt32();
                header.pixelformat.bbitmask = reader.ReadUInt32();
                header.pixelformat.alphabitmask = reader.ReadUInt32();

                //caps
                header.ddscaps.caps1 = reader.ReadUInt32();
                header.ddscaps.caps2 = reader.ReadUInt32();
                header.ddscaps.caps3 = reader.ReadUInt32();
                header.ddscaps.caps4 = reader.ReadUInt32();
                header.texturestage = reader.ReadUInt32();

                return true;
            }

            public SurfaceFormat GetSurfaceFormat() 
                => DDSHelper.GetSurfaceFormat(ref this);
        }

        private static SurfaceFormat GetSurfaceFormat(ref DDSStruct header)
        {
            uint blockSize = 0;
            PixelFormat format = GetFormatInternal(ref header, ref blockSize);
            switch (format) {
                case PixelFormat.DXT1:
                    return SurfaceFormat.Dxt1;
                case PixelFormat.DXT5:
                    return SurfaceFormat.Dxt5;
                default:
                    throw new System.Exception("Unsupported format.");
            }
        }

        private static PixelFormat GetFormatInternal(ref DDSStruct header, ref uint blocksize)
        {
            PixelFormat format;
            if ((header.pixelformat.flags & DDPF_FOURCC) == DDPF_FOURCC) {
                blocksize = ((header.width + 3) / 4) * ((header.height + 3) / 4) * header.depth;
                switch (header.pixelformat.fourcc) {
                    case FOURCC_DXT1:
                        format = PixelFormat.DXT1;
                        blocksize *= 8;
                        break;
                    case FOURCC_DXT5:
                        format = PixelFormat.DXT5;
                        blocksize *= 16;
                        break;
                    case FOURCC_ATI1:
                        format = PixelFormat.ATI1N;
                        blocksize *= 8;
                        break;
                    case FOURCC_ATI2:
                        format = PixelFormat.THREEDC;
                        blocksize *= 16;
                        break;
                    case FOURCC_RXGB:
                        format = PixelFormat.RXGB;
                        blocksize *= 16;
                        break;
                    case FOURCC_DOLLARNULL:
                        format = PixelFormat.A16B16G16R16;
                        blocksize = header.width * header.height * header.depth * 8;
                        break;
                    case FOURCC_oNULL:
                        format = PixelFormat.R16F;
                        blocksize = header.width * header.height * header.depth * 2;
                        break;
                    case FOURCC_pNULL:
                        format = PixelFormat.G16R16F;
                        blocksize = header.width * header.height * header.depth * 4;
                        break;
                    case FOURCC_qNULL:
                        format = PixelFormat.A16B16G16R16F;
                        blocksize = header.width * header.height * header.depth * 8;
                        break;
                    case FOURCC_rNULL:
                        format = PixelFormat.R32F;
                        blocksize = header.width * header.height * header.depth * 4;
                        break;
                    case FOURCC_sNULL:
                        format = PixelFormat.G32R32F;
                        blocksize = header.width * header.height * header.depth * 8;
                        break;
                    case FOURCC_tNULL:
                        format = PixelFormat.A32B32G32R32F;
                        blocksize = header.width * header.height * header.depth * 16;
                        break;
                    default:
                        format = PixelFormat.UNKNOWN;
                        blocksize *= 16;
                        break;
                } 
            } else {
                // uncompressed image
                if ((header.pixelformat.flags & DDPF_LUMINANCE) == DDPF_LUMINANCE) {
                    format = (header.pixelformat.flags & DDPF_ALPHAPIXELS) == DDPF_ALPHAPIXELS 
                        ? PixelFormat.LUMINANCE_ALPHA 
                        : PixelFormat.LUMINANCE;
                } else {
                    format = (header.pixelformat.flags & DDPF_ALPHAPIXELS) == DDPF_ALPHAPIXELS 
                        ? PixelFormat.RGBA 
                        : PixelFormat.RGB;
                }

                blocksize = (header.width * header.height * header.depth * (header.pixelformat.rgbbitcount >> 3));
            }
            return format;
        }

        /// <summary>
        /// Various pixel formats/compressors used by the DDS image.
        /// </summary>
        private enum PixelFormat
        {
            ARGB,
            RGBA,
            RGB,
            DXT1,
            DXT5,
            THREEDC,
            ATI1N,
            LUMINANCE,
            LUMINANCE_ALPHA,
            RXGB,
            A16B16G16R16,
            R16F,
            G16R16F,
            A16B16G16R16F,
            R32F,
            G32R32F,
            A32B32G32R32F,
            UNKNOWN
        }
    }
}