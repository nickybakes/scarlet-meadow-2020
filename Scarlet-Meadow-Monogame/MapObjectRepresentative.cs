using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/26/2020
//MapObjectRepresentation
//This represents an object that the user placed in the map; an instance, entity, or prop
namespace Scarlet_Meadow_Monogame
{
    public class MapObjectRepresentative
    {
        /// <summary>
        /// this id is used to determine what map object this class represents
        /// Will be ObjectType followed by its NumericId (index in the list of instances, entities, or props)
        /// EX: i23 means the instance at index 23 in the instances list
        /// </summary>
        private String id;

        /// <summary>
        /// where in the map this map object is placed
        /// </summary>
        private Point position;

        /// <summary>
        /// Returns the ID of this representative, which is:
        /// Will be ObjectType followed by its NumericId (index in the list of instances, entities, or props)
        /// EX: i23 means the instance at index 23 in the instances list
        /// </summary>
        public String ID
        {
            get { return id; }
        }

        /// <summary>
        /// get and set the location of this rep on the map
        /// </summary>
        public Point Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Constructor that sets all the data such as id, position, and layer of this object
        /// </summary>
        /// <param name="id">the id for what object this is representing</param>
        /// <param name="position">the position of this representative on the map</param>
        public MapObjectRepresentative(String id, Point position)
        {
            this.id = id;
            this.position = position;
        }
    }
}
