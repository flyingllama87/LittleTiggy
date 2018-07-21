using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LittleTiggy
{

    public partial class LittleTiggy : Game
    {

        // Menu resources
        Texture2D menuRectangle;
        Texture2D menuRectangleHover; // Used to change color of menu rectangle on mouse hover.
        public static SoundEffect menuSound;
        private SpriteFont smallFont, font, bigFont, arialFont, bigArialFont;

        // Variables used to assist UI layout
        bool[] menuButtonHover = new bool[4]; // Used to set/get which button the mouse is hovering over, if any.
        Vector2 mouseXY;
        int menuItemSpaceDistance = 25; // Used as the standard spacing between UI elements
        int menuButtonXOffset; // Used to set the starting X position to draw the menu button rectangle.  Varies depending on viewport width.
        public static float menuScaleFactor;  // Used to scale menu draw vertically on portrait screens (mobile)

        DateTime menuButtonTimer = DateTime.Now; 
        double menuButtonTimeSeconds = 0.3; // Used to disallow activating a button again for the specified length of time.
        bool bHasTappedButtonLastUpdate = false; // used as button activation (i.e. rectangle intersection) can be detected for multiple update loops but we only need the button to be activated once.

        DateTime leaderboardTimeoutTimer = DateTime.Now;
        double leaderboardTimeoutSeconds = LittleTiggy.apiTimeOut / 1000; // Used to countdown timeout of connection to leaderboard.

        // Game option switches
        public static bool bHasEnteredName = false;
        bool bPlayerRequestedGame = false;
        bool bFlashPlayerNamePrompt = false;

        // Player name vars
        static public string kbInput = "";
        static public string kbName = "";
        static public string playerName = "";
        Task<string> androidNameTask = null; // Used for name input on android

#if !ANDROID // Class for keyboard input on desktop platforms.
        KbNameHandler kbNameHandler = new KbNameHandler();
#endif

        void InitialiseMenu()
        {
            menuButtonXOffset = (viewportWidth - 600) / 2;
            menuScaleFactor = (float)GraphicsDevice.Viewport.Height / (float)GameConstants.menuHeight;
        }


        void LoadMenuItems()
        {
            if (menuRectangle == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/RoundedRectangle.png"))
                {
                    menuRectangle = Texture2D.FromStream(GraphicsDevice, stream);
                }
            }

            if (menuRectangleHover == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/RoundedRectangleDarkGreen.png"))
                {
                    menuRectangleHover = Texture2D.FromStream(GraphicsDevice, stream);
                }
            }

            menuSound = Content.Load<SoundEffect>("killenemy");

            GameOptionsFile.LoadOptions();
        }

        void resetButtonHover()
        {
            menuButtonHover = new bool[4];
        }

        void menuUpdate(GameTime gameTime)
        {
            resetButtonHover();
            MouseState mouseState = Mouse.GetState();
            mouseXY = new Vector2(mouseState.X, mouseState.Y);

            TouchCollection touchCollection = TouchPanel.GetState();
            Vector2 touchXY = new Vector2(0, 0);


            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                touchXY = new Vector2(mouseState.X, mouseState.Y); // Mouse clicks are treated as touches.
            }
            else if (touchCollection.Count > 0)
            {
                touchXY = new Vector2(touchCollection[0].Position.X, touchCollection[0].Position.Y);
            }

            Rectangle mouseRectangle = new Rectangle((int)mouseXY.X, (int)mouseXY.Y, 2, 2);
            Rectangle touchRectangle = new Rectangle((int)touchXY.X, (int)touchXY.Y, 2, 2);

            // int elementPositionY += menuRectangle.Height + menuItemSpaceDistance * (int)menuScaleFactor;

            Rectangle rectanglePlayGame = new Rectangle(menuButtonXOffset, (int)(275 * menuScaleFactor), 600, 150);
            Rectangle rectangleChangeName = new Rectangle(menuButtonXOffset, (int)(450 * menuScaleFactor), 600, 150);
            Rectangle rectangleLeaderBoards = new Rectangle(menuButtonXOffset, (int)(625 * menuScaleFactor), 600, 150);
            Rectangle rectangleInstructions = new Rectangle(menuButtonXOffset, (int)(800 * menuScaleFactor), 600, 150);

            if (touchXY.X != 0 && touchXY.Y != 0)
            {
                if (rectanglePlayGame.Intersects(touchRectangle))
                {
                    LittleTiggy.menuSound.Play();

                    if (!bHasEnteredName)  //Get player to enter their name if they try to play without entering it.
                    {
                        gameState = GameState.optionsMenu;
                        bPlayerRequestedGame = true;
                    }
                    else
                        gameState = GameState.inGame;

                }
                if (rectangleChangeName.Intersects(touchRectangle))
                {
                    LittleTiggy.menuSound.Play();
                    gameState = GameState.optionsMenu;
                }
                if (rectangleLeaderBoards.Intersects(touchRectangle))
                {
                    LittleTiggy.menuSound.Play();
                    gameState = GameState.leaderBoard;
                }
                if (rectangleInstructions.Intersects(touchRectangle))
                {
                    LittleTiggy.menuSound.Play();
                    gameState = GameState.instructions;
                }
            }

            if (mouseXY.X != 0 && mouseXY.Y != 0)
            {
                if (rectanglePlayGame.Intersects(mouseRectangle))
                {
                    menuButtonHover[0] = true;
                }
                if (rectangleChangeName.Intersects(mouseRectangle))
                {
                    menuButtonHover[1] = true;
                }
                if (rectangleLeaderBoards.Intersects(mouseRectangle))
                {
                    menuButtonHover[2] = true;
                }
                if (rectangleInstructions.Intersects(mouseRectangle))
                {
                    menuButtonHover[3] = true;
                }
            }
        }

        void optionsMenuUpdate(GameTime gameTime)
        {

            resetButtonHover();
            MouseState mouseState = Mouse.GetState();
            mouseXY = new Vector2(mouseState.X, mouseState.Y);
            TouchCollection touchCollection = TouchPanel.GetState();
            Vector2 touchXY = new Vector2(0, 0);
            

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                touchXY = new Vector2(mouseState.X, mouseState.Y);
            }
            else if (touchCollection.Count > 0)
            {
                touchXY = new Vector2(touchCollection[0].Position.X, touchCollection[0].Position.Y);
            }

            // Convert position of mouse and touch input to rectangles to be able to use intersection methods.
            Rectangle mouseRectangle = new Rectangle((int)mouseXY.X, (int)mouseXY.Y, 1, 1);
            Rectangle touchRectangle = new Rectangle((int)touchXY.X, (int)touchXY.Y, 1, 1);

            // Set rectangles used to layout options menu.
            int elementPositionY = menuItemSpaceDistance; // Used to position the elements of the menu from top to bottom.  Set first element position.
            Rectangle rectangleGoBack = new Rectangle(menuButtonXOffset, (int)(elementPositionY * menuScaleFactor), 600, 150);
            elementPositionY += menuRectangle.Height + menuItemSpaceDistance * (int)menuScaleFactor;

            Rectangle rectangleChangeName = new Rectangle(menuButtonXOffset, (int)(elementPositionY * menuScaleFactor), 600, 150);
            elementPositionY += menuRectangle.Height + menuItemSpaceDistance * (int)menuScaleFactor;

            Vector2 stringSize = font.MeasureString("STRING"); // Used to get the height of a string rendered with this font.  Used here to match layout in draw functions.
            elementPositionY += (int)stringSize.Y * (int)menuScaleFactor; 

            Rectangle rectangleSetDifficulty = new Rectangle(menuButtonXOffset, (int)(elementPositionY * menuScaleFactor), 600, 150);
            elementPositionY += menuRectangle.Height + (int)stringSize.Y;

            Rectangle rectangleSetTouchControl = new Rectangle(menuButtonXOffset, (int)(elementPositionY * menuScaleFactor), 600, 150);

            // Logic for UI elements

            if (bHasEnteredName == false)
            {

#if ANDROID     
                if (androidNameTask == null && !KeyboardInput.IsVisible)
                {
                    androidNameTask = KeyboardInput.Show("Name", "What's your name?", "Player");
                }

                if (androidNameTask != null && androidNameTask.IsCompleted)
                {
                    if (androidNameTask.Result != null)
                    { 
                        playerName = androidNameTask.Result;
                        bHasEnteredName = true;
                    }
                }

#endif

#if !ANDROID
                if (touchXY.X > 0 || touchXY.Y > 0) // If the user is clicking or tapping anywhere if they are supposed to be typing their name, flash text to prompt user to type name.
                {
                    bFlashPlayerNamePrompt = true;
                    menuButtonTimer = DateTime.Now.AddSeconds(menuButtonTimeSeconds); // Flash text for this long.
                }

                if (kbName == "")
                {
                    kbNameHandler.Update();
                }
                else
                {
                    playerName = kbName;
                    bHasEnteredName = true;
                }

                bHasTappedButtonLastUpdate = false;
#endif
            }
            else // Don't let them click/tap anything if they haven't entered their name
            {
                if (touchXY.X != 0 && touchXY.Y != 0) // Touch control
                {
                    if (rectangleGoBack.Intersects(touchRectangle))
                    {
                        try
                        {
                            GameOptionsFile.SaveOptions();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Unable to save, Error: " + ex.Message);
                        }

                        if (bPlayerRequestedGame)
                            gameState = GameState.inGame;
                        else
                        {
                            gameState = GameState.menu;
                        }
                        LittleTiggy.menuSound.Play();
                    }

                    if (rectangleChangeName.Intersects(touchRectangle) && !bHasTappedButtonLastUpdate && menuButtonTimer.CompareTo(DateTime.Now) < 0)
                    {
                        menuButtonTimer = DateTime.Now.AddSeconds(menuButtonTimeSeconds);
                        LittleTiggy.menuSound.Play();
                        bHasTappedButtonLastUpdate = true; // Used as sometimes a touch is held for multiple updates but we only want the button to be activated once.

                        bHasEnteredName = false;
                        androidNameTask = null;
                        kbName = "";
                        kbInput = "";

                    }

                    if (rectangleSetDifficulty.Intersects(touchRectangle) && !bHasTappedButtonLastUpdate && menuButtonTimer.CompareTo(DateTime.Now) < 0)
                    {
                        menuButtonTimer = DateTime.Now.AddSeconds(menuButtonTimeSeconds);
                        LittleTiggy.menuSound.Play();
                        bHasTappedButtonLastUpdate = true; // Used as sometimes a touch is held for multiple updates but we only want the button to be activated once.

                        if (gameDifficulty == GameDifficulty.Hard)
                            gameDifficulty = GameDifficulty.Easy;
                        else
                            gameDifficulty = gameDifficulty + 1;

                        bChangedDifficulty = true; // Used to reset the game if the difficulty has been changed to stop leaderboard cheating.

                    }

                    if (rectangleSetTouchControl.Intersects(touchRectangle) && !bHasTappedButtonLastUpdate && menuButtonTimer.CompareTo(DateTime.Now) < 0)
                    {
                        menuButtonTimer = DateTime.Now.AddSeconds(menuButtonTimeSeconds);
                        LittleTiggy.menuSound.Play();
                        bHasTappedButtonLastUpdate = true; // Used as sometimes a touch is held for multiple updates but we only want the button to be activated once.

                        if (gameTouchControlMethod == GameTouchControlMethod.Joystick)
                            gameTouchControlMethod = GameTouchControlMethod.ScreenTap;
                        else
                            gameTouchControlMethod = GameTouchControlMethod.Joystick;
                    }

                }
                else
                {
                    bHasTappedButtonLastUpdate = false;
                }


                if (mouseXY.X != 0 && mouseXY.Y != 0) // Mouse control
                {
                    if (rectangleGoBack.Intersects(mouseRectangle))
                    {
                        menuButtonHover[0] = true;
                    }
                    if (rectangleChangeName.Intersects(mouseRectangle))
                    {
                        menuButtonHover[1] = true;
                    }
                    if (rectangleSetDifficulty.Intersects(mouseRectangle))
                    {
                        menuButtonHover[2] = true;
                    }
                    if (rectangleSetTouchControl.Intersects(mouseRectangle))
                    {
                        menuButtonHover[3] = true;
                    }
                }
            }

        }


        void instructionsUpdate(GameTime gameTime)
        {
            resetButtonHover();
            MouseState mouseState = Mouse.GetState();
            mouseXY = new Vector2(mouseState.X, mouseState.Y);
            TouchCollection touchCollection = TouchPanel.GetState();
            Vector2 touchXY = new Vector2(0, 0);

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                touchXY = new Vector2(mouseState.X, mouseState.Y);
            }
            else if (touchCollection.Count > 0)
            {
                touchXY = new Vector2(touchCollection[0].Position.X, touchCollection[0].Position.Y);
            }

            Rectangle touchRectangle = new Rectangle((int)touchXY.X, (int)touchXY.Y, 1, 1);
            Rectangle rectangleGoBack = new Rectangle(menuButtonXOffset, (int)(10 * menuScaleFactor), 600, 150);
            Rectangle mouseRectangle = new Rectangle((int)mouseXY.X, (int)mouseXY.Y, 2, 2);

            if (mouseXY.X != 0 && mouseXY.Y != 0)
            {
                if (rectangleGoBack.Intersects(mouseRectangle))
                {
                    menuButtonHover[0] = true;
                }
            }

            if (touchXY.X != 0 && touchXY.Y != 0)
            {
                if (rectangleGoBack.Intersects(touchRectangle) == true)
                {
                    LittleTiggy.menuSound.Play();
                    gameState = GameState.menu;
                }
            }
        }

        void leaderBoardUpdate(GameTime gameTime)
        {

            // Call LeaderBoardAPI GetScores method on new thread via BG worker
            if (!bGetScoresRequested && !LittleTiggy.bDisableNetworkCalls)
            {
                BackgroundHTTPWorker_Initialise(LeaderBoardAPICall.GetScores);
                LeaderBoardClient LittleTiggyLBClient = new LeaderBoardClient();
                LittleTiggyLBClient.APICall = LeaderBoardAPICall.GetScores;
                BackgroundHTTPWorker.RunWorkerAsync(LittleTiggyLBClient);
                bGetScoresRequested = true;
            }


            resetButtonHover();
            MouseState mouseState = Mouse.GetState();
            mouseXY = new Vector2(mouseState.X, mouseState.Y);
            TouchCollection touchCollection = TouchPanel.GetState();
            Vector2 touchXY = new Vector2(0, 0);

            float elementPositionY = (float)menuItemSpaceDistance * menuScaleFactor;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                touchXY = new Vector2(mouseState.X, mouseState.Y);
            }
            else if (touchCollection.Count > 0)
            {
                touchXY = new Vector2(touchCollection[0].Position.X, touchCollection[0].Position.Y);
            }

            Rectangle mouseRectangle = new Rectangle((int)mouseXY.X, (int)mouseXY.Y, 2, 2);
            Rectangle touchRectangle = new Rectangle((int)touchXY.X, (int)touchXY.Y, 1, 1);
            Rectangle rectangleGoBack = new Rectangle(menuButtonXOffset, (int)(elementPositionY), 600, 150);

            if (mouseXY.X != 0 && mouseXY.Y != 0)
            {
                if (rectangleGoBack.Intersects(mouseRectangle))
                {
                    menuButtonHover[0] = true;
                }
            }

            if (touchXY.X != 0 && touchXY.Y != 0)
            {
                if (rectangleGoBack.Intersects(touchRectangle) == true)
                {
                    LittleTiggy.menuSound.Play();
                    gameState = GameState.menu;
                }
            }
        }


        void menuDraw(GameTime gameTime)
        {

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null);

            // Draw Logo

            Vector2 stringSize = bigFont.MeasureString("LittleTiggy");
            Vector2 textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 0);

            spriteBatch.DrawString(bigFont, "LittleTiggy", textPosition, colorLTRed);

            textPosition = new Vector2((viewportWidth / 2) + 150, (stringSize.Y - 25));
            stringSize = smallFont.MeasureString("By Morgan");

            spriteBatch.DrawString(smallFont, "By Morgan", textPosition, colorLTRed);

            // Draw Rectangles for menu items

            if (menuButtonHover[0] == true)
                spriteBatch.Draw(menuRectangleHover, new Vector2(menuButtonXOffset, 275 * menuScaleFactor), Color.White);
            else
                spriteBatch.Draw(menuRectangle, new Vector2(menuButtonXOffset, 275 * menuScaleFactor), Color.White);

            if (menuButtonHover[1] == true)
                spriteBatch.Draw(menuRectangleHover, new Vector2(menuButtonXOffset, 450 * menuScaleFactor), Color.White);
            else
                spriteBatch.Draw(menuRectangle, new Vector2(menuButtonXOffset, 450 * menuScaleFactor), Color.White);

            if (menuButtonHover[2] == true)
                spriteBatch.Draw(menuRectangleHover, new Vector2(menuButtonXOffset, 625 * menuScaleFactor), Color.White);
            else
                spriteBatch.Draw(menuRectangle, new Vector2(menuButtonXOffset, 625 * menuScaleFactor), Color.White);

            if (menuButtonHover[3] == true)
                spriteBatch.Draw(menuRectangleHover, new Vector2(menuButtonXOffset, 800 * menuScaleFactor), Color.White);
            else
                spriteBatch.Draw(menuRectangle, new Vector2(menuButtonXOffset, 800 * menuScaleFactor), Color.White);

            // Draw menu items

            stringSize = font.MeasureString("Play Game");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 275 * menuScaleFactor);
            spriteBatch.DrawString(font, "Play Game", textPosition, colorLTRed);

            stringSize = font.MeasureString("Options");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 450 * menuScaleFactor);
            spriteBatch.DrawString(font, "Options", textPosition, colorLTRed);

            stringSize = font.MeasureString("Leader Board");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 625 * menuScaleFactor);
            spriteBatch.DrawString(font, "Leader Board", textPosition, colorLTRed);

            stringSize = font.MeasureString("Instructions");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 800 * menuScaleFactor);
            spriteBatch.DrawString(font, "Instructions", textPosition, colorLTRed);

        }

        void optionsMenuDraw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null);
            
            Vector2 stringSize; 
            Vector2 textPosition; // Where to star drawing the text string.

            int elementPositionY = menuItemSpaceDistance; // Where to draw the first menu element.

            // Draw First button & button text.  Either back or start depending on how the player has reached the screen (via menu or if they went to start the game without a name set)
            if (menuButtonHover[0] == true)
                spriteBatch.Draw(menuRectangleHover, new Vector2(menuButtonXOffset, elementPositionY * menuScaleFactor), Color.White);
            else
                spriteBatch.Draw(menuRectangle, new Vector2(menuButtonXOffset, elementPositionY * menuScaleFactor), Color.White);
           

            if (!bPlayerRequestedGame)
            {
                stringSize = font.MeasureString("Go Back");
                textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), elementPositionY * menuScaleFactor);
                spriteBatch.DrawString(font, "Go Back", textPosition, colorLTRed);
            }
            else
            {
                stringSize = font.MeasureString("Start Game");
                textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), elementPositionY * menuScaleFactor);
                spriteBatch.DrawString(font, "Start Game", textPosition, colorLTRed);
            }

            elementPositionY += menuRectangle.Height + menuItemSpaceDistance;

            // Draw Change Name button & button text
            if (menuButtonHover[1] == true)
                spriteBatch.Draw(menuRectangleHover, new Vector2(menuButtonXOffset, elementPositionY * menuScaleFactor), Color.White);
            else
                spriteBatch.Draw(menuRectangle, new Vector2(menuButtonXOffset, elementPositionY * menuScaleFactor), Color.White);

            stringSize = font.MeasureString("Change Name");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), elementPositionY * menuScaleFactor);
            spriteBatch.DrawString(font, "Change Name", textPosition, colorLTRed);

            elementPositionY += menuRectangle.Height;



