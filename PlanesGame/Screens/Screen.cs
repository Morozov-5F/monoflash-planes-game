using System;
using System.Collections.Generic;
using System.Text;
using MonoFlash.Display;

namespace PlanesGame.Screens
{
    public abstract class Screen : Sprite
    {
        public abstract void Load();
        public abstract void Unload();
        public abstract void Update(float deltaTime);
    }
}
