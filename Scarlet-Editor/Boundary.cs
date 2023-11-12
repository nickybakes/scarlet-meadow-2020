using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/21/2020
//Boundary
//This stores 4 corners of the bounds of an area.
namespace Scarlet_Editor
{

    /// <summary>
    /// Represents a handle for a bundary: the corners, sides, middle (entire), or none
    /// </summary>
    public enum Handle
    {
        None,
        Entire,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Left,
        Right,
        Top,
        Bottom
    }

    /// <summary>
    /// This stores 4 corners and sides of the bounds of an area.
    /// </summary>
    class Boundary
    {
        /// <summary>
        /// The top left corner of this boundary
        /// </summary>
        private Point topLeft;

        /// <summary>
        /// The top right corner of this boundary
        /// </summary>
        private Point topRight;

        /// <summary>
        /// The bottom left corner of this boundary
        /// </summary>
        private Point bottomLeft;

        /// <summary>
        /// The bottom right corner of this boundary
        /// </summary>
        private Point bottomRight;

        /// <summary>
        /// the max width this boundary can have so it does not go further out than the map's entire boundary horizontally
        /// </summary>
        private int maxMapWidth;

        /// <summary>
        /// the max height this boundary can have so it does not go further out than the map's entire boundary vertically
        /// </summary>
        private int maxMapHeight;

        /// <summary>
        /// simple get and set for the Point on the top left corner
        /// </summary>
        public Point TopLeft
        {
            get { return topLeft; }
            set { topLeft = value; }
        }

        /// <summary>
        /// simple get and set for the Point on the top right corner
        /// </summary>
        public Point TopRight
        {
            get { return topRight; }
            set { topRight = value; }
        }

        /// <summary>
        /// simple get and set for the Point on the bottom left corner
        /// </summary>
        public Point BottomLeft
        {
            get { return bottomLeft; }
            set { bottomLeft = value; }
        }

        /// <summary>
        /// simple get and set for the Point on the top right corner
        /// </summary>
        public Point BottomRight
        {
            get { return bottomRight; }
            set { bottomRight = value; }
        }

        /// <summary>
        /// gets the width of this boundary (right side X value - left side X value
        /// </summary>
        public int Width
        {
            get { return topRight.X - topLeft.X; }
        }

        /// <summary>
        /// gets the height of this boundary (bottom side Y value - top side Y value
        /// </summary>
        public int Height
        {
            get { return bottomLeft.Y - topLeft.Y; }
        }

        /// <summary>
        /// get and set the X coord of the left side of the boundary
        /// </summary>
        public int Left
        {
            get { return topLeft.X; }
            set 
            { 
                topLeft.X = value;
                bottomLeft.X = value;
            }
        }

        /// <summary>
        /// get and set the X coord of the right side of the boundary
        /// </summary>
        public int Right
        {
            get { return topRight.X; }
            set
            {
                topRight.X = value;
                bottomRight.X = value;
            }
        }

        /// <summary>
        /// get and set the Y coord of the top side of the boundary
        /// </summary>
        public int Top
        {
            get { return topLeft.Y; }
            set
            {
                topLeft.Y = value;
                topRight.Y = value;
            }
        }

        /// <summary>
        /// get and set the Y coord of the bottom side of the boundary
        /// </summary>
        public int Bottom
        {
            get { return bottomLeft.Y; }
            set
            {
                bottomLeft.Y = value;
                bottomRight.Y = value;
            }
        }

