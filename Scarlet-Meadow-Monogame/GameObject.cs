using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/12/2020
//Scarlet Meadow monogame project
//this represents all objects in a level, has position and size and texture
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// These are used to give default collision actions for platforms and objects
    /// </summary>
    public enum CollisionType
    {
        DynamicEntity, //player, entities, collectible items, and other moving objects
        StaticEntity, //static objects like trampolines and ziplines. these dont move
        Solid, //A very solid wall/platform
        SemiSolid, //Only solid on top, can jump through it
        Damage, //damages and boucnes the player, kills all other things
        Fall, //instantly loses stage if touched
        None //no collision detection
    }

    /// <summary>
    /// this represents all objects in a level, has position and size and texture
    /// </summary>
    public class GameObject
    {
        /// <summary>
        /// this stores the position and size of the object
        /// </summary>
        protected Rectangle transform;

        /// <summary>
        /// the velocity of this object, position is changed every frame depending on this vector's values
        /// </summary>
        protected Vector2 velocity;

        /// <summary>
        /// the graphical representation of this object
        /// </summary>
        protected Texture2D texture;

        /// <summary>
        /// whether this object is on the ground or not
        /// </summary>
        protected bool onGround;

        /// <summary>
        /// the vertical velocity to affect this object each frame
        /// </summary>
        protected float gravity;

        /// <summary>
        /// True if the entity should be drawn facing left (flipped)
        /// </summary>
        protected bool facingLeft;

        /// <summary>
        /// Set to true if the object is active in the level and should be updated, drawn, and checked for collisions
        /// </summary>
        protected bool active;

        /// <summary>
        /// Determines what type of collision should be used for this game object
        /// </summary>
        protected CollisionType collisionType;

        /// <summary>
        /// this allows this object to check its collision with other objects on the stage
        /// </summary>
        protected Stage currentStage;

        
        /// <summary>
        /// get the position and size of this entity
        /// </summary>
        public Rectangle Transform
        {
            get { return transform; }
        }

        /// <summary>
        /// get and set the X coord of this object
        /// </summary>
        public int X
        {
            get { return transform.X; }
            set { transform.X = value; }
        }


        /// <summary>
        /// get and set the Y coord of this object
        /// </summary>
        public int Y
        {
            get { return transform.Y; }
            set { transform.Y = value; }
        }

        public Vector2 VelocityVector
        {
            get { return velocity; }
        }

        /// <summary>
        /// get and set the horizontal velocity of this object
        /// </summary>
        public int VelocityX
        {
            get { return (int) velocity.X; }
            set { velocity.X = value; }
        }

        /// <summary>
        /// get and set the vertical velocity of this object
        /// </summary>
        public int VelocityY
        {
            get { return (int) velocity.Y; }
            set { velocity.Y = value; }
        }

        /// <summary>
        /// Get and set this game object's position as a vector so we can do vector maths on it!
        /// </summary>
        public Vector2 PositionVector
        {
            get { return new Vector2(transform.X, transform.Y); }
            set { transform.X = (int)value.X; transform.Y = (int)value.Y; }
        }

        /// <summary>
        /// Set to true if the object is colliding with the toop part of level geometry
        /// </summary>
        public bool OnGround
        {
            get { return onGround; }
            set { onGround = value; }
        }

        /// <summary>
        /// Set to true if the object is active in the level and should be updated, drawn, and checked for collisions
        /// </summary>
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        /// <summary>
        /// The stage this entity is within
        /// </summary>
        public Stage CurrentStage
        {
            get { return currentStage; }
            set { currentStage = value; }
        }

        /// <summary>
        /// True if the object is facing the left side of the screen
        /// </summary>
        public bool FacingLeft
        {
            get { return facingLeft; }
        }

        /// <summary>
        /// What type of collision this object is, solid? semiSolid? dynamic entity? etc.
        /// </summary>
        public CollisionType TypeOfCollision
        {
            get { return collisionType; }
        }

        /// <summary>
        /// constructor for the a general game Object, sets data for the object
        /// </summary>
        /// <param name="x">starting X coord</param>
        /// <param name="y">starting Y coord</param>
        /// <param name="width">width of this objects hitbox</param>
        /// <param name="height">height of this objects hitbox</param>
        /// <param name="texture">height of this objects hitbox</param>
        /// <param name="gravity">the gravity that affects this object</param>
        /// <param name="currentStage">the stage that contains this object</param>
        public GameObject(int x, int y, int width, int height, Texture2D texture, float gravity, Stage currentStage)
        {
            this.transform = new Rectangle(x, y, width, height);
            this.velocity = new Vector2(0,0);
            this.texture = texture;
            this.gravity = gravity;
            this.currentStage = currentStage;
            active = true;
        }

        /// <summary>
        /// Constructor that only needs position and size as a rectangle. Best used for platforms/collison data
        /// </summary>
        /// <param name="rect">The rectangle storing position and size</param>
        /// <param name="collisionType">The type of collision for this platform (ie Solid, SemiSolid, Damage)</param>
        public GameObject(Rectangle rect, CollisionType collisionType)
        {
            this.transform = rect;
            this.velocity = Vector2.Zero;
            this.collisionType = collisionType;
            this.texture = null;
            this.gravity = 0;
            this.currentStage = null;
            active = true;
        }

        /// <summary>
        /// Call this every frame for every active object in the stage. Default update checks 
        /// for collision and applies velocity to position
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            //applies movement on the X axis
            transform.X += this.VelocityX;
            //check for collisions
            currentStage.CheckCollisionsWithOtherEntities(this);
            currentStage.CheckCollisionsWithStage(this);

            //applies movement on the Y axis
            transform.Y += this.VelocityY;
            //check for collisions
            currentStage.CheckCollisionsWithOtherEntities(this);
            currentStage.CheckCollisionsWithStage(this);

            //NOTE: checking for collisions per axis results in more accurate collision detection

            //if this object is on the ground:
            if (onGround)
            {
                //their velocity should not be affected by gravity, so set it to 0
                this.VelocityY = 0;

                //if they are moving, apply friction
                if (velocity.X > 0)
                    velocity.X -= 2;
                else if (velocity.X < 0)
                    velocity.X += 2;

                //if X velocity is close enough to 0, just make it 0
                if (Math.Abs(this.velocity.X) < 2)
                    velocity.X = 0;
            }
            //if the object is in the air:
            else
            {
                //apply gravity to the object's velocity
                this.velocity.Y += gravity;

                //if they are moving, apply slight air resistance
                if (velocity.X > 0)
                    velocity.X -= .2f;
                else if(velocity.X < 0)
                    velocity.X += .2f;

                //if X velocity is close enough to 0, just make it 0
                if (Math.Abs(this.velocity.X) < 2)
                    velocity.X = 0;
            }

            //clamping Y velocity because sometimes falling too fast made the player fall through platforms
            velocity.Y = Math.Max(-60, Math.Min(40, velocity.Y));

            //if the object falls out of the map, then remove them from the stage
            if(Y > currentStage.Transform.Height + currentStage.Transform.Y)
            {
                //"lose" the stage
                if (this is Player)
                {
                    Game1.LoseStage("You fell from a great height!");
                }
                //then kill anything else instantly
                else if (this is Enemy)
                {
                    ((Enemy)this).Die();
                }
                else if (this is Civilian)
                {
                    ((Civilian)this).SaveCivilian();
                }
                else
                {
                    currentStage.RemoveEntity(this);
                }
            }

            //limit y movement so we dont go above the stage
            Y = Math.Max(currentStage.Transform.Y, Y);

            //and limit X movement so we dont go past the edges
            X = Math.Max(currentStage.Transform.X, Math.Min(currentStage.Transform.X + currentStage.Transform.Width - transform.Width, X));
        }

        /// <summary>
        /// Checks if "other" is hitting the top, bottom, left, or right side of this object,
        /// </summary>
        /// <param name="other">The entity hitting this object</param>
        public void CheckCollision(GameObject other)
        {
            if (collisionType == CollisionType.None || other.collisionType == CollisionType.None)
                return;

            //first we check if any intersections are happening at all
            if (transform.Intersects(other.Transform))
            {
                //then we get the rectangle of the intersection
                Rectangle intersect = Rectangle.Intersect(transform, other.Transform);

                //we always check if the objects are still active
                //if the height of the intersection is less than the width, then a vertical collision must have occured
                if (this.Active && other.Active && intersect.Height <= intersect.Width)
                {
                    //if the other object is on top of this one and its moving downward
                    if (this.Active && other.Active && other.Transform.Y < transform.Y && other.VelocityY >= 0)
                    {
                        //then call their collide top actions on each other
                        other.CollideTop(this);
                        this.CollideTop(other);
                    }
                    //if the other object is on the bottom of this one and its moving upward
                    if (this.Active && other.Active && other.Transform.Y >= transform.Y && other.VelocityY < 0)
                    {
                        //then call their collide bottom actions on each other
                        other.CollideBottom(this);
                        this.CollideBottom(other);
                    }
                }
                //if the width of the intersection is less than the height, then a horizontal collision must have occured
                if (this.Active && other.Active && intersect.Width < intersect.Height)
                {
                    //if the other object is to the left of this one and its moving leftward
                    if (this.Active && other.Active && other.Transform.X < transform.X && other.VelocityX > 0)
                    {
                        //then call their collide left actions on each other
                        other.CollideLeft(this);
                        this.CollideLeft(other);
                    }
                    //if the other object is to the right of this one and its moving rightward
                    if (this.Active && other.Active && other.Transform.X >= transform.X && other.VelocityX < 0)
                    {
                        //then call their collide right actions on each other
                        other.CollideRight(this);
                        this.CollideRight(other);
                    }
                }

            }


            //after the more accurate checks, if the object is still inside another, apply more brute force collision detection
            if (this.Active && other.Active && this.collisionType != CollisionType.SemiSolid && transform.Intersects(other.Transform))
            {
                Rectangle intersect = Rectangle.Intersect(transform, other.Transform);
                //if the other object is on top of this one
                if (this.Active && other.Active && other.Transform.Y < transform.Y)
                {
                    //then call their collide top actions
                    other.CollideTop(this);
                    this.CollideTop(other);
                }
                //if the other object is on the bottom of this one 
                if (this.Active && other.Active && other.Transform.Y >= transform.Y)
                {
                    other.CollideBottom(this);
                    this.CollideBottom(other);
                }

                //if the width of the intersection is less than the height, then a horizontal collision must have occured
                if (this.Active && other.Active && intersect.Width < intersect.Height)
                {
                    //if the other object is to the left of this one
                    if (this.Active && other.Active && other.Transform.X < transform.X)
                    {
                        //then call their collide left actions on each other
                        other.CollideLeft(this);
                        this.CollideLeft(other);
                    }
                    //if the other object is to the right of this one
                    if (this.Active && other.Active && other.Transform.X >= transform.X)
                    {
                        //then call their collide right actions on each other
                        other.CollideRight(this);
                        this.CollideRight(other);
                    }
                }
            }
        }

        /// <summary>
        /// what to do when this object collides with the top of the "other" object
        /// -default collision stops the object like a solid
        /// THE TOP COLLISION is like the top side of a platform!!!
        /// </summary>
        /// <param name="other">the object that collided with this object</param>
        public virtual void CollideTop(GameObject other)
        {
            switch (other.TypeOfCollision)
            {
                case (CollisionType.Solid):
                    if (!onGround)
                    {
                        //set the other objects position to be ontop of this one but not in it
                        Y = other.Y - transform.Height + 1;
                        onGround = true;
                    }
                    break;

                case (CollisionType.SemiSolid):
                    if (!onGround)
                    {
                        if(transform.Bottom < other.transform.Bottom)
                        {
                            //set the other objects position to be ontop of this one but not in it
                            Y = other.Y - transform.Height + 1;
                            onGround = true;
                        }
                    }
                    break;

                case (CollisionType.Damage):
                    //damage platforms damage the player and bounce them upward
                    if(this is Player)
                    {
                        ((Player)this).Damage(25);
                        this.VelocityY = -60;
                        Y = other.Y - transform.Height + 1;
                    }
                    //they kill anything else instantly
                    else if(this is Enemy)
                    {
                        ((Enemy)this).Die();
                    }
                    else if (this is Civilian)
                    {
                        ((Civilian)this).SaveCivilian();
                    }
                    else
                    {
                        currentStage.RemoveEntity(this);
                    }
                    break;

                case (CollisionType.Fall):
                    //fall platform instante loses the stage if the player touches it
                    if (this is Player)
                    {
                        Game1.LoseStage("You fell from a great height!");
                        ((Player)this).Kill();
                    }
                    //they kill anything else instantly
                    else if (this is Enemy)
                    {
                        ((Enemy)this).Die();
                    }
                    else if (this is Civilian)
                    {
                        ((Civilian)this).SaveCivilian();
                    }
                    else
                    {
                        currentStage.RemoveEntity(this);
                    }
                    break;
            }

        }

        /// <summary>
        /// what to do when this object collides with the bottom of the "other" object
        /// -default collision stops the object like a solid
        /// </summary>
        /// <param name="other">the object that collided with this object</param>
        public virtual void CollideBottom(GameObject other)
        {
            switch (other.TypeOfCollision)
            {
                case (CollisionType.Solid):
                    //set the other objects position to be below of this one but not in it
                    Y = other.Y + other.transform.Height;

                    //set Y velocity to 0 like they just bonked their head
                    this.VelocityY = 0;
                    break;
            }

        }

        /// <summary>
        /// what to do when this object collides with the left of the "other" object
        /// -default collision stops the object like a solid
        /// </summary>
        /// <param name="other">the object that collided with this object</param>
        public virtual void CollideLeft(GameObject other)
        {
            switch (other.TypeOfCollision)
            {
                case (CollisionType.Solid):
                    //set the other objects position to be to the left of this one but not in it
                    X = other.X - transform.Width - 1;
                    break;
            }

        }

        /// <summary>
        /// what to do when this object collides with the right of the "other" object
        /// -default collision stops the object like a solid
        /// </summary>
        /// <param name="other">the object that collided with this object</param>
        public virtual void CollideRight(GameObject other)
        {
            switch (other.TypeOfCollision)
            {
                case (CollisionType.Solid):
                    //set the other objects position to be to the right of this one but not in it
                    X = other.X + other.transform.Width;
                    break;
            }
        }


        /// <summary>
        /// When the user presses the interact key, it will go through the entities in the stage and call this method.
        /// By default does nothing, if an object is interactible, check for distance and do action here.
        /// </summary>
        /// <param name="interactor">The object that interacted with this one</param>
        public virtual void Interact(GameObject interactor)
        {

        }

        /// <summary>
        /// Basic draw to screen method with optional camera offset
        /// </summary>
        /// <param name="sb">the spritebatch to draw to</param>
        /// <param name="cameraOffset">the position of the camera at which to offset objects on screen</param>
        public virtual void Draw(SpriteBatch sb, Vector2 cameraOffset)
        {
            //if the texture is null, dont render it, its invisible
            if (texture != null)
            {
                //if the cameraoffset is null, just draw straight to the screen
                if (cameraOffset != null)
                {
                    //if its not null, draw it to the screen affected by the camera offset
                    Rectangle destinationRectangle = new Rectangle(transform.X - (int)cameraOffset.X, transform.Y - (int)cameraOffset.Y, transform.Width, transform.Height);
                    if (facingLeft)
                    {
                        //if this object is facing left, flip it horizontally
                        //(all sprites should be drawn facing right originally)
                        sb.Draw(texture, destinationRectangle, null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                    }
                    else
                    {
                        sb.Draw(texture, destinationRectangle, Color.White);
                    }
                }
                else
                {
                    if (facingLeft)
                    {
                        //if this object is facing left, flip it horizontally
                        //(all sprites should be drawn facing right originally)
                        sb.Draw(texture, transform, null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                    }
                    else
                    {
                        sb.Draw(texture, transform, Color.White);
                    }
                }
            }
            //draw spikes for damage type platforms
            else if(collisionType == CollisionType.Damage || collisionType == CollisionType.Fall)
            {
                for(int x = transform.Left; x < transform.Right; x += Game1.Scale)
                {
                    for (int y = transform.Top; y < transform.Bottom; y += Game1.Scale)
                    {
                        if (Vector2.DistanceSquared(currentStage.MainPlayer.PositionVector, new Vector2(x, y)) < Math.Pow(40 * Game1.Scale, 2))
                            sb.Draw(Game1.MiscTextures[14], new Rectangle(x-(int)cameraOffset.X, y-(int)cameraOffset.Y, Game1.Scale, Game1.Scale), Color.White);
                    }
                }
            }

            //debugger code for drawing collision boxes for platforms
            //if (texture == null)
            //{
            //    sb.Draw(Game1.lassoTexture, new Rectangle(transform.X - (int)cameraOffset.X, transform.Y - (int)cameraOffset.Y, transform.Width, 3), Color.DeepPink);
            //    sb.Draw(Game1.lassoTexture, new Rectangle(transform.X + transform.Width - (int)cameraOffset.X, transform.Y - (int)cameraOffset.Y, 3, transform.Height), Color.DeepPink);
            //    sb.Draw(Game1.lassoTexture, new Rectangle(transform.X - (int)cameraOffset.X, transform.Y - (int)cameraOffset.Y, 3, transform.Height), Color.DeepPink);
            //    sb.Draw(Game1.lassoTexture, new Rectangle(transform.X - (int)cameraOffset.X, transform.Y + transform.Height - (int)cameraOffset.Y, transform.Width, 3), Color.DeepPink);
            //}
        }
    }
}
