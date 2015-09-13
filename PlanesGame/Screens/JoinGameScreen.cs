using System;
using MonoFlash.Display;
using MonoFlash;
using MonoFlash.Text;
using Lidgren.Network;
using System.Diagnostics;
using System.Collections.Generic;
using PlanesGame.Network;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoFlash.Events;

namespace PlanesGame.Screens
{
    public class HostLabel : Sprite
    {
        private TextField nameField;
        private TextField uidField;

        public string nick;
        public long uid;

        private bool isSelected;

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                nameField.textColor = uidField.textColor = (isSelected) ? Color.LightGreen : Color.White;
            }
        } 

        public HostLabel(string nick, long uid, SpriteFont font)
        {
            this.nick = nick;
            this.uid = uid;

            nameField = new TextField();
            nameField.font = font;
            nameField.text = "Room name: " + nick;
            AddChild(nameField);

            uidField = new TextField();
            uidField.font = font;
            uidField.text = "Room UID: " + NetUtility.ToHexString(uid);
            uidField.ScaleX = uidField.ScaleY = 0.5f;
            uidField.Y = nameField.Height;
            AddChild(uidField);

            isSelected = false;
        }
    }

    public class JoinGameScreen : Screen
    {
        private Sprite guiContainer;
        private Sprite bgContainer;

        private Sprite [] bgLayers;
        private float[] LAYER_DEPTHS = { 0, 0.5f, 0.75f };

        private float bgPosX;

        private List<HostLabel> hosts;

        private NetClient client;
        private string nick;

        private SpriteFont font;
        private TextField connectButton, refreshButton;

        TextField pointerLabel;

        public JoinGameScreen(float bgPosX = 0)
        {
            this.bgPosX = bgPosX;
        }

        public override void Load()
        {
            // Background
            bgContainer = new Sprite();
            bgLayers = new Sprite[3];
            AddChild(bgContainer);
            for (int i = 0; i < 3; ++i)
            {
                bgLayers[i] = new Sprite();

                var bmp1 = new Bitmap(Assets.GetBitmapData("assets/background/1/" + (i + 1).ToString(), true));
                var bmp2 = new Bitmap(Assets.GetBitmapData("assets/background/1/" + (i + 1).ToString(), true));

                bmp2.X = bmp1.Width;

                bgLayers[i].AddChild(bmp1);
                bgLayers[i].AddChild(bmp2);

                bgContainer.AddChild(bgLayers[i]);
            }
            // GUI
            guiContainer = new Sprite();
            AddChild(guiContainer);

            hosts = new List<HostLabel>();

            font = Assets.GetFont("assets/MainFont");

            var screenNameLabel = new TextField();
            screenNameLabel.font = font;
            screenNameLabel.text = "JOIN GAME";
            screenNameLabel.X = GameMain.ScreenWidth / 2 - screenNameLabel.Width / 2;
            screenNameLabel.Y = 10 / GameMain.mainScale;
            guiContainer.AddChild(screenNameLabel);

            pointerLabel = new TextField();
            pointerLabel.font = screenNameLabel.font;
            pointerLabel.text = "Select a game to join";
            pointerLabel.ScaleX = pointerLabel.ScaleY = 0.4f;
            pointerLabel.X = GameMain.ScreenWidth / 2 - pointerLabel.Width / 2;
            pointerLabel.Y = screenNameLabel.Y + screenNameLabel.Height;
            guiContainer.AddChild(pointerLabel);

            connectButton = new TextField();
            connectButton.font = font;
            connectButton.text = "Connect";
            connectButton.ScaleX = connectButton.ScaleY = 0.5f;
            connectButton.X = GameMain.ScreenWidth * 0.25f - connectButton.Width / 2;
            connectButton.Y = GameMain.ScreenHeight - connectButton.Height * 2;
            connectButton.textColor = Color.DarkGray;
            guiContainer.AddChild(connectButton);

            refreshButton = new TextField();
            refreshButton.font = font;
            refreshButton.text = "Refresh";
            refreshButton.ScaleX = refreshButton.ScaleY = 0.5f;
            refreshButton.X = GameMain.ScreenWidth * 0.75f - refreshButton.Width / 2;
            refreshButton.Y = connectButton.Y;
            guiContainer.AddChild(refreshButton);

            connectButton.AddEventListener(Event.TOUCH_END, onButtonDown);
            refreshButton.AddEventListener(Event.TOUCH_END, onButtonDown);
            
            NetPeerConfiguration config = new NetPeerConfiguration("multiplanes-network");
            client = new NetClient(config);
            client.Start();
            nick = NickGenerator.GenerateNick((client.UniqueIdentifier));
            client.DiscoverLocalPeers(14242);
            client.DiscoverKnownPeer(GameClient.NET_ADRESS, GameClient.NET_PORT);
        }

        public override void Unload()
        {
//            bgLayers = null;
        }

        public void RefreshHostList()
        {
            hosts.Clear();
            NetOutgoingMessage listRequest = client.CreateMessage();
            listRequest.Write((byte)GameClient.DataMessageTypes.HostsListRequest);
            client.SendMessage(listRequest, NetDeliveryMethod.ReliableOrdered);
        }

        private void JoinGameRequest(int index)
        {
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((byte)GameClient.DataMessageTypes.HostConnectionRequest);
            msg.Write(hosts[index].uid);
            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private void JoinGameApproved(string remoteNick)
        {
            GameMain.screenManager.LoadScreen(new GameScreen(client, nick, remoteNick, false));
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
                        approval.Write(GameClient.JOIN_BYTE);
                        approval.Write(nick);
                        client.Connect(msg.SenderEndPoint, approval);
                        break;

                    case NetIncomingMessageType.Data:
                        byte msgCode = msg.ReadByte();
                        if (msgCode == (byte)GameClient.DataMessageTypes.HostsListResponse)
                        {
                            hosts.Clear();
                            int count = msg.ReadInt32();
                            for (int i = 0; i < count; ++i)
                            {
                                long uid = msg.ReadInt64();
                                string name = msg.ReadString();
                                HostLabel field = new HostLabel(name, uid, font);
                                field.ScaleX = field.ScaleY = 0.5f;
                                field.X = GameMain.ScreenWidth / 10;
                                field.Y = pointerLabel.Y + pointerLabel.Height + (i + 1) * field.Height;
                                guiContainer.AddChild(field);
                                field.AddEventListener(Event.TOUCH_END, onTouchUp);
                                hosts.Add(field);

                                Debug.WriteLine(uid + ": " + name);
                            }
                        }
                        if (msgCode == (byte)GameClient.DataMessageTypes.HostConnectionResponse)
                        {
                            byte response = msg.ReadByte();
                            if (response != 0x01)
                            {
                                Debug.WriteLine("Error: wrong uid");
                                break;
                            }
                            string remoteNick = msg.ReadString();
                            JoinGameApproved(remoteNick);
                        }
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                        if (status == NetConnectionStatus.Connected)
                        {
                            Debug.WriteLine("Connected");

                            NetOutgoingMessage listRequest = client.CreateMessage();
                            listRequest.Write((byte)GameClient.DataMessageTypes.HostsListRequest);
                            client.SendMessage(listRequest, NetDeliveryMethod.ReliableOrdered);
                        }

                        if (status == NetConnectionStatus.Disconnected)
                            Debug.WriteLine("Disconnect");
                        break;
                }
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

        void onTouchUp(Event e)
        {
            var field = e.target as HostLabel;
            if (!field.IsSelected)
            {
                foreach (var currentHost in hosts)
                {
                    if (currentHost.IsSelected)
                        currentHost.IsSelected = false;
                }
            }
            field.IsSelected = !field.IsSelected;
            connectButton.textColor = (field.IsSelected) ? Color.White : Color.DarkGray;
            Debug.WriteLine(field.IsSelected);
        }

        void onButtonDown(Event e)
        {
            var field = e.target as TextField;
            if (field == connectButton)
            {
                if (field.textColor != Color.DarkGray)
                {
                    int index = hosts.FindIndex(item => item.IsSelected);
                    Debug.WriteLine(index);
                    JoinGameRequest(index);
                }
            }
            if (field == refreshButton)
            {
                RefreshHostList();
            }
        }
    }
}

