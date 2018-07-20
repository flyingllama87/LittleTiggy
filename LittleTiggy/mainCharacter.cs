using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;


namespace LittleTiggy
{

    public class MainCharacter
    {
        static Texture2D characterSheetTexture;
        static Animation walkDown;
        static Animation walkLeft;
        static Animation walkRight;
        static Animation walkUp;
        static Animation standDown;
        static Animation standLeft;
        static Animation standRight;
        static Animation standUp;
        static Animation idle;
        static Animation currentAnimation;

        Vector2 desiredDestinationPosition; // Used for storing the position the player is trying to move to.
        Vector2 desiredDestinationTilePosition; // Used for storing the destination tile the player is moving to when touch controls are active.

        public bool isMovingToTile = false; // Used as a switch for controlling and determining if the player is moving to a set tile when touch controls are active.

        public static Boolean isPoweredUp = false;
        public static DateTime powerUpTimer;

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

        public MainCharacter(GraphicsDevice graphicsDevice)
        {

            if (characterSheetTexture == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/CharacterSheet.png"))
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

            if (isMovingToTile) // if moving to a tile previously set by touch controls.
            {
                MoveToTile();
            }
            else
            {
                desiredDestinationPosition = new Vector2(MainCharacter.X, MainCharacter.Y); //Reset this var to the player's current position.  To be modified by the input functions and then checked for validity.

                MouseState mouseState = Mouse.GetState();
                TouchCollection touchCollection = TouchPanel.GetState();

                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    ProcessMouseInput(mouseState);
                }
                else if (touchCollection.Count > 0)
                {
                    ProcessTouchInput(touchCollection);
                }
                else // keyboard controls
                {
                    ProcessKeyboardInput(gameTime, walls);
                }
            }

            currentAnimation.Update(gameTime);

            if (isPoweredUp && (powerUpTimer.CompareTo(DateTime.Now) < 0))
            {
                isPoweredUp = false;
                changeSkin();
            }

        }

