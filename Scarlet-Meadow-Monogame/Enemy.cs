using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/28/2020
//Scarlet Meadow monogame project
//Holds methods that all enemies should, have will have children classes for more specific enemy behavior
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// The different possible states the enemy can be in
    /// </summary>
    public enum EnemyState
    {
        Patrol, //before the player shows up, the enemy is just there (possibly walking around, "patrolling the area")
        Idle, //just standing still
        Enraged, //Aggrevated, going to attack the player
        Attack, //actually attacking the player
        Knockback, //getting knocked back from a player's attacks
        Restrained, //possibly stuck in the Player's lasso, cannot move or attack from this state
    }

    /// <summary>
    /// Holds methods that all enemies should, have will have children classes for more specific enemy behavior
    /// </summary>
    class Enemy : GameObject
    {

        /// <summary>
        /// the current health of the enemy. if it reaches 0, they die!
        /// </summary>
        protected int health;

        /// <summary>
        /// the maximum health that the enemy starts with. current health cannot go over this!
        /// </summary>
        protected int healthMax;

        /// <summary>
        /// the amount of damage this enemy does to the player in one attack
        /// </summary>
        protected int damageOutput;

        /// <summary>
        /// if the player is inside this hit box, the enemy switches to the Enraged state!
        /// </summary>
        protected Rectangle visionHitbox;

        /// <summary>
        /// determines the current behavior of the enemy; i.e. what they should be
        /// doing right now (See EnemyState enum for more detail)
        /// </summary>
        protected EnemyState currentState;

        /// <summary>
        /// the set of animations for this enemy, one animation for each state
        /// Put specific animations in the Enemy child classes (See grabber enemy)
        /// </summary>
        protected Animation[] animations;

        /// <summary>
        /// how much time is currently left until the enemy can go back to the patrol state
        /// </summary>
        protected double idleCoolDownTimer;

        /// <summary>
        /// maximum amount of time enemy has to wait until they can go back to the patrol state
        /// </summary>
        protected double idleCoolDownTimerMax;

        /// <summary>
        /// how much time is currently left until the enemy can possibly go back to Enrage state
        /// </summary>
        protected double patrolCoolDownTimer;

        /// <summary>
        /// maximum amount of time enemy has to wait until they can possibly go back to Enrage state
        /// </summary>
        protected double patrolCoolDownTimerMax;

        /// <summary>
        /// how much time is currently left until the enemy is done the attack
        /// </summary>
        protected double attackCoolDownTimer;

        /// <summary>
        /// maximum amount of time enemy has to wait until they are done the attack
        /// </summary>
        protected double attackCoolDownTimerMax;

        /// <summary>
        /// how much time is currently left until the enemy stops being invincible. 
        /// they will be blinking during this time
        /// </summary>
        protected double invincibilityTimer;

        /// <summary>
        /// maximum amount of invincibility time enemy gets
        /// </summary>
        protected const double invincibilityTimerMax = 2;

        /// <summary>
        /// controls how often the enemy's animation should be blinking
        /// while they are invinsible
        /// </summary>
        protected double invincibilityBlinkTimer;

        /// <summary>
        /// maximum time between each invinciblity blink
        /// </summary>
        protected const double invincibilityBlinkTimerMax = .18;

        /// <summary>
        /// how much time the enemy stays in the "restrained" state after being stunned
        /// </summary>
        private double stunTimer;

        /// <summary>
        /// All of the items this enemy is holding. These are dropped when killed
        /// </summary>
        private List<Collectible> collectibles;

        /// <summary>
        /// the number of Big Badge Bit items to drop when killed
        /// </summary>
        protected int bigBitsToDrop;

        /// <summary>
        /// the number of Big Badge Bit items to drop when killed
        /// </summary>
        protected int smallBitsToDrop;

        /// <summary>
        /// Returns true if the enemy has invincibility blinking
        /// </summary>
        public bool Invincible
        {
            get { return invincibilityTimer < invincibilityTimerMax; }
        }

        /// <summary>
        /// Get and set the state that determines the current behavior of the enemy; 
        /// i.e. what they should be doing right now (See EnemyState enum for more detail)
        /// </summary>
        public EnemyState State
        {
            get { return currentState; }
            set { currentState = value; }
        }


        /// <summary>
        /// constructor stores the starting position and animation array for the enemy
        /// </summary>
        /// <param name="x">starting x position</param>
        /// <param name="y">start y position</param>
        /// <param name="width">width of the enemy's hitbox</param>
        /// <param name="height">height of the enemy's hitbox</param>
        /// <param name="animations">the set of animations for this enem</param>
        /// <param name="currentStage">the stage this enemy is being added to</param>
        /// <param name="facingLeft">true if the enemy should be facing left at the start of the game</param>
        public Enemy(int x, int y, int width, int height, Animation[] animations, Stage currentStage, bool facingLeft) : base(x, y, width, height, null, .6f, currentStage)
        {
            //store all of this data
            this.animations = animations;
            this.facingLeft = facingLeft;

            collectibles = new List<Collectible>();

            //initialize timers
            invincibilityTimer = invincibilityTimerMax;
            stunTimer = -1;
            this.idleCoolDownTimer = 0;
            this.patrolCoolDownTimer = 0;
            this.attackCoolDownTimer = 0;
        }

        /// <summary>
        /// Stores this item in this enemy. These items are dropped when killed
        /// </summary>
        /// <param name="collectible">the item to store in this enemy</param>
        public void AddItem(Collectible collectible)
        {
            collectibles.Add(collectible);
        }

        /// <summary>
        /// This method is called when the enemy unleashes an attack. Override it in enemy child classes
        /// </summary>
        public virtual void DoAttack()
        {

        }

        /// <summary>
        /// If an enemy has a special condition where they cannot be attacked/lassoed, do it here
        /// </summary>
        /// <returns>true if the enemy can be attacked</returns>
        public virtual bool CanBeAttacked()
        {
            if (!Invincible && currentState != EnemyState.Knockback)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Removed this enemy from the stage and spawns in collectible items for the player
        /// </summary>
        public void Die()
        {
            //drop stored collectible items
            foreach(Collectible item in collectibles)
            {
                item.PositionVector = this.PositionVector;
                currentStage.AddEntity(item);
            }

            //drop big badge bit items
            for(int i = 0; i < bigBitsToDrop; i++)
            {
                currentStage.AddEntity(new Collectible(X, Y, Game1.Scale, 2*Game1.Scale, Game1.MiscTextures[12], currentStage,
                    ItemType.BadgeBitBig, true, new Vector2(Game1.RNG.Next(-18, 18), Game1.RNG.Next(-18, -5))));
            }

            //drop small badge bit items
            for (int i = 0; i < smallBitsToDrop; i++)
            {
                currentStage.AddEntity(new Collectible(X, Y, Game1.Scale, Game1.Scale, Game1.MiscTextures[13], currentStage,
                ItemType.BadgeBitSmall, true, new Vector2(Game1.RNG.Next(-18, 18), Game1.RNG.Next(-18, -5))));
            }
            //finally, remove this entity from the stage
            currentStage.RemoveEntity(this);
        }


        /// <summary>
        /// Starts the stun timer by setting it to timeStunned seconds, and also makes this enemy not invinsible anymore
        /// </summary>
        /// <param name="timeStunned">amount of time (in seconds) to be stunned for</param>
        public void Stun(int timeStunned)
        {
            stunTimer = timeStunned;
            invincibilityTimer = invincibilityTimerMax;
        }

        /// <summary>
        /// Causes this enemy to be launched in a direction and switched to the knockback state
        /// </summary>
        /// <param name="damageAmount">The amount to damage the enemy by</param>
        /// <param name="knockbackVelocity">the speed an direction to send the enemy toward</param>
        public void DamageKnockback(int damageAmount, Vector2 knockbackVelocity)
        {
            //the nemy cannot be damaged/knockedback again if they are already knocked back or invincible
            if (!Invincible && currentState != EnemyState.Knockback)
            {
                health -= damageAmount;
                if(stunTimer > 0)
                    invincibilityTimer = invincibilityTimerMax*.75;
                else
                    invincibilityTimer = 0;


                //getting hit to the right should make the player face left when being knocked back
                if (knockbackVelocity.X > 0)
                    facingLeft = true;
                //getting hit to the left should make the player face right when being knocked back
                else if (knockbackVelocity.X < 0)
                    facingLeft = false;

                //apply the velocity and state
                this.velocity = knockbackVelocity;
                currentState = EnemyState.Knockback;

                //when an enemy is hit, have a random chance to spawn some items!
                int randomBitChance = Game1.RNG.Next(0, 5);
                if (randomBitChance == 0)
                {
                    currentStage.AddEntity(new Collectible(X, Y, Game1.Scale, Game1.Scale, Game1.MiscTextures[13], currentStage,
                        ItemType.BadgeBitSmall, true, new Vector2(Game1.RNG.Next(-18, 18), Game1.RNG.Next(-18, -5))));
                }
                else if(randomBitChance == 1)
                {
                    currentStage.AddEntity(new Collectible(X, Y, Game1.Scale, 2 * Game1.Scale, Game1.MiscTextures[12], currentStage,
                        ItemType.BadgeBitBig, true, new Vector2(Game1.RNG.Next(-18, 18), Game1.RNG.Next(-18, -5))));
                }
                else if(randomBitChance == 2)
                {
                    currentStage.AddEntity(new Collectible(X, Y, Game1.Scale, Game1.Scale, Game1.MiscTextures[8], currentStage,
                        ItemType.HappySkull, true, new Vector2(Game1.RNG.Next(-18, 18), Game1.RNG.Next(-18, -5))));
                }
            }

            //if health reaches 0, enemy dies!
            if (health <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Specific behaviors for each state is controlled in child classes, but this method controls cooldown timers and such
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public override void Update(GameTime gameTime)
        {
            //updating the invinciblity timer and its blink timer
            if (invincibilityTimer < invincibilityTimerMax)
            {
                invincibilityTimer += gameTime.ElapsedGameTime.TotalSeconds;
                invincibilityBlinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
            }

            //while stunned, restrain the enemy and count down on the stun timer
            if(stunTimer > 0)
            {
                currentState = EnemyState.Restrained;
                if (stunTimer - gameTime.ElapsedGameTime.TotalSeconds < 0)
                    stunTimer = 0;
                else
                    stunTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            //when the stun timer is over, return the enemy to the patrol mode
            else if(stunTimer == 0)
            {
                invincibilityTimer = 0;
                currentState = EnemyState.Patrol;
                patrolCoolDownTimer = 0;
                stunTimer--;
            }

            switch (currentState)
            {
                case (EnemyState.Patrol):
                    base.Update(gameTime);

                    //if they are cooldown from a recent attack, then count up on that timer
                    if (patrolCoolDownTimer < patrolCoolDownTimerMax)
                        patrolCoolDownTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    else
                    //if not cool down, check if the player is inside this enemies vision...
                        if (currentStage.MainPlayer.Transform.Intersects(visionHitbox))
                        //...if so, then make them become enraged/ready to attack
                        currentState = EnemyState.Enraged;

                    break;

                case (EnemyState.Idle):
                    //count up how much time left in this state
                    idleCoolDownTimer += gameTime.ElapsedGameTime.TotalSeconds;

                    base.Update(gameTime);

                    if (idleCoolDownTimer >= idleCoolDownTimerMax)
                    {
                        idleCoolDownTimer = 0;
                        currentState = EnemyState.Patrol;
                    }

                    break;

                case (EnemyState.Enraged):
                    base.Update(gameTime);

                    //if the player leaves the enemy's vision or the enemy is about to walk off the edge, reset the enemy to the patrol state
                    if (!currentStage.MainPlayer.Transform.Intersects(visionHitbox))
                    {
                        currentState = EnemyState.Patrol;
                        patrolCoolDownTimer = patrolCoolDownTimerMax/2.0;
                    }

                    break;


                case (EnemyState.Attack):
                    //count up how much time left in this state
                    attackCoolDownTimer += gameTime.ElapsedGameTime.TotalSeconds;

                    //when the attack state is done, unleash the attack and go into idle!
                    if (attackCoolDownTimer >= attackCoolDownTimerMax)
                    {
                        attackCoolDownTimer = 0;
                        currentState = EnemyState.Idle;
                        idleCoolDownTimer = 0;
                        DoAttack();
                    }

                    break;


                case (EnemyState.Knockback):
                    //enemy does nothing but default update stuff while being knockedback
                    base.Update(gameTime);
                    break;

                    //do nothing while restrained!
                case (EnemyState.Restrained):
                    break;

            }

            UpdateAnimations(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// update the current state's animation with the elapsed game time
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public virtual void UpdateAnimations(GameTime gameTime)
        {
            animations[(int)currentState].Update(gameTime);
        }

        /// <summary>
        /// Draw simply draws the current state's animation to the screen
        /// </summary>
        /// <param name="sb">the sprite batch to draw to</param>
        /// <param name="cameraOffset">the camera's offset in the world</param>
        public override void Draw(SpriteBatch sb, Vector2 cameraOffset)
        {
            //just draw the animation!
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

            if(drawAnimation && stunTimer >= 0)
                animations[(int)currentState].Draw(sb, transform, cameraOffset, Color.Goldenrod, facingLeft);
            else if (drawAnimation)
                animations[(int) currentState].Draw(sb, transform, cameraOffset, Color.White, facingLeft);
        }

        /// <summary>
        /// if the enemy has been knocked back, when it hits the ground, go back to patorling
        /// </summary>
        /// <param name="other"></param>
        public override void CollideTop(GameObject other)
        {
            base.CollideTop(other);

            switch (currentState)
            {
                case (EnemyState.Knockback):
                    //hitting the top of solid, semi solid, damage, or trampoline platforms switches enemy back to patrol state
                    if (other.TypeOfCollision == CollisionType.Solid || other.TypeOfCollision == CollisionType.SemiSolid ||
                        other.TypeOfCollision == CollisionType.Damage)
                    {
                        currentState = EnemyState.Patrol;
                        patrolCoolDownTimer = 0;
                    }
                    break;
            }
        }


        /// <summary>
        /// checking if the enemy is about to walk off the platform they are on
        /// </summary>
        /// <returns>Returns true if the enemy is about to walk off the edge</returns>
        public bool CheckForPlatformEdge()
        {
            if (!onGround)
                return false;

            bool notOnPlatform = true;
            //if they are facing left...
            if (facingLeft)
            {
                //...and their bottom left corner of their hitbox is not within the platform they are standing on...
                Point bottomLeftCorner = new Point(transform.Left, transform.Bottom + 2);
                foreach (GameObject platform in currentStage.Platforms)
                {

                    if (platform.Transform.Contains(bottomLeftCorner))
                    {
                        //...then they must be about to walk off the edge.
                        notOnPlatform = false;
                    }
                }
            }

            //if they are facing right...
            else
            {
                Point bottomRightCorner = new Point(transform.Right, transform.Bottom + 2);
                //...and their bottom right corner of their hitbox is not within the platform they are standing on...
                foreach (GameObject platform in currentStage.Platforms)
                {
                    //...and their bottom left corner of their hitbox is not within the platform they are standing on...
                    if (platform.Transform.Contains(bottomRightCorner))
                    {
                        //...then they must be about to walk off the edge.
                        notOnPlatform = false;
                    }
                }
            }

            return notOnPlatform;
        }
    }
}
