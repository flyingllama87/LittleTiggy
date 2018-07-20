// #define _DEBUG

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace LittleTiggy
{
    public enum GameState
    {
        menu,
        optionsMenu,
        leaderBoard,
        instructions,
        inGame
    }

    public static class GameConstants
    {
        public const int menuHeight = 1024;
        public const int gameWidth = 512; 
        public const int gameHeight = 512;
        public const int tileSize = 16;
        public const int characterHeight = 15;
        public const int characterWidth = 10;
        public const int noWallsToSpawn = 300;
    }

    public partial class LittleTiggy : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static float gameScaleFactor; // Used to scale game for difference viewport sizes
        public static int viewportWidth;
        public static int viewportHeight;

        GameState gameState; // simple state machine 

        Color colorLTNeutralGrey = new Color(82, 82, 82);
        Color colorLTBlue = new Color(70, 70, 219);
        Color colorLTRed = new Color(219, 70, 70);
        Color colorLTGreen = new Color(70, 200, 70);

        public LittleTiggy()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;

#if DesktopGL
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = 1024;  // Set window width
            graphics.PreferredBackBufferHeight = 1024;   // Set window height
#endif
            graphics.ApplyChanges();

        }

        protected override void Initialize()
        {
            base.Initialize();

            viewportWidth = GraphicsDevice.Viewport.Width;
            viewportHeight = GraphicsDevice.Viewport.Height;

            InitialiseGame();

            InitialiseMenu();

            graphics.ApplyChanges();

            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

        }

        protected override void LoadContent()
        {
            // Create main game objects
            spriteBatch = new SpriteBatch(GraphicsDevice);

            LoadFonts();
            LoadMenuItems();
            LoadGame();

        }

        void LoadFonts()
        {
            smallFont = Content.Load<SpriteFont>("SweetEasy12");
            font = Content.Load<SpriteFont>("SweetEasy36");
#if ANDROID
            bigFont = Content.Load<SpriteFont>("SweetEasy72");
#endif
#if !ANDROID
            bigFont = Content.Load<SpriteFont>("SweetEasy64");
#endif
            arialFont = Content.Load<SpriteFont>("Arial24");
            bigArialFont = Content.Load<SpriteFont>("Arial36");
            gameFont = Content.Load<SpriteFont>("GameFont");
        }


        protected override void Update(GameTime gameTime) 
        {
            base.Update(gameTime);


            // Modify state if player wants to go back.
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) && menuButtonTimer.CompareTo(DateTime.Now) < 0)
            {
                menuButtonTimer = DateTime.Now.AddSeconds(menuButtonTimeSeconds);
                LittleTiggy.menuSound.Play();

                if (gameState != GameState.menu)
                {
                    gameState = GameState.menu;
                }
                else
                {
                    Exit();
                }
            }

            // call appropriate update method depending on game state.
            switch (gameState)
            {
                case GameState.menu:
                    menuUpdate(gameTime);
                    break;
                case GameState.optionsMenu:
                    optionsMenuUpdate(gameTime);
                    break;
                case GameState.inGame:
                    inGameUpdate(gameTime);
                    break;
                case GameState.instructions:
                    instructionsUpdate(gameTime);
                    break;
                case GameState.leaderBoard:
                    leaderBoardUpdate(gameTime);
                    break;
            }
        }

        protected override void Draw(GameTime gameTime) 
        {

            GraphicsDevice.Clear(colorLTNeutralGrey);  // set BG color

            base.Draw(gameTime);

            switch (gameState)  // call appropriate draw method depending on game state.
            {
                case GameState.menu:
                    menuDraw(gameTime);
                    break;
                case GameState.optionsMenu:
                    optionsMenuDraw(gameTime);
                    break;
                case GameState.inGame:
                    inGameDraw(gameTime);
                    break;
                case GameState.instructions:
                    instructionsDraw(gameTime);
                    break;
                case GameState.leaderBoard:
                    leaderBoardDraw(gameTime);
                    break;
            }

            spriteBatch.End(); 

        }
    }
}

