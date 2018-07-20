using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;



namespace LittleTiggy
{

    public class ControlOverlay
    {

        Texture2D controlOverlayTexture;

        public ControlOverlay(GraphicsDevice graphicsDevice)
        {
            if (controlOverlayTexture == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/JoystickCircle.png"))
                {
                    controlOverlayTexture = Texture2D.FromStream(graphicsDevice, stream);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle overlayRectangle = new Rectangle(0, 0, 511, 511);
            spriteBatch.Draw(controlOverlayTexture, new Vector2(0, 0), overlayRectangle, Color.White * 0.5f);
        }
    }

    public class VirtualThumbstick
    {

        // the distance in screen pixels that represents a thumbstick value of 1f.
        private const float maxThumbstickDistance = 60f;

        // the current positions of the physical touches
        private static Vector2 Position;

        private static Texture2D thumbstickTexture;

        public static Vector2? virtualThumbstickCenter { get; private set; }

        public static Vector2 Thumbstick
        {
            get
            {
                if (!virtualThumbstickCenter.HasValue)
                    return Vector2.Zero;

                // calculate the scaled vector from the touch position to the center,
                // scaled by the maximum thumbstick distance
                Vector2 p = (Position - virtualThumbstickCenter.Value) / maxThumbstickDistance;

                // if the length is more than 1, normalize the vector
                if (p.LengthSquared() > 1f)
                    p.Normalize();

                return p;
            }
        }

        public VirtualThumbstick(GraphicsDevice graphicsDevice)
        {
            if (thumbstickTexture == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/Thumbstick.png"))
                {
                    thumbstickTexture = Texture2D.FromStream(graphicsDevice, stream);
                }
            }

            virtualThumbstickCenter = new Vector2(0,0);

        }


        public void Update()
        {
            // Set joystick centre depending on set game option.  'Tap Control' is really just a virtual joystick in the middle of the screen with a different overlay.

            if (LittleTiggy.gameTouchControlMethod == GameTouchControlMethod.Joystick)
                virtualThumbstickCenter = new Vector2(LittleTiggy.viewportWidth / 2f, LittleTiggy.viewportHeight - 200);
            else
                virtualThumbstickCenter = new Vector2(LittleTiggy.viewportWidth / 2f, LittleTiggy.viewportHeight / 2f);

            TouchLocation? Touch = null;
            TouchCollection touches = TouchPanel.GetState();

            foreach (var touch in touches)
            {
                Touch = touch;
            }

            // if we have a touch, save the position of the touch
            if (Touch.HasValue)
                Position = Touch.Value.Position;

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 pos = virtualThumbstickCenter.Value - new Vector2(thumbstickTexture.Width / 2f, thumbstickTexture.Height / 2f);

            spriteBatch.Draw(thumbstickTexture, virtualThumbstickCenter.Value - new Vector2(thumbstickTexture.Width / 2f, thumbstickTexture.Height / 2f), Color.Black);
        }

    }

}