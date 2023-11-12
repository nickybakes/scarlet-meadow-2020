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
//When entities touch the top of this, they get launched back upward like a trampoline!
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// When entities touch the top of this, they get launched back upward like a trampoline!
    /// </summary>
    class Trampoline : GameObject
    {
        private Direction facingDirection;

        /// <summary>
        /// Constructor takes and stores position and direction this trampoline is facing
        /// </summary>
        /// <param name="x">topleft x position</param>
        /// <param name="y">topleft y position</param>
        /// <param name="facingDirection">direction this trampoline is facing</param>
        public Trampoline(int x, int y, Direction facingDirection) : base(new Rectangle(x, y, 0, 3 * Game1.Scale), CollisionType.StaticEntity)
        {
            //stores data
            this.facingDirection = facingDirection;
            this.collisionType = CollisionType.StaticEntity;

            //changes size and texture depending on direction facing
            switch (this.facingDirection)
            {
                case (Direction.Front):
                    this.transform.Width = 9 * Game1.Scale;
                    this.texture = Game1.MiscTextures[0];
                    break;

                case (Direction.West):
                    this.transform.Width = 7 * Game1.Scale;
                    this.texture = Game1.MiscTextures[1];
                    break;

                case (Direction.East):
                    this.transform.Width = 7 * Game1.Scale;
                    this.texture = Game1.MiscTextures[2];
                    break;
            }
        }


        /// <summary>
        /// When "other" collides with this, make it bounce in the correct direction!
        /// </summary>
        /// <param name="other"></param>
        public override void CollideTop(GameObject other)
        {
            
            //only launch the object if it is a player, projectile, and they are falling
            if (!(other is Player) && !(other is Projectile) || other.VelocityY < 0)
                return;

            //launch the object depending on the direction this trampoline is facing
            switch (this.facingDirection)
            {
                case (Direction.Front):
                    other.VelocityY = -60;
                    break;

                case (Direction.West):
                    other.VelocityY = -60;
                    other.X = this.transform.Center.X - (other.Transform.Width / 2);
                    other.VelocityX = -30;
                    break;

                case (Direction.East):
                    other.VelocityY = -60;
                    other.X = this.transform.Center.X - (other.Transform.Width / 2);
                    other.VelocityX = 30;
                    break;
            }
        }
    }
}
