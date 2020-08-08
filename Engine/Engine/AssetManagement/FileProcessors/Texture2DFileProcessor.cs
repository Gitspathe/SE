using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement.FileProcessors.Textures;
using SE.Core.Exceptions;
using static SE.AssetManagement.FileProcessors.Textures.DDSHelper;

namespace SE.AssetManagement.FileProcessors
{
    public class Texture2DFileProcessor : FileProcessor
    {
        public override Type Type => typeof(Texture2D);
        public override string[] FileExtensions => new [] { ".png", ".dds" };

        // TODO: Pre-multiply alpha?
        protected override bool LoadFile(GraphicsDevice gfxDevice, BinaryReader reader, ref SEFileHeader header, out object obj)
        {
            if (gfxDevice == null)
                throw new HeadlessNotSupportedException($"Texture '{reader}' was not loaded in headless display mode.");
            
            Texture2D tex;
            if (header.OriginalExtension == ".dds") {
                DDSStruct ddsHeader = DDSStruct.Create(reader);
                tex = new Texture2D(gfxDevice, (int) ddsHeader.width, (int) ddsHeader.height, false, ddsHeader.GetSurfaceFormat());
                byte[] textureData = reader.ReadBytes((int) header.FileSize);
                tex.SetData(textureData, 0, textureData.Length);
            } else {
                tex = Texture2D.FromStream(gfxDevice, reader.BaseStream);
            }

            obj = tex;
            return true;
        }
    }
}
