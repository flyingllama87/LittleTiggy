using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;


namespace LittleTiggy
{

    public class mainCharacter
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
        Animation idle;
        Animation currentAnimation;

        Vector2 desiredDestinationPosition;

        Boolean isPoweredUp;

        // KeyboardState oldKeyboardState;

        const float charSpeed = 0.00001F;
        long ticksSinceLastUpdate = 0;


        public static float X
        {
            get;
            set;
        }

        public static float Y
        {
            get;
            set;
        }

        public static float GridAlignedX
        {
            get
            {
                return X - (X % 16);
            }
        }

        public static float GridAlignedY
        {
            get
            {
                return Y - (Y % 16);
            }
        }

        public mainCharacter(GraphicsDevice graphicsDevice)
        {
            if (characterSheetTexture == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/charactersheet.png"))
                {
                    characterSheetTexture = Texture2D.FromStream(graphicsDevice, stream);
                }
            }

            walkDown = new Animation();
            walkDown.AddFrame(new Rectangle(3, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkDown.AddFrame(new Rectangle(19, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkDown.AddFrame(new Rectangle(3, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkDown.AddFrame(new Rectangle(35, 1, 10, 15), TimeSpan.FromSeconds(.25));

            walkLeft = new Animation();
            walkLeft.AddFrame(new Rectangle(51, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkLeft.AddFrame(new Rectangle(67, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkLeft.AddFrame(new Rectangle(51, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkLeft.AddFrame(new Rectangle(83, 1, 10, 15), TimeSpan.FromSeconds(.25));

            walkRight = new Animation();
            walkRight.AddFrame(new Rectangle(99, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkRight.AddFrame(new Rectangle(115, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkRight.AddFrame(new Rectangle(99, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkRight.AddFrame(new Rectangle(131, 1, 10, 15), TimeSpan.FromSeconds(.25));

            walkUp = new Animation();
            walkUp.AddFrame(new Rectangle(147, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkUp.AddFrame(new Rectangle(163, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkUp.AddFrame(new Rectangle(147, 1, 10, 15), TimeSpan.FromSeconds(.25));
            walkUp.AddFrame(new Rectangle(179, 1, 10, 15), TimeSpan.FromSeconds(.25));

            idle = new Animation();
            idle.AddFrame(new Rectangle(3, 1, 10, 15), TimeSpan.FromSeconds(.25));

            standDown = new Animation();
            standDown.AddFrame(new Rectangle(3, 1, 10, 15), TimeSpan.FromSeconds(.25));

            standLeft = new Animation();
            standLeft.AddFrame(new Rectangle(51, 1, 10, 15), TimeSpan.FromSeconds(.25));

            standRight = new Animation();
            standRight.AddFrame(new Rectangle(99, 1, 10, 15), TimeSpan.FromSeconds(.25));

            standUp = new Animation();
            standUp.AddFrame(new Rectangle(147, 1, 10, 15), TimeSpan.FromSeconds(.25));

            currentAnimation = idle;

            X = 1; Y = 1;

        }

        public void Update(GameTime gameTime, GraphicsDevice graphicsDevice, EnvironmentBlock[] walls)
        {

            ticksSinceLastUpdate = gameTime.ElapsedGameTime.Ticks;

            desiredDestinationPosition = new Vector2(mainCharacter.X, mainCharacter.Y);
     
            Vector2 velocity = GetDesiredVelocityFromInput();  // Touch / mouse controls

            if (velocity != Vector2.Zero) // if we have input from either touch or mouse
            {
                ProcessTouchInput(velocity);
            }
            else // keyboard controls
            { 
                ProcessKeyboardInput(gameTime, walls);
            }

            currentAnimation.Update(gameTime);

        }

        void ProcessKeyboardInput(GameTime gameTime, EnvironmentBlock[] walls)
        {
            // select standing animation based off last walk animation
            if (currentAnimation == walkLeft) currentAnimation = standLeft;
            else if (currentAnimation == walkRight) currentAnimation = standRight;
            else if (currentAnimation == walkUp) currentAnimation = standUp;
            else if (currentAnimation == walkDown) currentAnimation = standDown;

            // change character position and animation based on key press
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                currentAnimation = walkDown;
                currentAnimation.Update(gameTime);

                desiredDestinationPosition.Y += charSpeed * ticksSinceLastUpdate;

            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                currentAnimation = walkUp;
                currentAnimation.Update(gameTime);

                desiredDestinationPosition.Y -= charSpeed * ticksSinceLastUpdate;


            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                currentAnimation = walkLeft;
                currentAnimation.Update(gameTime);

                desiredDestinationPosition.X -= charSpeed * ticksSinceLastUpdate;

            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                currentAnimation = walkRight;
                currentAnimation.Update(gameTime);

                desiredDestinationPosition.X += charSpeed * ticksSinceLastUpdate;

            }


            //check collisions

            if (IsEnvironmentCollision(walls, new Vector2(desiredDestinationPosition.X, Y)) == false)
            {
                X = desiredDestinationPosition.X;
            }

            if (IsEnvironmentCollision(walls, new Vector2(X, desiredDestinationPosition.Y)) == false)
            {
                Y = desiredDestinationPosition.Y;
            }

#if _DEBUG
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && Game1.respawn > 0 && !OldKeyboardState.IsKeyDown(Keys.Space))
            {

                Game1.respawn--;

                do
                {
                    Random randomNumber = new Random();

                    int gridAlignedX = randomNumber.Next(0, GameConstants.windowWidth);
                    int gridAlignedY = randomNumber.Next(0, GameConstants.windowHeight);

                    gridAlignedX = gridAlignedX - (gridAlignedX % 16);
                    gridAlignedY = gridAlignedY - (gridAlignedY % 16);

                    X = gridAlignedX;
                    Y = gridAlignedY;
                } while (IsEnvironmentCollision(walls));

            }

            OldKeyboardState = Keyboard.GetState();
#endif
        }

        void ProcessTouchInput(Vector2 velocity)
        {
            desiredDestinationPosition.X += velocity.X * charSpeed * ticksSinceLastUpdate;
            desiredDestinationPosition.Y += velocity.Y * charSpeed * ticksSinceLastUpdate;

            if (IsEnvironmentCollision(Game1.walls, new Vector2(desiredDestinationPosition.X, Y)) == false)
            {
                X = desiredDestinationPosition.X;
            }

            if (IsEnvironmentCollision(Game1.walls, new Vector2(X, desiredDestinationPosition.Y)) == false)
            {
                Y = desiredDestinationPosition.Y;
            }
            
            // select animation based on direction mouse/touch input is pointing
            bool movingHorizontally = Math.Abs(velocity.X) > Math.Abs(velocity.Y);
            if (movingHorizontally)
            {
                if (velocity.X > 0) currentAnimation = walkRight;
                else currentAnimation = walkLeft;
            }
            else
            {
                if (velocity.Y > 0) currentAnimation = walkDown;
                else currentAnimation = walkUp;
            }
        }



        Vector2 GetDesiredVelocityFromInput()
        {
            Vector2 desiredVelocity = new Vector2();

            TouchCollection touchCollection = TouchPanel.GetState();
            MouseState mouseState = Mouse.GetState();

            if (touchCollection.Count > 0)
            {
                desiredVelocity.X = touchCollection[0].Position.X - X;
                desiredVelocity.Y = touchCollection[0].Position.Y - Y;

                if (desiredVelocity.X != 0 || desiredVelocity.Y != 0)
                {
                    desiredVelocity.Normalize();
                }
            }
            else if (mouseState.LeftButton == ButtonState.Pressed)
            {
                desiredVelocity.X = mouseState.X - X;
                desiredVelocity.Y = mouseState.Y - Y;

                if (desiredVelocity.X != 0 || desiredVelocity.Y != 0)
                {
                    desiredVelocity.Normalize();
                }
            }

            return desiredVelocity;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 topLeftOfSprite = new Vector2(X, Y);
            Color tintColor = Color.White;
            var sourceRectangle = currentAnimation.CurrentRectangle;

            spriteBatch.Draw(characterSheetTexture, topLeftOfSprite, sourceRectangle, tintColor);

        }

        public static bool IsEnvironmentCollision(EnvironmentBlock[] walls, Vector2 position)
        {
            Rectangle characterRect = new Rectangle((int)position.X, (int)position.Y, 10, 15);

            foreach (EnvironmentBlock wall in walls)
           {
                Rectangle wallRect = new Rectangle((int)wall.X, (int)wall.Y, 16, 16);

                if (characterRect.Intersects(wallRect))
                {
                    return true;
                }
            }

            if (position.X > GameConstants.windowWidth - GameConstants.characterWidth || position.X < 0 || position.Y < 0)
                return true;

            return false;
        }

 #if _DEBUG
        void SetCollisionTimer()
        {
            Game1.collisionTimerOn = true;
            Game1.TimerDateTime = DateTime.Now;
            Game1.TimerDateTime = Game1.TimerDateTime.AddSeconds(0.1);

        }

        void EvaluateCollisionTimer()
        {
            if (Game1.TimerDateTime.CompareTo(DateTime.Now) < 0)
            {
                Game1.collisionTimerOn = false;
                Game1.collidingBottom = false;
                Game1.collidingLeft = false;
                Game1.collidingRight = false;
                Game1.collidingTop = false;
            }
        }
#endif

    }
}