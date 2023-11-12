using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//Nick Baker, Jack Kalina, Vicente Bernal, Reese Lodwick
//2/12/2020
//Scarlet Meadow monogame project
//this is the root of our game, everything starts here
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// Represents any direction in the game world
    /// </summary>
    public enum Direction
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
        Front
    }

    public enum GameState
    {
        Menu,
        Play,
        Loading,
        Pause,
        Victory,
        GameOver,
    }

    /// <summary>
    /// Any action the player can do in the game with a keyboard or controller
    /// </summary>
    public enum Action
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Jump,
        Accept,
        Melee,
        Attack,
        Special,
        Interact,
        Pause,
        Back,
        Inventory,
    }

    /// <summary>
    /// When we wipe the screen, the action we want to do while the screen is covered
    /// will be determined by this. Could be to load a stage, could be to go back to the menu
    /// </summary>
    public enum ScreenWipeAction
    {
        LoadStage,
        ReturnToMenu
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        /// <summary>
        /// the max height for the map grid, used in map loading
        /// </summary>
        private int maxMapHeight = 1024;

        /// <summary>
        /// the max width for the map grid, used in map loading
        /// </summary>
        private int maxMapWidth = 1024;

        /// <summary>
        /// The amount of pixels each grid tile of the map is scaled to in the final game
        /// </summary>
        private static int gridScale = 40;

        /// <summary>
        /// Used for generating any random values for the game
        /// </summary>
        private static Random rng;

        private SpriteFont arial18;
        private SpriteFont arial24;
        public static Texture2D lassoTexture;
        private Texture2D logoTexture;

        /// <summary>
        /// the state of the game in the current frame
        /// </summary>
        private static GameState currentGameState;

        private MenuManager menuManager;

        /// <summary>
        /// The keyboard state of the current fame
        /// </summary>
        private static KeyboardState kbState;

        /// <summary>
        /// The keybaord state from the previous frame
        /// </summary>
        private static KeyboardState previousKbState;

        /// <summary>
        /// The gamepad state for all controllers of the current frame
        /// </summary>
        private static GamePadState[] gpStates;

        /// <summary>
        /// the gamepad state for all controllers of the previous frame
        /// </summary>
        private static GamePadState[] previousGPStates;

        /// <summary>
        /// A string that was shown in the top right corner of the screen for testing
        /// </summary>
        public static String debug;

        /// <summary>
        /// basically a canvas/sprite we are drawing all of our graphics to
        /// </summary>
        private RenderTarget2D currentRenderTarget;

        /// <summary>
        /// the width of what our game is rendering to
        /// </summary>
        public const int RenderTargetWidth = 1920;

        /// <summary>
        /// the height of what our game is rendering to
        /// </summary>
        public const int RenderTargetHeight = 1080;

        /// <summary>
        /// the starting window width
        /// </summary>
        private static int PreferredWindowWidth = 1600;

        /// <summary>
        /// the starting window height
        /// </summary>
        private static int PreferredWindowHeight = 900;

        /// <summary>
        /// Reference to the player object that is current in the stage.
        /// </summary>
        private Player player;

        /// <summary>
        /// The camera in our game follows the player so they are always in the center. This represents how much
        /// to offset drawing objects in the stage to give the illusion of a moving camera.
        /// </summary>
        private Vector2 cameraOffset;

        /// <summary>
        /// The stage that the user is currently in. Is loaded up when the user selected a stage
        /// from the stage selection screen
        /// </summary>
        private static Stage currentStage;

        /// <summary>
        /// the number of the level the user selected in the menu. Tells us what map to load in.
        /// </summary>
        private int selectedStageIndex;

        /// <summary>
        /// the id of the character that the player has selected to play as 
        /// (0 - Scarlet, 1 - Cocoa)
        /// </summary>
        private int selectedCharacterId;

        /// <summary>
        /// Stores the platform data for the current stage, makes resetting the current level really quick
        /// </summary>
        private List<GameObject> currentStagePlatforms;

        /// <summary>
        /// Stores the entity data for the current stage, makes resetting the current level really quick
        /// </summary>
        private List<MapObjectRepresentative> currentStageEntityReps;

        /// <summary>
        /// Stores the prop data for the current stage, makes resetting the current level really quick
        /// </summary>
        private List<Prop>[] currentStagePropLayers;

        /// <summary>
        /// The position, width and height of the current stage
        /// </summary>
        private Rectangle currentStageTransform;

        /// <summary>
        /// this holds the code for loading map files!
        /// </summary>
        private FileManager fileManager;

        /// <summary>
        /// List of instances that can be selected and placed in the map;
        /// </summary>
        private static List<MapObject> instances;

        /// <summary>
        /// List of props that can be selected and placed in the map;
        /// </summary>
        private static List<MapObject> props;

        /// <summary>
        /// Miscellanous textures for various objects in the game
        /// </summary>
        private static List<Texture2D> miscTextures;

        /// <summary>
        /// Textures used for the UI and HUD
        /// </summary>
        private static List<Texture2D> uiTextures;

        /// <summary>
        /// How much time until the screen is fully wiped by a texture. Used for loading screen transition entry
        /// </summary>
        private double screenWipeEntryTimer;

        /// <summary>
        /// How much time until the screen is fully shown after a wipe. Used for loading screen transition exit
        /// </summary>
        private double screenWipeExitTimer;

        /// <summary>
        /// Maximum time for a screen wipe transition
        /// </summary>
        private const double screenWipeTimeMax = .15;

        /// <summary>
        /// How much time is left in the HUD objective display animation at the beginning of a stage
        /// </summary>
        private static double objectiveDisplayWipeTimer;

        /// <summary>
        /// How much time after the victory screen is trigger before all of its elements are shown on the screen
        /// </summary>
        private static double victoryScreenAnimationTimer;

        /// <summary>
        /// How much time after the game over screen is trigger before all of its elements are shown on the screen
        /// </summary>
        private static double gameOverScreenAnimationTimer;

        /// <summary>
        /// The reason why the player lost the stage
        /// </summary>
        private static string loseMessage;

        /// <summary>
        /// The custom "scarlet color" we use for that yellow to pink gradient
        /// </summary>
        public static Color ScarletColor = new Color(255, 29, 97, 255);

        /// <summary>
        /// Time until the health percentage blinks on the HUD, warning the player of low health!
        /// </summary>
        private double healthBlinkTimer;

        /// <summary>
        /// Time until the special percentage blinks on the HUD, reminding the player that they have full special!
        /// </summary>
        private double specialBlinkTimer;

        /// <summary>
        /// Time until the time left display blinks on the HUD, warning the player they are almost out of time!
        /// </summary>
        private double timeLeftBlinkTimer;

        /// <summary>
        /// How much of the stage's objective the player has accomplished
        /// </summary>
        private static double stageCompletionPercentage;

        /// <summary>
        /// A collection of encouraging messages that display when the player loses a stage!
        /// </summary>
        private static String[] encouragementMessages;

        /// <summary>
        /// The index of the randomly chosen encouragement message to display
        /// </summary>
        private static int chosenEncouragementMessageIndex;

        /// <summary>
        /// When loading a new stage, once it is done loading, we wait a second or else it is
        /// really laggy when starting the stage. Delaying the start of a stage a bit fixes the lag
        /// </summary>
        private double postLoadTimer;

        /// <summary>
        /// The parallax background layers for the Day time background
        /// </summary>
        private Texture2D[] dayBackgroundLayers;

        /// <summary>
        /// The parallax background layers for the Night time background
        /// </summary>
        private Texture2D[] nightBackgroundLayers;

        /// <summary>
        /// The parallax background layers for the Morning time background
        /// </summary>
        private Texture2D[] morningBackgroundLayers;

        /// <summary>
        /// The parallax background layers for the Evening time background
        /// </summary>
        private Texture2D[] eveningBackgroundLayers;

        /// <summary>
        /// This array stores all the backgrounds layers in this game. We do this so we can
        /// easily randomly pick a background for the background of the title/menu stage
        /// </summary>
        private Texture2D[][] allBackgrounds;

        /// <summary>
        /// The stage we render in the background of the menu. 
        /// We have this stage so we can do parallax scrolling of the background images
        /// </summary>
        private Stage titleStage;

        /// <summary>
        /// The camera offset that offsets the graphics in the title stage
        /// </summary>
        private Vector2 titleCameraOffset;

        /// <summary>
        /// If true, the camera offset for the title background will be moving left rather than right
        /// </summary>
        private bool titleCameraScrollLeft;

        /// <summary>
        /// The action we want to do when we wipe the screen, such as loading in a stage or returning to the menu.
        /// </summary>
        private ScreenWipeAction screenWipeAction;

        #region Animation Sets for our characters
        /// <summary>
        /// Animation set for Scarlet (first playable character)
        /// </summary>
        private Animation[] scarletAnims;

        /// <summary>
        /// Animation set for Cocoa (second playable character)
        /// </summary>
        private Animation[] cocoaAnims;

        /// <summary>
        /// Animations for the lawmaker enemy (the Grabbers)
        /// </summary>
        private Animation[] lawmakerAnims;

        /// <summary>
        /// Specific attack animations for the lawmaker enemy (the Grabbers)
        /// </summary>
        private Animation[] lawmakerAttackAnims;

        /// <summary>
        /// Animations for the Robber Baron enemy
        /// </summary>
        private Animation[] robberBaronAnims;

        /// <summary>
        /// Animations for the Cactus enemy
        /// </summary>
        private Animation[] cactusAnims;

        /// <summary>
        /// Animations for the Tumble Weed enemy
        /// </summary>
        private Animation[] tumbleWeedAnims;
        #endregion

        /// <summary>
        /// Get the list of map instances that can be placed in a stage
        /// </summary>
        public List<MapObject> Instances
        {
            get { return instances; }
        }

        /// <summary>
        /// Get the list of props that can be placed in a stage
        /// </summary>
        public List<MapObject> Props
        {
            get { return props; }
        }

        /// <summary>
        /// Get a list of various textures for items and objects
        /// </summary>
        public static List<Texture2D> MiscTextures
        {
            get { return miscTextures; }
        }

        public static List<Texture2D> UITextures
        {
            get { return uiTextures; }
        }

        /// <summary>
        /// returns the max map width limit used in map loading
        /// </summary>
        public int MaxMapWidth
        {
            get { return maxMapWidth; }
        }

        /// <summary>
        /// returns the max map height limit used in map loading
        /// </summary>
        public int MaxMapHeight
        {
            get { return maxMapHeight; }
        }

        /// <summary>
        /// Get the amount of pixels each grid tile of the map is scaled to in the final game
        /// </summary>
        public static int Scale
        {
            get { return gridScale; }
        }

        /// <summary>
        /// Get the random object for generating random values
        /// </summary>
        public static Random RNG
        {
            get { return rng; }
        }

        /// <summary>
        /// Get or set the state of the game
        /// </summary>
        public GameState GameState
        {
            get { return currentGameState; }
            set { currentGameState = value; }
        }

        /// <summary>
        /// Used by the current stage to trigger the HUD objective display animation
        /// </summary>
        public static double ObjectiveDisplayTimer
        {
            set { objectiveDisplayWipeTimer = value; }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            rng = new Random();

            this.IsMouseVisible = true;

            //setting window properties. it can be resized, but at first we want it at a predetermined size
            Window.AllowUserResizing = true;
            graphics.PreferredBackBufferHeight = PreferredWindowHeight;
            graphics.PreferredBackBufferWidth = PreferredWindowWidth;
            Window.Title = "Scarlet Meadow";

            //sets the window to be maximized automatically
            System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            form.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            //initializing our gamepad state array for gamepad/controller support
            gpStates = new GamePadState[4];
            previousGPStates = new GamePadState[4];

            //the game always starts in the menu state
            currentGameState = GameState.Menu;

            debug = "---";

            graphics.ApplyChanges();

            cameraOffset = new Vector2(0, 0);

            //when displaying our game, we draw it to a render target that is then drawn to fit the screen.
            //this allows us to stretch the game's graphics to fit the screen
            currentRenderTarget = new RenderTarget2D(graphics.GraphicsDevice, RenderTargetWidth, RenderTargetHeight);

            //initialize our texture storage
            fileManager = new FileManager(this);
            props = new List<MapObject>();
            miscTextures = new List<Texture2D>();
            uiTextures = new List<Texture2D>();

            menuManager = new MenuManager(this);

            titleStage = new Stage();

            screenWipeEntryTimer = -1;
            screenWipeExitTimer = -1;


            //these messages are displayed on the defeat screen when the player loses the stage
            encouragementMessages = new String[5];

            encouragementMessages[0] = "keep going!";

            encouragementMessages[1] = "you got this!";

            encouragementMessages[2] = "you're getting there!";

            encouragementMessages[3] = "don't give up!";

            encouragementMessages[4] = "never stop, never quit!";

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

            lassoTexture = Content.Load<Texture2D>("beta_lasso");
            arial18 = Content.Load<SpriteFont>("Arial18");
            arial24 = Content.Load<SpriteFont>("Arial24");
            logoTexture = Content.Load<Texture2D>("Scarlet Meadow Logo Final");

            //load content for our customized font drawer
            ScarletFontDrawer.LoadContent(this);

            #region LOADING SCARLET'S ANIMATIONS
            //Loading in Scarlet's animations
            scarletAnims = new Animation[9];

            //loading the run animation
            Texture2D[] scarletRunFrames = {Content.Load<Texture2D>("Characters/Scarlet/run0"),
            Content.Load<Texture2D>("Characters/Scarlet/run1"),
            Content.Load<Texture2D>("Characters/Scarlet/run2"),
            Content.Load<Texture2D>("Characters/Scarlet/run3"),
            Content.Load<Texture2D>("Characters/Scarlet/run4"),
            Content.Load<Texture2D>("Characters/Scarlet/run5")};
            scarletAnims[(int) PlayerState.Run] = new Animation(scarletRunFrames, 110, null, new Point(-45, 0), .2133f);

            //loading the idle animation
            Texture2D[] scarletIdleFrames = {Content.Load<Texture2D>("Characters/Scarlet/idle0"),
            Content.Load<Texture2D>("Characters/Scarlet/idle1"),
            Content.Load<Texture2D>("Characters/Scarlet/idle2")};
            scarletAnims[(int)PlayerState.Idle] = new Animation(scarletIdleFrames, 220, new int[] { 0, 1, 2, 1}, new Point(-30, 0), .2133f);

            //loading the charge animation
            Texture2D[] scarletChargeFrames = {Content.Load<Texture2D>("Characters/Scarlet/charge0"),
            Content.Load<Texture2D>("Characters/Scarlet/charge1"),
            Content.Load<Texture2D>("Characters/Scarlet/charge2")};
            scarletAnims[(int)PlayerState.Charge] = new Animation(scarletChargeFrames, 150, new int[] { 0, 1, 2, 1 }, new Point(-48, 0), .2133f);

            //loading in the rest of Scarlet's animations (all single frame)
            scarletAnims[(int)PlayerState.Attack] = new Animation(new Texture2D[] {Content.Load<Texture2D>("Characters/Scarlet/attack") },
                0, null, new Point(-30, 0), .2133f);
            scarletAnims[(int)PlayerState.Knockback] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Scarlet/damage") },
                0, null, Point.Zero, .2133f);
            scarletAnims[(int)PlayerState.Jump] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Scarlet/jump") },
                0, null, new Point(-38, 0), .2133f);
            scarletAnims[(int)PlayerState.Fall] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Scarlet/fall") },
                0, null, new Point(-38, 0), .2133f);
            scarletAnims[(int)PlayerState.Melee] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Scarlet/kick") },
                0, null, new Point(-60, 0), .2133f);
            scarletAnims[(int)PlayerState.Zipline] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Scarlet/zipline") },
                0, null, Point.Zero, .2133f);

            #endregion

            #region LOADING COCOA'S ANIMATIONS
            //Loading in Cocoa's animations
            cocoaAnims = new Animation[9];

            //loading the run animation
            Texture2D[] cocoaRunFrames = {Content.Load<Texture2D>("Characters/Cocoa/run0"),
            Content.Load<Texture2D>("Characters/Cocoa/run1"),
            Content.Load<Texture2D>("Characters/Cocoa/run2"),
            Content.Load<Texture2D>("Characters/Cocoa/run3"),
            Content.Load<Texture2D>("Characters/Cocoa/run4"),
            Content.Load<Texture2D>("Characters/Cocoa/run5")};
            cocoaAnims[(int)PlayerState.Run] = new Animation(cocoaRunFrames, 110, null, new Point(-45, 0), .2133f);

            //loading the idle animation
            Texture2D[] cocoaIdleFrames = {Content.Load<Texture2D>("Characters/Cocoa/idle0"),
            Content.Load<Texture2D>("Characters/Cocoa/idle1"),
            Content.Load<Texture2D>("Characters/Cocoa/idle2")};
            cocoaAnims[(int)PlayerState.Idle] = new Animation(cocoaIdleFrames, 220, new int[] { 0, 1, 2, 1 }, new Point(-30, 0), .2133f);
            cocoaAnims[(int)PlayerState.Charge] = cocoaAnims[(int)PlayerState.Idle];

            //loading in the rest of Cocoa's animations (all single frame)
            cocoaAnims[(int)PlayerState.Attack] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Cocoa/attack") },
                0, null, new Point(-30, 0), .2133f);
            cocoaAnims[(int)PlayerState.Knockback] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Cocoa/damage") },
                0, null, Point.Zero, .2133f);
            cocoaAnims[(int)PlayerState.Jump] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Cocoa/jump") },
                0, null, new Point(-38, 0), .2133f);
            cocoaAnims[(int)PlayerState.Fall] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Cocoa/fall") },
                0, null, new Point(-38, 0), .2133f);
            cocoaAnims[(int)PlayerState.Melee] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Cocoa/melee") },
                0, null, new Point(-60, 0), .2133f);
            cocoaAnims[(int)PlayerState.Zipline] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Cocoa/zipline") },
                0, null, Point.Zero, .2133f);

            #endregion

            #region LOADING LAWMAKER ENEMY'S ANIMATIONS

            lawmakerAnims = new Animation[6];

            Texture2D[] lawmakerWalkFrames = {Content.Load<Texture2D>("Characters/Lawmaker/walk0"),
                Content.Load<Texture2D>("Characters/Lawmaker/walk1"),
                Content.Load<Texture2D>("Characters/Lawmaker/walk2")};

            lawmakerAnims[(int)EnemyState.Patrol] = new Animation(lawmakerWalkFrames, 250, new int[] { 0, 1, 2, 1 }, new Point(-100, 0), .2333f);
            lawmakerAnims[(int)EnemyState.Enraged] = new Animation(lawmakerWalkFrames, 240, new int[] { 0, 1, 2, 1 }, new Point(-100, 0), .2333f);


            lawmakerAnims[(int)EnemyState.Idle] = new Animation(new Texture2D[] { lawmakerWalkFrames[1] }, 0, null, new Point(-100, 0), .2333f);
            lawmakerAnims[(int)EnemyState.Restrained] = new Animation(new Texture2D[] { lawmakerWalkFrames[1] }, 0, null, new Point(-100, 0), .2333f);
            lawmakerAnims[(int)EnemyState.Knockback] = new Animation(new Texture2D[] { Content.Load<Texture2D>("Characters/Lawmaker/damage") }, 0, null, new Point(-100, 0), .2333f);

            lawmakerAttackAnims = new Animation[2];

            Texture2D[] lawmakerAttackScarletFrames = {Content.Load<Texture2D>("Characters/Lawmaker/attack-scarlet0"),
                Content.Load<Texture2D>("Characters/Lawmaker/attack-scarlet1"),
                Content.Load<Texture2D>("Characters/Lawmaker/throw")};

            Texture2D[] lawmakerAttackCocoaFrames = {Content.Load<Texture2D>("Characters/Lawmaker/attack-cocoa0"),
                Content.Load<Texture2D>("Characters/Lawmaker/attack-cocoa1"),
                lawmakerAttackScarletFrames[2]};

            lawmakerAttackAnims[0] = new Animation(lawmakerAttackScarletFrames, 220, null, new Point(-100, 0), .2333f);
            lawmakerAttackAnims[1] = new Animation(lawmakerAttackCocoaFrames, 220, null, new Point(-100, 0), .2333f);

            #endregion

            #region LOADING ROBBER BARON ENEMY'S ANIMATIONS

            robberBaronAnims = new Animation[6];

            Texture2D robberBaronIdle = Content.Load<Texture2D>("Characters/Robber-Baron/idle");
            Texture2D robberBaronAttack = Content.Load<Texture2D>("Characters/Robber-Baron/attack");
            Texture2D robberBaronDamage = Content.Load<Texture2D>("Characters/Robber-Baron/damage");

            robberBaronAnims[(int)EnemyState.Patrol] = new Animation(new Texture2D[] {robberBaronIdle}, 0, null, new Point(0, 0), .2f);
            robberBaronAnims[(int)EnemyState.Attack] = new Animation(new Texture2D[] { robberBaronAttack }, 0, null, new Point(0, 0), .2f);
            robberBaronAnims[(int)EnemyState.Enraged] = new Animation(new Texture2D[] { robberBaronAttack }, 0, null, new Point(0, 0), .2f);
            robberBaronAnims[(int)EnemyState.Idle] = new Animation(new Texture2D[] { robberBaronIdle }, 0, null, new Point(0, 0), .2f);
            robberBaronAnims[(int)EnemyState.Knockback] = new Animation(new Texture2D[] { robberBaronDamage }, 0, null, new Point(0, 0), .2f);
            robberBaronAnims[(int)EnemyState.Restrained] = new Animation(new Texture2D[] { robberBaronDamage }, 0, null, new Point(0, 0), .2f);

            #endregion

            #region LOADING CACTUS ENEMY'S ANIMATIONS

            cactusAnims = new Animation[6];

            Texture2D[] cactusWalkFrames = {Content.Load<Texture2D>("Characters/Cactus/walk0"),
            Content.Load<Texture2D>("Characters/Cactus/walk1"),
            Content.Load<Texture2D>("Characters/Cactus/walk2")};

            Animation cactusWalkAnim = new Animation(cactusWalkFrames, 270, new int[] { 0, 1, 2, 1 }, new Point(-20, 0), .2667f);

            cactusAnims[(int)EnemyState.Patrol] = cactusWalkAnim;
            cactusAnims[(int)EnemyState.Attack] = cactusWalkAnim;
            cactusAnims[(int)EnemyState.Enraged] = cactusWalkAnim;
            cactusAnims[(int)EnemyState.Idle] = new Animation(new Texture2D[] { cactusWalkFrames[0] }, 0, null, new Point(-20, 0), .2667f);
            cactusAnims[(int)EnemyState.Knockback] = new Animation(new Texture2D[] { cactusWalkFrames[0] }, 0, null, new Point(-20, 0), .2667f);
            cactusAnims[(int)EnemyState.Restrained] = new Animation(new Texture2D[] { cactusWalkFrames[0] }, 0, null, new Point(-20, 0), .2667f);

            #endregion

            #region LOADING TUMBLE WEED ENEMY'S ANIMATIONS

            tumbleWeedAnims = new Animation[6];

            Texture2D[] tumbleWeedFrames = {Content.Load<Texture2D>("Characters/Tumbleweed/tumbleweedUp"),
            Content.Load<Texture2D>("Characters/Tumbleweed/tumbleweedRight"),
            Content.Load<Texture2D>("Characters/Tumbleweed/tumbleweedDown"),
            Content.Load<Texture2D>("Characters/Tumbleweed/tumbleweedLeft")};

            Animation tumbleWeedBounceAnim = new Animation(tumbleWeedFrames, 240, null, new Point(-20, 0), .24f);

            tumbleWeedAnims[(int)EnemyState.Patrol] = tumbleWeedBounceAnim;
            tumbleWeedAnims[(int)EnemyState.Attack] = tumbleWeedBounceAnim;
            tumbleWeedAnims[(int)EnemyState.Enraged] = tumbleWeedBounceAnim;
            tumbleWeedAnims[(int)EnemyState.Idle] = new Animation(new Texture2D[] { tumbleWeedFrames[0] }, 0, null, new Point(-20, 0), .24f);
            tumbleWeedAnims[(int)EnemyState.Knockback] = new Animation(new Texture2D[] { tumbleWeedFrames[2] }, 0, null, new Point(-20, 0), .24f);
            tumbleWeedAnims[(int)EnemyState.Restrained] = new Animation(new Texture2D[] { tumbleWeedFrames[0] }, 0, null, new Point(-20, 0), .24f);

            #endregion


            #region LOADING IN INSTANCE ASSETS ============================================================================================
            instances = fileManager.LoadInstances(@"..\..\..\..\Content\instances");
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

            string[] colors = { "blue", "green", "orange", "pink", "purple", "yellow" };
            string[] directions = { "front", "left", "right", "side", "back" };
            for (int i = 0; i < colors.Length; i++)
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
                if (i == 5)
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

            #region LOADING IN MISC TEXTURES ============================================================================================
            miscTextures.Add(Content.Load<Texture2D>("entities/trampolineFacingFront"));
            miscTextures.Add(Content.Load<Texture2D>("entities/trampolineFacingLeft"));
            miscTextures.Add(Content.Load<Texture2D>("entities/trampolineFacingRight"));
            miscTextures.Add(Content.Load<Texture2D>("entities/zipLineGoingLeftShort"));
            miscTextures.Add(Content.Load<Texture2D>("entities/zipLineGoingRightShort"));
            miscTextures.Add(Content.Load<Texture2D>("entities/boom")); //5
            miscTextures.Add(Content.Load<Texture2D>("entities/robber-bullet"));
            miscTextures.Add(Content.Load<Texture2D>("entities/cocoa-bomb"));
            miscTextures.Add(Content.Load<Texture2D>("entities/happyskull"));
            miscTextures.Add(Content.Load<Texture2D>("entities/shockedskull"));
            miscTextures.Add(Content.Load<Texture2D>("entities/luchadormask")); //10
            miscTextures.Add(Content.Load<Texture2D>("entities/jalapeno"));
            miscTextures.Add(Content.Load<Texture2D>("entities/badgebitbig"));
            miscTextures.Add(Content.Load<Texture2D>("entities/badgebitsmall"));
            miscTextures.Add(Content.Load<Texture2D>("entities/spikeUp"));
            miscTextures.Add(Content.Load<Texture2D>("entities/lasso-front")); //15
            miscTextures.Add(Content.Load<Texture2D>("entities/lasso-back"));
            miscTextures.Add(Content.Load<Texture2D>("entities/lasso-part"));
            miscTextures.Add(Content.Load<Texture2D>("entities/goalOpened"));
            miscTextures.Add(Content.Load<Texture2D>("entities/goalClosed"));
            miscTextures.Add(Content.Load<Texture2D>("entities/tombstone-scarlet")); //20
            miscTextures.Add(Content.Load<Texture2D>("entities/tombstone-cocoa"));
            miscTextures.Add(Content.Load<Texture2D>("entities/civilian"));
            miscTextures.Add(Content.Load<Texture2D>("entities/help"));
            #endregion

            #region LOADING IN UI/HUD TEXTURES ============================================================================================
            uiTextures.Add(Content.Load<Texture2D>("UI/special-background")); //0
            uiTextures.Add(Content.Load<Texture2D>("UI/special-border"));
            uiTextures.Add(Content.Load<Texture2D>("UI/special-Scarlet"));
            uiTextures.Add(Content.Load<Texture2D>("UI/special-Cocoa"));
            uiTextures.Add(Content.Load<Texture2D>("UI/badge"));
            uiTextures.Add(Content.Load<Texture2D>("UI/badge-background")); //5
            uiTextures.Add(Content.Load<Texture2D>("UI/hud-background"));
            uiTextures.Add(Content.Load<Texture2D>("UI/health-bar"));
            uiTextures.Add(Content.Load<Texture2D>("UI/screen-wipe"));
            uiTextures.Add(Content.Load<Texture2D>("UI/screen-black"));
            uiTextures.Add(Content.Load<Texture2D>("UI/screen-darken")); //10
            uiTextures.Add(Content.Load<Texture2D>("UI/yeehaw"));
            uiTextures.Add(Content.Load<Texture2D>("UI/victory-scarlet"));
            uiTextures.Add(Content.Load<Texture2D>("UI/victory-cocoa"));
            uiTextures.Add(Content.Load<Texture2D>("UI/corner-overlay"));
            uiTextures.Add(Content.Load<Texture2D>("UI/charge-border")); //15
            uiTextures.Add(Content.Load<Texture2D>("UI/charge-bar"));
            uiTextures.Add(Content.Load<Texture2D>("UI/badge-background-white"));
            uiTextures.Add(Content.Load<Texture2D>("UI/title-logo"));
            uiTextures.Add(Content.Load<Texture2D>("UI/title-characters"));
            uiTextures.Add(Content.Load<Texture2D>("UI/completion-border")); //20
            uiTextures.Add(Content.Load<Texture2D>("UI/completion-bar"));
            uiTextures.Add(Content.Load<Texture2D>("UI/icon-scarlet"));
            uiTextures.Add(Content.Load<Texture2D>("UI/icon-cocoa"));
            uiTextures.Add(Content.Load<Texture2D>("UI/button-basic-0"));
            uiTextures.Add(Content.Load<Texture2D>("UI/button-basic-1")); //25
            uiTextures.Add(Content.Load<Texture2D>("UI/css-scarlet-0"));
            uiTextures.Add(Content.Load<Texture2D>("UI/css-scarlet-1"));
            uiTextures.Add(Content.Load<Texture2D>("UI/css-cocoa-0"));
            uiTextures.Add(Content.Load<Texture2D>("UI/css-cocoa-1"));
            uiTextures.Add(Content.Load<Texture2D>("UI/portrait-scarlet")); //30
            uiTextures.Add(Content.Load<Texture2D>("UI/portrait-cocoa"));
            uiTextures.Add(Content.Load<Texture2D>("UI/howtoplay-pg1"));
            uiTextures.Add(Content.Load<Texture2D>("UI/howtoplay-pg2"));
            uiTextures.Add(Content.Load<Texture2D>("UI/howtoplay-pg3"));
            #endregion


            #region LOADING BACKGROUND TEXTURES
            dayBackgroundLayers = new Texture2D[3];
            dayBackgroundLayers[0] = Content.Load<Texture2D>("Backgrounds/day-sky");
            dayBackgroundLayers[1] = Content.Load<Texture2D>("Backgrounds/day-far");
            dayBackgroundLayers[2] = Content.Load<Texture2D>("Backgrounds/day-near");

            nightBackgroundLayers = new Texture2D[3];
            nightBackgroundLayers[0] = Content.Load<Texture2D>("Backgrounds/night-sky");
            nightBackgroundLayers[1] = Content.Load<Texture2D>("Backgrounds/night-far");
            nightBackgroundLayers[2] = Content.Load<Texture2D>("Backgrounds/night-near");

            morningBackgroundLayers = new Texture2D[3];
            morningBackgroundLayers[0] = Content.Load<Texture2D>("Backgrounds/morning-sky");
            morningBackgroundLayers[1] = Content.Load<Texture2D>("Backgrounds/morning-far");
            morningBackgroundLayers[2] = Content.Load<Texture2D>("Backgrounds/morning-near");

            eveningBackgroundLayers = new Texture2D[3];
            eveningBackgroundLayers[0] = Content.Load<Texture2D>("Backgrounds/evening-sky");
            eveningBackgroundLayers[1] = Content.Load<Texture2D>("Backgrounds/evening-far");
            eveningBackgroundLayers[2] = Content.Load<Texture2D>("Backgrounds/evening-near");

            allBackgrounds = new Texture2D[4][];

            allBackgrounds[0] = dayBackgroundLayers;
            allBackgrounds[1] = nightBackgroundLayers;
            allBackgrounds[2] = eveningBackgroundLayers;
            allBackgrounds[3] = morningBackgroundLayers;

            SetTitleStageBackground();
            #endregion

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!IsActive)
                return;


            //get the most recent states for our keyboard and gamepads
            previousKbState = kbState;

            kbState = Keyboard.GetState();

            previousGPStates[0] = gpStates[0];
            previousGPStates[1] = gpStates[1];
            previousGPStates[2] = gpStates[2];
            previousGPStates[3] = gpStates[3];

            gpStates[0] = GamePad.GetState(PlayerIndex.One);
            gpStates[1] = GamePad.GetState(PlayerIndex.Two);
            gpStates[2] = GamePad.GetState(PlayerIndex.Three);
            gpStates[3] = GamePad.GetState(PlayerIndex.Four);


            switch (currentGameState)
            {
                case GameState.Menu:
                    //we scrolling through a background in the menu, when it hits the edge, 
                    //make it scroll the opposite wayu
                    if(titleCameraOffset.X <= 0)
                        titleCameraScrollLeft = false;
                    else if(titleCameraOffset.X >= 1024 * Scale)
                        titleCameraScrollLeft = true;

                    if (titleCameraScrollLeft)
                        titleCameraOffset.X -= 5;
                    else
                        titleCameraOffset.X += 5;

                    //just update the menu
                    menuManager.Update(gameTime);

                    //and set the current stage to null so we know to load in a new one next time we play a stage
                    if (currentStage != null)
                        currentStage = null;
                    break;

                case GameState.Loading:
                    //When we go into the load state, we first need to determine if we are actually loading a new stage
                    //or just restarting the current one.

                    //if no stage is loaded, then we much load one from a file!
                    if(currentStage == null)
                    {
                        postLoadTimer = 1;
                        //loading in our selected stage
                        fileManager.LoadStage(@"..\..\..\..\Content\stages\level" + (selectedStageIndex + 1) + ".scar");
                    }

                    //when finished loading from a file, the game lags for a bit, so we add a delay to the loading screen
                    //to get rid of the lag before the player starts playing the stage
                    if(postLoadTimer > 0)
                    {
                        postLoadTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                    }

                    //when that delay timer is done, then we can start playing!
                    else
                    {
                        currentGameState = GameState.Play;
                        screenWipeExitTimer = screenWipeTimeMax;
                    }
                    break;

                case GameState.Play:
                    //pressing pause obviously pauses the game :)
                    if (ActionPressedThisFrame(Action.Pause))
                    {
                        currentGameState = GameState.Pause;
                        menuManager.PauseSelectionId = 0;
                    }


                    //if a stage is loaded, then be updating the stage and camera offset!
                    if (currentStage != null)
                    {
                        currentStage.Update(gameTime);

                        //player is normally in the center of the screen
                        cameraOffset.X = player.Transform.Center.X - RenderTargetWidth / 2;

                        //but we must clamp the X camera offset to the edges of the map so we don't see past the edges
                        cameraOffset.X = Math.Max(currentStageTransform.X, Math.Min(currentStageTransform.X + currentStageTransform.Width - RenderTargetWidth, cameraOffset.X));

                        //vertical camera movement is a bit different. We want the player to be in the center of the screen
                        //when they are in the air, but when they are on solid ground, we want them to be in the
                        //bottom quater of the screen. Finally, we want to SMOOTHLY switch between these states, as not to
                        //confused the player or possibly make them motion sick with a jittery camera

                        //if the player is not on solid ground, put them in the middle of the screen
                        float cameraYInAir = player.Transform.Center.Y - RenderTargetHeight / 2;

                        //if they are on solid ground, put them near the bottom of the screen
                        float cameraYOnSolidGround = player.Transform.Bottom - RenderTargetHeight + (RenderTargetHeight / 4);

                        //find the difference between the two positions
                        float cameraYDifference = cameraYOnSolidGround - cameraYInAir;

                        //finally, offset the camera vertically depending on how much time the player has been on solid ground or not
                        cameraOffset.Y = cameraYInAir + (cameraYDifference*(float)player.TimeSpentOnSolidGroundPercent);

                        //and clamp the vertical position so we dont see past the edges
                        cameraOffset.Y = Math.Max(currentStageTransform.Y, Math.Min(currentStageTransform.Y + currentStageTransform.Height - RenderTargetHeight, cameraOffset.Y));
                    }

                    //if the objective display hud element is still playing its animation, then keep updating the animation timer
                    if(objectiveDisplayWipeTimer >= 0)
                    {
                        objectiveDisplayWipeTimer -= gameTime.ElapsedGameTime.TotalSeconds;

                        if(currentStage.CurrentObjective == GameObjective.GetToEndUnderTimeLimit)
                        {
                            objectiveDisplayWipeTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                        }
                    }
                    break;
                case GameState.Pause:
                    menuManager.UpdatePauseMenu(gameTime);
                    if (ActionPressed(Action.Jump) && currentStage != null && player != null)
                    {
                        player.CanJump = false;
                    }
                    break;

                case GameState.Victory:
                    //wait until the screen animation is done...
                    if (victoryScreenAnimationTimer > 0)
                        victoryScreenAnimationTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                    //...then accept user inputs
                    else
                    {
                        if (ActionPressedThisFrame(Action.Accept) && screenWipeEntryTimer == -1)
                            RestartStage();
                        else if (ActionPressedThisFrame(Action.Back) && screenWipeEntryTimer == -1)
                            QuitStage();
                    }
                    break;

                case GameState.GameOver:
                    //wait until the screen animation is done...
                    if (gameOverScreenAnimationTimer > 0)
                        gameOverScreenAnimationTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                    //...then accept user inputs
                    else
                    {
                        if (ActionPressedThisFrame(Action.Accept) && screenWipeEntryTimer == -1)
                            RestartStage();
                        else if (ActionPressedThisFrame(Action.Back) && screenWipeEntryTimer == -1)
                            QuitStage();
                    }
                    break;

                default:
                    currentGameState = GameState.Menu;
                    break;
            }

            //the screen wipes are a HUD animation used when a level is loading, and we are just updating their timers here:

            //update the animation timer for when the screen wipe is entering the screen from the left
            if (screenWipeEntryTimer > 0)
            {
                if (screenWipeEntryTimer - gameTime.ElapsedGameTime.TotalSeconds < 0)
                    screenWipeEntryTimer = 0;
                else
                    screenWipeEntryTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            //wehn the screen is wiped and covered, then we start the load sequence
            else if (screenWipeEntryTimer == 0)
            {
                switch (screenWipeAction)
                {
                    case (ScreenWipeAction.LoadStage):
                        //if no stage is loaded, we must load one from a file
                        if (currentStage == null)
                            currentGameState = GameState.Loading;
                        //if a stage is loaded, then just restart the current one!
                        else
                            LoadStage(currentStagePlatforms, currentStageEntityReps, currentStagePropLayers, currentStageTransform);
                        break;

                    case (ScreenWipeAction.ReturnToMenu):
                        currentGameState = GameState.Menu;
                        screenWipeExitTimer = screenWipeTimeMax;
                        break;
                }

                screenWipeEntryTimer--;
            }

            //once we are done loading, then the screen wipe/cover should exit off to the right
            if (screenWipeExitTimer > 0)
            {
                screenWipeExitTimer -= gameTime.ElapsedGameTime.TotalSeconds;
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
            GraphicsDevice.SetRenderTarget(currentRenderTarget);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            //draw our graphics here!
            spriteBatch.Begin();



            switch (currentGameState)
            {
                case GameState.Menu:
                    titleStage.Draw(spriteBatch, titleCameraOffset);
                    //spriteBatch.Draw(uiTextures[9], new Rectangle(0, 0, RenderTargetWidth, RenderTargetHeight), Color.White);
                    menuManager.Draw(spriteBatch);
                    break;

                case GameState.Loading:
                    spriteBatch.Draw(uiTextures[9], new Rectangle(0, 0, RenderTargetWidth, RenderTargetHeight), Color.White);
                    ScarletFontDrawer.DrawStringScaled(spriteBatch, "loading...", RenderTargetWidth - 30, RenderTargetHeight - 30, 80, TextAlignment.BottomRight, true, Color.White, Color.White);
                    break;

                case GameState.Play:
                    //draw all the objects in the stage
                    currentStage.Draw(spriteBatch, cameraOffset);


                    #region DRAWING OUR IN GAME HUD

                    //draw the charge meter for the player's main attack charge
                    if(player.ChargePercentage > 0)
                    {
                        spriteBatch.Draw(uiTextures[16], new Vector2(player.X - cameraOffset.X, player.Transform.Bottom + 10 - cameraOffset.Y),
                            new Rectangle(0, 0, (int)(uiTextures[16].Width * player.ChargePercentage), uiTextures[16].Height), Color.White);
                        spriteBatch.Draw(uiTextures[15], new Vector2(player.X - cameraOffset.X, player.Transform.Bottom + 10 - cameraOffset.Y), Color.White);
                    }

                    //drawing the health bar
                    spriteBatch.Draw(uiTextures[7], new Vector2(162, 41), new Rectangle(0, 0, (int)(uiTextures[7].Width * (player.Health/100.0)), uiTextures[7].Height), Color.White);
                    spriteBatch.Draw(uiTextures[6], new Rectangle(-30, 0, 600, 300), Color.White);
                    //if the players health is greater than 50, then display it as normal
                    if (player.Health > 50)
                    {
                        ScarletFontDrawer.DrawString(spriteBatch, "hp: " + player.Health + "%", 320, 90, 250, 75, TextAlignment.TopLeft, true, Color.White, Color.White);
                    }
                    //if its less than 50, make the percentage blink to warn the player
                    else
                    {
                        healthBlinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
                        if (healthBlinkTimer > .18)
                        {
                            healthBlinkTimer = 0;
                        }
                        else
                        {
                            ScarletFontDrawer.DrawString(spriteBatch, "hp: " + player.Health + "%", 320, 90, 290, 90, TextAlignment.TopLeft, true, Color.Yellow, Color.Yellow);
                        }
                    }


                    int specialBadgeSize = 350;
                    int specialBadgePos = -40;

                    spriteBatch.Draw(uiTextures[5], new Rectangle(specialBadgePos, specialBadgePos, specialBadgeSize, specialBadgeSize), Color.White);

                    //if the player has full special meter, blink it to remind the player
                    if(player.SpecialMeterFill >= 100)
                    {
                        specialBlinkTimer += gameTime.ElapsedGameTime.TotalSeconds;

                        if(specialBlinkTimer > .18)
                        {
                            specialBlinkTimer = 0;
                            spriteBatch.Draw(uiTextures[17], new Rectangle(specialBadgePos, specialBadgePos, specialBadgeSize, specialBadgeSize), Color.White);
                        }
                        else
                        {
                            spriteBatch.Draw(uiTextures[4], new Rectangle(specialBadgePos, specialBadgePos, specialBadgeSize, specialBadgeSize), Color.White);
                            ScarletFontDrawer.DrawString(spriteBatch, "special", specialBadgePos + (specialBadgeSize / 2), specialBadgePos + (specialBadgeSize / 2), (specialBadgeSize / 2), (specialBadgeSize / 8), TextAlignment.BottomCenter, false, Color.Yellow, Color.DeepPink);
                            ScarletFontDrawer.DrawString(spriteBatch, player.SpecialMeterFill + "%", specialBadgePos + (specialBadgeSize / 2), specialBadgePos + (specialBadgeSize / 2), (specialBadgeSize / 3), (specialBadgeSize / 6), TextAlignment.TopCenter, false, Color.Yellow, Color.DeepPink);
                        }
                    }
                    //other than that, just draw it as normal
                    else
                    {
                        //as the special meter increases, the sheriff badge image gets bigger. this just does some maths to figure out what size/position it should be display in
                        spriteBatch.Draw(uiTextures[4], new Rectangle(specialBadgePos + (specialBadgeSize / 2) - (int)((specialBadgeSize / 2) * player.SpecialMeterFill / 100.0), specialBadgePos + (specialBadgeSize / 2) - (int)((specialBadgeSize / 2) * player.SpecialMeterFill / 100.0),
                            (int)(specialBadgeSize * player.SpecialMeterFill / 100.0), (int)(specialBadgeSize * player.SpecialMeterFill / 100.0)), Color.White);
                        ScarletFontDrawer.DrawString(spriteBatch, "special", specialBadgePos + (specialBadgeSize / 2), specialBadgePos + (specialBadgeSize / 2), (specialBadgeSize / 2), (specialBadgeSize / 8), TextAlignment.BottomCenter, false, Color.White, Color.White);
                        ScarletFontDrawer.DrawString(spriteBatch, player.SpecialMeterFill + "%", specialBadgePos + (specialBadgeSize / 2), specialBadgePos + (specialBadgeSize / 2), (specialBadgeSize / 3), (specialBadgeSize / 6), TextAlignment.TopCenter, false, Color.White, Color.White);
                    }

                    //drawing objective specific hud elements like timers, enemy count, etc
                    switch (currentStage.CurrentObjective)
                    {
                        //normal goal just draws timer
                        case GameObjective.GetToEnd:
                            ScarletFontDrawer.DrawTimeScaled(spriteBatch, currentStage.TimeSpentOnStage, 1500, 20, 70, TextAlignment.TopLeft, true, Color.Yellow, ScarletColor);
                            break;

                            //draw timer and amount of enemies defeated
                        case GameObjective.KillAllEnemies:
                            ScarletFontDrawer.DrawString(spriteBatch, "enemies: " + currentStage.EnemyObjectiveString, 600, 20, 600, 70, TextAlignment.TopLeft, true, Color.Magenta, Color.Magenta);
                            ScarletFontDrawer.DrawTimeScaled(spriteBatch, currentStage.TimeSpentOnStage, 1500, 20, 70, TextAlignment.TopLeft, true, Color.Yellow, ScarletColor);
                            break;

                            //draw timer and amount of civilians saved
                        case GameObjective.CollectAllObjectives:
                            ScarletFontDrawer.DrawString(spriteBatch, "civilians: " + currentStage.CivilianObjectiveString, 600, 20, 600, 70, TextAlignment.TopLeft, true, Color.Lime, Color.Green);
                            ScarletFontDrawer.DrawTimeScaled(spriteBatch, currentStage.TimeSpentOnStage, 1500, 20, 70, TextAlignment.TopLeft, true, Color.Yellow, ScarletColor);
                            break;

                            //draw how much time is left
                        case GameObjective.GetToEndUnderTimeLimit:

                            //if less than 10 seconds are left, make the timer blink!
                            if (currentStage.TimeLeft < 10)
                            {
                                timeLeftBlinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
                                if(timeLeftBlinkTimer > .18)
                                {
                                    timeLeftBlinkTimer = 0;
                                }
                                else
                                {
                                    ScarletFontDrawer.DrawStringScaled(spriteBatch, "time left: ", 1400, 20, 70, TextAlignment.TopRight, true, Color.White, Color.White);
                                    ScarletFontDrawer.DrawTimeScaled(spriteBatch, currentStage.TimeLeft, 1400, 20, 95, TextAlignment.TopLeft, true, Color.Yellow, ScarletColor);
                                }
                            }
                            //other than that, just draw it normally
                            else
                            {
                                ScarletFontDrawer.DrawStringScaled(spriteBatch, "time left: ", 1500, 20, 70, TextAlignment.TopRight, true, Color.White, Color.White);
                                ScarletFontDrawer.DrawTimeScaled(spriteBatch, currentStage.TimeLeft, 1500, 20, 70, TextAlignment.TopLeft, true, Color.Yellow, ScarletColor);
                            }

                            break;
                    }


                    //drawing the objective instructions display, it tells you what objective needs to get done
                    if (objectiveDisplayWipeTimer >= 0)
                    {
                        string objectiveDisplay = "";
                        Color objectiveDisplayColor = Color.White;
                        switch (currentStage.CurrentObjective)
                        {
                            case GameObjective.GetToEnd:
                                objectiveDisplay = "Get to the goal!";
                                break;

                            case GameObjective.GetToEndUnderTimeLimit:
                                objectiveDisplay = "Speedrun to goal!!";
                                objectiveDisplayColor = Color.Yellow;
                                break;

                            case GameObjective.KillAllEnemies:
                                objectiveDisplay = "Defeat all enemies!";
                                objectiveDisplayColor = Color.Magenta;
                                break;

                            case GameObjective.CollectAllObjectives:
                                objectiveDisplay = "Save all civilians!";
                                objectiveDisplayColor = Color.Lime;
                                break;
                        }

                        //instructions wiping in from left
                        if(objectiveDisplayWipeTimer > 2.5)
                        {
                            ScarletFontDrawer.DrawString(spriteBatch, objectiveDisplay, (RenderTargetWidth/2) - (int)((RenderTargetWidth) * (objectiveDisplayWipeTimer - 2.5) / .5), RenderTargetHeight/2, 1600, 100, TextAlignment.MiddleCenter, true, objectiveDisplayColor, objectiveDisplayColor);
                        }
                        //staying in the center for a bit
                        else if(objectiveDisplayWipeTimer > .5)
                        {
                            ScarletFontDrawer.DrawString(spriteBatch, objectiveDisplay, RenderTargetWidth / 2, RenderTargetHeight/2, 1600, 100, TextAlignment.MiddleCenter, true, objectiveDisplayColor, objectiveDisplayColor);
                        }
                        //then wiping out to the right
                        else
                        {
                            ScarletFontDrawer.DrawString(spriteBatch, objectiveDisplay, (RenderTargetWidth/2) + (int)((RenderTargetWidth) * (.5 - objectiveDisplayWipeTimer) / .5), RenderTargetHeight/2, 1600, 100, TextAlignment.MiddleCenter, true, objectiveDisplayColor, objectiveDisplayColor);
                        }
                    }


                    //drawing the UI elements for the special move cutscene
                    if (currentStage.SpecialMoveCutsceneTime > 0)
                    {
                        int yPos = -100;
                        int width = 1600;
                        int height = 1200;
                        //draw the special move HUD elements coming in from the right
                        if (currentStage.SpecialMoveCutsceneTime > .7)
                        {
                            spriteBatch.Draw(uiTextures[0], new Rectangle((int)(-((currentStage.SpecialMoveCutsceneTime - .7) / .1) * width), yPos, width, height), Color.White);
                            if (player is Scarlet)
                                spriteBatch.Draw(uiTextures[2], new Rectangle((int)(-((currentStage.SpecialMoveCutsceneTime - .7) / .1) * width) + rng.Next(-17, 17), yPos + rng.Next(-17, 17), width, height), Color.White);
                            else
                                spriteBatch.Draw(uiTextures[3], new Rectangle((int)(-((currentStage.SpecialMoveCutsceneTime - .7) / .1) * width) + rng.Next(-17, 17), yPos + rng.Next(-17, 17), width, height), Color.White);
                            spriteBatch.Draw(uiTextures[1], new Rectangle((int)(-((currentStage.SpecialMoveCutsceneTime - .7) / .1) * width), yPos, width, height), Color.White);
                        }
                        //draw the special move HUD elements still
                        else if (currentStage.SpecialMoveCutsceneTime > .1)
                        {
                            spriteBatch.Draw(uiTextures[0], new Rectangle(0, yPos, width, height), Color.White);
                            if (player is Scarlet)
                                spriteBatch.Draw(uiTextures[2], new Rectangle(rng.Next(-17, 17), yPos + rng.Next(-17, 17), width, height), Color.White);
                            else
                                spriteBatch.Draw(uiTextures[3], new Rectangle(rng.Next(-17, 17), yPos + rng.Next(-17, 17), width, height), Color.White);
                            spriteBatch.Draw(uiTextures[1], new Rectangle(0, yPos, width, height), Color.White);
                        }
                        //draw the special move HUD elements going back off the screen to the right
                        else
                        {
                            spriteBatch.Draw(uiTextures[0], new Rectangle(-width + (int)((currentStage.SpecialMoveCutsceneTime / .1) * width), yPos, width, height), Color.White);
                            if (player is Scarlet)
                                spriteBatch.Draw(uiTextures[2], new Rectangle(-width + (int)((currentStage.SpecialMoveCutsceneTime / .1) * width) + rng.Next(-17, 17), yPos + rng.Next(-17, 17), width, height), Color.White);
                            else
                                spriteBatch.Draw(uiTextures[3], new Rectangle(-width + (int)((currentStage.SpecialMoveCutsceneTime / .1) * width) + rng.Next(-17, 17), yPos + rng.Next(-17, 17), width, height), Color.White);
                            spriteBatch.Draw(uiTextures[1], new Rectangle(-width + (int)((currentStage.SpecialMoveCutsceneTime / .1) * width), yPos, width, height), Color.White);
                        }
                    }

                    #endregion

                    break;
                case GameState.Pause:
                    //draw all the objects in the stage
                    currentStage.Draw(spriteBatch, cameraOffset);

                    //draw our pause menu
                    menuManager.DrawPauseMenu(spriteBatch);
                    break;

                case GameState.Victory:
                    //draw all the objects in the stage
                    currentStage.Draw(spriteBatch, cameraOffset);

                    spriteBatch.Draw(uiTextures[10], new Rectangle(0, 0, RenderTargetWidth, RenderTargetHeight), Color.White);
                    spriteBatch.Draw(uiTextures[14], new Rectangle(0, 0, RenderTargetWidth, RenderTargetHeight), Color.White);

                    //elements wipe in from the side of the screen
                    if (victoryScreenAnimationTimer > 0)
                    {
                        //player portrait comes in from left
                        if (player is Scarlet)
                            spriteBatch.Draw(uiTextures[12], new Vector2(-RenderTargetWidth + (int)(RenderTargetWidth * (.2 - victoryScreenAnimationTimer) / .2), 0), Color.White);
                        else if (player is Cocoa)
                            spriteBatch.Draw(uiTextures[13], new Vector2(-RenderTargetWidth + (int)(RenderTargetWidth * (.2 - victoryScreenAnimationTimer) / .2), 0), Color.White);

                        //"yeehaw" text comes in from the right
                        spriteBatch.Draw(uiTextures[11], new Vector2(RenderTargetWidth + uiTextures[11].Width - (int)(RenderTargetWidth * (.2 - victoryScreenAnimationTimer) / .2), 0), Color.White);
                    }
                    else
                    {
                        //these elements just stay still afterwards
                        if (player is Scarlet)
                            spriteBatch.Draw(uiTextures[12], Vector2.Zero, Color.White);
                        else if (player is Cocoa)
                            spriteBatch.Draw(uiTextures[13], Vector2.Zero, Color.White);

                        spriteBatch.Draw(uiTextures[11], new Vector2(RenderTargetWidth - uiTextures[11].Width, 0), Color.White);

                        if(currentStage.CurrentObjective == GameObjective.GetToEndUnderTimeLimit)
                        {
                            //we also tell the user how much time they spent on the level...
                            ScarletFontDrawer.DrawStringScaled(spriteBatch, " time left", 1300, 600, 60, TextAlignment.BottomCenter, true, Color.White, Color.White);
                            ScarletFontDrawer.DrawTimeScaled(spriteBatch, currentStage.TimeLeft, 1300, 610, 70, TextAlignment.TopCenter, true, Color.Yellow, ScarletColor);
                        }
                        else
                        {
                            //we also tell the user how much time they spent on the level...
                            ScarletFontDrawer.DrawStringScaled(spriteBatch, " time spent", 1300, 600, 60, TextAlignment.BottomCenter, true, Color.White, Color.White);
                            ScarletFontDrawer.DrawTimeScaled(spriteBatch, currentStage.TimeSpentOnStage, 1300, 610, 70, TextAlignment.TopCenter, true, Color.Yellow, ScarletColor);
                        }

                        if (currentStage.Pacifist && currentStage.MainPlayer.NoDamage)
                        {
                            ScarletFontDrawer.DrawStringScaled(spriteBatch, "pacifist, no damage!", 1300, 700, 70, TextAlignment.TopCenter, true, Color.White, Color.White);
                        }
                        else if (currentStage.Pacifist)
                        {
                            ScarletFontDrawer.DrawStringScaled(spriteBatch, "pacifist!", 1300, 700, 70, TextAlignment.TopCenter, true, Color.White, Color.White);
                        }
                        else if (currentStage.Vigilante && currentStage.MainPlayer.NoDamage)
                        {
                            ScarletFontDrawer.DrawStringScaled(spriteBatch, "vigilante, no damage!", 1300, 700, 70, TextAlignment.TopCenter, true, Color.White, Color.White);
                        }
                        else if (currentStage.Vigilante)
                        {
                            ScarletFontDrawer.DrawStringScaled(spriteBatch, "vigilante!", 1300, 700, 70, TextAlignment.TopCenter, true, Color.White, Color.White);
                        }
                        else if (currentStage.MainPlayer.NoDamage)
                        {
                            ScarletFontDrawer.DrawStringScaled(spriteBatch, "no damage!", 1300, 700, 70, TextAlignment.TopCenter, true, Color.White, Color.White);
                        }



                        //...and what to do now
                        DrawPostGameInstructions(new Point(1300, 900));
                    }

                    break;

                case GameState.GameOver:
                    //draw all the objects in the stage
                    currentStage.Draw(spriteBatch, cameraOffset);

                    spriteBatch.Draw(uiTextures[10], new Rectangle(0, 0, RenderTargetWidth, RenderTargetHeight), Color.White);
                    spriteBatch.Draw(uiTextures[14], new Rectangle(0, 0, RenderTargetWidth, RenderTargetHeight), Color.White);
                    ScarletFontDrawer.DrawStringScaled(spriteBatch, "\"this vigilante is no more...\"", RenderTargetWidth / 2, 200, 60, TextAlignment.BottomCenter, true, Color.Yellow, ScarletColor);

                    //we wait a bit before showing instructions and how the player died.
                    if (gameOverScreenAnimationTimer <= 0)
                    {
                        ScarletFontDrawer.DrawStringScaled(spriteBatch, loseMessage, RenderTargetWidth / 2, 370, 60, TextAlignment.BottomCenter, true, Color.Yellow, ScarletColor);

                        #region DRAWING COMPLETION BAR
                        int completionBarHeight = 550;

                        spriteBatch.Draw(uiTextures[21], new Vector2(RenderTargetWidth / 2 - uiTextures[20].Width / 2, completionBarHeight),
                            new Rectangle(0, 0, (int)(uiTextures[21].Width * stageCompletionPercentage), uiTextures[21].Height), Color.White);
                        spriteBatch.Draw(uiTextures[20], new Vector2(RenderTargetWidth / 2 - uiTextures[20].Width / 2, completionBarHeight), Color.White);

                        int iconSize = 70;
                        int iconOffsetY = 15;

                        if (player is Scarlet)
                        {
                            spriteBatch.Draw(uiTextures[22], new Rectangle((RenderTargetWidth / 2 - uiTextures[20].Width / 2) + (int)(uiTextures[21].Width * stageCompletionPercentage) - iconSize / 2,
                                completionBarHeight - iconSize + iconOffsetY, iconSize, iconSize), Color.White);
                        }
                        else if (player is Cocoa)
                        {
                            spriteBatch.Draw(uiTextures[23], new Rectangle((RenderTargetWidth / 2 - uiTextures[20].Width / 2) + (int)(uiTextures[21].Width * stageCompletionPercentage) - iconSize / 2,
                                completionBarHeight - iconSize + iconOffsetY, iconSize, iconSize), Color.White);
                        }

                        ScarletFontDrawer.DrawStringScaled(spriteBatch, encouragementMessages[chosenEncouragementMessageIndex], RenderTargetWidth / 2, 630, 40, TextAlignment.TopCenter, true, Color.White, Color.White);

                        ScarletFontDrawer.DrawStringScaled(spriteBatch, ((int)(stageCompletionPercentage * 100)).ToString() + "%", (RenderTargetWidth / 2 - uiTextures[20].Width / 2) + (int)(uiTextures[21].Width * stageCompletionPercentage),
                            completionBarHeight - iconSize + iconOffsetY, 40, TextAlignment.BottomCenter, true, Color.White, Color.White);

                        #endregion

                        DrawPostGameInstructions(new Point(RenderTargetWidth / 2, 850));
                    }
                    break;
                default:
                    currentGameState = GameState.Menu;
                    break;
            }

            //if a screen wipe entry animation is playing, then draw it coming in from the left
            if (screenWipeEntryTimer >= 0)
            {
                spriteBatch.Draw(uiTextures[8], new Rectangle(-RenderTargetWidth - 480 + (int)((RenderTargetWidth + 240) * (screenWipeTimeMax - screenWipeEntryTimer) / screenWipeTimeMax), 0, RenderTargetWidth + 480, RenderTargetHeight), Color.White);
            }

            //if a screen wipe exit animation is playing, then draw it going out to the right
            if (screenWipeExitTimer >= 0)
            {
                spriteBatch.Draw(uiTextures[8], new Rectangle((int)((RenderTargetWidth + 240) * (screenWipeTimeMax - screenWipeExitTimer) / screenWipeTimeMax) - 240, 0, RenderTargetWidth + 480, RenderTargetHeight), Color.White);
            }

            spriteBatch.End();


            //we are now drawing onto the window/actual screen
            GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin();

            //draw our final image stretched to the entire window!
            spriteBatch.Draw(currentRenderTarget, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// When the user selects a stage from the list, this is called. 
        /// Triggers the screen wipe animation which starts the loading sequence once done.
        /// </summary>
        /// <param name="stageIndex">The index/ID of the stage that the user selected (most likely the index of the button they selected)</param>
        /// <param name="characterId">The id of the character the player chose to play as</param>
        public void SelectStage(int stageIndex, int characterId)
        {
            screenWipeEntryTimer = screenWipeTimeMax;
            screenWipeAction = ScreenWipeAction.LoadStage;
            selectedStageIndex = stageIndex;
            selectedCharacterId = characterId;
        }

        /// <summary>
        /// Draws the instuctions "A/Enter - Replay, B/Esc - Main Menu"
        /// </summary>
        /// <param name="point">the center point to draw these instructions to</param>
        private void DrawPostGameInstructions(Point point)
        {
            ScarletFontDrawer.DrawStringScaled(spriteBatch, "a / enter - replay", point.X, point.Y, 70, TextAlignment.BottomCenter, true, Color.White, Color.White);
            ScarletFontDrawer.DrawStringScaled(spriteBatch, "b / esc - main menu", point.X, point.Y + 10, 70, TextAlignment.TopCenter, true, Color.White, Color.White);
        }

        /// <summary>
        /// Randomly picks from all backgrounds to render for the background of the menu/title screen
        /// </summary>
        public void SetTitleStageBackground()
        {
            titleStage.BackgroundLayers = allBackgrounds[rng.Next(0, allBackgrounds.Length)];
        }

        /// <summary>
        /// When the player completes the stage, this triggers the Victory Screen animation to play
        /// </summary>
        public static void CompleteStage()
        {
            victoryScreenAnimationTimer = .2;
            currentGameState = GameState.Victory;
        }

        /// <summary>
        /// When the player selects to quit the stage and return back to the menu (either from the pause menu, victory screen, or defeat screen)...
        /// Call this method so it plays an animation when going into the menu
        /// </summary>
        public void QuitStage()
        {
            screenWipeEntryTimer = screenWipeTimeMax;
            screenWipeAction = ScreenWipeAction.ReturnToMenu;

            //also randomize the background for the menu again cuz thats fun
            SetTitleStageBackground();
        }

        /// <summary>
        /// When the player dies or runs out of time, call this with a reason why they lost
        /// </summary>
        /// <param name="message">the reason why they lost</param>
        public static void LoseStage(string message)
        {
            gameOverScreenAnimationTimer = .8;
            currentGameState = GameState.GameOver;

            //if the player lost, the completion percentage could never be over 99%, so cap it at that
            stageCompletionPercentage = Math.Min(currentStage.GetCompletionPercentage(currentStage.StartingObjective), .99);

            loseMessage = message;
            chosenEncouragementMessageIndex = rng.Next(0, encouragementMessages.Length);
        }

        /// <summary>
        /// Once we have a list of platforms, entity representatives, and prop layers, then we can
        /// finally set up the stage for playing! This method spawns/resets all entities on a stage
        /// </summary>
        /// <param name="platforms">the platform GameObjects that make up the stage</param>
        /// <param name="entities">List of MapObjectRepresentatives that represent the entities placed in the map</param>
        /// <param name="propLayers">the layers of placed props in the map</param>
        /// <param name="mapSize">the overall size of the map</param>
        public void LoadStage(List<GameObject> platforms, List<MapObjectRepresentative> entities, List<Prop>[] propLayers, Rectangle mapTransform)
        {
            //store stage data for quick reloading/resetting of the current stage
            currentStagePlatforms = platforms;
            currentStageEntityReps = entities;
            currentStagePropLayers = propLayers;
            currentStageTransform = mapTransform;

            currentStage = new Stage(platforms, propLayers, eveningBackgroundLayers, mapTransform);

            //depending on what character the player selected, spawn them in as that character
            if(selectedCharacterId == 0)
                player = new Scarlet(0, 0, 80, 160, scarletAnims, 2, currentStage);
            else if(selectedCharacterId == 1)
                player = new Cocoa(0, 0, 80, 160, cocoaAnims, 2, currentStage);

            foreach (MapObjectRepresentative rep in entities)
            {
                switch (int.Parse(rep.ID.Substring(1)))
                {
                    //0 is player spawn
                    case (0):
                        player.X = rep.Position.X;
                        player.Y = rep.Position.Y;
                        currentStage.PlayerSpawn = rep.Position;
                        break;

                    //1 is front facing trampoline
                    case (1):
                        currentStage.AddEntity(new Trampoline(rep.Position.X, rep.Position.Y, Direction.Front));
                        break;

                    //2 is left facing trampoline
                    case (2):
                        currentStage.AddEntity(new Trampoline(rep.Position.X, rep.Position.Y, Direction.West));
                        break;

                    //3 is right facing trampoline
                    case (3):
                        currentStage.AddEntity(new Trampoline(rep.Position.X, rep.Position.Y, Direction.East));
                        break;

                    //4 is left going short zip line
                    case (4):
                        Zipline ziplineToAddLeftShort = new Zipline(rep.Position, rep.Position + new Point(0, 4 * Scale), rep.Position + new Point(12 * Scale, 0),
                            new Point(12 * Scale, 4 * Scale), true, miscTextures[3], currentStage);
                        //looking through already added ziplines to see if they can be strung together
                        foreach (GameObject gameObject in currentStage.EntitiesToAdd)
                        {
                            if (gameObject is Zipline)
                            {
                                //if a previously placed zipline's left end matches this new zipline's right end
                                if (((Zipline)gameObject).LeftPoint == ziplineToAddLeftShort.RightPoint)
                                {
                                    //then make the previously placed one's "next zipline" be the new zipline
                                    ((Zipline)gameObject).NextZipLine = ziplineToAddLeftShort;
                                    break;
                                }

                                //if a previously placed zipline's right end matches this new zipline's left end
                                if (((Zipline)gameObject).RightPoint == ziplineToAddLeftShort.LeftPoint)
                                {
                                    //then make the new zipline's "next zipline" be the previously placed one
                                    ziplineToAddLeftShort.NextZipLine = ((Zipline)gameObject);
                                    break;
                                }
                            }
                        }
                        currentStage.AddEntity(ziplineToAddLeftShort);
                        break;

                    //5 is right going short zip line
                    case (5):
                        Zipline ziplineToAddRightShort = new Zipline(rep.Position, rep.Position, rep.Position + new Point(12 * Scale, 4 * Scale),
                            new Point(12 * Scale, 4 * Scale), false, miscTextures[4], currentStage);
                        //looking through already added ziplines to see if they can be strung together
                        foreach (GameObject gameObject in currentStage.EntitiesToAdd)
                        {
                            if(gameObject is Zipline)
                            {
                                //if a previously placed zipline's right end matches this new zipline's left end
                                if (((Zipline)gameObject).RightPoint == ziplineToAddRightShort.LeftPoint)
                                {
                                    //then make the previously placed one's "next zipline" be the new zipline
                                    ((Zipline)gameObject).NextZipLine = ziplineToAddRightShort;
                                    break;
                                }

                                //if a previously placed zipline's left end matches this new zipline's right end
                                if (((Zipline)gameObject).LeftPoint == ziplineToAddRightShort.RightPoint)
                                {
                                    //then make the new zipline's "next zipline" be the previously placed one
                                    ziplineToAddRightShort.NextZipLine = ((Zipline)gameObject);
                                    break;
                                }
                            }
                        }
                        currentStage.AddEntity(ziplineToAddRightShort);
                        break;

                    //6 is Lawmaker enemy (Grabber) facing right
                    case (6):
                        Enemy grabberRight = new Grabber(rep.Position.X, rep.Position.Y, Animation.GetCopy(lawmakerAnims), Animation.GetCopy(lawmakerAttackAnims), currentStage, false);
                        //check for any items in the stage to add to this enemy
                        AddItemsToEnemy(grabberRight);
                        currentStage.AddEntity(grabberRight);
                        break;

                    //7 is Lawmaker enemy (Grabber) facing left
                    case (7):
                        Enemy grabberLeft = new Grabber(rep.Position.X, rep.Position.Y, Animation.GetCopy(lawmakerAnims), Animation.GetCopy(lawmakerAttackAnims), currentStage, true);
                        //check for any items in the stage to add to this enemy
                        AddItemsToEnemy(grabberLeft);
                        currentStage.AddEntity(grabberLeft);
                        break;

                    //8 is Robber Baron enemy facing right
                    case (8):
                        Enemy robberRight = new RobberBaron(rep.Position.X, rep.Position.Y, Animation.GetCopy(robberBaronAnims), currentStage, false);
                        //check for any items in the stage to add to this enemy
                        AddItemsToEnemy(robberRight);
                        currentStage.AddEntity(robberRight);
                        break;

                    //9 is Robber Baron enemy facing left
                    case (9):
                        Enemy robberLeft = new RobberBaron(rep.Position.X, rep.Position.Y, Animation.GetCopy(robberBaronAnims), currentStage, true);
                        //check for any items in the stage to add to this enemy
                        AddItemsToEnemy(robberLeft);
                        currentStage.AddEntity(robberLeft);
                        break;

                    //10 is Cactus enemy facing right
                    case (10):
                        Enemy cactusRight = new Cactus(rep.Position.X, rep.Position.Y, Animation.GetCopy(cactusAnims), currentStage, false);
                        //check for any items in the stage to add to this enemy
                        AddItemsToEnemy(cactusRight);
                        currentStage.AddEntity(cactusRight);
                        break;

                    //11 is Cactus enemy facing left
                    case (11):
                        Enemy cactusLeft = new Cactus(rep.Position.X, rep.Position.Y, Animation.GetCopy(cactusAnims), currentStage, true);
                        //check for any items in the stage to add to this enemy
                        AddItemsToEnemy(cactusLeft);
                        currentStage.AddEntity(cactusLeft);
                        break;

                    //12 is Tumble weed enemy facing right
                    case (12):
                        Enemy tumbleRight = new TumbleWeed(rep.Position.X, rep.Position.Y, Animation.GetCopy(tumbleWeedAnims), currentStage, false);
                        //check for any items in the stage to add to this enemy
                        AddItemsToEnemy(tumbleRight);
                        currentStage.AddEntity(tumbleRight);
                        break;

                    //13 is Tumble weed enemy facing left
                    case (13):
                        Enemy tumbleLeft = new TumbleWeed(rep.Position.X, rep.Position.Y, Animation.GetCopy(tumbleWeedAnims), currentStage, true);
                        //check for any items in the stage to add to this enemy
                        AddItemsToEnemy(tumbleLeft);
                        currentStage.AddEntity(tumbleLeft);
                        break;

                    //14 is adding a happy skull item
                    case (14):
                        //check if this item is inside an enemy
                        Enemy enemyToPutThisItemIn = CheckIfPointInEnemy(rep.Position);
                        //if so, add the item to the enemy
                        if (enemyToPutThisItemIn != null)
                            enemyToPutThisItemIn.AddItem(new Collectible(rep.Position.X, rep.Position.Y, Scale, Scale, MiscTextures[8], currentStage,
                                ItemType.HappySkull, true, new Vector2(rng.Next(-18, 18), rng.Next(-18, -5))));
                        //if not, add the item to the stage as normal
                        else
                            currentStage.AddEntity(new Collectible(rep.Position.X, rep.Position.Y, Scale, Scale, MiscTextures[8], currentStage,
                                ItemType.HappySkull, false, Vector2.Zero));
                        break;

                    //15 is adding a shocked skull item
                    case (15):
                        //check if this item is inside an enemy
                        enemyToPutThisItemIn = CheckIfPointInEnemy(rep.Position);
                        //if so, add the item to the enemy
                        if (enemyToPutThisItemIn != null)
                            enemyToPutThisItemIn.AddItem(new Collectible(rep.Position.X, rep.Position.Y, 2 * Scale, 2 * Scale, MiscTextures[9], currentStage,
                                ItemType.ShockedSkull, true, new Vector2(rng.Next(-18, 18), rng.Next(-18, -5))));
                        //if not, add the item to the stage as normal
                        else
                            currentStage.AddEntity(new Collectible(rep.Position.X, rep.Position.Y, 2 * Scale, 2 * Scale, MiscTextures[9], currentStage,
                                ItemType.ShockedSkull, false, Vector2.Zero));
                        break;

                    //16 is adding a luchador mask item
                    case (16):
                        //check if this item is inside an enemy
                        enemyToPutThisItemIn = CheckIfPointInEnemy(rep.Position);
                        //if so, add the item to the enemy
                        if (enemyToPutThisItemIn != null)
                            enemyToPutThisItemIn.AddItem(new Collectible(rep.Position.X, rep.Position.Y, 2 * Scale, 2 * Scale, MiscTextures[10], currentStage,
                                ItemType.LuchadorMask, true, new Vector2(rng.Next(-18, 18), rng.Next(-18, -5))));
                        //if not, add the item to the stage as normal
                        else
                            currentStage.AddEntity(new Collectible(rep.Position.X, rep.Position.Y, 2 * Scale, 2 * Scale, MiscTextures[10], currentStage,
                                ItemType.LuchadorMask, false, Vector2.Zero));
                        break;

                    //17 is adding a jalapeno item
                    case (17):
                        //check if this item is inside an enemy
                        enemyToPutThisItemIn = CheckIfPointInEnemy(rep.Position);
                        //if so, add the item to the enemy
                        if (enemyToPutThisItemIn != null)
                            enemyToPutThisItemIn.AddItem(new Collectible(rep.Position.X, rep.Position.Y, 2 * Scale, 2 * Scale, MiscTextures[11], currentStage,
                                ItemType.Jalapeno, true, new Vector2(rng.Next(-18, 18), rng.Next(-18, -5))));
                        //if not, add the item to the stage as normal
                        else
                            currentStage.AddEntity(new Collectible(rep.Position.X, rep.Position.Y, 2 * Scale, 2 * Scale, MiscTextures[11], currentStage,
                                ItemType.Jalapeno, false, Vector2.Zero));
                        break;

                    //18 is adding a badge bit big item
                    case (18):
                        //check if this item is inside an enemy
                        enemyToPutThisItemIn = CheckIfPointInEnemy(rep.Position);
                        //if so, add the item to the enemy
                        if (enemyToPutThisItemIn != null)
                            enemyToPutThisItemIn.AddItem(new Collectible(rep.Position.X, rep.Position.Y, Scale, 2 * Scale, MiscTextures[12], currentStage,
                                ItemType.BadgeBitBig, true, new Vector2(rng.Next(-18, 18), rng.Next(-18, -5))));
                        //if not, add the item to the stage as normal
                        else
                            currentStage.AddEntity(new Collectible(rep.Position.X, rep.Position.Y, Scale, 2 * Scale, MiscTextures[12], currentStage,
                                ItemType.BadgeBitBig, false, Vector2.Zero));
                        break;

                    //19 is adding a badge bit small item
                    case (19):
                        //check if this item is inside an enemy
                        enemyToPutThisItemIn = CheckIfPointInEnemy(rep.Position);
                        //if so, add the item to the enemy
                        if (enemyToPutThisItemIn != null)
                            enemyToPutThisItemIn.AddItem(new Collectible(rep.Position.X, rep.Position.Y, Scale, Scale, MiscTextures[13], currentStage,
                                ItemType.BadgeBitSmall, true, new Vector2(rng.Next(-18, 18), rng.Next(-18, -5))));
                        //if not, add the item to the stage as normal
                        else
                            currentStage.AddEntity(new Collectible(rep.Position.X, rep.Position.Y, Scale, Scale, MiscTextures[13], currentStage,
                                ItemType.BadgeBitSmall, false, Vector2.Zero));
                        break;

                    //20 is the normal "Get to the end" objective goal
                    case (20):
                        currentStage.AddEntity(new Goal(rep.Position.X, rep.Position.Y, currentStage, GameObjective.GetToEnd));
                        break;

                    //21 is the Get to the end under the time limit objective goal
                    case (21):
                        currentStage.AddEntity(new Goal(rep.Position.X, rep.Position.Y, currentStage, GameObjective.GetToEndUnderTimeLimit));
                        break;

                    //22 is the kill all enemies objective goal
                    case (22):
                        currentStage.AddEntity(new Goal(rep.Position.X, rep.Position.Y, currentStage, GameObjective.KillAllEnemies));
                        break;

                    //23 is the save all civilians objective goal
                    case (23):
                        currentStage.AddEntity(new Goal(rep.Position.X, rep.Position.Y, currentStage, GameObjective.CollectAllObjectives));
                        break;

                    //24 adds 5 seconds to the time limit, lets us customize how much time is on each stage with a time limit objective
                    case (24):
                        currentStage.IncreaseTimeLimit(5);
                        break;

                    //25 adds a female civilian! (would do a male one but dont have much time to do their art)
                    case (25):
                        currentStage.AddEntity(new Civilian(rep.Position.X, rep.Position.Y, MiscTextures[22], currentStage));
                        break;

                    //26 sets background to day time
                    case (26):
                        currentStage.BackgroundLayers = dayBackgroundLayers;
                        break;

                    //27 sets background to evening time
                    case (27):
                        currentStage.BackgroundLayers = eveningBackgroundLayers;
                        break;

                    //28 sets background to night time
                    case (28):
                        currentStage.BackgroundLayers = nightBackgroundLayers;
                        break;

                    //29 sets background to morning time
                    case (29):
                        currentStage.BackgroundLayers = morningBackgroundLayers;
                        break;

                }
            }

            //initializing our starting camera offset
            cameraOffset.X = player.Transform.Center.X - RenderTargetWidth / 2;
            cameraOffset.Y = player.Transform.Center.Y - RenderTargetHeight / 2;

            //finally we add the player and start the game!
            currentStage.AddEntity(player);
            currentStage.MainPlayer = player;
            if(postLoadTimer <= 0)
            {
                currentGameState = GameState.Play;
                screenWipeExitTimer = screenWipeTimeMax;
            }

        }

        /// <summary>
        /// Checks if a point is in an enemy. This is used to add items to enemies so they drop them when killed
        /// </summary>
        /// <param name="point">the location of the item</param>
        /// <returns>Returns the enemy if one is found with the point inside them. Returns null if none were found</returns>
        private Enemy CheckIfPointInEnemy(Point point)
        {
            //loop through all enemies in the stage
            foreach(GameObject gameObject in currentStage.EntitiesToAdd)
            {
                //if the point is inside the enemy
                if(gameObject is Enemy && gameObject.Transform.Contains(point))
                {
                    //return it
                    return ((Enemy)gameObject);
                }
            }

            //Returns null if no enemies contained the point
            return null;
        }

        /// <summary>
        /// Used when placing an enemy object, checks if there are any items inside it to add to the enemy
        /// </summary>
        /// <param name="enemy">the enemy to add items to</param>
        private void AddItemsToEnemy(Enemy enemy)
        {
            //loop through all collectible items in the stage
            foreach (GameObject gameObject in currentStage.EntitiesToAdd)
            {
                //if the item's location is inside the enemy
                if (gameObject is Collectible && enemy.Transform.Contains(gameObject.PositionVector))
                {
                    //add the item to the enemy
                    enemy.AddItem((Collectible)gameObject);
                    //remove the item from the stage
                    currentStage.RemoveEntity(gameObject);
                }
            }
        }

        /// <summary>
        /// Restarting just initiates the screen wipe animation, which then reloads the level (See the Update method)
        /// </summary>
        public void RestartStage()
        {
            screenWipeEntryTimer = screenWipeTimeMax;
            screenWipeAction = ScreenWipeAction.LoadStage;
        }

        /// <summary>
        /// Determines if a certain action button is being pressed right now, regardless of if it was not pressed previously
        /// </summary>
        /// <param name="action">the action to check for (Use Action enum)</param>
        /// <returns>True if the button is being pressed down</returns>
        public static bool ActionPressed(Action action)
        {
            //just checking if each gamepad or keyboard has the button pressed for the requested action
            for (int i = 0; i < 4; i++)
            {
                switch (action)
                {
                    case (Action.MoveUp):
                        if (kbState.IsKeyDown(Keys.W) || gpStates[i].IsButtonDown(Buttons.DPadUp) || gpStates[i].IsButtonDown(Buttons.LeftThumbstickUp))
                            return true;
                        break;

                    case (Action.MoveDown):
                        if (kbState.IsKeyDown(Keys.S) || gpStates[i].IsButtonDown(Buttons.DPadDown) || gpStates[i].IsButtonDown(Buttons.LeftThumbstickDown))
                            return true;
                        break;

                    case (Action.MoveLeft):
                        if (kbState.IsKeyDown(Keys.A) || gpStates[i].IsButtonDown(Buttons.DPadLeft) || gpStates[i].IsButtonDown(Buttons.LeftThumbstickLeft))
                            return true;
                        break;

                    case (Action.MoveRight):
                        if (kbState.IsKeyDown(Keys.D) || gpStates[i].IsButtonDown(Buttons.DPadRight) || gpStates[i].IsButtonDown(Buttons.LeftThumbstickRight))
                            return true;
                        break;

                    case (Action.Jump):
                        if (kbState.IsKeyDown(Keys.Space) || gpStates[i].IsButtonDown(Buttons.A))
                            return true;
                        break;

                    case (Action.Accept):
                        if (kbState.IsKeyDown(Keys.Enter) || gpStates[i].IsButtonDown(Buttons.A))
                            return true;
                        break;

                    case (Action.Attack):
                        if (kbState.IsKeyDown(Keys.Left) || gpStates[i].IsButtonDown(Buttons.X))
                            return true;
                        break;

                    case (Action.Melee):
                        if (kbState.IsKeyDown(Keys.Right) || gpStates[i].IsButtonDown(Buttons.B))
                            return true;
                        break;

                    case (Action.Special):
                        if (kbState.IsKeyDown(Keys.Down) || gpStates[i].IsButtonDown(Buttons.Y))
                            return true;
                        break;

                    case (Action.Interact):
                        if (kbState.IsKeyDown(Keys.Up) || gpStates[i].IsButtonDown(Buttons.RightShoulder) || gpStates[i].IsButtonDown(Buttons.LeftShoulder))
                            return true;
                        break;

                    case (Action.Inventory):
                        if (kbState.IsKeyDown(Keys.E) || gpStates[i].IsButtonDown(Buttons.Back))
                            return true;
                        break;

                    case (Action.Pause):
                        if (kbState.IsKeyDown(Keys.Escape) || gpStates[i].IsButtonDown(Buttons.Start))
                            return true;
                        break;

                    case (Action.Back):
                        if (kbState.IsKeyDown(Keys.Escape) || gpStates[i].IsButtonDown(Buttons.B))
                            return true;
                        break;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if a certain action button is being pressed ONLY in the current frame
        /// </summary>
        /// <param name="action">the action to check for (Use Action enum)</param>
        /// <returns>True if the button is being pressed down</returns>
        public static bool ActionPressedThisFrame(Action action)
        {
            //just checking if each gamepad or keyboard has the button pressed ONLY ON THIS FRAME for the requested action
            for (int i = 0; i < 4; i++)
            {
                switch (action)
                {
                    case (Action.MoveUp):
                        if ((kbState.IsKeyDown(Keys.W) && previousKbState.IsKeyUp(Keys.W))
                             || (gpStates[i].IsButtonDown(Buttons.DPadUp) && previousGPStates[i].IsButtonUp(Buttons.DPadUp))
                             || (gpStates[i].IsButtonDown(Buttons.LeftThumbstickUp) && previousGPStates[i].IsButtonUp(Buttons.LeftThumbstickUp)))
                            return true;
                        break;

                    case (Action.MoveDown):
                        if ((kbState.IsKeyDown(Keys.S) && previousKbState.IsKeyUp(Keys.S))
                            || (gpStates[i].IsButtonDown(Buttons.DPadDown) && previousGPStates[i].IsButtonUp(Buttons.DPadDown))
                             || (gpStates[i].IsButtonDown(Buttons.LeftThumbstickDown) && previousGPStates[i].IsButtonUp(Buttons.LeftThumbstickDown)))
                            return true;
                        break;

                    case (Action.MoveLeft):
                        if (kbState.IsKeyDown(Keys.A) && previousKbState.IsKeyUp(Keys.A)
                            || (gpStates[i].IsButtonDown(Buttons.DPadLeft) && previousGPStates[i].IsButtonUp(Buttons.DPadLeft))
                             || (gpStates[i].IsButtonDown(Buttons.LeftThumbstickLeft) && previousGPStates[i].IsButtonUp(Buttons.LeftThumbstickLeft)))
                            return true;
                        break;

                    case (Action.MoveRight):
                        if ((kbState.IsKeyDown(Keys.D) && previousKbState.IsKeyUp(Keys.D))
                            || (gpStates[i].IsButtonDown(Buttons.DPadRight) && previousGPStates[i].IsButtonUp(Buttons.DPadRight))
                             || (gpStates[i].IsButtonDown(Buttons.LeftThumbstickRight) && previousGPStates[i].IsButtonUp(Buttons.LeftThumbstickRight)))
                            return true;
                        break;

                    case (Action.Jump):
                        if ((kbState.IsKeyDown(Keys.Space) && previousKbState.IsKeyUp(Keys.Space))
                            || (gpStates[i].IsButtonDown(Buttons.A) && previousGPStates[i].IsButtonUp(Buttons.A)))
                            return true;
                        break;

                    case (Action.Accept):
                        if ((kbState.IsKeyDown(Keys.Enter) && previousKbState.IsKeyUp(Keys.Enter))
                            || (gpStates[i].IsButtonDown(Buttons.A) && previousGPStates[i].IsButtonUp(Buttons.A)))
                            return true;
                        break;

                    case (Action.Attack):
                        if ((kbState.IsKeyDown(Keys.Left) && previousKbState.IsKeyUp(Keys.Left))
                            || (gpStates[i].IsButtonDown(Buttons.X) && previousGPStates[i].IsButtonUp(Buttons.X)))
                            return true;
                        break;

                    case (Action.Melee):
                        if ((kbState.IsKeyDown(Keys.Right) && previousKbState.IsKeyUp(Keys.Right))
                            || (gpStates[i].IsButtonDown(Buttons.B) && previousGPStates[i].IsButtonUp(Buttons.B)))
                            return true;
                        break;

                    case (Action.Special):
                        if ((kbState.IsKeyDown(Keys.Down) && previousKbState.IsKeyUp(Keys.Down))
                            || (gpStates[i].IsButtonDown(Buttons.Y) && previousGPStates[i].IsButtonUp(Buttons.Y)))
                            return true;
                        break;

                    case (Action.Interact):
                        if ((kbState.IsKeyDown(Keys.Up) && previousKbState.IsKeyUp(Keys.Up))
                            || (gpStates[i].IsButtonDown(Buttons.RightShoulder) && previousGPStates[i].IsButtonUp(Buttons.RightShoulder))
                             || (gpStates[i].IsButtonDown(Buttons.LeftShoulder) && previousGPStates[i].IsButtonUp(Buttons.LeftShoulder)))
                            return true;
                        break;

                    case (Action.Inventory):
                        if ((kbState.IsKeyDown(Keys.E) && previousKbState.IsKeyUp(Keys.E))
                            || (gpStates[i].IsButtonDown(Buttons.Back) && previousGPStates[i].IsButtonUp(Buttons.Back)))
                            return true;
                        break;

                    case (Action.Pause):
                        if ((kbState.IsKeyDown(Keys.Escape) && previousKbState.IsKeyUp(Keys.Escape))
                            || (gpStates[i].IsButtonDown(Buttons.Start) && previousGPStates[i].IsButtonUp(Buttons.Start)))
                            return true;
                        break;

                    case (Action.Back):
                        if ((kbState.IsKeyDown(Keys.Escape) && previousKbState.IsKeyUp(Keys.Escape))
                            || (gpStates[i].IsButtonDown(Buttons.B) && previousGPStates[i].IsButtonUp(Buttons.B)))
                            return true;
                        break;
                }
            }
            return false;
        }
    }
}
