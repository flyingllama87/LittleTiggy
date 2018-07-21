using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



namespace LittleTiggy
{

    public class PowerUp
    {
        static Texture2D itemSheetTexture;
        static Random randomNumber = new Random();

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

        public PowerUp(GraphicsDevice graphicsDevice, EnvironmentBlock[] walls)
        {
            if (itemSheetTexture == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/ItemSheet.png"))
                {
                    itemSheetTexture = Texture2D.FromStream(graphicsDevice, stream);
                }
            }

            Idle = new Animation();
            Idle.AddFrame(new Rectangle(0, 0, 16, 16), TimeSpan.FromSeconds(.25));

            currentAnimation = Idle;

            // Place this object in a random, grid aligned location on the map & ensure it's not colliding with the environment / a wall.

            do 
            {
                this.X = (float)randomNumber.Next(0, GameConstants.gameWidth);
                this.Y = randomNumber.Next(0, GameConstants.gameHeight);

                this.X -= this.X % GameConstants.tileSize;
                this.Y -= this.Y % GameConstants.tileSize;

            } while (IsEnvironmentCollision(walls));

        }

        public void Update(GameTime gameTime, GraphicsDevice graphicsDevice)
        {

            if (IsPlayerCollision())
            {
                MainCharacter.isPoweredUp = true;
                LittleTiggy.powerUpSound.Play();
                MainCharacter.changeSkin();
                MainCharacter.powerUpTimer = DateTime.Now.AddSeconds(GameConstants.powerUpTimeSeconds);
                this.X = -16;
                this.Y = -16;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 topLeftOfSprite = new Vector2(this.X, this.Y);
            Color tintColor = Color.White;
            var sourceRectangle = currentAnimation.CurrentRectangle;

            spriteBatch.Draw(itemSheetTexture, topLeftOfSprite, sourceRectangle, tintColor);

        }

        bool IsEnvironmentCollision(EnvironmentBlock[] walls)
        {
            for (int i = 0; i < walls.Length; i++)
            {
                Rectangle wall = new Rectangle((int)walls[i].X + 1, (int)walls[i].Y + 1, 14, 14);
                Rectangle character = new Rectangle((int)this.X, (int)this.Y, GameConstants.characterWidth, GameConstants.characterHeight);

                if (character.Intersects(wall))
                {
                    return true;
                }

            }

            return false;

        }

        bool IsPlayerCollision()
        {
            Rectangle character = new Rectangle((int)MainCharacter.X, (int)MainCharacter.Y, 16, 16);
            Rectangle powerUp = new Rectangle((int)this.X, (int)this.Y, 16, 16);

            if (character.Intersects(powerUp))
                return true;

            return false;
        }

        bool IsEnvironmentCollision(EnvironmentBlock[] walls, Vector2 characterVector)
        {
            for (int i = 0; i < walls.Length; i++)
            {
                Rectangle wall = new Rectangle((int)walls[i].X + 1, (int)walls[i].Y + 1, 14, 14);
                Rectangle character = new Rectangle((int)characterVector.X, (int)characterVector.Y, 16, 16);

                if (character.Intersects(wall))
                {
                    return true;
                }

            }

            return false;

        }
    }
}