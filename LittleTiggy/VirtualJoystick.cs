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

            spriteBatch.Draw(controlOverlayTexture, new Vector2(0, 0), overlayRectangle, Color.White * 0.1f);
        }
    }

    public class VirtualThumbstick
    {

        // the distance in screen pixels that represents a thumbstick value of 1f.
        private const float maxThumbstickDistance = 80f;

        // the current positions of the physical touches
        private static Vector2 Position;

        // the IDs of the touches we are tracking for the thumbstick
        private static int touchId = -1;

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
        }

        public void Update()
        {
            TouchLocation? Touch = null;
            TouchCollection touches = TouchPanel.GetState();

            foreach (var touch in touches)
            {
                if (touch.Id == touchId)
                {
                    // This is a motion of a touch that we're already tracking
                    Touch = touch;
                    continue;
                }

                TouchLocation earliestTouch;
                if (!touch.TryGetPreviousLocation(out earliestTouch))
                    earliestTouch = touch;

                if (touchId == -1)
                {
                    if (earliestTouch.Position.X < TouchPanel.DisplayWidth / 2)
                    {
                        Touch = earliestTouch;
                        continue;
                    }
                }
            }

            // if we have a touch
            if (Touch.HasValue)
            {
                // if we have no center, this position is our center
                if (!virtualThumbstickCenter.HasValue)
                    virtualThumbstickCenter = Touch.Value.Position;

                // save the position of the touch
                Position = Touch.Value.Position;

                // save the ID of the touch
                touchId = Touch.Value.Id;
            }
            else
            {
                // otherwise reset our values to not track any touches
                // for the thumbstick
                virtualThumbstickCenter = null;
                touchId = -1;
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (virtualThumbstickCenter.HasValue)
            {
                spriteBatch.Draw(
                    thumbstickTexture,
                    virtualThumbstickCenter.Value - new Vector2(thumbstickTexture.Width / 2f, thumbstickTexture.Height / 2f),
                    Color.Green);
            }
        }

    }

}