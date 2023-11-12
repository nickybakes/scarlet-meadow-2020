using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/19/2020
//Scarlet Editor
//This is the map editor for the Scarlet Meadow game!
namespace Scarlet_Editor
{
    /// <summary>
    /// The tool that the user currently has actived
    /// </summary>
    public enum SelectedTool
    {
        Select,
        Brush,
        Cordon,
        Object,
    }

    /// <summary>
    /// the different buttons on the toolbar
    /// </summary>
    public enum ButtonID
    {
        NewFile,
        OpenFile,
        Save,
        SaveAs,
        ToggleView,
        ResetCamera,
        Select,
        Brush,
        Cordon,
        Object
    }

    /// <summary>
    /// There are 3 types of objects, this enum represents the different modes that the user can be in
    /// when placing objects
    /// 
    /// Instances are other Scar level files that are imported into this one
    /// Entities are game objects in the level like player/enemy spawn, collectibles, the goal
    /// Props are just simple graphics in the level
    /// </summary>
    public enum ObjectMode
    {
        Instance,
        Entity,
        Prop
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        #region ALL OF THE FIELDS NEEDED FOR THE EDITOR ==========================================
        /// <summary>
        /// This is the part of the screen to render the map to
        /// </summary>
        private RenderTarget2D mapViewPort;

        /// <summary>
        /// the original width of the window
        /// </summary>
        private const int PrefferedWidth = 1600;

        /// <summary>
        /// the original height of the window
        /// </summary>
        private const int PrefferedHeight = 900;

        /// <summary>
        /// the height of the toolbar
        /// </summary>
        private const int toolBarHeight = 72;

        /// <summary>
        /// the size of the handles for boundaries
        /// </summary>
        private const int handleSize = 16;

        /// <summary>
        /// the max height for the map grid
        /// </summary>
        private int maxMapHeight = 1024;

        /// <summary>
        /// the max width for the map grid
        /// </summary>
        private int maxMapWidth = 1024;

        /// <summary>
        /// the amount of pixels on screen for each grid tile
        /// </summary>
        private int blockPixelSize = 32;

        /// <summary>
        /// the amount of pixels on screen for the larger grid tiles (orange ones)
        /// </summary>
        private int largeBlockPixelSize = 1024;

        /// <summary>
        /// true if the grid is being rendered as dots, false if rendered as lines
        /// </summary>
        private bool dotMode;

        /// <summary>
        /// If true, we only show the objects on the current layer
        /// </summary>
        private bool renderOnlyCurrentLayerMode;

        /// <summary>
        /// If true, render collision data on top of darkened objects/graphics.
        /// If false, render collision data as transparent on top of objects/graphics
        /// </summary>
        private bool collisionMode;

        /// <summary>
        /// the base texture used to render most of the graphics for the map editor
        /// </summary>
        private Texture2D whiteSquare;

        /// <summary>
        /// the texture used for the handles for boundaries
        /// </summary>
        private Texture2D handleTexture;

        /// <summary>
        /// Arial size 24 sprite font
        /// </summary>
        private SpriteFont arial24;

        /// <summary>
        /// Arial size 18 sprite font
        /// </summary>
        private SpriteFont arial18;

        /// <summary>
        /// Arial size 10 sprite font
        /// </summary>
        private SpriteFont arial10;

        /// <summary>
        /// Consalas size 10 sprite font
        /// </summary>
        private SpriteFont consolas10;

        /// <summary>
        /// Consalas size 14 sprite font
        /// </summary>
        private SpriteFont consolas14;

        /// <summary>
        /// The amount of offset to give when drawing objects on screen. This lets the user pan around the map
        /// </summary>
        private Vector2 cameraOffset;

        /// <summary>
        /// The camera offset when the user started clicking and dragging to pan around the map
        /// </summary>
        private Vector2 originalCameraOffset;

        /// <summary>
        /// The state of the mouse from the previous frame
        /// </summary>
        private MouseState previousMouseState;

        /// <summary>
        /// the state of the mouse in the current frame
        /// </summary>
        private MouseState mouseState;

        /// <summary>
        /// the state of the keyboard in the previous frame
        /// </summary>
        private KeyboardState previousKbState;

        /// <summary>
        /// the state of the keyboard in the current frame
        /// </summary>
        private KeyboardState kbState;

        /// <summary>
        /// When the user starts clicking and dragging to do an action, this stores the starting position of that drag
        /// </summary>
        private Point mouseDragStart;

        /// <summary>
        /// While the user is clicking and dragging, this stores the current/final position of the drag
        /// </summary>
        private Point mouseDragEnd;

        /// <summary>
        /// The tile on the grid that the mouse is currently hovering over
        /// </summary>
        private Point currentMouseSelection;

        /// <summary>
        /// The tile on the grid that the mouse was hovering over in the last frame
        /// </summary>
        private Point previousMouseSelection;

        /// <summary>
        /// the line of text at the top of the screen that displays info like current mouse selection, etc
        /// </summary>
        private string debugTextLine = "---";

        /// <summary>
        /// All the buttons on the toolbar
        /// </summary>
        private Button[] toolBarButtons;

        /// <summary>
        /// the button name to display in the tool tip box
        /// </summary>
        private string toolTipName;

        /// <summary>
        /// the button description to display in the tool tip box
        /// </summary>
        private string toolTipDescription;

        /// <summary>
        /// what tool the user has active right now
        /// </summary>
        private SelectedTool currentTool;

        /// <summary>
        /// the boundaries of the map, can be changed by the Cordon tool. basically crops the map to a certain area
        /// What is outside of these bounds will not be saved to the file
        /// </summary>
        private Boundary mapBoundaries;

        /// <summary>
        /// the handle of a Boundary that the mouse is current hovering over/the user has selected
        /// </summary>
        private Handle selectedHandle;

        /// <summary>
        /// the boundaries of the selection box, created with the Selection Tool. 
        /// user can fill in this box with current color or delete whats in the box (fill with black)
        /// </summary>
        private Boundary selectionBoundaries;

        /// <summary>
        /// true if the user has dragged out a selection. Once true, the user can then resize the selection and move it around
        /// </summary>
        private bool initialSelectionMade;

        /// <summary>
        /// Stores the painted on collision tiles for the map as colors
        /// </summary>
        private Color[,] mapTiles;

        /// <summary>
        /// the size of the brush for when painting on collision tiles
        /// </summary>
        private int brushSize;

        /// <summary>
        /// True if the user is in paint mode: clicking with brush tool sets those tiles to selected Color.
        /// False if user is in Eraser mode: clicking with brush tool sets those tiles to black (nothing).
        /// </summary>
        private bool paintMode;

        /// <summary>
        /// the index on the color palatte array for the currently selected color
        /// </summary>
        private int selectedColorIndex;

        /// <summary>
        /// the color palette array that gives the user some choices of colors for different collision types
        /// </summary>
        private Color[] colorPalette;

        /// <summary>
        /// this holds the code for loading and storing our file!
        /// </summary>
        private FileManager fileManager;

        /// <summary>
        /// True if unsaved changes exist
        /// </summary>
        private bool unsavedChanges;

        /// <summary>
        /// this is the current mode the user has selected when in object tool
        /// </summary>
        private ObjectMode currentObjectMode;

        /// <summary>
        /// the current selected object ID when placing instances
        /// </summary>
        private int currentInstanceID;

        /// <summary>
        /// the current selected object ID when placing entities
        /// </summary>
        private int currentEntityID;

        /// <summary>
        /// the current selected object ID when placing props
        /// </summary>
        private int currentPropID;

        /// <summary>
        /// List of instances that can be selected and placed in the map;
        /// </summary>
        public static List<MapObject> instances;

        /// <summary>
        /// List of entities that can be selected and placed in the map;
        /// </summary>
        public static List<MapObject> entities;

        /// <summary>
        /// List of props that can be selected and placed in the map;
        /// </summary>
        public static List<MapObject> props;

        /// <summary>
        /// the map objects the user has placed in the map
        /// </summary>
        private List<MapObjectRepresentative> placedMapObjects;

        /// <summary>
        /// When the user is in Object mode, drags out a selection box, then presses enter,
        /// any object inside the selection will be added to this list
        /// </summary>
        private List<MapObjectRepresentative> selectedMapObjects;


        /// <summary>
        /// if the user is clicking and dragging to move around selected objects, dont
        /// try to create another selection box
        /// </summary>
        private bool movingAroundSelectedObjects;

        /// <summary>
        /// the currently selected layer for viewing props
        /// </summary>
        private int currentLayer;

        /// <summary>
        /// When the user presses Ctrl + I, they open the "Enter Index Mode", where the ycan type in the index of
        /// an object to quickly change the selected index, rather than having to cycle through with the A and D keys
        /// </summary>
        private bool inEnterIndexMode;

        /// <summary>
        /// When typing in the index they want, we store the user's typing in here!
        /// </summary>
        private string enteredIndex;

        /// <summary>
        /// The map object that is represented by the hoeveredRep field
        /// </summary>
        private MapObject hoveredObject;

        /// <summary>
        /// The map object representative that the mouse is currently hovering over
        /// </summary>
        private MapObjectRepresentative hoveredRep;

        /// <summary>
        /// The amount to nudge objects when the arrow keys are pressed
        /// </summary>
        private int nudgeAmount;

        private float blockWidthOnScreen;

        private float blockHeightOnScreen;

        private Vector2 cameraOffsetOnScreen;


        #endregion

        #region PROPERTIES FOR THE EDITOR ====================================

        /// <summary>
        /// Get and set for unsaved changes bool. if there are unsaved changes, this is true
        /// </summary>
        public bool UnsavedChanges
        {
            get { return unsavedChanges; }
            set { unsavedChanges = value; }
        }

        /// <summary>
        /// returns the max map width limit
        /// </summary>
        public int MaxMapWidth
        {
            get { return maxMapWidth; }
        }

