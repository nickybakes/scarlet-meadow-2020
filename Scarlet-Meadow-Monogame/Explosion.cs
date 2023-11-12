using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//3/23/2020
//Scarlet Meadow monogame project
//An explosion has an area of effect where anything that touches it will take damage and get knocked back.
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// An explosion has an area of effect where anything that touches it will take damage and get knocked back.
    /// </summary>
    class Explosion : GameObject
    {

        /// <summary>
        /// Used to determine whether to damage enemies or player
        /// </summary>
        private bool madeByPlayer;

        /// <summary>
        /// How much damage this explosion should do to an object that it touches
        /// </summary>
        private int damageOutput;

        /// <summary>
        /// How much time until this explosion disappears
        /// </summary>
        private double timeLeftActive;

        /// <summary>
        /// How much time is left untill the sprite blinks
        /// </summary>
        private double blinkTimer;

        /// <summary>
        /// Maximum amount of time until this sprite blinks
        /// </summary>
        private const double blinkTimerMax = .1;

        /// <summary>
        /// Constructor simply sets data for the explosion
        /// </summary>
        /// <param name="centerX">Center X coord of where to spawn this projectile</param>
        /// <param name="centerY">Center Y coord of where to spawn this projectile</param>
        /// <param name="madeByPlayer">True if a player's projectile or attack made this explosion, false if an enemy made it</param>
        /// <param name="damageOutput">How much damage this explosion should do to objects that touch it</param>
        /// <param name="currentStage">the stage its being added to</param>
        public Explosion(int centerX, int centerY, bool madeByPlayer, int damageOutput, Stage currentStage) : 
            base(centerX - (int)(4*Game1.Scale), centerY - (int)(4 * Game1.Scale), 8*Game1.Scale, 8*Game1.Scale, Game1.MiscTextures[5], 0, currentStage)
        {
            collisionType = CollisionType.StaticEntity;
            this.madeByPlayer = madeByPlayer;
            this.damageOutput = damageOutput;
            this.timeLeftActive = 1;
        }

        /// <summary>
        /// Update simply counts down on active timer and blink timer. When active timer reaches 0, remove it from the stage
        /// </summary>
        /// <param name="gameTime">time elapsed since last frame</param>
        public override void Update(GameTime gameTime)
        {
            blinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
            timeLeftActive -= gameTime.ElapsedGameTime.TotalSeconds;

            if(timeLeftActive <= 0)
            {
                currentStage.RemoveEntity(this);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the sprite if not blinking, but if blink timer hits the max, then make the sprite blink
        /// </summary>
        /// <param name="sb">spritebatch to draw to</param>
        /// <param name="cameraOffset">camera's offset in the world</param>
        public override void Draw(SpriteBatch sb, Vector2 cameraOffset)
        {
            if(blinkTimer >= blinkTimerMax)
            {
                blinkTimer = 0;
            }
            else
            {
                base.Draw(sb, cameraOffset);
            }
        }

        /// <summary>
        /// Colliding with top or bottom just refers to Colliding with left or right
        /// </summary>
        /// <param name="other">object that collided with this one</param>
        public override void CollideTop(GameObject other)
        {
            //if the object is on the explosions left side, then its right side must be touching this left side
            if(other.X < transform.Center.X)
                CollideRight(other);
            //if the object is on the explosions right side, then its left side must be touching this right side
            else
                CollideLeft(other);
        }

        /// <summary>
        /// Colliding with top or bottom just refers to Colliding with left or right
        /// </summary>
        /// <param name="other">object that collided with this one</param>
        public override void CollideBottom(GameObject other)
        {
            //if the object is on the explosions left side, then its right side must be touching this left side
            if (other.X < transform.Center.X)
                CollideRight(other);
            //if the object is on the explosions right side, then its left side must be touching this right side
            else
                CollideLeft(other);
        }

        /// <summary>
        /// When an object touches this explosions's left side, knockback the object to the left
        /// </summary>
        /// <param name="other">the object touching this explosion</param>
        public override void CollideRight(GameObject other)
        {
            //if the player made this explosion
            if (madeByPlayer)
            {
                //then do damage to ENEMIES that touch it
                if (other is Enemy && ((Enemy)other).CanBeAttacked())
                {
                    other.Y -= 3;
                    ((Enemy)other).DamageKnockback(damageOutput, new Vector2(Game1.RNG.Next(-20, -15), Game1.RNG.Next(-20, -15)));
                }
            }
            //if the enemy made this
            else
            {
                //the do damage to PLAYERS that touch it
                if (other is Player && !((Player)other).Invincible)
                {
                    other.Y -= 3;
                    ((Player)other).Knockback(new Vector2(Game1.RNG.Next(-22, -16), Game1.RNG.Next(-29, -23)));
                    ((Player)other).Damage(damageOutput);
                }
            }
        }

        /// <summary>
        /// When an object touches this explosions's right side, knockback the object to the right
        /// </summary>
        /// <param name="other">the object touching this explosion</param>
        public override void CollideLeft(GameObject other)
        {
            //if the player made this explosion
            if (madeByPlayer)
            {
                //then do damage to ENEMIES that touch it
                if (other is Enemy && ((Enemy)other).CanBeAttacked())
                {
                    other.Y -= 3;
                    ((Enemy)other).DamageKnockback(damageOutput, new Vector2(Game1.RNG.Next(15, 20), Game1.RNG.Next(-20, -15)));
                }
            }
            //if the enemy made this
            else
            {
                //the do damage to PLAYERS that touch it
                if (other is Player && !((Player)other).Invincible)
                {
                    other.Y -= 3;
                    ((Player)other).Knockback(new Vector2(Game1.RNG.Next(16, 22), Game1.RNG.Next(-29, -23)));
                    ((Player)other).Damage(damageOutput);
                }
            }
        }

    }
}
