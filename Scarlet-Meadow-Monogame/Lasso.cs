using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/17/2020
//Scarlet Meadow monogame project
//this controls the lasso object the Scarlet can throw, it can collect and hold onto enemies and collectibles, then it brings them to Scarlet!
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// this controls the lasso object the Scarlet can throw, it can collect and hold onto enemies and collectibles, then it brings them to Scarlet!
    /// </summary>
    public class Lasso : GameObject
    {
        /// <summary>
        /// Refernece to the player object that is controlling this lasso
        /// </summary>
        private Player ownerPlayer;

        /// <summary>
        /// The amount of time that the thrown lasso will be flying away from the player
        /// </summary>
        private int distanceThrown;

        /// <summary>
        /// A list of objects that get caught in this lasso
        /// </summary>
        private List<GameObject> heldObjects;

        /// <summary>
        /// When objects are held in the lasso, we dont want them directly overlapping each other,
        /// so this stores each object's random positional offset while in the lasso
        /// </summary>
        private List<Point> offsets;

        /// <summary>
        /// if the lasso has been thrown by the player, set this to true
        /// </summary>
        private bool thrown;

        /// <summary>
        /// Get and set the amount of time that the thrown lasso will be flying away from the player
        /// </summary>
        public int DistanceThrown
        {
            get { return distanceThrown; }
            set { distanceThrown = value; }
        }

        /// <summary>
        /// Get and set if the lasso has been thrown by the player
        /// </summary>
        public bool Thrown
        {
            get { return thrown; }
            set { thrown = value; }
        }

        /// <summary>
        /// Constructor simply initializes lists and stores stats like the owner player
        /// </summary>
        /// <param name="ownerPlayer">Refernece to the player object that is controlling this lasso</param>
        public Lasso(Player ownerPlayer) 
            : base(0, 0, 100, 100, null, 1, ownerPlayer.CurrentStage)
        {
            thrown = false;
            this.ownerPlayer = ownerPlayer;
            heldObjects = new List<GameObject>();
            offsets = new List<Point>();
        }

        /// <summary>
        /// Controls the lasso's behavior when it has and when it has not been thrown by the player
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public override void Update(GameTime gameTime)
        {
            //if the lasso has not been thrown...
            if (!thrown)
            {
                //...then keep it tied to the player
                this.X = ownerPlayer.X;
                this.Y = ownerPlayer.Y;
                this.velocity = Vector2.Zero;
            }

            //if it has been thrown however...
            else
            {
                //if the lasso is flying away from the player
                if (distanceThrown > 0)
                {
                    //count down the amount of time is has until it starts coming back to the player
                    distanceThrown -= 3;
                }

                //if its coming back to the player...
                else
                {
                    //find and set the velocity of the lasso to the direction from the lasso to the player
                    this.VelocityX = ownerPlayer.X - this.X;
                    this.VelocityY = ownerPlayer.Y - this.Y;
                    velocity.Normalize();
                    velocity *= 30;
                }

                //update physics and collision
                base.Update(gameTime);

                //if the player does a melee attack and the lasso is close enough to the player
                if (ownerPlayer.State == PlayerState.Melee && Vector2.Distance(this.transform.Center.ToVector2(), ownerPlayer.Transform.Center.ToVector2()) < 1.3*Game1.Scale)
                {
                    //make the lasso start coming back to the player
                    distanceThrown = 0;

                    //drops all objects
                    DropAllObjects();
                }

                //going through each object held in the lasso and moving it with the lasso
                for(int i = 0; i < heldObjects.Count; i++)
                {
                    heldObjects[i].X = this.transform.Center.X - (heldObjects[i].Transform.Width / 2) + offsets[i].X;
                    heldObjects[i].Y = this.transform.Center.Y - (heldObjects[i].Transform.Height / 2) + offsets[i].Y;
                }
            }
        }

        /// <summary>
        /// Throwing the lasso gives it a velocity in that direction
        /// </summary>
        /// <param name="direction">the direction to throw the lasso towards</param>
        public void Throw(Direction direction)
        {
            switch (direction)
            {

                case (Direction.East):
                    velocity = new Vector2(30, -distanceThrown / 10)*1.15f;
                    break;

                case (Direction.West):
                    velocity = new Vector2(-30, -distanceThrown / 10) * 1.15f;
                    break;

                case (Direction.North):
                    velocity = new Vector2(0, -30) * 1.15f;
                    break;

                case (Direction.South):
                    velocity = new Vector2(0, 30) * 1.15f;
                    break;

                case (Direction.NorthEast):
                    velocity = new Vector2(24, -21) * 1.15f;
                    break;

                case (Direction.SouthEast):
                    velocity = new Vector2(24, 21) * 1.15f;
                    break;

                case (Direction.NorthWest):
                    velocity = new Vector2(-24, -21) * 1.15f;
                    break;

                case (Direction.SouthWest):
                    velocity = new Vector2(-24, 21) * 1.15f;
                    break;
            }
        }

        /// <summary>
        /// Drops all objects that are held in this lasso
        /// </summary>
        private void DropAllObjects()
        {
            foreach (GameObject gameObject in heldObjects)
            {
                //resets enemies to a knockback state (if they were not already attacking)
                if (gameObject is Enemy && ((Enemy)gameObject).State != EnemyState.Attack)
                {
                    ((Enemy)gameObject).State = EnemyState.Knockback;
                }
                //and sets all collectibles to have physics so they fall to the ground
                else if (gameObject is Collectible)
                {
                    ((Collectible)gameObject).HasPhysics = true;
                }

                gameObject.Y = ownerPlayer.Transform.Bottom - gameObject.Transform.Height - 1;
            }

            //finally, let go of all the held objects
            heldObjects.Clear();
            offsets.Clear();
        }

        /// <summary>
        /// When the lasso collides with the stage, make it start coming back to the player
        /// </summary>
        private void CollideWithStage()
        {
            distanceThrown = 0;
        }

        /// <summary>
        /// Controls the behavior of the lasso when it collides with various objects
        /// </summary>
        /// <param name="other">the object the lasso collided with</param>
        private void CollisionAll(GameObject other)
        {
            //if it hits a solid wall
            if (other.TypeOfCollision == CollisionType.Solid)
            {
                //then make the lasso come back the player
                CollideWithStage();
            }
            //if it hits the player
            else if (other is Player && distanceThrown <= 10)
            {
                //then make the player catch it
                thrown = false;

                //drop all objects in the lasso
                this.DropAllObjects();
            }

            //if it hits an enemy and the enemy can be attacked and the enemy hasn't already been caught in this lasso
            else if (other is Enemy && ((Enemy)other).CanBeAttacked() && !heldObjects.Contains(other))
            {
                //then grab the enemy in this lasso!
                heldObjects.Add(other);
                offsets.Add(new Point(Game1.RNG.Next(-20, 20), Game1.RNG.Next(-20, 20)));
                ((Enemy)other).State = EnemyState.Restrained;
                currentStage.DoImpactTime();
            }
            else if(other is Collectible && !heldObjects.Contains(other))
            {
                //if it hits a collectible item, grab the item!
                heldObjects.Add(other);
                offsets.Add(new Point(Game1.RNG.Next(-20, 20), Game1.RNG.Next(-20, 20)));
            }
            else if (other is Civilian && !heldObjects.Contains(other))
            {
                //if it hits a collectible item, grab the item!
                heldObjects.Add(other);
                offsets.Add(new Point(Game1.RNG.Next(-20, 20), Game1.RNG.Next(-20, 20)));
                currentStage.DoImpactTime();
            }
        }

        /// <summary>
        /// When the lasso collides with anything, send the data to the CollisionAll method
        /// </summary>
        /// <param name="other">the object the lasso collided with</param>
        public override void CollideTop(GameObject other)
        {
            CollisionAll(other);
        }

        /// <summary>
        /// When the lasso collides with anything, send the data to the CollisionAll method
        /// </summary>
        /// <param name="other">the object the lasso collided with</param>
        public override void CollideBottom(GameObject other)
        {
            CollisionAll(other);
        }

        /// <summary>
        /// When the lasso collides with anything, send the data to the CollisionAll method
        /// </summary>
        /// <param name="other">the object the lasso collided with</param>
        public override void CollideLeft(GameObject other)
        {
            CollisionAll(other);
        }

        /// <summary>
        /// When the lasso collides with anything, send the data to the CollisionAll method
        /// </summary>
        /// <param name="other">the object the lasso collided with</param>
        public override void CollideRight(GameObject other)
        {
            CollisionAll(other);
        }

        /// <summary>
        /// Drawing the lasso not only draws the lasso object, but also draws a rope from the player to the lasso
        /// </summary>
        /// <param name="sb">the spritebatch to draw to</param>
        /// <param name="cameraOffset">the camera offset in the world</param>
        public override void Draw(SpriteBatch sb, Vector2 cameraOffset)
        {
            //only draw the lasso if it has been thrown
            if (thrown)
            {
                if(heldObjects.Count == 0)
                {
                    //draws the back segment of the lasso if nothing is in it
                    texture = Game1.MiscTextures[16];
                    base.Draw(sb, cameraOffset);
                }

                //then draw the front segment of the lasso
                texture = Game1.MiscTextures[15];
                base.Draw(sb, cameraOffset);

                //doing some vector math to draw a sort-of-line of "rope" to the lasso so it looks like the player is holding it
                Vector2 lassoCenter = this.transform.Center.ToVector2();
                Vector2 ownerPlayerCenter = ownerPlayer.Transform.Center.ToVector2();
                Vector2 lineToPlayer = lassoCenter - ownerPlayerCenter;
                for (float i = 0; i < 1; i += .05f)
                {
                    //increment throug the line and draw small sprites to generate a visual line
                    sb.Draw(texture = Game1.MiscTextures[17], new Rectangle((int)(lineToPlayer.X  * i + ownerPlayerCenter.X - cameraOffset.X),
                        (int)(lineToPlayer.Y  * i + ownerPlayerCenter.Y - cameraOffset.Y), 30, 30), Color.White);
                }

            }

        }
    }
}
