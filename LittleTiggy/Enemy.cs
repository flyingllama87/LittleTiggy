﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;



namespace LittleTiggy
{

    public class Enemy
    {
        static Texture2D characterSheetTexture;
        Animation walkDown;
        Animation walkLeft;
        Animation walkRight;
        Animation walkUp;
        Animation standDown;
        Animation standLeft;
        Animation standRight;
        Animation standUp;
        Animation Idle;
        Animation currentAnimation;

        const float charSpeed = 0.00001F;
        long ticksSinceLastUpdate = 0;


        public float X
        {
            get;
            set;
        }

        public float Y
        {
            get;
            set;
        }

        public Enemy(GraphicsDevice graphicsDevice, EnvironmentBlock[] walls)
        {
            if (characterSheetTexture == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/charactersheet.png"))
                {
                    characterSheetTexture = Texture2D.FromStream(graphicsDevice, stream);
                }
            }

            walkDown = new Animation();
            walkDown.AddFrame(new Rectangle(0, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkDown.AddFrame(new Rectangle(16, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkDown.AddFrame(new Rectangle(0, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkDown.AddFrame(new Rectangle(32, 16, 16, 16), TimeSpan.FromSeconds(.25));

            walkLeft = new Animation();
            walkLeft.AddFrame(new Rectangle(48, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkLeft.AddFrame(new Rectangle(64, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkLeft.AddFrame(new Rectangle(48, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkLeft.AddFrame(new Rectangle(80, 16, 16, 16), TimeSpan.FromSeconds(.25));

            walkRight = new Animation();
            walkRight.AddFrame(new Rectangle(96, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkRight.AddFrame(new Rectangle(112, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkRight.AddFrame(new Rectangle(96, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkRight.AddFrame(new Rectangle(128, 16, 16, 16), TimeSpan.FromSeconds(.25));

            walkUp = new Animation();
            walkUp.AddFrame(new Rectangle(144, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkUp.AddFrame(new Rectangle(160, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkUp.AddFrame(new Rectangle(144, 16, 16, 16), TimeSpan.FromSeconds(.25));
            walkUp.AddFrame(new Rectangle(176, 16, 16, 16), TimeSpan.FromSeconds(.25));

            Idle = new Animation();
            Idle.AddFrame(new Rectangle(0, 16, 16, 16), TimeSpan.FromSeconds(.25));

            standDown = new Animation();
            standDown.AddFrame(new Rectangle(0, 16, 16, 16), TimeSpan.FromSeconds(.25));

            standLeft = new Animation();
            standLeft.AddFrame(new Rectangle(48, 16, 16, 16), TimeSpan.FromSeconds(.25));

            standRight = new Animation();
            standRight.AddFrame(new Rectangle(96, 16, 16, 16), TimeSpan.FromSeconds(.25));

            standUp = new Animation();
            standUp.AddFrame(new Rectangle(144, 16, 16, 16), TimeSpan.FromSeconds(.25));

            currentAnimation = Idle;

            Random randomNumber = new Random();
            do
            {
                this.X = (float)randomNumber.Next(0, 512);
                this.Y = randomNumber.Next(0, 512);

                this.X -= this.X % 16;
                this.Y -= this.Y % 16;

            } while (IsEnvironmentCollision(walls));

        }

        public void Update(GameTime gameTime, GraphicsDevice graphicsDevice, EnvironmentBlock[] walls)
        {
            ticksSinceLastUpdate = gameTime.ElapsedGameTime.Ticks;

            //TODO: check collisions with environment walls


            //TODO: Check player collision

            
            // Get direction of player character and set enemy to move in that direction at half player speed.
            var velocity = GetPlayerVelocity();

            this.X += (velocity.X * (charSpeed * ticksSinceLastUpdate)) / 2;
            this.Y += (velocity.Y * (charSpeed * ticksSinceLastUpdate)) / 2;


            // select appropriate animation based on movement direction

            bool movingHorizontally = Math.Abs(velocity.X) > Math.Abs(velocity.Y);

            if (movingHorizontally)
            {
                if (velocity.X > 0) currentAnimation = walkRight;
                else  currentAnimation = walkLeft;
            }
            else
            {
                if (velocity.Y > 0) currentAnimation = walkDown;
                else currentAnimation = walkUp;
            }

            currentAnimation.Update(gameTime);
   
            // Logic to stop enemy object from going outside game play area.  Probably not needed.

            if (this.X < graphicsDevice.Viewport.Width - 16)
                this.X += charSpeed * ticksSinceLastUpdate;

            if (this.X > 0 )
                this.X -= charSpeed * ticksSinceLastUpdate;

            if (this.Y < graphicsDevice.Viewport.Height - 16)
                this.Y += charSpeed * ticksSinceLastUpdate;

            if (this.Y > 0)
                this.Y -= charSpeed * ticksSinceLastUpdate;

        }



        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 topLeftOfSprite = new Vector2(this.X, this.Y);
            Color tintColor = Color.White;
            var sourceRectangle = currentAnimation.CurrentRectangle;

            spriteBatch.Draw(characterSheetTexture, topLeftOfSprite, sourceRectangle, tintColor);

        }

        bool IsEnvironmentCollision(EnvironmentBlock[] walls)
        {
            for (int i = 0; i < walls.Length; i++)
            {
                Rectangle wall = new Rectangle((int)walls[i].X, (int)walls[i].Y, 16, 16);
                Rectangle character = new Rectangle((int)this.X, (int)this.Y, 16, 16);

                if (character.Intersects(wall))
                {
                    return true;
                }

            }

            return false;

        }

        Vector2 GetPlayerVelocity()
        {
            Vector2 desiredVelocity = new Vector2(mainCharacter.X - this.X, mainCharacter.Y - this.Y);

            desiredVelocity.Normalize();

            return desiredVelocity;
        }


    }
}