using System;
using Lidgren.Network;
using PlanesGame.Network;
using System.Diagnostics;
using MonoFlash.Display;
using MonoFlash;
using MonoFlash.Text;

namespace PlanesGame.Screens
{
    public class HostGameScreen:Screen
    {
        private Sprite bgContainer;
        private Sprite[] bgLayers;
        private Sprite guiContainer;
        private float[] LAYER_DEPTHS = { 0, 0.5f, 0.75f };

        private NetClient client;

        private string nick;

        TextField waitLabel;

        float waitLabelUpdateTime;

        public HostGameScreen()
        {
            
        }

        public override void Load()
        {
            bgContainer = new Sprite();
            bgLayers = new Sprite[3];
            AddChild(bgContainer);
            for (int i = 0; i < 3; ++i)
            {
                bgLayers[i] = new Sprite();

                var bmp1 = new Bitmap(Assets.GetBitmapData("assets/background/" + (i + 1).ToString(), true));
                var bmp2 = new Bitmap(Assets.GetBitmapData("assets/background/" + (i + 1).ToString(), true));

                bmp2.X = bmp1.Width;

                bgLayers[i].AddChild(bmp1);
                bgLayers[i].AddChild(bmp2);

                bgContainer.AddChild(bgLayers[i]);
            }
            // GUI
            guiContainer = new Sprite();
            AddChild(guiContainer);

            NetPeerConfiguration config = new NetPeerConfiguration("multiplanes-network");
            client = new NetClient(config); 
            client.Start();
            client.DiscoverLocalPeers(14242);
            client.DiscoverKnownPeer(GameClient.NET_ADRESS, GameClient.NET_PORT);

            nick = NickGenerator.GenerateNick(client.UniqueIdentifier);

            var screenNameLabel = new TextField();
            screenNameLabel.font = Assets.GetFont("assets/MainFont");
            screenNameLabel.text = "HOST GAME";
            screenNameLabel.X = GameMain.ScreenWidth / 2 - screenNameLabel.Width / 2;
            screenNameLabel.Y = 10 / GameMain.mainScale;
            guiContainer.AddChild(screenNameLabel);

            var roomNameLabel = new TextField();
            roomNameLabel.font = screenNameLabel.font;
            roomNameLabel.text = "Room name: " + nick;
            roomNameLabel.X = GameMain.ScreenWidth * 0.1f;
            roomNameLabel.Y = GameMain.ScreenHeight / 2 - roomNameLabel.Height / 2;
            roomNameLabel.ScaleX = roomNameLabel.ScaleY = 0.5f;
            guiContainer.AddChild(roomNameLabel);

            waitLabel = new TextField();
            waitLabel.font = roomNameLabel.font;
            waitLabel.text = "Waiting for other player to connect. . .";
            waitLabel.X = roomNameLabel.X;
            waitLabel.Y = roomNameLabel.Y + roomNameLabel.Height * 2;
            waitLabel.ScaleX = waitLabel.ScaleY = 0.25f;
            guiContainer.AddChild(waitLabel);
        }

        public override void Unload()
        {
            // TODO: 
        }

        public void PlayerJoined(string remoteNick)
        {
            GameMain.screenManager.LoadScreen(new GameScreen(client, nick, remoteNick));
        }

        public override void Update(float deltaTime)
        {
            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse:
                        Debug.WriteLine("Server message: " + msg.ReadString());

                        NetOutgoingMessage approval = client.CreateMessage();
                        approval.Write(GameClient.APPROVAL_BYTE);
                        approval.Write(GameClient.HOST_BYTE);
                        approval.Write(NickGenerator.GenerateNick(client.UniqueIdentifier));
                        client.Connect(msg.SenderEndPoint, approval);
                        break;

                    case NetIncomingMessageType.Data:
                        byte msgType = msg.ReadByte();
                        if (msgType == (byte)GameClient.DataMessageTypes.ClientJoined)
                        {
                            string clientName = msg.ReadString();
                            PlayerJoined(clientName);
                        }
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                        if (status == NetConnectionStatus.Connected)
                            Debug.WriteLine("Connected");

                        if (status == NetConnectionStatus.Disconnected)
                            Debug.WriteLine("Disconnect");
                        break;
                }
            }

            waitLabelUpdateTime += deltaTime;

            if (waitLabelUpdateTime < 1)
            {
                waitLabel.text = "Waiting for other player to connect.";
            }
            else if (waitLabelUpdateTime > 1 && waitLabelUpdateTime < 2)
            {
                waitLabel.text = "Waiting for other player to connect. .";
            }
            else if (waitLabelUpdateTime > 2 && waitLabelUpdateTime < 3)
            {
                waitLabel.text = "Waiting for other player to connect. . .";
            }
            else
            {
                waitLabelUpdateTime = 0;
            }

            for (int i = 0; i < bgLayers.Length; ++ i)
            {
                var layer = bgLayers[i];
                layer.X -= 100 * deltaTime * LAYER_DEPTHS[i];
                if (layer.X <= -layer.Width / 2)
                {
                    layer.X += layer.Width / 2;
                }
            }
        }
    }
}

