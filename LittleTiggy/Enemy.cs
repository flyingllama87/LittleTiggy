using System;
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

        Vector2 vectorFinalDestinationPosition;
        bool isRandomWalking = false;

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

            //TODO: Check player collision


            if (isRandomWalking)
            {

                if (Math.Floor(vectorFinalDestinationPosition.X) - Math.Floor(this.X) == 0 && Math.Floor(vectorFinalDestinationPosition.Y) - Math.Floor(this.Y) == 0)
                    isRandomWalking = false;

                if (Math.Floor(vectorFinalDestinationPosition.X) - Math.Floor(this.X) > 0)
                {
                    this.X += 1;
                }
                else if (Math.Floor(vectorFinalDestinationPosition.X) - Math.Floor(this.X) < 0)
                {
                    this.X -= 1;
                }
                else if (Math.Floor(vectorFinalDestinationPosition.Y) - Math.Floor(this.Y) > 0)
                {
                    this.Y += 1;
                }
                else if (Math.Floor(vectorFinalDestinationPosition.Y) - Math.Floor(this.Y) < 0)
                {
                    this.Y -= 1;
                }


            }
            else
            {

                // Get direction of player character and set enemy to move in that direction at half player speed.
                var velocity = GetPlayerVelocity();

                float XPosToMoveTo = this.X + (velocity.X * (charSpeed * ticksSinceLastUpdate) / 2);
                float YPosToMoveTo = this.Y + (velocity.Y * (charSpeed * ticksSinceLastUpdate) / 2);

                Vector2 vectorImmediatePosToMoveTo = new Vector2(XPosToMoveTo, YPosToMoveTo);

                if (!IsEnvironmentCollision(walls, vectorImmediatePosToMoveTo))
                {
                    this.X = XPosToMoveTo;
                    this.Y = YPosToMoveTo;
                }
                else
                {
                    Random direction = new Random();

                    do
                    {

                        vectorFinalDestinationPosition.X = this.X;
                        vectorFinalDestinationPosition.Y = this.Y;

                        switch (direction.Next(1, 4))
                        {
                            case 1:
                                vectorFinalDestinationPosition.X = (vectorFinalDestinationPosition.X - (vectorFinalDestinationPosition.X % 16)) + 16;
                                break;
                            case 2:
                                vectorFinalDestinationPosition.X = (vectorFinalDestinationPosition.X - (vectorFinalDestinationPosition.X % 16)) - 16;
                                break;
                            case 3:
                                vectorFinalDestinationPosition.Y = (vectorFinalDestinationPosition.Y - (vectorFinalDestinationPosition.Y % 16)) + 16;
                                break;
                            case 4:
                                vectorFinalDestinationPosition.Y = (vectorFinalDestinationPosition.Y - (vectorFinalDestinationPosition.Y % 16)) - 16;
                                break;
                        }

                        

                    } while (IsEnvironmentCollision(walls, new Vector2(vectorFinalDestinationPosition.X, vectorFinalDestinationPosition.Y)));

                    isRandomWalking = true;

                    //this.X = XPosToMoveTo;
                    //this.Y = YPosToMoveTo;

                }



                // select appropriate animation based on movement direction

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
                Rectangle wall = new Rectangle((int)walls[i].X + 1, (int)walls[i].Y + 1, 14, 14);
                Rectangle character = new Rectangle((int)this.X + 3, (int)this.Y + 2, 10, 13);

                if (character.Intersects(wall))
                {
                    return true;
                }

            }

            return false;

        }

        bool IsEnvironmentCollision(EnvironmentBlock[] walls, Vector2 characterVector)
        {
            for (int i = 0; i < walls.Length; i++)
            {
                Rectangle wall = new Rectangle((int)walls[i].X + 1, (int)walls[i].Y + 1, 14, 14);
                Rectangle character = new Rectangle((int)characterVector.X, (int)characterVector.Y, 16, 16);

                if (character.Intersects(wall))
                {
                    return true;
                }

            }

            return false;

        }

        Vector2 GetPlayerVelocity()
        {
            //get velocity of player character relative to enemy's position & normalize so we end up with a direction to move in.

            Vector2 desiredVelocity = new Vector2(mainCharacter.X - this.X, mainCharacter.Y - this.Y);

            desiredVelocity.Normalize();

            return desiredVelocity;
        }


    }
}