        /// <summary>
        /// returns the max map height limit
        /// </summary>
        public int MaxMapHeight
        {
            get { return maxMapHeight; }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        #endregion

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            
            //sets properties of the window
            Window.AllowUserResizing = true;

            //set the preffered window size
            graphics.PreferredBackBufferHeight = PrefferedHeight;
            graphics.PreferredBackBufferWidth = PrefferedWidth;

            //sets the window to be maximized automatically
            System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            form.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            Window.Title = "Scarlet Editor";

            graphics.ApplyChanges();

            //initialize our file manager instance with a reference to this class
            fileManager = new FileManager(this);

            //initialize all of our data that we need to store
            cameraOffset = new Vector2(0, 0);
            mouseDragStart = new Point(0, 0);
            mouseDragEnd = new Point(0, 0);
            currentMouseSelection = new Point(0, 0);
            mapTiles = new Color[1024, 1024];
            mapBoundaries = new Boundary(0, 0, maxMapWidth, maxMapHeight, maxMapWidth, maxMapHeight);
            mapViewPort = new RenderTarget2D(graphics.GraphicsDevice, PrefferedWidth, PrefferedHeight);

            brushSize = 1;
            paintMode = true;

            //initialize color palette with color choices
            colorPalette = new Color[4];

            colorPalette[0] = Color.Tomato;

            colorPalette[1] = Color.Gray;

            colorPalette[2] = Color.Red;

            colorPalette[3] = Color.Maroon;

            //default Object Mode is on instance
            currentObjectMode = ObjectMode.Instance;

            //initialize the lists of map objects
            instances = new List<MapObject>();

            entities = new List<MapObject>();

            props = new List<MapObject>();

            placedMapObjects = new List<MapObjectRepresentative>();

            //default tool is Selection tool
            SetCurrentToolToSelect();

            //default display mode is Dot because its easier on the eyes
            dotMode = true;

            renderOnlyCurrentLayerMode = false;

            enteredIndex = "";

            //default mode is object mode
            collisionMode = false;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //gets the form for this editor window and changes the Close event so it warns the user about unsaved changes
            System.Windows.Forms.Form thisForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            if (thisForm != null)
            {
                thisForm.FormClosing += EditorForm_FormClosing;
            }

            //load in our sprite fonts and base textures
            whiteSquare = Content.Load<Texture2D>("editor/whiteSquare64");
            handleTexture = Content.Load<Texture2D>("editor/handle");
            arial18 = Content.Load<SpriteFont>("Arial18");
            arial24 = Content.Load<SpriteFont>("Arial24");
            arial10 = Content.Load<SpriteFont>("Arial10");
            consolas10 = Content.Load<SpriteFont>("Consolas10");
            consolas14 = Content.Load<SpriteFont>("Consolas14");

            #region LOADING IN TOOLBAR ASSETS ===============================================================================================
            //load in our button textures, initialize them, and link up those buttons 
            //OnClick events to appropriate methods
            toolBarButtons = new Button[10];

            toolBarButtons[(int)ButtonID.NewFile] = new Button(Content.Load<Texture2D>("editor/button-newFile"),
                10, 20, 50, 50, "New File (Ctrl + N)", "Creates an empty map ready to be edited.");
            toolBarButtons[(int)ButtonID.NewFile].OnClick += NewFile;

            toolBarButtons[(int)ButtonID.OpenFile] = new Button(Content.Load<Texture2D>("editor/button-openFile"),
                60, 20, 50, 50, "Open File (Ctrl + O)", "Opens a file dialog so you can browse and open an alreay saved map.");
            toolBarButtons[(int)ButtonID.OpenFile].OnClick += OpenFile;

            toolBarButtons[(int)ButtonID.Save] = new Button(Content.Load<Texture2D>("editor/button-save"),
                110, 20, 50, 50, "Save Map (Ctrl + S)", "Saves this map to a file (browse for a location if this is the first time saving).");
            toolBarButtons[(int)ButtonID.Save].OnClick += SaveFileButtonClick;

            toolBarButtons[(int)ButtonID.SaveAs] = new Button(Content.Load<Texture2D>("editor/button-saveAs"),
                160, 20, 50, 50, "Save As (Ctrl + Shift + S)", "Saves this map to a new file (browse for a location).");
            toolBarButtons[(int)ButtonID.SaveAs].OnClick += SaveFileAsButtonClick;

            toolBarButtons[(int)ButtonID.ToggleView] = new Button(Content.Load<Texture2D>("editor/button-toggleView"),
                360, 20, 50, 50, "Toggle View (T)", "Toggle between Collision Paint View and Object View.");
            toolBarButtons[(int)ButtonID.ToggleView].OnClick += ToggleView;

            toolBarButtons[(int)ButtonID.ResetCamera] = new Button(Content.Load<Texture2D>("editor/button-resetCamera"),
                410, 20, 50, 50, "Reset Camera (0)", "Reset the camera offset back to (0,0).");
            toolBarButtons[(int)ButtonID.ResetCamera].OnClick += ResetCamera;

            toolBarButtons[(int)ButtonID.Select] = new Button(Content.Load<Texture2D>("editor/tool-select"),
                510, 20, 50, 50, "Selection Tool (S)", "Click and drag to make a selection. In Collision View, (F) fills with current color, " +
                "(Del) to fill with black (nothing). \nIn Object View, Click, CTRL + Click, or press (Enter) with selection box to select objects. Click on objects and drag to move them!");
            toolBarButtons[(int)ButtonID.Select].OnClick += SetCurrentToolToSelect;

            toolBarButtons[(int)ButtonID.Brush] = new Button(Content.Load<Texture2D>("editor/tool-brush"),
                560, 20, 50, 50, "Brush Tool (B)", "Paint collision data as tiles on the map. Collision type (color), brush mode, and size can be changed.");
            toolBarButtons[(int)ButtonID.Brush].OnClick += SetCurrentToolToBrush;

            toolBarButtons[(int)ButtonID.Cordon] = new Button(Content.Load<Texture2D>("editor/tool-cordon"),
                610, 20, 50, 50, "Cordon Tool (C)", "Adjust the boundaries of the map. What is outside of the cordon will NOT be saved when this map is closed.");
            toolBarButtons[(int)ButtonID.Cordon].OnClick += SetCurrentToolToCordon;

            toolBarButtons[(int)ButtonID.Object] = new Button(Content.Load<Texture2D>("editor/tool-object"),
                660, 20, 50, 50, "Object Tool (O)", "Place instances of other maps, props, and entities like enemy spawns, items, and the player spawn.");
            toolBarButtons[(int)ButtonID.Object].OnClick += SetCurrentToolToObject;

            #endregion

            #region LOADING IN INSTANCE ASSETS ============================================================================================
            instances = fileManager.LoadInstances(@"..\..\..\..\Content\instances");
            #endregion

            #region LOADING IN ENTITY ASSETS ============================================================================================
            entities.Add(new MapObject(2, 4, Content.Load<Texture2D>("entities/playerSpawn")));
            entities.Add(new MapObject(9, 3, Content.Load<Texture2D>("entities/trampolineFacingFront")));
            entities.Add(new MapObject(7, 3, Content.Load<Texture2D>("entities/trampolineFacingLeft")));
            entities.Add(new MapObject(7, 3, Content.Load<Texture2D>("entities/trampolineFacingRight")));
            entities.Add(new MapObject(12, 4, Content.Load<Texture2D>("entities/zipLineGoingLeftShort")));
            entities.Add(new MapObject(12, 4, Content.Load<Texture2D>("entities/zipLineGoingRightShort"))); //5
            entities.Add(new MapObject(3, 7, Content.Load<Texture2D>("entities/lawmakerFacingRight")));
            entities.Add(new MapObject(3, 7, Content.Load<Texture2D>("entities/lawmakerFacingLeft")));
            entities.Add(new MapObject(4, 6, Content.Load<Texture2D>("entities/robberBaronFacingRight")));
            entities.Add(new MapObject(4, 6, Content.Load<Texture2D>("entities/robberBaronFacingLeft")));
            entities.Add(new MapObject(3, 4, Content.Load<Texture2D>("entities/cactusFacingRight"))); //10
            entities.Add(new MapObject(3, 4, Content.Load<Texture2D>("entities/cactusFacingLeft")));
            entities.Add(new MapObject(3, 3, Content.Load<Texture2D>("entities/tumbleweedRight")));
            entities.Add(new MapObject(3, 3, Content.Load<Texture2D>("entities/tumbleweedLeft")));
            entities.Add(new MapObject(1, 1, Content.Load<Texture2D>("entities/happyskull")));
            entities.Add(new MapObject(2, 2, Content.Load<Texture2D>("entities/shockedskull"))); //15
            entities.Add(new MapObject(2, 2, Content.Load<Texture2D>("entities/luchadormask")));
            entities.Add(new MapObject(2, 2, Content.Load<Texture2D>("entities/jalapeno")));
            entities.Add(new MapObject(1, 2, Content.Load<Texture2D>("entities/badgebitbig")));
            entities.Add(new MapObject(1, 1, Content.Load<Texture2D>("entities/badgebitsmall")));
            entities.Add(new MapObject(6, 6, Content.Load<Texture2D>("entities/goal-normal"))); //20
            entities.Add(new MapObject(6, 6, Content.Load<Texture2D>("entities/goal-timelimit")));
            entities.Add(new MapObject(6, 6, Content.Load<Texture2D>("entities/goal-defeatenemies")));
            entities.Add(new MapObject(6, 6, Content.Load<Texture2D>("entities/goal-savecivs")));
            entities.Add(new MapObject(2, 2, Content.Load<Texture2D>("entities/add5sec")));
            entities.Add(new MapObject(3, 4, Content.Load<Texture2D>("entities/civilian"))); //25
            entities.Add(new MapObject(2, 2, Content.Load<Texture2D>("entities/background-day")));
            entities.Add(new MapObject(2, 2, Content.Load<Texture2D>("entities/background-evening")));
            entities.Add(new MapObject(2, 2, Content.Load<Texture2D>("entities/background-night")));
            entities.Add(new MapObject(2, 2, Content.Load<Texture2D>("entities/background-morning")));

            #endregion


            // Layers of props to display for the stage
            // 0 - background foliage
            // 1 - front walls, indoor trims
            // 2 - side walls, entrance back halfs, patches
            // 3 - window fills, outdoor trims
            // 4 - edges, windows
            // 5 - graffiti
            // 6 - floor, ground vines
            // -----***  ENTITIES LAYER  ***-----
            // 7 - entrance front halfs
            // 8 - balconies, vine0.png, roofs
            #region LOADING IN PROP ASSETS ============================================================================================
            props.Add(new MapObject(1, 3, Content.Load<Texture2D>("props/balconyMid"), 8));
            props.Add(new MapObject(7, 1, Content.Load<Texture2D>("props/platform"), 6));

            string[] colors = {"blue", "green", "orange", "pink", "purple", "yellow"};
            string[] directions = {"front", "left", "right", "side", "back" };
            for(int i = 0; i < colors.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    props.Add(new MapObject(4, 3, Content.Load<Texture2D>(string.Format("props/balconies/balcony-{0}-{1}", colors[i], directions[j])), 8));
                }
            }

            props.Add(new MapObject(9, 3, Content.Load<Texture2D>("props/decor/foliage0"), 0));
            props.Add(new MapObject(8, 8, Content.Load<Texture2D>("props/decor/graffiti0"), 5));
            props.Add(new MapObject(8, 4, Content.Load<Texture2D>("props/decor/graffiti1"), 5));
            props.Add(new MapObject(5, 5, Content.Load<Texture2D>("props/decor/graffiti2"), 5));
            props.Add(new MapObject(6, 4, Content.Load<Texture2D>("props/decor/graffiti3"), 5));
            props.Add(new MapObject(8, 8, Content.Load<Texture2D>("props/decor/graffiti4"), 5));
            props.Add(new MapObject(2, 2, Content.Load<Texture2D>("props/decor/patch0"), 2));
            props.Add(new MapObject(2, 2, Content.Load<Texture2D>("props/decor/patch1"), 2));
            props.Add(new MapObject(5, 4, Content.Load<Texture2D>("props/decor/patch2"), 2));
            props.Add(new MapObject(5, 4, Content.Load<Texture2D>("props/decor/patch3"), 2));
            props.Add(new MapObject(3, 2, Content.Load<Texture2D>("props/decor/patch4"), 2));
            props.Add(new MapObject(4, 3, Content.Load<Texture2D>("props/decor/patch5"), 2));
            props.Add(new MapObject(1, 2, Content.Load<Texture2D>("props/decor/vine0"), 8));
            props.Add(new MapObject(3, 3, Content.Load<Texture2D>("props/decor/vine-ground0"), 6));
            props.Add(new MapObject(3, 3, Content.Load<Texture2D>("props/decor/vine-ground1"), 6));

            for (int i = 0; i < colors.Length; i++)
            {
                if(i == 5)
                {
                    props.Add(new MapObject(1, 12, Content.Load<Texture2D>("props/edges/edge-white-front"), 4));
                    props.Add(new MapObject(3, 12, Content.Load<Texture2D>("props/edges/edge-white-side"), 4));
                }
                props.Add(new MapObject(1, 12, Content.Load<Texture2D>(string.Format("props/edges/edge-{0}-front", colors[i])), 4));
                props.Add(new MapObject(3, 12, Content.Load<Texture2D>(string.Format("props/edges/edge-{0}-side", colors[i])), 4));
            }

            for (int i = 0; i < colors.Length; i++)
            {
                props.Add(new MapObject(2, 12, Content.Load<Texture2D>(string.Format("props/entrances/entrance-{0}-left-back", colors[i])), 2));
                props.Add(new MapObject(1, 12, Content.Load<Texture2D>(string.Format("props/entrances/entrance-{0}-left-front", colors[i])), 7));
                props.Add(new MapObject(2, 12, Content.Load<Texture2D>(string.Format("props/entrances/entrance-{0}-right-back", colors[i])), 2));
                props.Add(new MapObject(1, 12, Content.Load<Texture2D>(string.Format("props/entrances/entrance-{0}-right-front", colors[i])), 7));
            }

            props.Add(new MapObject(3, 1, Content.Load<Texture2D>("props/floors/floor-tile-dark"), 6));
            props.Add(new MapObject(3, 1, Content.Load<Texture2D>("props/floors/floor-tile-light"), 6));
            props.Add(new MapObject(3, 6, Content.Load<Texture2D>("props/floors/floor-under"), 6));
            props.Add(new MapObject(3, 1, Content.Load<Texture2D>("props/floors/floor-wood-dark"), 6));
            props.Add(new MapObject(3, 1, Content.Load<Texture2D>("props/floors/floor-wood-light"), 6));

            props.Add(new MapObject(12, 1, Content.Load<Texture2D>("props/trims/roof-front"), 8));
            props.Add(new MapObject(3, 1, Content.Load<Texture2D>("props/trims/roof-side"), 8));
            props.Add(new MapObject(12, 3, Content.Load<Texture2D>("props/trims/trim-brick-dark"), 1));
            props.Add(new MapObject(12, 3, Content.Load<Texture2D>("props/trims/trim-brick-light"), 1));
            props.Add(new MapObject(12, 3, Content.Load<Texture2D>("props/trims/trim-white-front"), 3));
            props.Add(new MapObject(3, 3, Content.Load<Texture2D>("props/trims/trim-white-side"), 3));
            props.Add(new MapObject(12, 3, Content.Load<Texture2D>("props/trims/trim-wood-dark"), 1));
            props.Add(new MapObject(12, 3, Content.Load<Texture2D>("props/trims/trim-wood-light"), 1));

            props.Add(new MapObject(4, 9, Content.Load<Texture2D>("props/walls/concrete-wall"), 1));
            props.Add(new MapObject(12, 12, Content.Load<Texture2D>("props/walls/indoor-wall-dark"), 1));
            props.Add(new MapObject(12, 12, Content.Load<Texture2D>("props/walls/indoor-wall-light"), 1));
            for (int i = 0; i < colors.Length; i++)
            {
                props.Add(new MapObject(12, 12, Content.Load<Texture2D>(string.Format("props/walls/wall-{0}-front", colors[i])), 1));
                props.Add(new MapObject(3, 12, Content.Load<Texture2D>(string.Format("props/walls/wall-{0}-side", colors[i])), 2));
            }

            props.Add(new MapObject(4, 5, Content.Load<Texture2D>("props/windows/fill-dark-front"), 3));
            props.Add(new MapObject(3, 5, Content.Load<Texture2D>("props/windows/fill-dark-side"), 3));
            props.Add(new MapObject(4, 5, Content.Load<Texture2D>("props/windows/fill-light-front"), 3));
            props.Add(new MapObject(3, 5, Content.Load<Texture2D>("props/windows/fill-light-side"), 3));
            props.Add(new MapObject(4, 5, Content.Load<Texture2D>("props/windows/fill-med-front"), 3));
            props.Add(new MapObject(3, 5, Content.Load<Texture2D>("props/windows/fill-med-side"), 3));

            for (int i = 0; i < colors.Length; i++)
            {
                props.Add(new MapObject(4, 5, Content.Load<Texture2D>(string.Format("props/windows/window-{0}-front", colors[i])), 4));
                props.Add(new MapObject(3, 5, Content.Load<Texture2D>(string.Format("props/windows/window-{0}-side", colors[i])), 4));
            }



            #endregion

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (initialSelectionMade && selectionBoundaries == null)
            {
                initialSelectionMade = false;
            }

            if (!IsActive)
                return;

            //get keyboard and mouse states
            previousKbState = kbState;

            kbState = Keyboard.GetState();

            previousMouseState = mouseState;

            mouseState = Mouse.GetState();


            #region UPDATING THE TOOLBAR ===========================================================================================

            //reset the tool tip info
            toolTipName = null;
            toolTipDescription = null;

            //loop through buttons, checking if the mouse is inside of it
            for (int i = 0; i < toolBarButtons.Length; i++)
            {
                //tell the button so it knows to draw with the highlighted color or not
                toolBarButtons[i].Selected = toolBarButtons[i].Bounds.Contains(mouseState.Position);
                if (toolBarButtons[i].Selected)
                {
                    //if mouse is hovering over it, set the tool tip info
                    toolTipName = toolBarButtons[i].Name;
                    toolTipDescription = toolBarButtons[i].Description;


                    //if the mouse is inside the bounds of this button and 
                    //the left button is pressed (but wasnt pressed on the previous frame)
                    if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                    {
                        //"click" this button!
                        if (toolBarButtons[i].ClickButton != null)
                            toolBarButtons[i].ClickButton();
                    }
                }
            }

            #endregion

            #region GETTING THE TILE SELECTED BY THE MOUSE ==============================================================================

            //set the previous mouse selection to the one from the last frame
            previousMouseSelection = currentMouseSelection;

