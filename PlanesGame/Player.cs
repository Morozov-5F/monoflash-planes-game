using System;
using MonoFlash.Display;
using MonoFlash;
using Microsoft.Xna.Framework;
using MonoFlash.Text;

namespace PlanesGame
{ 
    public class Player : Sprite
    {
        private bool isLocal;
        private float power; 

        private const float ROTATION_LIMIT = 60;
        private const float SPEED = 30;
        private const float GRAVITY = 20; 

        public const float ROTATION_SPEED = 120;
        public const float POWER_SPEED = 0.5f;

        public const float RELOAD_TIME = 0.5f;
        public const int MAX_HP = 3;

        public string nick;
        public Bitmap bmp;

        public bool isDead;

        private int hp;

        TextField name;

        public int HP
        {
            get
            {
                return hp;
            }
            set
            {
                hp = value;
                if (hp < 0)
                {
                    hp = 0;
                }
            }
        }

        public float Power
        {
            get
            {
                return power;
            }
            set
            {
                value = Math.Max(value, 0);
                value = Math.Min(value, 1);

                power = value;
            }
        }

        public Player(string nick = "NoName", bool isLocal = false)
        {
            string planeColor = (isLocal) ? "red" : "blue";

            HP = MAX_HP;

            bmp = new Bitmap(Assets.GetBitmapData("assets/plane_" + planeColor, false));   
            bmp.X = -bmp.Width / 2;
            bmp.Y = -bmp.Height / 2;
            AddChild(bmp);

            this.isLocal = isLocal;
            this.nick = nick;

            name = new TextField();
            name.font = Assets.GetFont("assets/MainFont");
            name.text = nick;
            name.ScaleX = name.ScaleY = 0.3f;
            name.X = bmp.X - (name.Width / 2);
            name.Y = bmp.Y - bmp.Height - name.Height;
//            AddChild(name);
        }

        public void Update(float deltaTime)
        {
//            name.Rotation = -Rotation;
//            name.flippedY = false;

            if (isDead)
                return;

            if (isLocal)
            {
                UpdateLocal(deltaTime);
            }
            else
            {
                UpdateRemote(deltaTime);
            }
        }

        private void UpdateLocal(float deltaTime)
        {
            if (Rotation >= 270 - ROTATION_LIMIT && Rotation <= 270 + ROTATION_LIMIT)
            {
                var rotationDiff = 1 - (float)Math.Abs(270 - Rotation) / ROTATION_LIMIT;
                Power = Power - POWER_SPEED * 2.5f * rotationDiff * deltaTime;
            }
            var moveX = (float)Math.Cos(MathHelper.ToRadians(Rotation));
            var moveY = (float)Math.Sin(MathHelper.ToRadians(Rotation));

            var moveSpeed = SPEED * Power;
            var gravitySpeed = moveY + GRAVITY * (1 - Power);

            X += moveX * moveSpeed * deltaTime;
            Y += (moveY * moveSpeed + gravitySpeed) * deltaTime;
        }

        void UpdateRemote(float deltaTime)
        {
                
        }
    }
}

