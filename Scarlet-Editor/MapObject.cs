using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/26/2020
//MapObject
//This holds the data of objects in the map; an instance, entity, or prop
namespace Scarlet_Editor
{
    /// <summary>
    /// This represents an object in the map; an instance, entity, or prop
    /// </summary>
    public class MapObject
    {
        /// <summary>
        /// size of this map object (in grid tiles, not pixels)
        /// </summary>
        private Point size;

        /// <summary>
        /// visual representation of this map object
        /// </summary>
        private Texture2D graphic;

        /// <summary>
        /// collision data in this object, used for instances
        /// </summary>
        private Color[,] mapTiles;

        /// <summary>
        /// List of other map objects referenced in this map object
        /// </summary>
        private List<MapObjectRepresentative> placedMapObjects;

        /// <summary>
        /// Mainly for props, what layer to render this object's graphic on
        /// </summary>
        private int layer;

        /// <summary>
        /// Get the size of this object as a Point (size in grid tiles)
        /// </summary>
        public Point Size
        {
            get { return size; }
        }

        /// <summary>
        /// Get the graphic for this object
        /// </summary>
        public Texture2D Image
        {
            get { return graphic; }
        }

        /// <summary>
        /// Get the collision data for this object as an 2D array of colors
        /// </summary>
        public Color[,] MapTiles
        {
            get { return mapTiles; }
        }

        /// <summary>
        /// Get the list of mapObjectRepresentatives that are in this object
        /// </summary>
        public List<MapObjectRepresentative> PlacedMapObjects
        {
            get { return placedMapObjects; }
        }

        /// <summary>
        /// gets the layer to render this map object on
        /// </summary>
        public int Layer
        {
            get { return layer; }
        }

        /// <summary>
        /// Constructor for entities, objects that only need a size and a visual
        /// </summary>
        /// <param name="width">width of this object (in grid tiles, not pixels)</param>
        /// <param name="height">height of this object (in grid tles, not pixels)</param>
        /// <param name="graphic">the graphic for this object (used for props and entities)</param>
        public MapObject(int width, int height, Texture2D graphic)
        {
            size = new Point(width, height);
            this.graphic = graphic;
            //in the editor, we render all entities abve props for easier visualization
            layer = 9;
        }

        /// <summary>
        /// Constructor for props,  need a size, a visual, and a layer
        /// </summary>
        /// <param name="width">width of this object (in grid tiles, not pixels)</param>
        /// <param name="height">height of this object (in grid tles, not pixels)</param>
        /// <param name="graphic">the graphic for this object (used for props and entities)</param>
        /// <param name="layer">what layer to render the graphic of this object on (used for props)</param>
        public MapObject(int width, int height, Texture2D graphic, int layer)
        {
            size = new Point(width, height);
            this.graphic = graphic;
            this.layer = layer;
        }

        /// <summary>
        /// Constructor used for instances, which only need a size, collision data, and list of other objects
        /// </summary>
        /// <param name="width">width of this object (in grid tiles, not pixels)</param>
        /// <param name="height">height of this object (in grid tles, not pixels)</param>
        /// <param name="mapTiles">collision data for this object (used by instances)</param>
        /// <param name="placedMapObjects">other objects placed inside this instance (used by instances)</param>
        public MapObject(int width, int height, Color[,] mapTiles, List<MapObjectRepresentative> placedMapObjects)
        {
            size = new Point(width, height);
            this.mapTiles = mapTiles;
            this.placedMapObjects = placedMapObjects;
        }

        /// <summary>
        /// Constructor that takes in all the data
        /// </summary>
        /// <param name="width">width of this object (in grid tiles, not pixels)</param>
        /// <param name="height">height of this object (in grid tles, not pixels)</param>
        /// <param name="graphic">the graphic for this object (used for props and entities)</param>
        /// <param name="mapTiles">collision data for this object (used by instances)</param>
        /// <param name="placedMapObjects">other objects placed inside this instance (used by instances)</param>
        public MapObject(int width, int height, Texture2D graphic, Color[,] mapTiles, List<MapObjectRepresentative> placedMapObjects)
        {
            size = new Point(width, height);
            this.graphic = graphic;
            this.mapTiles = mapTiles;
            this.placedMapObjects = placedMapObjects;
        }


