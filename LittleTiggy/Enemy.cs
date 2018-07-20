using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace LittleTiggy
{

    public class Enemy
    {
        // ENEMY GRAPHICS
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

        // USED FOR ENEMY SPEED
        float charSpeed;
        long ticksSinceLastUpdate = 0;

        // PATHFINDING OBJECTS AND VALUES
        private BackgroundWorker BackgroundPathfinderWorker = new BackgroundWorker();
        Pathfinder enemyPathfinder;
        Vector2 vectorDestinationPosition { get; set; }
        bool isFollowingPath = false;
        List<Vector2> pathToFollow;
        DateTime pathfindingTimer = DateTime.Now;
        double pathfindingLongTimerIntervalSeconds = 2.0; //normally 2.0 // Used when a path to player is already established but should be updated as players move :)
        double pathfindingShortTimerIntervalSeconds = 0.5; //normally 0.5 // Used when a path to player is not establised and a path is needed sooner rather than later

        // ENEMY POSITION
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

            // Set enemy speed depending on game difficulty
            if (LittleTiggy.gameDifficulty == GameDifficulty.Easy)
                this.charSpeed = 0.000004F;
            else if (LittleTiggy.gameDifficulty == GameDifficulty.Medium)
                this.charSpeed = 0.000006F;
            else // Hard
                this.charSpeed = 0.00001F;

            this.enemyPathfinder = new Pathfinder(graphicsDevice);
            BackgroundPathfinderWorker_Initialise();
            
            if (characterSheetTexture == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/CharacterSheet.png"))
                {
                    characterSheetTexture = Texture2D.FromStream(graphicsDevice, stream);
                }
            }

            // define animation frames
            
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
            
            do // Spawn enemy in grid aligned random position on map where no walls exist.
            {
                this.X = (float)randomNumber.Next(128, GameConstants.gameWidth - GameConstants.tileSize); // Don't allow enemy to spawn too close to the player start position.
                this.Y = randomNumber.Next(128, GameConstants.gameHeight - GameConstants.tileSize);

                this.X -= this.X % 16;
                this.Y -= this.Y % 16;

            } while (IsEnvironmentCollision(walls));

        }

        public void Update(GameTime gameTime, GraphicsDevice graphicsDevice, EnvironmentBlock[] walls)
        {
            ticksSinceLastUpdate = gameTime.ElapsedGameTime.Ticks;

            if (MainCharacter.isPoweredUp) // If the player is powered up.
            {
                AvoidPlayer(walls); // Avoid the player
            }
            else // If the player is not powered up
            {
                if ((ManhattanDistance(new Vector2(this.X, this.Y), new Vector2(MainCharacter.X, MainCharacter.Y)) < 32)) // If we are close to the character, run directly towards them.
                {
                    MoveDirectlyTowardsPlayer(walls);
                }
                else // If not, use a* pathfinding algorithm
                {
                    if (isFollowingPath) // If a path is set, continue following path to the nearest grid tile.
                    {
                        FollowPath();
                    }
                    else // If not moving toward a set grid tile, set a path!
                    {
                        SetPath(walls);
                    }
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

        void FollowPath()
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

        void SetPath( EnvironmentBlock[] walls)
        {
            // Refresh the path to follow periodically
            if (pathfindingTimer.CompareTo(DateTime.Now) < 0 && !BackgroundPathfinderWorker.IsBusy)
            {
                pathfindingTimer = DateTime.Now;
                pathfindingTimer = pathfindingTimer.AddSeconds(pathfindingShortTimerIntervalSeconds);

                enemyPathfinder.from = new Vector2(this.X - this.X % 16, this.Y - this.Y % 16);
                enemyPathfinder.destination = new Vector2(MainCharacter.X - MainCharacter.X % 16, MainCharacter.Y - MainCharacter.Y % 16);
                enemyPathfinder.walls = walls;

                BackgroundPathfinderWorker.RunWorkerAsync();

            }

            if (pathToFollow != null && !(pathToFollow.Count == 0)) // If we have a path to follow, set the next position in the path as our immediate destination
            {
                vectorDestinationPosition = pathToFollow.LastOrDefault();
                pathToFollow.RemoveAt(pathToFollow.Count - 1);
                isFollowingPath = true;
            }

            
        }

        void AvoidPlayer(EnvironmentBlock[] walls)
        {
            if (isFollowingPath)
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
            else
            {
                // Refresh the path to follow periodically
                if (pathfindingTimer.CompareTo(DateTime.Now) < 0 && !BackgroundPathfinderWorker.IsBusy)
                {
                    pathfindingTimer = DateTime.Now;
                    pathfindingTimer = pathfindingTimer.AddSeconds(pathfindingLongTimerIntervalSeconds);

                    float randomGridAlignedX;
                    float randomGridAlignedY;

                    do
                    {
                        randomGridAlignedX = randomNumber.Next(0, 512);
                        randomGridAlignedY = randomNumber.Next(0, 512);

                        randomGridAlignedX -= randomGridAlignedX % 16;
                        randomGridAlignedY -= randomGridAlignedY % 16;

                    } while (IsEnvironmentCollision(walls, new Vector2(randomGridAlignedX, randomGridAlignedY)));


                    enemyPathfinder.from = new Vector2(this.X - this.X % 16, this.Y - this.Y % 16);
                    enemyPathfinder.destination = new Vector2(randomGridAlignedX, randomGridAlignedY);
                    enemyPathfinder.walls = walls;

                    BackgroundPathfinderWorker.RunWorkerAsync();

                }

                if (pathToFollow != null && !(pathToFollow.Count == 0)) // If we have a path to follow, set the next position in the path as our immediate destination
                {
                    vectorDestinationPosition = pathToFollow[pathToFollow.Count - 1];
                    pathToFollow.RemoveAt(pathToFollow.Count - 1);
                    isFollowingPath = true;
                }
                else
                {
                    isFollowingPath = false;
                }

                
            }
        }

        List<Vector2> OptomisePath(List<Vector2> inPath) 
        {
            // If the path finding algo returns a path, optomise it so that any series of nodes in a line are compressed to a single node/destination for the enemy player.

            if (inPath == null || inPath.Count == 0)
            {
                return new List<Vector2>();
            }
            else
            {
                List<Vector2> optomisedPath = new List<Vector2>();

                Vector2 startPosition;
                Vector2 lastNode = new Vector2(0,0);
                Vector2 destinationNode = new Vector2(0,0);

                NodeDirection nodeDirection = NodeDirection.Unset;
                NodeDirection lastNodeDirection = NodeDirection.Unset;
                // NodeDirection lineDirection;

                startPosition = inPath.First<Vector2>(); // pop off the first element to prime the loop
                lastNode = inPath.First<Vector2>();
                optomisedPath.Add(inPath.First<Vector2>());
                inPath.Remove(inPath.First<Vector2>());

                foreach (Vector2 Node in inPath) // This is processed forwards and builds a List of vectors to be returned along the way, which is exactly what the enemy character expects.
                {
                    Vector2 difference = Node - lastNode;

                    if (difference.X == 16)
                    {
                        nodeDirection = NodeDirection.Right;
                    }
                    else if (difference.X == -16)
                    {
                        nodeDirection = NodeDirection.Left;
                    }
                    else if (difference.Y == 16)
                    {
                        nodeDirection = NodeDirection.Down;
                    }
                    else if (difference.Y == -16)
                    {
                        nodeDirection = NodeDirection.Up;
                    }
                    else
                    {
                        Debug.Assert(true);
                    }

                    

                    if (lastNodeDirection != NodeDirection.Unset && lastNodeDirection != nodeDirection)
                    {
                        optomisedPath.Add(startPosition);
                        startPosition = Node;
                    }
                    else
                    {
                        startPosition = startPosition + difference;
                    }

                    lastNodeDirection = nodeDirection;
                    lastNode = Node;

                }

                optomisedPath.Add(startPosition);

                return optomisedPath;
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (LittleTiggy.playerName.ToUpper() == "DEBUG")
            {
                enemyPathfinder.Draw(spriteBatch); //draw pathfinding path for debugging
            }
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

            Vector2 desiredVelocity = new Vector2(MainCharacter.X - this.X, MainCharacter.Y - this.Y);

            desiredVelocity.Normalize();

            return desiredVelocity;
        }

        private void BackgroundPathfinderWorker_Initialise()
        {
            BackgroundPathfinderWorker.DoWork += new DoWorkEventHandler(BackgroundPathfinderWorker_DoWork);
            BackgroundPathfinderWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundPathfinderWorker_Complete);

        }

        private void BackgroundPathfinderWorker_DoWork(object sender, DoWorkEventArgs eventArgs)
        {
               eventArgs.Result = this.enemyPathfinder.Pathfind();

        }

        private void BackgroundPathfinderWorker_Complete(object sender, RunWorkerCompletedEventArgs eventArgs)
        {
            if (eventArgs.Result != null)
            {
                // bPathfinderTaskComplete = true;
                // Debug.WriteLine("BG Pathfinding task completed");

                pathToFollow = (List<Vector2>)eventArgs.Result;
                enemyPathfinder.PathToDraw = pathToFollow;
                pathToFollow = OptomisePath(pathToFollow);
                
                

                if (pathToFollow.Count != 0)
                {
                    isFollowingPath = true;
                    vectorDestinationPosition = pathToFollow[pathToFollow.Count - 1];
                    pathToFollow.RemoveAt(pathToFollow.Count - 1);
                }
                else
                {
                    Debug.WriteLine("Pathfinding count is 0! Pathfinding from ", enemyPathfinder.from.ToString(), "To: ", enemyPathfinder.destination.ToString());
                }
            }

            // BackgroundPathfinderWorker.DoWork -= new DoWorkEventHandler(BackgroundPathfinderWorker_DoWork);
            // BackgroundPathfinderWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(BackgroundPathfinderWorker_Complete);
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
            Rectangle mainCharacterRectangle = new Rectangle((int)MainCharacter.X, (int)MainCharacter.Y, 10, 15);
            Rectangle thisEnemyRectangle = new Rectangle((int)this.X, (int)this.Y, 10, 15);
            if (thisEnemyRectangle.Intersects(mainCharacterRectangle))
            {
                this.X = 1;
                this.Y = 1;
                isFollowingPath = false;
                vectorDestinationPosition = new Vector2(0, 0);
                if (pathToFollow != null)
                    pathToFollow.Clear();
                if (MainCharacter.isPoweredUp)
                    LittleTiggy.killEnemySound.Play();
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