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
//When the player interacts with this, they will start sliding down the zipline very fast!
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// When the player interacts with this, they will start sliding down the zipline very fast!
    /// </summary>
    class Zipline : GameObject
    {

        /// <summary>
        /// Used to link up zipline objects for continious ziplining action
        /// </summary>
        private Zipline nextZipline;

        /// <summary>
        /// When the player is attached to the zipline, this references them
        /// </summary>
        private Player attachedPlayer;

        /// <summary>
        /// the left most end of the zip line
        /// </summary>
        private Point leftEnd;

        /// <summary>
        /// the right most end of the zip line
        /// </summary>
        private Point rightEnd;

        /// <summary>
        /// true if the zipline is sending the player toward the left
        /// </summary>
        private bool goingLeft;

        /// <summary>
        /// The slope of the line between both ends of the zipline
        /// </summary>
        private Vector2 slope;

        /// <summary>
        /// the dot product of the slope with itself. Equals the lines magintude squared, used for vector maths
        /// </summary>
        private float dotSlope;

        /// <summary>
        /// Used to set the next zipline object with the map is loading in
        /// </summary>
        public Zipline NextZipLine
        {
            set { nextZipline = value; }
        }

        /// <summary>
        /// Gets the left most end of line
        /// </summary>
        public Point LeftPoint
        {
            get { return leftEnd; }
        }

        /// <summary>
        /// Gets the right most end of the line
        /// </summary>
        public Point RightPoint
        {
            get { return rightEnd; }
        }

        /// <summary>
        /// Constructor stores given data like the end points and computes data like the slope and its dot product
        /// </summary>
        /// <param name="origin">the top left corner of this object</param>
        /// <param name="leftEnd">the location of the left most end of the zip line</param>
        /// <param name="rightEnd">the location of the right most end of the zip line</param>
        /// <param name="size">the overall size of the object</param>
        /// <param name="goingLeft">true if the zipline is sending the player left</param>
        /// <param name="texture">the texture of the zipline</param>
        /// <param name="currentStage">the stage the zipline is being added to</param>
        public Zipline(Point origin, Point leftEnd, Point rightEnd, Point size, bool goingLeft, Texture2D texture, Stage currentStage):
            base(origin.X, origin.Y, size.X, size.Y, texture, 0, currentStage)
        {
            this.collisionType = CollisionType.StaticEntity;
            this.leftEnd = leftEnd;
            this.rightEnd = rightEnd;
            this.goingLeft = goingLeft;

            //the direction/slope of our zipline
            this.slope = rightEnd.ToVector2() - leftEnd.ToVector2();

            this.dotSlope = Vector2.Dot(slope, slope);

        }

        /// <summary>
        /// If a player is attached, this method sends the player through the zipline!
        /// </summary>
        /// <param name="gameTime">GameTime for elapsed time since last frame</param>
        public override void Update(GameTime gameTime)
        {

            //if there is an attached player
            if(attachedPlayer != null)
            {
                //if the player jumps, unattach them from the zipline and send em flying!
                if(attachedPlayer.State != PlayerState.Zipline)
                {
                    attachedPlayer.VelocityY = -attachedPlayer.JumpHeight;
                    attachedPlayer.VelocityX = (int)(attachedPlayer.VelocityX * 1.3f);
                    attachedPlayer = null;
                    return;
                }

                //determing what part of the zipline the player is on
                Vector2 startToPlayer = attachedPlayer.PositionVector - this.PositionVector;
                float dotStartToSlope = Vector2.Dot(slope, startToPlayer);

                float pointOnLine = dotStartToSlope / dotSlope;


                //if its meant to be going to the right and they has not reached the end of the zipline...
                if (!goingLeft && pointOnLine < 1)
                {
                    //keep moving them down the zipline!
                    Vector2 playerDirection = new Vector2(slope.X, slope.Y);
                    playerDirection.Normalize();

                    playerDirection *= attachedPlayer.CurrentMaxSpeed + 20;

                    attachedPlayer.VelocityX = (int)playerDirection.X;
                    attachedPlayer.VelocityY = (int)playerDirection.Y;
                    attachedPlayer.X += (int)playerDirection.X;
                    attachedPlayer.Y += (int)playerDirection.Y;
                }
                //if the zipline is meant to be going to the left, and they havent reached the end...
                else if (goingLeft && pointOnLine >= 0)
                {
                    //keep moving them down the zipline!
                    Vector2 playerDirection = new Vector2(slope.X, slope.Y);
                    playerDirection.Normalize();

                    playerDirection *= -(attachedPlayer.CurrentMaxSpeed + 20);

                    attachedPlayer.VelocityX = (int)playerDirection.X;
                    attachedPlayer.VelocityY = (int)playerDirection.Y;
                    attachedPlayer.X += (int)playerDirection.X;
                    attachedPlayer.Y += (int)playerDirection.Y;
                }
                //if the player has reached the end of the zipline, and a "next" zipline exists...
                else if(nextZipline != null)
                {
                    //attach the player to this next zipline
                    Game1.debug = "got the next zipline";
                    attachedPlayer.VelocityX = 0;
                    attachedPlayer.VelocityY = 0;
                    nextZipline.attachedPlayer = attachedPlayer;
                    attachedPlayer = null;
                }
                //if the player reached the end and no "next" zipline exists, de-attach the player!
                else
                {
                    attachedPlayer.State = PlayerState.Fall;
                    attachedPlayer = null;
                }
            }
        }

        /// <summary>
        /// When the player tries to interact with this zipline, it determines the distance of the player from
        /// this line. If its close enough, it puts the player on the part of the line closest to them
        /// </summary>
        /// <param name="interactor">the thing interacting with this zipline</param>
        public override void Interact(GameObject interactor)
        {
            if(interactor is Player)
            {
                //cast it to player
                Player player = (Player)interactor;

                //if the player is not already in a zipline
                if(player.State != PlayerState.Zipline)
                {
                    //Vector W in our equation
                    Vector2 startToPlayer = player.PositionVector - this.PositionVector;

                    float dotStartToPlayer = Vector2.Dot(startToPlayer, startToPlayer);
                    float dotStartToSlope = Vector2.Dot(slope, startToPlayer);
                    
                    //utilizing vector maths to find the diatnce from the line to the player
                    float distanceFromLineToPlayer = dotStartToPlayer * (1 - (float)Math.Pow((dotStartToSlope / (slope.Length() * startToPlayer.Length())), 2));

                    Game1.debug = "not close enough";

                    //if the distance from the player to this line is short enough
                    if (distanceFromLineToPlayer < 25 * Game1.Scale * Game1.Scale)
                    {
                        //find the point on the line thats closest to the player
                        float pointOnLine = dotStartToSlope / dotSlope;

                        Game1.debug = pointOnLine.ToString();
                        if (pointOnLine >= -.4f && pointOnLine <= 1.3f)
                        {
                            attachedPlayer = player;
                            //limit the pointOnLine to 0 to 1
                            pointOnLine = Math.Max(0, Math.Min(1, pointOnLine));

                            //set the player's position to the point on the line they are closest to
                            player.PositionVector = leftEnd.ToVector2() + (pointOnLine*slope);

                            //adjusting the players position so they are visually closer on the zipline
                            if (goingLeft)
                            {
                                player.Y -= Game1.Scale;
                            }

                            //put them in the zipline state
                            attachedPlayer.State = PlayerState.Zipline;
                        }
                    }
                }
            }
        }
    }
}
