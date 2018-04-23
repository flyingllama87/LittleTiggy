using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;



namespace LittleTiggy
{

    public class Enemy
    {
        static Texture2D characterSheetTexture;
        static Random randomNumber = new Random();

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

        Vector2 vectorDestinationPosition;
        bool isFollowingPath = false;

        List<Vector2> pathToFollow;
        DateTime pathfindingTimer = DateTime.Now;

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

        public float GridAlignedX
        {
            get
            {
                return X - (X % 16);
            }
        }

        public float GridAlignedY
        {
            get
            {
                return Y - (Y % 16);
            }
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
            
            do
            {
                this.X = (float)randomNumber.Next(0, 512);
                this.Y = randomNumber.Next(0, 512);

                this.X -= this.X % 16;
                this.Y -= this.Y % 16;

            } while (IsEnvironmentCollision(walls));

        }

        public void Update(GameTime gameTime, GraphicsDevice graphicsDevice, EnvironmentBlock[] walls, Pathfinder pathfinder)
        {

            ticksSinceLastUpdate = gameTime.ElapsedGameTime.Ticks;


            if ((ManhattanDistance(new Vector2(this.X, this.Y), new Vector2(mainCharacter.X, mainCharacter.Y)) < 32))
            {
                MoveDirectlyTowardsPlayer(walls);
            }
            else
            {

                if (isFollowingPath) // Continue following path set by pathfinding algorithm if one is set to the nearest grid tile.
                {
                    // Check if the destination set to the next grid tile by the path is reached.
                    if (Math.Floor(vectorDestinationPosition.X) == Math.Floor(this.X) && Math.Floor(vectorDestinationPosition.Y) == Math.Floor(this.Y))
                        isFollowingPath = false;

                    // Move enemy closer to destination at normal speed if it's more than 1 unit away.
                    if (Math.Floor(vectorDestinationPosition.X) - Math.Floor(this.X) > 1)
                    {
                        this.X += (charSpeed * ticksSinceLastUpdate);
                        currentAnimation = walkRight;
                    }
                    else if (Math.Floor(vectorDestinationPosition.X) - Math.Floor(this.X) < -1)
                    {
                        this.X -= (charSpeed * ticksSinceLastUpdate);
                        currentAnimation = walkLeft;
                    }
                    else if (Math.Floor(vectorDestinationPosition.Y) - Math.Floor(this.Y) > 1)
                    {
                        this.Y += (charSpeed * ticksSinceLastUpdate);
                        currentAnimation = walkDown;
                    }
                    else if (Math.Floor(vectorDestinationPosition.Y) - Math.Floor(this.Y) < -1)
                    {
                        this.Y -= (charSpeed * ticksSinceLastUpdate);
                        currentAnimation = walkUp;
                    }
                    // Move enemy closer to destination just a little bit if it's just one unit away. 
                    else if (Math.Floor(vectorDestinationPosition.X) - Math.Floor(this.X) == 1)
                    {
                        this.X += 1;
                    }
                    else if (Math.Floor(vectorDestinationPosition.X) - Math.Floor(this.X) == -1)
                    {
                        this.X -= 1;
                    }
                    else if (Math.Floor(vectorDestinationPosition.Y) - Math.Floor(this.Y) == 1)
                    {
                        this.Y += 1;
                    }
                    else if (Math.Floor(vectorDestinationPosition.Y) - Math.Floor(this.Y) == -1)
                    {
                        this.Y -= 1;
                    }

                }
                else // If not moving toward a set grid tile
                {
                    // Refresh the path to follow periodically
                    if (pathfindingTimer.CompareTo(DateTime.Now) < 0)
                    {
                        pathfindingTimer = DateTime.Now;
                        pathfindingTimer = pathfindingTimer.AddSeconds(0.5);

                        pathToFollow = pathfinder.Pathfind(new Vector2(this.X - this.X % 16, this.Y - this.Y % 16), new Vector2(mainCharacter.X - mainCharacter.X % 16, mainCharacter.Y - mainCharacter.Y % 16), walls);
                        Pathfinder.PathToDraw = pathToFollow;
                    }

                    if (!(pathToFollow.Count == 0)) // If we have a path to follow, set the next position in the path as our immediate destination
                    {
                        vectorDestinationPosition = pathToFollow[pathToFollow.Count - 1];
                        pathToFollow.RemoveAt(pathToFollow.Count - 1);
                    }

                    isFollowingPath = true;
                }

            }
                // select appropriate animation based on movement direction

            currentAnimation.Update(gameTime);
   
            // Logic to stop enemy object from going outside game play area.
            
            if (this.X > graphicsDevice.Viewport.Width - GameConstants.characterWidth)
                this.X -= charSpeed * ticksSinceLastUpdate;

            if (this.X < 0 )
                this.X += charSpeed * ticksSinceLastUpdate;

            if (this.Y > graphicsDevice.Viewport.Height - GameConstants.characterHeight)
                this.Y -= charSpeed * ticksSinceLastUpdate;

            if (this.Y < 0)
                this.Y += charSpeed * ticksSinceLastUpdate;
            

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 topLeftOfSprite = new Vector2(this.X, this.Y);
            Color tintColor = Color.White;
            var sourceRectangle = currentAnimation.CurrentRectangle;

            spriteBatch.Draw(characterSheetTexture, topLeftOfSprite, sourceRectangle, tintColor);

        }


        // Get direction of player character and set enemy to move in that direction at half player speed.

        void MoveDirectlyTowardsPlayer(EnvironmentBlock[] walls)
        {
            Vector2 velocity = GetPlayerVelocity();

            float XPosToMoveTo = this.X + (velocity.X * (charSpeed * ticksSinceLastUpdate));
            float YPosToMoveTo = this.Y + (velocity.Y * (charSpeed * ticksSinceLastUpdate));


            Vector2 vectorImmediatePosToMoveTo = new Vector2(XPosToMoveTo, YPosToMoveTo);

            if (!IsEnvironmentCollision(walls, vectorImmediatePosToMoveTo))
            {
                this.X = XPosToMoveTo;
                this.Y = YPosToMoveTo;
            }
            else
            {
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


        }

        Vector2 GetPlayerVelocity()
        {
            //get velocity of player character relative to enemy's position & normalize so we end up with a direction to face.

            Vector2 desiredVelocity = new Vector2(mainCharacter.X - this.X, mainCharacter.Y - this.Y);

            desiredVelocity.Normalize();

            return desiredVelocity;
        }

        bool IsEnvironmentCollision(EnvironmentBlock[] walls)
        {
            for (int i = 0; i < walls.Length; i++)
            {
                Rectangle wall = new Rectangle((int)walls[i].X, (int)walls[i].Y, 16, 16);
                Rectangle character = new Rectangle((int)this.X, (int)this.Y, 15, 15);

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

        public bool IsPlayerCollision()
        {
            Rectangle mainCharacterRectangle = new Rectangle((int)mainCharacter.X, (int)mainCharacter.Y, 10, 15);
            Rectangle thisEnemyRectangle = new Rectangle((int)this.X, (int)this.Y, 10, 15);
            if (thisEnemyRectangle.Intersects(mainCharacterRectangle))
            {
                this.X = 1;
                this.Y = 1;
                isFollowingPath = false;
                vectorDestinationPosition = new Vector2(0, 0);
                pathToFollow.Clear();
                return true;
            }
            return false;
        }

        public ushort ManhattanDistance(Vector2 source, Vector2 destination)
        {
            return (ushort)Math.Floor(Math.Abs(source.X - destination.X) + (Math.Abs(source.Y - destination.Y)));
        }

    }
}