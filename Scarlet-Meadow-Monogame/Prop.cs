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
//this is a visual object in the scene that has no collision data
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// this is a visual object in the scene that has no collision data
    /// </summary>
    public class Prop : GameObject
    {

        /// <summary>
        /// constructor for the prop, sets data for the prop
        /// </summary>
        /// <param name="x">top left X coord</param>
        /// <param name="y">top left Y coord</param>
        /// <param name="width">width of this prop</param>
        /// <param name="height">height of this prop</param>
        /// <param name="texture">the texture for this prop</param>
        public Prop(int x, int y, int width, int height, Texture2D texture, Stage currentStage)
            : base(x, y, width, height, texture, 0, currentStage)
        {
            
        }
    }
}