#if !ANDROID

            // Draw message prompting user to start typing or display player name!

            if (kbInput == "")
            {
                stringSize = font.MeasureString("Please type your name and press Enter!");
                textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), elementPositionY * menuScaleFactor);
                if (bFlashPlayerNamePrompt)  // draw this in red if the flash flag is set.  Used so this text 'pops' on screen to draw attention to player.
                    spriteBatch.DrawString(font, "Please type your name and press Enter!", textPosition, colorLTRed);
                else
                    spriteBatch.DrawString(font, "Please type your name and press Enter!", textPosition, colorLTGreen); 
            }
            else // Draw player name
            {
                stringSize = font.MeasureString("Player name is ");
                Vector2 stringSize2 = font.MeasureString(kbInput);
                textPosition = new Vector2((viewportWidth / 2) - ((stringSize.X + stringSize2.X) / 2), elementPositionY * menuScaleFactor);
                spriteBatch.DrawString(font, "Player name is ", textPosition, colorLTGreen);
                textPosition = new Vector2((viewportWidth / 2) - ((stringSize.X + stringSize2.X) / 2) + stringSize.X, elementPositionY * menuScaleFactor);

                if (playerName.ToUpper() != "LOLA") // Easter egg for my daughter :)
                    spriteBatch.DrawString(font, kbInput, textPosition, colorLTRed);
                else
                    spriteBatch.DrawString(font, kbInput, textPosition, Color.DeepPink);
            }


            if (menuButtonTimer.CompareTo(DateTime.Now) < 0) // If the timer has elapsed, allow the above text to draw normally
                bFlashPlayerNamePrompt = false;
