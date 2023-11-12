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
//2/21/2020
//FileManager
//This simply holds code for saving, loading, and parsing data from files
namespace Scarlet_Editor
{
    /// <summary>
    /// This simply holds code for saving, loading, and parsing data from files
    /// </summary>
    class FileManager
    {

        /// <summary>
        /// the name/directory for the file we are working on
        /// </summary>
        private String storedFileName;

        /// <summary>
        /// this is the Game class that is storing our map data. Its a reference given through the constructor
        /// </summary>
        private Game1 mainEditor;

        /// <summary>
        /// get the file name we have stored, used to check if a version of this map already exists or not
        /// </summary>
        public String StoredFileName
        {
            get { return storedFileName; }
            set { storedFileName = value; }
        }

        /// <summary>
        /// Constructor is given a reference to the main Game1 class that creates this instance
        /// </summary>
        /// <param name="mainEditor">the main Game1 class that creates this instance</param>
        public FileManager(Game1 mainEditor)
        {
            this.mainEditor = mainEditor;
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
                    int mapWidth = reader.ReadInt32();
                    int mapHeight = reader.ReadInt32();

                    Color[,] mapTiles = new Color[mainEditor.MaxMapWidth, mainEditor.MaxMapHeight];

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

                    List<MapObjectRepresentative> mapObjectRepresentatives = new List<MapObjectRepresentative>();

                    //retrieving the data for all the objects in a map
                    for (int i = 0; i < numberOfMapObjects; i++)
                    {
                        MapObjectRepresentative rep = new MapObjectRepresentative(reader.ReadString(),
                            new Point(reader.ReadInt32(), reader.ReadInt32()));
                        mapObjectRepresentatives.Add(rep);
                    }

                    //construct the map object instance based on the data collected
                    MapObject loadedInstance = new MapObject(mapWidth, mapHeight, mapTiles, mapObjectRepresentatives);

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


        /// <summary>
        /// This method reads info from a binary file as denoted by its file path passed
        /// to this method. It creates the map grid and sets colors based on file's data
        /// </summary>
        /// <param name="fileName">String path to the file</param>
        public void LoadFile(String fileName)
        {

            //creating references for stream and reader
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
                int mapWidth = reader.ReadInt32();
                int mapHeight = reader.ReadInt32();

                Color[,] mapTiles = new Color[mainEditor.MaxMapWidth, mainEditor.MaxMapHeight];

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

                List<MapObjectRepresentative> mapObjectRepresentatives = new List<MapObjectRepresentative>();

                //retrieving the data for all the objects in a map
                for(int i = 0; i < numberOfMapObjects; i++)
                {
                    MapObjectRepresentative rep = new MapObjectRepresentative(reader.ReadString(),
                        new Point(reader.ReadInt32(), reader.ReadInt32()));
                    mapObjectRepresentatives.Add(rep);
                }

                //finaly, set the map data in the editor!
                mainEditor.SetMapData(topLeftX, topLeftY, mapWidth, mapHeight, mapTiles, mapObjectRepresentatives);

                //sets unsaved changes to false because no changes could have been made
                mainEditor.UnsavedChanges = false;

                //Sets the titlebar to have the file's name
                String[] fileNameSplit = fileName.Split('\\');
                mainEditor.Window.Title = "Scarlet Editor - " + fileNameSplit[fileNameSplit.Length - 1];
                storedFileName = fileName;

                //if everything is loaded, show a message box saying so
                System.Windows.Forms.MessageBox.Show("File loaded successfully!",
                    "File Loaded", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                //error writing file
                System.Windows.Forms.MessageBox.Show("Error loading the file.",
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






        /// <summary>
        /// When the Save File button is clicked, this allows the user to select where to save the file,
        /// and then writes the dimensions and colors of the map to that file.
        /// </summary>
        /// <returns>Returns true if file was saved successfully. False if not.
        /// This is used when the user choses to Save when prompted after clicking the
        /// Close button on the editor. If the user doesn't successfully save,
        /// then don't close the editor.</returns>
        public bool SaveFile(String fileName, Boundary mapBoundaries, Color[,] mapTiles, List<MapObjectRepresentative> mapObjectRepresentatives)
        {
            //the map boundaries are used to crop the map so that only the data needed for the map is
            //saved to the file. The goal was to save space in the file. However I ran into an issue where if overwriting
            //a file (basically a normal save), and i went from a larger map size to a smaller one, the extra data would not
            //be removed, leaving to an extra bloated file. Deleting the original file first and then saving it, although
            //a little risky, fixed this issue.
            if (File.Exists(fileName))
                File.Delete(fileName);

            int topLeftX = mapBoundaries.Left;
            int topLeftY = mapBoundaries.Top;
            int mapWidth = mapBoundaries.Width;
            int mapHeight = mapBoundaries.Height;

            //creating references for stream and reader
            Stream outputStream = null;
            BinaryWriter writer = null;
            try
            {
                //opening the stream to the binary file, creating a reader
                outputStream = File.OpenWrite(fileName);
                writer = new BinaryWriter(outputStream);

                //first thing written to file is the map's positions and size
                writer.Write(topLeftX);
                writer.Write(topLeftY);
                writer.Write(mapWidth);
                writer.Write(mapHeight);

                //Creates each box in a grid like fashion and places them in the viewport
                for (int x = 0; x < mapWidth; x++)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        //get the color from the mapTiles array
                        System.Drawing.Color colorToSave = System.Drawing.Color.FromArgb(mapTiles[x + topLeftX, y + topLeftY].R, 
                            mapTiles[x + topLeftX, y + topLeftY].G, mapTiles[x + topLeftX, y + topLeftY].B);

                        //writing the color into the file
                        writer.Write(colorToSave.ToArgb());
                    }
                }

                writer.Write(mapObjectRepresentatives.Count);

                foreach(MapObjectRepresentative rep in mapObjectRepresentatives)
                {
                    writer.Write(rep.ID);
                    writer.Write(rep.Position.X);
                    writer.Write(rep.Position.Y);
                }

                //Sets unsaved changes to false because changes are saved
                mainEditor.UnsavedChanges = false;

                //Sets the titlebar to have the file's name
                String[] fileNameSplit = fileName.Split('\\');
                mainEditor.Window.Title = "Scarlet Editor - " + fileNameSplit[fileNameSplit.Length - 1];
                storedFileName = fileName;

                //if everything as saved, show message box saying so
                System.Windows.Forms.MessageBox.Show("File saved successfully!",
                    "File Saved", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                //error writing file
                System.Windows.Forms.MessageBox.Show("Error writing the file.",
                    "Error Saving", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                //closin the stream
                if (writer != null)
                {
                    writer.Close();
                }
            }
            return false;
        }
    }
}
