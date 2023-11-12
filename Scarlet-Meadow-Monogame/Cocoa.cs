using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//3/26/2020
//Scarlet Meadow monogame project
//holds the code for Cocoa's specific actions, animations, and stats
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// holds the code for Cocoa's specific actions, animations, and stats
    /// </summary>
    class Cocoa : Player
    {
        /// <summary>
        /// the current time left until another grenade can be shot
        /// </summary>
        private double cocoaCoolDown;

        /// <summary>
        /// the maximum amount of cooldown time before another grenade can be shot
        /// </summary>
        private const double cocoaCoolDownMax = .6;

        /// <summary>
        /// Number of times Cocoa has propelled herself in air with her grenade launcher
        /// </summary>
        private int timesJumpedInAir;

        /// <summary>
        /// Constructor sets up movement speed and other stats for a Cocoa player
        /// </summary>
        /// <param name="x">the starting x position</param>
        /// <param name="y">the starting y position</param>
        /// <param name="width">width of the player's hitbox</param>
        /// <param name="height">height of the player's hitbox</param>
        /// <param name="animations">Set of animations used by the player</param>
        /// <param name="gravity">how much gravity affects the player</param>
        /// <param name="currentStage">the stage the player is added to</param>
        public Cocoa(int x, int y, int width, int height, Animation[] animations, int gravity, Stage currentStage)
            : base(x, y, width, height, animations, gravity, currentStage)
        {
            //set up Cocoa specific statistics
            maxRunSpeed = 17;
            jumpHeight = 42;
            baseCharge = 5;
            maxCharge = 20;
            damageOutput = 15;
            charge = baseCharge;
            cocoaCoolDown = cocoaCoolDownMax;
        }

        /// <summary>
        /// When cocoa is charging her attack, she increases the speed that her grenades will go when launched
        /// </summary>
        public override void ChargeAttack()
        {
            if (charge < maxCharge)
                charge += 1;
        }

        /// <summary>
        /// When cocoa does her attack, she shoots a grenade/bomb in the aimed direction
        /// </summary>
        /// <param name="direction">the direction the player is aiming</param>
        public override void DoAttack(Direction direction)
        {
            //give the projectile a velocity depending on the aimed direction...
            //if the player aims downward and is in the air, give them a little boost (send them upward again)
            switch (direction)
            {
                case (Direction.East):
                    currentStage.AddEntity(new Projectile(X, Y, 100, 100, Game1.MiscTextures[7], currentStage, new Vector2(1, -.6f) * charge, CurrentDamageOutput, ProjectileType.Bouncy, true, 2.5));
                    break;

                case (Direction.West):
                    currentStage.AddEntity(new Projectile(X - 10, Y, 100, 100, Game1.MiscTextures[7], currentStage, new Vector2(-1, -.6f) * charge, CurrentDamageOutput, ProjectileType.Bouncy, true, 2.5));
                    break;

                case (Direction.North):
                    currentStage.AddEntity(new Projectile(X, Y, 100, 100, Game1.MiscTextures[7], currentStage, new Vector2(0, -2) * charge, CurrentDamageOutput, ProjectileType.Bouncy, true, 2.5));
                    break;

                case (Direction.South):
                    currentStage.AddEntity(new Projectile(X, Y, 100, 100, Game1.MiscTextures[7], currentStage, new Vector2(0, 2) * charge, CurrentDamageOutput, ProjectileType.Bouncy, true, 2.5));
                    if (!onGround && timesJumpedInAir < 2)
                    {
                        timesJumpedInAir++;
                        velocity.Y = -jumpHeight;
                    }
                    break;

                case (Direction.NorthEast):
                    currentStage.AddEntity(new Projectile(X, Y, 100, 100, Game1.MiscTextures[7], currentStage, new Vector2(1.3f, -1.3f) * charge, CurrentDamageOutput, ProjectileType.Bouncy, true, 2.5));
                    break;

                case (Direction.SouthEast):
                    currentStage.AddEntity(new Projectile(X, Y, 100, 100, Game1.MiscTextures[7], currentStage, new Vector2(1.3f, 1.3f) * charge, CurrentDamageOutput, ProjectileType.Bouncy, true, 2.5));
                    if (!onGround && timesJumpedInAir < 2)
                    {
                        timesJumpedInAir++;
                        velocity.Y = -jumpHeight;
                    }
                    break;

                case (Direction.NorthWest):
                    currentStage.AddEntity(new Projectile(X - 10, Y, 100, 100, Game1.MiscTextures[7], currentStage, new Vector2(-1.3f, -1.3f) * charge, CurrentDamageOutput, ProjectileType.Bouncy, true, 2.5));
                    break;

                case (Direction.SouthWest):
                    currentStage.AddEntity(new Projectile(X - 10, Y, 100, 100, Game1.MiscTextures[7], currentStage, new Vector2(-1.3f, 1.3f) * charge, CurrentDamageOutput, ProjectileType.Bouncy, true, 2.5));
                    if (!onGround && timesJumpedInAir < 2)
                    {
                        timesJumpedInAir++;
                        velocity.Y = -jumpHeight;
                    }
                    break;
            }
            cocoaCoolDown = 0;
            charge = baseCharge;
        }

        /// <summary>
        /// Cocoa's special ability spawns a bunch of explosions around her.
        /// </summary>
        public override void SpecialMove()
        {
            int explosionSize = Game1.Scale * 4;
            for(int x = transform.Center.X - explosionSize*3; x < X + explosionSize * 4; x += explosionSize*2)
            {
                for (int y = transform.Center.Y - explosionSize * 5; y < Y + explosionSize * 6; y += explosionSize*2)
                {
                    currentStage.AddEntity(new Explosion(x, y, true, 100, currentStage));
                }
            }
            base.SpecialMove();
        }

        /// <summary>
        /// Cocoa's update tracks her cooldown for her main attack
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            //counts up on the cooldown timer
            if (cocoaCoolDown < cocoaCoolDownMax)
            {
                cocoaCoolDown += gameTime.ElapsedGameTime.TotalSeconds;
                canChargeAttack = false;
            }
            else
            {
                canChargeAttack = true;
            }

            base.Update(gameTime);

            if (onGround)
                timesJumpedInAir = 0;
        }
    }
}
