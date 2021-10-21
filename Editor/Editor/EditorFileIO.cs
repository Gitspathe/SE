using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Editor
{
    public static class EditorFileIO
    {
        public static string EditorDirectory { get; private set; }

        static EditorFileIO()
        {
            EditorDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
