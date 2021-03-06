﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LittleTiggy
{

    public enum GameDifficulty { Easy, Medium, Hard };
    public enum GameTouchControlMethod { Joystick, ScreenTap}

    public partial class LittleTiggy : Game
    {
        // GAME OBJECTS
        GameBorder gameBorder;
        MainCharacter character;
        List<Enemy> enemies;
        List<PowerUp> powerUps;
        ControlOverlay touchControlOverlay;
        Pathfinder pathfinder;
        public static EnvironmentBlock[] walls = new EnvironmentBlock[GameConstants.noWallsToSpawn];
        public static VirtualThumbstick virtualThumbstick;
        private FrameCounter frameCounter = new FrameCounter(); // Used in debug mode

        // GAME SWITCHES
        bool bChangedDifficulty = false; // used to reset game if player has changed difficulty to stop leaderboard cheating.

        // GAME RESOURCES

        // Sounds
        Song songBGM;
        public static SoundEffect powerUpSound;
        SoundEffect winGameSound;
        SoundEffect loseGameSound;
        public static SoundEffect killEnemySound;

        // Font
        SpriteFont gameFont;

        // GAME SETTINGS - set defaults
        public static int level { get; set; } = 1;
        public static GameDifficulty gameDifficulty = GameDifficulty.Easy;
        public static GameTouchControlMethod gameTouchControlMethod = GameTouchControlMethod.ScreenTap;


#if _DEBUG
        public static bool collidingLeft { get; set; } = false;
        public static bool collidingRight { get; set; } = false;
        public static bool collidingTop { get; set; } = false;
        public static bool collidingBottom { get; set; } = false;
        public static bool collisionTimerOn { get; set; } = false;
        public static string collisionString { get; set; }  = "";
        public static DateTime TimerDateTime { get; set; }
        public static DateTime powerUpCooldown { get; set; }

        public static bool collidingEnemy { get; set; } = false;

        int numberOfRandomWalls = 0;
        int numberOfPlacedWalls = 0;
#endif

        void InitialiseGame()
        {
            if (GraphicsDevice.Viewport.Width > GraphicsDevice.Viewport.Height)
                gameScaleFactor = (float)GraphicsDevice.Viewport.Height / (float)GameConstants.gameHeight;
            else
                gameScaleFactor = (float)GraphicsDevice.Viewport.Width / (float)GameConstants.gameWidth;

            // The following can only be loaded when we know the viewport dimensions and game scale factor.
            gameBorder.LoadContent(GraphicsDevice);
        }

        void LoadGame()
        {
            gameBorder = new GameBorder();
            character = new MainCharacter(GraphicsDevice);
            touchControlOverlay = new ControlOverlay(GraphicsDevice);
            virtualThumbstick = new VirtualThumbstick(GraphicsDevice);
            songBGM = Content.Load<Song>("BackgroundMusic");
            MediaPlayer.Play(songBGM);
            MediaPlayer.IsRepeating = true;

            powerUpSound = Content.Load<SoundEffect>("powerup");
            winGameSound = Content.Load<SoundEffect>("wingame");
            loseGameSound = Content.Load<SoundEffect>("losegame");
            killEnemySound = Content.Load<SoundEffect>("killenemy");

            LoadLevel(level);
        }

        void SubmitScore(int level)
        {
            BackgroundHTTPWorker_Initialise(LeaderBoardAPICall.AddScore);
            LeaderBoardClient LittleTiggyLBClient = new LeaderBoardClient();

            LittleTiggyLBClient.APICall = LeaderBoardAPICall.AddScore;
            LittleTiggyLBClient.name = playerName;
            LittleTiggyLBClient.score = level;
            LittleTiggyLBClient.difficulty = gameDifficulty.ToString();

            BackgroundHTTPWorker.RunWorkerAsync(LittleTiggyLBClient);
        }

        public void LoadLevel(int level)
        {

            if (level > 1 && !string.IsNullOrEmpty(playerName) && !bDisableNetworkCalls)
            {
                SubmitScore(level);
                bGetScoresRequested = false; // Allow the player to get the latest leaderboard scores if they've won a level
            }

            // main character start position
            MainCharacter.X = 1;
            MainCharacter.Y = 1;
            character.isMovingToTile = false;

            // Loop until valid start conditions from random generation are met.

            // numberOfPlacedWalls = 0; //Used for debugging only
            // numberOfRandomWalls = 0; //Used for debugging only

            do
            {
                // Place 10 walls randomly (aligned to a GameConstants.tileSizexGameConstants.tileSize grid) around level.

                Random randomNumber = new Random();

                for (int i = 0; i < 10; i++)
                {
                    int gridAlignedX = randomNumber.Next(0, GameConstants.gameWidth);
                    int gridAlignedY = randomNumber.Next(0, GameConstants.gameHeight);

                    gridAlignedX = gridAlignedX - (gridAlignedX % GameConstants.tileSize);
                    gridAlignedY = gridAlignedY - (gridAlignedY % GameConstants.tileSize);

                    walls[i] = new EnvironmentBlock(this.GraphicsDevice, gridAlignedX, gridAlignedY);
                    // numberOfRandomWalls++; //Used for debugging only
                }

                // Randomly place more walls (grid aligned) adjacent to other walls to create a maze-ish level.  This works but is confusing - should have commented it more when I wrote it.

                int f = 0;

                for (int i = 10; i < walls.Length; i++) // Keep going until we have a position for every wall
                {
                    int gridAlignedX = 0;
                    int gridAlignedY = 0;

                    for (int c = 0; c < GameConstants.gameWidth; c = c + GameConstants.tileSize) //Assess each grid square a column at a time
                    {
                        for (int d = 0; d < GameConstants.gameHeight; d = d + GameConstants.tileSize) // See above
                        {
                            for (int e = f; e < i; e++) // Used to count between previously generated walls? & the total number of placed walls
                            {
                                if ((walls[e].X == (c - GameConstants.tileSize)) || (walls[e].Y == (d - GameConstants.tileSize))) // If any previously generated walls exists above or to the left of the current tile being evaluated
                                {
                                    if (randomNumber.Next(1, 100) == 15) // This is only True 1% of the time however as each grid square gets evaluated for every wall to be allocated, it gets a lot of attempts.
                                    {
                                        gridAlignedX = c;
                                        gridAlignedY = d;
                                        f = e;
                                        f++;
                                        walls[i] = new EnvironmentBlock(this.GraphicsDevice, gridAlignedX, gridAlignedY);
                                        // numberOfPlacedWalls++; // Used for debugging only
                                        break; // We've assigned a wall, so exit the loop
                                    }
                                }
                            }
                            if (gridAlignedX > 0 || gridAlignedY > 0) // If we assigned a wall, exit loop
                                break;
                        }
                        if (gridAlignedX > 0 || gridAlignedY > 0) // If we assigned a wall, exit loop
                            break;
                    }

                    if (!(gridAlignedX > 0 || gridAlignedY > 0)) // If the above code did not generate a wall adjacent to an already existing wall based on chance, place a random wall.
                    {
                        gridAlignedX = randomNumber.Next(0, GameConstants.gameWidth);
                        gridAlignedY = randomNumber.Next(0, GameConstants.gameHeight);

                        gridAlignedX = gridAlignedX - (gridAlignedX % GameConstants.tileSize);
                        gridAlignedY = gridAlignedY - (gridAlignedY % GameConstants.tileSize);

                        walls[i] = new EnvironmentBlock(this.GraphicsDevice, gridAlignedX, gridAlignedY);
                        // numberOfRandomWalls++; // Used for debugging only
                    }

                }

                // spawn other elements on map now that walls have been created

                enemies = new List<Enemy>();
                for (int noOfEnemiesToSpawn = 0; noOfEnemiesToSpawn < level; noOfEnemiesToSpawn++)
                {
                    Enemy enemy = new Enemy(this.GraphicsDevice, walls);
                    enemies.Add(enemy);
                }

                powerUps = new List<PowerUp>();
                for (int noOfPowerUpsToSpawn = 0; noOfPowerUpsToSpawn < (level / 2); noOfPowerUpsToSpawn++)
                {
                    PowerUp powerUp = new PowerUp(this.GraphicsDevice, walls);
                    powerUps.Add(powerUp);
                }

                pathfinder = new Pathfinder(this.GraphicsDevice);

                // Loop until player can get to bottom of map & the enemy is in a position to get to the player.

            } while (pathfinder.IsRoutable(new Vector2(0, 0), new Vector2(496, 496), walls) == false || MainCharacter.IsEnvironmentCollision(walls, new Vector2(1, 1)));
        }

        void inGameUpdate(GameTime gameTime)
        {

            if (bChangedDifficulty) // reset game if player changes difficulty
            {
                bChangedDifficulty = false;
                level = 1;
                LoadLevel(level);
            }
                
            virtualThumbstick.Update();

            character.Update(gameTime, GraphicsDevice, walls);

            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime, GraphicsDevice, walls);
            }

            foreach (PowerUp powerUp in powerUps)
            {
                powerUp.Update(gameTime, GraphicsDevice);
            }

            CheckWinCondition();
            CheckLoseCondition();
        }

        void CheckWinCondition()
        {
            if (MainCharacter.GridAlignedY == GameConstants.gameHeight - (GameConstants.characterHeight + 1))
            {
                level++;
                winGameSound.Play();
                LoadLevel(level);
            }
        }

        void CheckLoseCondition()
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy.IsPlayerCollision() && !MainCharacter.isPoweredUp)
                {
                    if (level > 1 && !(LittleTiggy.gameDifficulty == GameDifficulty.Easy))
                        level--;
                    loseGameSound.Play();
                    LoadLevel(level);
                }
            }
        }

        void inGameDraw(GameTime gameTime)
        {

            // Scale game to render on render targets of all sizes
            Matrix renderMatrix = Matrix.CreateScale(gameScaleFactor);

            // Set up matrix to translate game to middle of viewport.
            float offsetY = (viewportHeight - (GameConstants.gameHeight * gameScaleFactor)) / 2;
            float offsetX = (viewportWidth - (GameConstants.gameWidth * gameScaleFactor)) / 2;
            Matrix translationMatrix = Matrix.CreateTranslation(offsetX, offsetY, 0);

            renderMatrix = Matrix.Multiply(renderMatrix, translationMatrix); 

            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, renderMatrix);

            // Draw each wall
            for (int i = 0; i < walls.Length; i++)
            {
                walls[i].Draw(spriteBatch);
            }

            // pathfinder.Draw(spriteBatch); // Used for visualising pathfinding path
            
            foreach (Enemy enemy in enemies)
            {
                enemy.Draw(spriteBatch);
            }
            foreach (PowerUp powerUp in powerUps)
            {
                powerUp.Draw(spriteBatch);
            }

            character.Draw(spriteBatch);

            spriteBatch.DrawString(gameFont, "Level: " + level, new Vector2(16, 16), colorLTGreen);


            // Draw FPS counter if name is debug.
            if (LittleTiggy.playerName.ToUpper() == "DEBUG")
            {
                var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                frameCounter.Update(deltaTime);
                var fps = string.Format("{0}", frameCounter.CurrentFramesPerSecond.ToString("#"));
                spriteBatch.DrawString(gameFont, fps, new Vector2(512 - 32, 16), colorLTGreen);
                // virtualThumbstick.Draw(spriteBatch);
            }


            // DEBUG code for drawing wall generation and collision information