        void MoveToTile()
        {
            // Check if the destination set to the next grid tile by the path is reached.
            if (Math.Floor(desiredDestinationTilePosition.X) == Math.Floor(X) && Math.Floor(desiredDestinationTilePosition.Y) == Math.Floor(Y))
                isMovingToTile = false;

            // Move enemy closer to destination at normal speed if it's more than 1 unit away.
            if (Math.Floor(desiredDestinationTilePosition.X) - Math.Floor(X) > 1)
            {
                X += (charSpeed * ticksSinceLastUpdate);
                currentAnimation = walkRight;
            }
            else if (Math.Floor(desiredDestinationTilePosition.X) - Math.Floor(X) < -1)
            {
                X -= (charSpeed * ticksSinceLastUpdate);
                currentAnimation = walkLeft;
            }
            else if (Math.Floor(desiredDestinationTilePosition.Y) - Math.Floor(Y) > 1)
            {
                Y += (charSpeed * ticksSinceLastUpdate);
                currentAnimation = walkDown;
            }
            else if (Math.Floor(desiredDestinationTilePosition.Y) - Math.Floor(Y) < -1)
            {
                Y -= (charSpeed * ticksSinceLastUpdate);
                currentAnimation = walkUp;
            }
            // Move enemy closer to destination just a little bit if it's just one unit away. 
            else if (Math.Floor(desiredDestinationTilePosition.X) - Math.Floor(X) == 1)
            {
                X += 1;
            }
            else if (Math.Floor(desiredDestinationTilePosition.X) - Math.Floor(X) == -1)
            {
                X -= 1;
            }
            else if (Math.Floor(desiredDestinationTilePosition.Y) - Math.Floor(Y) == 1)
            {
                Y += 1;
            }
            else if (Math.Floor(desiredDestinationTilePosition.Y) - Math.Floor(Y) == -1)
            {
                Y -= 1;
            }
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

            //check collisions & if the player isn't going to collide with the environment, allow them to move in that direction.  Process the X & Y co-ordinate independantly.

            if (IsEnvironmentCollision(walls, new Vector2(desiredDestinationPosition.X, Y)) == false)
            {
                X = desiredDestinationPosition.X;
            }

            if (IsEnvironmentCollision(walls, new Vector2(X, desiredDestinationPosition.Y)) == false)
            {
                Y = desiredDestinationPosition.Y;
            }



#if _DEBUG // Code used for a 'teleport' function when the space bar is pressed.
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

        void ProcessMouseInput(MouseState mouseState)
        {
            // Generate the position the player is trying to move to.  Then assess if a wall is in the way but assess the X & Y positions independantly.  Allow the player to move in the desired location if a wall is not in the way.

            Vector2 desiredVelocity = new Vector2();

            desiredVelocity.X = mouseState.X - X;
            desiredVelocity.Y = mouseState.Y - Y;

            if (desiredVelocity.X != 0 || desiredVelocity.Y != 0)
            {
                desiredVelocity.Normalize();
            }

            desiredDestinationPosition.X += desiredVelocity.X * charSpeed * ticksSinceLastUpdate;
            desiredDestinationPosition.Y += desiredVelocity.Y * charSpeed * ticksSinceLastUpdate;

            if (IsEnvironmentCollision(LittleTiggy.walls, new Vector2(desiredDestinationPosition.X, (int)Y)) == false)
            {
                X = desiredDestinationPosition.X;
            }

            if (IsEnvironmentCollision(LittleTiggy.walls, new Vector2((int)X, desiredDestinationPosition.Y)) == false)
            {
                Y = desiredDestinationPosition.Y;
            }

            // select animation based on direction mouse/touch input is pointing
            bool movingHorizontally = Math.Abs(desiredVelocity.X) > Math.Abs(desiredVelocity.Y);
            if (movingHorizontally)
            {
                if (desiredVelocity.X > 0) currentAnimation = walkRight;
                else currentAnimation = walkLeft;
            }
            else
            {
                if (desiredVelocity.Y > 0) currentAnimation = walkDown;
                else currentAnimation = walkUp;
            }
        }

        void ProcessTouchInput(TouchCollection touchCollection)  // Acquire a normalised velocity in the direction the player has their touch or mouse input.
        {
            Vector2 desiredVelocity = new Vector2();

            // Acquire the direction the player wants to move based on where they are touching the screen relative to player's position
            // desiredVelocity.X = (touchCollection[0].Position.X / LittleTiggy.gameScaleFactor) - X;
            // desiredVelocity.Y = (touchCollection[0].Position.Y / LittleTiggy.gameScaleFactor) - Y;

            // Acquire the direction the player wants to move based on where they are touching the screen relative to the centre of the game screen

            /*
            desiredVelocity.X = touchCollection[0].Position.X - (LittleTiggy.viewportWidth / 2);
            desiredVelocity.Y = touchCollection[0].Position.Y - (LittleTiggy.viewportHeight / 2);
           
            // Normalize the input vector and use the direction to move the character to the nearest grid aligned position.
            desiredVelocity.Normalize();
            */


            // Virtual Joystick Test
            desiredVelocity = VirtualThumbstick.Thumbstick; // LittleTiggy.virtualThumbstick.;
            if (desiredVelocity != new Vector2(0, 0))
            {
                desiredVelocity.Normalize();
            }
            


            if (Math.Abs(desiredVelocity.X) > Math.Abs(desiredVelocity.Y) && desiredVelocity.X > 0)
            {
                desiredDestinationTilePosition = new Vector2(GridAlignedX + 16, GridAlignedY);

                if (IsEnvironmentCollision(LittleTiggy.walls, desiredDestinationTilePosition) == false)
                {
                    isMovingToTile = true;
                }
            }
            else if (Math.Abs(desiredVelocity.X) > Math.Abs(desiredVelocity.Y) && desiredVelocity.X < 0)
            {
                desiredDestinationTilePosition = new Vector2(GridAlignedX - 16, GridAlignedY);
                if (IsEnvironmentCollision(LittleTiggy.walls, desiredDestinationTilePosition) == false)
                {
                    isMovingToTile = true;
                }
            }
            else if (Math.Abs(desiredVelocity.X) < Math.Abs(desiredVelocity.Y) && desiredVelocity.Y > 0)
            {
                desiredDestinationTilePosition = new Vector2(GridAlignedX, GridAlignedY + 16);
                if (IsEnvironmentCollision(LittleTiggy.walls, desiredDestinationTilePosition) == false)
                {
                    isMovingToTile = true;
                }
            }
            else if (Math.Abs(desiredVelocity.X) < Math.Abs(desiredVelocity.Y) && desiredVelocity.Y < 0)
            {
                desiredDestinationTilePosition = new Vector2(GridAlignedX, GridAlignedY - 16);
                if (IsEnvironmentCollision(LittleTiggy.walls, desiredDestinationTilePosition) == false)
                {
                    isMovingToTile = true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
#if ANDROID // Compensate for drawing on android devices (which uses grid based collision system) by offsetting the main character by drawing character 3 pixels to the right.  Looks more natural.
            Vector2 topLeftOfSprite = new Vector2(X+3, Y);
#endif
#if !ANDROID
            Vector2 topLeftOfSprite = new Vector2(X, Y);
#endif
            Color tintColor = Color.White;
            var sourceRectangle = currentAnimation.CurrentRectangle;

            spriteBatch.Draw(characterSheetTexture, topLeftOfSprite, sourceRectangle, tintColor);
        }

        public static bool IsEnvironmentCollision(EnvironmentBlock[] walls, Vector2 position) // Determine if a particular XY position + character dimensions will collide with the environment.
        {
            Rectangle characterRect = new Rectangle((int)position.X, (int)position.Y, GameConstants.characterWidth, GameConstants.characterHeight);

            foreach (EnvironmentBlock wall in walls)
            {
                Rectangle wallRect = new Rectangle((int)wall.X, (int)wall.Y, GameConstants.tileSize, GameConstants.tileSize);

                if (characterRect.Intersects(wallRect))
                {
                    return true;
                }
            }

            if (position.X > GameConstants.gameWidth - GameConstants.characterWidth || position.X < 0 || position.Y < 0) // See if the player is trying to go outside the game play area.
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
        public static void changeSkin()
        {
            if (isPoweredUp)
            {

                walkDown = new Animation();
                walkDown.AddFrame(new Rectangle(3, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkDown.AddFrame(new Rectangle(19, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkDown.AddFrame(new Rectangle(3, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkDown.AddFrame(new Rectangle(35, 33, 10, 15), TimeSpan.FromSeconds(.25));

                walkLeft = new Animation();
                walkLeft.AddFrame(new Rectangle(51, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkLeft.AddFrame(new Rectangle(67, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkLeft.AddFrame(new Rectangle(51, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkLeft.AddFrame(new Rectangle(83, 33, 10, 15), TimeSpan.FromSeconds(.25));

                walkRight = new Animation();
                walkRight.AddFrame(new Rectangle(99, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkRight.AddFrame(new Rectangle(115, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkRight.AddFrame(new Rectangle(99, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkRight.AddFrame(new Rectangle(131, 33, 10, 15), TimeSpan.FromSeconds(.25));

                walkUp = new Animation();
                walkUp.AddFrame(new Rectangle(147, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkUp.AddFrame(new Rectangle(163, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkUp.AddFrame(new Rectangle(147, 33, 10, 15), TimeSpan.FromSeconds(.25));
                walkUp.AddFrame(new Rectangle(179, 33, 10, 15), TimeSpan.FromSeconds(.25));

                idle = new Animation();
                idle.AddFrame(new Rectangle(3, 33, 10, 15), TimeSpan.FromSeconds(.25));

                standDown = new Animation();
                standDown.AddFrame(new Rectangle(3, 33, 10, 15), TimeSpan.FromSeconds(.25));

                standLeft = new Animation();
                standLeft.AddFrame(new Rectangle(51, 33, 10, 15), TimeSpan.FromSeconds(.25));

                standRight = new Animation();
                standRight.AddFrame(new Rectangle(99, 33, 10, 15), TimeSpan.FromSeconds(.25));

                standUp = new Animation();
                standUp.AddFrame(new Rectangle(147, 33, 10, 15), TimeSpan.FromSeconds(.25));
            }
            else
            {

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
            }
        }
    }
}

