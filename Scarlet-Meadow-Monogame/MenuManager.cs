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
//This handles all menu input, updating, and drawing by working in conjunction with Game1
namespace Scarlet_Meadow_Monogame
{
    /// <summary>
    /// The different states our menu can be in. Each one has a different number of buttons, what each button can do,
    /// what gets drawn for that part of the menu, and animations for transitions between the menus
    /// </summary>
    public enum MenuState
    {
        TitleAnimation,
        TitleScreen,
        MainMenu,
        CharacterSelect,
        StageSelect,
        HowToPlay
    }


    /// <summary>
    /// This handles all menu input, updating, and drawing by working in conjunction with Game1
    /// </summary>
    class MenuManager
    {
        /// <summary>
        /// The root game that is hosting this menu
        /// </summary>
        private Game1 gameBase;

        /// <summary>
        /// array of text buttons that the main menu has (used for drawing the basic buttons)
        /// </summary>
        private String[] mainMenuButtons = { "play game", "how to play", "quit" };

        /// <summary>
        /// array of text buttons that the pause menu has
        /// </summary>
        private String[] pauseMenuButtons = { "resume", "restart", "quit"};

        /// <summary>
        /// the index of the button that the player has selected in the PAUSE MENU
        /// </summary>
        private int pauseSelectionId;

        /// <summary>
        /// the index of the button that the player has selected
        /// </summary>
        private int selectionId;

        /// <summary>
        /// the highest selection id the player can go to
        /// </summary>
        private int selectionIdMax;

        /// <summary>
        /// the id of the character that the player has selected to play as 
        /// (0 - Scarlet, 1 - Cocoa)
        /// </summary>
        private int selectedCharacterId;

        /// <summary>
        /// the index of the stage that the player has chosen to play
        /// </summary>
        private int selectedStageIndex;

        /// <summary>
        /// The state of the menu the player is currently in
        /// </summary>
        private MenuState currentMenuState;

        /// <summary>
        /// The state of the menu the player is transitioning from
        /// </summary>
        private MenuState previousMenuState;

        /// <summary>
        /// Amount of time passed in the animation for the title logo
        /// </summary>
        private double titleAnimationTimer;

        /// <summary>
        /// Max amount of time for the animation for the title logo
        /// </summary>
        private const double titleAnimationTimerMax = 1.2;

        /// <summary>
        /// Amount of time until the text that says "press A to start" or something blinks
        /// </summary>
        private double titleInstructionsBlinkTimer;

        /// <summary>
        /// Maximum amount of time until these instructions blink
        /// </summary>
        private const double titleInstructionsBlinkTimerMax = .5;

        /// <summary>
        /// Amount of time passed during transitions between menus
        /// </summary>
        private double transitionTimer;

        /// <summary>
        /// Max amount of time between display menus, (the transition between 2 menus)
        /// </summary>
        private const double transitionTimerMax = .2;

        /// <summary>
        /// Amount of time until the user can press buttons again. Makes it so they cannot spam certain things
        /// </summary>
        private double inActionTimer;

        /// <summary>
        /// Total time allotted to not letting the player spam certain things
        /// </summary>
        private const double inActionTimerMax = .35;

        /// <summary>
        /// Amount of time passed during the how to play page turn transition
        /// </summary>
        private double howToPlayTransitionTimer;

        /// <summary>
        /// Amount of time between pages in the how to play guide
        /// </summary>
        private const double howToPlayTransitionTimerMax = .3;

        /// <summary>
        /// The page of the how to play guide the player is currently in
        /// </summary>
        private int currentHowToPlayPage;

        /// <summary>
        /// The page of the how to play guide the player is transitioning from
        /// </summary>
        private int previousHowToPlayPage;

        /// <summary>
        /// Set the selected button index for the pause menu
        /// </summary>
        public int PauseSelectionId
        {
            set { pauseSelectionId = value; }
        }