        /// <summary>
        /// Draws the collision tiles of this object
        /// </summary>
        /// <param name="spriteBatch">the spritebatch to draw to</param>
        /// <param name="position">the map objects position on the map</param>
        /// <param name="whiteSquare">the texture to draw with</param>
        /// <param name="blockPixelSize">the pixel size of grid tiles on the screen</param>
        /// <param name="cameraOffset">the offset of the camera to apply</param>
        /// <param name="windowWidth">the preferred width of the window</param>
        /// <param name="windowHeight">the preferred height of the window</param>
        public void DrawCollision(SpriteBatch spriteBatch, Point position, Texture2D whiteSquare, int blockPixelSize, Vector2 cameraOffset, int windowWidth, int windowHeight)
        {
            //if there even is collision data in here, then draw it!
            if(mapTiles != null)
            {
                if (blockPixelSize >= 4)
                {
                    for (int x = 0; x < size.X; x++)
                    {
                        for (int y = 0; y < size.Y; y++)
                        {
                            //if the block is within the camera's view
                            if ((position.X + x) * blockPixelSize - (int)cameraOffset.X >= -blockPixelSize && (position.X + x) * blockPixelSize - (int)cameraOffset.X <= windowWidth
                                && (position.Y + y) * blockPixelSize - (int)cameraOffset.Y >= -blockPixelSize && (position.Y + y) * blockPixelSize - (int)cameraOffset.Y <= windowHeight)
                            {
                                //draw the tile in its color if its not black (treat black as transparent, nothing there)
                                if (mapTiles[x, y] != Color.Black)
                                {
                                    spriteBatch.Draw(whiteSquare, new Rectangle((position.X + x) * blockPixelSize - (int)cameraOffset.X,
                                        (position.Y + y) * blockPixelSize - (int)cameraOffset.Y, blockPixelSize, blockPixelSize), mapTiles[x, y]);
                                }
                            }
                        }
                    }
                }
                else
                {
                    //if the grid is really zoomed out, display a cheaper preview of the grid tiles,
                    //this cheaper preview skips over every other tile
                    for (int x = 0; x < size.X; x += 2)
                    {
                        for (int y = 0; y < size.Y; y += 2)
                        {
                            if ((position.X + x) * blockPixelSize - (int)cameraOffset.X >= -blockPixelSize && (position.X + x) * blockPixelSize - (int)cameraOffset.X <= windowWidth
                                && (position.Y + y) * blockPixelSize - (int)cameraOffset.Y >= -blockPixelSize && (position.Y + y) * blockPixelSize - (int)cameraOffset.Y <= windowHeight)
                            {
                                //draw the tile in its color if its not black (treat black as transparent, nothing there)
                                if (mapTiles[x, y] != Color.Black)
                                {
                                    spriteBatch.Draw(whiteSquare, new Rectangle((position.X + x) * blockPixelSize - (int)cameraOffset.X,
                                        (position.Y + y) * blockPixelSize - (int)cameraOffset.Y, 2, 2), mapTiles[x, y]);
                                }
                            }
                        }
                    }
                }
            }

            //recursively draw the collision tiles for each object inside this object if this has objects
            if (placedMapObjects != null)
            {
                foreach (MapObjectRepresentative rep in placedMapObjects)
                {
                    Game1.ParseMapObjectRep(rep.ID).DrawCollision(spriteBatch, position + rep.Position, whiteSquare, blockPixelSize, cameraOffset, windowWidth, windowHeight);
                }
            }

        }

