using System;
using DeeZ.Core;
using DeeZ.Engine.Networking;
using Microsoft.Xna.Framework;
using SE.Common;
using SE.Core;
using SE.Editor.UI;
using SE.UI;
using Button = SE.UI.Button;
using Random = SE.Random;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.GameObjects.UI
{
    public class NetworkTestMenu : UIObject
    {
        public override bool IsRootUIMenu => true;

        public override string RootUIName => "NetworkTestMenu";

        public override int RootUIPriority => 2;

        public override bool DestroyOnLoad => true;

        private GameObject notConnectedMenu;
        private Button spawnButton;

        public NetworkTestMenu() : base(Vector2.Zero)
        {
            if (Screen.IsFullHeadless)
                return;

            notConnectedMenu = new GameObject(Vector2.Zero, 0f, Vector2.One);
            Transform parentTransform = notConnectedMenu.Transform;
            parentTransform.SetParent(Transform);

            Button hostServer = EditorTheme.CreateButton(new Vector2(100, 100), new Point(200, 100), 
                "Start server", parentTransform, EditorTheme.ColorSet.Blue);

            spawnButton = EditorTheme.CreateButton(new Vector2(100, 100), new Point(200, 100),
                "Spawn stuff", Transform, EditorTheme.ColorSet.Light);

            Button connect = EditorTheme.CreateButton(new Vector2(100, 400), new Point(200, 100),
                "Connect", parentTransform, EditorTheme.ColorSet.Blue);

            TextInputField addressField = EditorTheme.CreateTextField(new Vector2(310, 100), new Point(200, 40), null, parentTransform);
            addressField.DefocusOnConfirm = false;
            addressField.ClearOnUnfocus = false;
            addressField.SetValue("127.0.0.1");

            TextInputField portField = EditorTheme.CreateTextField(new Vector2(620, 100), new Point(100, 40), null, parentTransform);
            portField.DefocusOnConfirm = false;
            portField.ConfirmOnEnter = false;
            portField.ClearOnUnfocus = false;
            portField.SetValue(Random.Next(1, 25000).ToString());

            spawnButton.Disable();

            addressField.Confirmed += (sender, args) => Network.Connect(addressField.Input.Value, Convert.ToInt32(portField.Input.Value), 919);
            hostServer.Clicked += (sender, args) => Network.StartServer(919, 920);
            spawnButton.Clicked += (sender, args) => Game.SpawnStuff();
            connect.Clicked += (sender, args) => Network.Connect(addressField.Input.Value, Convert.ToInt32(portField.Input.Value), 919);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (Screen.IsFullHeadless)
                return;

            if (Network.InstanceType != NetInstanceType.None) {
                notConnectedMenu.Disable();
                spawnButton.Enable();
            }
        }
    }

}