        /// <summary>
        /// Constructor sets the points of the corners based on the X and Y and sizes given
        /// </summary>
        /// <param name="x">the X of the top left corner</param>
        /// <param name="y">the Y of the top left corner</param>
        /// <param name="width">the width of this boundary</param>
        /// <param name="height">the height of this boundary</param>
        /// <param name="maxMapWidth">the max width this can have so it wont go outside of the map</param>
        /// <param name="maxMapHeight">the max height this can have so it wont go outside of the map</param>
        public Boundary(int x, int y, int width, int height, int maxMapWidth, int maxMapHeight)
        {
            //sets the corner points
            topLeft = new Point(x, y);
            topRight = new Point(x+width, y);
            bottomLeft = new Point(x, y + height);
            bottomRight = new Point(x + width, y + height);

            //stores the info of the max size
            this.maxMapWidth = maxMapWidth;
            this.maxMapHeight = maxMapHeight;
        }

        /// <summary>
        /// This checks the tile the mouse is on and determines with handle (those white boxes on the edge of the boundary)
        /// that the user is hovering over
        /// </summary>
        /// <param name="currentMousePosition">the current tile that the mouse is hovering over</param>
        /// <param name="handleSizeOnScreen">the size of the handles on the display</param>
        /// <param name="blockWidthOnScreen">the width of blocks on the display (the zoom amount)</param>
        /// <param name="blockHeightOnScreen">the height of blocks on the display (the zoom amount)</param>
        /// <param name="toolBarHeight">the height of the toolBar (offsets the display of the grid graphics)</param>
        /// <param name="cameraOffsetOnScreen">the amount to offset this boundary on the display</param>
        /// <returns>Returns the handle that the mouse is hovering over/selected</returns>
        public Handle CheckSelectedHandle(Point currentMousePosition, float handleSizeOnScreen, float blockWidthOnScreen, float blockHeightOnScreen, int toolBarHeight, Vector2 cameraOffsetOnScreen)
        {

            //checks if closest to top left handle
            if (Vector2.Distance(currentMousePosition.ToVector2(), new Vector2(Left * blockWidthOnScreen - (int)cameraOffsetOnScreen.X, Top * blockHeightOnScreen - (int)cameraOffsetOnScreen.Y + toolBarHeight)) < handleSizeOnScreen/2)
            {
                Mouse.SetCursor(MouseCursor.SizeNWSE);
                return Handle.TopLeft;
            }
            //checks if closest to bottom right handle
            else if (Vector2.Distance(currentMousePosition.ToVector2(), new Vector2(Right * blockWidthOnScreen - (int)cameraOffsetOnScreen.X, Bottom * blockHeightOnScreen - (int)cameraOffsetOnScreen.Y + toolBarHeight)) < handleSizeOnScreen / 2)
            {
                Mouse.SetCursor(MouseCursor.SizeNWSE);
                return Handle.BottomRight;
            }

            //checks if closest to top right handle
            else if (Vector2.Distance(currentMousePosition.ToVector2(), new Vector2(Right * blockWidthOnScreen - (int)cameraOffsetOnScreen.X, Top * blockHeightOnScreen - (int)cameraOffsetOnScreen.Y + toolBarHeight)) < handleSizeOnScreen / 2)
            {
                Mouse.SetCursor(MouseCursor.SizeNESW);
                return Handle.TopRight;
            }
            //checks if closest to bottom left handle
            else if (Vector2.Distance(currentMousePosition.ToVector2(), new Vector2(Left * blockWidthOnScreen - (int)cameraOffsetOnScreen.X, Bottom * blockHeightOnScreen - (int)cameraOffsetOnScreen.Y + toolBarHeight)) < handleSizeOnScreen / 2)
            {
                Mouse.SetCursor(MouseCursor.SizeNESW);
                return Handle.BottomLeft;
            }
            //checks if closest to left side handle
            else if (Vector2.Distance(currentMousePosition.ToVector2(), new Vector2(Left * blockWidthOnScreen - (int)cameraOffsetOnScreen.X, (Top + Height/2) * blockHeightOnScreen - (int)cameraOffsetOnScreen.Y + toolBarHeight)) < handleSizeOnScreen / 2)
            {
                Mouse.SetCursor(MouseCursor.SizeWE);
                return Handle.Left;
            }
            //checks if closest to right side handle
            else if (Vector2.Distance(currentMousePosition.ToVector2(), new Vector2(Right * blockWidthOnScreen - (int)cameraOffsetOnScreen.X, (Top + Height / 2) * blockHeightOnScreen - (int)cameraOffsetOnScreen.Y + toolBarHeight)) < handleSizeOnScreen / 2)
            {
                Mouse.SetCursor(MouseCursor.SizeWE);
                return Handle.Right;
            }
            //checks if closest to top side handle
            else if (Vector2.Distance(currentMousePosition.ToVector2(), new Vector2((Left + Width/2) * blockWidthOnScreen - (int)cameraOffsetOnScreen.X, Top * blockHeightOnScreen - (int)cameraOffsetOnScreen.Y + toolBarHeight)) < handleSizeOnScreen / 2)
            {
                Mouse.SetCursor(MouseCursor.SizeNS);
                return Handle.Top;
            }
            //checks if closest to bottom side handle
            else if (Vector2.Distance(currentMousePosition.ToVector2(), new Vector2((Left + Width / 2) * blockWidthOnScreen - (int)cameraOffsetOnScreen.X, Bottom * blockHeightOnScreen - (int)cameraOffsetOnScreen.Y + toolBarHeight)) < handleSizeOnScreen / 2)
            {
                Mouse.SetCursor(MouseCursor.SizeNS);
                return Handle.Bottom;
            }
            //if the mouse isnt on a handle, checks if its in the middle of the boundary
            else if (new Rectangle((int)(Left * blockWidthOnScreen - (int)cameraOffsetOnScreen.X), (int) (Top * blockHeightOnScreen - (int)cameraOffsetOnScreen.Y + toolBarHeight),
                (int) (Width * blockWidthOnScreen), (int) (Height * blockHeightOnScreen)).Contains(currentMousePosition))
            {
                Mouse.SetCursor(MouseCursor.SizeAll);
                return Handle.Entire;
            }
            //otherwise, the mouse is outside the boundary and not selecting a handle
            else
            {
                Mouse.SetCursor(MouseCursor.Arrow);
                return Handle.None;
            }
        }

