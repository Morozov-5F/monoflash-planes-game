using System;
using MonoFlash.Display;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace PlanesGame
{
    public class InputManager : Sprite
    {
        public static float ValueX, ValueY;
        private static float maxTouchValue = 40;
        private static float startX, startY;

        public InputManager()
        {
            
        }

        public static void Update(float deltaTime)
        {
            #region Keyboard movement
            var kbSate = Keyboard.GetState();
            // Left movement
            if (kbSate.IsKeyDown(Keys.Left))
                ValueX = -1;
            // Right movement
            if (kbSate.IsKeyDown(Keys.Right))
                ValueX = 1;
            // Right movement
            if (kbSate.IsKeyDown(Keys.Up))
                ValueY = 1;
            // Right movement
            if (kbSate.IsKeyDown(Keys.Down))
                ValueY = -1;

            if (kbSate.IsKeyUp(Keys.Left) && ValueX < 0)
                ValueX = 0;

            if (kbSate.IsKeyUp(Keys.Right) && ValueX > 0)
                ValueX = 0;

            if (kbSate.IsKeyUp(Keys.Up) && ValueY > 0)
                ValueY = 0;

            if (kbSate.IsKeyUp(Keys.Down) && ValueY < 0)
                ValueY = 0;
            #endregion

            #region Mouse Movement 
            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                // mouse begin
                if (startX == -100500 && startY == -100500)
                {
                    startX = mouseState.X;
                    startY = mouseState.Y;
                }
                else
                {
                    var x = mouseState.X - startX;
                    var y = mouseState.Y - startY;
                    var sensitivity = 1;
                    x *= sensitivity; y *= sensitivity;

                    var len = MathHelper.Distance(x, y);
                    if (len > maxTouchValue)
                    {
                        x *= maxTouchValue / len;
                        y *= maxTouchValue / len;
                    }

                    ValueX = MathHelper.Clamp(x / maxTouchValue, -1, 1);
                    ValueY = MathHelper.Clamp(y / maxTouchValue, -1, 1);
                }
            }
            else
            {
                ValueX = 0;
                ValueY = 0;

                startX = -100500;
                startY = -100500;
            }
            #endregion

            #region Touch Movement
            var tc = TouchPanel.GetState();
            foreach(TouchLocation tl in tc)
            {
                if (tl.State == TouchLocationState.Moved)
                {
                    if (startX == -100500 && startY == -100500)
                    {
                        startX = tl.Position.X;
                        startY = tl.Position.Y;
                    }
                    else
                    {
                        var x = tl.Position.X - startX;
                        var y = tl.Position.Y - startY;
                        var sensitivity = 1;
                        x *= sensitivity; y *= sensitivity;

                        var len = MathHelper.Distance(x, y);
                        if (len > maxTouchValue)
                        {
                            x *= maxTouchValue / len;
                            y *= maxTouchValue / len;
                        }

                        ValueX = MathHelper.Clamp(x / maxTouchValue, -1, 1);
                        ValueY = MathHelper.Clamp(y / maxTouchValue, -1, 1);
                    }
                }
                if (tl.State == TouchLocationState.Released)
                {
                    ValueX = 0;
                    ValueY = 0;

                    startX = -100500;
                    startY = -100500;
                }
            }
            #endregion
        }
    }
}

