using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/28/2020
//Scarlet Meadow monogame project
//The tumble weed enemy bounces around and knocks the player back when it touches the player
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// The tumble weed enemy bounces around and knocks the player back when it touches the player
    /// </summary>
    class TumbleWeed : Enemy
    {
        /// <summary>
        /// the horizontal range that the TumbleWeed can see
        /// </summary>
        private const int visionRange = 1400;

        /// <summary>
        /// the constant horizontal speed of the tumble weed
        /// </summary>
        private const int movementSpeedX = 6;

        /// <summary>
        /// Constructor just stores data. that is all
        /// </summary>
        /// <param name="x">topleft x position</param>
        /// <param name="y">top left y position</param>
        /// <param name="animations">set of animations used for tumble weed enemy</param>
        /// <param name="currentStage">stage its being added to</param>
        /// <param name="facingLeft">starting direction</param>
        public TumbleWeed(int x, int y, Animation[] animations, Stage currentStage, bool facingLeft) :
            base(x, y, 3 * Game1.Scale, 3 * Game1.Scale, animations, currentStage, facingLeft)
        {
            //set default values for the tumble weed enemy
            this.healthMax = 25;
            this.health = this.healthMax;
            this.damageOutput = 15;
            this.idleCoolDownTimerMax = 0;
            this.patrolCoolDownTimerMax = .2;
            this.attackCoolDownTimerMax = 0;
            this.bigBitsToDrop = 2;
            this.smallBitsToDrop = 4;
        }

        /// <summary>
        /// The tumble weed enemy simple bounces around, when it hits a wall or the player, it's direction flips
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            //make the vision hit box to the sides of this enemy
            visionHitbox = new Rectangle(transform.Center.X - visionRange, transform.Y, visionRange * 2, transform.Height);

            //update this enemy depending on what state they are in
            switch (currentState)
            {
                case (EnemyState.Patrol):
                case (EnemyState.Enraged):

                    //keep this velocity at a constant speed
                    if (facingLeft)
                        VelocityX = -movementSpeedX;
                    else
                        VelocityX = movementSpeedX;

                    break;
            }

            //update the base enemy class
            base.Update(gameTime);

        }

        /// <summary>
        /// The tumble weed just knocks the player back and does some damage
        /// </summary>
        public override void DoAttack()
        {
            //if the player is on the left side...
            if (currentStage.MainPlayer.Transform.Center.X < transform.Center.X)
            {
                //hit the player and turn the tumble weed around
                facingLeft = false;
                currentStage.MainPlayer.Knockback(new Vector2(-20, -23));
            }
            //if the player is on the right side...
            else
            {
                //hit the player and turn the tumble weed around
                facingLeft = true;
                currentStage.MainPlayer.Knockback(new Vector2(20, -23));
            }

            //inflict damage to the player
            currentStage.MainPlayer.Damage(damageOutput);
        }

        /// <summary>
        /// if it hits a wall, bounce the other away, if it hits the player, attack!
        /// </summary>
        /// <param name="other">object collided with</param>
        public override void CollideLeft(GameObject other)
        {
            base.CollideLeft(other);

            //hit a wall...
            if (other.TypeOfCollision == CollisionType.Solid)
            {
                //...turn around
                facingLeft = true;
            }

            //hits a player and is enraged
            else if (other is Player && currentState == EnemyState.Enraged)
            {
                //and if the player isn't invincible or already being grabbed
                if (!currentStage.MainPlayer.Invincible && currentStage.MainPlayer.State != PlayerState.Restrained)
                {
                    //then grab the player!
                    currentState = EnemyState.Attack;
                }
            }
        }

        /// <summary>
        /// if it hits a wall, bounce the other away, if it hits the player, attack!
        /// </summary>
        /// <param name="other">object collided with</param>
        public override void CollideRight(GameObject other)
        {
            base.CollideRight(other);

            //hit a wall...
            if (other.TypeOfCollision == CollisionType.Solid)
            {
                //...turn around
                facingLeft = false;
            }

            //hits a player and is enraged
            else if (other is Player && currentState == EnemyState.Enraged)
            {
                //and if the player isn't invincible or already being grabbed
                if (!currentStage.MainPlayer.Invincible && currentStage.MainPlayer.State != PlayerState.Restrained)
                {
                    //then grab the player!
                    currentState = EnemyState.Attack;
                }
            }
        }

        /// <summary>
        /// If it hits the player, attack!
        /// </summary>
        /// <param name="other">object collided with</param>
        public override void CollideBottom(GameObject other)
        {
            base.CollideBottom(other);

            if (other is Player)
            {
                //if the object is on the left side, then its right side must be touching
                if (other.X < transform.Center.X)
                    CollideRight(other);
                //if the object is on the right side, then its left side must be touching
                else
                    CollideLeft(other);
            }

        }

        /// <summary>
        /// if it hits the floor, bounce back upward!, if it hits the player, attack!
        /// </summary>
        /// <param name="other">object collided with</param>
        public override void CollideTop(GameObject other)
        {
            base.CollideTop(other);

            //hits the floor...
            if (other.TypeOfCollision == CollisionType.Solid || other.TypeOfCollision == CollisionType.SemiSolid)
            {
                //...bounce back upward
                this.VelocityY = -15;
            }
            else if(other is Player)
            {
                //if the object is on the left side, then its right side must be touching
                if (other.X < transform.Center.X)
                    CollideRight(other);
                //if the object is on the right side, then its left side must be touching
                else
                    CollideLeft(other);
            }
        }
    }
}
