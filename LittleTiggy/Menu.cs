﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LittleTiggy
{
    public partial class LittleTiggy : Game
    {
        Texture2D menuRectangle;
        int menuButtonOffset;
        private SpriteFont smallFont, font, bigFont, arialFont, bigArialFont;
        bool bHasEnteredName = false;
        static public string kbInput = "";
        static public string kbName = "";
        static public string playerName = "";
        Task<string> androidNameTask = null;
        public static float menuScaleFactor;
#if !ANDROID
        KbNameHandler kbNameHandler = new KbNameHandler();
#endif

        void InitialiseMenu()
        {
            menuButtonOffset = (viewportWidth - 600) / 2;
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
        }

        void menuUpdate(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
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

            Rectangle touchRectangle = new Rectangle((int)touchXY.X, (int)touchXY.Y, 2, 2);
            Rectangle rectanglePlayGame = new Rectangle(menuButtonOffset, (int)(275 * menuScaleFactor), 600, 150);
            Rectangle rectangleChangeName = new Rectangle(menuButtonOffset, (int)(450 * menuScaleFactor), 600, 150);
            Rectangle rectangleLeaderBoards = new Rectangle(menuButtonOffset, (int)(625 * menuScaleFactor), 600, 150);
            Rectangle rectangleInstructions = new Rectangle(menuButtonOffset, (int)(800 * menuScaleFactor), 600, 150);

            if (touchXY.X != 0 && touchXY.Y != 0)
            {

                if (rectanglePlayGame.Intersects(touchRectangle))
                {
                    gameState = GameState.inGame;
                }
                if (rectangleChangeName.Intersects(touchRectangle))
                {
                    gameState = GameState.nameInput;
                }
                if (rectangleLeaderBoards.Intersects(touchRectangle))
                {
                    gameState = GameState.leaderBoard;
                }
                if (rectangleInstructions.Intersects(touchRectangle))
                {
                    gameState = GameState.instructions;
                }

            }

            // message = "Rect position: " + rectanglePlayGame.X + " " + rectanglePlayGame.Y + " Touch Pos: " + touchXY.X + " " + touchXY.Y;
        }

        void changeNameUpdate(GameTime gameTime)
        {

            MouseState mouseState = Mouse.GetState();
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

            if (touchXY.X != 0 && touchXY.Y != 0)
            {
                Rectangle touchRectangle = new Rectangle((int)touchXY.X, (int)touchXY.Y, 1, 1);
                Rectangle rectangleGoBack = new Rectangle(menuButtonOffset, (int)(10 * menuScaleFactor), 600, 150);
                Rectangle rectangleChangeName = new Rectangle(menuButtonOffset, (int)(185 * menuScaleFactor), 600, 150);


                if (rectangleGoBack.Intersects(touchRectangle) == true)
                {
                    gameState = GameState.menu;
                }
                if (rectangleChangeName.Intersects(touchRectangle) == true)
                {
                    bHasEnteredName = false;
                    androidNameTask = null;
                    kbName = "";
                    bAddScoreComplete = false;
                }

            }

            if (bHasEnteredName == false && !LittleTiggy.bLeaderboardNetworkFailure)
            {
                if (androidNameTask == null && kbName == "")
                {
#if ANDROID
                    androidNameTask = KeyboardInput.Show("Name", "What's your name?", "Player");
#endif

#if !ANDROID
                    kbNameHandler.Update();
#endif

                }
                else if (androidNameTask != null && androidNameTask.IsCompleted && !bLeaderboardNetworkFailure)
                {
                    playerName = androidNameTask.Result;

                    BackgroundHTTPWorker_Initialise(LeaderBoardAPICall.AddScore);
                    LeaderBoardClient LittleTiggyLBClient = new LeaderBoardClient();

                    LittleTiggyLBClient.APICall = LeaderBoardAPICall.AddScore;
                    LittleTiggyLBClient.name = playerName;
                    LittleTiggyLBClient.score = 12;

                    BackgroundHTTPWorker.RunWorkerAsync(LittleTiggyLBClient);

                    bHasEnteredName = true;

                }
                else if (kbName != "" && !bLeaderboardNetworkFailure)
                {
                    playerName = kbName;

                    BackgroundHTTPWorker_Initialise(LeaderBoardAPICall.AddScore);
                    LeaderBoardClient LittleTiggyLBClient = new LeaderBoardClient();

                    LittleTiggyLBClient.APICall = LeaderBoardAPICall.AddScore;
                    LittleTiggyLBClient.name = playerName;
                    LittleTiggyLBClient.score = 12;

                    BackgroundHTTPWorker.RunWorkerAsync(LittleTiggyLBClient);

                    bHasEnteredName = true;
                }
            }
        }


        void instructionsUpdate(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
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

            if (touchXY.X != 0 && touchXY.Y != 0)
            {
                Rectangle touchRectangle = new Rectangle((int)touchXY.X, (int)touchXY.Y, 1, 1);
                Rectangle rectangleGoBack = new Rectangle(menuButtonOffset, (int)(10 * menuScaleFactor), 600, 150);

                if (rectangleGoBack.Intersects(touchRectangle) == true)
                {
                    gameState = GameState.menu;
                }
            }
        }

        void leaderBoardUpdate(GameTime gameTime)
        {

            // Call LeaderBoardAPI GetScores method on new thread via BG worker
            if (!bGetScoresRequested && !LittleTiggy.bLeaderboardNetworkFailure)
            {
                BackgroundHTTPWorker_Initialise(LeaderBoardAPICall.GetScores);
                LeaderBoardClient LittleTiggyLBClient = new LeaderBoardClient();
                LittleTiggyLBClient.APICall = LeaderBoardAPICall.GetScores;
                BackgroundHTTPWorker.RunWorkerAsync(LittleTiggyLBClient);
                bGetScoresRequested = true;
            }

            MouseState mouseState = Mouse.GetState();
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

            if (touchXY.X != 0 && touchXY.Y != 0)
            {
                Rectangle touchRectangle = new Rectangle((int)touchXY.X, (int)touchXY.Y, 1, 1);
                Rectangle rectangleGoBack = new Rectangle(menuButtonOffset, (int)(1 * menuScaleFactor), 600, 150);

                if (rectangleGoBack.Intersects(touchRectangle) == true)
                {
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

            spriteBatch.Draw(menuRectangle, new Vector2(menuButtonOffset, 275 * menuScaleFactor), Color.White);
            spriteBatch.Draw(menuRectangle, new Vector2(menuButtonOffset, 450 * menuScaleFactor), Color.White);
            spriteBatch.Draw(menuRectangle, new Vector2(menuButtonOffset, 625 * menuScaleFactor), Color.White);
            spriteBatch.Draw(menuRectangle, new Vector2(menuButtonOffset, 800 * menuScaleFactor), Color.White);

            // Draw menu items

            stringSize = font.MeasureString("Play Game");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 275 * menuScaleFactor);
            spriteBatch.DrawString(font, "Play Game", textPosition, colorLTRed);

            stringSize = font.MeasureString("Change Name");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 450 * menuScaleFactor);
            spriteBatch.DrawString(font, "Change Name", textPosition, colorLTRed);

            stringSize = font.MeasureString("Leader Board");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 625 * menuScaleFactor);
            spriteBatch.DrawString(font, "Leader Board", textPosition, colorLTRed);

            stringSize = font.MeasureString("Instructions");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 800 * menuScaleFactor);
            spriteBatch.DrawString(font, "Instructions", textPosition, colorLTRed);

            // Draw debug stuff

            // stringSize = arialFont.MeasureString("Viewport size: " + viewportWidth + " " + viewportHeight + ".  Scale Factor: " + menuScaleFactor);
            // textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), viewportHeight - (stringSize.Y));
            // spriteBatch.DrawString(arialFont, "Viewport size: " + viewportWidth + " " + viewportHeight + ".  Scale Factor: " + menuScaleFactor, textPosition, colorLTGreen);

            // Draw gamestate & touch pos  

            // stringSize = arialFont.MeasureString(message);
            // textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 750 * menuScaleFactor);
            // spriteBatch.DrawString(arialFont, message, textPosition, colorLTRed);

        }

        void changeNameDraw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null);

            // Draw back button
            spriteBatch.Draw(menuRectangle, new Vector2(menuButtonOffset, 10 * menuScaleFactor), Color.White);

            Vector2 stringSize = font.MeasureString("Go Back");
            Vector2 textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 10 * menuScaleFactor);
            spriteBatch.DrawString(font, "Go Back", textPosition, colorLTRed);

            // Draw change name button
            spriteBatch.Draw(menuRectangle, new Vector2(menuButtonOffset, 185 * menuScaleFactor), Color.White);

            stringSize = font.MeasureString("Change Name");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 185 * menuScaleFactor);
            spriteBatch.DrawString(font, "Change Name", textPosition, colorLTRed);


            // Draw player name
