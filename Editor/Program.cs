using System;
using System.Collections.Generic;
using System.Text;

namespace SE
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (EditorApp app = new EditorApp()) {
                app.Run();
            }
        }
    }
}
