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
using MonoFlash.Text;

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

        private float[] LAYER_DEPTHS = { 0, 0.5f, 0.75f };

        private List<Bullet> bullets;
        private int levelNumber;

        #if __MOBILE__
        private Bitmap decelerateButton;
        private Bitmap leftButton;
        private Bitmap rightButton;
        private Bitmap fireButton;
        #endif

        private NetClient client;
        private string localNick, remoteNick;
        public const int WORLD_WIDTH = 140;
        private bool isHost;

        private float respTime;
        private float RespawnTime
        {
            get
            {
                return respTime;
            }
            set
            {
                respTime = value;
                respTime = Math.Max(0, respTime);
            }  
        }

        private TextField resultLabel, countLabel;

        public GameScreen(NetClient client, string localNick, string remoteNick, bool isHost)
        {
            this.client = client;
            this.localNick = localNick;
            this.remoteNick = remoteNick;

            this.isHost = isHost;  
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

            LoadBackground(levelNumber);

            // Игра
            gameContainer = new Sprite();
            worldContainer.AddChild(gameContainer);

            localPlayer = new Player(localNick, true);
            remotePlayer = new Player(remoteNick, false);
            if (isHost)
            {
                localPlayer.X = localPlayer.Y = remotePlayer.Y = 10;
                remotePlayer.X = WORLD_WIDTH - GameMain.ScreenWidth / 2;
                remotePlayer.Rotation = 180;
                remotePlayer.ScaleY = -1;
            }
            else
            {
                remotePlayer.X = remotePlayer.Y = localPlayer.Y = 10;
                localPlayer.X = WORLD_WIDTH - GameMain.ScreenWidth / 2;
                localPlayer.Rotation = 180;
                localPlayer.ScaleY = -1;
            }
            gameContainer.AddChild(localPlayer);
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

            if (isHost)
                ChangeLevel();

            resultLabel = new TextField();
            resultLabel.font = Assets.GetFont("assets/MainFont");
            resultLabel.visible = false;

            countLabel = new TextField();
            countLabel.font = resultLabel.font;
            countLabel.visible = false;

            guiContainer.AddChild(resultLabel);
            guiContainer.AddChild(countLabel);
        }

        #if __MOBILE__
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
        #endif

        void LoadBackground(int levelNumber)
        {
            levelNumber += 1;
            if (levelNumber > 4)
                throw new IndexOutOfRangeException();

            // Удаляем старый фон
            for (int i = 0; i < 3; ++i)
            {
                bgContainer.RemoveChild(bgLayers[i]);
                bgLayers[i] = null;
            }

            for (int i = 0; i < 3; ++i)
            {
                bgLayers[i] = new Sprite();

                BitmapData bmpData = Assets.GetBitmapData("assets/background/" + levelNumber + "/"+ (i + 1).ToString(), true);
                var bmp1 = new Bitmap(bmpData);
                var bmp2 = new Bitmap(bmpData);

                bmp2.X = bmp1.Width; 
                bgLayers[i].AddChild(bmp1);
                bgLayers[i].AddChild(bmp2);
                bgContainer.AddChild(bgLayers[i]);
            }
        }

        private void ChangeLevel()
        {
            NetOutgoingMessage outMsg = client.CreateMessage();
            outMsg.Write((byte)GameClient.DataMessageTypes.LocalChangeLevel);
            var newLevelIndex = new Random((int)DateTime.Now.Ticks).Next(0, 4);
            outMsg.Write((byte)newLevelIndex);
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            LoadBackground(newLevelIndex);
        }

        public void CreateLocalShot()
        {
            if ((reloadTime >= Player.RELOAD_TIME) && (!localPlayer.isDead))
            {
                reloadTime = 0;
                var newBullet = CreateBullet(localPlayer);
                gameContainer.AddChildAt(newBullet, gameContainer.GetChildIndex(localPlayer) - 1);
                bullets.Add(newBullet);

                NetOutgoingMessage bulMsg = client.CreateMessage();
                bulMsg.Write((byte)GameClient.DataMessageTypes.LocalShooting);
                client.SendMessage(bulMsg, NetDeliveryMethod.Unreliable);
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

        public void ShowLabel(bool lost = false)
        {
            resultLabel.text = (lost) ? "YOU LOST" : "YOU WON";
            resultLabel.textColor = (lost) ? Color.LightSalmon : Color.LightGreen;
            resultLabel.X = GameMain.ScreenWidth / 2 - resultLabel.Width / 2;
            resultLabel.Y = GameMain.ScreenHeight / 2 - resultLabel.Height * 1.25f;
            resultLabel.visible = true;

            countLabel.text = "3";
            countLabel.X = GameMain.ScreenWidth / 2 - countLabel.Width / 2;
            countLabel.Y = resultLabel.Y + resultLabel.Height;
            countLabel.textColor = resultLabel.textColor;
            countLabel.visible = true;

            RespawnTime = 3;
        }
 
        private void RespawnLocalPlayer()
        {
            if (isHost)
            {
                localPlayer.X = localPlayer.Y = remotePlayer.Y = 10;
            }
            else
            {
                localPlayer.Y = 10;
                localPlayer.X = WORLD_WIDTH - GameMain.ScreenWidth / 2;
                localPlayer.Rotation = 180;
            }
            localPlayer.HP = Player.MAX_HP;
            localPlayer.isDead = false;
            localPlayer.visible = true;
        }

        public override void Update(float deltaTime)
        {
            reloadTime += deltaTime;
            // TODO: maybe replace updates?
            localPlayer.Update(deltaTime);
            remotePlayer.Update(deltaTime);
//            Debug.WriteLine(bullets.Count);
            var worldX = -localPlayer.X + GameMain.ScreenWidth / 2;
            worldX = Math.Max(worldX, -GameScreen.WORLD_WIDTH + GameMain.ScreenWidth);
            worldX = Math.Min(worldX, 0);

            var dx = worldX - worldContainer.X;

            for (int i = 0; i < 3; ++i)
            {
                var layer = bgLayers[i];
                layer.X += dx * LAYER_DEPTHS[i];

                if (layer.X + layer.Width < GameMain.ScreenWidth)
                {
                    layer.X += layer.Width / 2;
                }
                else if (layer.X > 0)
                {
                    layer.X -= layer.Width / 2;
                }
            }
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
                    bullets[i] = null;
                    continue;
                }
                Player playerToHit = cB.isLocal ? remotePlayer : localPlayer;
                if (cB.HitTestObject(playerToHit))
                {
                    gameContainer.RemoveChild(cB);
                    bullets[i] = null;
                    // Если игрок локальный, то отсылаем серверу попадания
                    if (playerToHit == localPlayer)
                    {
                        localPlayer.HP--;
                        if (localPlayer.HP <= 0)
                        {
                            localPlayer.isDead = true;
                            localPlayer.visible = false;

                            ShowLabel(true);
                        }
                        NetOutgoingMessage hitMsg = client.CreateMessage();
                        hitMsg.Write((byte)GameClient.DataMessageTypes.LocalHit);
                        client.SendMessage(hitMsg, NetDeliveryMethod.Unreliable);
                    }

                    continue;
                }
            }
            bullets.RemoveAll(x => x == null);

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
                localPlayer.X = WORLD_WIDTH - localPlayer.Width / 2;
            }
            if (localPlayer.X - localPlayer.Width / 2 > WORLD_WIDTH)
            {
                localPlayer.X = -localPlayer.Width / 2;
            }

            #if DEBUG
            if (client == null)
                return;
            #endif

            // Send coordinates
            if (!localPlayer.isDead)
            {
                NetOutgoingMessage outMsg = client.CreateMessage();
                outMsg.Write((byte)GameClient.DataMessageTypes.LocalCoordinates);
                outMsg.Write(localPlayer.X);
                outMsg.Write(localPlayer.Y);
                outMsg.Write(localPlayer.Rotation);
                client.SendMessage(outMsg, NetDeliveryMethod.Unreliable);
            }

            if (localPlayer.isDead || remotePlayer.isDead)
            {
                RespawnTime -= deltaTime;
                countLabel.text = Math.Round(RespawnTime).ToString();
                if (localPlayer.isDead && RespawnTime <= 0)
                {
                    RespawnLocalPlayer();

                    NetOutgoingMessage respMsg = client.CreateMessage();
                    respMsg.Write((byte)GameClient.DataMessageTypes.LocalRespawn);
                    client.SendMessage(respMsg, NetDeliveryMethod.ReliableOrdered);

                    resultLabel.visible = false;
                    countLabel.visible = false;

                    if (isHost)
                        ChangeLevel();
                }
            }

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
                        if (msgCode == (byte)GameClient.DataMessageTypes.RemoteChangeLevel)
                        {
                            LoadBackground(msg.ReadByte());
                        }
                        if (msgCode == (byte)GameClient.DataMessageTypes.RemoteHit)
                        {
                            remotePlayer.HP--;
                            if (remotePlayer.HP <= 0)
                            {
                                remotePlayer.isDead = true;
                                remotePlayer.visible = false;

                                ShowLabel();
                            }
                        }
                        if (msgCode == (byte)GameClient.DataMessageTypes.RemoteRespawn)
                        {
                            remotePlayer.HP = Player.MAX_HP;
                            remotePlayer.isDead = false;
                            remotePlayer.visible = true;

                            resultLabel.visible = countLabel.visible = false;
                            if (isHost)
                                ChangeLevel();
                        }
                        break;
                    default:
                        break;
                }
                client.Recycle(msg);
            }
        }
    }
}

