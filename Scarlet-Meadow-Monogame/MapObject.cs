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
//This holds the data for instances
namespace Scarlet_Meadow_Monogame
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
        /// Constructor for props and entities, objects that only need a size and a visual
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

    }
}
