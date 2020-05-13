using System;
using DeeZ;
using DeeZ.Core;
using DeeZ.Editor.UI;
using DeeZ.Engine.Networking;
using DeeZ.Engine.UI;
using Microsoft.Xna.Framework;
using Button = DeeZ.Engine.UI.Button;
using Random = DeeZ.Engine.Random;
using Vector2 = System.Numerics.Vector2;

namespace DeeZEngine_Demos.GameObjects.UI
{

    public class NetworkTestMenu : UIObject
    {

        public override bool IsRootUIMenu => true;

        public override string RootUIName => "NetworkTestMenu";

        public override int RootUIPriority => 2;

        public override bool DestroyOnLoad => true;

        public NetworkTestMenu() : base(Vector2.Zero)
        {
            if (GameEngine.IsFullHeadless)
                return;

            Button hostServer = EditorTheme.CreateButton(new Vector2(100, 100), new Point(200, 100), 
                "Start server", Transform, EditorTheme.ColorSet.Blue);

            Button connect = EditorTheme.CreateButton(new Vector2(100, 400), new Point(200, 100),
                "Connect", Transform, EditorTheme.ColorSet.Blue);

            TextInputField addressField = EditorTheme.CreateTextField(new Vector2(310, 100), new Point(200, 40), null, Transform);
            addressField.DefocusOnConfirm = false;
            addressField.ClearOnUnfocus = false;
            addressField.SetValue("127.0.0.1");

            TextInputField portField = EditorTheme.CreateTextField(new Vector2(620, 100), new Point(100, 40), null, Transform);
            portField.DefocusOnConfirm = false;
            portField.ConfirmOnEnter = false;
            portField.ClearOnUnfocus = false;
            portField.SetValue(Random.Next(1, 25000).ToString());

            addressField.Confirmed += (sender, args) => Network.Connect(addressField.Input.Value, Convert.ToInt32(portField.Input.Value), 919);
            hostServer.Clicked += (sender, args) => Network.StartServer(919, 920);
            connect.Clicked += (sender, args) => Network.Connect(addressField.Input.Value, Convert.ToInt32(portField.Input.Value), 919);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (Network.InstanceType != NetInstanceType.None) {
                Disable(true);
            }
        }


    }

}
