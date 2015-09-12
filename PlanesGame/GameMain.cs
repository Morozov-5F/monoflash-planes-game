#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using MonoFlash.Display;
using PlanesGame.Screens;
using MonoFlash.Events;

#endregion

namespace PlanesGame
{
    public class GameMain : Sprite
    {
        public static ScreenManager screenManager;
        // Глобальный масштаб
        public static float mainScale;
        // Размер экрана с учетом масштаба
        public static Vector2 screenSize;
        public static float ScreenWidth     { get { return screenSize.X; } set { screenSize.X = value; } }
        public static float ScreenHeight    { get { return screenSize.Y; } set { screenSize.Y = value; } }

        public GameMain()
        {
            // Инициализация игры после добавления на сцену
            AddEventListener(Event.ADDED_TO_STAGE, Initialize);
        }

        private void Initialize(Event e)
        {
            RemoveEventListener(Event.ADDED_TO_STAGE, Initialize);

            // Глобальный масштаб и размер экрана
            mainScale = stage.StageHeight / 64;
            screenSize = new Vector2(stage.StageWidth, stage.StageHeight) / mainScale;

            screenManager = new ScreenManager();
            screenManager.ScaleX = screenManager.ScaleY = mainScale;
            AddChild(screenManager);

            // Запуск начального экрана 
            screenManager.LoadScreen(new MenuScreen());
        }
    }
}

