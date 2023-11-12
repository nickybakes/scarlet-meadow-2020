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
//A projectile can be either bouncy or move in a straight linear direction. They cause an explosion when touched!
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// Determines the movement type of this projectile, if it goes in a linear(straight) line or bounces
    /// </summary>
    public enum ProjectileType
    {
        Linear,
        Bouncy
    }

    /// <summary>
    /// A projectile can be either bouncy or move in a straight linear direction. They cause an explosion when touched!
    /// </summary>
    class Projectile : GameObject
    {
        /// <summary>
        /// The amount of damage the explosion made by this projectile shot
        /// </summary>
        private int damage;

        /// <summary>
        /// Determines the movement type of this projectile, if it goes in a linear(straight) line or bounces
        /// </summary>
        private ProjectileType projectileType;

        /// <summary>
        /// True if the player shot this projectile. 
        /// Used to determine whether to damage enemies or player
        /// </summary>
        private bool shotByPlayer;

        /// <summary>
        /// How much time is left before this projectile automatically explodes!
        /// </summary>
        private double timeTillExplode;

        /// <summary>
        /// the initial velocity of this projectile
        /// </summary>
        private Vector2 initialVelocity;

        /// <summary>
        /// When the projectile hits ground for the first time, set this to true. Used for bouncy projectiles.
        /// If they hit an enemy before hitting the ground, they immediately explode!
        /// </summary>
        private bool hitGround;

        /// <summary>
        /// Constructor simple gets and stores projectile specific data such as what type of projectile it is
        /// </summary>
        /// <param name="x">starting x position</param>
        /// <param name="y">starting y position</param>
        /// <param name="width">width of the hit box</param>
        /// <param name="height">height of the hit box</param>
        /// <param name="texture">the texture to display for this projectile</param>
        /// <param name="currentStage">the stage the projectile is being added to</param>
        /// <param name="initialVelocity">the initial velocity of this projectile</param>
        /// <param name="damage">The amount of damage the explosion made by this projectile shot</param>
        /// <param name="projectileType">Determines the movement type of this projectile, if it goes in a linear(straight) line or bounces</param>
        /// <param name="shotByPlayer">True if the player shot this projectile. 
        /// Used to determine whether to damage enemies or player</param>
        /// <param name="timeTillExplode">How much time is left before this projectile automatically explodes!</param>
        public Projectile(int x, int y, int width, int height, Texture2D texture, Stage currentStage, Vector2 initialVelocity, int damage, ProjectileType projectileType, bool shotByPlayer, double timeTillExplode) : 
            base(x, y, width, height, texture, 0, currentStage)
        {
            this.damage = damage;
            this.projectileType = projectileType;
            this.shotByPlayer = shotByPlayer;
            this.timeTillExplode = timeTillExplode;
            this.initialVelocity = initialVelocity;
            this.velocity = initialVelocity;
            switch (projectileType)
            {
                //linear projectiles are not affected by gravity
                case (ProjectileType.Linear):
                    this.gravity = 0;
                    break;

                //bouncy ones are, are however affected by gravity
                case (ProjectileType.Bouncy):
                    this.gravity = 1.5f;
                    break;
            }
        }

        /// <summary>
        /// Counts down on the explosion timer and moves the projectile, then does a standard collision check
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public override void Update(GameTime gameTime)
        {
            //Counts down on the explosion timer
            timeTillExplode -= gameTime.ElapsedGameTime.TotalSeconds;

            //if its time to explode, then do so!
            if(timeTillExplode <= 0)
                this.Explode();

            //linear projectiles have a constant velocity, bouncy ones have a constant X velocity
            switch (projectileType)
            {
                case (ProjectileType.Linear):
                    velocity = initialVelocity;
                    break;

                case (ProjectileType.Bouncy):
                    velocity.X = initialVelocity.X;
                    break;
            }

            if (VelocityX > 0)
                facingLeft = false;
            else if (VelocityX < 0)
                facingLeft = true;

            base.Update(gameTime);
        }

        /// <summary>
        /// When its time to explode, call this method!
        /// </summary>
        public void Explode()
        {
            currentStage.AddEntity(new Explosion(transform.Center.X, transform.Center.Y, shotByPlayer, damage, currentStage));
            currentStage.RemoveEntity(this);
        }

        /// <summary>
        /// Colliding with an entity will result in an explosion. If this is a bouncy projectile and it collides with
        /// part of the stage, however, then make it bounce!
        /// </summary>
        /// <param name="other">object this collided with</param>
        public override void CollideTop(GameObject other)
        {
            Vector2 velocityBeforeHittingGround = velocity;

            base.CollideTop(other);


            switch (projectileType)
            {
                case (ProjectileType.Linear):
                    //if its a linear projectile
                    if (other.TypeOfCollision == CollisionType.Solid)
                    {
                        //and it hits a wall, explode!
                        this.Explode();
                    }
                    else if (other is Enemy && shotByPlayer)
                    {
                        //it hits an enemy and it was shot by the player, explode on the enemy!
                        this.Explode();
                    }
                    else if (other is Player && !shotByPlayer)
                    {
                        //it hits the player and was shot by an enemy, explode on the player
                        this.Explode();
                    }
                    break;

                case (ProjectileType.Bouncy):
                    if (other.TypeOfCollision == CollisionType.Solid || other.TypeOfCollision == CollisionType.SemiSolid)
                    {
                        onGround = false;
                        velocity.Y = velocityBeforeHittingGround.Y*(-.7f);
                        Y -= 3;
                        initialVelocity.X *= .8f;
                        hitGround = true;
                    }
                    else if (other is Enemy && shotByPlayer && !hitGround && ((Enemy)other).CanBeAttacked())
                    {
                        //it hits an enemy, was shot by the player, and has not hit the ground yet, then explode!
                        this.Explode();
                        currentStage.DoImpactTime();
                    }
                    else if (other is Player && !shotByPlayer && !hitGround)
                    {
                        //it hits an player, was shot by the enemy, and has not hit the ground yet, then explode!
                        this.Explode();
                    }
                    break;
            }
        }

        /// <summary>
        /// Colliding with an entity will result in an explosion. If this is a bouncy projectile and it collides with
        /// part of the stage, however, then make it bounce!
        /// </summary>
        /// <param name="other">object this collided with</param>
        public override void CollideLeft(GameObject other)
        {
            base.CollideLeft(other);

            switch (projectileType)
            {
                case (ProjectileType.Linear):
                    //if its a linear projectile
                    if(other.TypeOfCollision == CollisionType.Solid)
                    {
                        //and it hits a wall, explode!
                        this.Explode();
                    }
                    else if(other is Enemy && shotByPlayer)
                    {
                        //it hits an enemy and it was shot by the player, explode on the enemy!
                        this.Explode();
                    }
                    else if (other is Player && !shotByPlayer)
                    {
                        //it hits the player and was shot by an enemy, explode on the player
                        this.Explode();
                    }
                    break;

                case (ProjectileType.Bouncy):
                    //if its a bouncy projectile
                    if (other.TypeOfCollision == CollisionType.Solid)
                    {
                        //and it hits a wall, just turn it around!
                        if(initialVelocity.X > 0)
                            this.initialVelocity.X *= -1; 
                    }
                    else if (other is Enemy && shotByPlayer && !hitGround && ((Enemy)other).CanBeAttacked())
                    {
                        //it hits an enemy, was shot by the player, and has not hit the ground yet, then explode!
                        this.Explode();
                        currentStage.DoImpactTime();
                    }
                    else if (other is Player && !shotByPlayer && !hitGround)
                    {
                        //it hits an player, was shot by the enemy, and has not hit the ground yet, then explode!
                        this.Explode();
                    }
                    break;
            }
        }

        /// <summary>
        /// Colliding with an entity will result in an explosion. If this is a bouncy projectile and it collides with
        /// part of the stage, however, then make it bounce!
        /// </summary>
        /// <param name="other">object this collided with</param>
        public override void CollideRight(GameObject other)
        {
            base.CollideRight(other);

            switch (projectileType)
            {
                case (ProjectileType.Linear):
                    //if its a linear projectile
                    if (other.TypeOfCollision == CollisionType.Solid)
                    {
                        //and it hits a wall, explode!
                        this.Explode();
                    }
                    else if (other is Enemy && shotByPlayer)
                    {
                        //it hits an enemy and it was shot by the player, explode on the enemy!
                        this.Explode();
                    }
                    else if (other is Player && !shotByPlayer)
                    {
                        //it hits the player and was shot by an enemy, explode on the player
                        this.Explode();
                    }
                    break;

                case (ProjectileType.Bouncy):
                    //if its a bouncy projectile
                    if (other.TypeOfCollision == CollisionType.Solid)
                    {
                        //and it hits a wall, just turn it around!
                        if (initialVelocity.X < 0)
                            this.initialVelocity.X *= -1;
                    }
                    else if (other is Enemy && shotByPlayer && !hitGround && ((Enemy)other).CanBeAttacked())
                    {
                        //it hits an enemy, was shot by the player, and has not hit the ground yet, then explode!
                        this.Explode();
                        currentStage.DoImpactTime();
                    }
                    else if (other is Player && !shotByPlayer && !hitGround)
                    {
                        //it hits an player, was shot by the enemy, and has not hit the ground yet, then explode!
                        this.Explode();
                    }
                    break;
            }
        }
    }
}
