using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Linq;

/* This is the main class file for this application, it runs the initial load content
 * and handles loading of all the assets for each room. It contains the main update 
 * method of this application, it also deals with the menu screens, updating enemy/player
 * attacks, enemy movements and turning the sound on/off & pausing the game.
 * 
 * Chris Forgie 2015
 * XNA3dDemo
 */

namespace XNA3dDemo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //Characters and Enemies
        private Character heroCharacter;

        private Enemy enemyCharacter1, enemyCharacter2, enemyCharacter3, enemyCharacter4, enemyCharacter5, enemyBoss;
        private Enemy[] room1Enemies, room2Enemies, room3Enemies;
        private int numEnemyLeftRoom1, numEnemyLeftRoom2, numEnemyLeftRoom3;
        private Model heroModel, enemyModel;
        public static Model heroAttack, bossAttack;
        private Texture2D enemyTexture;

        //Position, rotation and scale of these models
        private Vector3 modPosition, modRotation;

        private float modScale, enemyHealth, enemySpeed;

        //Cameras
        private Camera mainCam, personCam, switchCam;

        //Current State of Game
        private string currentGameState, newGameState;

        //Menu variables including buttons
        private bool gamePaused;

        private MenuScreen mainMenu, controlsMenu, gameOverMenu, gameWinMenu;
        private Rectangle button1Place, button2Place, button3Place;
        private Texture2D menuScreen, t_playOption, t_playOptionHover, t_controlsOption, t_controlsOptionHover, t_quitOption, t_quitOptionHover;
        private Texture2D controlsScreen, t_backOption;
        private Texture2D gameOverScreen, t_menuOption, t_menuOptionHover;
        private Texture2D gameWinScreen;

        //Game Sound Effects
        private Song bgMusic;

        //Make the sound effects accessible to the Character and Enemy Classes
        public static SoundEffect heroAttackSound, bossAttackSound;

        private bool soundOff;

        //Keyboard, Mouse & GamePad States
        private KeyboardState oldKeyState, currentKeyState;

        private MouseState oldMouseState, currentMouseState;
        private GamePadState oldPadState, currentPadState;

        //Floor and Wall models
        private Model floorModel, wallModelLeft, wallModelRight, wallModelVeryBack, wallModelBackLeft, wallModelBackRight, exitDoorModel;

        private Texture2D floorTex, wallTex, exitDoorTex;
        private Vector3 exitDoorPos, exitDoor2Pos;

        //Font
        private SpriteFont gameFont;

        //Health Bar
        private Texture2D healthBarOutline, healthBar;

        private Color healthBarColor;

        //Game score and time played
        private int gameScore;

        private string totalTimePlayed;
        private DateTime startTime, currentTime, timePaused;
        private TimeSpan gameTimeSpan, pausedTimeSpan;
        private bool timeSet, pausedTime;

        //World, view and projection matrices
        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));

        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), Vector3.UnitY);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.1f, 100f);

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
            // TODO: Add your initialization logic here

            graphics.PreferredBackBufferHeight = GameConstants.gameWindowHeight;
            graphics.PreferredBackBufferWidth = GameConstants.gameWindowWidth;
            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            LoadGame();
        }

        /* This method will load/re-load the game from the main menu state
        *  It resets all variables and prepares the game by loading the models
        *  and assets for the first room, the music and each menu state. */

        private void LoadGame()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            GameConstants gameVariables = new GameConstants();
            gameVariables.ResetConstants();

            //
            //  LOAD CHARACTERS AND ENEMIES
            //
            //Load our main character
            heroModel = Content.Load<Model>("Models\\CharModel");
            modPosition = GameConstants.heroStartPosition;
            modRotation = GameConstants.heroStartRotation;
            modScale = GameConstants.heroStartScale;
            //Assign the attack to the model. Not actually created until HeroAttack.cs
            heroAttack = Content.Load<Model>("Models\\BulletAttack");
            GameConstants.heroAttackTexure = Content.Load<Texture2D>("Textures\\HeroAttack");
            heroCharacter = new Character(heroModel, modPosition, modRotation, modScale, heroAttack);

            //Declary enemies for each room
            //Define the number of enemies in each room and what enemies are contained within
            room1Enemies = new Enemy[GameConstants.numRoom1Enemies];
            numEnemyLeftRoom1 = GameConstants.numRoom1Enemies;
            room2Enemies = new Enemy[GameConstants.numRoom2Enemies];
            numEnemyLeftRoom2 = GameConstants.numRoom2Enemies;
            room3Enemies = new Enemy[GameConstants.numRoom3Enemies];
            numEnemyLeftRoom3 = GameConstants.numRoom3Enemies;
            //Assign the boss attack to the model. Not actually created until BossAttack.cs
            bossAttack = Content.Load<Model>("Models\\BossAttack");

            //
            //  LOAD CAMERAS
            //
            //Load our top down static camera
            mainCam = new Camera(false, heroCharacter);
            personCam = new Camera(true, heroCharacter); //Third Person Camera (Change on/off by using C button)
            switchCam = mainCam; //Used as a holder to switch cameras

            //Load the default room assets which is the first room
            LoadRoom1();

            //
            //  LOAD OTHER ASSETS
            //
            //Floor and Wall Models
            floorModel = Content.Load<Model>("Models\\Cuboid");
            wallModelLeft = Content.Load<Model>("Models\\Cuboid");
            wallModelRight = Content.Load<Model>("Models\\Cuboid");
            wallModelVeryBack = Content.Load<Model>("Models\\Cuboid");
            wallModelBackLeft = Content.Load<Model>("Models\\Cuboid");
            wallModelBackRight = Content.Load<Model>("Models\\Cuboid");
            exitDoorModel = Content.Load<Model>("Models\\ExitDoor");
            //Images for these models
            floorTex = Content.Load<Texture2D>("Textures\\GroundTexture");
            wallTex = Content.Load<Texture2D>("Textures\\CaveWall");
            exitDoorTex = Content.Load<Texture2D>("Textures\\DoorTexture");

            //Set initial (closed) positions for the exit doors
            exitDoorPos = new Vector3(0, 121f, -10);
            exitDoor2Pos = new Vector3(0, 301f, -10);

            //Load the font for displaying text
            gameFont = Content.Load<SpriteFont>("Other\\GameFont");
            //Load health bar images
            healthBarOutline = Content.Load<Texture2D>("Textures\\HealthBarOutline");
            healthBar = Content.Load<Texture2D>("Textures\\HealthBar");

            //
            //  LOAD GAME MUSIC
            //
            //Background Music
            //Music used is from Game Boy game: Zelda: Link's Awakening
            //Turtle Rock Music : http://www.zeldauniverse.net/music/zelda-soundtracks/links-awakening-ost/
            bgMusic = Content.Load<Song>("Sounds\\ZLA_TurtleRock");
            MediaPlayer.Volume = 0.12f; //Lower the volume
            MediaPlayer.IsRepeating = true; //Have the song on repeat
            MediaPlayer.Play(bgMusic);

            //Load the sound effects
            //Sound Effects used from Metroid games
            //http://metroidr.brpxqzme.net/index.php?act=resdb&param=01&c=5
            heroAttackSound = Content.Load<SoundEffect>("Sounds\\HeroAttackWav");
            bossAttackSound = Content.Load<SoundEffect>("Sounds\\BossAttackWav");
            SoundEffect.MasterVolume = 0.08f; //Lower the volume

            //Set the default game score and the timer to be used within the game
            gameScore = 0;
            startTime = DateTime.Now;
            gameTimeSpan = new TimeSpan();
            timeSet = false; //Set true when game is over.
            gamePaused = false;
            pausedTime = false;
            pausedTimeSpan = new TimeSpan();

            //
            //  LOAD EACH MENU STATE AND THE ASSEST NEEDED TO CREATE THAT MENU
            //

            //Main Menu
            menuScreen = Content.Load<Texture2D>("Textures\\Menu\\NewMenuScreen");
            t_playOption = Content.Load<Texture2D>("Textures\\Menu\\MenuButton1");
            t_playOptionHover = Content.Load<Texture2D>("Textures\\Menu\\MenuButton1_Hover");
            t_controlsOption = Content.Load<Texture2D>("Textures\\Menu\\MenuButton2");
            t_controlsOptionHover = Content.Load<Texture2D>("Textures\\Menu\\MenuButton2_Hover");
            t_quitOption = Content.Load<Texture2D>("Textures\\Menu\\MenuButton3");
            t_quitOptionHover = Content.Load<Texture2D>("Textures\\Menu\\MenuButton3_Hover");

            //Controls Menu
            controlsScreen = Content.Load<Texture2D>("Textures\\Menu\\ControlsScreen");
            t_backOption = Content.Load<Texture2D>("Textures\\Menu\\MenuButtonBack");

            //Game Over Menu
            gameOverScreen = Content.Load<Texture2D>("Textures\\Menu\\NewGameOverScreen");
            t_menuOption = Content.Load<Texture2D>("Textures\\Menu\\MenuButtonMainMenu");
            t_menuOptionHover = Content.Load<Texture2D>("Textures\\Menu\\MenuButtonMainMenu_Hover");

            //Game Win Menu
            gameWinScreen = Content.Load<Texture2D>("Textures\\Menu\\FinalWinScreen");

            int buttonPlaceWidth = (GameConstants.gameWindowWidth / 2) - (t_playOption.Width / 2);
            int buttonPlaceHeight = (GameConstants.gameWindowHeight / 2) - (t_playOption.Height / 2);

            button1Place = new Rectangle(buttonPlaceWidth, buttonPlaceHeight - t_playOption.Height, t_playOption.Width, t_playOption.Height);
            button2Place = new Rectangle(buttonPlaceWidth, buttonPlaceHeight + 10, t_playOption.Width, t_playOption.Height);
            button3Place = new Rectangle(buttonPlaceWidth, buttonPlaceHeight + t_playOption.Height + 20, t_playOption.Width, t_playOption.Height);

            mainMenu = new MenuScreen(menuScreen, t_playOption, t_playOptionHover, t_controlsOption, t_controlsOptionHover, t_quitOption, t_quitOptionHover);
            mainMenu.setButtonPlace(button1Place, button2Place, button3Place);

            controlsMenu = new MenuScreen(controlsScreen, t_backOption, t_backOption, null, null, null, null);
            button1Place = new Rectangle(buttonPlaceWidth, buttonPlaceHeight + t_playOption.Height * 2, t_playOption.Width, t_playOption.Height);
            button2Place = Rectangle.Empty;
            controlsMenu.setButtonPlace(button1Place, button2Place, button2Place);
            controlsMenu.ScreenActive = false;

            gameOverMenu = new MenuScreen(gameOverScreen, t_menuOption, t_menuOptionHover, t_quitOption, t_quitOptionHover, null, null);
            button1Place = new Rectangle(buttonPlaceWidth, buttonPlaceHeight + 10, t_playOption.Width, t_playOption.Height);
            button2Place = new Rectangle(buttonPlaceWidth, buttonPlaceHeight + t_playOption.Height + 20, t_playOption.Width, t_playOption.Height);
            button3Place = Rectangle.Empty;
            gameOverMenu.setButtonPlace(button1Place, button2Place, button3Place);
            gameOverMenu.ScreenActive = false;

            gameWinMenu = new MenuScreen(gameWinScreen, t_menuOption, t_menuOptionHover, t_quitOption, t_quitOptionHover, null, null);
            button1Place = new Rectangle(buttonPlaceWidth, buttonPlaceHeight + 10, t_playOption.Width, t_playOption.Height);
            button2Place = new Rectangle(buttonPlaceWidth, buttonPlaceHeight + t_playOption.Height + 20, t_playOption.Width, t_playOption.Height);
            button3Place = Rectangle.Empty;
            gameWinMenu.setButtonPlace(button1Place, button2Place, button3Place);
            gameWinMenu.ScreenActive = false;

            //Set the initial game load state to the Main Menu
            if(currentGameState != "Main Menu") currentGameState = "Main Menu";
            newGameState = currentGameState;

            //Set the mouse handle to this window
            Mouse.WindowHandle = Window.Handle;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            //Get the new keyboard/gamepad/mouse state for this frame
            currentKeyState = Keyboard.GetState();
            currentPadState = GamePad.GetState(PlayerIndex.One);
            currentMouseState = Mouse.GetState();

            //If a menu is active then update it
            if (currentGameState == "Main Menu" || currentGameState == "Controls Menu" || currentGameState == "Game Over Menu" || currentGameState == "Game Win Menu")
            {
                updateMenuScreens();
            }

            //Only update game methods if we are in the game state
            if (currentGameState == "Game" && !gamePaused)
            {
                //Update Character Movement
                heroCharacter.Update();

                //Update Enemy Character movements
                UpdateEnemyMovements(GameConstants.currentRoom);

                //Check for collisions between the hero and any enemies
                //Also enemies and of the hero's attacks
                CollisionChecking();

                //Update any attacks and remove them from screen
                UpdateAttacks();
                UpdateBossAttacks();

                //Update enemies that are dead
                UpdateEnemyHealth();

                //Update whatever room we are in
                UpdateRoomCheck(gameTime);

                //Check to see if the hero has lost all his health i.e. game over
                CheckHeroHealth();

                //Update Camera
                CheckCameraChange();
                mainCam.Update();
            }

            //Check for key presses to start or unpause
            CheckKeyboardChanges();

            //Update Last Key State
            oldKeyState = currentKeyState;
            oldPadState = currentPadState;
            oldMouseState = currentMouseState;

            //Update game state
            currentGameState = newGameState;

            base.Update(gameTime);
        }

        /* This method will update a menu screen if one is active, it is called from the Update function if we are
         * currently in a menu state. Depending on the state, the update function for that menu will be called
         * and if an option is chosen (button selected on the menu) then the switch method will act accordingly
         * and change the game state to the one chosen */

        private void updateMenuScreens()
        {
            //Reset mouse handle as sometimes the handle is lost between the game and the menu
            if (Mouse.WindowHandle != Window.Handle) Mouse.WindowHandle = Window.Handle;

            //Main Menu State
            if (currentGameState == "Main Menu")
            {
                mainMenu.Update(currentKeyState, oldKeyState, currentPadState, oldPadState, currentMouseState, oldMouseState);

                if (mainMenu.ButtonSelected != 0)
                {
                    switch (mainMenu.ButtonSelected)
                    {
                        case 1:
                            LoadGame();
                            newGameState = "Game";
                            break;

                        case 2:
                            newGameState = "Controls Menu";
                            controlsMenu.ScreenActive = true;
                            break;

                        case 3:
                            this.Exit();
                            break;
                    }

                    mainMenu.ScreenActive = false;
                }
            }

            //Controls Menu State
            if (currentGameState == "Controls Menu")
            {
                controlsMenu.Update(currentKeyState, oldKeyState, currentPadState, oldPadState, currentMouseState, oldMouseState);

                if (controlsMenu.ButtonSelected != 0)
                {
                    newGameState = "Main Menu";
                    mainMenu.ScreenActive = true;
                    controlsMenu.ScreenActive = false;
                }
            }

            //Game Over State
            if (currentGameState == "Game Over Menu")
            {
                gameOverMenu.Update(currentKeyState, oldKeyState, currentPadState, oldPadState, currentMouseState, oldMouseState);

                if (gameOverMenu.ButtonSelected != 0)
                {
                    switch (gameOverMenu.ButtonSelected)
                    {
                        case 1:
                            newGameState = "Main Menu";
                            mainMenu.ScreenActive = true;
                            LoadGame();
                            break;

                        case 2:
                            this.Exit();
                            break;
                    }

                    gameOverMenu.ScreenActive = false;
                }
            }

            //Game Win State
            if (currentGameState == "Game Win Menu")
            {
                gameWinMenu.Update(currentKeyState, oldKeyState, currentPadState, oldPadState, currentMouseState, oldMouseState);

                if (gameWinMenu.ButtonSelected != 0)
                {
                    switch (gameWinMenu.ButtonSelected)
                    {
                        case 1:
                            newGameState = "Main Menu";
                            mainMenu.ScreenActive = true;
                            LoadGame();
                            break;

                        case 2:
                            this.Exit();
                            break;
                    }

                    gameWinMenu.ScreenActive = false;
                }
            }
        }

        //Load all the enemies and assets for room 1
        private void LoadRoom1()
        {
            //ROOM 1
            //Load our enemy 1 - Room 1
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(-50, 110, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 150.0f;
            enemySpeed = 0.3f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\UndeadYeti");
            enemyCharacter1 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture, null);
            //Load our enemy 2 - Room 1
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(0, 110, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 200.0f;
            enemySpeed = 0.45f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Skeleton");
            enemyCharacter2 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture, null);
            //Load our enemy 3 - Room 1
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(50, 110, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 150.0f;
            enemySpeed = 0.4f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\UndeadYeti");
            enemyCharacter3 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture, null);

            room1Enemies[0] = enemyCharacter1;
            room1Enemies[1] = enemyCharacter2;
            room1Enemies[2] = enemyCharacter3;
        }

        //Load all the enemies and assets for room 2
        private void LoadRoom2()
        {
            //ROOM 2
            //Load our enemy 1 - Room 2
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(-70, 300, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 150.0f;
            enemySpeed = 0.36f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Skeleton");
            enemyCharacter1 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture, null);
            //Load our enemy 2 - Room 2
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(-35, 300, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 300.0f;
            enemySpeed = 0.24f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Zombie");
            enemyCharacter2 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture, null);
            //Load our enemy 3 - Room 2
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(0, 300, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 250.0f;
            enemySpeed = 0.48f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Mummy");
            enemyCharacter3 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture, null);
            //Load our enemy 4 - Room 2
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(35, 300, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 240.0f;
            enemySpeed = 0.42f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\SwampMonster");
            enemyCharacter4 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture, null);
            //Load our enemy 5 - Room 2
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(70, 300, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 260.0f;
            enemySpeed = 0.45f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\PumpkinMan");
            enemyCharacter5 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture, null);

            room2Enemies[0] = enemyCharacter1;
            room2Enemies[1] = enemyCharacter2;
            room2Enemies[2] = enemyCharacter3;
            room2Enemies[3] = enemyCharacter4;
            room2Enemies[4] = enemyCharacter5;
        }

        //Load all the enemies and assets for room 3
        private void LoadRoom3()
        {
            //ROOM 3
            //Load our enemy 1 - Room 3
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(0, 455, 25);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 3f;
            enemyHealth = 3000.0f;
            enemySpeed = 0f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Robot");
            enemyBoss = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture, bossAttack);

            //This enemy is the final boss
            enemyBoss.IsBoss = true;

            //Load our enemy 2 - Room 3
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(-80, 400, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 450.0f;
            enemySpeed = 0.4f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Santa");
            enemyCharacter2 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture, null);
            //Load our enemy 3 - Room 3
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(80, 400, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 300.0f;
            enemySpeed = 0.35f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Elf");
            enemyCharacter3 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture, null);

            room3Enemies[0] = enemyBoss;
            room3Enemies[1] = enemyCharacter2;
            room3Enemies[2] = enemyCharacter3;
        }

        //Check if we are finished for this room and then proceeed to call the assets for the next room to be loaded
        private void UpdateRoomCheck(GameTime gameTime)
        {
            //If we are in room 1
            if (GameConstants.currentRoom == 1)
            {
                //If we've killed all the enemies
                if (numEnemyLeftRoom1 <= 0)
                {
                    //Open the door
                    exitDoorPos = new Vector3(0, 121f, 100);

                    heroCharacter.DoorOpen = true;

                    if (heroCharacter.Position.Y > (((GameConstants.currentRoom) * GameConstants.roomSize) - GameConstants.vertBoundaryBack + 20))
                    {
                        Vector3 newCamPos = new Vector3(0, (GameConstants.currentRoom * GameConstants.vertBoundary) + 18, 55);
                        Vector3 newCamTarget = new Vector3(0, (GameConstants.currentRoom * GameConstants.vertBoundary) + 65, 0);

                        mainCam.UpdateTarget(newCamPos, newCamTarget);
                        personCam.UpdateTarget(newCamPos, newCamTarget);

                        //Close the door
                        exitDoorPos = new Vector3(0, 121f, -10);
                        heroCharacter.DoorOpen = false;

                        //Load Room 2
                        GameConstants.currentRoom = 2;
                        LoadRoom2();
                    }
                }
            }
            //If we are in room 2
            if (GameConstants.currentRoom == 2)
            {
                //If we've killed all the enemies
                if (numEnemyLeftRoom2 <= 0)
                {
                    //Open the door
                    exitDoor2Pos = new Vector3(0, 301f, 100);
                    heroCharacter.DoorOpen = true;

                    if (heroCharacter.Position.Y > (((GameConstants.currentRoom) * GameConstants.roomSize) - GameConstants.vertBoundaryBack + 40))
                    {
                        Vector3 newCamPos = new Vector3(0, (GameConstants.currentRoom * GameConstants.vertBoundary) + 83, 55);
                        Vector3 newCamTarget = new Vector3(0, (GameConstants.currentRoom * GameConstants.vertBoundary) + 136, 0);

                        mainCam.UpdateTarget(newCamPos, newCamTarget);
                        personCam.UpdateTarget(newCamPos, newCamTarget);

                        //Close the door
                        exitDoor2Pos = new Vector3(0, 299f, -10);
                        heroCharacter.DoorOpen = false;

                        //Load Room 2
                        GameConstants.currentRoom = 3;
                        LoadRoom3();
                    }
                }
            }
            //If we are in room 3
            if (GameConstants.currentRoom == 3)
            {
                //If we've killed all the enemies
                if (numEnemyLeftRoom3 <= 0)
                {
                    //Final Game Time
                    if (!timeSet)
                    {
                        currentTime = DateTime.Now;
                        gameTimeSpan = currentTime - startTime;
                        totalTimePlayed = string.Format("{0}:{1}", Math.Floor(gameTimeSpan.TotalMinutes), gameTimeSpan.ToString("ss"));
                    }

                    //Final game score
                    gameScore += (int)(heroCharacter.Health * 3);

                    //Go to Game Over Screen
                    newGameState = "Game Win Menu";
                    gameWinMenu.ScreenActive = true;
                }
            }
        }

        //Check to see if the hero has lost all his health, if so then go to the game over screen
        private void CheckHeroHealth()
        {
            if (heroCharacter.checkHealth())
            {
                //Game Over Screen
                newGameState = "Game Over Menu";
                gameOverMenu.ScreenActive = true;
            }
        }

        //Check to see if attack is still active, if not destroy and re-arrange the array for new attacks
        //NOTE: This can't be placed in a foreach loop like the start of this method as "hAttack" is read-only so this is a work-around
        private void UpdateAttacks()
        {
            //Call the update function of the hero attack
            foreach (HeroAttack hAttack in heroCharacter.OnScreenAttacks)
            {
                if (hAttack != null)
                {
                    hAttack.Update();
                }
            }

            for (int i = 0; i < (heroCharacter.OnScreenAttacks.Length); i++)
            {
                if (heroCharacter.OnScreenAttacks[i] != null)
                {
                    if (!heroCharacter.OnScreenAttacks[i].IsActive)
                    {
                        //Destory the object.
                        heroCharacter.OnScreenAttacks[i] = null;

                        //Give the hero another attack back
                        if (heroCharacter.NumAttacks > 0)
                        {
                            heroCharacter.NumAttacks--;
                        }

                        //Make sure we are still within the bounds of the array
                        if ((i + 1) < heroCharacter.OnScreenAttacks.Length)
                        {
                            //Re-order
                            if (heroCharacter.OnScreenAttacks[i + 1] != null)
                            {
                                heroCharacter.OnScreenAttacks[i] = heroCharacter.OnScreenAttacks[i + 1];
                            }
                        }
                    }
                }
            }
        }

        //Check to see if boss attack is still active, if not then delete it and allow a new one to spawn
        private void UpdateBossAttacks()
        {
            //Call the update function of the boss attack
            if (enemyBoss != null)
            {
                foreach (BossAttack bAttack in enemyBoss.OnScreenBossAttacks)
                {
                    if (bAttack != null)
                    {
                        bAttack.Update();
                    }
                }

                //Remove the boss attack if it has went off-screen
                if (enemyBoss.OnScreenBossAttacks[0] != null)
                {
                    if (enemyBoss.OnScreenBossAttacks[0].IsActive == false)
                    {
                        //Destory the object.
                        enemyBoss.OnScreenBossAttacks[0] = null;
                    }
                }
            }
        }

        //Check to see if an enemy still has health left or is inactive an destory
        private void UpdateEnemyHealth()
        {
            //Room 1 Enemies
            for (int i = 0; i < (room1Enemies.Length); i++)
            {
                if (room1Enemies[i] != null)
                {
                    if ((!room1Enemies[i].IsAlive) || (room1Enemies[i].Health <= 0))
                    {
                        //Destory the enemy.
                        room1Enemies[i] = null;
                        numEnemyLeftRoom1--;
                    }
                }
            }
            //Room 2 Enemies
            for (int i = 0; i < (room2Enemies.Length); i++)
            {
                if (room2Enemies[i] != null)
                {
                    if ((!room2Enemies[i].IsAlive) || (room2Enemies[i].Health <= 0))
                    {
                        //Destory the enemy.
                        room2Enemies[i] = null;
                        numEnemyLeftRoom2--;
                    }
                }
            }
            //Room 3 Enemies
            for (int i = 0; i < (room3Enemies.Length); i++)
            {
                if (room3Enemies[i] != null)
                {
                    if ((!room3Enemies[i].IsAlive) || (room3Enemies[i].Health <= 0))
                    {
                        //Destory the enemy.
                        room3Enemies[i] = null;
                        numEnemyLeftRoom3--;
                    }
                }
            }
        }

        //Check for collisions between the character and enemies/enemies and attacks
        private void CollisionChecking()
        {
            foreach (Enemy en in room1Enemies.Concat(room2Enemies).Concat(room3Enemies))
            {
                if (en != null)
                {
                    if (heroCharacter.CollisionCheck(en.ColSphere))
                    {
                        float xDiff = en.Position.X - heroCharacter.Position.X;
                        float yDiff = en.Position.Y - heroCharacter.Position.Y;
                        if (!en.IsBoss)
                        {
                            //push the enemy back from the player
                            en.Position += new Vector3(((en.Speed) * xDiff * 5), ((en.Speed) * yDiff * 5), 0f);
                        }
                        heroCharacter.Health -= 10.0f;
                    }

                    //Loop through all boss attacks to see if they have collided with the player
                    if (enemyBoss != null)
                    {
                        foreach (BossAttack bAttack in enemyBoss.OnScreenBossAttacks)
                        {
                            if (bAttack != null)
                            {
                                if (heroCharacter.CollisionCheck(bAttack.ColSphere))
                                {
                                    //Boss attack has collided with the player
                                    heroCharacter.Health -= (int)(GameConstants.bossAttackPower);
                                    bAttack.IsActive = false;
                                }
                            }
                        }
                    }

                    //Check if any of the hero's attacks have hit an enemy
                    foreach (HeroAttack hAttack in heroCharacter.OnScreenAttacks)
                    {
                        if (hAttack != null)
                        {
                            if (en.CollisionCheck(hAttack.ColSphere))
                            {
                                //If enemy has enough health to take a hit
                                if (en.Health > GameConstants.heroAttackPower)
                                {
                                    en.Health -= GameConstants.heroAttackPower;
                                    //Only push back if the enemy isnt the final boss
                                    if (!en.IsBoss)
                                    {
                                        en.Position += new Vector3((GameConstants.heroAttackSpeed * ((float)Math.Cos(hAttack.Rotation.Z))) * 10, (GameConstants.heroAttackSpeed * ((float)Math.Sin(hAttack.Rotation.Z))) * 10, 0);
                                    }
                                    hAttack.IsActive = false;
                                }
                                else
                                {
                                    //Enemy is dead
                                    en.Health -= GameConstants.heroAttackPower;
                                    if (!en.IsBoss)
                                    {
                                        en.Position += new Vector3((GameConstants.heroAttackSpeed * ((float)Math.Cos(hAttack.Rotation.Z))) * 10, (GameConstants.heroAttackSpeed * ((float)Math.Sin(hAttack.Rotation.Z))) * 10, 0);
                                    }
                                    hAttack.IsActive = false;

                                    en.IsAlive = false;

                                    //Increase the game score
                                    Random rndNum = new Random();
                                    int randomNum = rndNum.Next(1, 3);
                                    gameScore += (int)en.InitHealth * randomNum;
                                }
                            }
                        }
                    }
                }
            }
        }

        //Rotate enemy towards player (Do this here instead of Enemy class - saves passsing in hero Character)
        //Move enemy towards player
        private void UpdateEnemyMovements(int currentRoom)
        {
            foreach (Enemy en in room1Enemies.Concat(room2Enemies).Concat(room3Enemies))
            {
                if (en != null)
                {
                    //Work out angle of difference
                    float xDiff = heroCharacter.Position.X - en.Position.X;
                    float yDiff = heroCharacter.Position.Y - en.Position.Y;
                    float angleDiff = (float)Math.Atan2(yDiff, xDiff) + MathHelper.ToRadians(90);

                    //Rotate the enemy towards the player
                    if (en.Rotation.Z != angleDiff)
                    {
                        en.Rotation = new Vector3(en.Rotation.X, en.Rotation.Y, angleDiff);
                    }

                    //Move the enemy
                    //xDiff and yDiff are hero - enemy position, we want enemy - hero position
                    xDiff *= -1;
                    yDiff *= -1;

                    double hypotenuse = Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff));
                    xDiff /= (float)hypotenuse;
                    yDiff /= (float)hypotenuse;

                    Vector3 finalPos = en.Position - new Vector3(xDiff * en.Speed, yDiff * en.Speed, 0);

                    //Stop enemies passing through each other -- check their collisions
                    foreach (Enemy enCol in room1Enemies.Concat(room2Enemies).Concat(room3Enemies))
                    {
                        if (enCol != null)
                        {
                            if (enCol != en)
                            {
                                if (!en.CollisionCheck(enCol.ColSphere))
                                {
                                    en.Position = finalPos;
                                }
                                else
                                {
                                    //If they collide then set them slightly apart
                                    float xDiff2 = en.Position.X - enCol.Position.X;
                                    float yDiff2 = en.Position.Y - enCol.Position.Y;

                                    en.Position += new Vector3(xDiff2 * en.Speed / 15, yDiff2 * en.Speed / 15, 0);

                                    break;
                                }
                            }

                            //If final enemy left in the room, he has nothing to collide with so his position never gets updated
                            //Updated here if that condition is met
                            if (numEnemyLeftRoom1 == 1 || numEnemyLeftRoom2 == 1 || numEnemyLeftRoom3 == 1)
                            {
                                en.Position = finalPos;
                            }
                        }
                    }

                    //Update our enemy movements
                    en.Update();
                }
            }
        }

        //Check if the camera has been switched from top down to third person and vice-versa
        private void CheckCameraChange()
        {
            //Change camera perspective
            if (keyPressed(Keys.C, Buttons.Y))
            {
                //Do the switch
                switchCam = mainCam;
                mainCam = personCam;
                personCam = switchCam;
            }
        }

        //Check for key presses to pause/unpause, turn on/off sound and return the game to the main menu
        private void CheckKeyboardChanges()
        {
            //Pause/Unpause the game -- only if we are in the game state
            if (currentGameState == "Game")
            {
                if (keyPressed(Keys.P, Buttons.Start))
                {
                    gamePaused = !gamePaused;
                }
            }

            //Return the game to the main menu state
            if (keyPressed(Keys.Escape, Buttons.Back))
            {
                newGameState = "Main Menu";
                mainMenu.ScreenActive = true;
            }

            //Mute the game from any game state
            if (keyPressed(Keys.W, Buttons.X))
            {
                //Turn the sound on or off
                soundOff = !soundOff;
                if (soundOff)
                {
                    //Turn the volume to 0 for the media player and sound effects
                    MediaPlayer.Volume = 0;
                    SoundEffect.MasterVolume = 0;
                }
                else
                {
                    //Turn the volume back to their default values for the media player and sound effects
                    MediaPlayer.Volume = 0.1f;
                    SoundEffect.MasterVolume = 0.1f;
                }
            }
        }

        //Check to see if a key or button has been pressed
        public bool keyPressed(Keys keyPress, Buttons buttonPress)
        {
            if ((currentKeyState.IsKeyDown(keyPress) && oldKeyState.IsKeyUp(keyPress)) || (currentPadState.IsButtonDown(buttonPress) && oldPadState.IsButtonUp(buttonPress)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Fix for the depth state issue while drawing both 3D and 2D objects on screen
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Draw different scenarios on screen depening on which game state we are in
            //Game States: Main Menu, Controls, Game Win, Game Over and Game (running) states
            if (currentGameState == "Main Menu" || currentGameState == "Controls Menu" || currentGameState == "Game Over Menu" || currentGameState == "Game Win Menu")
            {
                spriteBatch.Begin();

                this.IsMouseVisible = true;

                if (currentGameState == "Main Menu") mainMenu.Draw(spriteBatch);
                if (currentGameState == "Controls Menu") controlsMenu.Draw(spriteBatch);
                if (currentGameState == "Game Over Menu") gameOverMenu.Draw(spriteBatch);
                if (currentGameState == "Game Win Menu")
                {
                    gameWinMenu.Draw(spriteBatch);

                    //Draw total time played on top of game win screen

                    spriteBatch.DrawString(gameFont, "" + gameScore, new Vector2(GameConstants.gameWindowWidth / 2 - 50, GameConstants.gameWindowHeight / 2 - 132), Color.White);
                    spriteBatch.DrawString(gameFont, "" + totalTimePlayed, new Vector2(GameConstants.gameWindowWidth / 2 - 50, GameConstants.gameWindowHeight / 2 - 85), Color.White);
                }

                spriteBatch.End();
            }

            //The game is running and not in a menu state
            if (currentGameState == "Game")
            {
                this.IsMouseVisible = false;

                //Draw ground and walls for each room
                //Room1
                DrawModel(floorModel, world, view, projection, floorTex, 1.5f, Vector3.Zero, new Vector3(0, 35, -10), true);
                DrawModel(wallModelLeft, world, view, projection, wallTex, 1.5f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-85, 36, -15), false);
                DrawModel(wallModelRight, world, view, projection, wallTex, 1.5f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(85, 36, -15), false);
                DrawModel(wallModelVeryBack, world, view, projection, wallTex, 1.55f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(0, -55, -15), false);
                DrawModel(wallModelBackLeft, world, view, projection, wallTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(-105, 120, -10), false);
                DrawModel(wallModelBackRight, world, view, projection, wallTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(105, 120, -10), false);
                DrawModel(exitDoorModel, world, view, projection, exitDoorTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), exitDoorPos, false);
                //Room2
                DrawModel(floorModel, world, view, projection, floorTex, 1.5f, Vector3.Zero, new Vector3(0, 210, -10), true);
                DrawModel(wallModelLeft, world, view, projection, wallTex, 1.6f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-85, 200, -15), false);
                DrawModel(wallModelRight, world, view, projection, wallTex, 1.6f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(85, 200, -15), false);
                DrawModel(wallModelBackLeft, world, view, projection, wallTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(-105, 300, -10), false);
                DrawModel(wallModelBackRight, world, view, projection, wallTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(105, 300, -10), false);
                DrawModel(exitDoorModel, world, view, projection, exitDoorTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), exitDoor2Pos, false);
                //Room3
                DrawModel(floorModel, world, view, projection, floorTex, 1.5f, Vector3.Zero, new Vector3(0, 380, -10), true);
                DrawModel(wallModelLeft, world, view, projection, wallTex, 1.65f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-85, 365, -15), false);
                DrawModel(wallModelRight, world, view, projection, wallTex, 1.65f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(85, 365, -15), false);
                DrawModel(wallModelVeryBack, world, view, projection, wallTex, 1.6f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(0, 465, -10), false);

                //Draw the hero character
                heroCharacter.DrawModel(world, view, projection, mainCam);

                //Draw each enemy
                foreach (Enemy en in room1Enemies.Concat(room2Enemies).Concat(room3Enemies))
                {
                    if (en != null)
                    {
                        en.DrawModel(world, view, projection, mainCam);
                    }
                }
                //Draw each hero attack
                for (int i = 0; i < heroCharacter.NumAttacks; i++)
                {
                    foreach (HeroAttack hAttack in heroCharacter.OnScreenAttacks)
                    {
                        if (hAttack != null)
                        {
                            hAttack.DrawModel(world, view, projection, mainCam);
                        }
                    }
                }
                //Draw each boss attack if the enemy boss is present on screen
                if (enemyBoss != null)
                {
                    foreach (BossAttack bAttack in enemyBoss.OnScreenBossAttacks)
                    {
                        if (bAttack != null)
                        {
                            bAttack.DrawModel(world, view, projection, mainCam);
                        }
                    }
                }

                //Draw 2D Text on the screen for the score, time and real 3D space texture for each enemies health bar
                spriteBatch.Begin();

                //If the game is paused then draw that string on screen
                if (gamePaused)
                {
                    spriteBatch.DrawString(gameFont, "GAME PAUSED", new Vector2(340, 90), Color.Red);
                }

                //Draw the players health bar
                Vector2 heroHealthPos = new Vector2(90, 15);
                int heroHealthPercentage = (int)((heroCharacter.Health / GameConstants.heroHealth) * 100);
                if (heroHealthPercentage >= 75)
                {
                    healthBarColor = Color.Green;
                }
                else if (heroHealthPercentage >= 40)
                {
                    healthBarColor = Color.Orange;
                }
                else
                {
                    healthBarColor = Color.Red;
                }

                spriteBatch.DrawString(gameFont, "Health: ", new Vector2(10, 10), Color.White);
                spriteBatch.Draw(healthBar, heroHealthPos, new Rectangle((int)heroHealthPos.X, (int)heroHealthPos.Y, heroHealthPercentage, healthBar.Height), healthBarColor);
                spriteBatch.Draw(healthBarOutline, heroHealthPos, Color.White);

                //Draw the amount of attacks, the hero has left
                spriteBatch.DrawString(gameFont, "Attacks Left: " + (GameConstants.heroNumAttacks - heroCharacter.NumAttacks), new Vector2(10, 30), Color.White);

                //Draw string for turning sound on and off
                if (soundOff)
                {
                    spriteBatch.DrawString(gameFont, "Press W to turn the sound ON.", new Vector2(10, 50), Color.White);
                }
                else
                {
                    spriteBatch.DrawString(gameFont, "Press W to turn the sound OFF.", new Vector2(10, 50), Color.White);
                }

                //String for drawing frames per second on the screen
                //spriteBatch.DrawString(gameFont, "FPS: " + Math.Ceiling(1/(float)gameTime.ElapsedGameTime.TotalSeconds), new Vector2(10, 70), Color.White);

                //Draw game time
                if (gamePaused)
                {
                    if (!pausedTime) timePaused = DateTime.Now - pausedTimeSpan;
                    pausedTime = true;
                    gameTimeSpan = timePaused - startTime;
                    string timePlayed = string.Format("{0}:{1}", Math.Floor(gameTimeSpan.TotalMinutes), gameTimeSpan.ToString("ss"));
                    spriteBatch.DrawString(gameFont, "Time Played: " + timePlayed, new Vector2(580, 10), Color.White);
                }
                else
                {
                    if (pausedTime)
                    {
                        pausedTimeSpan = DateTime.Now - timePaused;
                        pausedTime = false;
                    }
                    currentTime = DateTime.Now;
                    gameTimeSpan = (currentTime - startTime) - pausedTimeSpan;
                    string timePlayed = string.Format("{0}:{1}", Math.Floor(gameTimeSpan.TotalMinutes), gameTimeSpan.ToString("ss"));
                    spriteBatch.DrawString(gameFont, "Time Played: " + timePlayed, new Vector2(580, 10), Color.White);
                }


                //Draw a health bar for each enemy
                foreach (Enemy en in room1Enemies.Concat(room2Enemies).Concat(room3Enemies))
                {
                    if (en != null)
                    {
                        //Convert to screen space. (0,0) in spriteBatch isn't same as the camera's view matrix
                        Vector3 screenSpace = GraphicsDevice.Viewport.Project(Vector3.Zero, mainCam.projectionMatrix, mainCam.viewMatrix, Matrix.CreateTranslation(en.Position));
                        Vector2 healthPos;
                        if (en.IsBoss)
                        {
                            //Boss health should be placed slightly lower
                            healthPos = new Vector2((int)screenSpace.X - (healthBarOutline.Width / 2), (int)screenSpace.Y + 60);
                        }
                        else
                        {
                            //Regular enemy health position
                            healthPos = new Vector2((int)screenSpace.X - (healthBarOutline.Width / 2), (int)screenSpace.Y + 25);
                        }

                        //Get the health of the enemy as a percentage between 0 and 100
                        //Assign a color of the health depending on how much is left i.e. Green - most health, Red - near death
                        int enemyHealthPercentage = (int)((en.Health / en.InitHealth) * 100);
                        if (enemyHealthPercentage >= 75)
                        {
                            healthBarColor = Color.Green;
                        }
                        else if (enemyHealthPercentage >= 40)
                        {
                            healthBarColor = Color.Orange;
                        }
                        else
                        {
                            healthBarColor = Color.Red;
                        }
                        spriteBatch.Draw(healthBar, healthPos, new Rectangle((int)healthPos.X, (int)healthPos.Y, enemyHealthPercentage, healthBar.Height), healthBarColor);
                        spriteBatch.Draw(healthBarOutline, healthPos, Color.White);
                    }
                }

                //Draw Game Score
                spriteBatch.DrawString(gameFont, "Total Score: " + gameScore, new Vector2(580, 30), Color.White);

                //End spriteBatch for drawing
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        // Draw Model Function (Characters & Enemies not drawn here - they are drawn within their own respective classes
        public void DrawModel(Model model, Matrix world, Matrix view, Matrix projection, Texture2D modelTexure, float modelScale, Vector3 modelRotation, Vector3 modelPosition, bool lightOn)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //When texture sizes not a power of 2, textures have to be clamped
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                    //Create a static light source for each wall in the scene (character/enemies and attacks not drawn with this method)
                    if (lightOn)
                    {
                        effect.EnableDefaultLighting();
                        effect.LightingEnabled = true;
                        effect.DirectionalLight0.DiffuseColor = new Vector3(.15f, .15f, .15f);
                        effect.DirectionalLight0.Direction = new Vector3(5, 2, 2);
                    }

                    //Apply Texture
                    effect.TextureEnabled = true;
                    effect.Texture = modelTexure;

                    //Take matrices from the camera used
                    //Update world view depending on the characters values
                    world = Matrix.CreateScale(modelScale) * Matrix.CreateRotationX(modelRotation.X) * Matrix.CreateRotationY(modelRotation.Y) * Matrix.CreateRotationZ(modelRotation.Z) * Matrix.CreateTranslation(modelPosition);

                    effect.World = world;
                    effect.View = mainCam.viewMatrix;
                    effect.Projection = mainCam.projectionMatrix;
                }

                mesh.Draw();
            }
        }
    }
}