// #define _DEBUG


/*
 * TO DO:
 * - GENERAL: Code clean up / rename / make use of constants / sort out tile aligned values / move code to methods / capatilisation / commenting
 * - FEATURE: Add instructions screen.
 * - FEATURE: Add leaderboards?
 * - PLATFORM: Try to build / deploy on android.
 * - BUGFIX: When idle, player animation does not switch back to normal skin after powerup expires. 
 * 
 * WIN CONDITION: 1) Restart with new map with +1 level 2) show text "you win".
 * LOSE CONDITION: 1) Restart with new map with -1 level 2) Show text "you lose".
 * 
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace LittleTiggy
{
    public static class GameConstants
    {
        public const int windowWidth = 512;
        public const int windowHeight = 512;
        public const int tileSize = 16;
        public const int characterHeight = 15;
        public const int characterWidth = 10;
        public const int noWallsToSpawn = 300;
    }

    public class LittleTiggy : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static float scaleFactor;
        public static int viewportWidth;
        public static int viewportHeight;

        MainCharacter character;
        List<Enemy> enemies;
        List<PowerUp> powerUps;
        SpriteFont mainFont;
        public static EnvironmentBlock[] walls = new EnvironmentBlock[GameConstants.noWallsToSpawn];

        Song songBGM;
        public static SoundEffect powerUpSound;
        SoundEffect winGameSound;
        SoundEffect loseGameSound;
        public static SoundEffect killEnemySound;

        Pathfinder pathfinder;


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

        public static int level { get; set; } = 1;
        // public static int respawn { get; set; } = 1;


        public LittleTiggy()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1024;  // Set window width
            graphics.PreferredBackBufferHeight = 1024;   // Set window height
            graphics.ApplyChanges();

            viewportWidth = GraphicsDevice.Viewport.Width;
            viewportHeight = GraphicsDevice.Viewport.Height;

            if (GraphicsDevice.Viewport.Width > GraphicsDevice.Viewport.Height)
                scaleFactor = GraphicsDevice.Viewport.Height / GameConstants.windowHeight;
            else
                scaleFactor = GraphicsDevice.Viewport.Width / GameConstants.windowWidth;


        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create main game objects
            spriteBatch = new SpriteBatch(GraphicsDevice);
            character = new MainCharacter(this.GraphicsDevice);
            mainFont = Content.Load<SpriteFont>("MainFont");
            songBGM = Content.Load<Song>("bgm");
            MediaPlayer.Play(songBGM);
            MediaPlayer.IsRepeating = true;

            powerUpSound = Content.Load<SoundEffect>("powerup");
            winGameSound = Content.Load<SoundEffect>("wingame");
            loseGameSound = Content.Load<SoundEffect>("losegame");
            killEnemySound = Content.Load<SoundEffect>("killenemy");

            LoadLevel(level);
        }

        public void LoadLevel(int level)
        {

            // main character start position
            MainCharacter.X = 1;
            MainCharacter.Y = 1;

            // Loop until valid start conditions from random generation are met.

            // numberOfPlacedWalls = 0; //Used for debugging only
            // numberOfRandomWalls = 0; //Used for debugging only

            do
            {
                // Place 10 walls randomly (aligned to a GameConstants.tileSizexGameConstants.tileSize grid) around level.

                Random randomNumber = new Random();

                for (int i = 0; i < 10; i++)
                {
                    int gridAlignedX = randomNumber.Next(0, GameConstants.windowWidth);
                    int gridAlignedY = randomNumber.Next(0, GameConstants.windowHeight);

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

                    for (int c = 0; c < GameConstants.windowWidth; c = c + GameConstants.tileSize) //Assess each grid square a column at a time
                    {
                        for (int d = 0; d < GameConstants.windowHeight; d = d + GameConstants.tileSize) // See above
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
                        gridAlignedX = randomNumber.Next(0, GameConstants.windowWidth);
                        gridAlignedY = randomNumber.Next(0, GameConstants.windowHeight);

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

                // powerUp = new PowerUp(this.GraphicsDevice, walls);
                pathfinder = new Pathfinder(this.GraphicsDevice);

                // Loop until player can get to bottom of map & the enemy is in a position to get to the player.

            } while (pathfinder.IsRoutable(new Vector2(0, 0), new Vector2(496, 496), walls) == false || MainCharacter.IsEnvironmentCollision(walls, new Vector2(1, 1)));
            // while (pathfinder.IsRoutable(new Vector2(0, 0), new Vector2(496, 496), walls) == false || pathfinder.IsRoutable(new Vector2(enemy.X, enemy.Y), new Vector2(mainCharacter.X, mainCharacter.Y), walls) == false);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        protected override void Update(GameTime gameTime) // Primary game logic that's updated for every game loop
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            character.Update(gameTime, GraphicsDevice, walls);
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime, GraphicsDevice, walls, pathfinder);
            }
            base.Update(gameTime);
            foreach (PowerUp powerUp in powerUps)
            {
                powerUp.Update(gameTime, GraphicsDevice);
            }

            CheckWinCondition();
            CheckLoseCondition();
        }

        void CheckWinCondition()
        {
            if (MainCharacter.GridAlignedY == GameConstants.windowHeight - (GameConstants.characterHeight + 1))
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
                    if (level > 1)
                        level--;
                    loseGameSound.Play();
                    LoadLevel(level);
                }
            }

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(82, 82, 82)); // set BG color

            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(scaleFactor));

            // Draw each wall
            for (int i = 0; i < walls.Length; i++)
            {
                walls[i].Draw(spriteBatch);
            }

            pathfinder.Draw(spriteBatch);
            character.Draw(spriteBatch);
            foreach (Enemy enemy in enemies)
            {
                enemy.Draw(spriteBatch);
            }
            foreach (PowerUp powerUp in powerUps)
            {
                powerUp.Draw(spriteBatch);
            }
            // powerUp.Draw(spriteBatch);

            // DEBUG code for drawing wall generation and collision information
#if _DEBUG
            spriteBatch.DrawString(mainFont, "Number of Random walls is " + numberOfRandomWalls + ".  Number of Placed Walls is " + numberOfPlacedWalls, new Vector2(20, 20), Color.Black);

            spriteBatch.DrawString(mainFont, "Collision Left: " + collidingLeft + "\nCollision Right: " + collidingRight + "\nCollision Top: " + collidingTop + "\nCollision Bottom: " + collidingBottom, new Vector2(20, 50), Color.Black);
#endif
            spriteBatch.DrawString(mainFont, "Level: " + level, new Vector2(16, 16), Color.Red);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