#endif

#if ANDROID // Draw player name
            stringSize = font.MeasureString("Player name is ");
            Vector2 stringSize2 = font.MeasureString(playerName);
            textPosition = new Vector2((viewportWidth / 2) - ((stringSize.X + stringSize2.X) / 2), elementPositionY * menuScaleFactor);
            spriteBatch.DrawString(font, "Player name is ", textPosition, colorLTGreen);
            textPosition = new Vector2((viewportWidth / 2) - ((stringSize.X + stringSize2.X) / 2) + stringSize.X, elementPositionY * menuScaleFactor);

            if (playerName.ToUpper() != "LOLA") // Easter egg for my daughter :)
                spriteBatch.DrawString(font, playerName, textPosition, colorLTRed);
            else
                spriteBatch.DrawString(font, playerName, textPosition, Color.DeepPink);
#endif
            elementPositionY += (int)stringSize.Y; // Add height of last drawn element to y pos tracker 

            // Draw difficulty button & button text.

            if (menuButtonHover[2])
                spriteBatch.Draw(menuRectangleHover, new Vector2(menuButtonXOffset, elementPositionY * menuScaleFactor), Color.White);
            else
                spriteBatch.Draw(menuRectangle, new Vector2(menuButtonXOffset, elementPositionY * menuScaleFactor), Color.White);

            stringSize = font.MeasureString(LittleTiggy.gameDifficulty.ToString());
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), elementPositionY * menuScaleFactor);
            spriteBatch.DrawString(font, LittleTiggy.gameDifficulty.ToString(), textPosition, colorLTRed);

            elementPositionY += menuRectangle.Height + menuItemSpaceDistance;

            // Draw touch control header text, button & button text.

            stringSize = font.MeasureString("Touch Control Options:");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), elementPositionY * menuScaleFactor);
            spriteBatch.DrawString(font, "Touch Control Options:", textPosition, colorLTGreen);

            elementPositionY += (int)stringSize.Y;

            if (menuButtonHover[3])
                spriteBatch.Draw(menuRectangleHover, new Vector2(menuButtonXOffset, elementPositionY * menuScaleFactor), Color.White);
            else
                spriteBatch.Draw(menuRectangle, new Vector2(menuButtonXOffset, elementPositionY * menuScaleFactor), Color.White);

            if (LittleTiggy.gameTouchControlMethod == GameTouchControlMethod.ScreenTap)
            {
                stringSize = font.MeasureString("Screen Tap");
                textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), elementPositionY * menuScaleFactor);
                spriteBatch.DrawString(font, "Screen Tap", textPosition, colorLTRed);
            }
            else
            {
                stringSize = font.MeasureString("Virtual Joystick");
                textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), elementPositionY * menuScaleFactor);
                spriteBatch.DrawString(font, "Virtual Joystick", textPosition, colorLTRed);
            }

        }


        void leaderBoardDraw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null);

            float elementPositionY = (float)menuItemSpaceDistance * menuScaleFactor;

            // Draw back button
            if (menuButtonHover[0] == true)
                spriteBatch.Draw(menuRectangleHover, new Vector2(menuButtonXOffset, elementPositionY), Color.White);
            else
                spriteBatch.Draw(menuRectangle, new Vector2(menuButtonXOffset, elementPositionY), Color.White);

            Vector2 stringSize = font.MeasureString("Go Back");
            Vector2 textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), elementPositionY);
            spriteBatch.DrawString(font, "Go Back", textPosition, colorLTRed);

            // Draw Leaderboard

            if (bGetScoresComplete == true) // Draw table of scores if the call has completed.
            {
                elementPositionY += (float)menuRectangle.Height + (((float)menuItemSpaceDistance) * (float)menuScaleFactor);

                stringSize = font.MeasureString("Name   Level   Difficulty");
                textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), elementPositionY);
                spriteBatch.DrawString(font, "Name   Level   Difficulty", textPosition, colorLTRed);

                // Get the X position of the start, middle and end of the header to layout scores.  Add offsets for formatting due to large font used by heading.
                float scorePositionNameX = textPosition.X + 50; ;
                float scorePositionScoreX = textPosition.X + (stringSize.X / 2) - 40;
                float scorePositionDifficultyX = textPosition.X + (stringSize.X - 25);

                elementPositionY += stringSize.Y;

                int counter = 0;
                foreach (Tuple<string, int, string> scoreEntry in leaderBoardScores)
                {
                    counter++;

                    Vector2 stringSizeDifficulty = arialFont.MeasureString(scoreEntry.Item3);

                    if (scoreEntry.Item1.ToUpper() != "LOLA") // Easter egg for my daughter :)
                    {
                        spriteBatch.DrawString(arialFont, scoreEntry.Item1, new Vector2(scorePositionNameX, elementPositionY), colorLTGreen);
                        spriteBatch.DrawString(arialFont, scoreEntry.Item2.ToString(), new Vector2(scorePositionScoreX, elementPositionY), colorLTGreen);
                        spriteBatch.DrawString(arialFont, scoreEntry.Item3, new Vector2(scorePositionDifficultyX - stringSizeDifficulty.X, elementPositionY), colorLTGreen);
                    }
                    else
                    {
                        spriteBatch.DrawString(arialFont, scoreEntry.Item1, new Vector2(scorePositionNameX, elementPositionY), Color.DeepPink);
                        spriteBatch.DrawString(arialFont, scoreEntry.Item2.ToString(), new Vector2(scorePositionScoreX, elementPositionY), Color.DeepPink);
                        spriteBatch.DrawString(arialFont, scoreEntry.Item3, new Vector2(scorePositionDifficultyX - stringSizeDifficulty.X, elementPositionY), Color.DeepPink);
                    }
                        
                    elementPositionY += arialFont.MeasureString(scoreEntry.Item1).Y + (float)menuItemSpaceDistance * (int)menuScaleFactor;

                }
            }
            else if (leaderboardTimeoutTimer.CompareTo(DateTime.Now) < 0 && !LittleTiggy.bDisableNetworkCalls) // Set a timer for leaderboard timeout if we haven't already done so and we haven't disabled network calls.
            {
                leaderboardTimeoutTimer = DateTime.Now.AddSeconds(leaderboardTimeoutSeconds);
            }
            else if (leaderboardTimeoutTimer.CompareTo(DateTime.Now) > 0 && !LittleTiggy.bDisableNetworkCalls) // Display timer that counts down until Leaderboard server times out
            {
                TimeSpan timeLeftBeforeTimeout = leaderboardTimeoutTimer.Subtract(DateTime.Now);

                stringSize = bigArialFont.MeasureString("Connecting...");
                textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), (viewportHeight / 2) - (stringSize.Y / 2) - (100f * menuScaleFactor));
                spriteBatch.DrawString(bigArialFont, "Connecting...", textPosition, colorLTGreen);

                stringSize = bigArialFont.MeasureString(timeLeftBeforeTimeout.Seconds.ToString() +  "." + timeLeftBeforeTimeout.Milliseconds.ToString().Substring(0,1) + " before timeout.");
                textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), (viewportHeight / 2) - (stringSize.Y / 2));
                spriteBatch.DrawString(bigArialFont, timeLeftBeforeTimeout.Seconds.ToString() + "." + timeLeftBeforeTimeout.Milliseconds.ToString().Substring(0, 1) + " before timeout.", textPosition, colorLTGreen);
            }

            if (LittleTiggy.bDisableNetworkCalls)
            {
                stringSize = font.MeasureString("Network Error");
                textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 500 * menuScaleFactor);
                spriteBatch.DrawString(font, "Network Error", textPosition, colorLTGreen);
            }
        }

        void instructionsDraw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null);

            // Draw back button
            if (menuButtonHover[0] == true)
                spriteBatch.Draw(menuRectangleHover, new Vector2(menuButtonXOffset, 10 * menuScaleFactor), Color.White);
            else
                spriteBatch.Draw(menuRectangle, new Vector2(menuButtonXOffset, 10 * menuScaleFactor), Color.White);

            Vector2 stringSize = font.MeasureString("Go Back");
            Vector2 textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 10 * menuScaleFactor);
            spriteBatch.DrawString(font, "Go Back", textPosition, colorLTRed);


            // Draw instructions

            int linePositionY = 175;

            stringSize = bigArialFont.MeasureString("- Get to the bottom of the map to win the level.");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), linePositionY * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "- Get to the bottom of the map to win the level.", textPosition, colorLTGreen);

            linePositionY += (int)stringSize.Y + 10;
            stringSize = bigArialFont.MeasureString("- Avoid the enemies trying to tag you.");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), linePositionY * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "- Avoid the enemies trying to tag you.", textPosition, colorLTGreen);

            linePositionY += (int)stringSize.Y + 10;
            stringSize = bigArialFont.MeasureString("- If you get the power up,");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), linePositionY * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "- If you get the power up,", textPosition, colorLTGreen);

            linePositionY += (int)stringSize.Y - 10;
            stringSize = bigArialFont.MeasureString("you can tag the enemies temporarily.");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), linePositionY * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "you can tag the enemies temporarily.", textPosition, colorLTGreen);

            linePositionY += (int)stringSize.Y + 10;
            stringSize = bigArialFont.MeasureString("- If you get tagged you will go back a level.");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), linePositionY * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "- If you get tagged you will go back a level.", textPosition, colorLTGreen);

            linePositionY += (int)stringSize.Y + 10;
            stringSize = bigArialFont.MeasureString("- Use the keyboard or mouse on PC.");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), linePositionY * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "- Use the keyboard or mouse on PC.", textPosition, colorLTGreen);

            linePositionY += (int)stringSize.Y + 30;
            stringSize = bigArialFont.MeasureString("Touch controls:");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), linePositionY * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "Touch controls:", textPosition, colorLTRed);

            linePositionY += (int)stringSize.Y + 10;
            stringSize = bigArialFont.MeasureString("- Provides a tap based control or virtual joystick.");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), linePositionY * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "- Provides a tap based control or virtual joystick.", textPosition, colorLTGreen);

            linePositionY += (int)stringSize.Y + 10;
            stringSize = bigArialFont.MeasureString("- Use of two hands is recommended with");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), linePositionY * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "- Use of two hands is recommended with", textPosition, colorLTGreen);

            linePositionY += (int)stringSize.Y - 10;
            stringSize = bigArialFont.MeasureString("tap control.");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), linePositionY * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "tap control.", textPosition, colorLTGreen);

        }

    }

    public class KbNameHandler
    {
        private Keys[] lastPressedKeys;
        public Keys[] pressedKeys;

        public KbNameHandler()
        {
            lastPressedKeys = new Keys[0];
        }

        public void Update()
        {
            KeyboardState kbState = Keyboard.GetState();
            pressedKeys = kbState.GetPressedKeys();

            //check if the currently pressed keys were already pressed
            foreach (Keys key in pressedKeys)
            {
                if (!lastPressedKeys.Contains(key))
                    OnKeyDown(key);
            }

            //save the currently pressed keys so we can compare on the next update
            lastPressedKeys = pressedKeys;
        }

        private void OnKeyDown(Keys key)
        {
            if (key.Equals(Keys.Enter))
            {
                LittleTiggy.menuSound.Play();
                LittleTiggy.kbName = LittleTiggy.kbInput;
            }
            else if (key.Equals(Keys.Back) && LittleTiggy.kbInput.Length > 0)
            {
                LittleTiggy.kbInput = LittleTiggy.kbInput.Remove(LittleTiggy.kbInput.Length - 1, 1);
            }
            else if ((key >= Keys.A && key <= Keys.Z))
            {

                // Limit length of this str?
                if (!lastPressedKeys.Contains(Keys.LeftShift) && !pressedKeys.Contains(Keys.RightShift))
                {
                    LittleTiggy.kbInput += key.ToString().ToLower();
                }
                else
                {
                    LittleTiggy.kbInput += key.ToString();
                }
            }
            else if (key == Keys.Space)
            {
                LittleTiggy.kbInput += " ";
            }
        }
    }

}
