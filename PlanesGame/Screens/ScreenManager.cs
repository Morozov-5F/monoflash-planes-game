using System;
using System.Collections.Generic;
using System.Diagnostics;

using MonoFlash.Display;
using MonoFlash.Events;
using MonoFlash;

namespace PlanesGame.Screens
{
    public class ScreenManager : Sprite
    {
        private Screen currentScreen;

        public ScreenManager()
        {
            AddEventListener(Event.ENTER_FRAME, Update);
        }

        private void Update(Event e)
        {
            if (currentScreen != null)
            {
                currentScreen.Update(Application.deltaTime);
            }
        }

        public bool LoadScreen(Screen newScreen)
        {
            if (currentScreen != null)
            {
                if (!RemoveChild(currentScreen))
                {
                    return false;
                }
                currentScreen.Unload();
            }
            if (newScreen == null)
            {
                return false;
            }
            currentScreen = newScreen;
            AddChild(currentScreen);
            currentScreen.Load();
            return true;
        }
    }
}
