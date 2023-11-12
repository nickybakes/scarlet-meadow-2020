using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//3/27/2020
//Scarlet Meadow monogame project
//A collectible is any item the player can collect. Touching the item will give a certain effect to the player
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// Determines which item this collectible is. Each one has its own behavior
    /// when the player collects it
    /// </summary>
    public enum ItemType
    {
        HappySkull, //heals 5 health
        ShockedSkull, //heals 25 health
        LuchadorMask, //increases attack power
        Jalapeno, //increases movement speed for 5 seconds
        BadgeBitBig, //fills 3 special points
        BadgeBitSmall //fills 1 special points
    }

    /// <summary>
    /// A collectible is any item the player can collect. Touching the item will give a certain effect to the player
    /// </summary>
    class Collectible : GameObject
    {
        /// <summary>
        /// True if the collectible should be affected by gravity,
        /// false if it should just be floating
        /// </summary>
        private bool hasPhysics;

        /// <summary>
        /// When an item spawns, there is about half a second of delay before the player can grab it.
        /// This is so the player can see what it is before picking it up instantly
        /// </summary>
        private double collectDelayTime;

        /// <summary>
        /// What type of item this is. Determines behavior when collected. See ItemType enum
        /// </summary>
        private ItemType itemType;

        /// <summary>
        /// Set if the collectible should be affected by gravity
        /// </summary>
        public bool HasPhysics
        {
            set { hasPhysics = value; }
        }

        /// <summary>
        /// Simple stores data about the item, position size, its item type, etc.
        /// </summary>
        /// <param name="x">starting x position</param>
        /// <param name="y">starting y position</param>
        /// <param name="width">width of the items hitbox</param>
        /// <param name="height">height of the items hitbox</param>
        /// <param name="texture">the texture to show for this item</param>
        /// <param name="currentStage">the stage this item is in</param>
        /// <param name="itemType">What type of item this is. Determines behavior when collected. See ItemType enum</param>
        /// <param name="hasPhysics">true if the item is affected by gravity</param>
        /// <param name="initialVelocity">Starting velocity of this item. Used to make it "pop out" of an enemy when killed</param>
        public Collectible(int x, int y, int width, int height, Texture2D texture, Stage currentStage, ItemType itemType, bool hasPhysics, Vector2 initialVelocity) 
            : base(x, y, width, height, texture, 0, currentStage)
        {
            this.itemType = itemType;
            this.hasPhysics = hasPhysics;
            this.velocity = initialVelocity;
            this.collectDelayTime = .6;
        }

        /// <summary>
        /// What this item does to the player when they collect it
        /// </summary>
        /// <param name="player">the player that touched this collectible</param>
        public void CollectItem(Player player)
        {

            switch (itemType)
            {
                case (ItemType.HappySkull)://heals 5 health
                    player.Heal(5);
                    break;

                case (ItemType.ShockedSkull)://heals 25 health
                    player.Heal(25);
                    break;

                case (ItemType.LuchadorMask)://increases attack power
                    player.IncreaseAttackBuffTimer();
                    break;

                case (ItemType.Jalapeno)://increases movement speed for 5 seconds
                    player.IncreaseSpeedBuffTimer();
                    break;

                case (ItemType.BadgeBitBig)://fills 3 special points
                    player.FillSpecialMeter(3);
                    break;

                case (ItemType.BadgeBitSmall)://fills 1 special points
                    player.FillSpecialMeter(1);
                    break;
            }

            currentStage.RemoveEntity(this);
        }

        /// <summary>
        /// Normal collision action, but if it collides with the player and it has surpassed the delay time, then "collect the item"
        /// </summary>
        /// <param name="other">the object this item collided with</param>
        public override void CollideBottom(GameObject other)
        {
            base.CollideBottom(other);
            if (other is Player && collectDelayTime <= 0)
                CollectItem((Player)other);
        }

        /// <summary>
        /// Normal collision action, but if it collides with the player and it has surpassed the delay time, then "collect the item"
        /// </summary>
        /// <param name="other">the object this item collided with</param>
        public override void CollideLeft(GameObject other)
        {
            base.CollideLeft(other);
            if (other is Player && collectDelayTime <= 0)
                CollectItem((Player)other);
        }

        /// <summary>
        /// Normal collision action, but if it collides with the player and it has surpassed the delay time, then "collect the item"
        /// </summary>
        /// <param name="other">the object this item collided with</param>
        public override void CollideRight(GameObject other)
        {
            base.CollideRight(other);
            if (other is Player && collectDelayTime <= 0)
                CollectItem((Player)other);
        }

        /// <summary>
        /// Normal collision action, but if it collides with the player and it has surpassed the delay time, then "collect the item"
        /// </summary>
        /// <param name="other">the object this item collided with</param>
        public override void CollideTop(GameObject other)
        {
            base.CollideTop(other);
            if (other is Player && collectDelayTime <= 0)
                CollectItem((Player)other);
        }

        /// <summary>
        /// counts down on the delay timer and updates physics
        /// </summary>
        /// <param name="gameTime">time since last frame</param>
        public override void Update(GameTime gameTime)
        {
            //count down on the collect delay timer
            if (collectDelayTime > 0)
                collectDelayTime -= gameTime.ElapsedGameTime.TotalSeconds;

            //if the item has physics, then set its gravity to > 0
            if (hasPhysics)
                gravity = 1;
            else
                gravity = 0;

            base.Update(gameTime);
        }
    }
}
