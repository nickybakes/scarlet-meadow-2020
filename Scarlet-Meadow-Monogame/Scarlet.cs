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
//holds the code for Scarlet specific actions, animations, and stats
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// holds the code for Scarlet specific actions, animations, and stats
    /// </summary>
    class Scarlet : Player
    {

        /// <summary>
        /// the current time left until Scarlet can throw her lasso again after she gets it back
        /// </summary>
        private double scarletCoolDown;

        /// <summary>
        /// the maximum amount of cooldown time before Scarlet can throw her lasso again after she gets it back
        /// </summary>
        private const double scarletCoolDownMax = .3;

        /// <summary>
        /// The lasso object that Scarlet is able to throw and control
        /// </summary>
        private Lasso lasso;

        /// <summary>
        /// Get and set for the lasso object that Scarlet is able to throw/control
        /// </summary>
        public Lasso ScarletLasso
        {
            get { return lasso; }
            set { lasso = value; }
        }

        /// <summary>
        /// Constructor sets up movement speed and other stats for a Scarlet player
        /// </summary>
        /// <param name="x">the starting x position</param>
        /// <param name="y">the starting y position</param>
        /// <param name="width">width of the player's hitbox</param>
        /// <param name="height">height of the player's hitbox</param>
        /// <param name="animations">Set of animations used by the player</param>
        /// <param name="gravity">how much gravity affects the player</param>
        /// <param name="currentStage">the stage the player is added to</param>
        public Scarlet(int x, int y, int width, int height, Animation[] animations, int gravity, Stage currentStage) 
            :base(x, y, width, height, animations, gravity, currentStage)
        {
            //store a new lasso object with a reference to this player
            lasso = new Lasso(this);

            //set up Scarlet specific statistics
            maxRunSpeed = 17;
            jumpHeight = 42;
            baseCharge = 25;
            maxCharge = 80;
            damageOutput = 20;
            charge = baseCharge;
            scarletCoolDown = scarletCoolDownMax;
        }

        /// <summary>
        /// When scarlet is charging her attack, she is increases the distance the lasso will go when thrown
        /// </summary>
        public override void ChargeAttack()
        {
            //only charge the attack if the lasso hasnt already been thrown
            if (!lasso.Thrown)
            {
                if (charge < maxCharge)
                    charge += 2;
                lasso.DistanceThrown = charge;
            }
        }

        /// <summary>
        /// When scarlet does her attack, she throws the lasso with the aimed idrection
        /// </summary>
        /// <param name="direction">the aimed direction to do the attack in</param>
        public override void DoAttack(Direction direction)
        {
            //only do the attack if the lasso hasnt already been thrown
            if (!lasso.Thrown)
            {
                lasso.Thrown = !lasso.Thrown;
                charge = baseCharge;
                lasso.Throw(direction);
                scarletCoolDown = 0;
            }
        }

        /// <summary>
        /// Scarlet's special move stuns all enemies on the stage. She yells "Stop in the name of the law!"
        /// </summary>
        public override void SpecialMove()
        {
            foreach(GameObject gameObject in currentStage.Entities)
            {
                if(gameObject is Enemy && Vector2.DistanceSquared(gameObject.PositionVector, this.PositionVector) < Math.Pow(60 * Game1.Scale, 2))
                {
                    ((Enemy)gameObject).Stun(7);
                }
            }
            base.SpecialMove();
        }

        /// <summary>
        /// Scarlet's update method also updates her lasso object
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public override void Update(GameTime gameTime)
        {
            //counts up on the cooldown timer
            if (scarletCoolDown < scarletCoolDownMax && !lasso.Thrown)
            {
                scarletCoolDown += gameTime.ElapsedGameTime.TotalSeconds;
                canChargeAttack = false;
            }
            //she cannot charge an attack if her lasso is still flying in the air
            else
            {
                canChargeAttack = !lasso.Thrown;
            }

            base.Update(gameTime);

            lasso.Update(gameTime);
        }

        /// <summary>
        /// Draws the lasso as well as scarlet!
        /// </summary>
        /// <param name="sb">the spritebatch to draw to</param>
        /// <param name="cameraOffset">the offset of the camera in the world</param>
        public override void Draw(SpriteBatch sb, Vector2 cameraOffset)
        {
            //draws the lasso
            lasso.Draw(sb, cameraOffset);

            //then draws the player's (Scarlet's) animations!
            base.Draw(sb, cameraOffset);

        }
    }
}