        /// <summary>
        /// This is used when the user is dragging the handles around to resize the selection/boundary.
        /// After the initial resize, this method checks to make sure its valid (not outside the map, not inside out),
        /// and makes adjustments accordingly
        /// </summary>
        /// <param name="currentMouseSelection">the grid tile this mouse is on now</param>
        /// <param name="previousMouseSelection">the grid tile the mouse was on in the previous frame</param>
        /// <param name="selectedHandle">the handle the user is draggin around</param>
        public void AdjustBoundaries(Point currentMouseSelection, Point previousMouseSelection, Handle selectedHandle)
        {
            switch (selectedHandle)
            {

                //if the user is dragging the top left corner handle
                case Handle.TopLeft:
                    //change the top and left side coordinates
                    Left = currentMouseSelection.X;
                    Top = currentMouseSelection.Y;
                    break;

                //if the user is dragging the bottom right corner handle
                case Handle.BottomRight:
                    Right = currentMouseSelection.X;
                    Bottom = currentMouseSelection.Y;
                    break;

                //if the user is dragging the top right corner handle
                case Handle.TopRight:
                    Right = currentMouseSelection.X;
                    Top = currentMouseSelection.Y;
                    break;

                //if the user is dragging the bottom left corner handle
                case Handle.BottomLeft:
                    Left = currentMouseSelection.X;
                    Bottom = currentMouseSelection.Y;
                    break;

                //if the user is dragging the left side handle
                case Handle.Left:
                    Left = currentMouseSelection.X;
                    break;

                //if the user is dragging the right side handle
                case Handle.Right:
                    Right = currentMouseSelection.X;
                    break;

                //if the user is dragging the top side handle
                case Handle.Top:
                    Top = currentMouseSelection.Y;
                    break;

                //if the user is dragging the bottom side handle
                case Handle.Bottom:
                    Bottom = currentMouseSelection.Y;
                    break;

                //if the user is dragging the entire boundary (mouse is in the middle)
                case Handle.Entire:
                    MoveEntireBoundary(currentMouseSelection - previousMouseSelection);
                    break;
            }


            //making sure the boundaries of the map dont exceed the max/minimum map size
            //making the left and right edges dont go off the egde of the map
            if (Left < 0)
                Left = 0;
            if (Right < 1)
                Right = 1;
            if (Left > maxMapWidth-1)
                Left = maxMapWidth-1;
            if (Right > maxMapWidth)
                Right = maxMapWidth;

            //making sure the top and bottom edges done go off the edge of the map
            if (Top < 0)
                Top = 0;
            if (Bottom < 1)
                Bottom = 1;
            if (Top > maxMapHeight - 1)
                Top = maxMapHeight - 1;
            if (Bottom > maxMapHeight)
                Bottom = maxMapHeight;

            //if the left side is further right than the right side
            if (Left >= Right)
                //push left side left so that there is aleast 1 column of tiles between both edges
                Left = Right - 1;

            //if the top side is further down than the bottom side
            if (Top >= Bottom)
            {
                //push top up so that there is aleast 1 row of tiles between both edges
                Top = Bottom - 1;
            }
        }