            //this block of code takes the mouse position on screen, and factoring the zoom level, camera offset, and window size
            //that the user has, figures out which tile on the grid the mouse is hovering over

            //factors in the size of the grid tiles on screen
            blockWidthOnScreen = (blockPixelSize * (GraphicsDevice.Viewport.Width / (float)PrefferedWidth));
            blockHeightOnScreen = (blockPixelSize * ((GraphicsDevice.Viewport.Height - toolBarHeight) / (float)PrefferedHeight));

            //factors in the offset of the grid on screen
            cameraOffsetOnScreen = new Vector2(cameraOffset.X * (GraphicsDevice.Viewport.Width / (float)PrefferedWidth)
                , cameraOffset.Y * ((GraphicsDevice.Viewport.Height - toolBarHeight) / (float)PrefferedHeight));

            //finally, gets the user's selection based on these factors
            currentMouseSelection.X = (int)((mouseState.X + (int)cameraOffsetOnScreen.X) / blockWidthOnScreen);
            currentMouseSelection.Y = (int)((mouseState.Y - toolBarHeight + (int)cameraOffsetOnScreen.Y) / blockHeightOnScreen);

            #endregion

            #region ZOOMING IN AND OUT ====================================================================================

            //if mouse scroll is smaller than before, user is scrolling down, zoom OUT
            if (mouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue && largeBlockPixelSize > 32)
            {
                //decrease the pixel size of the blocks
                largeBlockPixelSize /= 2;
                blockPixelSize = largeBlockPixelSize / 32;

                //adjust the camera offset so the user is somewhat looking in the same area after zooming
                if (cameraOffset.X > 0)
                    cameraOffset.X = (int)(cameraOffset.X / 2.25);
                if (cameraOffset.Y > 0)
                    cameraOffset.Y = (int)(cameraOffset.Y / 2.25);



                //if the mouse is on the left area of the view port
                if (mouseState.X < mapViewPort.Bounds.Center.X / 2)
                {
                    //adjust the camera offset so they are zooming out from the left
                    cameraOffset.X += largeBlockPixelSize;
                }

                //right area of viewport
                else if (mouseState.X > mapViewPort.Bounds.Center.X + (mapViewPort.Bounds.Width / 4))
                {
                    //adjust the camera offset so they are zooming out from the right
                    cameraOffset.X -= largeBlockPixelSize;
                }


                //if the mouse is on the top area of the view port
                if (mouseState.Y < mapViewPort.Bounds.Center.Y / 2)
                {
                    //adjust the camera offset so they are zooming out from the top
                    cameraOffset.Y += largeBlockPixelSize;
                }

                //bottom area of viewport
                else if (mouseState.Y > mapViewPort.Bounds.Center.Y + (mapViewPort.Bounds.Height / 4))
                {
                    //adjust the camera offset so they are zooming out from the bottom
                    cameraOffset.Y -= largeBlockPixelSize;
                }

                //this helps if the user is on the max edge (1024) of the map; in case the camera offset is
                //moved way out into the void, this pushes it back close to the grid
                if (cameraOffset.X > largeBlockPixelSize * 32)
                    cameraOffset.X = largeBlockPixelSize * 31.5f;
                if (cameraOffset.Y > largeBlockPixelSize * 32)
                    cameraOffset.Y = largeBlockPixelSize * 31.5f;
            }


            //if mouse scroll is larger than before, user is scrolling up, zoom IN
            else if (mouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue && largeBlockPixelSize < 4096)
            {
                //increase the pixel size of the blocks
                largeBlockPixelSize *= 2;
                blockPixelSize = largeBlockPixelSize / 32;

                //adjust the camera offset so the user is somewhat looking in the same area after zooming
                if (cameraOffset.X > 0)
                    cameraOffset.X = (int)(cameraOffset.X * 2.25);
                if (cameraOffset.Y > 0)
                    cameraOffset.Y = (int)(cameraOffset.Y * 2.25);

                //if the mouse is on the left area of the view port
                if (mouseState.X < mapViewPort.Bounds.Center.X / 2)
                {
                    //adjust the camera offset so they are zooming into the left
                    cameraOffset.X -= largeBlockPixelSize;
                }

                //right area of view port
                else if (mouseState.X > mapViewPort.Bounds.Center.X + (mapViewPort.Bounds.Width / 4))
                {
                    //adjust the camera offset so they are zooming into the right
                    cameraOffset.X += largeBlockPixelSize;
                }

                //if the mouse is on the top area of the view port
                if (mouseState.Y < mapViewPort.Bounds.Center.Y / 2)
                {
                    //adjust the camera offset so they are zooming into the top
                    cameraOffset.Y -= largeBlockPixelSize;
                }

                //bottom area of view port
                else if (mouseState.Y > mapViewPort.Bounds.Center.Y + (mapViewPort.Bounds.Height / 4))
                {
                    //adjust the camera offset so they are zooming into the bottom
                    cameraOffset.Y += largeBlockPixelSize;
                }


                //this helps if the user is on the max edge (1024) of the map; in case the camera offset is
                //moved way out into the void, this pushes it back close to the grid
                if (cameraOffset.X > largeBlockPixelSize * 32)
                    cameraOffset.X = largeBlockPixelSize * 31.5f;
                if (cameraOffset.Y > largeBlockPixelSize * 32)
                    cameraOffset.Y = largeBlockPixelSize * 31.5f;
            }

            #endregion

            #region ENTER INDEX MODE CONTROL ==========================================================================
            if (inEnterIndexMode)
            {
                //let the user type in numbers if the string has less than 3 characters in it (limit the chosen index to 3 digits
                if ((previousKbState.IsKeyDown(Keys.D0) && kbState.IsKeyUp(Keys.D0) || previousKbState.IsKeyDown(Keys.NumPad0) && kbState.IsKeyUp(Keys.NumPad0)) && enteredIndex.Length < 3)
                    enteredIndex += "0";
                if ((kbState.IsKeyDown(Keys.D1) && previousKbState.IsKeyUp(Keys.D1) || previousKbState.IsKeyDown(Keys.NumPad1) && kbState.IsKeyUp(Keys.NumPad1)) && enteredIndex.Length < 3)
                    enteredIndex += "1";
                if ((kbState.IsKeyDown(Keys.D2) && previousKbState.IsKeyUp(Keys.D2) || previousKbState.IsKeyDown(Keys.NumPad2) && kbState.IsKeyUp(Keys.NumPad2)) && enteredIndex.Length < 3)
                    enteredIndex += "2";
                if ((kbState.IsKeyDown(Keys.D3) && previousKbState.IsKeyUp(Keys.D3) || previousKbState.IsKeyDown(Keys.NumPad3) && kbState.IsKeyUp(Keys.NumPad3)) && enteredIndex.Length < 3)
                    enteredIndex += "3";
                if ((kbState.IsKeyDown(Keys.D4) && previousKbState.IsKeyUp(Keys.D4) || previousKbState.IsKeyDown(Keys.NumPad4) && kbState.IsKeyUp(Keys.NumPad4)) && enteredIndex.Length < 3)
                    enteredIndex += "4";
                if ((kbState.IsKeyDown(Keys.D5) && previousKbState.IsKeyUp(Keys.D5) || previousKbState.IsKeyDown(Keys.NumPad5) && kbState.IsKeyUp(Keys.NumPad5)) && enteredIndex.Length < 3)
                    enteredIndex += "5";
                if ((kbState.IsKeyDown(Keys.D6) && previousKbState.IsKeyUp(Keys.D6) || previousKbState.IsKeyDown(Keys.NumPad6) && kbState.IsKeyUp(Keys.NumPad6)) && enteredIndex.Length < 3)
                    enteredIndex += "6"; 
                if ((kbState.IsKeyDown(Keys.D7) && previousKbState.IsKeyUp(Keys.D7) || previousKbState.IsKeyDown(Keys.NumPad7) && kbState.IsKeyUp(Keys.NumPad7)) && enteredIndex.Length < 3)
                    enteredIndex += "7";
                if ((kbState.IsKeyDown(Keys.D8) && previousKbState.IsKeyUp(Keys.D8) || previousKbState.IsKeyDown(Keys.NumPad8) && kbState.IsKeyUp(Keys.NumPad8)) && enteredIndex.Length < 3)
                    enteredIndex += "8";
                if ((kbState.IsKeyDown(Keys.D9) && previousKbState.IsKeyUp(Keys.D9) || previousKbState.IsKeyDown(Keys.NumPad9) && kbState.IsKeyUp(Keys.NumPad9)) && enteredIndex.Length < 3)
                    enteredIndex += "9";
                //let the user cancel the operation
                if(kbState.IsKeyDown(Keys.Escape) && previousKbState.IsKeyUp(Keys.Escape))
                {
                    enteredIndex = "";
                    inEnterIndexMode = false;
                }
                //if the user presses the backspace or delete key key, then delete the rightmost digit
                if (kbState.IsKeyDown(Keys.Back) && previousKbState.IsKeyUp(Keys.Back) || kbState.IsKeyDown(Keys.Delete) && previousKbState.IsKeyUp(Keys.Delete))
                {
                    if(enteredIndex.Length > 0)
                    {
                        enteredIndex = enteredIndex.Substring(0, enteredIndex.Length - 1);
                    }
                }

                //if the user presses Enter or Space, then apply the changed index
                if ((kbState.IsKeyDown(Keys.Enter) && previousKbState.IsKeyUp(Keys.Enter)) || (kbState.IsKeyDown(Keys.Space) && previousKbState.IsKeyUp(Keys.Space)))
                {
                    if (enteredIndex.Length > 0)
                    {
                        //first we parse it from the string
                        int newIndex = int.Parse(enteredIndex);

                        //if the new index is to large, we clamp it down to the end of the list of current objects
                        switch (currentObjectMode)
                        {
                            case (ObjectMode.Instance):
                                if (newIndex < instances.Count)
                                    currentInstanceID = newIndex;
                                else
                                    currentInstanceID = instances.Count - 1;
                                break;

                            case (ObjectMode.Entity):
                                if (newIndex < entities.Count)
                                    currentEntityID = newIndex;
                                else
                                    currentEntityID = entities.Count - 1;
                                break;

                            case (ObjectMode.Prop):
                                if (newIndex < props.Count)
                                    currentPropID = newIndex;
                                else
                                    currentPropID = props.Count - 1;
                                break;
                        }

                        SetCurrentToolToObject();
                    }
                    enteredIndex = "";
                    inEnterIndexMode = false;
                }

                //we still allow the user to switch between Object modes
                if (kbState.IsKeyDown(Keys.Q) && previousKbState.IsKeyUp(Keys.Q))
                {
                    currentObjectMode = ObjectMode.Instance;
                }
                else if (kbState.IsKeyDown(Keys.W) && previousKbState.IsKeyUp(Keys.W))
                {
                    currentObjectMode = ObjectMode.Entity;
                }
                else if (kbState.IsKeyDown(Keys.E) && previousKbState.IsKeyUp(Keys.E))
                {
                    currentObjectMode = ObjectMode.Prop;
                }
                //add some extra dashes to the display string to show how much type space is left
                string extraDashes = "";
                for (int i = enteredIndex.Length; i < 3; i++)
                    extraDashes += "_";

                toolTipName = "Entered Index: " + enteredIndex + extraDashes;
                //depending on the current object mode, tell the user what indexes they can choose from
                switch (currentObjectMode)
                {
                    case (ObjectMode.Instance):
                        toolTipDescription = string.Format("[Q,W,E] Instance indexes: 0 - {0} (inclusive)", instances.Count - 1);
                        break;

                    case (ObjectMode.Entity):
                        toolTipDescription = "[Q,W,E] Entity indexes: Misc 0-5, Enemies 6-13, Items 14-19, Goals 20-25, Backgrounds 26-29";
                        break;

                    case (ObjectMode.Prop):
                        toolTipDescription = "[Q,W,E] Prop indexes: Balconies 2-19, Decor 20-34, Edges 35-48, Entrances 49-72, Floors 73-77, Trims 78-85, Walls 86-100, Windows 101-118";
                        break;
                }


                base.Update(gameTime);

                return;
            }

            #endregion

            nudgeAmount = 1;

            #region DETERMINING IF GLOBAL HOTKEYS ARE PRESSED ===============================================================