        /// <summary>
        /// Constructor simply stores a reference to our base Game1 class, and starts up our title screen animation
        /// </summary>
        /// <param name="gameBase">a reference to our base Game1 class</param>
        public MenuManager(Game1 gameBase)
        {
            this.gameBase = gameBase;

            currentMenuState = MenuState.TitleAnimation;
            transitionTimer = transitionTimerMax;
            howToPlayTransitionTimer = howToPlayTransitionTimerMax;
        }

        /// <summary>
        /// Lets the player change their selected button for each menu, as well as confirm the selection, and go back a menu
        /// </summary>
        /// <param name="gameTime">the time since last frame</param>
        public void Update(GameTime gameTime)
        {
            //if we are transitioning, count up on the timer and dont let the user control anything
            if(transitionTimer < transitionTimerMax)
            {
                transitionTimer += gameTime.ElapsedGameTime.TotalSeconds;
                return;
            }

            //stops the user from spamming shit, specifically when quitting stages
            if (inActionTimer < inActionTimerMax)
            {
                inActionTimer += gameTime.ElapsedGameTime.TotalSeconds;
                return;
            }

            switch (currentMenuState)
            {

                //in this state, just animate the title and when its done, go to the title screen
                case (MenuState.TitleAnimation):
                    if(titleAnimationTimer <= titleAnimationTimerMax)
                        titleAnimationTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    else
                        currentMenuState = MenuState.TitleScreen;
                    break;

                    //title screen just lets the user press a button to go to the main menu
                    //also there is some blinking text
                case (MenuState.TitleScreen):
                    titleInstructionsBlinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    if(Game1.ActionPressedThisFrame(Action.Accept) || Game1.ActionPressedThisFrame(Action.Jump))
                    {
                        previousMenuState = MenuState.TitleScreen;
                        currentMenuState = MenuState.MainMenu;
                        transitionTimer = 0;
                    }
                    break;

                    //Main menu lets the user select between 3 buttons...
                case (MenuState.MainMenu):
                    selectionIdMax = 2;
                    UpdateSelectionId();
                    //pressing back just shows the title again
                    if (Game1.ActionPressedThisFrame(Action.Back))
                    {
                        previousMenuState = MenuState.MainMenu;
                        currentMenuState = MenuState.TitleScreen;
                        transitionTimer = 0;
                        selectionId = 0;
                    }
                    else if (Game1.ActionPressedThisFrame(Action.Accept) || Game1.ActionPressedThisFrame(Action.Jump))
                    {
                        //Main menu lets the user select "play the game",
                        if (selectionId == 0)
                        {
                            previousMenuState = MenuState.MainMenu;
                            currentMenuState = MenuState.CharacterSelect;
                            transitionTimer = 0;
                        }
                        //open a how to play guide
                        else if(selectionId == 1)
                        {
                            previousMenuState = MenuState.MainMenu;
                            currentMenuState = MenuState.HowToPlay;
                            transitionTimer = 0;
                            currentHowToPlayPage = 0;
                            previousHowToPlayPage = 0;
                        }
                        //or quit the game
                        else if (selectionId == 2)
                        {
                            gameBase.Exit();
                        }
                    }
                    break;

                    //play can select from 2 different characters!
                case (MenuState.CharacterSelect):
                    selectionIdMax = 1;
                    UpdateSelectionId();
                    //pressing back just goes back to the main menu
                    if (Game1.ActionPressedThisFrame(Action.Back))
                    {
                        previousMenuState = MenuState.CharacterSelect;
                        currentMenuState = MenuState.MainMenu;
                        transitionTimer = 0;
                        selectionId = 0;
                        selectedStageIndex = 0;
                    }
                    //pressing accept stores the character id they selected, and goes ont othe stage selection screen
                    else if (Game1.ActionPressedThisFrame(Action.Accept) || Game1.ActionPressedThisFrame(Action.Jump))
                    {
                        previousMenuState = MenuState.CharacterSelect;
                        currentMenuState = MenuState.StageSelect;
                        transitionTimer = 0;
                        selectedCharacterId = selectionId;
                        selectionId = selectedStageIndex;
                    }
                    break;

                    //User can select from a list of 16 different stages to play!!
                case (MenuState.StageSelect):
                    selectionIdMax = 15;
                    UpdateSelectionId();
                    selectedStageIndex = selectionId;

                    //pressing back just goes back to the char select screen
                    if (Game1.ActionPressedThisFrame(Action.Back))
                    {
                        previousMenuState = MenuState.StageSelect;
                        currentMenuState = MenuState.CharacterSelect;
                        transitionTimer = 0;
                        selectionId = selectedCharacterId;
                    }
                    //pressing accept loads the stage they are selecting!
                    else if (Game1.ActionPressedThisFrame(Action.Accept) || Game1.ActionPressedThisFrame(Action.Jump))
                    {
                        gameBase.SelectStage(selectedStageIndex, selectedCharacterId);
                        inActionTimer = 0;
                    }
                    break;

                    //how to play will show a multipage comic book styled guide on how to play the game!
                    //user can press left or right/up or down to switch between pages
                case (MenuState.HowToPlay):
                    if(howToPlayTransitionTimer <= howToPlayTransitionTimerMax)
                    {
                        howToPlayTransitionTimer += gameTime.ElapsedGameTime.TotalSeconds;
                        break;
                    }

                    //pressing back goes back to the main menu
                    if (Game1.ActionPressedThisFrame(Action.Back))
                    {
                        previousMenuState = MenuState.HowToPlay;
                        currentMenuState = MenuState.MainMenu;
                        transitionTimer = 0;
                    }
                    //pressing right goes to the next page in the how to play guide
                    else if (Game1.ActionPressedThisFrame(Action.MoveRight) || Game1.ActionPressedThisFrame(Action.MoveDown))
                    {
                        if(currentHowToPlayPage != 2)
                        {
                            previousHowToPlayPage = currentHowToPlayPage;
                            currentHowToPlayPage++;
                            howToPlayTransitionTimer = 0;
                        }
                    }
                    //pressing left goes to the previous page in the how to play guide
                    else if (Game1.ActionPressedThisFrame(Action.MoveLeft) || Game1.ActionPressedThisFrame(Action.MoveUp))
                    {
                        if (currentHowToPlayPage != 0)
                        {
                            previousHowToPlayPage = currentHowToPlayPage;
                            currentHowToPlayPage--;
                            howToPlayTransitionTimer = 0;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Draws our menu depending on which part of the menu the player is on, as well as transitions between each of them
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to draw to</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            //this animates all the transitions between each menu
            if (transitionTimer < transitionTimerMax)
            {
                //from title screen to main menu
                if(previousMenuState == MenuState.TitleScreen && currentMenuState == MenuState.MainMenu)
                {
                    DrawMainMenu(spriteBatch, transitionTimer/transitionTimerMax, 0);
                }
                //main menu back to title screen
                else if (previousMenuState == MenuState.MainMenu && currentMenuState == MenuState.TitleScreen)
                {
                    DrawMainMenu(spriteBatch, 1 - transitionTimer / transitionTimerMax, 0);
                }
                //main menu to char selection screen
                else if (previousMenuState == MenuState.MainMenu && currentMenuState == MenuState.CharacterSelect)
                {
                    DrawMainMenu(spriteBatch, 1, transitionTimer / transitionTimerMax);
                    DrawCharacterSelectScreen(spriteBatch, transitionTimer / transitionTimerMax, 0);
                }
                //main menu to how to play
                else if (previousMenuState == MenuState.MainMenu && currentMenuState == MenuState.HowToPlay)
                {
                    DrawMainMenu(spriteBatch, 1, transitionTimer / transitionTimerMax);
                    DrawHowToPlayGuide(spriteBatch, transitionTimer / transitionTimerMax, 1);
                }
                //char select back to main menu
                else if (previousMenuState == MenuState.CharacterSelect && currentMenuState == MenuState.MainMenu)
                {
                    DrawMainMenu(spriteBatch, 1, 1 - transitionTimer / transitionTimerMax);
                    DrawCharacterSelectScreen(spriteBatch, 1 - transitionTimer / transitionTimerMax, 0);
                }
                //char select to stage select
                else if (previousMenuState == MenuState.CharacterSelect && currentMenuState == MenuState.StageSelect)
                {
                    DrawCharacterSelectScreen(spriteBatch, 1, transitionTimer / transitionTimerMax);
                    DrawStageSelectScreen(spriteBatch, transitionTimer / transitionTimerMax);
                }
                //stage select back to char select
                else if(previousMenuState == MenuState.StageSelect && currentMenuState == MenuState.CharacterSelect)
                {
                    DrawCharacterSelectScreen(spriteBatch, 1, 1 - transitionTimer / transitionTimerMax);
                    DrawStageSelectScreen(spriteBatch, 1 - transitionTimer / transitionTimerMax);
                }
                //how to play back to main menu
                else if (previousMenuState == MenuState.HowToPlay && currentMenuState == MenuState.MainMenu)
                {
                    DrawHowToPlayGuide(spriteBatch, 1 - transitionTimer / transitionTimerMax, 1);
                    DrawMainMenu(spriteBatch, 1, 1 - transitionTimer / transitionTimerMax);
                }
                return;
            }

            //if we are not transitioning between 2 states, then just display the current menu as normal!
            switch (currentMenuState)
            {
                case (MenuState.TitleAnimation):
                    DrawTitleAnimation(spriteBatch);
                    break;

                case (MenuState.TitleScreen):
                    //draw the title logo
                    spriteBatch.Draw(Game1.UITextures[18], new Vector2(Game1.RenderTargetWidth / 2 - Game1.UITextures[18].Width / 2, 400 - Game1.UITextures[18].Height / 2), Color.White);

                    //draw blinking instructions that tells the player to "press a or enter"
                    if(titleInstructionsBlinkTimer >= titleInstructionsBlinkTimerMax + .5)
                    {
                        titleInstructionsBlinkTimer = 0;
                    }
                    else if(titleInstructionsBlinkTimer < titleInstructionsBlinkTimerMax)
                    {
                        ScarletFontDrawer.DrawStringScaled(spriteBatch, "press a or enter!", Game1.RenderTargetWidth / 2, 800, 60, TextAlignment.TopCenter, true, Color.White, Color.White);
                    }
                    break;

                //just draw the rest of the menus normally
                case (MenuState.MainMenu):
                    DrawMainMenu(spriteBatch, 1, 0);
                    break;

                case (MenuState.CharacterSelect):
                    DrawCharacterSelectScreen(spriteBatch, 1, 0);
                    break;

                case (MenuState.StageSelect):
                    DrawStageSelectScreen(spriteBatch, 1);
                    break;

                    //make sure to give the how to play screen info on its page turn transition timer
                case (MenuState.HowToPlay):
                    DrawHowToPlayGuide(spriteBatch, 1, howToPlayTransitionTimer / howToPlayTransitionTimerMax);
                    break;
            }
        }

        /// <summary>
        /// Updates ONLY the pause menu. lets the player resume the game, restart the stage, or quit back to the menu
        /// </summary>
        /// <param name="gameTime">elapsed time since last frame</param>
        public void UpdatePauseMenu(GameTime gameTime)
        {
            if (inActionTimer < inActionTimerMax)
            {
                inActionTimer += gameTime.ElapsedGameTime.TotalSeconds;
                return;
            }


            UpdatePauseSelectionId();
            if (Game1.ActionPressedThisFrame(Action.Back) || Game1.ActionPressedThisFrame(Action.Pause))
            {
                gameBase.GameState = GameState.Play;
            }
            else if (Game1.ActionPressedThisFrame(Action.Accept) || Game1.ActionPressedThisFrame(Action.Jump))
            {
                if (pauseSelectionId == 0)
                {
                    gameBase.GameState = GameState.Play;
                }
                else if (pauseSelectionId == 1)
                {
                    gameBase.RestartStage();
                    inActionTimer = 0;
                }
                else if (pauseSelectionId == 2)
                {
                    gameBase.QuitStage();
                    inActionTimer = 0;
                }
            }
        }

        /// <summary>
        /// Draws our pause menu.
        /// </summary>
        /// <param name="spriteBatch">the spritebatch to draw to</param>
        public void DrawPauseMenu(SpriteBatch spriteBatch)
        {
            //draw background overlays
            spriteBatch.Draw(Game1.UITextures[10], new Rectangle(0, 0, Game1.RenderTargetWidth, Game1.RenderTargetHeight), Color.White);
            spriteBatch.Draw(Game1.UITextures[14], new Rectangle(0, 0, Game1.RenderTargetWidth, Game1.RenderTargetHeight), Color.White);

            //draws our buttons for the pause menu
            for (int i = 0; i < pauseMenuButtons.Length; i++)
            {
                DrawBasicButton(spriteBatch, pauseMenuButtons[i], new Point(600, 500 + (120 * i)), (pauseSelectionId == i));
            }

            //draw text that says "paused!"
            ScarletFontDrawer.DrawStringScaled(spriteBatch, "paused!", Game1.RenderTargetWidth/2, 230, 120, TextAlignment.TopCenter, true, Color.White, Color.White);
        }

        /// <summary>
        /// Draws the main menu and any animations it has for transitions between other menus
        /// </summary>
        /// <param name="spriteBatch">the spritebatch to draw to</param>
        /// <param name="titleTransitionPercentage">the transition period between the title screen and the main menu</param>
        /// <param name="cssTransitionPercentage">the transition period between the main menu and the character select screen</param>
        private void DrawMainMenu(SpriteBatch spriteBatch, double titleTransitionPercentage, double cssTransitionPercentage)
        {
            //drawing our logo in the upper right corner of the screen
            //transitioning between the middle if we are coming from the title screen state
            Rectangle titleTransformTitleScreen = new Rectangle(Game1.RenderTargetWidth / 2 - Game1.UITextures[18].Width / 2, 400 - Game1.UITextures[18].Height / 2, Game1.UITextures[18].Width, Game1.UITextures[18].Height);
            Rectangle titleTransformMainMenu = new Rectangle((int)(-Game1.RenderTargetWidth * cssTransitionPercentage) + Game1.RenderTargetWidth - 800, 350 - Game1.UITextures[18].Height / 2, 650, 500);

            spriteBatch.Draw(Game1.UITextures[18], new Rectangle((int)(titleTransformTitleScreen.X + (titleTransformMainMenu.X - titleTransformTitleScreen.X) * titleTransitionPercentage), 
                (int)(titleTransformTitleScreen.Y + (titleTransformMainMenu.Y - titleTransformTitleScreen.Y) * titleTransitionPercentage),
                (int)(titleTransformTitleScreen.Width + (titleTransformMainMenu.Width - titleTransformTitleScreen.Width) * titleTransitionPercentage),
                (int)(titleTransformTitleScreen.Height + (titleTransformMainMenu.Height - titleTransformTitleScreen.Height) * titleTransitionPercentage)), Color.White);

            //drawing our characters standing on a building on the left
            int charactersWidth = 1000;

            spriteBatch.Draw(Game1.UITextures[19], new Rectangle((int)(-Game1.RenderTargetWidth * cssTransitionPercentage) + (int)(-charactersWidth + (charactersWidth * titleTransitionPercentage)), 0, charactersWidth, Game1.UITextures[19].Height), Color.White);

            //drawing our buttons in the lower right
            for(int i = 0; i < mainMenuButtons.Length; i++)
            {
                DrawBasicButton(spriteBatch, mainMenuButtons[i], new Point((int)(-Game1.RenderTargetWidth * cssTransitionPercentage) + Game1.RenderTargetWidth - (int)(850 * titleTransitionPercentage), 600 + (120*i)), (selectionId == i));
            }

            //drawing some instructions on how to control the menus
            ScarletFontDrawer.DrawStringScaled(spriteBatch, "gamepad/wasd - move", (int)(-Game1.RenderTargetWidth * cssTransitionPercentage) + Game1.RenderTargetWidth - (int)(475 * titleTransitionPercentage), 970, 40, TextAlignment.TopCenter, true, Color.White, Color.White);
            ScarletFontDrawer.DrawStringScaled(spriteBatch, "a /enter-select, b /esc-back", (int)(-Game1.RenderTargetWidth * cssTransitionPercentage) + Game1.RenderTargetWidth - (int)(475 * titleTransitionPercentage), 1020, 40, TextAlignment.TopCenter, true, Color.White, Color.White);
        }

        /// <summary>
        /// Draws our character select screen with transitions
        /// </summary>
        /// <param name="spriteBatch">the spritebatch to draw to</param>
        /// <param name="mainTransitionPercentage">the transition period between the main menu and the character select screen</param>
        /// <param name="stageTransitionPercentage">the transition period between the character select screen and the stage select screen</param>
        private void DrawCharacterSelectScreen(SpriteBatch spriteBatch, double mainTransitionPercentage, double stageTransitionPercentage)
        {
            //drawing background overlays that darken the screen
            spriteBatch.Draw(Game1.UITextures[10], new Rectangle((int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)), 0, Game1.RenderTargetWidth, Game1.RenderTargetHeight), Color.White);
            spriteBatch.Draw(Game1.UITextures[14], new Rectangle((int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)), 0, Game1.RenderTargetWidth, Game1.RenderTargetHeight), Color.White);

            //draw our character buttons with scarlet's selected/highlighted
            if(selectionId == 0)
            {
                spriteBatch.Draw(Game1.UITextures[27], new Vector2((int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)) - 50 - (int)(Game1.RenderTargetWidth * stageTransitionPercentage), 110), Color.White);
                spriteBatch.Draw(Game1.UITextures[28], new Vector2((int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)) + 920 - (int)(Game1.RenderTargetWidth * stageTransitionPercentage), 270), Color.White);
            }