        /// <summary>
        /// This is used to move the entire boundary, and not just one side/corner
        /// </summary>
        /// <param name="distance">the amount to move in X and Y direction</param>
        public void MoveEntireBoundary(Point distance)
        {
            //in the X direction
            for(int i = 0; i < Math.Abs(distance.X); i++)
            {
                //moves the selection as much to the right and left sides as it can (while staying within map limit)
                if (Left + Math.Sign(distance.X) >= 0 && Right + Math.Sign(distance.X) <= maxMapWidth)
                {
                    Left += Math.Sign(distance.X);
                    Right += Math.Sign(distance.X);
                }
            }

            //in the Y direction
            for (int i = 0; i < Math.Abs(distance.Y); i++)
            {
                //moves the selection up and down as much as it can (while staying within map limit)
                if (Top + Math.Sign(distance.Y) >= 0 && Bottom + Math.Sign(distance.Y) <= maxMapHeight)
                {
                    Top += Math.Sign(distance.Y);
                    Bottom += Math.Sign(distance.Y);
                }
            }
        }

        /// <summary>
        /// Draws the edges of the boundary
        /// </summary>
        /// <param name="spriteBatch">the spritebatch to draw to</param>
        /// <param name="whiteSquare">the texture used</param>
        /// <param name="blockPixelSize">the size of blocks (the zoom amount)</param>
        /// <param name="cameraOffset">the amount to offset this boundary on the display</param>
        /// <param name="color">the color to draw the edges in</param>
        public void DrawBoundary(SpriteBatch spriteBatch, Texture2D whiteSquare, int blockPixelSize, Vector2 cameraOffset, Color color)
        {
            //draw the left side
            spriteBatch.Draw(whiteSquare, new Rectangle(Left * blockPixelSize - (int)cameraOffset.X,
                Top * blockPixelSize - (int)cameraOffset.Y, 3, blockPixelSize * Height), color);
            //draw the top side
            spriteBatch.Draw(whiteSquare, new Rectangle(Left * blockPixelSize - (int)cameraOffset.X,
                Top * blockPixelSize - (int)cameraOffset.Y, blockPixelSize * Width, 3), color);
            //draw the right side
            spriteBatch.Draw(whiteSquare, new Rectangle((Left + Width) * blockPixelSize - (int)cameraOffset.X,
                Top * blockPixelSize - (int)cameraOffset.Y, 3, blockPixelSize * Height), color);
            //draw the bottom side
            spriteBatch.Draw(whiteSquare, new Rectangle(Left * blockPixelSize - (int)cameraOffset.X,
                (Top + Height) * blockPixelSize - (int)cameraOffset.Y, blockPixelSize * Width, 3), color);
        }

