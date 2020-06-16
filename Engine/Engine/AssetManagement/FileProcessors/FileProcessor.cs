using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SE.Core;
using SE.Core.Exceptions;
using SE.Editor.Debug.Commands.LevelEdit;
using SE.Utility;

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
