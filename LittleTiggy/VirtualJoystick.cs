using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;



namespace LittleTiggy
{

    public class VirtualJoystick
    {

        Texture2D virtualJoystickTexture;
        Boolean bWasTouchPresentLastUpdate = false;
        public static Vector2 virtualJoystickPosition;

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

        public VirtualJoystick(GraphicsDevice graphicsDevice)
        {
            if (virtualJoystickTexture == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/JoystickCircle.png"))
                {
                    virtualJoystickTexture = Texture2D.FromStream(graphicsDevice, stream);
                }
            }

            virtualJoystickPosition.X = LittleTiggy.viewportWidth / 2;
            virtualJoystickPosition.Y = LittleTiggy.viewportHeight / 2;

        }

        public void Update()
        {

            TouchCollection touchCollection = TouchPanel.GetState();

            if (touchCollection.Count > 0) // If the player is using a touch screen and has touched the screen.
            {
                // If we detect a new touch, set the touch position as the middle of the virtual joystick.

                if (bWasTouchPresentLastUpdate == false)
                    virtualJoystickPosition = new Vector2(touchCollection[0].Position.X / LittleTiggy.gameScaleFactor, touchCollection[0].Position.Y / LittleTiggy.gameScaleFactor);

                bWasTouchPresentLastUpdate = true;

            }
            else
            {
                bWasTouchPresentLastUpdate = false;
            }


        }

        public void Draw(SpriteBatch spriteBatch)
        {

            if (virtualJoystickPosition != null)
            {
                Rectangle joystickRectangle = new Rectangle(0, 0, 511, 511);
                
                // virtualJoystickPosition.X = (GameConstants.gameWidth / 2) - (virtualJoystickTexture.Width / 2);
                // virtualJoystickPosition.Y = (GameConstants.gameHeight / 2) - (virtualJoystickTexture.Height / 2);

                // spriteBatch.Draw(virtualJoystickTexture, new Vector2(virtualJoystickPosition.X, virtualJoystickPosition.Y), joystickRectangle, Color.White * 0.1f);
                spriteBatch.Draw(virtualJoystickTexture, new Vector2(0, 0), joystickRectangle, Color.White * 0.1f);
            }

        }

    }
}