#if _DEBUG
            spriteBatch.DrawString(gameFont, "Number of Random walls is " + numberOfRandomWalls + ".  Number of Placed Walls is " + numberOfPlacedWalls, new Vector2(20, 20), Color.Black);

            spriteBatch.DrawString(gameFont, "Collision Left: " + collidingLeft + "\nCollision Right: " + collidingRight + "\nCollision Top: " + collidingTop + "\nCollision Bottom: " + collidingBottom, new Vector2(20, 50), Color.Black);
#endif


#if ANDROID // If we are on android, draw a border and touch controls joystick
            if (LittleTiggy.gameTouchControlMethod == GameTouchControlMethod.ScreenTap)
                touchControlOverlay.Draw(spriteBatch); // Draw the touch control overlay if control scheme is set appropriately


            spriteBatch.End(); // End current spriteBatch used for game elements that are scaled to viewport size.

            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp); // start a new non-scaled or offset spriteBatch 

            // Draw game border
            gameBorder.Draw(spriteBatch);

            if (LittleTiggy.gameTouchControlMethod == GameTouchControlMethod.Joystick)
                virtualThumbstick.Draw(spriteBatch);

            // Main LT source file ends the spriteBatch
#endif

            
#if !ANDROID // Debug on PC if name is 'debug'
            if (LittleTiggy.playerName.ToUpper() == "DEBUG" && LittleTiggy.gameTouchControlMethod == GameTouchControlMethod.Joystick)
            {
                spriteBatch.End(); // End current spriteBatch used for game elements that are scaled to viewport size.
                spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp); // start a new non-scaled or offset spriteBatch 
                virtualThumbstick.Draw(spriteBatch);
            }
