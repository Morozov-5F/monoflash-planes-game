using System;
using MonoFlash.Display;
using MonoFlash;
using Microsoft.Xna.Framework;

namespace PlanesGame
{
    public class Bullet : Sprite
    {
        public const float MAX_VELOCITY = 60;
        public const float MAX_LIFETIME = 2f;

        public float lifeTime;
        public bool isLocal;

        public Bullet(float x, float y, float rotation, bool isLocal)
        {
            X = x;
            Y = y;
            Rotation = rotation;

            var bmp = new Bitmap(Assets.GetBitmapData("assets/bullet", true));
            bmp.X = -bmp.Width / 2;
            bmp.Y = -bmp.Height / 2;
            AddChild(bmp);

            this.isLocal = isLocal;
            this.lifeTime = MAX_LIFETIME;
        }

        public void Update(float deltaTime)
        {
            X += MAX_VELOCITY * (float)Math.Cos(MathHelper.ToRadians(Rotation)) * deltaTime;
            Y += MAX_VELOCITY * (float)Math.Sin(MathHelper.ToRadians(Rotation)) * deltaTime;

            lifeTime -= deltaTime;
        }
    }
}

