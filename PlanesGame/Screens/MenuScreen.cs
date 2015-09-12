using System;
using MonoFlash.Text;
using MonoFlash;
using MonoFlash.Display;
using MonoFlash.Events;
using System.Diagnostics;
using System.Threading;

namespace PlanesGame.Screens
{
    public class MenuScreen : Screen
    {
        private Sprite guiContainer;
        private Sprite bgContainer;

        private Sprite [] bgLayers;
        private float[] LAYER_DEPTHS = { 0, 0.5f, 0.75f };

        private TextField logo;
        private TextField hostButton;
        private TextField joinButton;
        private Bitmap plane;

        private float time;

        public MenuScreen()
        {
            
        }

        public override void Load()
        {
            // Создаем фон
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
            // Интерфейс Меню
            guiContainer = new Sprite();
            AddChild(guiContainer);
            // Лого 
            logo = new TextField();
            logo.font = Assets.GetFont("assets/MainFont");
            logo.text = "MULTIPLANES";
            logo.X = GameMain.ScreenWidth / 2 - logo.Width / 2;
            logo.Y = 10 / GameMain.mainScale;
            time = 0;
            guiContainer.AddChild(logo);
            // Кнопка join
            joinButton = new TextField();
            joinButton.font = logo.font;
            joinButton.text = "Join game";
            joinButton.ScaleX = joinButton.ScaleY = 0.5f;
            joinButton.X = GameMain.ScreenWidth / 2 - joinButton.Width / 2;
            joinButton.Y = GameMain.ScreenHeight / 2 + joinButton.Height;
            guiContainer.AddChild(joinButton);
            // Самолетик
            plane = new Bitmap(Assets.GetBitmapData("assets/plane"));
            plane.Y = (joinButton.Y + (logo.Y + logo.Height)) / 2 - plane.Height / 2;
            plane.X = GameMain.ScreenWidth / 2 - plane.Width / 2;
            guiContainer.AddChild(plane);
            // Кнопка host
            hostButton = new TextField();
            hostButton.font = logo.font;
            hostButton.text = "Host game";
            hostButton.ScaleX = hostButton.ScaleY = 0.5f;
            hostButton.X = GameMain.ScreenWidth / 2 - hostButton.Width / 2;
            hostButton.Y = joinButton.Y + joinButton.Height * 2;
            guiContainer.AddChild(hostButton);
            joinButton.AddEventListener(Event.TOUCH_END, joinGameEvent);
            hostButton.AddEventListener(Event.TOUCH_END, hostGameEvent);
        }
  
        void joinGameEvent(Event e)
        {
//            
            Debug.WriteLine("Joining Game!");
            GameMain.screenManager.LoadScreen(new JoinGameScreen());
        }

        void hostGameEvent(Event e)
        {
            Debug.WriteLine("Hosting game!");
            GameMain.screenManager.LoadScreen(new HostGameScreen());
        }

        public override void Unload()
        {
            joinButton.RemoveEventListener(Event.TOUCH_END, joinGameEvent);
            hostButton.RemoveEventListener(Event.TOUCH_END, hostGameEvent);

            bgLayers = null;
            logo = null;
            time = 0;
        }

        public override void Update(float deltaTime)
        {
            time += deltaTime;
//            logo.Y += (float)Math.Sin(time ) * 0.008f;
            plane.Y += (float)Math.Sin(time * 4) * 0.08f;
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

