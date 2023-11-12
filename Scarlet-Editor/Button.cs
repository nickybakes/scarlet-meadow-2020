using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker
//2/7/2020
//HW 2 - Monogame Firstgame!
//This stores the image, size, location, and OnClick action of a button that user can click on.
namespace Scarlet_Editor
{
    /// <summary>
    /// This stores the image, size, location, and OnClick action of a button that user can click on.
    /// </summary>
    class Button
    {
        /// <summary>
        /// Holds the default sprite for the button
        /// </summary>
        private Texture2D image;

        /// <summary>
        /// Holds the position and size of this button
        /// </summary>
        private Rectangle bounds;

        /// <summary>
        /// the name of the button for tool tips
        /// </summary>
        private String buttonName;

        /// <summary>
        /// the description of what this button does for tool tips
        /// </summary>
        private String buttonDescription;

        /// <summary>
        /// true if the mouse is hovering over this or not
        /// </summary>
        private bool selected;

        /// <summary>
        /// the delegate used for when the button is clicked
        /// </summary>
        public delegate void ButtonDelegate();

        /// <summary>
        /// The methods to run when this button is clicked
        /// </summary>
        public event ButtonDelegate OnClick;

        /// <summary>
        /// Get the name of the button (for tool tip)
        /// </summary>
        public String Name
        {
            get { return buttonName; }
        }

        /// <summary>
        /// get the description of what the button does when clicked (for tool tip)
        /// </summary>
        public String Description
        {
            get { return buttonDescription; }
        }

        /// <summary>
        /// gets the size and position of the button
        /// </summary>
        public Rectangle Bounds
        {
            get { return bounds; }
        }

        /// <summary>
        /// gets and sets whether this button is being hovered over or not
        /// </summary>
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        /// <summary>
        /// get the event for when the button is clicked
        /// </summary>
        public ButtonDelegate ClickButton
        {
            get { return OnClick; }
        }

        /// <summary>
        /// Constructor sets the images for this button as well as the rectangle that holds the position and size
        /// </summary>
        /// <param name="image">Holds the default sprite for the button</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Width of the button</param>
        /// <param name="height">Height of the button</param>
        /// <param name="buttonName">Name of the button</param>
        /// <param name="buttonDescription">Description of what the button does when clicked</param>
        public Button(Texture2D image, int x, int y, int width, int height, string buttonName, string buttonDescription)
        {
            this.image = image;
            this.bounds = new Rectangle(x, y, width, height);
            this.buttonName = buttonName;
            this.buttonDescription = buttonDescription;
        }

        /// <summary>
        /// Draws the button onto the screen with its size and position and a color tint
        /// </summary>
        /// <param name="sb">the spritebatch to draw to</param>
        /// <param name="color">the Color tint to apply to the graphic</param>
        public void Draw(SpriteBatch sb, Color color)
        {
            sb.Draw(image, bounds, color);
        }
    }
}
