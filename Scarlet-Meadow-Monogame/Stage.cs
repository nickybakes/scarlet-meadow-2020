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
//this stores what platforms and objects are in this stage/level
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// A stage has an objective that needs to be completed. This signifies with objective it is
    /// </summary>
    public enum GameObjective
    {
      GetToEnd, //Just get to the goal
      GetToEndUnderTimeLimit, //Get to the goal before time runs out
      KillAllEnemies, //Kill all enemies in the stage before getting to the goal
      CollectAllObjectives //Touch all objectives (Save all Civilians) before getting to the goal
    }

    /// <summary>
    /// this stores what platforms and objects are in this stage/level
    /// </summary>
    public class Stage
    {
        /// <summary>
        /// stores all of the static platforms and their collision data in this level
        /// </summary>
        private List<GameObject> platforms;

        /// <summary>
        /// stores all of the dynamic objects in this level
        /// </summary>
        private List<GameObject> entities;

        /// <summary>
        /// List of game objects to add to the list of entities at the end of the frame
        /// </summary>
        private List<GameObject> entitiesToAdd;

        /// <summary>
        /// List of game objects to remove from the list of entities at the end of the frame
        /// </summary>
        private List<GameObject> entitiesToRemove;

        /// <summary>
        /// the Player on the stage controled by the user
        /// </summary>
        private Player player;

        /// <summary>
        /// Layers of props to display for the stage
        /// 0 - background foliage
        /// 1 - front walls, indoor trims
        /// 2 - side walls, entrance back halfs, patches
        /// 3 - window fills, outdoor trims
        /// 4 - edges, windows
        /// 5 - graffiti
        /// 6 - floor, ground vines
        /// -----***  ENTITIES LAYER  ***-----
        /// 7 - entrance front halfs
        /// 8 - balconies, vine0.png, roofs
        /// </summary>
        private List<Prop>[] propLayers;

        /// <summary>
        /// Total amount of time the player has spent on the stage, in seconds.
        /// </summary>
        private double timeSpentOnStage;

        /// <summary>
        /// When a player activates their special move, there is a small cutscene that plays.
        /// This is how much time is left in the cutscene before we return to the actual game
        /// </summary>
        private double specialMoveCutsceneTime;

        /// <summary>
        /// The current objective the player is trying to complete. See GameObjective for more detail
        /// </summary>
        private GameObjective currentObjective;

        /// <summary>
        /// The objective the stage started with. Used to determine how far the player is from completing the stage.
        /// </summary>
        private GameObjective startingObjective;

        /// <summary>
        /// The amount of time the player is given to complete the stage under the "get to goal under time limit" objective
        /// </summary>
        private double timeLeftMax;

        /// <summary>
        /// The total amount of enemies on the stage when the stage is started
        /// </summary>
        private int totalEnemyAmount;

        /// <summary>
        /// The total amount of enemies that have been defeated while the stage is playing
        /// </summary>
        private int enemiesDefeated;

        /// <summary>
        /// The total amount of civilians on the stage when the stage is started
        /// </summary>
        private int totalCivilianCount;

        /// <summary>
        /// The total amount of civilians that have been saved/removed from the stage while the stage is playing
        /// </summary>
        private int civiliansSaved;

        /// <summary>
        /// When the player lands an attack, we freeze the game for a split second to make
        /// everything feel a bit more impactful and crisp.
        /// This is how much time is left until the game unfreezes
        /// </summary>
        private double impactPauseTimer;

        /// <summary>
        /// the position and height and width of the stage
        /// </summary>
        private Rectangle stageTransform;

        /// <summary>
        /// The spawnpoint of the player, used to determine how far the player is from completing the stage.
        /// </summary>
        private Point playerSpawn;

        /// <summary>
        /// The location of the goal that the player needs to touch, used to determine how far the player is from completing the stage.
        /// </summary>
        private Point goalPosition;

        /// <summary>
        /// True if this is being used as a background for the title screen/menu.
        /// Makes it so this stage does not bother updating entities because it wouldn't have any
        /// </summary>
        private bool titleBackgroundStage;

        /// <summary>
        /// Stores which background to display in the stage (etc a night time background, day time, etc)
        /// </summary>
        public Texture2D[] BackgroundLayers
        {
            get; set;
        }

        /// <summary>
        /// Gets how much time is left on the stage
        /// </summary>
        public double TimeLeft
        {
            get { return timeLeftMax - timeSpentOnStage; }
        }

        /// <summary>
        /// Returns true if the player has not killed any enemies
        /// </summary>
        public bool Pacifist
        {
            get { return (enemiesDefeated == 0); }
        }

        /// <summary>
        /// Returns true if the player has killed all enemies on the stage
        /// </summary>
        public bool Vigilante
        {
            get 
            {
                if (totalEnemyAmount == 0)
                    return false;
                return (enemiesDefeated == totalEnemyAmount); 
            }
        }

        /// <summary>
        /// Gets a formatted string showing how many enemies have been defeated
        /// </summary>
        public string EnemyObjectiveString
        {
            get { return enemiesDefeated + "/" + totalEnemyAmount; }
        }

        /// <summary>
        /// Gets a formatted string showing how many civilians have been saved
        /// </summary>
        public string CivilianObjectiveString
        {
            get { return civiliansSaved + "/" + totalCivilianCount; }
        }

        /// <summary>
        /// Get and set the list of platform collision objects in this stage
        /// </summary>
        public List<GameObject> Platforms
        {
            get { return platforms; }
            set { platforms = value; }
        }

        /// <summary>
        /// Get and Set the list of entities currently present in this stage
        /// </summary>
        public List<GameObject> Entities
        {
            get { return entities; }
            set { entities = value; }
        }

        /// <summary>
        /// Get the lsit of entities to add to this stage at the end of this frame
        /// </summary>
        public List<GameObject> EntitiesToAdd
        {
            get { return entitiesToAdd; }
        }

        /// <summary>
        /// Get and set the reference to the player on the stage
        /// </summary>
        public Player MainPlayer
        {
            get { return player; }
            set { player = value; }
        }

        /// <summary>
        /// Get the amount of time spent on this stage
        /// </summary>
        public Double TimeSpentOnStage
        {
            get { return timeSpentOnStage; }
        }

        /// <summary>
        /// Get the amount of time left in the cutscene for the special move
        /// </summary>
        public Double SpecialMoveCutsceneTime
        {
            get { return specialMoveCutsceneTime; }
        }

        /// <summary>
        /// Get the position, width, and height of the stage
        /// </summary>
        public Rectangle Transform
        {
            get { return stageTransform; }
        }

        /// <summary>
        /// Normal get for the current objective. Setting the objective resets the HUD Objective display timer
        /// so it displays the current/newly set objective to the player
        /// </summary>
        public GameObjective CurrentObjective
        {
            get { return currentObjective; }
            set { currentObjective = value;
                Game1.ObjectiveDisplayTimer = 3; }
        }

        /// <summary>
        /// The objective the stage started with. Used ot determine how far the player is from completing the stage
        /// </summary>
        public GameObjective StartingObjective
        {
            get { return startingObjective; }
            set { startingObjective = value; }
        }

        /// <summary>
        /// Set the spawnpoint of the player, used to determine how far the player is from completing the stage.
        /// </summary>
        public Point PlayerSpawn
        {
            set { playerSpawn = value; }
        }

        /// <summary>
        /// Set the location of the goal that the player needs to touch, used to determine how far the player is from completing the stage.
        /// </summary>
        public Point GoalPosition
        {
            set { goalPosition = value; }
        }

        /// <summary>
        /// Constructor initializes default values like the time limit, and also initializes our entity storage
        /// </summary>
        /// <param name="platforms">The list of gameobjects that have the collision data for the platforms and walls on the stage</param>
        /// <param name="propLayers">the lists of props on the stage, each list in the array is a separate layer</param>
        /// <param name="defaultBackgroundLayers">the default layers of textures to display in the background with a parallax scrolling effect</param>
        /// <param name="mapTransform">the position and height and width of the stage</param>
        public Stage(List<GameObject> platforms, List<Prop>[] propLayers, Texture2D[] defaultBackgroundLayers, Rectangle mapTransform)
        {
            //give each platform its "current stage" field
            foreach (GameObject platform in platforms)
            {
                platform.CurrentStage = this;
            }

            //default time limit starts at 20 seconds
            timeLeftMax = 20;

            BackgroundLayers = defaultBackgroundLayers;
            stageTransform = mapTransform;

            //storing our platforms and props, and initializing our entity storage
            this.platforms = platforms;
            this.propLayers = propLayers;
            entities = new List<GameObject>(256);
            entitiesToAdd = new List<GameObject>(32);
            entitiesToRemove = new List<GameObject>(32);

            titleBackgroundStage = false;
        }

        /// <summary>
        /// This automatically sets up an empty stage. 
        /// Used for the title stage that is rendered in the background of the menu
        /// </summary>
        public Stage()
        {
            titleBackgroundStage = true;
            stageTransform = new Rectangle(0, 0, 1920, 1080);
        }

        /// <summary>
        /// Adds a game object to the entities list of this stage.
        /// </summary>
        /// <param name="gameObject">The game object to add to the entities list</param>
        public void AddEntity(GameObject gameObject)
        {
            entitiesToAdd.Add(gameObject);
            gameObject.CurrentStage = this;

            //keep track of how many enemies and civilians are added to this stage
            if (gameObject is Enemy)
                totalEnemyAmount++;
            else if (gameObject is Civilian)
                totalCivilianCount++;
        }

        /// <summary>
        /// Removes a game object from the entities list of this stage.
        /// </summary>
        /// <param name="gameObject">The game object to remove from the entities list</param>
        public void RemoveEntity(GameObject gameObject)
        {
            //if we are removing an enemy or civilian, track that with the total amounts
            if (gameObject is Enemy)
                enemiesDefeated++;
            else if (gameObject is Civilian)
                civiliansSaved++;

            gameObject.Active = false;
            entitiesToRemove.Add(gameObject);
        }

        /// <summary>
        /// When loading in a stage, there is an entity that increases the time limit by 5 sec,
        /// so we can edit how much time is given for objectives under the time limit.
        /// Use this method for that
        /// </summary>
        /// <param name="amount">the amount to increase the time limit by</param>
        public void IncreaseTimeLimit(double amount)
        {
            timeLeftMax += amount;
        }

        /// <summary>
        /// When the player lands an attack, this pauses the game for a split second to make
        /// everything feel a bit more impactful and crisp
        /// </summary>
        public void DoImpactTime()
        {
            if(impactPauseTimer <= 0)
                impactPauseTimer = .1;
        }

        /// <summary>
        /// Updates all objects in the stage
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public void Update(GameTime gameTime)
        {
            //do not update anything on this stage if it is used as the background for the menu
            if (titleBackgroundStage)
                return;

            //if we are in a special move Cutscene...
            if(specialMoveCutsceneTime >= 0)
            {
                //...then count down on its timer
                specialMoveCutsceneTime -= gameTime.ElapsedGameTime.TotalSeconds;
                return;
                //and dont do anything else
            }

            //Keep counting up on the timeSpentOnStage even during impact time
            if (impactPauseTimer >= 0)
            {
                impactPauseTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                return;
            }

            //limit the time to 99 minutes, 99 seconds, 99 miliseconds
            if (timeSpentOnStage >= 5999.99)
                timeSpentOnStage = 5999.99;
            else
                timeSpentOnStage += gameTime.ElapsedGameTime.TotalSeconds;

            if (currentObjective == GameObjective.GetToEndUnderTimeLimit && TimeLeft <= 0)
                Game1.LoseStage("You ran out of time!");

            //go through each active entity, and update them! Update their position, state, check for collision, etc
            foreach (GameObject entity in entities)
            {
                //only update if its active!
                if (entity.Active)
                {
                    //if the entity is an enemy, then only update it if they are close to the player
                    if (entity is Enemy && Vector2.DistanceSquared(player.PositionVector, entity.PositionVector) <= Math.Pow(90 * Game1.Scale, 2))
                    {
                        entity.Update(gameTime);
                    }
                    //or update them if they are falling (this fixes a bug where an enemy would be about to fall off the stage, but then would get too far away from the player,
                    //so they would stop being updated and would never actually hit the bottom of the stage and die, thus making some "Kill All Enemies" stages uncompletable
                    else if (entity is Enemy && entity.VelocityY > 10)
                    {
                        entity.Update(gameTime);
                    }
                    else if(!(entity is Enemy))
                    {
                        entity.Update(gameTime);
                    }
                }


            }

            //After everything has been updated, go through the queue of entities needing to be added and add them to the stage!
            foreach(GameObject entityToAdd in entitiesToAdd)
            {
                entities.Add(entityToAdd);
            }
            entitiesToAdd.Clear();

            //After everything has been updated, go through the queue of entities needing to be removed and remove them from the stage!
            foreach (GameObject entityToRemove in entitiesToRemove)
            {
                entities.Remove(entityToRemove);
            }
            entitiesToRemove.Clear();


            //if our current objective is to kill all enemies
            if(CurrentObjective == GameObjective.KillAllEnemies)
            {
                //and we have completed that, then switch to just "get to end"
                if (enemiesDefeated >= totalEnemyAmount)
                    CurrentObjective = GameObjective.GetToEnd;
            }
            //if our current objective is to save civilians
            else if (CurrentObjective == GameObjective.CollectAllObjectives)
            {
                //and we have completed that, then switch to just "get to end"
                if (civiliansSaved >= totalCivilianCount)
                    CurrentObjective = GameObjective.GetToEnd;
            }

        }

        /// <summary>
        /// Checks if other entities in the stage are colliding with this one
        /// </summary>
        /// <param name="entity">the entity to check with all the others</param>
        public void CheckCollisionsWithOtherEntities(GameObject entity)
        {
            foreach (GameObject entity2 in entities)
            {
                if (entity2 != entity)
                {
                    entity2.CheckCollision(entity);
                }
            }
        }

        /// <summary>
        /// Checks if platforms in the stage are colliding with this entity
        /// </summary>
        /// <param name="entity">the entity to check with all the platforms</param>
        public void CheckCollisionsWithStage(GameObject entity)
        {
            //only check for collision if the entity is a dynamic one. Static ones dont need to collide with platforms
            if (entity.TypeOfCollision == CollisionType.DynamicEntity)
            {
                entity.OnGround = false;
                foreach (GameObject platform in platforms)
                {
                    platform.CheckCollision(entity);
                }
            }
        }

        /// <summary>
        /// Shows a HUD animation for the special move and pauses stage updates for a certain amount of time
        /// </summary>
        public void SpecialMoveCutscene()
        {
            specialMoveCutsceneTime = .8;
        }

        /// <summary>
        /// When the user presses the interact key, go through each object in the stage and attempt to interact with it
        /// </summary>
        public void InteractWithStage(GameObject interactor)
        {
            foreach(GameObject interactee in entities)
            {
                if (interactor != interactee)
                    interactee.Interact(interactor);
            }
        }

        /// <summary>
        /// Draws the background of the stage and then objects on the stage
        /// </summary>
        /// <param name="sb">the spritebatch to draw to</param>
        /// <param name="cameraOffset">the amount to offset each object's display position depending on the camera's position in the world</param>
        public void Draw(SpriteBatch sb, Vector2 cameraOffset)
        {
            for(int i = 0; i < BackgroundLayers.Length; i++)
            {
                DrawParallaxLayer(sb, cameraOffset, BackgroundLayers[i], i);
            }

            //only draw the parallax background if this is used for the menu background
            if (titleBackgroundStage)
                return;

            //draw all the props on layers 0-1 inclusive (should be behind the player)
            for (int i = 0; i < 7; i++)
            {
                foreach (Prop prop in propLayers[i])
                {
                    if(Vector2.DistanceSquared(player.PositionVector, prop.PositionVector) < Math.Pow(54 * Game1.Scale, 2))
                        prop.Draw(sb, cameraOffset);
                }
            }

            //go through each active entity in the stage and draw them!
            foreach (GameObject entity in entities)
            {
                if (!entity.Active || Vector2.DistanceSquared(player.PositionVector, entity.PositionVector) > Math.Pow(54 * Game1.Scale, 2))
                    continue;

                entity.Draw(sb, cameraOffset);
            }

            //draw the rest of the prop layers in front of the player
            for (int i = 7; i < 9; i++)
            {
                foreach (Prop prop in propLayers[i])
                {
                    if (Vector2.DistanceSquared(player.PositionVector, prop.PositionVector) < Math.Pow(54 * Game1.Scale, 2))
                        prop.Draw(sb, cameraOffset);
                }
            }

            //if the debug mode is on, then platforms' hitboxes get draw as well!
            foreach (GameObject platform in platforms)
            {
                platform.Draw(sb, cameraOffset);
            }
        }

        /// <summary>
        /// Draws one layer of a parallax background to the screen. This gives the background the illusion of depth
        /// by moving the layers further away from the camera less than the layers closer to the camera
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to draw to</param>
        /// <param name="cameraOffset">the camera's offset in the world</param>
        /// <param name="texture">the texture of the layer we are drawing</param>
        /// <param name="layer">the index of the layer that is being drawn (0 is the furthest away (the sky))</param>
        public void DrawParallaxLayer(SpriteBatch spriteBatch, Vector2 cameraOffset, Texture2D texture, int layer)
        {
            //first we get the position to draw the texture on the screen, 
            //offset by the camera's offset and scaled down to make it move less than the plane/layer of the player
            int startingXPosition = (int)(-cameraOffset.X / (1.5*(BackgroundLayers.Length - layer) + 1.2));


            //we need to shrink this position down to the closest it can be to the edge of the screen
            //so that we dont have to draw it lots of times off screen for larger camera offsets
            while (startingXPosition <= -texture.Width || startingXPosition > 0)
            {
                if (startingXPosition > 0)
                    startingXPosition -= texture.Width;
                else
                    startingXPosition += texture.Width;
            }

            //we start by getting the Y position;
            int yPos = (int)((stageTransform.Y - cameraOffset.Y) / (BackgroundLayers.Length - layer + 1.2));

            //because the texture do not tile Vertically, we need to scale them depending on how long the map is vertically
            int verticalTextureHeight = Math.Max(Game1.RenderTargetHeight, Math.Min(Game1.RenderTargetHeight * 2, stageTransform.Height / 2));


            //if this the background layer, then height should not affect its position
            if (layer == 0)
            {
                yPos = 0;
                verticalTextureHeight = Game1.RenderTargetHeight;
            }
            else
            {
                //but if it is not the background and height is affecting its position, then clamp it so the textures edges do not show
                yPos = Math.Max(Game1.RenderTargetHeight - verticalTextureHeight, Math.Min(verticalTextureHeight, (int)(verticalTextureHeight*((double)yPos/ (double)stageTransform.Height))));
            }

            //then we draw the texture to the screen as many times as we need until the screen is filled horizontally
            for (int i = startingXPosition; i <= Game1.RenderTargetWidth; i += texture.Width)
            {
                spriteBatch.Draw(texture, new Rectangle(i, yPos, texture.Width, verticalTextureHeight), Color.White);
            }
        }

        /// <summary>
        /// Based on the objective of the stage, this calculates the percentage of the stage that
        /// the player has completed. Used for Game Over/Defeat screen to show the player how close
        /// they were to finishing the stage!
        /// </summary>
        /// <param name="objective">the objective to calculate completion for</param>
        /// <returns>Returns the percentage of that objective that the player has completed</returns>
        public double GetCompletionPercentage(GameObjective objective)
        {
            double completionPercentage = 0;

            switch (objective)
            {
                case (GameObjective.GetToEnd):
                case (GameObjective.GetToEndUnderTimeLimit):
                    float distFromSpawn = Vector2.Distance(playerSpawn.ToVector2(), player.PositionVector);
                    float distFromGoal = Vector2.Distance(goalPosition.ToVector2(), player.PositionVector);

                    //if the player is closer to the spawn
                    if (distFromSpawn < distFromGoal)
                    {
                        //then calculate distance from spawn as the percentage
                        completionPercentage = (double)(distFromSpawn / (distFromSpawn + distFromGoal));
                    }
                    //otherwise, if they are closer to the goal
                    else
                    {
                        //then calculate distance from the goal as the percentage
                        completionPercentage = 1 - (double)(distFromGoal / (distFromSpawn + distFromGoal));
                    }

                    break;

                case (GameObjective.KillAllEnemies):
                    //completion percentage is based on objectives done
                    completionPercentage = .85 * ((double) enemiesDefeated / (double) totalEnemyAmount);

                    //if those objectives were finished, then add on to it the percentage based on distance from the goal point
                    if (enemiesDefeated == totalEnemyAmount)
                        completionPercentage += .15 * GetCompletionPercentage(GameObjective.GetToEnd);
                    break;

                case (GameObjective.CollectAllObjectives):
                    //completion percentage is based on objectives done
                    completionPercentage = .85 * ((double)civiliansSaved / (double)totalCivilianCount);

                    //if those objectives were finished, then add on to it the percentage based on distance from the goal point
                    if (civiliansSaved == totalCivilianCount)
                        completionPercentage += .15 * GetCompletionPercentage(GameObjective.GetToEnd);
                    break;
            }

            return completionPercentage;
        }
    }
}
