using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//4/1/2020
//Scarlet Meadow monogame project
//This is the Goal class, which represents a sort of "door" that opens when the user completesthe objective for that stage.
//Touching this when the goal is open completes the stage, showing the victory screen
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// This is the Goal class, which represents a sort of "door" that opens when the user completesthe objective for that stage.
    /// Touching this when the goal is open completes the stage, showing the victory screen
    /// </summary>
    class Goal : GameObject
    {


        /// <summary>
        /// When true, the player is allowed to touch it and complete the stage.
        /// Will be false if enemies still need to be defeated or civilians still need to be saved.
        /// </summary>
        private bool isOpened;

        /// <summary>
        /// Constructor sets the stage's current objective to what is in this constructor's parameters
        /// </summary>
        /// <param name="x">top left x position of this goal</param>
        /// <param name="y">top left y position of this goal</param>
        /// <param name="currentStage">the stage this is being added to</param>
        /// <param name="currentObjective">the starting objective for the stage</param>
        public Goal(int x, int y, Stage currentStage, GameObjective currentObjective)
        : base(x, y, Game1.Scale * 6, Game1.Scale * 6, null, 0, currentStage)
        {
            currentStage.CurrentObjective = currentObjective;
            currentStage.StartingObjective = currentObjective;
            currentStage.GoalPosition = this.PositionVector.ToPoint();
            this.collisionType = CollisionType.StaticEntity;
        }

        /// <summary>
        /// If the current objective is to get to the goal, then the goal should be opened.
        /// if enemies still need to be defeated or civilians still need to be saved, then the goal is closed.
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public override void Update(GameTime gameTime)
        {
            switch (currentStage.CurrentObjective)
            {
                //goal is always opened for these objectives
                case GameObjective.GetToEnd:
                case GameObjective.GetToEndUnderTimeLimit:
                    isOpened = true;
                    break;

                //goal is never opened for these. Once these objectives are completed, the main objective turns to "Get to End"
                case GameObjective.KillAllEnemies:
                case GameObjective.CollectAllObjectives:
                    isOpened = false;
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// When the player touches the goal, if it is opened, then win the game!
        /// </summary>
        public void TouchGoal()
        {
            if (isOpened)
            {
                Game1.CompleteStage();
                isOpened = false;
            }
        }

        /// <summary>
        /// When the player touches the goal, if it is opened, then win the game!
        /// </summary>
        /// <param name="other">game objective that collided with this one</param>
        public override void CollideTop(GameObject other)
        {
            if (other is Player)
                TouchGoal();
        }

        /// <summary>
        /// When the player touches the goal, if it is opened, then win the game!
        /// </summary>
        /// <param name="other">game objective that collided with this one</param>
        public override void CollideBottom(GameObject other)
        {
            if (other is Player)
                TouchGoal();
        }

        /// <summary>
        /// When the player touches the goal, if it is opened, then win the game!
        /// </summary>
        /// <param name="other">game objective that collided with this one</param>
        public override void CollideLeft(GameObject other)
        {
            if (other is Player)
                TouchGoal();
        }

        /// <summary>
        /// When the player touches the goal, if it is opened, then win the game!
        /// </summary>
        /// <param name="other">game objective that collided with this one</param>
        public override void CollideRight(GameObject other)
        {
            if (other is Player)
                TouchGoal();
        }

        /// <summary>
        /// Draws the currect texture if the goal is opened or not
        /// </summary>
        /// <param name="sb">sprite batch to draw to</param>
        /// <param name="cameraOffset">camera offset in world</param>
        public override void Draw(SpriteBatch sb, Vector2 cameraOffset)
        {
            if (isOpened)
                texture = Game1.MiscTextures[18];
            else
                texture = Game1.MiscTextures[19];

            base.Draw(sb, cameraOffset);
        }

    }

}