            if (kbState.IsKeyDown(Keys.LeftControl) || kbState.IsKeyDown(Keys.RightControl))
            {
                nudgeAmount = 10;

                if (kbState.IsKeyDown(Keys.N) && previousKbState.IsKeyUp(Keys.N))
                {
                    NewFile();
                }
                else if (kbState.IsKeyDown(Keys.O) && previousKbState.IsKeyUp(Keys.O))
                {
                    OpenFile();
                }
                else if (kbState.IsKeyUp(Keys.LeftShift) && kbState.IsKeyDown(Keys.S) && previousKbState.IsKeyUp(Keys.S))
                {
                    SaveFile();
                }
                else if (kbState.IsKeyDown(Keys.LeftShift) && kbState.IsKeyDown(Keys.S) && previousKbState.IsKeyUp(Keys.S))
                {
                    SaveFileAs();
                }
                //pressing ctrl I or Ctrl E opens the Enter Index mode
                else if (kbState.IsKeyDown(Keys.I) && previousKbState.IsKeyUp(Keys.I))
                {
                    inEnterIndexMode = true;
                }
                else if (kbState.IsKeyDown(Keys.Q) && previousKbState.IsKeyUp(Keys.Q))
                {
                    inEnterIndexMode = true;
                    currentObjectMode = ObjectMode.Instance;
                }
                else if (kbState.IsKeyDown(Keys.W) && previousKbState.IsKeyUp(Keys.W))
                {
                    inEnterIndexMode = true;
                    currentObjectMode = ObjectMode.Entity;
                }
                else if (kbState.IsKeyDown(Keys.E) && previousKbState.IsKeyUp(Keys.E))
                {
                    inEnterIndexMode = true;
                    currentObjectMode = ObjectMode.Prop;
                }
                //pressing Ctrl B sends the selected objects to the back of the render list (front of the placed map objects list)
                else if ((kbState.IsKeyDown(Keys.B) && previousKbState.IsKeyUp(Keys.B)))
                {
                    if(selectedMapObjects != null)
                    {
                        foreach(MapObjectRepresentative rep in selectedMapObjects)
                        {
                            placedMapObjects.Remove(rep);
                        }
                        foreach (MapObjectRepresentative rep in selectedMapObjects)
                        {
                            placedMapObjects.Insert(0, rep);
                        }
                    }
                }
                //pressing Ctrl F sends the selected objects to the front of the render list (back of the placed map objects list)
                else if ((kbState.IsKeyDown(Keys.F) && previousKbState.IsKeyUp(Keys.F)))
                {
                    if (selectedMapObjects != null)
                    {
                        foreach (MapObjectRepresentative rep in selectedMapObjects)
                        {
                            placedMapObjects.Remove(rep);
                        }
                        foreach (MapObjectRepresentative rep in selectedMapObjects)
                        {
                            placedMapObjects.Add(rep);
                        }
                    }
                }
            }
            else if (kbState.IsKeyDown(Keys.LeftShift) || kbState.IsKeyDown(Keys.RightShift))
            {
                if (kbState.IsKeyDown(Keys.E) && previousKbState.IsKeyUp(Keys.E))
                {
                    SetCurrentToolToObject();
                    currentObjectMode = ObjectMode.Prop;
                }
                else if (kbState.IsKeyDown(Keys.W) && previousKbState.IsKeyUp(Keys.W))
                {
                    SetCurrentToolToObject();
                    currentObjectMode = ObjectMode.Entity;
                }
                else if (kbState.IsKeyDown(Keys.Q) && previousKbState.IsKeyUp(Keys.Q))
                {
                    SetCurrentToolToObject();
                    currentObjectMode = ObjectMode.Instance;
                }
            }
            else 
            {
                //S key sets current tool to Selection tool
                if (kbState.IsKeyDown(Keys.S) && previousKbState.IsKeyUp(Keys.S))
                {
                    SetCurrentToolToSelect();
                }
                //B key sets current tool to Brush tool
                else if (kbState.IsKeyDown(Keys.B) && previousKbState.IsKeyUp(Keys.B))
                {
                    SetCurrentToolToBrush();
                }
                //C key sets current tool to Cordon tool
                else if (kbState.IsKeyDown(Keys.C) && previousKbState.IsKeyUp(Keys.C))
                {
                    SetCurrentToolToCordon();
                }
                //O key sets current tool to Object tool
                else if (kbState.IsKeyDown(Keys.O) && previousKbState.IsKeyUp(Keys.O))
                {
                    SetCurrentToolToObject();
                }
                //T key toggles between collision and object view
                else if (kbState.IsKeyDown(Keys.T) && previousKbState.IsKeyUp(Keys.T))
                {
                    ToggleView();
                }            
                //if the user presses the 0 key, reset the camera position to (0,0)
                //great if they get lost in the void
                else if (previousKbState.IsKeyDown(Keys.D0) && kbState.IsKeyUp(Keys.D0))
                {
                    ResetCamera();
                }
                //changing the current layer with keys 1, 2, and 3
                else if (kbState.IsKeyDown(Keys.D1) && previousKbState.IsKeyUp(Keys.D1))
                    currentLayer = 0;
                else if (kbState.IsKeyDown(Keys.D2) && previousKbState.IsKeyUp(Keys.D2))
                    currentLayer = 1;
                else if (kbState.IsKeyDown(Keys.D3) && previousKbState.IsKeyUp(Keys.D3))
                    currentLayer = 2;
                else if (kbState.IsKeyDown(Keys.D4) && previousKbState.IsKeyUp(Keys.D4))
                    currentLayer = 3;
                else if (kbState.IsKeyDown(Keys.D5) && previousKbState.IsKeyUp(Keys.D5))
                    currentLayer = 4;
                else if (kbState.IsKeyDown(Keys.D6) && previousKbState.IsKeyUp(Keys.D6))
                    currentLayer = 5;
                else if (kbState.IsKeyDown(Keys.D7) && previousKbState.IsKeyUp(Keys.D7))
                    currentLayer = 6;
                else if (kbState.IsKeyDown(Keys.D8) && previousKbState.IsKeyUp(Keys.D8))
                    currentLayer = 7;
                else if (kbState.IsKeyDown(Keys.D9) && previousKbState.IsKeyUp(Keys.D9))
                    currentLayer = 8;
                //if the user pressed the G key on this frame, toggle between displaying the grid as dots or lines
                else if (kbState.IsKeyDown(Keys.G) && previousKbState.IsKeyUp(Keys.G))
                    dotMode = !dotMode;
                //pressing L toggles layer view mode
                else if (kbState.IsKeyDown(Keys.L) && previousKbState.IsKeyUp(Keys.L))
                    renderOnlyCurrentLayerMode = !renderOnlyCurrentLayerMode;
                //if the user pressed the f5 key, refresh the instance list
                else if(kbState.IsKeyDown(Keys.F5) && previousKbState.IsKeyUp(Keys.F5))
                    instances = fileManager.LoadInstances(@"..\..\..\..\Content\instances");
            }

            #endregion

            #region PANNING AROUND THE MAP ======================================================================

            //if the user is holding down space (or alt) and they drag the mouse around while clicked,
            //let them pan around the map
            if (kbState.IsKeyDown(Keys.Space) || kbState.IsKeyDown(Keys.LeftAlt))
            {
                //set the cursor image to be that 4 point arrow scrolly thing
                Mouse.SetCursor(MouseCursor.SizeAll);

                //if the user starts to click and drag the mouse...
                if (mouseState.LeftButton == ButtonState.Pressed &&
                    previousMouseState.LeftButton == ButtonState.Released)
                {
                    //store where they started clicking and dragging
                    mouseDragStart = mouseState.Position;

                    //and when the camera was at this time
                    originalCameraOffset = new Vector2(cameraOffset.X, cameraOffset.Y);
                }

                //while the user is still dragging...
                else if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    //store where the mouse is now
                    mouseDragEnd = mouseState.Position;

                    //pan the camera around the map based on the original camera position, 
                    //and where the mouse dragged to
                    cameraOffset.X = originalCameraOffset.X + mouseDragStart.X - mouseDragEnd.X;
                    cameraOffset.Y = originalCameraOffset.Y + mouseDragStart.Y - mouseDragEnd.Y;
                }
            }

            #endregion


            #region OPERATING THE TOOLS IF THE MOUSE IS IN THE WINDOW ======================================================================