        /// <summary>
        /// draws the corner and side handles of this boundary.
        /// </summary>
        /// <param name="spriteBatch">the spritebatch to draw to</param>
        /// <param name="handleTexture">the texture to draw</param>
        /// <param name="handleSize">size of the handles</param>
        /// <param name="blockPixelSize">the size of blicks (the zoom amount)</param>
        /// <param name="cameraOffset">the amount to offset this boundary on the display</param>
        public void DrawHandles(SpriteBatch spriteBatch, Texture2D handleTexture, int handleSize, int blockPixelSize, Vector2 cameraOffset)
        {
            //draws top left corner handle
            spriteBatch.Draw(handleTexture, new Rectangle(Left * blockPixelSize - (int)cameraOffset.X - (handleSize / 2),
                Top * blockPixelSize - (int)cameraOffset.Y - (handleSize / 2), handleSize, handleSize), Color.White);

            //draws top right corner handle
            spriteBatch.Draw(handleTexture, new Rectangle(Right * blockPixelSize - (int)cameraOffset.X - (handleSize / 2),
                Top * blockPixelSize - (int)cameraOffset.Y - (handleSize / 2), handleSize, handleSize), Color.White);

            //draws bottom left corner handle
            spriteBatch.Draw(handleTexture, new Rectangle(Left * blockPixelSize - (int)cameraOffset.X - (handleSize / 2),
                (Top + Height) * blockPixelSize - (int)cameraOffset.Y - (handleSize / 2), handleSize, handleSize), Color.White);

            //draws the bottom right corner handle
            spriteBatch.Draw(handleTexture, new Rectangle(Right * blockPixelSize - (int)cameraOffset.X - (handleSize / 2),
                (Top + Height) * blockPixelSize - (int)cameraOffset.Y - (handleSize / 2), handleSize, handleSize), Color.White);

            //draws the left side handle
            spriteBatch.Draw(handleTexture, new Rectangle(Left * blockPixelSize - (int)cameraOffset.X - (handleSize / 2),
                (Top + Height / 2) * blockPixelSize - (int)cameraOffset.Y - (handleSize / 2), handleSize, handleSize), Color.White);

            //draws the right side handle
            spriteBatch.Draw(handleTexture, new Rectangle(Right * blockPixelSize - (int)cameraOffset.X - (handleSize / 2),
                (Top + Height / 2) * blockPixelSize - (int)cameraOffset.Y - (handleSize / 2), handleSize, handleSize), Color.White);

            //draws the top side handle
            spriteBatch.Draw(handleTexture, new Rectangle((Left + Width / 2) * blockPixelSize - (int)cameraOffset.X - (handleSize / 2),
                Top * blockPixelSize - (int)cameraOffset.Y - (handleSize / 2), handleSize, handleSize), Color.White);

            //draws the bottom side handle
            spriteBatch.Draw(handleTexture, new Rectangle((Left + Width / 2) * blockPixelSize - (int)cameraOffset.X - (handleSize / 2),
                Bottom * blockPixelSize - (int)cameraOffset.Y - (handleSize / 2), handleSize, handleSize), Color.White);
        }

        /// <summary>
        /// Converts this boundary to a rectangle and returns it
        /// </summary>
        /// <returns>the rectangle created by this data held by this boundary</returns>
        public Rectangle ToRectangle()
        {
            int x = topLeft.X;
            int y = topLeft.Y;
            int width = topRight.X - topLeft.X;
            int height = bottomRight.Y - topLeft.Y;

            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Convert data of this boundary into a well formated string
        /// </summary>
        /// <returns>the well formated string based on this boundary's data</returns>
        public override string ToString()
        {
            return this.ToRectangle().ToString();
        }
    }
}