#if !ANDROID
            stringSize = font.MeasureString("Player name is ");
            Vector2 stringSize2 = font.MeasureString(kbInput);
            textPosition = new Vector2((viewportWidth / 2) - ((stringSize.X + stringSize2.X) / 2), 600 * menuScaleFactor);
            spriteBatch.DrawString(font, "Player name is ", textPosition, colorLTRed);
            textPosition = new Vector2((viewportWidth / 2) - ((stringSize.X + stringSize2.X) / 2) + stringSize.X, 600 * menuScaleFactor);
            spriteBatch.DrawString(font, kbInput, textPosition, colorLTGreen);
#endif

#if ANDROID
            stringSize = font.MeasureString("Player name is ");
            Vector2 stringSize2 = font.MeasureString(playerName);
            textPosition = new Vector2((viewportWidth / 2) - ((stringSize.X + stringSize2.X) / 2), 600 * menuScaleFactor);
            spriteBatch.DrawString(font, "Player name is ", textPosition, colorLTRed);
            textPosition = new Vector2((viewportWidth / 2) - ((stringSize.X + stringSize2.X) / 2 + stringSize2.X), 600 * menuScaleFactor);
            spriteBatch.DrawString(font, playerName, textPosition, colorLTGreen);
#endif

            // Draw message prompting user to start typing!

            if (bHasEnteredName == false)
            {
                stringSize = font.MeasureString("Type your name and press Enter!");
                textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 700 * menuScaleFactor);
                spriteBatch.DrawString(font, "Type your name and press Enter!", textPosition, colorLTGreen);
            }
        }


        void leaderBoardDraw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null);

            // Draw back button
            spriteBatch.Draw(menuRectangle, new Vector2(menuButtonOffset, 10 * menuScaleFactor), Color.White);

            Vector2 stringSize = font.MeasureString("Go Back");
            Vector2 textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 10 * menuScaleFactor);
            spriteBatch.DrawString(font, "Go Back", textPosition, colorLTRed);

            // Draw gamestate string 
            // stringSize = arialFont.MeasureString(message);
            // textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 700 * menuScaleFactor);
            // spriteBatch.DrawString(arialFont, message, textPosition, colorLTRed);

            // Draw Leaderboard

            if (bGetScoresComplete == true)
            {
                stringSize = font.MeasureString("Name   Score");
                textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 150 * menuScaleFactor);
                spriteBatch.DrawString(font, "Name   Score", textPosition, colorLTRed);

                int counter = 0;
                foreach (Tuple<string, int> scoreEntry in leaderBoardScores)
                {
                    counter++;
                    stringSize = arialFont.MeasureString(scoreEntry.Item1 + "          " + scoreEntry.Item2.ToString());
                    textPosition = new Vector2((viewportWidth / 2) - (stringSize.X) + 100, (225 + (counter * 50)) * menuScaleFactor);
                    spriteBatch.DrawString(arialFont, scoreEntry.Item1 + "          " + scoreEntry.Item2.ToString(), textPosition, colorLTGreen);
                }

            }

            if (LittleTiggy.bLeaderboardNetworkFailure)
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
            spriteBatch.Draw(menuRectangle, new Vector2(menuButtonOffset, 10 * menuScaleFactor), Color.White);

            Vector2 stringSize = font.MeasureString("Go Back");
            Vector2 textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 10 * menuScaleFactor);
            spriteBatch.DrawString(font, "Go Back", textPosition, colorLTRed);


            // Draw instructions
            stringSize = bigArialFont.MeasureString("- Get to the bottom of the map to win the level.");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 175 * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "- Get to the bottom of the map to win the level.", textPosition, colorLTGreen);

            stringSize = bigArialFont.MeasureString("- Avoid the enemies trying to tag you.");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 250 * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "- Avoid the enemies trying to tag you.", textPosition, colorLTGreen);

            stringSize = bigArialFont.MeasureString("- If you get the power up,");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 325 * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "- If you get the power up,", textPosition, colorLTGreen);

            stringSize = bigArialFont.MeasureString("you can tag the enemies temporarily.");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 400 * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "you can tag the enemies temporarily.", textPosition, colorLTGreen);

            stringSize = bigArialFont.MeasureString("- If you get tagged you will go back a level.");
            textPosition = new Vector2((viewportWidth / 2) - (stringSize.X / 2), 475 * menuScaleFactor);
            spriteBatch.DrawString(bigArialFont, "- If you get tagged you will go back a level.", textPosition, colorLTGreen);

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