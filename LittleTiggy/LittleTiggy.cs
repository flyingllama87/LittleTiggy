// #define _DEBUG


/*
 * TO DO:
 * - GENERAL: Code clean up / rename / make use of constants / sort out tile aligned values / move code to methods / capatilisation / commenting
 * - FEATURE: Add instructions screen.
 * - FEATURE: Add leaderboards?
 * - FEATURE: Add threading for pathfinding?
 * - Fix up icon for desktopGL builds
 * - Run stylecop over build
 * - Improve controls on android build.
 * - Menu sounds
 * - Menu highlight?
 * 
 * - BUGFIX: Fix issue of virtual keyboard input box not appearing a second time on android builds.
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
    public enum GameState
    {
        menu,
        nameInput,
        leaderBoard,
        instructions,
        inGame
    }

    public static class GameConstants
    {
#if !ANDROID
        public const int menuHeight = 1024;
#endif
#if ANDROID
        public const int menuHeight = 1024;
#endif
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

        public static float gameScaleFactor;
        public static int viewportWidth;
        public static int viewportHeight;

        GameState gameState;

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


        protected override void Update(GameTime gameTime) // Primary game logic that's updated for every game loop
        {
            base.Update(gameTime);

            switch (gameState)
            {
                case GameState.menu:
                    menuUpdate(gameTime);
                    break;
                case GameState.nameInput:
                    changeNameUpdate(gameTime);
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

            switch (gameState)
            {
                case GameState.menu:
                    menuDraw(gameTime);
                    break;
                case GameState.nameInput:
                    changeNameDraw(gameTime);
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

