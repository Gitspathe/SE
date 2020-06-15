using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement.FileProcessors.Textures;
using SE.Core.Exceptions;
using static SE.AssetManagement.FileProcessors.Textures.DDSHelper;

namespace SE.AssetManagement.FileProcessors
{

    // TODO: DXT texture support!!
    public class Texture2DFileProcessor : FileProcessor
    {
        public override Type Type => typeof(Texture2D);
        public override string ContentSubDirectory => "Images";
        public override string[] AllowedExtensions => new[] { ".png", ".bmp", ".jpg", ".gif", ".tif", ".dds" };
       
        protected override bool LoadFile(GraphicsDevice gfxDevice, BinaryReader file, SEFileHeader header, out object obj)
        {
            if (gfxDevice == null)
                throw new HeadlessNotSupportedException($"Texture '{file}' was not loaded in headless display mode.");
            
            Texture2D tex;
            if (header.OriginalExtension == ".dds") {
                DDSStruct ddsHeader = new DDSStruct();
                DDSStruct.ReadHeader(file, ref ddsHeader);
                tex = new Texture2D(gfxDevice, (int) ddsHeader.width, (int) ddsHeader.height, false, SurfaceFormat.Dxt5);
                
                byte[] textureData = file.ReadBytes((int) header.FileSize);
                tex.SetData(textureData, 0, textureData.Length);
            } else {
                tex = Texture2D.FromStream(gfxDevice, file.BaseStream);
            }

            obj = tex;
            return true;
        }
    }
}
