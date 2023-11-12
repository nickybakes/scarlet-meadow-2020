using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/14/2020
//Scarlet Meadow monogame project
//this controls the player on screen, manages stats, and gets their inputs
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// determines what behavior the player should be in dpending on their current state
    /// </summary>
    public enum PlayerState
    {
        Idle, //while th eplayer is on the ground pressing nothing
        Run, //while the player is on the ground and pressing A or D
        Jump, //in air while Y velocity is negative (going up)
        Fall, //in air while Y velocity is positive (going down)
        Charge, //when the player is charging up their main attack
        Attack, //The frames that the player is unleashing an attack on
        Melee, //the frames that the player is doing a melee attack on
        Knockback, //when the player is being knocked back from a damage source
        Zipline, //when the player is zoomin by hangin on a zipline
        Restrained //when the player is  restrained by another source and is unable to do actions
    }

    /// <summary>
    /// this controls the player on screen, manages stats, and gets their inputs
    /// </summary>
    public class Player : GameObject
    {

        /// <summary>
        /// the current state to display for the player, gotten by the CharacterStats
        /// </summary>
        protected PlayerState currentState;

        /// <summary>
        /// the name of the character
        /// </summary>
        private string name;

        /// <summary>
        /// When the player is pressing a directional key, they move at this speed. Value is determined by the Player's child classes
        /// </summary>
        protected int maxRunSpeed;

        /// <summary>
        /// When the player presses space, make them jump at negative this velocity. Value is determined by the Player's child classes
        /// </summary>
        protected int jumpHeight;

        /// <summary>
        /// the animations for this character, use CharacterAnim enum for this
        /// </summary>
        private Animation[] animations;

        /// <summary>
        /// the health of the player, out of 100
        /// </summary>
        protected int health;

        /// <summary>
        /// The normal amount of damage this character does when attacking
        /// </summary>
        protected int damageOutput;

        /// <summary>
        /// How much this player's special meter is filled. Up to 100
        /// </summary>
        protected int specialMeterFill;

        /// <summary>
        /// how much time is currently left until the player is done their main attack
        /// </summary>
        protected double attackCoolDown;

        /// <summary>
        /// maximum amount of time the player has to wait until they are done their main attack
        /// </summary>
        protected const double attackCoolDownMax = .25;

        /// <summary>
        /// how much time is currently left until the player is done their melee attack
        /// </summary>
        protected double meleeCoolDown;

        /// <summary>
        /// maximum amount of time the player has to wait until they are done their melee attack
        /// </summary>
        protected const double meleeCoolDownMax = .35;

        /// <summary>
        /// True if the player is able to start charging their main attack
        /// (possible false: if the lasso has not returned to scarlet yet)
        /// </summary>
        protected bool canChargeAttack;

        /// <summary>
        /// True if the player is currently holding down the charge button and is able to charge
        /// </summary>
        protected bool isCharging;

        /// <summary>
        /// how much time is currently left until the player blinks red while charging
        /// </summary>
        protected double chargeBlinkTimer;

        /// <summary>
        /// maximum amount of time the between blinking red while charging
        /// </summary>
        protected const double chargeBlinkTimerMax = .25;

        /// <summary>
        /// How much the player has charged up their main attack
        /// </summary>
        protected int charge;

        /// <summary>
        /// The maximum amount the player can charge up (set in player child classes)
        /// </summary>
        protected int maxCharge;

        /// <summary>
        /// The minimum amount the player can charge up (set in player child classes)
        /// </summary>
        protected int baseCharge;

        /// <summary>
        /// how much time is currently left until the player can take damage again
        /// </summary>
        protected double invincibilityTimer;

        /// <summary>
        /// maximum amount of time enemies have to wait until this player can take damage again
        /// </summary>
        protected const double invincibilityTimerMax = 3;

        /// <summary>
        /// how much time is currently left until the player blinks invisible while invincible
        /// </summary>
        protected double invincibilityBlinkTimer;

        /// <summary>
        /// maximum amount of time the between blinking invisible while invincible
        /// </summary>
        protected const double invincibilityBlinkTimerMax = .18;

        /// <summary>
        /// When greater than 0, the player's movement speed is much higher than normal
        /// </summary>
        protected double speedBuffTimer;

        /// <summary>
        /// When greater than 0, the player's attack power is greater than normal
        /// </summary>
        protected double attackBuffTimer;

        /// <summary>
        /// True when the player is on SOLID ground only. Used for camera offset position
        /// </summary>
        private bool onSolidGround;

        /// <summary>
        /// The amount of time, in seconds, the player has been on solid ground. Used for camera offset position
        /// </summary>
        private double timeOnSolidGround;

        /// <summary>
        /// True if the player has not yet taken any damage!
        /// </summary>
        private bool noDamage;

        /// <summary>
        /// True if the player can jump. Set it to false if they select "resume" from the pause menu so
        /// that the player doesn't jump directly after pressing A to select "resume"
        /// </summary>
        private bool canJump;

        /// <summary>
        /// Get the current health of the player
        /// </summary>
        public int Health
        {
            get { return health; }
        }

        /// <summary>
        /// Get the amount of special meter this play has filled
        /// </summary>
        public int SpecialMeterFill
        {
            get { return specialMeterFill; }
        }

        /// <summary>
        /// the current state of the player
        /// </summary>
        public PlayerState State
        {
            get { return currentState; }
            set { currentState = value; }
        }

        /// <summary>
        /// Returns true if the player has invincibility blinking
        /// </summary>
        public bool Invincible
        {
            get { return invincibilityTimer > 0; }
        }

        /// <summary>
        /// Gte the jump height of this player
        /// </summary>
        public int JumpHeight
        {
            get { return jumpHeight; }
        }

        /// <summary>
        /// Get the percentage of the main attack charge that the player has built up
        /// </summary>
        public double ChargePercentage
        {
            get { return (double)(charge - baseCharge)/ (double)(maxCharge - baseCharge); }
        }

        /// <summary>
        /// How much time, in seconds, the player has been on solid ground DIVIDED by the maximum camera scroll time
        /// </summary>
        public double TimeSpentOnSolidGroundPercent
        {
            get { return timeOnSolidGround/1.0; }
        }

        /// <summary>
        /// Gets the current max speed of the player, with buffs/effects
        /// </summary>
        public int CurrentMaxSpeed
        {
            get
            {
                if (speedBuffTimer > 0)
                    return maxRunSpeed + 6;
                return maxRunSpeed;
            }
        }

        /// <summary>
        /// Gets the current damage output of the player, with buffs/effects
        /// </summary>
        public int CurrentDamageOutput
        {
            get
            {
                if (attackBuffTimer > 0)
                    return damageOutput + 10;
                return damageOutput;
            }
        }

        /// <summary>
        /// Returns true if the player never took damage
        /// </summary>
        public bool NoDamage
        {
            get { return noDamage; }
        }

        /// <summary>
        /// True if the player can jump. Set it to false if they select "resume" from the pause menu so
        /// that the player doesn't jump directly after pressing A to select "resume"
        /// </summary>
        public bool CanJump
        {
            set { canJump = value; }
        }

        /// <summary>
        /// constructor for the player, sets data for the player
        /// </summary>
        /// <param name="x">starting X coord</param>
        /// <param name="y">starting Y coord</param>
        /// <param name="width">width of this objects hitbox</param>
        /// <param name="height">height of this objects hitbox</param>
        /// <param name="animations">the array of animations for this character</param>
        /// <param name="gravity">the amount that gravity affects this character</param>
        /// <param name="currentStage">the stage this character is in</param>
        public Player(int x, int y, int width, int height, Animation[] animations, int gravity, Stage currentStage)
            : base(x, y, width, height, null, gravity, currentStage)
        {
            //starts on the idle animation/state
            currentState = PlayerState.Idle;
            this.animations = animations;
            health = 100;
            canChargeAttack = true;
            noDamage = true;
        }

        /// <summary>
        /// What to do when the player is charging up their main attack
        /// </summary>
        public virtual void ChargeAttack()
        {

        }

        /// <summary>
        /// What the player's main attack does.
        /// </summary>
        /// <param name="direction">The direction that this attack is aimed toward</param>
        public virtual void DoAttack(Direction direction)
        {

        }


        /// <summary>
        /// When the player decides to use their special move, call this method!
        /// </summary>
        public virtual void SpecialMove()
        {
            specialMeterFill = 0;
            currentStage.SpecialMoveCutscene();
        }

        /// <summary>
        /// Causes damage to the player and activates the player's "invincibility blinking"
        /// </summary>
        /// <param name="damageAmount">The amount to damage the player by</param>
        public void Damage(int damageAmount)
        {
            //if the player is not invinsible
            if (!Invincible)
            {
                //do damage and start the invinsible timer
                invincibilityTimer = invincibilityTimerMax;
                if (health - damageAmount <= 0)
                    health = 0;
                else
                    health -= damageAmount;
                noDamage = false;
            }

            if(health == 0)
            {
                Game1.LoseStage("You ran out of health!");
            }
        }

        /// <summary>
        /// Sets health to 0 so it displays the tombstone sprite for the player
        /// </summary>
        public void Kill()
        {
            health = 0;
        }

        /// <summary>
        /// Heals the player up to 100 health, health cannot go over 100
        /// </summary>
        /// <param name="healAmount">the amount to add to the player's health</param>
        public void Heal(int healAmount)
        {
            if (health + healAmount >= 100)
                health = 100;
            else
                health += healAmount;
        }

        /// <summary>
        /// fills the player's special meter up to 100 health, special meter cannot go over 100
        /// </summary>
        /// <param name="fillAmount">the amount to add to the player's specialMeter</param>
        public void FillSpecialMeter(int fillAmount)
        {
            if (specialMeterFill + fillAmount >= 100)
                specialMeterFill = 100;
            else
                specialMeterFill += fillAmount;
        }

        /// <summary>
        /// This increases the speed buff timer (used for hot jalepeno item)
        /// </summary>
        public void IncreaseSpeedBuffTimer()
        {
            speedBuffTimer += 5;
        }

        /// <summary>
        /// This increases the attack buff timer (used for luchador mask item)
        /// </summary>
        public void IncreaseAttackBuffTimer()
        {
            attackBuffTimer += 5;
        }

        /// <summary>
        /// This increases the invincibility timer (used also for luchador mask item)
        /// </summary>
        public void IncreaseInvincibilityTimer()
        {
            invincibilityTimer += 5;
        }


        /// <summary>
        /// Causes this player to be launched in a direction and switched to the knockback state
        /// </summary>
        /// <param name="knockbackVelocity">the speed an direction to send the player toward</param>
        public void Knockback(Vector2 knockbackVelocity)
        {
            if (!Invincible)
            {
                //getting hit to the right should make the player face left when being knocked back
                if (knockbackVelocity.X > 0)
                    facingLeft = true;
                //getting hit to the left should make the player face right when being knocked back
                else if (knockbackVelocity.X < 0)
                    facingLeft = false;

                //reset any charging that is accuring
                isCharging = false;

                //minor adjustment to position
                Y -= 3;

                //apply the velocity and state
                this.velocity = knockbackVelocity;
                currentState = PlayerState.Knockback;
            }
        }

        public override void Update(GameTime gameTime)
        {

            //if the player is currently charing...
            if (isCharging)
            {
                //then actually charge the attack (controlled in Player's child classes)
                ChargeAttack();

                //make the character blink
                chargeBlinkTimer += gameTime.ElapsedGameTime.TotalSeconds;


                //when the player releases the Left arrow key, do the attack!
                if (!Game1.ActionPressed(Action.Attack))
                {
                    attackCoolDown = 0;
                    isCharging = false;
                    currentState = PlayerState.Attack;

                    //by default, aim the attack to where ever the player is facing
                    Direction attackAim = Direction.East;
                    if (facingLeft)
                        attackAim = Direction.West;

                    //if the player is pressing a diagonal button (Up/Down and left/right) then aim them that direction
                    if(Game1.ActionPressed(Action.MoveRight) && Game1.ActionPressed(Action.MoveUp))
                        attackAim = Direction.NorthEast;
                    else if (Game1.ActionPressed(Action.MoveLeft) && Game1.ActionPressed(Action.MoveUp))
                        attackAim = Direction.NorthWest;
                    else if (Game1.ActionPressed(Action.MoveRight) && Game1.ActionPressed(Action.MoveDown))
                        attackAim = Direction.SouthEast;
                    else if (Game1.ActionPressed(Action.MoveLeft) && Game1.ActionPressed(Action.MoveDown))
                        attackAim = Direction.SouthWest;
                    //or if they are just aiming up or down, aim them up or down!
                    else if (Game1.ActionPressed(Action.MoveUp))
                        attackAim = Direction.North;
                    else if (Game1.ActionPressed(Action.MoveDown))
                        attackAim = Direction.South;

                    //finally do the attack in the correct direction!
                    DoAttack(attackAim);
                }
            }
            //if the player is not charging, then set their current charge to the base charge amount
            else
            {
                charge = baseCharge;
            }

            //count down on the invincibility timers
            if (Invincible)
            {
                invincibilityTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                invincibilityBlinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
            }

            //if the player has speed buff, then increase their current movement speed and count down on the timer
            if (speedBuffTimer > 0)
            {
                speedBuffTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }


            //if the player has attack buff, then increase their current damage output and count down on the timer
            if (attackBuffTimer > 0)
            {
                attackBuffTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (!Game1.ActionPressed(Action.Jump))
            {
                canJump = true;
            }

            //player has different behavior while in different states
            switch (currentState)
            {
                case (PlayerState.Idle):
                    base.Update(gameTime);

                    if (Game1.ActionPressedThisFrame(Action.Interact))
                        currentStage.InteractWithStage(this);

                    if (currentState == PlayerState.Zipline)
                        break;

                    //pressing A faces left and switches to Run state
                    if (Game1.ActionPressed(Action.MoveLeft))
                    {
                        currentState = PlayerState.Run;
                        facingLeft = true;
                    }

                    //pressing D faces right and switches to Run state
                    if (Game1.ActionPressed(Action.MoveRight))
                    {
                        currentState = PlayerState.Run;
                        facingLeft = false;
                    }

                    //player can charge an attack, do a melee attack, do special move, or jump while idling
                    if (Game1.ActionPressed(Action.Attack) && canChargeAttack)
                        isCharging = true;

                    if (Game1.ActionPressedThisFrame(Action.Melee))
                        currentState = PlayerState.Melee;

                    if (Game1.ActionPressedThisFrame(Action.Special) && specialMeterFill == 100)
                        SpecialMove();

                    if (onGround && Game1.ActionPressed(Action.Jump) && canJump)
                    {
                        VelocityY = -jumpHeight;
                        onGround = false;
                    }

                    if(VelocityY < 0)
                        currentState = PlayerState.Jump;
                    else if (VelocityY > 0)
                        currentState = PlayerState.Fall;
                    break;

                case (PlayerState.Run):

                    //A and D switches to facing left or right respectively
                    if (Game1.ActionPressed(Action.MoveLeft))
                        facingLeft = true;
                    if (Game1.ActionPressed(Action.MoveRight))
                        facingLeft = false;

                    //in the run state, the player moves depending on what way they are facing
                    if (facingLeft)
                        velocity.X = -CurrentMaxSpeed;
                    else
                        velocity.X = CurrentMaxSpeed;

                    base.Update(gameTime);

                    if (Game1.ActionPressedThisFrame(Action.Interact))
                        currentStage.InteractWithStage(this);

                    if (currentState == PlayerState.Zipline)
                        break;

                    //releasing both A and D keys returns to idle state
                    if (!Game1.ActionPressed(Action.MoveLeft) && !Game1.ActionPressed(Action.MoveRight))
                        currentState = PlayerState.Idle;

                    //player can charge an attack, do special move, or do a melee attack while running
                    if (Game1.ActionPressed(Action.Attack) && canChargeAttack)
                        isCharging = true;

                    if (Game1.ActionPressedThisFrame(Action.Special) && specialMeterFill == 100)
                        SpecialMove();

                    if (Game1.ActionPressedThisFrame(Action.Melee))
                        currentState = PlayerState.Melee;

                    //pressing Space while on the ground gives the player a velocity upwards
                    if (onGround && Game1.ActionPressed(Action.Jump) && canJump)
                    {
                        VelocityY = -jumpHeight;
                        onGround = false;
                    }

                    //if velocity is negative (position going north) or positive (position going south), change to Jump or Fall respectively
                    if (VelocityY < 0)
                        currentState = PlayerState.Jump;
                    else if (VelocityY > 0)
                        currentState = PlayerState.Fall;
                    break;

                case (PlayerState.Jump):
                    //player can change direction/affect movement when jumping
                    if (Game1.ActionPressed(Action.MoveLeft) && VelocityX > -CurrentMaxSpeed)
                    {
                        velocity.X = -CurrentMaxSpeed;
                        facingLeft = true;
                    }

                    if (Game1.ActionPressed(Action.MoveRight) && VelocityX < CurrentMaxSpeed)
                    {
                        velocity.X = CurrentMaxSpeed;
                        facingLeft = false;
                    }

                    base.Update(gameTime);

                    //player can zipline, charge an attack, special move, or melee when jumping
                    if (Game1.ActionPressedThisFrame(Action.Interact))
                        currentStage.InteractWithStage(this);

                    if (currentState == PlayerState.Zipline)
                        break;

                    if (Game1.ActionPressed(Action.Attack) && canChargeAttack)
                        isCharging = true;

                    if (Game1.ActionPressedThisFrame(Action.Special) && specialMeterFill == 100)
                        SpecialMove();

                    if (Game1.ActionPressedThisFrame(Action.Melee))
                        currentState = PlayerState.Melee;

                    //when the player lands, switch to idle
                    if (onGround)
                        currentState = PlayerState.Idle;

                    //when the player hits the peak of their jump and starts falling, switch to fall state
                    if (VelocityY > 0)
                        currentState = PlayerState.Fall;


                    break;

                case (PlayerState.Fall):
                    //if the player isn't already at the max leftward running speed and they are pressing A, put them that that speed
                    if (Game1.ActionPressed(Action.MoveLeft) && VelocityX > -CurrentMaxSpeed)
                    {
                        velocity.X = -CurrentMaxSpeed;
                        facingLeft = true;
                    }

                    //if the player isn't already at the max rightward running speed and they are pressing D, put them that that speed
                    if (Game1.ActionPressed(Action.MoveRight) && VelocityX < CurrentMaxSpeed)
                    {
                        velocity.X = CurrentMaxSpeed;
                        facingLeft = false;
                    }

                    //they can charge an attack in the air
                    if (Game1.ActionPressed(Action.Attack) && canChargeAttack)
                        isCharging = true;

                    if (Game1.ActionPressedThisFrame(Action.Melee))
                        currentState = PlayerState.Melee;

                    base.Update(gameTime);

                    //player can latch to a zipline when in the air
                    if (Game1.ActionPressedThisFrame(Action.Interact))
                        currentStage.InteractWithStage(this);

                    if (currentState == PlayerState.Zipline)
                        break;

                    //player can special move in the air
                    if (Game1.ActionPressedThisFrame(Action.Special) && specialMeterFill == 100)
                        SpecialMove();

                    //when the player lands, switch to idle
                    if (onGround)
                        currentState = PlayerState.Idle;

                    //if they player gets launched back upwards, put them back in Jump state
                    if (VelocityY < 0)
                        currentState = PlayerState.Jump;

                    break;

                case (PlayerState.Attack):
                    attackCoolDown += gameTime.ElapsedGameTime.TotalSeconds;

                    //player can affect their movement when attacking
                    if (Game1.ActionPressed(Action.MoveLeft) && VelocityX > -CurrentMaxSpeed)
                    {
                        velocity.X = -CurrentMaxSpeed;
                        facingLeft = true;
                    }

                    if (Game1.ActionPressed(Action.MoveRight) && VelocityX < CurrentMaxSpeed)
                    {
                        velocity.X = CurrentMaxSpeed;
                        facingLeft = false;
                    }

                    //player can jump when attacking
                    if (onGround && Game1.ActionPressed(Action.Jump) && canJump)
                    {
                        VelocityY = -jumpHeight;
                        onGround = false;
                    }

                    base.Update(gameTime);

                    //player can zipline, special move, and melee when main attacking
                    if (Game1.ActionPressedThisFrame(Action.Interact))
                        currentStage.InteractWithStage(this);
                    if (currentState == PlayerState.Zipline)
                        break;

                    if (Game1.ActionPressedThisFrame(Action.Special) && specialMeterFill == 100)
                        SpecialMove();

                    //return to idle after attacking
                    if (attackCoolDown >= attackCoolDownMax)
                        currentState = PlayerState.Idle;

                    if (Game1.ActionPressedThisFrame(Action.Melee))
                        currentState = PlayerState.Melee;

                    break;

                //when melee attacking, create a hitbox to the left or right of the player that damages any enemies inside
                case (PlayerState.Melee):
                    meleeCoolDown += gameTime.ElapsedGameTime.TotalSeconds;

                    //the player can affect their movement while in the air while meleeing
                    if (!onGround && Game1.ActionPressed(Action.MoveLeft) && VelocityX > -CurrentMaxSpeed)
                    {
                        velocity.X = -CurrentMaxSpeed;
                        facingLeft = true;
                    }

                    if (!onGround && Game1.ActionPressed(Action.MoveRight) && VelocityX < CurrentMaxSpeed)
                    {
                        velocity.X = CurrentMaxSpeed;
                        facingLeft = false;
                    }

                    //if the player is facing left...
                    if (facingLeft)
                    {
                        //create a hitbox to the left
                        Rectangle hitbox = new Rectangle(transform.X - (int)(1.5 * Game1.Scale), transform.Y, transform.Width + (int)(1.5*Game1.Scale), transform.Height);
                        foreach(GameObject gameObject in currentStage.Entities)
                        {
                            //and damage any enemies inside of it
                            if(gameObject is Enemy && gameObject.Transform.Intersects(hitbox) && ((Enemy)gameObject).CanBeAttacked())
                            {
                                gameObject.Y -= 3;
                                ((Enemy)gameObject).DamageKnockback(CurrentDamageOutput, new Vector2(Game1.RNG.Next(-20, -15), Game1.RNG.Next(-20, -15)));
                                currentStage.DoImpactTime();
                            }
                        }
                    }
                    //if the player is facing right...
                    else
                    {
                        //create a hitbox to the right
                        Rectangle hitbox = new Rectangle(transform.Right, transform.Y, transform.Width + (int)(1.5 * Game1.Scale), transform.Height);
                        foreach (GameObject gameObject in currentStage.Entities)
                        {
                            //and damage any enemies inside of it
                            if (gameObject is Enemy && gameObject.Transform.Intersects(hitbox) && ((Enemy)gameObject).CanBeAttacked())
                            {
                                gameObject.Y -= 3;
                                ((Enemy)gameObject).DamageKnockback(CurrentDamageOutput, new Vector2(Game1.RNG.Next(15, 20), Game1.RNG.Next(-20, -15)));
                                currentStage.DoImpactTime();
                            }
                        }
                    }


                    base.Update(gameTime);

                    //the player can try to zipline while meleeing
                    if (Game1.ActionPressedThisFrame(Action.Interact))
                        currentStage.InteractWithStage(this);
                    if (currentState == PlayerState.Zipline)
                        break;

                    //player can also special move
                    if (Game1.ActionPressedThisFrame(Action.Special) && specialMeterFill == 100)
                        SpecialMove();

                    //once melee timer has ran out, change them back to the idle state
                    if (meleeCoolDown >= meleeCoolDownMax)
                    {
                        currentState = PlayerState.Idle;
                        meleeCoolDown = 0;
                    }

                    break;

                //cannot charge while knockedback
                case (PlayerState.Knockback):
                    isCharging = false;
                    base.Update(gameTime);
                    break;

                //can jump out of zipline or connect to a different one or use special move while ziplining
                case (PlayerState.Zipline):
                    if (Game1.ActionPressedThisFrame(Action.Interact))
                        currentStage.InteractWithStage(this);
                    if (Game1.ActionPressed(Action.Jump) && canJump)
                        currentState = PlayerState.Jump;
                    if (Game1.ActionPressedThisFrame(Action.Special) && specialMeterFill == 100)
                        SpecialMove();
                    onSolidGround = false;
                    break;

                //cannot charge while restrained
                case (PlayerState.Restrained):
                    isCharging = false;
                    break;
            }

            if (!onGround)
                onSolidGround = false;

            if (onSolidGround && timeOnSolidGround < 1)
            {
                timeOnSolidGround += gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (!onSolidGround && timeOnSolidGround > 0)
            {
                timeOnSolidGround -= gameTime.ElapsedGameTime.TotalSeconds;
            }

            //after all of the gameplay updates, update the currently displayed animation
            if (isCharging && currentState == PlayerState.Idle)
                //if we are showing the charge animation, make sure to update it
                animations[(int)PlayerState.Charge].Update(gameTime);
            else if(currentState != PlayerState.Restrained)
                //update the current animation
                animations[(int)currentState].Update(gameTime);

        }

        /// <summary>
        /// When the player collides with the top of a platform/trampoline, take them out of the knock back state
        /// </summary>
        /// <param name="other">the other object the player collided with</param>
        public override void CollideTop(GameObject other)
        {
            base.CollideTop(other);

            switch (currentState)
            {
                case (PlayerState.Knockback):
                    if (other.TypeOfCollision == CollisionType.Solid || other.TypeOfCollision == CollisionType.SemiSolid ||
                        other.TypeOfCollision == CollisionType.Damage || other is Trampoline)
                    {
                        currentState = PlayerState.Idle;
                    }
                    break;
            }

            onSolidGround = (other.TypeOfCollision == CollisionType.Solid);
        }

        public override void Draw(SpriteBatch sb, Vector2 cameraOffset)
        {
            //dont draw the player when they are restrained.
            //the only time they are ever restrained is when they are grabbed by an enemy, and in that event
            //the player's visual is in the enemy's animation
            if (currentState == PlayerState.Restrained)
                return;

            //if the player is dead, draw their tombstone
            if(health <= 0)
            {
                facingLeft = false;
                if(this is Scarlet)
                    texture = Game1.MiscTextures[20];
                else if (this is Cocoa)
                    texture = Game1.MiscTextures[21];

                base.Draw(sb, cameraOffset);
                return;
            }

            bool drawAnimation = true;
            if(Invincible)
            {
                //when its time to blink invisible while invincible, do so!
                if (invincibilityBlinkTimer >= invincibilityBlinkTimerMax)
                {
                    invincibilityBlinkTimer = 0;
                    drawAnimation = false;
                }
            }

            //if the sprite is not meant to be invisible, then draw it
            if(drawAnimation)
            {
                Color colorToDrawWith = Color.White;

                //color the player red when they eat a hit jalapeno
                if (speedBuffTimer > 0)
                    colorToDrawWith = Color.Tomato;

                //color the player blue when they have the attack buff (they have the luchador mask)
                if (attackBuffTimer > 0)
                    colorToDrawWith = Color.Aquamarine;

                //draw it to the screen affected by the camera offset, the animation's own offset, and its scale factor
                if (isCharging)
                {
                    //when its time to blink red while charging, do so!
                    if (chargeBlinkTimer >= chargeBlinkTimerMax)
                    {
                        chargeBlinkTimer = 0;
                        colorToDrawWith = new Color(255, 29, 97, 255);
                    }

                    //while charging and idle, draw the charge attack animation
                    if (currentState == PlayerState.Idle)
                        animations[(int)PlayerState.Charge].Draw(sb, transform, cameraOffset, colorToDrawWith, facingLeft);
                    else
                        //otherwise, just draw normal animations
                        animations[(int)currentState].Draw(sb, transform, cameraOffset, colorToDrawWith, facingLeft);
                }
                else
                {
                    //otherwise, otherwise, just draw normal animations
                    animations[(int)currentState].Draw(sb, transform, cameraOffset, colorToDrawWith, facingLeft);
                }
            }
        }
    }
}
