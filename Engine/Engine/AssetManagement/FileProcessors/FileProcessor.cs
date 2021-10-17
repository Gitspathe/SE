using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace SE.AssetManagement.FileProcessors
{
    public abstract class FileProcessor
    {
        public abstract Type Type { get; }
        public abstract string[] FileExtensions { get; }

        internal bool LoadFileInternal(GraphicsDevice gfxDevice, BinaryReader reader, ref SEFileHeader header, out object obj)
            => LoadFile(gfxDevice, reader, ref header, out obj);

        protected abstract bool LoadFile(GraphicsDevice gfxDevice, BinaryReader reader, ref SEFileHeader header, out object obj);
    }
}
