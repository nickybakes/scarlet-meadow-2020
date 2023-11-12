using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//3/27/2020
//Scarlet Meadow monogame project
//Civilian's are NPC's that stand still and are "rescued"/"saved" by the player when the player touches them
//Used for the "Save the Civilians" stage objective
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// Civilian's are NPC's that stand still and are "rescued"/"saved" by the player when the player touches them
    /// Used for the "Save the Civilians" stage objective
    /// </summary>
    class Civilian : GameObject
    {
        /// <summary>
        /// time left until the speech bubble that displays over the civilians head blinks
        /// </summary>
        private double speechBubbleBlinkTimer;

        /// <summary>
        /// When true, show the speech bubble. Used to make it flash slowly
        /// </summary>
        private bool speechBubbleShown;

        /// <summary>
        /// Constructor simply stores position and texture
        /// </summary>
        /// <param name="x">topleft X position</param>
        /// <param name="y">topLeft Y position</param>
        /// <param name="texture">texture for this civilian (male or female)</param>
        /// <param name="currentStage">the stage they are being added to</param>
        public Civilian(int x, int y, Texture2D texture, Stage currentStage) 
            : base(x,y, Game1.Scale * 3, Game1.Scale * 4, texture, 1.5f, currentStage)
        {
            speechBubbleShown = true;
            speechBubbleBlinkTimer = 2;
        }

        /// <summary>
        /// When a civilian is saved, increment the saved civs count in the stage,
        /// drop some items, and remove the civ from the stage
        /// </summary>
        public void SaveCivilian()
        {
            //drop big badge bit items
            for (int i = 0; i < 2; i++)
            {
                currentStage.AddEntity(new Collectible(X, Y, Game1.Scale, 2 * Game1.Scale, Game1.MiscTextures[12], currentStage,
                    ItemType.BadgeBitBig, true, new Vector2(Game1.RNG.Next(-18, 18), Game1.RNG.Next(-18, -5))));
            }

            //drop small badge bit items
            for (int i = 0; i < 5; i++)
            {
                currentStage.AddEntity(new Collectible(X, Y, Game1.Scale, Game1.Scale, Game1.MiscTextures[13], currentStage,
                ItemType.BadgeBitSmall, true, new Vector2(Game1.RNG.Next(-18, 18), Game1.RNG.Next(-18, -5))));
            }
            currentStage.DoImpactTime();
            currentStage.RemoveEntity(this);
        }



        /// <summary>
        /// Player collides with the civilian entity, "save" the civilian!
        /// </summary>
        /// <param name="other">the object that collided with this one</param>
        public override void CollideBottom(GameObject other)
        {
            base.CollideBottom(other);
            if (other is Player)
                SaveCivilian();
        }

        /// <summary>
        /// Player collides with the civilian entity, "save" the civilian!
        /// </summary>
        /// <param name="other">the object that collided with this one</param>
        public override void CollideLeft(GameObject other)
        {
            base.CollideLeft(other);
            if (other is Player)
                SaveCivilian();
        }

        /// <summary>
        /// Player collides with the civilian entity, "save" the civilian!
        /// </summary>
        /// <param name="other">the object that collided with this one</param>
        public override void CollideRight(GameObject other)
        {
            base.CollideRight(other);
            if (other is Player)
                SaveCivilian();
        }

        /// <summary>
        /// Player collides with the civilian entity, "save" the civilian!
        /// </summary>
        /// <param name="other">the object that collided with this one</param>
        public override void CollideTop(GameObject other)
        {
            base.CollideTop(other);
            if (other is Player)
                SaveCivilian();
        }

        /// <summary>
        /// Counts down on the speech bubble blink timer and alternates between showing and not showing it
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public override void Update(GameTime gameTime)
        {
            //count down on the speech bubble blink timer
            if(speechBubbleBlinkTimer > 0)
            {
                speechBubbleBlinkTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                //alternate between showing and not showing the speech bubble
                speechBubbleShown = !speechBubbleShown;

                //also make the character flip for a bit, makes them feel a bit more alive
                facingLeft = !facingLeft;

                //if we are showing the speech bubble, show it for 2 seconds
                if (speechBubbleShown)
                    speechBubbleBlinkTimer = 2;
                //if we are not showing the speech bubble, only do that for .8 seconds
                else
                    speechBubbleBlinkTimer = .7;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Draw the normal civilian texture but also draw a speech bubble on top of them saying "HELP!"
        /// </summary>
        /// <param name="sb">the sprite batch to draw to</param>
        /// <param name="cameraOffset">the camera offset in the world</param>
        public override void Draw(SpriteBatch sb, Vector2 cameraOffset)
        {
            //if we are showing the speech bubble (as determined by the blink timer in Update method), then...
            if(speechBubbleShown)
            {
                //...draw the speech bubble saying "HELP!"
                Rectangle speechBubbleRectangle = new Rectangle(transform.X - (int)cameraOffset.X - 4 * Game1.Scale, transform.Y - (int)cameraOffset.Y - 5 * Game1.Scale, 9 * Game1.Scale, 5 * Game1.Scale);
                sb.Draw(Game1.MiscTextures[23], speechBubbleRectangle, Color.White);
            }

            //then draw the normal civilian texture
            base.Draw(sb, cameraOffset);
        }
    }
}
