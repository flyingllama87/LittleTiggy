using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

namespace LittleTiggy
{
    public class EnvironmentBlock
    {
        static Texture2D environmentSheetTexture;
        Animation Idle;
        Animation currentAnimation;


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


        public EnvironmentBlock(GraphicsDevice graphicsDevice, int X, int Y)
        {
            if (environmentSheetTexture == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/EnvironmentSheet.png"))
                {
                    environmentSheetTexture = Texture2D.FromStream(graphicsDevice, stream);
                }
            }

            Idle = new Animation();
            Idle.AddFrame(new Rectangle(0, 0, 16, 16), TimeSpan.FromSeconds(.25));

            currentAnimation = Idle;

            this.X = X; this.Y = Y;

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 topLeftOfSprite = new Vector2(this.X, this.Y);
            Color tintColor = Color.White;
            var sourceRectangle = currentAnimation.CurrentRectangle;

            // spriteBatch.Draw(environmentSheetTexture, topLeftOfSprite, sourceRectangle, tintColor, 0f, Vector2.Zero, Game1.scaleFactor, SpriteEffects.None, 0f);
            spriteBatch.Draw(environmentSheetTexture, topLeftOfSprite, sourceRectangle, tintColor);

        }

    }
}