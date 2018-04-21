// #define _DEBUG


/*
 * TO DO:
 * - GENERAL: Code clean up / rename / make use of constants / sort out tile aligned values / move code to methods / capatilisation / commenting
 * - GENERAL: Remove use of animation system for static items such as power up & walls
 * - FEATURE: Add text display system
 * - FEATURE: Draw level number
 * - FEATURE: Add logic for power up to allow player to 'capture' enemy and have enemy respawn randomly
 * - FEATURE: Add logic for enemy to run away from player if player is powered up
 * - FEATURE: Add Art for when player is 'powered up'
 * - FEATURE: Add logic for levels (Support for multiple enemies and power ups)
 * - FEATURE: If an enemy is close to the main character, have enemy run directly towards main char
 * - FEATURE: Add instructions screen.
 * - FEATURE: Add music / sounds
 * - FEATURE: Add leaderboards
 * - PLATFORM: Try to build / deploy on android.
 * - BUGFIX: Don't allow maps to spawn if there is a wall at 0,0 
 * 
 * WIN CONDITION: 1) Restart with new map with +1 level 2) show text "you win".
 * LOSE CONDITION: 1) Restart with new map with -1 level 2) Show text "you lose".
 * 
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    }

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        mainCharacter character;
        List<Enemy> enemies;
        PowerUp powerUp;
        SpriteFont mainFont;
        public static EnvironmentBlock[] walls = new EnvironmentBlock[300];

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


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //Mouse.WindowHandle = Window.Handle;
            this.IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 512;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 512;   // set this value to the desired height of your window
            graphics.ApplyChanges();

        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create main game objects
            spriteBatch = new SpriteBatch(GraphicsDevice);
            character = new mainCharacter(this.GraphicsDevice);
            mainFont = Content.Load<SpriteFont>("MainFont");
            LoadLevel(level);
        }

        public void LoadLevel(int level)
        {
            mainCharacter.X = 1;
            mainCharacter.Y = 1;
            // Loop until valid start conditions from random generation are met.

            do
            {

                // Place 10 walls randomly (aligned to a 16x16 grid) around level.

                Random randomNumber = new Random();

                for (int i = 0; i < 10; i++)
                {
                    int gridAlignedX = randomNumber.Next(0, this.GraphicsDevice.Viewport.Width);
                    int gridAlignedY = randomNumber.Next(0, this.GraphicsDevice.Viewport.Height);

                    gridAlignedX = gridAlignedX - (gridAlignedX % 16);
                    gridAlignedY = gridAlignedY - (gridAlignedY % 16);

                    walls[i] = new EnvironmentBlock(this.GraphicsDevice, gridAlignedX, gridAlignedY);
                    // numberOfRandomWalls++;
                }

                // Randomly place more walls (grid aligned) adjacent to other walls to create a maze-ish type level. 

                int f = 0;

                for (int i = 10; i < walls.Length; i++)
                {
                    int gridAlignedX = 0;
                    int gridAlignedY = 0;

                    for (int c = 0; c < this.GraphicsDevice.Viewport.Width; c = c + 16)
                    {
                        for (int d = 0; d < this.GraphicsDevice.Viewport.Height; d = d + 16)
                        {
                            for (int e = f; e < i; e++)
                            {
                                if ((walls[e].X == (c - 16)) || (walls[e].Y == (d - 16)))
                                {
                                    if (randomNumber.Next(1, 100) == 15)
                                    {
                                        gridAlignedX = c;
                                        gridAlignedY = d;
                                        f = e;
                                        f++;
                                        walls[i] = new EnvironmentBlock(this.GraphicsDevice, gridAlignedX, gridAlignedY);
                                        // numberOfPlacedWalls++;
                                        break;
                                    }
                                }
                            }
                            if (gridAlignedX > 0 || gridAlignedY > 0)
                                break;
                        }
                        if (gridAlignedX > 0 || gridAlignedY > 0)
                            break;
                    }

                    if (!(gridAlignedX > 0 || gridAlignedY > 0))
                    {
                        gridAlignedX = randomNumber.Next(0, this.GraphicsDevice.Viewport.Width);
                        gridAlignedY = randomNumber.Next(0, this.GraphicsDevice.Viewport.Height);

                        gridAlignedX = gridAlignedX - (gridAlignedX % 16);
                        gridAlignedY = gridAlignedY - (gridAlignedY % 16);

                        walls[i] = new EnvironmentBlock(this.GraphicsDevice, gridAlignedX, gridAlignedY);
                        // numberOfRandomWalls++;
                    }

                }

                // spawn other elements on map now that walls have been created

                enemies = new List<Enemy>();
                for (int noOfEnemiesToSpawn = 0; noOfEnemiesToSpawn < level; noOfEnemiesToSpawn++)
                {
                    Enemy enemy = new Enemy(this.GraphicsDevice, walls);
                    enemies.Add(enemy);
                }
                

                powerUp = new PowerUp(this.GraphicsDevice, walls);
                pathfinder = new Pathfinder(this.GraphicsDevice);

                // Loop until player can get to bottom of map & the enemy is in a position to get to the player.

            } while (pathfinder.IsRoutable(new Vector2(0, 0), new Vector2(496, 496), walls) == false || mainCharacter.IsEnvironmentCollision(walls, new Vector2(1,1)));
            // while (pathfinder.IsRoutable(new Vector2(0, 0), new Vector2(496, 496), walls) == false || pathfinder.IsRoutable(new Vector2(enemy.X, enemy.Y), new Vector2(mainCharacter.X, mainCharacter.Y), walls) == false);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

 
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            character.Update(gameTime, GraphicsDevice, walls);
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime, GraphicsDevice, walls, pathfinder);
            }
            base.Update(gameTime);
            powerUp.Update(gameTime, GraphicsDevice);
            // pathfinder.Update(GraphicsDevice, walls, enemy);
            CheckWinCondition();
            CheckLoseCondition();
        }

        void CheckWinCondition()
        {
            if (mainCharacter.GridAlignedY == GameConstants.windowHeight - (GameConstants.characterHeight + 1))
            {
                level++;
                LoadLevel(level);
            }
        }

        void CheckLoseCondition()
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy.IsPlayerCollision())
                {
                    if (level > 1)
                        level--;
                    LoadLevel(level);
                }

                    
            }

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(82,82,82)); // set BG color


            spriteBatch.Begin();


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
            powerUp.Draw(spriteBatch);

            // DEBUG code for drawing wall and collision information
#if _DEBUG
            spriteBatch.DrawString(mainFont, "Number of Random walls is " + numberOfRandomWalls + ".  Number of Placed Walls is " + numberOfPlacedWalls, new Vector2(20, 20), Color.Black);

            spriteBatch.DrawString(mainFont, "Collision Left: " + collidingLeft + "\nCollision Right: " + collidingRight + "\nCollision Top: " + collidingTop + "\nCollision Bottom: " + collidingBottom, new Vector2(20, 50), Color.Black);
#endif
            spriteBatch.DrawString(mainFont, "Level: " + level, new Vector2(20, 10), Color.Red);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