#endif 

        }

    }

    class GameBorder // Used to set up and draw the game's border on mobile devices.
    {
        Texture2D borderTexture;
        int borderStartY;
        int borderStartX;
        int borderSizeX;
        int borderSizeY;
        Rectangle leftBorder;
        Rectangle rightBorder;
        Rectangle topBorder;
        Rectangle bottomBorder;

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            borderTexture = new Texture2D(graphicsDevice, 1, 1);
            borderTexture.SetData(new Color[] { Color.White });

            borderStartY = (int)(LittleTiggy.viewportHeight - ((float)GameConstants.gameHeight * LittleTiggy.gameScaleFactor)) / 2;
            borderStartX = (int)(LittleTiggy.viewportWidth - ((float)GameConstants.gameWidth * LittleTiggy.gameScaleFactor)) / 2;

            borderSizeX = (int)((float)GameConstants.gameHeight * LittleTiggy.gameScaleFactor) - 1;
            borderSizeY = (int)((float)GameConstants.gameHeight * LittleTiggy.gameScaleFactor) - 1;

            leftBorder = new Rectangle(borderStartX, borderStartY, 1, borderSizeY);
            rightBorder = new Rectangle(borderStartX + borderSizeX, borderStartY, 1, borderSizeY);
            topBorder = new Rectangle(borderStartX, borderStartY, borderSizeX, 1);
            bottomBorder = new Rectangle(borderStartX, borderStartY + borderSizeY, borderSizeX, 1);

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(borderTexture, leftBorder, Color.Red);
            spriteBatch.Draw(borderTexture, rightBorder, Color.Red);
            spriteBatch.Draw(borderTexture, topBorder, Color.Red);
            spriteBatch.Draw(borderTexture, bottomBorder, Color.Red);
        }
    }

    public class FrameCounter // Thank you to https://stackoverflow.com/questions/20676185/xna-monogame-getting-the-frames-per-second
    {
        public FrameCounter()
        {
        }

        public long TotalFrames { get; private set; }
        public float TotalSeconds { get; private set; }
        public float AverageFramesPerSecond { get; private set; }
        public float CurrentFramesPerSecond { get; private set; }

        public const int MAXIMUM_SAMPLES = 100;

        private Queue<float> _sampleBuffer = new Queue<float>();

        public bool Update(float deltaTime)
        {
            CurrentFramesPerSecond = 1.0f / deltaTime;

            _sampleBuffer.Enqueue(CurrentFramesPerSecond);

            if (_sampleBuffer.Count > MAXIMUM_SAMPLES)
            {
                _sampleBuffer.Dequeue();
                AverageFramesPerSecond = _sampleBuffer.Average(i => i);
            }
            else
            {
                AverageFramesPerSecond = CurrentFramesPerSecond;
            }

            TotalFrames++;
            TotalSeconds += deltaTime;
            return true;
        }
    }


}
