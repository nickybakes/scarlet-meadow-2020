using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/12/2020
//Scarlet Meadow monogame project
//This simply holds code for loading and parsing data from .scar files to load Stages
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// This simply holds code for loading and parsing data from .scar files to load Stages
    /// </summary>
    public class FileManager
    {
        /// <summary>
        /// this is the Game class that is running our game. Its a reference given through the constructor
        /// </summary>
        private Game1 gameBase;

        /// <summary>
        /// List of platform gameobjects in the stage currently being loaded
        /// </summary>
        private List<GameObject> platforms;

        /// <summary>
        /// List of entity gameobjects in the stage currently being loaded
        /// </summary>
        private List<MapObjectRepresentative> entities;

        /// <summary>
        /// layers of props in the stage currently being loaded
        /// </summary>
        private List<Prop>[] propLayers;


        /// <summary>
        /// Constructor is given a reference to the main Game1 class that creates this instance
        /// </summary>
        /// <param name="mainEditor">the main Game1 class that creates this instance</param>
        public FileManager(Game1 gameBase)
        {
            this.gameBase = gameBase;
        }

        /// <summary>
        /// This method reads info from a binary file as denoted by its file path passed
        /// to this method. It creates the map grid and sets colors based on file's data
        /// </summary>
        /// <param name="fileName">String path to the file</param>
        public void LoadStage(String fileName)
        {
            Color[,] mapTiles = null;
            List<MapObjectRepresentative> mapObjectRepresentatives = null;
            int topLeftX = 0;
            int topLeftY = 0;
            int mapWidth = 0;
            int mapHeight = 0;

            //creating references for stream and reader
            Stream inputStream = null;
            BinaryReader reader = null;
            try
            {
                //opening the stream to the binary file, creating a reader
                inputStream = File.OpenRead(fileName);
                reader = new BinaryReader(inputStream);

                //reading the position and size of the boundaries of the map
                topLeftX = reader.ReadInt32();
                topLeftY = reader.ReadInt32();
                mapWidth = reader.ReadInt32();
                mapHeight = reader.ReadInt32();

                mapTiles = new Color[gameBase.MaxMapWidth, gameBase.MaxMapHeight];

                //Creates each box in a grid like fashion and places them in the viewport
                for (int x = 0; x < mapWidth; x++)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        //reading the color from the file
                        System.Drawing.Color loadedColor = System.Drawing.Color.FromArgb(reader.ReadInt32());
                        //storing it in the array!
                        mapTiles[x + topLeftX, y + topLeftY] = new Color(loadedColor.R, loadedColor.G, loadedColor.B);
                    }
                }

                //the amount of map objects that have been placed in this map
                int numberOfMapObjects = reader.ReadInt32();

                mapObjectRepresentatives = new List<MapObjectRepresentative>();

                //retrieving the data for all the objects in a map
                for (int i = 0; i < numberOfMapObjects; i++)
                {
                    MapObjectRepresentative rep = new MapObjectRepresentative(reader.ReadString(),
                        new Point(reader.ReadInt32(), reader.ReadInt32()));
                    mapObjectRepresentatives.Add(rep);
                }

            }
            catch (Exception ex)
            {
                //error writing file
                System.Windows.Forms.MessageBox.Show("Error loading level.",
                    "Error Loading", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            finally
            {
                //closin the stream
                if (reader != null)
                {
                    reader.Close();
                }
            }

            platforms = ConstructCollisions(mapTiles, new Point(0, 0));
            
            entities = new List<MapObjectRepresentative>();
            propLayers = new List<Prop>[9];
            for(int i = 0; i < 9; i++)
            {
                propLayers[i] = new List<Prop>();
            }

            LoadMapObjects(mapObjectRepresentatives, new Point(0, 0));

            gameBase.LoadStage(platforms, entities, propLayers, new Rectangle(topLeftX * Game1.Scale, topLeftY * Game1.Scale, mapWidth*Game1.Scale, mapHeight*Game1.Scale));
        }

        /// <summary>
        /// Interprets map object representatives and constructs collision, entity, and prop data from them
        /// </summary>
        /// <param name="mapObjectReps">List of map object representatives to interpet</param>
        /// <param name="topLeftOrigin">amount to offset map objects based on its parent instance's position</param>
        public void LoadMapObjects(List<MapObjectRepresentative> mapObjectReps, Point topLeftOrigin)
        {
            foreach (MapObjectRepresentative rep in mapObjectReps)
            {
                List<GameObject> platformsToAdd = new List<GameObject>();
                //if the first letter is "i", then its referring to an instance
                if (rep.ID.Substring(0, 1) == "i")
                {
                    //construct this instance's collision rectangles and recursively construct its map objects
                    ConstructInstance(gameBase.Instances[int.Parse(rep.ID.Substring(1))], rep.Position);
                }

                //if the first letter is "e", then its referring to an entity
                else if (rep.ID.Substring(0, 1) == "e")
                {
                    //store this entity's ID and position scaled for the game for later interpretation
                    entities.Add(new MapObjectRepresentative(rep.ID, new Point((topLeftOrigin.X + rep.Position.X)*Game1.Scale, 
                        (topLeftOrigin.Y + rep.Position.Y) * Game1.Scale)));
                }

                //if the first letter is "p", then its referring to an prop
                else if (rep.ID.Substring(0, 1) == "p")
                {
                    //store the prop as a new prop object with position and size scaled for the game
                    MapObject propToStore = gameBase.Props[int.Parse(rep.ID.Substring(1))];
                    propLayers[propToStore.Layer].Add(new Prop((topLeftOrigin.X + rep.Position.X)*Game1.Scale, (topLeftOrigin.Y + rep.Position.Y) * Game1.Scale,
                        propToStore.Size.X * Game1.Scale, propToStore.Size.Y * Game1.Scale, propToStore.Image, null));
                }
            }
        }

        /// <summary>
        /// Gets the collision rectangles from an instance and adds them to the platforms list, then
        /// recursively constructs its MapObjects, which could include another instance, repeating the process
        /// </summary>
        /// <param name="mapObject">The instance to construct</param>
        /// <param name="topLeftOrigin">amount to offset this instance based on its parent instance's position</param>
        public void ConstructInstance(MapObject mapObject, Point topLeftOrigin)
        {
            //gets its collision rectangles
            List<GameObject> platformsInThisMapObject = ConstructCollisions(mapObject.MapTiles, topLeftOrigin);

            //adds them to the current total list
            foreach(GameObject platform in platformsInThisMapObject)
            {
                platforms.Add(platform);
            }

            //then contructs this instance's map objects
            LoadMapObjects(mapObject.PlacedMapObjects, topLeftOrigin);

        }

        /// <summary>
        /// Goes through a grid of collision tiles and contructs a list of rectangles to use for collision data in game
        /// </summary>
        /// <param name="originalTiles">The grid of collision tiles</param>
        /// <param name="topLeftOrigin">amount to offset this instance's collisions based on its parent instance's position</param>
        /// <returns>List of rectangles to use for collision data in game</returns>
        public List<GameObject> ConstructCollisions(Color[,] originalTiles, Point topLeftOrigin)
        {
            //copy the original tiles to a new array so we arent modifying the original data
            Color[,] tiles = new Color[originalTiles.GetLength(0), originalTiles.GetLength(1)];
            for (int y = 0; y < originalTiles.GetLength(1); y++)
            {
                for (int x = 0; x < originalTiles.GetLength(0); x++)
                {
                    tiles[x, y] = originalTiles[x, y];
                }
            }

            //start a new list of platforms that can be added to
            List<GameObject> platforms = new List<GameObject>();

            //go through each tile in the grid looking for platforms/collision data to turn into rectangles
            for(int y = 0; y < gameBase.MaxMapHeight; y++)
            {
                for(int x = 0; x < gameBase.MaxMapWidth; x++)
                {
                    //if a grid tile is tomato, that means normal, solid collision data!
                    if(tiles[x, y] == Color.Tomato)
                    {
                        platforms.Add(new GameObject(FindRectangle(tiles, new Point(x, y), topLeftOrigin), CollisionType.Solid));
                    }
                    //if a grid tile is grey, that means semi solid collision data (can jump through but not fall through)!
                    else if (tiles[x, y] == Color.Gray)
                    {
                        platforms.Add(new GameObject(FindRectangle(tiles, new Point(x, y), topLeftOrigin), CollisionType.SemiSolid));
                    }
                    //if a grid tile is red, that means damage collision (like spikes)!
                    else if (tiles[x, y] == Color.Red)
                    {
                        platforms.Add(new GameObject(FindRectangle(tiles, new Point(x, y), topLeftOrigin), CollisionType.Damage));
                    }
                    //if a grid tile is maroon, that means falling pit. instantly lose the stage!
                    else if (tiles[x, y] == Color.Maroon)
                    {
                        platforms.Add(new GameObject(FindRectangle(tiles, new Point(x, y), topLeftOrigin), CollisionType.Fall));
                    }
                }
            }

            return platforms;
        }


        /// <summary>
        /// Assists in constructing a rectangle for collision by finding the rectangles size
        /// </summary>
        /// <param name="tiles">the grid of collision tiles</param>
        /// <param name="rectOrigin">the top left corner of the rectangle</param>
        /// <param name="topLeftOrigin">amount to offset this rectangle based on its parent instance's position</param>
        /// <returns>Returns a full rectangle with position and size scaled for the game</returns>
        private Rectangle FindRectangle(Color[,] tiles, Point rectOrigin, Point topLeftOrigin)
        {
            Color rectColor = tiles[rectOrigin.X, rectOrigin.Y];

            Rectangle finalRectangle = new Rectangle();
            int maxRectWidth = 0;
            int maxRectHeight = 0;

            //counts the largest possible width of the rectangle
            for (int i = rectOrigin.X; i < gameBase.MaxMapWidth; i++)
            {
                if(tiles[i, rectOrigin.Y] == rectColor)
                    maxRectWidth++;
                else
                    break;
            }

            //counts the largest possible height of the rectangle
            for (int j = rectOrigin.Y; j < gameBase.MaxMapHeight; j++)
            {
                if (tiles[rectOrigin.X, j] == rectColor)
                    maxRectHeight++;
                else
                    break;
            }

            //now that we have basically the perimeter of a large possible rectangle, we need to count how many rows
            //are actually full of this one color, and are not interrupted. If a row is interrupted, then our real height of the
            //rectangle is of the previous row.

            bool rowInterrupted = false;
            int finalHeight = 0;

            for (int y = rectOrigin.Y; y < rectOrigin.Y + maxRectHeight; y++)
            {
                for (int x = rectOrigin.X; x < rectOrigin.X + maxRectWidth; x++)
                {
                    if(tiles[x, y] != rectColor)
                    {
                        rowInterrupted = true;
                        break;
                    }
                }
                //if the row has been interrupted, stop counting
                if (rowInterrupted)
                    break;
                //if not though, increase our final height!
                else
                {
                    finalHeight++;

                }

            }


            //before we finish, we need to tell the engine not to count these grid tiles anymore, so set them to black/nothing!
            for (int y = rectOrigin.Y; y < rectOrigin.Y + finalHeight; y++)
            {
                for (int x = rectOrigin.X; x < rectOrigin.X + maxRectWidth; x++)
                {
                    tiles[x, y] = Color.Black;
                }
            }

            //now we can finally set our rectangle's data mutliplied by the scale of the game
            finalRectangle.X = (topLeftOrigin.X + rectOrigin.X) * Game1.Scale;
            finalRectangle.Y = (topLeftOrigin.Y + rectOrigin.Y) * Game1.Scale;
            finalRectangle.Width = maxRectWidth * Game1.Scale;
            finalRectangle.Height = finalHeight * Game1.Scale;


            return finalRectangle;
        }


        /// <summary>
        /// Loads in all .scar files in the given folder path as map objects, and returns a list of them
        /// </summary>
        /// <param name="folderPath">The folder path containing all of the instances</param>
        /// <returns></returns>
        public List<MapObject> LoadInstances(String folderPath)
        {
            bool d = Directory.Exists(folderPath);

            String[] filesInInstanceFolder = Directory.GetFiles(folderPath, "*.scar");

            List<MapObject> instances = new List<MapObject>();

            //loop through all of the scar files found in the folder, constructing map object instances based on them
            foreach (String fileName in filesInInstanceFolder)
            {
                Stream inputStream = null;
                BinaryReader reader = null;
                try
                {
                    //opening the stream to the binary file, creating a reader
                    inputStream = File.OpenRead(fileName);
                    reader = new BinaryReader(inputStream);

                    //reading the position and size of the boundaries of the map
                    int topLeftX = reader.ReadInt32();
                    int topLeftY = reader.ReadInt32();
                    int instanceWidth = reader.ReadInt32();
                    int instanceHeight = reader.ReadInt32();

                    Color[,] instanceTiles = new Color[gameBase.MaxMapWidth, gameBase.MaxMapHeight];

                    //Creates each box in a grid like fashion and places them in the viewport
                    for (int x = 0; x < instanceWidth; x++)
                    {
                        for (int y = 0; y < instanceHeight; y++)
                        {
                            //reading the color from the file
                            System.Drawing.Color loadedColor = System.Drawing.Color.FromArgb(reader.ReadInt32());
                            //storing it in the array!
                            instanceTiles[x + topLeftX, y + topLeftY] = new Color(loadedColor.R, loadedColor.G, loadedColor.B);
                        }
                    }

                    //the amount of map objects that have been placed in this map
                    int numberOfMapObjects = reader.ReadInt32();

                    List<MapObjectRepresentative> instanceObjectRepresentatives = new List<MapObjectRepresentative>();

                    //retrieving the data for all the objects in a map
                    for (int i = 0; i < numberOfMapObjects; i++)
                    {
                        MapObjectRepresentative rep = new MapObjectRepresentative(reader.ReadString(),
                            new Point(reader.ReadInt32(), reader.ReadInt32()));
                        instanceObjectRepresentatives.Add(rep);
                    }

                    //construct the map object instance based on the data collected
                    MapObject loadedInstance = new MapObject(instanceWidth, instanceHeight, instanceTiles, instanceObjectRepresentatives);

                    //finaly, add the loaded instance into the list
                    instances.Add(loadedInstance);

                }
                catch (Exception ex)
                {
                    //error writing file
                    System.Windows.Forms.MessageBox.Show("Error loading instances.",
                        "Error Loading", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                finally
                {
                    //closin the stream
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }

            //return the list of found instances!
            return instances;
        }

    }
}