            //if space is NOT being held down, the mouse is in the screen, and the editor window is active
            //then let the user click on the screen and use the tool they have
            else if (MouseIsInScreen() && IsActive)
            {
                hoveredObject = null;
                hoveredRep = null;
                Mouse.SetCursor(MouseCursor.Arrow);

                switch (currentTool)
                {

                    #region SELECTION TOOL =============================================================================

                    case SelectedTool.Select:

                        #region CONTROLLING OBJECT VIEW SPECIFIC THINGS (LIKE MOVING AROUND OBJECTS) ===================
                        if (!collisionMode)
                        {
                            //if the mouse is hovering over an object, then show the "clicky hand sprite"
                            foreach (MapObjectRepresentative rep in placedMapObjects)
                            {
                                MapObject possiblehoveredObject = ParseMapObjectRep(rep.ID);
                                Rectangle objectTransform = new Rectangle(rep.Position, possiblehoveredObject.Size);
                                //first check if we actually clicked on it
                                if (objectTransform.Contains(currentMouseSelection))
                                {
                                    //then, if we are not in render current layer only mode, then just select the object!
                                    //but if we are in render current layer only mode, then only count the object if it is being rendered
                                    if (!renderOnlyCurrentLayerMode || ParseMapObjectRep(rep.ID).Layer == currentLayer || ParseMapObjectRep(rep.ID).Layer == 9)
                                    {
                                        if(selectionBoundaries == null)
                                            Mouse.SetCursor(MouseCursor.Hand);
                                        hoveredObject = possiblehoveredObject;
                                        hoveredRep = rep;
                                    }
                                }
                            }

                            //if the mouse is not hovering over any object then switch back to the default arrow
                            if(hoveredRep == null)
                                Mouse.SetCursor(MouseCursor.Arrow);

                            //if user holds CONTROL and LEFT CLICKS
                            if (kbState.IsKeyDown(Keys.LeftControl) && mouseState.LeftButton == ButtonState.Pressed &&
                            previousMouseState.LeftButton == ButtonState.Released)
                            {
                                //creates a new list of selected objects if it doesn't alread yexist
                                if (selectedMapObjects == null)
                                {
                                    selectedMapObjects = new List<MapObjectRepresentative>();
                                }

                                MapObjectRepresentative selectedObject = null;

                                //get the object at the mouse cursor
                                foreach (MapObjectRepresentative rep in placedMapObjects)
                                {
                                    Rectangle objectTransform = new Rectangle(rep.Position, ParseMapObjectRep(rep.ID).Size);
                                    //first check if we actually clicked on it
                                    if (objectTransform.Contains(currentMouseSelection))
                                    {
                                        //then, if we are not in render current layer only mode, then just select the object!
                                        //but if we are in render current layer only mode, then only select the object if it is being rendered
                                        if (!renderOnlyCurrentLayerMode || ParseMapObjectRep(rep.ID).Layer == currentLayer || ParseMapObjectRep(rep.ID).Layer == 9)
                                        {
                                            selectedObject = rep;
                                        }
                                    }
                                }

                                //if an object was found
                                if (selectedObject != null)
                                {
                                    //REMOVES the object from the list if it is in there
                                    if (selectedMapObjects.Contains(selectedObject))
                                    {
                                        selectedMapObjects.Remove(selectedObject);
                                    }
                                    //ADDS the object if its not already in the list
                                    else
                                    {
                                        selectedMapObjects.Add(selectedObject);
                                    }
                                }
                            }
                            //user does NOT HOLD CONTROL, and LEFT CLICKS
                            else if (kbState.IsKeyUp(Keys.LeftControl) && mouseState.LeftButton == ButtonState.Pressed &&
                                previousMouseState.LeftButton == ButtonState.Released)
                            {
                                //check if the mouse is hovering over an object:
                                //(A)  if not, deselect everything,
                                //(B)  if it is but its not selected, deselect everything and then select that one object
                                //(C)  if it is and it is selected, go into "move mode" for moving objects around the map


                                mouseDragStart = currentMouseSelection;
                                movingAroundSelectedObjects = false;
                                if (selectedMapObjects != null)
                                {
                                    foreach (MapObjectRepresentative rep in selectedMapObjects)
                                    {
                                        Rectangle objectTransform = new Rectangle(rep.Position, ParseMapObjectRep(rep.ID).Size);
                                        if (objectTransform.Contains(currentMouseSelection))
                                        {
                                            //(C)   if you start clicking while mouse is inside a selected object, treat it like moving the object around
                                            movingAroundSelectedObjects = true;
                                            break;
                                        }
                                    }
                                }
                            }


                            //Letting go of the left click without control held down
                            else if (kbState.IsKeyUp(Keys.LeftControl) && mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
                            {
                                if (currentMouseSelection == mouseDragStart)
                                {
                                    mouseDragStart = currentMouseSelection;
                                    //(A) (B)  Deselect everything
                                    selectedMapObjects = new List<MapObjectRepresentative>();

                                    MapObjectRepresentative selectedObject = null;

                                    //(B)  selects the object the mouse is hovering over
                                    foreach (MapObjectRepresentative rep in placedMapObjects)
                                    {
                                        Rectangle objectTransform = new Rectangle(rep.Position, ParseMapObjectRep(rep.ID).Size);
                                        if (objectTransform.Contains(currentMouseSelection))
                                        {
                                            //then, if we are not in render current layer only mode, then just select the object!
                                            //but if we are in render current layer only mode, then only select the object if it is being rendered
                                            if (!renderOnlyCurrentLayerMode || ParseMapObjectRep(rep.ID).Layer == currentLayer || ParseMapObjectRep(rep.ID).Layer == 9)
                                            {
                                                selectedObject = rep;
                                            }
                                        }
                                    }

                                    //(B) if an object was found, add it to the selected objects list!
                                    if (selectedObject != null)
                                        selectedMapObjects.Add(selectedObject);
                                    else
                                    {
                                        selectedMapObjects = null;
                                    }
                                }
                            }

                            //still clicking without control held down
                            //(C)  move objects around the screen
                            else if (kbState.IsKeyUp(Keys.LeftControl) && mouseState.LeftButton == ButtonState.Pressed)
                            {
                                if(movingAroundSelectedObjects)
                                    MoveMapSelectedObjects(currentMouseSelection - previousMouseSelection);
                            }
                        }

                        #endregion

                        #region BOTH COLLISION AND OBJECT VIEW SELECTION CONTROLS =============================================
                        //this initial drag selection of the selection boundary box
                        if (!movingAroundSelectedObjects && !initialSelectionMade)
                        {
                            //when the user starts clicking, store that initial position
                            if (mouseState.LeftButton == ButtonState.Pressed &&
                                previousMouseState.LeftButton == ButtonState.Released)
                            {
                                mouseDragStart = currentMouseSelection;
                            }
                            //while they are still clicking
                            else if (mouseState.LeftButton == ButtonState.Pressed)
                            {
                                //if no selection box exists and the user drags the mouse to another spot, create a new selection box
                                if (selectionBoundaries == null && currentMouseSelection != mouseDragStart)
                                {
                                    selectionBoundaries = new Boundary(mouseDragStart.X, mouseDragStart.Y,
                                        currentMouseSelection.X - mouseDragStart.X, currentMouseSelection.Y - mouseDragStart.Y,
                                        maxMapWidth, maxMapHeight);
                                }

                                //if a selection box already exists, treat that mouse drag as adjusting the size and position of the box
                                else if (selectionBoundaries != null)
                                {
                                    selectionBoundaries.AdjustBoundaries(currentMouseSelection, previousMouseSelection, Handle.BottomRight);
                                }
                            }

                            //if the user releases the click, the initial selection has been made
                            else if (mouseState.LeftButton == ButtonState.Released &&
                                previousMouseState.LeftButton == ButtonState.Pressed)
                            {
                                initialSelectionMade = true;
                            }

                            //if the user releases the click and they never moved their mouse around, deselect everything
                            if (mouseState.LeftButton == ButtonState.Released &&
                                previousMouseState.LeftButton == ButtonState.Pressed && currentMouseSelection == mouseDragStart)
                            {
                                initialSelectionMade = false;
                                selectionBoundaries = null;
                            }
                        }

                        //if the initial selection has already been done and a selection box exists
                        //allow the user to adjust the size and position of it
                        else if(selectionBoundaries != null)
                        {
                            //find which handle of the selection box the mouse is hovering over
                            Handle hoveredHandleSelection = selectionBoundaries.CheckSelectedHandle(mouseState.Position, handleSize * (GraphicsDevice.Viewport.Width / (float)PrefferedWidth), blockWidthOnScreen, blockHeightOnScreen, toolBarHeight, cameraOffsetOnScreen);

                            //if the user has started clicking and dragging, and a handle is being hovered over
                            if (mouseState.LeftButton == ButtonState.Pressed &&
                                previousMouseState.LeftButton == ButtonState.Released && hoveredHandleSelection != Handle.None)
                            {
                                //set their selected handle to the one being hovered over
                                selectedHandle = hoveredHandleSelection;
                                //store where they started clicking and dragging
                                mouseDragStart = currentMouseSelection;
                            }

                            //if the user clicks and was not selecting a handle, deselct everything
                            if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released &&
                                hoveredHandleSelection == Handle.None)
                            {
                                mouseDragStart = currentMouseSelection;
                                initialSelectionMade = false;
                                selectionBoundaries = null;
                            }

                            //if the user is holding their mouse down and a handle is being selected
                            else if (mouseState.LeftButton == ButtonState.Pressed)
                            {
                                mouseDragEnd = currentMouseSelection;
                                //adjust the sides of the boundaries depending on what handle they are holding on to
                                selectionBoundaries.AdjustBoundaries(currentMouseSelection, previousMouseSelection, selectedHandle);
                            }

                            //when the user releases their click, reset the handle and mouse cursor sprite
                            else if (previousMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
                            {
                                Mouse.SetCursor(MouseCursor.Arrow);
                                selectedHandle = Handle.None;
                            }


                            #region HOTKEYS WHILE SELECTION HAS BEEN MADE =====================================================

                            //while in collision view mode, make keys affect the grid tiles
                            if (collisionMode)
                            {
                                //if the user pressed DELETE
                                if (kbState.IsKeyDown(Keys.Delete) && previousKbState.IsKeyUp(Keys.Delete))
                                {
                                    for (int x = selectionBoundaries.Left; x < selectionBoundaries.Right; x++)
                                    {
                                        for (int y = selectionBoundaries.Top; y < selectionBoundaries.Bottom; y++)
                                        {
                                            //delete tiles inside selection (set them to black)
                                            mapTiles[x, y] = Color.Black;
                                        }
                                    }
                                    MakeChangeToMap();

                                    //and reset selection
                                    selectionBoundaries = null;
                                    mouseDragStart = currentMouseSelection;
                                    initialSelectionMade = false;
                                }

                                //if the user pressed F...
                                else if (kbState.IsKeyDown(Keys.F) && previousKbState.IsKeyUp(Keys.F))
                                {
                                    for (int x = selectionBoundaries.Left; x < selectionBoundaries.Right; x++)
                                    {
                                        for (int y = selectionBoundaries.Top; y < selectionBoundaries.Bottom; y++)
                                        {
                                            //...fill the tiles in the selection with the chosen color
                                            mapTiles[x, y] = colorPalette[selectedColorIndex];
                                        }
                                    }
                                    MakeChangeToMap();
                                }
                            }

                            //if in object view mode
                            else
                            {
                                //if the user pressed ENTER...
                                if (kbState.IsKeyDown(Keys.Enter) && previousKbState.IsKeyUp(Keys.Enter))
                                {
                                    //and if the user is pressing control
                                    if (kbState.IsKeyDown(Keys.LeftControl))
                                    {
                                        //dont make a new selection list if it already exists
                                        if(selectedMapObjects == null)
                                            selectedMapObjects = new List<MapObjectRepresentative>();
                                    }

                                    //if the user isnt holding control and just presses enter by itself
                                    else
                                    {
                                        //make a new selection list
                                        selectedMapObjects = new List<MapObjectRepresentative>();
                                    }

                                    //add all objects within the selection to the SelectedMapObjects list
                                    foreach (MapObjectRepresentative rep in placedMapObjects)
                                    {
                                        Rectangle objectTransform = new Rectangle(rep.Position, ParseMapObjectRep(rep.ID).Size);
                                        if (selectionBoundaries.ToRectangle().Intersects(objectTransform))
                                        {
                                            //then, if we are not in render current layer only mode, then just select the object!
                                            //but if we are in render current layer only mode, then only select the object if it is being rendered
                                            if (rep != null && !renderOnlyCurrentLayerMode || ParseMapObjectRep(rep.ID).Layer == currentLayer || ParseMapObjectRep(rep.ID).Layer == 9)
                                            {
                                                //ADDS the object if its not already in the list
                                                if (!selectedMapObjects.Contains(rep))
                                                {
                                                    selectedMapObjects.Add(rep);
                                                }
                                            }
                                        }
                                    }


                                    //remove the selection box, as we are just left with the objects that are selected
                                    selectionBoundaries = null;
                                    initialSelectionMade = false;
                                }
                            }
                            

                            //Pressing the ARROW KEYS will nudge the selection box in that direction
                            if (kbState.IsKeyDown(Keys.Left) && previousKbState.IsKeyUp(Keys.Left))
                            {
                                selectionBoundaries.MoveEntireBoundary(new Point(-nudgeAmount, 0));
                            }
                            if (kbState.IsKeyDown(Keys.Right) && previousKbState.IsKeyUp(Keys.Right))
                            {
                                selectionBoundaries.MoveEntireBoundary(new Point(nudgeAmount, 0));
                            }
                            if (kbState.IsKeyDown(Keys.Up) && previousKbState.IsKeyUp(Keys.Up))
                            {
                                selectionBoundaries.MoveEntireBoundary(new Point(0, -nudgeAmount));
                            }
                            if (kbState.IsKeyDown(Keys.Down) && previousKbState.IsKeyUp(Keys.Down))
                            {
                                selectionBoundaries.MoveEntireBoundary(new Point(0, nudgeAmount));
                            }

                        }

                        #endregion

                        #region HOTKEYS WHILE THE SELECTION BOX HAS EITHER BEEN MADE OR NOT ===========================
                        //IF user is in OBJECT MODE
                        if (!collisionMode)
                        {
                            //IF OBJECTS ARE SELECTED
                            if (selectedMapObjects != null)
                            {
                                //pressing DELETE removes them from the map
                                if (kbState.IsKeyDown(Keys.Delete) && previousKbState.IsKeyUp(Keys.Delete))
                                {
                                    selectionBoundaries = null;
                                    initialSelectionMade = false;
                                    //remove all selected objects from the map
                                    foreach (MapObjectRepresentative rep in selectedMapObjects)
                                    {
                                        placedMapObjects.Remove(rep);
                                    }
                                    MakeChangeToMap();
                                    //and also remove the selection
                                    selectedMapObjects = null;
                                }

                                //pressing the ARROW KEYS will nudge the selected objects in that direction 
                                //if no selection box exists (because then that would be controlled by the arrow keys
                                if (selectionBoundaries == null)
                                {
                                    if (kbState.IsKeyDown(Keys.Left) && previousKbState.IsKeyUp(Keys.Left))
                                    {
                                        MoveMapSelectedObjects(new Point(-nudgeAmount, 0));
                                    }
                                    if (kbState.IsKeyDown(Keys.Right) && previousKbState.IsKeyUp(Keys.Right))
                                    {
                                        MoveMapSelectedObjects(new Point(nudgeAmount, 0));
                                    }
                                    if (kbState.IsKeyDown(Keys.Up) && previousKbState.IsKeyUp(Keys.Up))
                                    {
                                        MoveMapSelectedObjects(new Point(0, -nudgeAmount));
                                    }
                                    if (kbState.IsKeyDown(Keys.Down) && previousKbState.IsKeyUp(Keys.Down))
                                    {
                                        MoveMapSelectedObjects(new Point(0, nudgeAmount));
                                    }
                                }
                            }


                        }
                        #endregion

                        #region HOTKEYS WHILE THE SELECTION BOX OR SELECTED OBJECTS EXISTS OR NOT =======================
                        //pressing escape resets the selection
                        if (kbState.IsKeyDown(Keys.Escape) && previousKbState.IsKeyUp(Keys.Escape))
                        {
                            mouseDragStart = currentMouseSelection;
                            //if a selection box exists, reset that first
                            if(initialSelectionMade || selectionBoundaries != null)
                            {
                                initialSelectionMade = false;
                                selectionBoundaries = null;
                            }
                            //then reset the selected objects
                            else
                            {
                                selectedMapObjects = null;
                            }
                        }

                        //pressing D or A by themselves cycles through the color palette
                        if (kbState.IsKeyDown(Keys.D) && previousKbState.IsKeyUp(Keys.D))
                        {
                            //cycle forwards on the color palette
                            if (selectedColorIndex == colorPalette.Length - 1)
                                selectedColorIndex = 0;
                            else
                                selectedColorIndex++;
                        }
                        else if (kbState.IsKeyUp(Keys.LeftControl) && kbState.IsKeyDown(Keys.A) && previousKbState.IsKeyUp(Keys.A))
                        {
                            //cycle backwards on the color palette
                            if (selectedColorIndex == 0)
                                selectedColorIndex = colorPalette.Length - 1;
                            else
                                selectedColorIndex--;
                        }

                        //pressing CTRL + A selects the entire map!
                        else if (kbState.IsKeyDown(Keys.LeftControl) && kbState.IsKeyDown(Keys.A) && previousKbState.IsKeyUp(Keys.A))
                        {
                            selectionBoundaries = new Boundary(0, 0, maxMapWidth, maxMapHeight, maxMapWidth, maxMapHeight);
                            initialSelectionMade = true;
                        }

                        #endregion
                        break;

                    #endregion

                    #endregion


                    #region BRUSH TOOL =============================================================================
                    case SelectedTool.Brush:
                        Mouse.SetCursor(MouseCursor.Crosshair);

                        //If the user is selecting a tile thats inside the map
                        if (currentMouseSelection.X >= 0 && currentMouseSelection.X < maxMapWidth &&
                            currentMouseSelection.Y >= 0 && currentMouseSelection.Y < maxMapHeight)
                        {
                            //and they start left clicking
                            if(mouseState.LeftButton == ButtonState.Pressed)
                            {
                                for(int x = currentMouseSelection.X - (brushSize/2); x <= currentMouseSelection.X + (brushSize / 2); x++)
                                {
                                    for (int y = currentMouseSelection.Y - (brushSize / 2); y <= currentMouseSelection.Y + (brushSize / 2); y++)
                                    {
                                        if (paintMode)
                                        {
                                            //paint those tiles within the brush size the chosen color
                                            if (x >= 0 && x < maxMapWidth && y >= 0 && y < maxMapHeight)
                                                mapTiles[x, y] = colorPalette[selectedColorIndex];
                                        }
                                        else
                                        {
                                            //if eraser mode is on, delete those tiles (switch them to black)
                                            if (x >= 0 && x < maxMapWidth && y >= 0 && y < maxMapHeight)
                                                mapTiles[x, y] = Color.Black;
                                        }

                                    }
                                }
                                MakeChangeToMap();
                            }
                        }

                        #region BRUSH TOOL HOTKEYS ========================

                        //pressing ] (CLOSED BRACKET) increases brush size
                        if (kbState.IsKeyDown(Keys.OemCloseBrackets) && previousKbState.IsKeyUp(Keys.OemCloseBrackets) && brushSize < 255)
                        {
                            brushSize += 2;
                        }

                        //pressing [ (OPEN BRACKET) decreases brush size
                        else if (kbState.IsKeyDown(Keys.OemOpenBrackets) && previousKbState.IsKeyUp(Keys.OemOpenBrackets) && brushSize > 1)
                        {
                            brushSize -= 2;
                        }

                        //pressing E switches between paint and eraser mode
                        else if (kbState.IsKeyDown(Keys.E) && previousKbState.IsKeyUp(Keys.E))
                        {
                            paintMode = !paintMode;
                        }

                        //pressing D or A cycles through the color palette
                        else if (kbState.IsKeyDown(Keys.D) && previousKbState.IsKeyUp(Keys.D))
                        {
                            //cycle forwards on the color palette
                            if (selectedColorIndex == colorPalette.Length - 1)
                                selectedColorIndex = 0;
                            else
                                selectedColorIndex++;
                        }
                        else if (kbState.IsKeyDown(Keys.A) && previousKbState.IsKeyUp(Keys.A))
                        {
                            //cycle backwards on the color palette
                            if (selectedColorIndex == 0)
                                selectedColorIndex = colorPalette.Length - 1;
                            else
                                selectedColorIndex--;
                        }


                        //pressing escape resets the selection
                        if (kbState.IsKeyDown(Keys.Escape) && previousKbState.IsKeyUp(Keys.Escape))
                        {
                            //if a selection box exists, reset that first
                            if (initialSelectionMade || selectionBoundaries != null)
                            {
                                initialSelectionMade = false;
                                selectionBoundaries = null;
                            }
                            //then reset the selected objects
                            else
                            {
                                selectedMapObjects = null;
                            }
                        }
                        #endregion

                        break;

                    #endregion


                    #region CORDON TOOL ========================================================================
                    case SelectedTool.Cordon:
                        Handle hoveredHandleCordon = mapBoundaries.CheckSelectedHandle(mouseState.Position, handleSize, blockWidthOnScreen, blockHeightOnScreen, toolBarHeight, cameraOffsetOnScreen);
                        //if the user has started clicking and dragging, and a handle is being hovered over
                        if (mouseState.LeftButton == ButtonState.Pressed &&
                            previousMouseState.LeftButton == ButtonState.Released && hoveredHandleCordon != Handle.None)
                        {
                            //set their selected handle to the one being hovered over
                            selectedHandle = hoveredHandleCordon;
                            //store where they started clicking and dragging
                            mouseDragStart = currentMouseSelection;
                        }

                        //while the user is dragging their mouse around
                        if (mouseState.LeftButton == ButtonState.Pressed)
                        {
                            mouseDragEnd = currentMouseSelection;
                            //adjust the sides of the boundaries depending on what handle they are holding on to
                            mapBoundaries.AdjustBoundaries(currentMouseSelection, previousMouseSelection, selectedHandle);
                            MakeChangeToMap();
                        }

                        //when the user releases their click
                        else if(previousMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
                        {
                            //reset which handle they are selecting and the cursor sprite
                            Mouse.SetCursor(MouseCursor.Arrow);
                            selectedHandle = Handle.None;
                        }

                        //Pressing the ARROW KEYS will nudge the cordon in that direction
                        if (kbState.IsKeyDown(Keys.Left) && previousKbState.IsKeyUp(Keys.Left))
                        {
                            mapBoundaries.MoveEntireBoundary(new Point(-nudgeAmount, 0));
                        }
                        if (kbState.IsKeyDown(Keys.Right) && previousKbState.IsKeyUp(Keys.Right))
                        {
                            mapBoundaries.MoveEntireBoundary(new Point(nudgeAmount, 0));
                        }
                        if (kbState.IsKeyDown(Keys.Up) && previousKbState.IsKeyUp(Keys.Up))
                        {
                            mapBoundaries.MoveEntireBoundary(new Point(0, -nudgeAmount));
                        }
                        if (kbState.IsKeyDown(Keys.Down) && previousKbState.IsKeyUp(Keys.Down))
                        {
                            mapBoundaries.MoveEntireBoundary(new Point(0, nudgeAmount));
                        }

                        //pressing escape returns you back to the selection tool
                        if (kbState.IsKeyDown(Keys.Escape) && previousKbState.IsKeyUp(Keys.Escape))
                            SetCurrentToolToSelect();

                        break;

                    #endregion

                    #region OBJECT TOOL ===========================================================================
                    case SelectedTool.Object:

                        //Placing map objects in the map!
                        if (currentMouseSelection.X >= 0 && currentMouseSelection.X < maxMapWidth &&
                            currentMouseSelection.Y >= 0 && currentMouseSelection.Y < maxMapHeight)
                        {
                            if (mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
                            {
                                //place more objects!
                                PlaceMapObjectRepresentative();
                            }


                        }


                        #region HOTKEYS FOR CHANGING THE CHOSEN OBJECT AND OTHER THINGS ============================
                        //we still allow the user to switch between Object modes
                        if (kbState.IsKeyDown(Keys.Q) && previousKbState.IsKeyUp(Keys.Q))
                        {
                            currentObjectMode = ObjectMode.Instance;
                        }
                        else if (kbState.IsKeyDown(Keys.W) && previousKbState.IsKeyUp(Keys.W))
                        {
                            currentObjectMode = ObjectMode.Entity;
                        }
                        else if (kbState.IsKeyDown(Keys.E) && previousKbState.IsKeyUp(Keys.E))
                        {
                            currentObjectMode = ObjectMode.Prop;
                        }
                        //cycling backward on the current id
                        else if (kbState.IsKeyDown(Keys.A) && previousKbState.IsKeyUp(Keys.A))
                        {
                            //if the user is in instance mode, only cycle the selected instance id
                            if (currentObjectMode == ObjectMode.Instance)
                            {
                                if (currentInstanceID == 0)
                                    currentInstanceID = instances.Count-1;
                                else
                                    currentInstanceID--;
                            }
                            //if the user is in entity mode, only cycle the selected entity id
                            else if (currentObjectMode == ObjectMode.Entity)
                            {
                                if (currentEntityID == 0)
                                    currentEntityID = entities.Count-1;
                                else
                                    currentEntityID--;
                            }
                            //if the user is in prop mode, only cycle the selected prop id
                            else if (currentObjectMode == ObjectMode.Prop)
                            {
                                if (currentPropID == 0)
                                    currentPropID = props.Count-1;
                                else
                                    currentPropID--;
                            }
                        }

                        //cycling forward on the current id
                        else if (kbState.IsKeyDown(Keys.D) && previousKbState.IsKeyUp(Keys.D))
                        {
                            //if the user is in instance mode, only cycle the selected instance id
                            if (currentObjectMode == ObjectMode.Instance)
                            {
                                if (currentInstanceID == instances.Count - 1)
                                    currentInstanceID = 0;
                                else
                                    currentInstanceID++;
                            }
                            //if the user is in entity mode, only cycle the selected entity id
                            else if (currentObjectMode == ObjectMode.Entity)
                            {
                                if (currentEntityID == entities.Count - 1)
                                    currentEntityID = 0;
                                else
                                    currentEntityID++;
                            }
                            //if the user is in prop mode, only cycle the selected prop id
                            else if (currentObjectMode == ObjectMode.Prop)
                            {
                                if (currentPropID == props.Count - 1)
                                    currentPropID = 0;
                                else
                                    currentPropID++;
                            }
                        }

                        //Allowing some control over selected objects while in object mode
                        if (selectedMapObjects != null)
                        {
                            //pressing DELETE removes them from the map
                            if (kbState.IsKeyDown(Keys.Delete) && previousKbState.IsKeyUp(Keys.Delete))
                            {
                                selectionBoundaries = null;
                                initialSelectionMade = false;

                                //remove all selected objects from the map
                                foreach (MapObjectRepresentative rep in selectedMapObjects)
                                {
                                    placedMapObjects.Remove(rep);
                                }
                                MakeChangeToMap();
                                //and remove the selection
                                selectedMapObjects = null;
                            }
                            //pressing the ARROW KEYS will nudge the selected objects in that direction
                            if (kbState.IsKeyDown(Keys.Left) && previousKbState.IsKeyUp(Keys.Left))
                            {
                                MoveMapSelectedObjects(new Point(-nudgeAmount, 0));
                            }
                            if (kbState.IsKeyDown(Keys.Right) && previousKbState.IsKeyUp(Keys.Right))
                            {
                                MoveMapSelectedObjects(new Point(nudgeAmount, 0));
                            }
                            if (kbState.IsKeyDown(Keys.Up) && previousKbState.IsKeyUp(Keys.Up))
                            {
                                MoveMapSelectedObjects(new Point(0, -nudgeAmount));
                            }
                            if (kbState.IsKeyDown(Keys.Down) && previousKbState.IsKeyUp(Keys.Down))
                            {
                                MoveMapSelectedObjects(new Point(0, nudgeAmount));
                            }
                        }

                        //pressing escape resets the selection
                        if (kbState.IsKeyDown(Keys.Escape) && previousKbState.IsKeyUp(Keys.Escape))
                        {
                            mouseDragStart = currentMouseSelection;
                            //if a selection box exists, reset that first
                            if (initialSelectionMade || selectionBoundaries != null)
                            {
                                initialSelectionMade = false;
                                selectionBoundaries = null;
                            }
                            //then reset the selected objects
                            else
                            {
                                selectedMapObjects = null;
                            }
                        }
                        #endregion

                        break;
                    #endregion
                }
            }

            #endregion

            //if the user lets go of the Space bar, reset the image of the cursor back to the default arrow
            if (previousKbState.IsKeyDown(Keys.Space) && kbState.IsKeyUp(Keys.Space) || previousKbState.IsKeyDown(Keys.LeftAlt) && kbState.IsKeyUp(Keys.LeftAlt))
            {
                Mouse.SetCursor(MouseCursor.Arrow);
            }


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //set it so we are drawing sprites onto a determined render tagret size 1600x900
            GraphicsDevice.SetRenderTarget(mapViewPort);

            //if in collision view, background is black
            if (collisionMode)
            {
                GraphicsDevice.Clear(Color.Black);
            }

            //if in object view, background is dark blue/gray
            else
            {
                GraphicsDevice.Clear(new Color(30, 30, 40));
            }


            //draw our graphics here!
            spriteBatch.Begin();

            List<MapObjectRepresentative> objectsToRender = new List<MapObjectRepresentative>();
            foreach(MapObjectRepresentative rep in placedMapObjects)
            {
                MapObject mapObject = ParseMapObjectRep(rep.ID);
                if (rep.ID.Substring(0, 1) == "p" || rep.ID.Substring(0, 1) == "e")
                {
                    objectsToRender.Add(rep);
                }
                else if (rep.ID.Substring(0, 1) == "i")
                {
                    mapObject.AddObjectsToRenderPipeline(objectsToRender, rep.Position);
                }
            }

            #region DRAWING THE GRID AND TILES AND OBJECTS =====================================================================
            if (collisionMode)
            {
                //if we are only rendering the current layer...
                if (renderOnlyCurrentLayerMode)
                {
                    //then go through each prop on the current layer and render it
                    foreach (MapObjectRepresentative rep in objectsToRender)
                    {
                        MapObject mapObjectToRender = ParseMapObjectRep(rep.ID);
                        if (mapObjectToRender.Layer == currentLayer)
                            mapObjectToRender.DrawGraphic(spriteBatch, rep.Position, blockPixelSize, cameraOffset, Color.Gray);
                    }
                    //then go through each entity and render it above props
                    foreach (MapObjectRepresentative rep in objectsToRender)
                    {
                        MapObject mapObjectToRender = ParseMapObjectRep(rep.ID);
                        if (mapObjectToRender.Layer == 9)
                            mapObjectToRender.DrawGraphic(spriteBatch, rep.Position, blockPixelSize, cameraOffset, Color.Gray);
                    }
                }
                //otherwise, render all props in the order they should be in
                else
                {
                    //our props only have layers 0-8 inclusive, but we go to 9 to render all entities above props
                    for (int i = 0; i < 10; i++)
                    {
                        foreach (MapObjectRepresentative rep in objectsToRender)
                        {
                            MapObject mapObjectToRender = ParseMapObjectRep(rep.ID);
                            if(mapObjectToRender.Layer == i)
                            {
                                mapObjectToRender.DrawGraphic(spriteBatch, rep.Position, blockPixelSize, cameraOffset, Color.Gray);
                            }
                        }
                    }
                }

                //draw collision tiles normally
                DrawTiles();
                foreach (MapObjectRepresentative rep in placedMapObjects)
                {
                    ParseMapObjectRep(rep.ID).DrawCollision(spriteBatch, rep.Position, whiteSquare, blockPixelSize, cameraOffset, PrefferedWidth, PrefferedHeight);
                }

            }
            else
            {
                //draw collision tiles
                DrawTiles();
                foreach (MapObjectRepresentative rep in placedMapObjects)
                {
                    ParseMapObjectRep(rep.ID).DrawCollision(spriteBatch, rep.Position, whiteSquare, blockPixelSize, cameraOffset, PrefferedWidth, PrefferedHeight);
                }

                //if we are only rendering the current layer...
                if (renderOnlyCurrentLayerMode)
                {
                    //then go through each prop on the current layer and render it
                    foreach (MapObjectRepresentative rep in objectsToRender)
                    {
                        MapObject mapObjectToRender = ParseMapObjectRep(rep.ID);
                        if (mapObjectToRender.Layer == currentLayer)
                            mapObjectToRender.DrawGraphic(spriteBatch, rep.Position, blockPixelSize, cameraOffset, Color.White);
                    }
                    //then go through each entity and render it above props
                    foreach (MapObjectRepresentative rep in objectsToRender)
                    {
                        MapObject mapObjectToRender = ParseMapObjectRep(rep.ID);
                        if (mapObjectToRender.Layer == 9)
                            mapObjectToRender.DrawGraphic(spriteBatch, rep.Position, blockPixelSize, cameraOffset, Color.White);
                    }
                }
                //otherwise, render all props in the order they should be in
                else
                {
                    //our props only have layers 0-8 inclusive, but we go to 9 to render all entities above props
                    for (int i = 0; i < 10; i++)
                    {
                        foreach (MapObjectRepresentative rep in objectsToRender)
                        {
                            MapObject mapObjectToRender = ParseMapObjectRep(rep.ID);
                            if (mapObjectToRender.Layer == i)
                            {
                                mapObjectToRender.DrawGraphic(spriteBatch, rep.Position, blockPixelSize, cameraOffset, Color.White);
                            }
                        }
                    }
                }
            }


            DrawGrid();
            #endregion

            //if we are in object mode, draw a preview of the object we have selected
            if(currentTool == SelectedTool.Object)
            {
                //get the currently selected map object
                MapObject currentSelectedMapObject = null;
                switch (currentObjectMode)
                {
                    case (ObjectMode.Instance):
                        currentSelectedMapObject = ParseMapObjectRep(string.Format("i{0}", currentInstanceID));
                        break;

                    case (ObjectMode.Entity):
                        currentSelectedMapObject = ParseMapObjectRep(string.Format("e{0}", currentEntityID));
                        break;

                    case (ObjectMode.Prop):
                        currentSelectedMapObject = ParseMapObjectRep(string.Format("p{0}", currentPropID));
                        break;
                }

                //draw its bounding box
                currentSelectedMapObject.DrawBoundary(spriteBatch, currentMouseSelection, whiteSquare, blockPixelSize, cameraOffset, Color.CornflowerBlue);
                
                //draw a preview of the map object
                currentSelectedMapObject.DrawGraphic(spriteBatch, currentMouseSelection, blockPixelSize, cameraOffset, Color.White);
            }
            //other than that, just highlight the block we have selected
            else
            {
                spriteBatch.Draw(whiteSquare, new Rectangle(currentMouseSelection.X * blockPixelSize - (int)cameraOffset.X,
                    currentMouseSelection.Y * blockPixelSize - (int)cameraOffset.Y, blockPixelSize, blockPixelSize), Color.CornflowerBlue);
            }


            #region DRAW BOUNDARIES FOR VARIOUS THINGS IN THE MAP =========================================================

            //draw the cordon (boundaries of the map)
            mapBoundaries.DrawBoundary(spriteBatch, whiteSquare, blockPixelSize, cameraOffset, Color.Red);

            if (hoveredRep != null)
            {
                //draw its bounding box
                hoveredObject.DrawBoundary(spriteBatch, hoveredRep.Position, whiteSquare, blockPixelSize, cameraOffset, Color.CornflowerBlue);
                //then draw its ID
                DrawRepID(hoveredRep);
            }

            //draw boundaries for selected map objects
            if (selectedMapObjects != null)
            {
                foreach (MapObjectRepresentative rep in selectedMapObjects)
                {
                    if(rep == hoveredRep)
                    {
                        ParseMapObjectRep(rep.ID).DrawBoundary(spriteBatch, rep.Position, whiteSquare, blockPixelSize, cameraOffset, Color.Purple);
                    }
                    else
                    {
                        ParseMapObjectRep(rep.ID).DrawBoundary(spriteBatch, rep.Position, whiteSquare, blockPixelSize, cameraOffset, Color.DeepPink);
                    }

                    //draw the id of the selected objects in various size fonts depending on the zoom level
                    DrawRepID(rep);
                }
            }

            //draw the boundaries for selection box
            if (selectionBoundaries != null)
            {
                selectionBoundaries.DrawBoundary(spriteBatch, whiteSquare, blockPixelSize, cameraOffset, Color.Yellow);
            }

            #endregion


            #region DRAWING TOOL SPECIFIC THINGS =====================================================================
            switch (currentTool)
            {
                //DRAWING SELECTION TOOL ==================================================================================================================================
                case SelectedTool.Select:
                    if(selectionBoundaries != null)
                    {
                        //draw the handles of the selection boundaries
                        selectionBoundaries.DrawHandles(spriteBatch, handleTexture, handleSize, blockPixelSize, cameraOffset);
                    }
                    break;

                //DRAWING BRUSH TOOL ==================================================================================================================================

                case SelectedTool.Brush:
                    //draw the preview of the brush size onto the grid
                    for (int x = currentMouseSelection.X - (brushSize / 2); x <= currentMouseSelection.X + (brushSize / 2); x++)
                    {
                        for (int y = currentMouseSelection.Y - (brushSize / 2); y <= currentMouseSelection.Y + (brushSize / 2); y++)
                        {
                            spriteBatch.Draw(whiteSquare, new Rectangle(x * blockPixelSize - (int)cameraOffset.X,
                                        y * blockPixelSize - (int)cameraOffset.Y, blockPixelSize, blockPixelSize), new Color(100, 149, 237, 100));

                        }
                    }
                    break;

                //DRAWING CORDON TOOL ==================================================================================================================================

                case SelectedTool.Cordon:
                    //draw the handles of the map boundaries
                    mapBoundaries.DrawHandles(spriteBatch, handleTexture, handleSize, blockPixelSize, cameraOffset);
                    break;

                //DRAWING OBJECT TOOL ==================================================================================================================================

                case SelectedTool.Object:

                    break;

            }

            #endregion

            #region DRAWING TOOL TIPS =================================================================
            //to display tool tips for the buttons when hovered over them,
            //we first check if the tool tip name is null (if they are, no tool tip to display)
            if (toolTipName != null)
            {
                spriteBatch.Draw(whiteSquare, new Rectangle(0,0,GraphicsDevice.Viewport.Width, 120), Color.Cornsilk);
                spriteBatch.DrawString(arial24, toolTipName, Vector2.Zero, Color.Black);
                spriteBatch.DrawString(arial18, toolTipDescription, new Vector2(0, 50), Color.Black);
            }

            #endregion

            spriteBatch.End();

            //we are now drawing onto the window/actual screen
            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.Clear(Color.LightGray);

            spriteBatch.Begin();




            #region DRAWING THE TOOL BAR ==============================================================================================
            
            //writes some important info to the screen (the camera offset, the zoom level, what block is the mouse selecting, etc)
            debugTextLine = string.Format("Camera Offset:{0}   Zoom:{1}   Mouse at Block:{2}   Mouse Position:{3}   Map Boundaries:{4}",
                cameraOffset.ToPoint().ToString(), largeBlockPixelSize.ToString(), currentMouseSelection.ToString(), 
                mouseState.Position.ToString(), mapBoundaries.ToString());

            if(selectionBoundaries == null)
            {
                debugTextLine = string.Format("Camera Offset:{0}   Zoom:{1}   Mouse at Block:{2}   Mouse Position:{3}   Map Boundaries:{4}",
                    cameraOffset.ToPoint().ToString(), largeBlockPixelSize.ToString(), currentMouseSelection.ToString(),
                    mouseState.Position.ToString(), mapBoundaries.ToString());
            }
            else
            {
                debugTextLine = string.Format("Camera Offset:{0}   Zoom:{1}   Mouse at Block:{2}   Mouse Position:{3}   Selection Boundaries:{4}",
                    cameraOffset.ToPoint().ToString(), largeBlockPixelSize.ToString(), currentMouseSelection.ToString(),
                    mouseState.Position.ToString(), selectionBoundaries.ToString());
            }

            spriteBatch.DrawString(consolas14, collisionMode ? "Collision View" : "Object View", new Vector2(215, 26), Color.Black);
            spriteBatch.DrawString(consolas14, renderOnlyCurrentLayerMode ? "Current Layer" : "All Layers", new Vector2(215, 50), Color.Black);
            //spriteBatch.DrawString(consolas10, renderOnlyCurrentLayerMode ? "Render current \nlayer only: ON" : "Render current \nlayer only: OFF", new Vector2(215, 43), Color.Black);

            //drawing the tool bar
            for (int i = 0; i < toolBarButtons.Length; i++)
            {
                #region DRAWING SELECTION TOOL TOOLBAR STUFF ===================================
                //if the select tool is activated...
                if (i == (int)ButtonID.Select && currentTool == SelectedTool.Select)
                {
                    //...highlight the Select Tool button
                    toolBarButtons[i].Draw(spriteBatch, Color.Tomato);

                    //...display the current color choice
                    spriteBatch.DrawString(arial18, string.Format("Color (A,D): "), new Vector2(830, 30), Color.Black);
                    spriteBatch.Draw(whiteSquare, new Rectangle(975, 20, 50, 50), colorPalette[selectedColorIndex]);
                    spriteBatch.DrawString(arial18, string.Format("Layer (1,2,3): {0}",
                    currentLayer), new Vector2(1250, 30), Color.Black);
                }

                #endregion

                #region BRUSH TOOL TOOLBAR STUFF ===========================================
                //if the brush tool is activated...
                else if (i == (int)ButtonID.Brush && currentTool == SelectedTool.Brush)
                {
                    //...highlight the Brush tool button
                    toolBarButtons[i].Draw(spriteBatch, Color.Tomato);

                    //...display brush mode, size, and the current color choice
                    spriteBatch.DrawString(arial18, string.Format("Mode (E): {0}     Brush Size ([,]): {1}     Color (A,D): ", paintMode ? "Paint" : "Eraser",
                        brushSize), new Vector2(830, 30), Color.Black);
                    spriteBatch.Draw(whiteSquare, new Rectangle(1415, 20, 50, 50), colorPalette[selectedColorIndex]);
                }
                #endregion

                #region CORDON TOOL TOOLBAR STUFF ======================================
                //if the cordon tool is activated...
                else if (i == (int)ButtonID.Cordon && currentTool == SelectedTool.Cordon)
                    //...highlight the cordon tool button
                    toolBarButtons[i].Draw(spriteBatch, Color.Tomato);
                #endregion

                #region OBJECT TOOL TOOLBAR STUFF ==========================================
                //if the object tool is activated...
                else if (i == (int)ButtonID.Object && currentTool == SelectedTool.Object)
                {
                    //...highlight the object tool button
                    toolBarButtons[i].Draw(spriteBatch, Color.Tomato);

                    //Display the current Object mode and the selected ID for it
                    if(currentObjectMode == ObjectMode.Instance)
                    {
                        spriteBatch.DrawString(arial18, string.Format("Mode (Q,W,E): Instance     ID (A,D): {0}",
                            currentInstanceID), new Vector2(830, 30), Color.Black);
                        spriteBatch.DrawString(arial18, string.Format("Layer (1,2,3): {0}",
                            currentLayer), new Vector2(1250, 30), Color.Gray);
                    }
                    else if (currentObjectMode == ObjectMode.Entity)
                    {
                        spriteBatch.DrawString(arial18, string.Format("Mode (Q,W,E): Entity     ID (A,D): {0}",
                            currentEntityID), new Vector2(830, 30), Color.Black);
                        spriteBatch.DrawString(arial18, string.Format("Layer (1,2,3): {0}",
                            currentLayer), new Vector2(1250, 30), Color.Gray);
                    }
                    else if (currentObjectMode == ObjectMode.Prop)
                    {
                        spriteBatch.DrawString(arial18, string.Format("Mode (Q,W,E): Prop     ID (A,D): {0}",
                            currentPropID), new Vector2(830, 30), Color.Black);
                        spriteBatch.DrawString(arial18, string.Format("Layer (1,2,3): {0}",
                            currentLayer), new Vector2(1250, 30), Color.Black);
                    }

                }
                #endregion

                //if the user is hovering over this button with the mouse, draw it in grey
                else if (toolBarButtons[i].Selected)
                    toolBarButtons[i].Draw(spriteBatch, Color.Gray);
                //otherwise, display the button normally
                else
                    toolBarButtons[i].Draw(spriteBatch, Color.White);
            }

            //write that info in top left corner of screen
            spriteBatch.DrawString(consolas14, debugTextLine, Vector2.Zero, Color.Black);

            #endregion



            //draw our final image stretched to the window!
            spriteBatch.Draw(mapViewPort, new Rectangle(0, toolBarHeight, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height - toolBarHeight), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the id of a map object in various size fonts depending on the zoom level
        /// </summary>
        /// <param name="rep">the Map Object Rep that has the id you want to draw</param>
        private void DrawRepID(MapObjectRepresentative rep)
        {
            //we first draw a black version, and then a white version on top to give it a black outline
            if (largeBlockPixelSize < 512 && largeBlockPixelSize > 64)
            {
                spriteBatch.DrawString(arial10, rep.ID, rep.Position.ToVector2() * blockPixelSize - cameraOffset + new Vector2(2, 2), Color.Black);
                spriteBatch.DrawString(arial10, rep.ID, rep.Position.ToVector2() * blockPixelSize - cameraOffset, Color.White);
            }
            else if (largeBlockPixelSize < 1024 && largeBlockPixelSize >= 512)
            {
                spriteBatch.DrawString(arial18, rep.ID, rep.Position.ToVector2() * blockPixelSize - cameraOffset + new Vector2(2, 2), Color.Black);
                spriteBatch.DrawString(arial18, rep.ID, rep.Position.ToVector2() * blockPixelSize - cameraOffset, Color.White);
            }
            else if (largeBlockPixelSize >= 1024)
            {
                spriteBatch.DrawString(arial24, rep.ID, rep.Position.ToVector2() * blockPixelSize - cameraOffset + new Vector2(2, 2), Color.Black);
                spriteBatch.DrawString(arial24, rep.ID, rep.Position.ToVector2() * blockPixelSize - cameraOffset, Color.White);
            }
        }

        /// <summary>
        /// Simply draws the map's collision tiles to the screen
        /// </summary>
        private void DrawTiles()
        {
            if (blockPixelSize >= 4)
            {
                for (int x = 0; x < maxMapWidth; x++)
                {
                    for (int y = 0; y < maxMapHeight; y++)
                    {
                        //if the block is within the camera's view
                        if (x * blockPixelSize - (int)cameraOffset.X >= -blockPixelSize && x * blockPixelSize - (int)cameraOffset.X <= PrefferedWidth
                            && y * blockPixelSize - (int)cameraOffset.Y >= -blockPixelSize && y * blockPixelSize - (int)cameraOffset.Y <= PrefferedHeight)
                        {
                            //draw the tile in its color if its not black (treat black as transparent, nothing there)
                            if (mapTiles[x, y] != Color.Black)
                            {
                                spriteBatch.Draw(whiteSquare, new Rectangle(x * blockPixelSize - (int)cameraOffset.X,
                                    y * blockPixelSize - (int)cameraOffset.Y, blockPixelSize, blockPixelSize), mapTiles[x, y]);
                            }
                        }
                    }
                }
            }
            else
            {
                //if the grid is really zoomed out, display a cheaper preview of the grid tiles,
                //this cheaper preview skips over every other tile
                for (int x = 0; x < maxMapWidth; x += 2)
                {
                    for (int y = 0; y < maxMapHeight; y += 2)
                    {
                        if (x * blockPixelSize - (int)cameraOffset.X >= -blockPixelSize && x * blockPixelSize - (int)cameraOffset.X <= PrefferedWidth
                            && y * blockPixelSize - (int)cameraOffset.Y >= -blockPixelSize && y * blockPixelSize - (int)cameraOffset.Y <= PrefferedHeight)
                        {
                            //draw the tile in its color if its not black (treat black as transparent, nothing there)
                            if (mapTiles[x, y] != Color.Black)
                                spriteBatch.Draw(whiteSquare, new Rectangle(x * blockPixelSize - (int)cameraOffset.X,
                                    y * blockPixelSize - (int)cameraOffset.Y, 2, 2), mapTiles[x, y]);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Draws the grid of the editor as either dots on each tile's corners, or lines on edge tiles edges.
        /// Also draws larger orange, line-based grids for when zoomed out
        /// </summary>
        private void DrawGrid()
        {
            //if the pixel size of the small grid tiles is greater than 4, then draw them
            //if they are smaller than that, then the user is zoomed out so far that we should only render some of them
            if (blockPixelSize > 4)
            {
                //if the user is in "dot mode", then draw the grid as dots
                if (dotMode)
                {
                    for (int x = 0; x < maxMapWidth; x++)
                    {
                        for (int y = 0; y < maxMapHeight; y++)
                        {
                            if (x * blockPixelSize - (int)cameraOffset.X >= -blockPixelSize && x * blockPixelSize - (int)cameraOffset.X <= PrefferedWidth
                                && y * blockPixelSize - (int)cameraOffset.Y >= -blockPixelSize && y * blockPixelSize - (int)cameraOffset.Y <= PrefferedHeight)
                            {
                                //draw the dot for this tile
                                spriteBatch.Draw(whiteSquare, new Rectangle(x * blockPixelSize - (int)cameraOffset.X,
                                    y * blockPixelSize - (int)cameraOffset.Y, 1, 1), Color.White);
                            }
                        }
                    }
                }

                //if the user is NOT in "dot mode", then draw the grid as lines
                else
                {
                    for (int x = 0; x < maxMapWidth; x++)
                    {
                        for (int y = 0; y < maxMapHeight; y++)
                        {
                            if (x * blockPixelSize - (int)cameraOffset.X >= -blockPixelSize && x * blockPixelSize - (int)cameraOffset.X <= PrefferedWidth
                                && y * blockPixelSize - (int)cameraOffset.Y >= -blockPixelSize && y * blockPixelSize - (int)cameraOffset.Y <= PrefferedHeight)
                            {
                                //draw line horizontally for this tile's edge
                                spriteBatch.Draw(whiteSquare, new Rectangle(x * blockPixelSize - (int)cameraOffset.X,
                                    y * blockPixelSize - (int)cameraOffset.Y, blockPixelSize, 1), Color.White);
                                //draw line vertically for this tile's edge
                                spriteBatch.Draw(whiteSquare, new Rectangle(x * blockPixelSize - (int)cameraOffset.X,
                                    y * blockPixelSize - (int)cameraOffset.Y, 1, blockPixelSize), Color.White);
                            }
                        }
                    }
                }
            }


            #region DRAWING THE LARGER ORANGE GRID ====================================
            //this draws the larger, orange grid (with blue lines for the middle) which is useful when zoomed way out
            for (int x = 0; x <= 32; x++)
            {
                if (x == 8 || x == 24)
                    //draw large teal line vertically if this is the middle of the quadrants of the grid
                    spriteBatch.Draw(whiteSquare, new Rectangle(x * (int)((maxMapWidth * blockPixelSize) / 32.0) - (int)cameraOffset.X,
                        -(int)cameraOffset.Y, 2, largeBlockPixelSize * 32), Color.Teal);
                else if (x == 16)
                    //draw large blue line vertically if this is the very middle of the grid
                    spriteBatch.Draw(whiteSquare, new Rectangle(x * (int)((maxMapWidth * blockPixelSize) / 32.0) - (int)cameraOffset.X,
                        -(int)cameraOffset.Y, 2, largeBlockPixelSize * 32), Color.CornflowerBlue);
                else
                    //if not the middle line, draw large orange line vertically
                    spriteBatch.Draw(whiteSquare, new Rectangle(x * (int)((maxMapWidth * blockPixelSize) / 32.0) - (int)cameraOffset.X,
                        -(int)cameraOffset.Y, 2, largeBlockPixelSize * 32), Color.Tomato);
            }
            for (int y = 0; y <= 32; y++)
            {
                if (y == 8 || y == 24)
                    //draw large teal line horizontally if this is the middle of the quadrants of the grid
                    spriteBatch.Draw(whiteSquare, new Rectangle(-(int)cameraOffset.X,
                        y * (int)((maxMapHeight * blockPixelSize) / 32.0) - (int)cameraOffset.Y, largeBlockPixelSize * 32, 2), Color.Teal);
                else if (y == 16)
                    //draw large blue line horizontally if this is the very middle of the grid
                    spriteBatch.Draw(whiteSquare, new Rectangle(-(int)cameraOffset.X,
                        y * (int)((maxMapHeight * blockPixelSize) / 32.0) - (int)cameraOffset.Y, largeBlockPixelSize * 32, 2), Color.CornflowerBlue);
                else
                    //if not the middle line, draw large orange line horizontally
                    spriteBatch.Draw(whiteSquare, new Rectangle(-(int)cameraOffset.X,
                        y * (int)((maxMapHeight * blockPixelSize) / 32.0) - (int)cameraOffset.Y, largeBlockPixelSize * 32, 2), Color.Tomato);

            }
            #endregion
        }

        /// <summary>
        /// When loading in a Scarlet file, this method sets all the map data in the editor such as collision tiles and map objects.
        /// </summary>
        /// <param name="x">the X coord of the top left cornder of the map boundary</param>
        /// <param name="y">the Y coord of the top left cornder of the map boundary</param>
        /// <param name="width">the width of the map boundary</param>
        /// <param name="height">the height of the map boundary</param>
        /// <param name="mapTiles">Array of colors, stores collision tiles for the map</param>
        /// <param name="mapObjectRepresentatives">List of references to map objects placed in the map</param>
        public void SetMapData(int x, int y, int width, int height, Color[,] mapTiles, List<MapObjectRepresentative> mapObjectRepresentatives)
        {
            mapBoundaries = new Boundary(x, y, width, height, maxMapWidth, maxMapHeight);
            this.mapTiles = mapTiles;
            this.placedMapObjects = mapObjectRepresentatives;
            selectedMapObjects = null;
            initialSelectionMade = false;
            selectionBoundaries = null;
            movingAroundSelectedObjects = false;
        }

        /// <summary>
        /// Call this whenever a change to the map has been made so we can set status to unsaved and add the * to window title.
        /// </summary>
        private void MakeChangeToMap()
        {
            //sets unsavedChanges and adds an * to the title bar if it isn't already there
            if (!unsavedChanges)
            {
                unsavedChanges = true;
                Window.Title += " *";
            }
        }

        /// <summary>
        /// Stores the representation of a map object on this map depending on what object mode is currently in use
        /// </summary>
        private void PlaceMapObjectRepresentative()
        {

            MapObjectRepresentative objectToPlace = null;
            switch (currentObjectMode)
            {
                case (ObjectMode.Instance):
                    //if no instances are loaded, dont try to place anything!
                    if (instances.Count == 0)
                        return;

                    objectToPlace = new MapObjectRepresentative(string.Format("i{0}", currentInstanceID),
                        currentMouseSelection);
                    break;

                case (ObjectMode.Entity):
                    //if no entities are loaded, dont try to place anything!
                    if (entities.Count == 0)
                        return;

                    objectToPlace = new MapObjectRepresentative(string.Format("e{0}", currentEntityID),
                        currentMouseSelection);
                    break;

                case (ObjectMode.Prop):
                    //if no props are loaded, dont try to place anything!
                    if (props.Count == 0)
                        return;

                    objectToPlace = new MapObjectRepresentative(string.Format("p{0}", currentPropID),
                        currentMouseSelection);
                    break;
            }
            //checks if there is already the same object at the selected spot
            //if so, dont place a new one. This prevents accidentally putting a bunch of the same objects on top of one another
            foreach (MapObjectRepresentative rep in placedMapObjects)
            {
                if (rep.Position == currentMouseSelection && rep.ID == objectToPlace.ID)
                    return;
            }

            placedMapObjects.Add(objectToPlace);
            MakeChangeToMap();
        }

        /// <summary>
        /// Moves all of the selected objects a certain distance. Use of a boundary prevents the objects from
        /// going outside the map.
        /// </summary>
        /// <param name="distance"></param>
        private void MoveMapSelectedObjects(Point distance)
        {
            if(selectedMapObjects != null)
            {
                
                foreach (MapObjectRepresentative rep in selectedMapObjects)
                {
                    MapObject obj = ParseMapObjectRep(rep.ID);
                    //we create a boundary around the object so that we cannot move it outside the map
                    //(Boundary class already has the code for moving and making sure its inside the map)
                    Boundary boundary = new Boundary(rep.Position.X, rep.Position.Y, obj.Size.X, obj.Size.Y, maxMapWidth, maxMapHeight);
                    boundary.MoveEntireBoundary(distance);
                    rep.Position = boundary.TopLeft;
                }
            }
        }

        /// <summary>
        /// Takes an id of a map object rep and returns the map object its referring to
        /// </summary>
        /// <param name="id">The id of the map object rep</param>
        /// <returns>the map object its referring to</returns>
        public static MapObject ParseMapObjectRep(String id)
        {
            MapObject objectToReturn = null;

            //if the first letter is "i", then its referring to an instance
            if(id.Substring(0,1) == "i")
            {
                //get the instance at the index the id is referring to
                objectToReturn = instances[int.Parse(id.Substring(1))];
            }

            //if the first letter is "e", then its referring to an entity
            else if (id.Substring(0, 1) == "e")
            {
                //get the entity at the index the id is referring to
                objectToReturn = entities[int.Parse(id.Substring(1))];
            }

            //if the first letter is "p", then its referring to an prop
            else if (id.Substring(0, 1) == "p")
            {
                //get the prop at the index the id is referring to
                objectToReturn = props[int.Parse(id.Substring(1))];
            }

            return objectToReturn;
        }

        /// <summary>
        /// Resets the camera view to 0,0
        /// Useful for when you get lost in the void.
        /// </summary>
        public void ResetCamera()
        {
            cameraOffset = Vector2.Zero;
        }

        /// <summary>
        /// Toggles between collision view and object view
        /// </summary>
        public void ToggleView()
        {
            collisionMode = !collisionMode;
        }

        /// <summary>
        /// If unsaved changes were found, prompts the user to save first.
        /// Then, clears out the data in the editor and starts fresh!
        /// </summary>
        public void NewFile()
        {
            bool startNewFile = true;
            if (unsavedChanges)
            {
                //usaved changes are found
                //show message box with options
                System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("There are unsaved changes. Would you like to save before starting a new file?",
                    "Unsaved Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
                if (dr == System.Windows.Forms.DialogResult.Yes)
                {
                    //if user clicks yes, try to save the file
                    if (!SaveFile())
                    {
                        //if the user cancels the save dialog or saving isnt succesful, dont start a new file!
                        startNewFile = false;
                    }
                }
                else if (dr == System.Windows.Forms.DialogResult.Cancel)
                {
                    //if user clicks cancel, dont start a new file
                    startNewFile = false;
                }
            }
            //if user clicks yes (and saving was successful) or no, a new file will be started
            if (startNewFile)
            {
                SetMapData(0, 0, maxMapWidth, maxMapHeight, new Color[maxMapWidth, maxMapHeight], new List<MapObjectRepresentative>());
                fileManager.StoredFileName = null;

                //sets unsaved changes to false because no changes could have been made
                unsavedChanges = false;

                //Sets the titlebar to a blank file name
                Window.Title = "Scarlet Editor - ";
            }
        }

        /// <summary>
        /// If unsaved changes were found, prompts the user to save first.
        /// Then, opens up a file dialog for the user to select a .scar file to open up.
        /// </summary>
        public void OpenFile()
        {
            bool openFile = true;
            if (unsavedChanges)
            {
                //usaved changes are found
                //show message box with options
                System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("There are unsaved changes. Would you like to save before opening another file?",
                    "Unsaved Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
                if (dr == System.Windows.Forms.DialogResult.Yes)
                {
                    //if user clicks yes, try to save the file
                    if (!SaveFile())
                    {
                        //if the user cancels the save dialog or saving isnt succesful, dont start a new file!
                        openFile = false;
                    }
                }
                else if (dr == System.Windows.Forms.DialogResult.Cancel)
                {
                    //if user clicks cancel, dont start a new file
                    openFile = false;
                }
                
            }
            //if user clicks yes (and saving was successful) or no, a file will be chosen to open
            if (openFile)
            {
                //create a file dialog
                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();

                //sets its properties
                fileDialog.Title = "Open a Scarlet File.";
                fileDialog.Filter = "Scarlet Files | *.scar";

                //show it and store the result
                System.Windows.Forms.DialogResult result = fileDialog.ShowDialog();

                //if the user clicked OK
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    fileManager.LoadFile(fileDialog.FileName);
                }
            }

        }

        /// <summary>
        /// Just calls the SaveFile method. This is made so the button's OnClick event would work with these
        /// </summary>
        public void SaveFileButtonClick()
        {
            SaveFile();
        }

        /// <summary>
        /// Just calls the SaveFileAs method. This is made so the button's OnClick event would work with these
        /// </summary>
        public void SaveFileAsButtonClick()
        {
            SaveFileAs();
        }

        /// <summary>
        /// Saves the user's work to the already known location, or lets the user browse if this project is new
        /// </summary>
        /// <returns>Returns true if the save was successful, false otherwise</returns>
        public bool SaveFile()
        {
            //if no version of this map already exists, then run the SaveFileAs method
            if (fileManager.StoredFileName == null)
            {
                return SaveFileAs();
            }
            //otherwise, save the file with the already existing file name/path
            else
            {
                return fileManager.SaveFile(fileManager.StoredFileName, mapBoundaries, mapTiles, placedMapObjects);
            }
        }

        /// <summary>
        /// Opens a file dialog for the user to pick a location to save their work
        /// </summary>
        /// <returns>Returns true if save was successful, false otherwise</returns>
        public bool SaveFileAs()
        {
            //create a file dialog
            System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog();

            //sets its properties
            fileDialog.Title = "Save your Scarlet File.";
            fileDialog.Filter = "Scarlet Files | *.scar";

            //show the dialog and store the result
            System.Windows.Forms.DialogResult result = fileDialog.ShowDialog();

            //if the user clicked OK
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                return fileManager.SaveFile(fileDialog.FileName, mapBoundaries, mapTiles, placedMapObjects);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the currently active tool to the Selection Tool
        /// </summary>
        public void SetCurrentToolToSelect()
        {
            currentTool = SelectedTool.Select;
            Mouse.SetCursor(MouseCursor.Arrow);
        }

        /// <summary>
        /// Sets the currently active tool to the Brush Tool
        /// </summary>
        public void SetCurrentToolToBrush()
        {
            currentTool = SelectedTool.Brush;
            Mouse.SetCursor(MouseCursor.Crosshair);
        }

        /// <summary>
        /// Sets the currently active tool to the Cordon Tool
        /// </summary>
        public void SetCurrentToolToCordon()
        {
            currentTool = SelectedTool.Cordon;
            Mouse.SetCursor(MouseCursor.Arrow);
        }

        /// <summary>
        /// Sets the currently active tool to the Object Tool
        /// </summary>
        public void SetCurrentToolToObject()
        {
            currentTool = SelectedTool.Object;
            Mouse.SetCursor(MouseCursor.Arrow);
        }

        /// <summary>
        /// Checks if the mouse is on the map portion of the screen. Prevents any sort of accidental tile selection
        /// while trying to access the tool bar
        /// </summary>
        /// <returns>True if the mouse is on the map portion of the screen</returns>
        private bool MouseIsInScreen()
        {
            //checks if the mouse's position is inside the window but also not on the toolbar
            if(mouseState.Position.Y > toolBarHeight && mouseState.Position.Y < GraphicsDevice.Viewport.Height &&
                mouseState.Position.X > 0 && mouseState.Position.X < GraphicsDevice.Viewport.Width)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// If the user clicks close on the editor, and there are unsaved changes, it asks if the user
        /// wants to save them before closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (unsavedChanges)
            {
                //usaved changes are found
                //show message box with options
                System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("There are unsaved changes. Would you like to save before quiting?",
                    "Unsaved Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
                if (dr == System.Windows.Forms.DialogResult.Yes)
                {
                    //if user clicks yes, try to save the file
                    if (!SaveFile())
                    {
                        //if the user cancels the save dialog or saving isnt succesfull, dont close editor!
                        e.Cancel = true;
                    }
                }
                else if (dr == System.Windows.Forms.DialogResult.Cancel)
                {
                    //if user clicks cancel, dont close the form
                    e.Cancel = true;
                }
                //if user clicks yes (and saving was successful) or no, the form will close
            }
        }
    }
}
