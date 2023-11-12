using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//3/22/2020
//Scarlet Meadow monogame project
//The Cactus enemy simple walks around on its platform. If the player walks into the cactus, it damages the player
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// The Cactus enemy simple walks around on its platform. If the player walks into the cactus, it damages the player
    /// </summary>
    class Cactus : Enemy
    {
        /// <summary>
        /// the horizontal range that the Cactus can see
        /// </summary>
        private const int visionRange = 560;

        /// <summary>
        /// the default walk speed of the Cactus
        /// </summary>
        private const int walkSpeed = 3;

        /// <summary>
        /// Constructor for the Cactus enemy. Sets its health, damage, and other stat values
        /// </summary>
        /// <param name="x">starting x position</param>
        /// <param name="y">starting y position</param>
        /// <param name="animations">the set of animations for this enemy</param>
        /// <param name="currentStage">the stage this enemy is added to</param>
        /// <param name="facingLeft">whether the enemy should be facing left at the start or not</param>
        public Cactus(int x, int y, Animation[] animations, Stage currentStage, bool facingLeft) :
            base(x, y, 3 * Game1.Scale, 4 * Game1.Scale, animations, currentStage, facingLeft)
        {
            //set default values for the cactus enemy
            this.healthMax = 25;
            this.health = this.healthMax;
            this.damageOutput = 15;
            this.idleCoolDownTimerMax = 0;
            this.patrolCoolDownTimerMax = .25;
            this.attackCoolDownTimerMax = 0;
            this.bigBitsToDrop = 1;
            this.smallBitsToDrop = 6;
        }

        /// <summary>
        /// For the Cactus enemy, they walk up to the player, grab them, then throw them!
        /// This method controls that depending on what state the enemy is in
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public override void Update(GameTime gameTime)
        {
            //make the vision hit box to the sides of this enemy
            visionHitbox = new Rectangle(transform.Center.X - visionRange, transform.Y, visionRange * 2, transform.Height);

            //update this enemy depending on what state they are in
            switch (currentState)
            {
                case (EnemyState.Patrol):
                case (EnemyState.Enraged):
                    //checking if the enemy is about to walk off the platform they are on:
                    if (CheckForPlatformEdge())
                    {
                        //if so, flip them around
                        facingLeft = !facingLeft;
                    }

                    //enemy walks in whatever direction that are facing
                    if (facingLeft)
                        VelocityX = -walkSpeed;
                    else
                        VelocityX = walkSpeed;

                    break;
            }

            //update the base enemy class
            base.Update(gameTime);

        }

        /// <summary>
        /// The Cactus just knocks the player back and does some damage
        /// </summary>
        public override void DoAttack()
        {
            //if the player is on the left side...
            if (currentStage.MainPlayer.Transform.Center.X < transform.Center.X)
            {
                //hit the player and turn the cactus around
                facingLeft = false;
                currentStage.MainPlayer.Knockback(new Vector2(-20, -23));
            }
            //if the player is on the right side...
            else
            {
                //hit the player and turn the cactus around
                facingLeft = true;
                currentStage.MainPlayer.Knockback(new Vector2(20, -23));
            }

            //inflict damage to the player
            currentStage.MainPlayer.Damage(damageOutput);
        }

        /// <summary>
        /// When the Cactus hits the right side of a wall, turn them around, when they hit the player, make them attack!
        /// </summary>
        /// <param name="other">the object this enemy collided with</param>
        public override void CollideRight(GameObject other)
        {
            base.CollideRight(other);

            if (other.TypeOfCollision == CollisionType.Solid)
            {
                facingLeft = false;
            }

            if (other is Player && currentState == EnemyState.Enraged)
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
        /// When the Cactus hits the left side of a wall, turn them around, when they hit the player, make them attack!
        /// </summary>
        /// <param name="other">the object this enemy collided with</param>
        public override void CollideLeft(GameObject other)
        {
            base.CollideLeft(other);

            if (other.TypeOfCollision == CollisionType.Solid)
            {
                facingLeft = true;
            }

            if (other is Player && currentState == EnemyState.Enraged)
            {
                //and if the player isn't invincible or already being grabbed
                if (!currentStage.MainPlayer.Invincible && currentStage.MainPlayer.State != PlayerState.Restrained)
                {
                    //then grab the player!
                    currentState = EnemyState.Attack;
                }
            }
        }
    }
}
