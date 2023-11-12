using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//3/17/2020
//Scarlet Meadow monogame project
//Draws our custom font on the screen with the ability to stretch or shrink the font and color it in a gradiant
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// The point to align text to
    /// </summary>
    public enum TextAlignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    /// <summary>
    /// Draws our custom font on the screen with the ability to stretch or shrink the font and color it in a gradiant
    /// </summary>
    public static class ScarletFontDrawer
    {
        /// <summary>
        /// The available characters a string can have for this font drawer
        /// </summary>
        private const string availableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,:;?!-/\"%#'";

        /// <summary>
        /// How much to squish characters together when drawing them.
        /// Helps eliminate visual gaps between letters inside words (can be mistaken for spaces)
        /// </summary>
        private const int squishAmount = 44;

        /// <summary>
        /// Holds the textures for each character
        /// </summary>
        private static Dictionary<string, Texture2D> textures;

        /// <summary>
        /// Loads and stores the textures for alphanumeric and punctuation characters
        /// </summary>
        /// <param name="gameBase">the Game1 class that class this</param>
        public static void LoadContent(Game1 gameBase)
        {
            textures = new Dictionary<string, Texture2D>();

            //loading in our alphanumeric characters
            string alphanumerics = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            for (int i = 0; i < alphanumerics.Length; i++)
            {
                //adding them to the dictionary
                textures.Add(alphanumerics.Substring(i, 1), gameBase.Content.Load<Texture2D>("Scarlet-Font/" + alphanumerics.Substring(i, 1)));
            }

            //loading in our punctuation
            string punctuation = ".,:;?!-/\"%#'";
            for (int i = 0; i < punctuation.Length; i++)
            {
                //adding them to the dictionary
                textures.Add(punctuation.Substring(i, 1), gameBase.Content.Load<Texture2D>("Scarlet-Font/punc" + i));
            }
        }

        /// <summary>
        /// Displays time in a well formatted string of text
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to draw to</param>
        /// <param name="time">the time to draw in seconds</param>
        /// <param name="x">the starting x position</param>
        /// <param name="y">the starting Y position</param>
        /// <param name="desiredHeight">the height of the characters</param>
        /// <param name="alignment"> The point to align text to</param>
        /// <param name="dropShadow">If true, draws a black version of the text underneath and to the right of the original text</param>
        /// <param name="startColor">left color of the gradiant</param>
        /// <param name="endColor">right color of the gradiant</param>
        public static void DrawTimeScaled(SpriteBatch spriteBatch, double time, int x, int y, int desiredHeight, TextAlignment alignment, bool dropShadow, Color startColor, Color endColor)
        {
            string timeString = "";
            if (time % 60 >= 10)
                timeString = string.Format("{0}:{1}", (int)time / 60, (time % 60).ToString("F2"));
            else
                timeString = string.Format("{0}:0{1}", (int)time / 60, (time % 60).ToString("F2"));
            DrawStringScaled(spriteBatch, timeString, x, y, desiredHeight, alignment, dropShadow, startColor, endColor);
        }

        /// <summary>
        /// Draws a string in our custom font with an optional gradiant
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to draw to</param>
        /// <param name="text">the draw to write</param>
        /// <param name="x">the starting x position</param>
        /// <param name="y">the starting Y position</param>
        /// <param name="desiredWidth">the width of the ENTIRE string</param>
        /// <param name="desiredHeight">the height of the characters</param>
        /// <param name="alignment"> The point to align text to</param>
        /// <param name="dropShadow">If true, draws a black version of the text underneath and to the right of the original text</param>
        /// <param name="startColor">left color of the gradiant</param>
        /// <param name="endColor">right color of the gradiant</param>
        public static void DrawString(SpriteBatch spriteBatch, String text, int x, int y, int desiredWidth, int desiredHeight, TextAlignment alignment, bool dropShadow, Color startColor, Color endColor)
        {
            //we only have capital letters so just switch it to caps for now.
            text = text.ToUpper();

            //first we need to determine the total width we'd need to draw the string without any stretching
            int totalWidthNeeded = 0;

            //go through each character in the text to find the total width needed
            for (int i = 0; i < text.Length; i++)
            {
                //if its just a space, then just add the width of an example character to the width
                if (text.Substring(i, 1) == " ")
                {
                    totalWidthNeeded += textures["I"].Width;
                }
                //if the requested character isnt available, put it as a question mark
                else if (availableChars.IndexOf(text.Substring(i, 1)) == -1)
                {
                    totalWidthNeeded += textures["?"].Width - squishAmount;
                }
                //if not, use the appropriate character's texture's width
                else
                {
                    totalWidthNeeded += textures[text.Substring(i, 1)].Width - squishAmount;
                }
            }

            //find the scale factor for stretching/shrinking the characters horizontally
            double scaleFactor = (double)desiredWidth / (double)totalWidthNeeded;

            //store our x and y positions
            double xPos = x;
            double yPos = y;

            //adjusting where we start drawing based on what alignment was requested
            switch (alignment)
            {
                case (TextAlignment.TopCenter):
                    xPos -= desiredWidth / 2;
                    break;

                case (TextAlignment.TopRight):
                    xPos -= desiredWidth;
                    break;

                case (TextAlignment.MiddleLeft):
                    yPos -= desiredHeight / 2;
                    break;

                case (TextAlignment.MiddleCenter):
                    xPos -= desiredWidth / 2;
                    yPos -= desiredHeight / 2;
                    break;

                case (TextAlignment.MiddleRight):
                    xPos -= desiredWidth;
                    yPos -= desiredHeight / 2;
                    break;

                case (TextAlignment.BottomLeft):
                    yPos -= desiredHeight;
                    break;

                case (TextAlignment.BottomCenter):
                    xPos -= desiredWidth / 2;
                    yPos -= desiredHeight;
                    break;

                case (TextAlignment.BottomRight):
                    xPos -= desiredWidth;
                    yPos -= desiredHeight;
                    break;
            }

            //figuring out how much to change the colors per character to create a gradiant
            int redChangePerChar = endColor.R - startColor.R;
            int greenChangePerChar = endColor.G - startColor.G;
            int blueChangePerChar = endColor.B - startColor.B;
            int alphaChangePerChar = endColor.A - startColor.A;

            //if we have multiple characters, we go one more deeper to make the colors more bold
            if (text.Length > 1)
            {
                redChangePerChar = (endColor.R - startColor.R) / (text.Length - 1);
                greenChangePerChar = (endColor.G - startColor.G) / (text.Length - 1);
                blueChangePerChar = (endColor.B - startColor.B) / (text.Length - 1);
                alphaChangePerChar = (endColor.A - startColor.A) / (text.Length - 1);
            }


            for (int i = 0; i < text.Length; i++)
            {
                //getting the color to draw with based on hor far along the text we are
                Color drawColor = new Color(startColor.R + (redChangePerChar * i), startColor.G + (greenChangePerChar * i), startColor.B + (blueChangePerChar * i), startColor.A + (alphaChangePerChar * i));

                //if its just a space, then just move the x Position to the right for the next character
                if (text.Substring(i, 1) == " ")
                {
                    xPos += (textures["I"].Width * scaleFactor);
                }
                //if the requested character isnt available, put it as a question mark
                else if (availableChars.IndexOf(text.Substring(i, 1)) == -1)
                {
                    if (dropShadow)
                        spriteBatch.Draw(textures["?"], new Rectangle((int)xPos + (int)(textures["?"].Width * scaleFactor) / 6, (int)yPos + desiredHeight / 6, (int)(textures["?"].Width * scaleFactor), desiredHeight), Color.Black);
                    spriteBatch.Draw(textures["?"], new Rectangle((int)xPos, (int)yPos, (int)(textures["?"].Width * scaleFactor), desiredHeight), drawColor);

                    //move the x Position to the right for the next character
                    xPos += ((textures["?"].Width-squishAmount) * scaleFactor);
                }
                //if not, draw the character with the desired color and scale factor!
                else
                {
                    if (dropShadow)
                        spriteBatch.Draw(textures[text.Substring(i, 1)], new Rectangle((int)xPos + (int)(textures[text.Substring(i, 1)].Width * scaleFactor) / 6, (int)yPos + desiredHeight / 6, (int)(textures[text.Substring(i, 1)].Width * scaleFactor), desiredHeight), Color.Black);
                    spriteBatch.Draw(textures[text.Substring(i, 1)], new Rectangle((int)xPos, (int)yPos, (int)(textures[text.Substring(i, 1)].Width * scaleFactor), desiredHeight), drawColor);

                    //move the x Position to the right for the next character
                    xPos += ((textures[text.Substring(i, 1)].Width-squishAmount) * scaleFactor);
                }

            }
        }


        /// <summary>
        /// Draws a string scaled by the height desired in our custom font with an optional gradiant
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to draw to</param>
        /// <param name="text">the draw to write</param>
        /// <param name="x">the starting x position</param>
        /// <param name="y">the starting Y position</param>
        /// <param name="desiredHeight">the height of the characters</param>
        /// <param name="alignment"> The point to align text to</param>
        /// <param name="dropShadow">If true, draws a black version of the text underneath and to the right of the original text</param>
        /// <param name="startColor">left color of the gradiant</param>
        /// <param name="endColor">right color of the gradiant</param>
        public static void DrawStringScaled(SpriteBatch spriteBatch, String text, int x, int y, int desiredHeight, TextAlignment alignment, bool dropShadow, Color startColor, Color endColor)
        {
            //we only have capital letters so just switch it to caps for now.
            text = text.ToUpper();

            //find the scale factor for stretching/shrinking the characters in both directions (but based on the height since all characters have the same height)
            double scaleFactor = (double)desiredHeight / (double)textures["I"].Height;

            //first we need to determine the scaled total width we'd need to draw the string so we can align it correctly
            int scaledTotalWidthNeeded = 0;

            if(alignment != TextAlignment.TopLeft || alignment != TextAlignment.MiddleLeft || alignment != TextAlignment.BottomLeft)
            {
                //go through each character in the text to find the total width needed
                for (int i = 0; i < text.Length; i++)
                {
                    //if its just a space, then just add the width of an example character to the width
                    if (text.Substring(i, 1) == " ")
                    {
                        scaledTotalWidthNeeded += (int)(textures["I"].Width * scaleFactor);
                    }
                    //if the requested character isnt available, put it as a question mark
                    else if (availableChars.IndexOf(text.Substring(i, 1)) == -1)
                    {
                        scaledTotalWidthNeeded += (int)((textures["?"].Width-squishAmount) * scaleFactor);
                    }
                    //if not, use the appropriate character's texture's width
                    else
                    {
                        scaledTotalWidthNeeded += (int)((textures[text.Substring(i, 1)].Width-squishAmount) * scaleFactor);
                    }
                }
            }

            //store our x and y positions
            double xPos = x;
            double yPos = y;

            //adjusting where we start drawing based on what alignment was requested
            switch (alignment)
            {
                case (TextAlignment.TopCenter):
                    xPos -= scaledTotalWidthNeeded / 2;
                    break;

                case (TextAlignment.TopRight):
                    xPos -= scaledTotalWidthNeeded;
                    break;

                case (TextAlignment.MiddleLeft):
                    yPos -= desiredHeight / 2;
                    break;

                case (TextAlignment.MiddleCenter):
                    xPos -= scaledTotalWidthNeeded / 2;
                    yPos -= desiredHeight / 2;
                    break;

                case (TextAlignment.MiddleRight):
                    xPos -= scaledTotalWidthNeeded;
                    yPos -= desiredHeight / 2;
                    break;

                case (TextAlignment.BottomLeft):
                    yPos -= desiredHeight;
                    break;

                case (TextAlignment.BottomCenter):
                    xPos -= scaledTotalWidthNeeded / 2;
                    yPos -= desiredHeight;
                    break;

                case (TextAlignment.BottomRight):
                    xPos -= scaledTotalWidthNeeded;
                    yPos -= desiredHeight;
                    break;
            }


            //figuring out how much to change the colors per character to create a gradiant
            int redChangePerChar = endColor.R - startColor.R;
            int greenChangePerChar = endColor.G - startColor.G;
            int blueChangePerChar = endColor.B - startColor.B;
            int alphaChangePerChar = endColor.A - startColor.A;

            //if we have multiple characters, we go one more deeper to make the colors more bold
            if (text.Length > 1)
            {
                redChangePerChar = (endColor.R - startColor.R) / (text.Length - 1);
                greenChangePerChar = (endColor.G - startColor.G) / (text.Length - 1);
                blueChangePerChar = (endColor.B - startColor.B) / (text.Length - 1);
                alphaChangePerChar = (endColor.A - startColor.A) / (text.Length - 1);
            }

            for (int i = 0; i < text.Length; i++)
            {
                //getting the color to draw with based on hor far along the text we are
                Color drawColor = new Color(startColor.R + (redChangePerChar * i), startColor.G + (greenChangePerChar * i), startColor.B + (blueChangePerChar * i), startColor.A + (alphaChangePerChar * i));

                //if its just a space, then just move the x Position to the right for the next character
                if (text.Substring(i, 1) == " ")
                {
                    xPos += (textures["I"].Width * scaleFactor);
                }
                //if the requested character isnt available, put it as a question mark
                else if (availableChars.IndexOf(text.Substring(i, 1)) == -1)
                {
                    if (dropShadow)
                        spriteBatch.Draw(textures["?"], new Rectangle((int)xPos + (int)(textures["?"].Width * scaleFactor) / 6, (int)yPos + desiredHeight / 6, (int)(textures["?"].Width * scaleFactor), desiredHeight), Color.Black);
                    spriteBatch.Draw(textures["?"], new Rectangle((int)xPos, (int)yPos, (int)(textures["?"].Width * scaleFactor), desiredHeight), drawColor);

                    //move the x Position to the right for the next character
                    xPos += ((textures["?"].Width-squishAmount) * scaleFactor);
                }
                //if not, draw the character with the desired color and scale factor!
                else
                {
                    if (dropShadow)
                        spriteBatch.Draw(textures[text.Substring(i, 1)], new Rectangle((int)xPos + (int)(textures[text.Substring(i, 1)].Width * scaleFactor) / 6, (int)yPos + desiredHeight / 6, (int)(textures[text.Substring(i, 1)].Width * scaleFactor), desiredHeight), Color.Black);
                    spriteBatch.Draw(textures[text.Substring(i, 1)], new Rectangle((int)xPos, (int)yPos, (int)(textures[text.Substring(i, 1)].Width * scaleFactor), desiredHeight), drawColor);

                    //move the x Position to the right for the next character
                    xPos += ((textures[text.Substring(i, 1)].Width-squishAmount) * scaleFactor);
                }
            }
        }
    }
}
