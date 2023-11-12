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
//The Robber Baron enemy shoots projectiles toward the player!
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// The Robber Baron enemy shoots projectiles toward the player!
    /// </summary>
    class RobberBaron : Enemy
    {
        /// <summary>
        /// the horizontal range that the robber baron can see
        /// </summary>
        private const int visionRangeHorizontal = 2000;

        /// <summary>
        /// the vertical range that the robber baron can see
        /// </summary>
        private const int visionRangeVertical = 1000;

        /// <summary>
        /// how much time untill the robber baron fires his gun while enraged
        /// </summary>
        private double attackChargeTimer;

        /// <summary>
        /// The maximum time until the robber baron fires his gun
        /// </summary>
        private const double attackChargeTimerMax = .7;


        /// <summary>
        /// Sets robber baron stats to correct values
        /// </summary>
        /// <param name="x">topleft x position</param>
        /// <param name="y">top left y position</param>
        /// <param name="animations">set of animations used for robber barron enemy</param>
        /// <param name="currentStage">stage its being added to</param>
        /// <param name="facingLeft">starting direction</param>
        public RobberBaron(int x, int y, Animation[] animations, Stage currentStage, bool facingLeft) :
                base(x, y, 3 * Game1.Scale, 6 * Game1.Scale, animations, currentStage, facingLeft)
        {
            //set default values for the robber baron enemy
            this.healthMax = 30;
            this.health = this.healthMax;
            this.damageOutput = 20;
            this.idleCoolDownTimerMax = 1.2;
            this.patrolCoolDownTimerMax = 1.2;
            this.attackCoolDownTimerMax = 0;
            this.bigBitsToDrop = 3;
            this.smallBitsToDrop = 2;
        }

        /// <summary>
        /// The Robber baron should shoot linear projectiles depending on what direction its facing
        /// </summary>
        public override void DoAttack()
        {
            if (facingLeft)
                currentStage.AddEntity(new Projectile(X - 3 * Game1.Scale, Y + 3 * Game1.Scale, 3 * Game1.Scale, 2 * Game1.Scale, Game1.MiscTextures[6], currentStage, new Vector2(-11, 0), damageOutput, ProjectileType.Linear, false, 3));
            else
                currentStage.AddEntity(new Projectile(transform.Right, Y + 3 * Game1.Scale, 3 * Game1.Scale, 2 * Game1.Scale, Game1.MiscTextures[6], currentStage, new Vector2(11, 0), damageOutput, ProjectileType.Linear, false, 3));
        }


        /// <summary>
        /// The robber baron waits til the player is in its sights, then it charges up a shot!
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            //make the vision hit box to the sides of this enemy
            visionHitbox = new Rectangle(transform.Center.X - visionRangeHorizontal, transform.Center.Y - visionRangeVertical, visionRangeHorizontal * 2, visionRangeVertical * 2);


            switch (currentState)
            {
                case (EnemyState.Patrol):
                    //make the enemy face in the direction of the player
                    if (currentStage.MainPlayer.Transform.Center.X > transform.Center.X)
                        facingLeft = false;
                    else
                        facingLeft = true;


                    break;

                case (EnemyState.Enraged):
                    //charge up the projectile
                    if(attackChargeTimer < attackChargeTimerMax)
                    {
                        attackChargeTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        //then shoot it!
                        attackChargeTimer = 0;
                        currentState = EnemyState.Attack;
                    }

                    break;

            }

            base.Update(gameTime);

        }


        /// <summary>
        /// When the player touches this guy, hurt the player
        /// </summary>
        /// <param name="other">the object this enemy collided with</param>
        public override void CollideRight(GameObject other)
        {
            base.CollideRight(other);

            if (other is Player)
            {
                //and if the player isn't invincible or already being grabbed
                if ((currentState == EnemyState.Idle || currentState == EnemyState.Enraged || currentState == EnemyState.Attack) && !currentStage.MainPlayer.Invincible && currentStage.MainPlayer.State != PlayerState.Restrained)
                {
                    currentStage.MainPlayer.Knockback(new Vector2(25, -25));
                    currentStage.MainPlayer.Damage(damageOutput);
                }
            }
        }

        /// <summary>
        /// When the player touches this guy, hurt the player
        /// </summary>
        /// <param name="other">the object this enemy collided with</param>
        public override void CollideLeft(GameObject other)
        {
            base.CollideLeft(other);

            if (other is Player)
            {
                //and if the player isn't invincible or already being grabbed
                if ((currentState == EnemyState.Idle || currentState == EnemyState.Enraged || currentState == EnemyState.Attack) && !currentStage.MainPlayer.Invincible && currentStage.MainPlayer.State != PlayerState.Restrained)
                {
                    currentStage.MainPlayer.Knockback(new Vector2(-25, -25));
                    currentStage.MainPlayer.Damage(damageOutput);
                }
            }
        }
    }
}
