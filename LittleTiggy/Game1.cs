#define _DEBUG

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;


namespace LittleTiggy
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        // Texture2D characterSheetTexture;
        mainCharacter character;
        SpriteFont mainFont;
        public EnvironmentBlock[] walls = new EnvironmentBlock[300];
        // public EnvironmentBlock[] walls = new EnvironmentBlock[10];

        public static bool collidingLeft { get; set; } = false;
        public static bool collidingRight { get; set; } = false;
        public static bool collidingTop { get; set; } = false;
        public static bool collidingBottom { get; set; } = false;
        public static bool collisionTimerOn { get; set; } = false;
        public static string collisionString { get; set; }  = "";
        public static DateTime TimerDateTime { get; set; }

        public static int score { get; set; } = 0;
        public static int respawn { get; set; } = 30;

        int numberOfRandomWalls = 0;
        int numberOfPlacedWalls = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //Mouse.WindowHandle = Window.Handle;
            this.IsMouseVisible = true;

            // mainFont = new SpriteFont();


            graphics.PreferredBackBufferWidth = 512;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 512;   // set this value to the desired height of your window
            graphics.ApplyChanges();

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            character = new mainCharacter(this.GraphicsDevice);
            mainFont = Content.Load<SpriteFont>("MainFont");
            // EnvironmentBlock[] walls = new EnvironmentBlock[5];

            Random randomNumber = new Random();

            for (int i = 0; i < 10; i++)
            {
                int gridAlignedX = randomNumber.Next(0, this.GraphicsDevice.Viewport.Width);
                int gridAlignedY = randomNumber.Next(0, this.GraphicsDevice.Viewport.Height);

                gridAlignedX = gridAlignedX - (gridAlignedX % 16);
                gridAlignedY = gridAlignedY - (gridAlignedY % 16);

                walls[i] = new EnvironmentBlock(this.GraphicsDevice, gridAlignedX, gridAlignedY);
                numberOfRandomWalls++;
            }

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
                                    numberOfPlacedWalls++;
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
                    numberOfRandomWalls++;
                }


                //gridAlignedX = gridAlignedX - (gridAlignedX % 16);
                //gridAlignedY = gridAlignedY - (gridAlignedY % 16);


            }

            /*
            using (var stream = TitleContainer.OpenStream("Content/charactersheet.png"))
            {
                characterSheetTexture = Texture2D.FromStream(this.GraphicsDevice, stream);
            }*/


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            character.Update(gameTime, GraphicsDevice, walls);
            base.Update(gameTime);

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(82,82,82));


            // TODO: Add your drawing code here

            spriteBatch.Begin();

            for (int i = 0; i < walls.Length; i++)
            { 

                walls[i].Draw(spriteBatch);
            }


            character.Draw(spriteBatch);

#if _DEBUG
            spriteBatch.DrawString(mainFont, "Number of Random walls is " + numberOfRandomWalls + ".  Number of Placed Walls is " + numberOfPlacedWalls, new Vector2(20, 20), Color.Black);

            spriteBatch.DrawString(mainFont, "Collision Left: " + collidingLeft + "\nCollision Right: " + collidingRight + "\nCollision Top: " + collidingTop + "\nCollision Bottom: " + collidingBottom, new Vector2(20, 50), Color.Black);
#endif
            //Vector2 topLeftOfSprite = new Vector2(50, 50);
            //Color tintColor = Color.White;

            //spriteBatch.Draw(characterSheetTexture, topLeftOfSprite, tintColor);

            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
