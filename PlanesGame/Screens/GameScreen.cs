using System;
using MonoFlash.Display;
using MonoFlash;
using System.Diagnostics;
using MonoFlash.Events;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using PlanesGame.Network;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace PlanesGame.Screens
{
    public class GameScreen : Screen
    {
        // Game
        private Player localPlayer, remotePlayer;
        private Sprite worldContainer, gameContainer, guiContainer, bgContainer;
        private Sprite[] bgLayers;
        private float inputX = 0, inputY = 1;
        private float reloadTime;

        private List<Bullet> bullets;

        // UI
        private Bitmap decelerateButton;
        private Bitmap leftButton;
        private Bitmap rightButton;
        private Bitmap fireButton;

        private NetClient client;
        private string localNick, remoteNick;

        public GameScreen(NetClient client, string localNick, string remoteNick)
        {
            this.client = client;
            this.localNick = localNick;
            this.remoteNick = remoteNick;
        }

        public override void Load()
        {
            worldContainer = new Sprite();
            AddChild(worldContainer);

            bullets = new List<Bullet>();
            reloadTime = 0;

            // Фон
            bgLayers = new Sprite[3];
            bgContainer = new Sprite();
            worldContainer.AddChild(bgContainer);

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

            // Игра
            gameContainer = new Sprite();
            worldContainer.AddChild(gameContainer);

            localPlayer = new Player(localNick, true);
            localPlayer.X = localPlayer.Y = 10;
            gameContainer.AddChild(localPlayer);

            remotePlayer = new Player(remoteNick, false);
            remotePlayer.X = remotePlayer.Y = 16;
            gameContainer.AddChild(remotePlayer);

            // Интерфейс
            guiContainer = new Sprite();
            AddChild(guiContainer);

            #if __MOBILE__
            // Кнопка тормоза
            decelerateButton = new Bitmap(Assets.GetBitmapData("assets/ui/down"));
            decelerateButton.Y = GameMain.ScreenHeight - decelerateButton.Height;
            guiContainer.AddChild(decelerateButton);
            decelerateButton.AddEventListener(Event.TOUCH_MOVE, onDecelerateBegin);
            decelerateButton.AddEventListener(Event.TOUCH_END, onDecelerateEnd);
            // Кнопка вправо
            rightButton = new Bitmap(Assets.GetBitmapData("assets/ui/right"));
            rightButton.X = GameMain.ScreenWidth - rightButton.Width;
            rightButton.Y = decelerateButton.Y;
            guiContainer.AddChild(rightButton);
            rightButton.AddEventListener(Event.TOUCH_MOVE, onRotateBegin);
            rightButton.AddEventListener(Event.TOUCH_END, onRotateEnd);
            // Кнопка вправо 
            leftButton = new Bitmap(Assets.GetBitmapData("assets/ui/left"));
            leftButton.X = rightButton.X - leftButton.Width;
            leftButton.Y = rightButton.Y;
            guiContainer.AddChild(leftButton);
            leftButton.AddEventListener(Event.TOUCH_MOVE, onRotateBegin);
            leftButton.AddEventListener(Event.TOUCH_END, onRotateEnd);
            // Кнопка стрельбы 
            fireButton = new Bitmap(Assets.GetBitmapData("assets/ui/fire"));
            fireButton.X = decelerateButton.X + decelerateButton.Width;
            fireButton.Y = decelerateButton.Y;
            guiContainer.AddChild(fireButton);
            fireButton.AddEventListener(Event.TOUCH_END, onFireBegin);
            leftButton.color = rightButton.color = decelerateButton.color = fireButton.color = new Color(Color.DarkGray, 20);
            #endif
        }

        void onDecelerateBegin(Event e)
        {
            inputY = -1;   
        }

        void onDecelerateEnd(Event e)
        {
            inputY = 1;
        }

        void onRotateBegin(Event e)
        {
            if (e.currentTarget == leftButton)
                inputX = -1;
            if (e.currentTarget == rightButton)
                inputX = 1;
        }

        void onRotateEnd(Event e)
        {
            inputX = 0;
        }

        void onFireBegin(Event e)
        {
            CreateLocalShot();
        }

        public void CreateLocalShot()
        {
            if (reloadTime >= Player.RELOAD_TIME)
            {
                var newBullet = CreateBullet(localPlayer);
                gameContainer.AddChildAt(newBullet, gameContainer.GetChildIndex(localPlayer) - 1);
                bullets.Add(newBullet);

                NetOutgoingMessage bulMsg = client.CreateMessage();
                bulMsg.Write((byte)GameClient.DataMessageTypes.LocalShooting);
                client.SendMessage(bulMsg, NetDeliveryMethod.ReliableOrdered);
                reloadTime = 0;
            }
        }

        public Bullet CreateBullet(Player belongsTo)
        {
            var bullet = new Bullet(belongsTo.X, belongsTo.Y, belongsTo.Rotation, belongsTo == localPlayer);
            return bullet;
        }
            
        public override void Unload()
        {
            throw new NotImplementedException();
        }

        public override void Update(float deltaTime)
        {
            reloadTime += deltaTime;
            // TODO: maybe replace updates?
            localPlayer.Update(deltaTime);
            remotePlayer.Update(deltaTime);

            var worldX = -localPlayer.X + GameMain.ScreenWidth / 2;
            worldX = Math.Max(worldX, -bgContainer.Width + GameMain.ScreenWidth);
            worldX = Math.Min(worldX, 0);
            worldContainer.X = worldX; 

            for (int i = 0; i < bullets.Count; ++i)
            {
                if (bullets[i] == null)
                    continue;
                
                var cB = bullets[i];
                cB.Update(deltaTime);

                if (cB.lifeTime <= 0)
                {
                    gameContainer.RemoveChild(cB);
                    cB = null;
                    continue;
                }
                Player playerToHit = cB.isLocal ? remotePlayer : localPlayer;
                if (cB.HitTestObject(playerToHit))
                {
                    gameContainer.RemoveChild(cB);
                    cB = null;
                    continue;
                }
            }
            bullets.RemoveAll(item => (item == null));

            #if !__MOBILE__

            var kbState = Keyboard.GetState();
            if (kbState.IsKeyDown(Keys.Left))
                inputX = -1;
            else if (kbState.IsKeyDown(Keys.Right))
                inputX = 1;
            else if (kbState.IsKeyUp(Keys.Left) || kbState.IsKeyUp(Keys.Right))
                inputX = 0;

            if (kbState.IsKeyDown(Keys.Down))
                inputY = -1;
            else if (kbState.IsKeyUp(Keys.Down))
                inputY = 1;

            if (kbState.IsKeyDown(Keys.Space))
                CreateLocalShot();

            #endif

            localPlayer.Rotation += inputX * Player.ROTATION_SPEED  * deltaTime;
            localPlayer.Power += inputY * Player.POWER_SPEED  * deltaTime;

            if (localPlayer.X + localPlayer.Width / 2 < 0)
            {
                localPlayer.X = bgLayers[0].Width - localPlayer.Width / 2;
            }
            if (localPlayer.X - localPlayer.Width / 2 > bgLayers[0].Width)
            {
                localPlayer.X = -localPlayer.Width / 2;
            }

            // Send coordinates
            NetOutgoingMessage outMsg = client.CreateMessage();
            outMsg.Write((byte)GameClient.DataMessageTypes.LocalCoordinates);
            outMsg.Write(localPlayer.X);
            outMsg.Write(localPlayer.Y);
            outMsg.Write(localPlayer.Rotation);
            client.SendMessage(outMsg, NetDeliveryMethod.Unreliable);

            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        byte msgCode = msg.ReadByte();
                        if (msgCode == (byte)GameClient.DataMessageTypes.RemoteCoordinates)
                        {
                            remotePlayer.X = msg.ReadFloat();
                            remotePlayer.Y = msg.ReadFloat();
                            remotePlayer.Rotation = msg.ReadFloat();
                        }
                        if (msgCode == (byte)GameClient.DataMessageTypes.RemoteShooting)
                        {
                            var newBullet = CreateBullet(remotePlayer);
                            gameContainer.AddChildAt(newBullet, gameContainer.GetChildIndex(localPlayer) - 1);
                            bullets.Add(newBullet);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