            //draw our character buttons with cocoa's selected/highlighted
            else if(selectionId == 1)
            {
                spriteBatch.Draw(Game1.UITextures[26], new Vector2((int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)) - 50 - (int)(Game1.RenderTargetWidth * stageTransitionPercentage), 140), Color.White);
                spriteBatch.Draw(Game1.UITextures[29], new Vector2((int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)) + 920 - (int)(Game1.RenderTargetWidth * stageTransitionPercentage), 220), Color.White);
            }
            else
            {
                spriteBatch.Draw(Game1.UITextures[26], new Vector2((int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)) - 50 - (int)(Game1.RenderTargetWidth * stageTransitionPercentage), 140), Color.White);
                spriteBatch.Draw(Game1.UITextures[28], new Vector2((int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)) + 920 - (int)(Game1.RenderTargetWidth * stageTransitionPercentage), 270), Color.White);
            }

            //draw instructions on what to do
            ScarletFontDrawer.DrawStringScaled(spriteBatch, "B / ESC - back", (int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)) + 40, 950, 50, TextAlignment.TopLeft, true, Color.White, Color.White);
            ScarletFontDrawer.DrawStringScaled(spriteBatch, "select your vigilante!", (int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)) + 100 - (int)(Game1.RenderTargetWidth * stageTransitionPercentage), 80, 70, TextAlignment.TopLeft, true, Color.White, Color.White);
        }

        /// <summary>
        /// Draws our stage selection screen with transitions
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to draw to</param>
        /// <param name="charTransitionPercentage">the transition period between the character select screen and the stage select screen</param>
        private void DrawStageSelectScreen(SpriteBatch spriteBatch, double charTransitionPercentage)
        {
            //draw background overlays once the transition animation is done
            if(transitionTimer > transitionTimerMax)
            {
                spriteBatch.Draw(Game1.UITextures[10], new Rectangle(0, 0, Game1.RenderTargetWidth, Game1.RenderTargetHeight), Color.White);
                spriteBatch.Draw(Game1.UITextures[14], new Rectangle(0, 0, Game1.RenderTargetWidth, Game1.RenderTargetHeight), Color.White);
            }

            //draw our chosen character on the left side of the screen
            if(selectedCharacterId == 0)
                spriteBatch.Draw(Game1.UITextures[30], new Rectangle((int)(Game1.RenderTargetWidth * (1 - charTransitionPercentage)) + 200, 100, 700, 850), Color.White);
            else if (selectedCharacterId == 1)
                spriteBatch.Draw(Game1.UITextures[31], new Rectangle((int)(Game1.RenderTargetWidth * (1 - charTransitionPercentage)) + 200, 350, 640, 550), Color.White);

            //draw a list of buttons for each level in the game
            for(int i = 0; i < 16; i++)
            {
                DrawBasicButton(spriteBatch, "issue #" + (i+1), new Point((int)(Game1.RenderTargetWidth * (1 - charTransitionPercentage)) + 1000, (500 - 120 * selectionId) + (120 * i)), (selectionId == i));
            }

            //draw instructions on what to do
            ScarletFontDrawer.DrawStringScaled(spriteBatch, "B / ESC - back", 40, 950, 50, TextAlignment.TopLeft, true, Color.White, Color.White);
            ScarletFontDrawer.DrawStringScaled(spriteBatch, "select an issue to play!", (int)(Game1.RenderTargetWidth * (1 - charTransitionPercentage)) + 100, 80, 70, TextAlignment.TopLeft, true, Color.White, Color.White);
        }

        /// <summary>
        /// Draws the pages of a How to Play guide/comic
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to draw to</param>
        /// <param name="mainTransitionPercentage">the transition period between the main menu and the how to play guide</param>
        /// <param name="pageTurnTransitionPercentage">the transition period between the pages of the guide the player is switching between</param>
        private void DrawHowToPlayGuide(SpriteBatch spriteBatch, double mainTransitionPercentage, double pageTurnTransitionPercentage)
        {
            //if we transitioning between pages...
            if (pageTurnTransitionPercentage < 1)
            {
                //draw a transition going forwards in the guide
                if(currentHowToPlayPage > previousHowToPlayPage)
                {
                    spriteBatch.Draw(Game1.UITextures[32 + previousHowToPlayPage], new Vector2((int)-(Game1.RenderTargetWidth * (pageTurnTransitionPercentage)), 0), Color.White);
                    spriteBatch.Draw(Game1.UITextures[32 + currentHowToPlayPage], new Vector2((int)(Game1.RenderTargetWidth * (1 - pageTurnTransitionPercentage)), 0), Color.White);
                }
                //or draw a transition going backwards in the guide if thats wha tthe play chose to do
                else
                {
                    spriteBatch.Draw(Game1.UITextures[32 + previousHowToPlayPage], new Vector2((int)(Game1.RenderTargetWidth * (pageTurnTransitionPercentage)), 0), Color.White);
                    spriteBatch.Draw(Game1.UITextures[32 + currentHowToPlayPage], new Vector2((int)-(Game1.RenderTargetWidth * (1 - pageTurnTransitionPercentage)), 0), Color.White);
                }
            }
            //if we are not transitioning...
            else
            {
                //then just draw the current page normally
                spriteBatch.Draw(Game1.UITextures[32 + currentHowToPlayPage], new Vector2((int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)), 0), Color.White);
            }

            ScarletFontDrawer.DrawStringScaled(spriteBatch, "B / ESC - back", (int)(Game1.RenderTargetWidth * (1 - mainTransitionPercentage)) + 40, 950, 50, TextAlignment.TopLeft, true, Color.White, Color.White);
        }

        /// <summary>
        /// Draws a basic button with text. Button looks cooler if highlighted/selected
        /// If the button is off screen, it does not draw it
        /// </summary>
        /// <param name="spriteBatch">the spritebatch to draw to</param>
        /// <param name="text">the text of the button</param>
        /// <param name="buttonPosition">the button's top left corner on screen</param>
        /// <param name="highlighted">if the button is highlighted/selected or not</param>
        private void DrawBasicButton(SpriteBatch spriteBatch, String text, Point buttonPosition, bool highlighted)
        {
            //if the button is off screen, do not draw it!
            if(buttonPosition.X >= Game1.RenderTargetWidth || buttonPosition.X < -Game1.UITextures[24].Width || 
                buttonPosition.Y >= Game1.RenderTargetHeight || buttonPosition.Y < -Game1.UITextures[24].Height)
            {
                return;
            }

            //if the button is highlighted/selected
            if (highlighted)
            {
                //draw a drop shadow version of the button's background
                spriteBatch.Draw(Game1.UITextures[25], new Vector2(16 + buttonPosition.X,
                    16 + buttonPosition.Y), Color.Black);

                //then draw the highlighted button background
                spriteBatch.Draw(Game1.UITextures[25], buttonPosition.ToVector2(), Color.White);

                //then draw the text of the button with a cool color gradation
                ScarletFontDrawer.DrawStringScaled(spriteBatch, text, buttonPosition.X + Game1.UITextures[25].Width / 2, buttonPosition.Y + Game1.UITextures[25].Height / 2 - 6, 80, TextAlignment.MiddleCenter, true, Color.Yellow, Game1.ScarletColor);
            }
            //if its not selected
            else
            {
                //then draw the highlighted button background
                spriteBatch.Draw(Game1.UITextures[24], buttonPosition.ToVector2(), Color.White);

                //then draw the text just white
                ScarletFontDrawer.DrawStringScaled(spriteBatch, text, buttonPosition.X + Game1.UITextures[25].Width / 2, buttonPosition.Y + Game1.UITextures[25].Height / 2, 60, TextAlignment.MiddleCenter, false, Color.White, Color.White);
            }
        }


        /// <summary>
        /// Draws the animation for the title sequence when the game is launched
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to draw to</param>
        private void DrawTitleAnimation(SpriteBatch spriteBatch)
        {
            double titleZoomTime = .8;

            //if the title logo is still enlarging...
            if(titleAnimationTimer < titleZoomTime)
            {
                //...then keep drawing the black cover over the screen
                spriteBatch.Draw(Game1.UITextures[9], new Rectangle(0, 0, Game1.RenderTargetWidth, Game1.RenderTargetHeight), Color.White);
            }
            //when the logo is at its final size/position
            else
            {
                //then start scrolling the cover over to the left
                spriteBatch.Draw(Game1.UITextures[8], new Rectangle((int)((-Game1.RenderTargetWidth - 240) * (titleAnimationTimer - titleZoomTime) / (titleAnimationTimerMax - titleZoomTime)) - 240, 0, Game1.RenderTargetWidth + 480, Game1.RenderTargetHeight), Color.White);
            }


            //while timer is below the title logo's zoom time
            double titleScale = Math.Min(titleAnimationTimer, titleZoomTime) / titleZoomTime;

            //then make the title zoom/enlarge
            spriteBatch.Draw(Game1.UITextures[18], new Rectangle((int)((Game1.RenderTargetWidth/2) - (Game1.UITextures[18].Width*titleScale)/2),
                (int)(400 - (Game1.UITextures[18].Height * titleScale) / 2), (int)(Game1.UITextures[18].Width * titleScale), (int)(Game1.UITextures[18].Height * titleScale)), Color.White);
        }

        /// <summary>
        /// Moves the player's selection up/left or down/right depending on what button they are pressing
        /// </summary>
        private void UpdateSelectionId()
        {
            if(Game1.ActionPressedThisFrame(Action.MoveRight) || Game1.ActionPressedThisFrame(Action.MoveDown))
                selectionId++;
            else if (Game1.ActionPressedThisFrame(Action.MoveLeft) || Game1.ActionPressedThisFrame(Action.MoveUp))
                selectionId--;

            //clamp the selection to what the current menu's max is
            selectionId = Math.Max(0, Math.Min(selectionId, selectionIdMax));
        }

        /// <summary>
        /// Moves the player's selection for the pause menu up/left or down/right depending on what button they are pressing
        /// </summary>
        private void UpdatePauseSelectionId()
        {
            if (Game1.ActionPressedThisFrame(Action.MoveRight) || Game1.ActionPressedThisFrame(Action.MoveDown))
                pauseSelectionId++;
            else if (Game1.ActionPressedThisFrame(Action.MoveLeft) || Game1.ActionPressedThisFrame(Action.MoveUp))
                pauseSelectionId--;

            //clamp the selection to what the current menu's max is
            pauseSelectionId = Math.Max(0, Math.Min(pauseSelectionId, pauseMenuButtons.Length - 1));
        }
    }
}