        /// <summary>
        /// Draws the border of this map object
        /// </summary>
        /// <param name="spriteBatch">the spritebatch to draw to</param>
        /// <param name="position">the position of the map object in the map</param>
        /// <param name="whiteSquare">the texture used</param>
        /// <param name="blockPixelSize">the pixel size of grid tiles on the screen</param>
        /// <param name="cameraOffset">the amount to offset this boundary on the display</param>
        /// <param name="color">the color to draw the edges in</param>
        public void DrawBoundary(SpriteBatch spriteBatch, Point position, Texture2D whiteSquare, int blockPixelSize, Vector2 cameraOffset, Color color)
        {
            //draw the left side
            spriteBatch.Draw(whiteSquare, new Rectangle(position.X * blockPixelSize - (int)cameraOffset.X,
                position.Y * blockPixelSize - (int)cameraOffset.Y, 3, blockPixelSize * size.Y), color);
            //draw the top side
            spriteBatch.Draw(whiteSquare, new Rectangle(position.X * blockPixelSize - (int)cameraOffset.X,
                position.Y * blockPixelSize - (int)cameraOffset.Y, blockPixelSize * size.X, 3), color);
            //draw the right side
            spriteBatch.Draw(whiteSquare, new Rectangle((position.X + size.X) * blockPixelSize - (int)cameraOffset.X,
                position.Y * blockPixelSize - (int)cameraOffset.Y, 3, blockPixelSize * size.Y), color);
            //draw the bottom side
            spriteBatch.Draw(whiteSquare, new Rectangle(position.X * blockPixelSize - (int)cameraOffset.X,
                (position.Y + size.Y) * blockPixelSize - (int)cameraOffset.Y, blockPixelSize * size.X, 3), color);
        }


        /// <summary>
        /// Draws the graphics of this map object
        /// </summary>
        /// <param name="spriteBatch">he spritebatch to draw to</param>
        /// <param name="position">the position of this map object in the map</param>
        /// <param name="blockPixelSize">the pixel size of grid tiles on the screen</param>
        /// <param name="cameraOffset">the offset of the camera to apply</param>
        /// <param name="color">The color to tint this graphic when drawing</param>
        public void DrawGraphic(SpriteBatch spriteBatch, Point position, int blockPixelSize, Vector2 cameraOffset, Color color)
        {
            //if the texture exists, draw it!
            if(graphic != null)
            {
                spriteBatch.Draw(graphic, new Rectangle(position.X * blockPixelSize - (int)cameraOffset.X,
                    position.Y * blockPixelSize - (int)cameraOffset.Y, size.X*blockPixelSize, size.Y*blockPixelSize), color);
            }

            //recursively draw the graphics for each object inside this object if this has objects
            if(placedMapObjects != null)
            {
                foreach (MapObjectRepresentative rep in placedMapObjects)
                {
                    Game1.ParseMapObjectRep(rep.ID).DrawGraphic(spriteBatch, position + rep.Position, blockPixelSize, cameraOffset, color);
                }
            }
        }

        /// <summary>
        /// Adds all props and entities to a list for rendering
        /// </summary>
        /// <param name="objectsToRender">The list to add entities and props to</param>
        /// <param name="positionOffset">the position of this instance in the map</param>
        public void AddObjectsToRenderPipeline(List<MapObjectRepresentative> objectsToRender, Point positionOffset)
        {
            //recursively draw the graphics for each object inside this object if this has objects
            if (placedMapObjects != null)
            {
                //go through each object in this object's list of placed objects!
                foreach (MapObjectRepresentative rep in placedMapObjects)
                {
                    //if its a prop or entity, add it to the render pipline
                    if (rep.ID.Substring(0, 1) == "p" || rep.ID.Substring(0, 1) == "e")
                    {
                        objectsToRender.Add(new MapObjectRepresentative(rep.ID, positionOffset + rep.Position));
                    }
                    //if its an instance, then add ITS objects to the render pipline
                    else
                    {
                        Game1.ParseMapObjectRep(rep.ID).AddObjectsToRenderPipeline(objectsToRender, positionOffset + rep.Position);
                    }

                }
            }
        }

    }
}
