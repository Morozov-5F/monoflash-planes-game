using System;
using MonoFlash.Text;
using MonoFlash;
using MonoFlash.Display;
using MonoFlash.Events;

namespace PlanesGame.Screens
{
    public class MenuScreen : Screen
    {
        private Sprite guiContainer;
        private Sprite bgContainer;

        private Sprite [] bgLayers;
        private float[] LAYER_DEPTHS = { 0, 0.5f, 0.75f };

        private TextField logo;
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

                bmp2.X = -bmp1.Width;

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
            logo.text = "MAIN MENU";
            logo.ScaleX = logo.ScaleY = 1.5f;
            logo.X = GameMain.ScreenWidth / 2 - logo.Width / 2;
            logo.Y = 10 / GameMain.mainScale;
            time = 0;
            guiContainer.AddChild(logo);
            // Кнопка join
            var joinButton = new TextField();
            joinButton.font = logo.font;
            joinButton.text = "Join game";
            joinButton.ScaleX = joinButton.ScaleY = 0.5f;
            joinButton.X = GameMain.ScreenWidth / 2 - joinButton.Width / 2;
            joinButton.Y = GameMain.ScreenHeight / 2;
            guiContainer.AddChild(joinButton);
            // Кнопка host
            var hostButton = new TextField();
            hostButton.font = logo.font;
            hostButton.text = "Host game";
            hostButton.ScaleX = hostButton.ScaleY = 0.5f;
            hostButton.X = GameMain.ScreenWidth / 2 - hostButton.Width / 2;
            hostButton.Y = GameMain.ScreenHeight / 2 + joinButton.Height;
            guiContainer.AddChild(hostButton);
            joinButton.AddEventListener(Event.TOUCH_END, joinGameEvent);
            hostButton.AddEventListener(Event.TOUCH_END, hostGameEvent);
        }

        void joinGameEvent(Event e)
        {
            throw new NotImplementedException();
        }

        void hostGameEvent(Event e)
        {
            throw new NotImplementedException();
        }

        public override void Unload()
        {
            // TODO
        }

        public override void Update(float deltaTime)
        {
            time += deltaTime;
            logo.Y += (float)Math.Sin(time * 4) * 0.08f;

            for (int i = 0; i < bgLayers.Length; ++ i)
            {
                var layer = bgLayers[i];
                layer.X += 100 * deltaTime * LAYER_DEPTHS[i];
                if (layer.X >= GameMain.ScreenWidth)
                {
                    layer.X -= layer.Width / 2f;
                }
            }
        }
    }
}

