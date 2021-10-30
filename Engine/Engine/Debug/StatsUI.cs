using LiteNetLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Core;
using SE.Editor.UI;
using SE.UI;
using System.Text;
using Console = SE.Core.Console;
using Vector2 = System.Numerics.Vector2;

namespace SE.Debug
{

    public class StatsUI : UIObject
    {
        public override bool IsRootUIMenu => true;

        public override string RootUIName => "StatsUI";

        public override int RootUIPriority => 98;

        public override bool DestroyOnLoad => false;

        private SpriteBatch spriteBatch;
        private FPSCounter fpsCounter;
        private float timer = 0.2f;

        private static float sec = 1.0f;
        private static ulong bytesDownThisSec, bytesDownLastSec, bytesDownStart;
        private static ulong bytesUpThisSec, bytesUpLastSec, bytesUpStart;

        private static StringBuilder curFpsBuilder = new StringBuilder();
        private static StringBuilder avgFpsBuilder = new StringBuilder();
        private static StringBuilder networkBuilder = new StringBuilder();

        public StatsUI(SpriteBatch spriteBatch, FPSCounter fpsCounter) : base(Vector2.Zero)
        {
            Panel p = EditorTheme.CreatePanel(Vector2.Zero, new Color(Color.Black, 0.8f), new Point(300, 500), Transform);
            this.fpsCounter = fpsCounter;
            this.spriteBatch = spriteBatch;
        }

        protected override void OnEnable(bool enableAllChildren = true)
        {
            base.OnEnable(enableAllChildren);
        }

        protected override void OnDisable(bool isRoot = false)
        {
            base.OnDisable(isRoot);
        }

        public void Draw()
        {
            base.OnUpdate();
            if (timer <= 0) {
                timer = 0.1f;
            } else {
                timer -= Time.DeltaTime;
            }
            curFpsBuilder.Clear().Append("FPS: ").Append(fpsCounter.CurrentFPS);
            avgFpsBuilder.Clear().Append("AVG: ").Append(fpsCounter.AverageFPS);

            spriteBatch.DrawString(Console.DebugFont, curFpsBuilder, new Microsoft.Xna.Framework.Vector2(10, 10), Color.Red, 0f, Microsoft.Xna.Framework.Vector2.Zero, 1.0f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Console.DebugFont, avgFpsBuilder, new Microsoft.Xna.Framework.Vector2(160, 10), Color.Red, 0f, Microsoft.Xna.Framework.Vector2.Zero, 1.0f, SpriteEffects.None, 1f);

            NetStatistics stats = Network.Statistics;
            networkBuilder.Clear().Append("NETWORK STATISTICS\n");
            if (stats == null) {
                networkBuilder.Append("No connection.");
            } else {
                sec -= Time.DeltaTime;
                if (sec <= 0.0f) {
                    bytesDownStart = stats.BytesReceived;
                    bytesDownLastSec = bytesDownThisSec;
                    bytesUpStart = stats.BytesSent;
                    bytesUpLastSec = bytesUpThisSec;
                    sec += 1.0f;
                }
                bytesDownThisSec = stats.BytesReceived - bytesDownStart;
                bytesUpThisSec = stats.BytesSent - bytesUpStart;
                networkBuilder.Append("PACKETS:\n")
                   .Append("  Sent: ").Append(stats.PacketsSent).Append("\n")
                   .Append("  Recieved: ").Append(stats.PacketsReceived).Append("\n")
                   .Append("BYTES:\n")
                   .Append("  Sent: ").Append(stats.BytesSent).Append(" (").Append(bytesUpLastSec / 1000.0f).Append("KB/s) \n")
                   .Append("  Recieved: ").Append(stats.BytesReceived).Append(" (").Append(bytesDownLastSec / 1000.0f).Append("KB/s) \n")
                   .Append("RTT: ").Append(Network.AverageRTT).Append("\n")
                   .Append("IP: ").Append(Network.CurrentIPAddress);
            }
            spriteBatch.DrawString(Console.DebugFont, networkBuilder, new Microsoft.Xna.Framework.Vector2(10, 100), Color.Red, 0f, Microsoft.Xna.Framework.Vector2.Zero, 1.0f, SpriteEffects.None, 1f);
        }

    }

}
