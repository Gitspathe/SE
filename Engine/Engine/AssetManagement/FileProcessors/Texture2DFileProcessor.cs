using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Core.Exceptions;

namespace SE.AssetManagement.FileProcessors
{
    public class Texture2DFileProcessor : FileProcessor
    {
        public override Type Type => typeof(Texture2D);
        public override string ContentSubDirectory => "Images";
        public override string[] AllowedExtensions => new[] { ".png", ".bmp", ".jpg", ".gif", ".tif" };
       
        protected override bool LoadFile(GraphicsDevice gfxDevice, string file, out object obj)
        {
            if (gfxDevice == null)
                throw new HeadlessNotSupportedException($"Texture '{file}' was not loaded in headless display mode.");

            using Stream titleStream = TitleContainer.OpenStream(file);
            Texture2D tex = Texture2D.FromStream(gfxDevice, titleStream);
            obj = tex;
            return true;
        }
    }
}
