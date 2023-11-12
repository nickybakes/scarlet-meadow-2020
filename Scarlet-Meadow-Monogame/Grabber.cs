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
//The Grabber enemy (aka the Lawmaker enemy) grabs the player when they get close, and throws them!
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// The Grabber enemy (aka the Lawmaker enemy) grabs the player when they get close, and throws them!
    /// </summary>
    class Grabber : Enemy
    {
        /// <summary>
        /// The Grabber's specfic animations for attacking
        /// </summary>
        private Animation[] attackAnimations;

        /// <summary>
        /// the horizontal range that the grabber can see
        /// </summary>
        private const int visionRange = 560;

        /// <summary>
        /// the default walk speed of the grabber
        /// </summary>
        private const int walkSpeed = 5;

        /// <summary>
        /// Constructor for the Grabber (Lawmaker) enemy. Sets its health, damage, and other stat values
        /// </summary>
        /// <param name="x">starting x position</param>
        /// <param name="y">starting y position</param>
        /// <param name="animations">the set of animations for this enemy</param>
        /// <param name="attackAnimations">the set of specific attack animations for the grabber</param>
        /// <param name="currentStage">the stage this enemy is added to</param>
        /// <param name="facingLeft">whether the enemy should be facing left at the start or not</param>
        public Grabber(int x, int y, Animation[] animations, Animation[] attackAnimations, Stage currentStage, bool facingLeft) : 
            base(x, y, 3*Game1.Scale, 7 * Game1.Scale, animations, currentStage, facingLeft)
        {
            //set default values for the gabber enemy
            this.attackAnimations = attackAnimations;
            this.healthMax = 50;
            this.health = this.healthMax;
            this.damageOutput = 25;
            this.idleCoolDownTimerMax = .5;
            this.patrolCoolDownTimerMax = .3;
            this.attackCoolDownTimerMax = .6;
            this.bigBitsToDrop = 4;
            this.smallBitsToDrop = 3;
        }

        /// <summary>
        /// For the Grabber enemy, they walk up to the player, grab them, then throw them!
        /// This method controls that depending on what state the enemy is in
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public override void Update(GameTime gameTime)
        {
            //make the vision hit box to the sides of this enemy
            visionHitbox = new Rectangle(transform.Center.X - visionRange, transform.Y, visionRange*2, transform.Height);

            //update this enemy depending on what state they are in
            switch (currentState)
            {
                case (EnemyState.Patrol):
                    //checking if the enemy is about to walk off the platform they are on:
                    if (CheckForPlatformEdge())
                    {
                        facingLeft = !facingLeft;
                    }

                    //enemy walks in whatever direction that are facing
                    if (facingLeft)
                        VelocityX = -walkSpeed;
                    else
                        VelocityX = walkSpeed;

                    break;



                case (EnemyState.Enraged):
                    //if the player is far enough away from the enemy, make this enemy walk towards the player
                    if (Math.Abs(currentStage.MainPlayer.Transform.Center.X - transform.Center.X) > Game1.Scale)
                    {
                        //make the enemy face in the direction of the player
                        if (currentStage.MainPlayer.Transform.Center.X > transform.Center.X)
                            facingLeft = false;
                        else
                            facingLeft = true;

                        //enemy walks in whatever direction that are facing
                        if (facingLeft)
                            VelocityX = -walkSpeed - 2;
                        else
                            VelocityX = walkSpeed + 2;
                    }

                    //if the enemy is about to walk off the edge, reset the enemy to the patrol state
                    if (CheckForPlatformEdge())
                    {
                        currentState = EnemyState.Patrol;
                        patrolCoolDownTimer = 0;
                    }

                    break;

                case (EnemyState.Attack):
                    //if the grabber is facing left
                    if (facingLeft)
                    {
                        //position the player to the left!
                        currentStage.MainPlayer.PositionVector = new Vector2(transform.Left, transform.Y);
                    }
                    //if its facing right...
                    else
                    {
                        //...then position the player to the right!
                        currentStage.MainPlayer.PositionVector = new Vector2(transform.Right, transform.Y);
                    }
                    break;

            }

            //update the base enemy class
            base.Update(gameTime);

        }

        /// <summary>
        /// the grabber enemy has specific attack animations for each character the player can play as
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public override void UpdateAnimations(GameTime gameTime)
        {
            //the grabber enemy has specific attack animations for each character, so update those if its in the attack state
            if (currentState == EnemyState.Attack)
            {
                if (currentStage.MainPlayer is Scarlet)
                    attackAnimations[0].Update(gameTime);
                else if (currentStage.MainPlayer is Cocoa)
                    attackAnimations[1].Update(gameTime);
            }
            //other than that, just update the regular animations
            else
            {
                base.UpdateAnimations(gameTime);
            }
        }

        /// <summary>
        /// The grabber needs to draw a specific attack animation depending on what character the player is playing as
        /// </summary>
        /// <param name="sb">the spritebatch to draw to</param>
        /// <param name="cameraOffset">the camera's position/offset in the world</param>
        public override void Draw(SpriteBatch sb, Vector2 cameraOffset)
        {

            //determine if the sprite should be drawn or if its invisible due to the invinsibility blinking
            bool drawAnimation = true;
            if (invincibilityTimer < invincibilityTimerMax)
            {
                if (invincibilityBlinkTimer >= invincibilityBlinkTimerMax)
                {
                    invincibilityBlinkTimer = 0;
                    drawAnimation = false;
                }
            }

            //if we are drawing the sprite (and not blinking in this frame)
            if (drawAnimation)
            {
                //and if we are attacking...
                if (currentState == EnemyState.Attack)
                {
                    //and if the player is playing as scarlet
                    if (currentStage.MainPlayer is Scarlet)
                    {
                        //then draw the attack scarlet animation
                        attackAnimations[0].Draw(sb, transform, cameraOffset, Color.White, facingLeft);
                    }
                    //and if the player is playing as cocoa
                    else if (currentStage.MainPlayer is Cocoa)
                    {
                        //then draw the attack cocoa animation
                        attackAnimations[1].Draw(sb, transform, cameraOffset, Color.White, facingLeft);
                    }
                }
                //or just draw the regular animations if we aren't attacking
                else
                {
                    base.Draw(sb, cameraOffset);
                }
            }
        }

        /// <summary>
        /// The grabber enemy grabs and throws the player
        /// </summary>
        public override void DoAttack()
        {
            //if the grabber is facing left
            if (facingLeft)
            {
                //then throw the player to the left!
                currentStage.MainPlayer.PositionVector = new Vector2(transform.Left, transform.Y);
                currentStage.MainPlayer.Knockback(new Vector2(-25, -25));
            }
            //if its facing right...
            else
            {
                //...then throw the player to the right!
                currentStage.MainPlayer.PositionVector = new Vector2(transform.Right, transform.Y);
                currentStage.MainPlayer.Knockback(new Vector2(25, -25));
            }

            //inflict damage to the player
            currentStage.MainPlayer.Damage(damageOutput);

            //reset the specific attack animation depending on what character the player is
            if (currentStage.MainPlayer is Scarlet)
            {
                attackAnimations[0].Reset();
            }
            else if (currentStage.MainPlayer is Cocoa)
            {
                attackAnimations[1].Reset();
            }
        }


        /// <summary>
        /// The grabber cannot be attacked or lassoed if they are already attacking/grabbing the player
        /// </summary>
        /// <returns></returns>
        public override bool CanBeAttacked()
        {
            if(currentState == EnemyState.Attack)
            {
                return false;
            }
            return base.CanBeAttacked();
        }

        /// <summary>
        /// When the grabber hits the right side of a wall, turn them around, when they hit the player, make them attack!
        /// </summary>
        /// <param name="other">the object this enemy collided with</param>
        public override void CollideRight(GameObject other)
        {
            base.CollideRight(other);

            switch (currentState)
            {
                //if they are patrolling and hit a wall, turn them around
                case (EnemyState.Patrol):
                    if (other.TypeOfCollision == CollisionType.Solid)
                    {
                        facingLeft = false;
                    }
                    break;

                case (EnemyState.Enraged):
                    //if this enemy is enraged and they hit the player
                    if(other is Player)
                    {
                        //and if the player isn't invincible or already being grabbed
                        if (!currentStage.MainPlayer.Invincible && currentStage.MainPlayer.State != PlayerState.Restrained)
                        {
                            //then grab the player!
                            currentStage.MainPlayer.State = PlayerState.Restrained;
                            currentState = EnemyState.Attack;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// When the grabber hits the left side of a wall, turn them around, when they hit the player, make them attack!
        /// </summary>
        /// <param name="other">the object this enemy collided with</param>
        public override void CollideLeft(GameObject other)
        {
            base.CollideLeft(other);

            switch (currentState)
            {
                //if they are patrolling and hit a wall, turn them around
                case (EnemyState.Patrol):
                    if (other.TypeOfCollision == CollisionType.Solid)
                    {
                        facingLeft = true;
                    }
                    break;


                case (EnemyState.Enraged):
                    //if this enemy is enraged and they hit the player
                    if (other is Player)
                    {
                        //and if the player isn't invincible or already being grabbed
                        if (!currentStage.MainPlayer.Invincible && currentStage.MainPlayer.State != PlayerState.Restrained)
                        {
                            //then grab the player!
                            currentStage.MainPlayer.State = PlayerState.Restrained;
                            currentState = EnemyState.Attack;
                        }
                    }
                    break;
            }
        }
    }
}
