using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gp3Coursework
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

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
        //1: Start Screen, 2: Game, 3: End Screen/Game Over
        private short gameState;
        private bool gamePaused, gameOverDeath;
        private Texture2D titleScreen, winScreen, deathScreen;
        private string totalTimePlayed;

        //Game Sound Effects
        private Song bgMusic;
        //Make the sound effects accessible to the Character and Enemy Classes
        public static SoundEffect heroAttackSound, bossAttackSound;

        //Keyboard States
        KeyboardState oldKeyState, currentKeyState;
        //GamePad States (Xbox360 Controller)
        GamePadState oldPadState, currentPadState;

        //Other Models
        private Model floorModel, wallModelLeft, wallModelRight, wallModelVeryBack, wallModelBackLeft, wallModelBackRight, exitDoorModel;
        private Texture2D floorTex, wallTex, exitDoorTex;
        private Vector3 exitDoorPos, exitDoor2Pos;

        //Font
        private SpriteFont gameFont;
        //Health Bar
        private Texture2D healthBarOutline, healthBar;
        private Color healthBarColor;
        //Game Score
        private int gameScore;

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
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //
            //  LOAD CHARACTERS AND ENEMIES
            //
                //Load our main character
                heroModel = Content.Load<Model>("Models\\CharModel");
                modPosition = GameConstants.heroStartPosition;
                modRotation = GameConstants.heroStartRotation;
                modScale = GameConstants.heroStartScale;
                heroCharacter = new Character(heroModel, modPosition, modRotation, modScale);
                //Assign the attack to the model. Not actually created until HeroAttack.cs
                heroAttack = Content.Load<Model>("Models\\BulletAttack");
                GameConstants.heroAttackTexure = Content.Load<Texture2D>("Textures\\HeroAttack");

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
                GameConstants.heroAttackTexure = Content.Load<Texture2D>("Textures\\HeroAttack");


            //
            //  LOAD CAMERAS
            //
                //Load our top down static camera
                mainCam = new Camera(false, heroCharacter);
                personCam = new Camera(true, heroCharacter); //Third Person Camera
                switchCam = mainCam; //Used as a holder to switch cameras

                //Load the default room (Room1)
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

                //Load initial (closed) positions for the exit doors
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
                //Song taken from Game Boy game: Zelda: Link's Awakening
                //Turtle Rock Music : http://www.zeldauniverse.net/music/zelda-soundtracks/links-awakening-ost/
                bgMusic = Content.Load<Song>("Sounds\\ZLA_TurtleRock");
                MediaPlayer.Volume = 0.1f; //Lower the volume
                MediaPlayer.IsRepeating = true; //Have the song on repeat
                MediaPlayer.Play(bgMusic);

                //Load the sound effects
                //Sound Effects taken from Metroid games
                //http://metroidr.brpxqzme.net/index.php?act=resdb&param=01&c=5
                heroAttackSound = Content.Load<SoundEffect>("Sounds\\HeroAttackWav");
                bossAttackSound = Content.Load<SoundEffect>("Sounds\\BossAttackWav");
                SoundEffect.MasterVolume = 0.1f; //Lower the volume

            //
            //  GAME START FEATURES
            //
                //Start the game at the start screen
                gameState = 1;
                //Load various screens
                titleScreen = Content.Load<Texture2D>("Textures\\Screens\\Gp3_TitleScreen");
                winScreen = Content.Load<Texture2D>("Textures\\Screens\\Gp3_WinScreen");
                deathScreen = Content.Load<Texture2D>("Textures\\Screens\\Gp3_DeathScreen");
                
                //Default Game Score
                gameScore = 0;
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
            //Get the new keyboard/gamepad state
            currentKeyState = Keyboard.GetState();
            currentPadState = GamePad.GetState(PlayerIndex.One);

            //Only update game functions if we are in game
            //i.e. note start/end screen or paused
            if((gameState == 2) && (!gamePaused) && (!gameOverDeath))
            {
                ///
                ///  UPDATE GAME FUNCTIONS
                ///  
                    //Update Character Movement
                    heroCharacter.Update();

                    //Update Enemy Character movements
                    UpdateEnemyMovements();

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


            base.Update(gameTime);
        }

        //Load all the assets for room 1
        private void LoadRoom1()
        {
            //ROOM 1
            //Load our enemy 1 - Room 1
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(-50, 110, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 150.0f;
            enemySpeed = 0.1f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\UndeadYeti");
            enemyCharacter1 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture);
            //Load our enemy 2 - Room 1
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(0, 110, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 200.0f;
            enemySpeed = 0.2f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Skeleton");
            enemyCharacter2 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture);
            //Load our enemy 3 - Room 1
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(50, 110, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 150.0f;
            enemySpeed = 0.15f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\UndeadYeti");
            enemyCharacter3 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture);

            room1Enemies[0] = enemyCharacter1;
            room1Enemies[1] = enemyCharacter2;
            room1Enemies[2] = enemyCharacter3;
        }

        //Load all the assets for room 2
        private void LoadRoom2()
        {
            //ROOM 2
            //Load our enemy 1 - Room 2
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(-70, 300, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 150.0f;
            enemySpeed = 0.12f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Skeleton");
            enemyCharacter1 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture);
            //Load our enemy 2 - Room 2
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(-35, 300, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 300.0f;
            enemySpeed = 0.08f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Zombie");
            enemyCharacter2 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture);
            //Load our enemy 3 - Room 2
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(0, 300, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 250.0f;
            enemySpeed = 0.16f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Mummy");
            enemyCharacter3 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture);
            //Load our enemy 4 - Room 2
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(35, 300, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 240.0f;
            enemySpeed = 0.14f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\SwampMonster");
            enemyCharacter4 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture);
            //Load our enemy 5 - Room 2
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(70, 300, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 260.0f;
            enemySpeed = 0.18f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\PumpkinMan");
            enemyCharacter5 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture);

            room2Enemies[0] = enemyCharacter1;
            room2Enemies[1] = enemyCharacter2;
            room2Enemies[2] = enemyCharacter3;
            room2Enemies[3] = enemyCharacter4;
            room2Enemies[4] = enemyCharacter5;
        }

        //Load all the assets for room 2
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
            enemyBoss = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture);

            //This enemy is the final boss
            enemyBoss.IsBoss = true;

            //Load our enemy 2 - Room 3
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(-80, 400, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 450.0f;
            enemySpeed = 0.2f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Santa");
            enemyCharacter2 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture);
            //Load our enemy 3 - Room 3
            enemyModel = Content.Load<Model>("Models\\EnemyModel");
            modPosition = new Vector3(80, 400, 0);
            modRotation = new Vector3(MathHelper.PiOver2, 0, 0);
            modScale = 1f;
            enemyHealth = 300.0f;
            enemySpeed = 0.26f;
            enemyTexture = Content.Load<Texture2D>("Textures\\Enemies\\Elf");
            enemyCharacter3 = new Enemy(enemyModel, modPosition, modRotation, modScale, enemyHealth, enemySpeed, enemyTexture);

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

                    if (heroCharacter.Position.Y > (GameConstants.currentRoom * GameConstants.vertBoundary))
                    {
                        Vector3 newCamPos = new Vector3(0, (GameConstants.currentRoom * GameConstants.vertBoundary) + 15, 50);
                        Vector3 newCamTarget = new Vector3(0, (GameConstants.currentRoom * GameConstants.vertBoundary) + 65, 0);

                        mainCam.UpdateTarget(newCamPos, newCamTarget);
                        personCam.UpdateTarget(newCamPos, newCamTarget);

                        //Close the door
                        exitDoorPos = new Vector3(0, 121f, -10);

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

                    if (heroCharacter.Position.Y > (GameConstants.currentRoom * GameConstants.vertBoundary + 65))
                    {
                        Vector3 newCamPos = new Vector3(0, (GameConstants.currentRoom * GameConstants.vertBoundary) + 85, 50);
                        Vector3 newCamTarget = new Vector3(0, (GameConstants.currentRoom * GameConstants.vertBoundary) + 136, 0);

                        mainCam.UpdateTarget(newCamPos, newCamTarget);
                        personCam.UpdateTarget(newCamPos, newCamTarget);

                        //Close the door
                        exitDoorPos = new Vector3(0, 301f, -10);

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
                    totalTimePlayed = string.Format("{0}:{1}", Math.Floor(gameTime.TotalGameTime.TotalMinutes), gameTime.TotalGameTime.ToString("ss"));

                    //Final game score
                    gameScore += (int)(heroCharacter.Health * 3);

                    //Go to Game Over Screen
                    gameState = 3;
                }
            }
        }

        //Check to see if the hero has lost all his health, if so then go to the game over screen
        private void CheckHeroHealth()
        {
            if (heroCharacter.Health <= 0)
            {
                //Game Over Screen
                gameState = 3;
                gameOverDeath = true;
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
        //NOTE: This can't be placed in a foreach loop like the start of this method as "hAttack" is read-only so this is a work-around
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
        //NOTE: This can't be placed in a foreach loop like above as "en" is read-only so this is a work-around
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
                            en.Position += new Vector3(((en.Speed) * xDiff * 12), ((en.Speed) * yDiff * 12), 0f);
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
                                    //Boss attack seems to do three times it's own power
                                    //Likely due to how long its in contact (issue)
                                    heroCharacter.Health -= (int)((GameConstants.bossAttackPower /3));
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
        private void UpdateEnemyMovements()
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

                    //Once the enemy comes close to the player, don't slow him down
                    //xDiff and yDiff are hero - enemy position, we want enemy - hero position
                    xDiff *= -1;
                    yDiff *= -1;

                    if (xDiff < 5 && xDiff > -5)
                    {
                        xDiff *= 5;
                    }
                    if (yDiff < 5 && yDiff > -5)
                    {
                        yDiff *= 5;
                    }

                    en.Position -= new Vector3(((en.Speed / 18) * xDiff), ((en.Speed / 18) * yDiff), 0f);

                    //Update our enemy movements
                    en.Update();
                }
            }
        }

        //Check if the camera has been switched from top down to third person and vice-versa
        private void CheckCameraChange()
        {
            //Change camera perspective
            if ((currentKeyState.IsKeyDown(Keys.Q) && oldKeyState.IsKeyUp(Keys.Q)) || (currentPadState.IsButtonDown(Buttons.Y) && oldPadState.IsButtonUp(Buttons.Y)))
            {
                //Do the switch
                switchCam = mainCam;
                mainCam = personCam;
                personCam = switchCam;
            }
        }

        //Check for key presses to unpause the game or exit the title/end screen
        private void CheckKeyboardChanges()
        {
            //Pause/Unpause the game -- only if we are in the game state
            if (gameState == 2)
            {
                if ((currentKeyState.IsKeyDown(Keys.P) && oldKeyState.IsKeyUp(Keys.P)) || (currentPadState.IsButtonDown(Buttons.A) && oldPadState.IsButtonUp(Buttons.A)))
                {
                    gamePaused = !gamePaused;
                }
            }

            //Enter the game if we are on the start screen
            if (gameState == 1)
            {
                if ((currentKeyState.IsKeyDown(Keys.Enter) && oldKeyState.IsKeyUp(Keys.Enter)) || (currentPadState.IsButtonDown(Buttons.Start) && oldPadState.IsButtonUp(Buttons.Start)))
                {
                    gameState = 2;
                }
            }

            //Exit the game from any game state
            if ((currentKeyState.IsKeyDown(Keys.Escape) && oldKeyState.IsKeyUp(Keys.Escape)) || (currentPadState.IsButtonDown(Buttons.Back)))
            {
                this.Exit();
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
            //Fix found on: http://stackoverflow.com/questions/4996988/z-buffer-issue-by-mixing-2d-sprites-with-3d-model
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Draw ground and walls for each room
            //Room1
            DrawModel(floorModel, world, view, projection, floorTex, 1.5f, Vector3.Zero, new Vector3(0, 35, -10), true);
            DrawModel(wallModelLeft, world, view, projection, wallTex, 1.5f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-85, 36, -15), true);
            DrawModel(wallModelRight, world, view, projection, wallTex, 1.5f, new Vector3(0, MathHelper.PiOver2,0), new Vector3(85, 36, -15), true);
            DrawModel(wallModelVeryBack, world, view, projection, wallTex, 1.55f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(0, -55, -15), true);
            DrawModel(wallModelBackLeft, world, view, projection, wallTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(-105, 120, -10), true);
            DrawModel(wallModelBackRight, world, view, projection, wallTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(105, 120, -10), true);
            DrawModel(exitDoorModel, world, view, projection, exitDoorTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), exitDoorPos, false);
            //Room2
            DrawModel(floorModel, world, view, projection, floorTex, 1.5f, Vector3.Zero, new Vector3(0, 210, -10), true);
            DrawModel(wallModelLeft, world, view, projection, wallTex, 1.6f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-85, 200, -15), true);
            DrawModel(wallModelRight, world, view, projection, wallTex, 1.6f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(85, 200, -15), true);
            DrawModel(wallModelBackLeft, world, view, projection, wallTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(-105, 300, -10), true);
            DrawModel(wallModelBackRight, world, view, projection, wallTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(105, 300, -10), true);
            DrawModel(exitDoorModel, world, view, projection, exitDoorTex, 1.35f, new Vector3(MathHelper.PiOver2, 0, 0), exitDoor2Pos, false);
            //Room3
            DrawModel(floorModel, world, view, projection, floorTex, 1.5f, Vector3.Zero, new Vector3(0, 380, -10), true);
            DrawModel(wallModelLeft, world, view, projection, wallTex, 1.65f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(-85, 365, -15), true);
            DrawModel(wallModelRight, world, view, projection, wallTex, 1.65f, new Vector3(0, MathHelper.PiOver2, 0), new Vector3(85, 365, -15), true);
            DrawModel(wallModelVeryBack, world, view, projection, wallTex, 1.6f, new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(0, 465, -10), true);


            //Draw Models
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

            //Draw the title screen
            if (gameState == 1 || gameState == 3)
            {
                spriteBatch.Begin();

                    //Draw the title screen
                    if (gameState == 1)
                    {
                        spriteBatch.Draw(titleScreen, new Vector2(0, 0), Color.White);
                    }
                    
                    //Draw the game over (win) or the game over (lose) screen
                    if (gameState == 3)
                    {
                        //Game over death
                        if (gameOverDeath)
                        {
                            spriteBatch.Draw(deathScreen, new Vector2(0, 0), Color.White);
                        }
                        else 
                        {
                            //Game over win
                            spriteBatch.Draw(winScreen, new Vector2(0, 0), Color.White);
                            spriteBatch.DrawString(gameFont, "FINAL SCORE: " + gameScore, new Vector2(280, 300), Color.White);
                            spriteBatch.DrawString(gameFont, "TIME COMPLETION: " + totalTimePlayed, new Vector2(280, 340), Color.White);
                        }
                    }
                spriteBatch.End();
            }

            //ONLY DRAW THIS IF WE ARE IN THE GAME STATE
            if (gameState == 2)
            {
                //Draw text on the screen
                //Also draw the 2D health bars of the hero and the enemies
                spriteBatch.Begin();

                //If the game is paused then draw that string on screen
                if (gamePaused)
                {
                    spriteBatch.DrawString(gameFont, "GAME PAUSED", new Vector2(340, 50), Color.White);
                }

                //Draw Player Health
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

                //Draw amount of attacks left
                spriteBatch.DrawString(gameFont, "Attacks Left: " + (GameConstants.heroNumAttacks - heroCharacter.NumAttacks), new Vector2(10, 30), Color.White);

                //Draw Game Time
                TimeSpan ts = gameTime.TotalGameTime;
                string timePlayed = string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss"));
                spriteBatch.DrawString(gameFont, "Time Played: " + timePlayed, new Vector2(580, 10), Color.White);

                //Draw Enemy Health
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

                spriteBatch.End();

            }


            base.Draw(gameTime);
        }


        // Draw Model Function (Characters & Enemies not drawn here)
        public void DrawModel(Model model, Matrix world, Matrix view, Matrix projection, Texture2D modelTexure, float modelScale, Vector3 modelRotation, Vector3 modelPosition, bool lightOn)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {

                    //When texture sizes not a power of 2, textures have to be clamped
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                    if (lightOn)
                    {
                        effect.EnableDefaultLighting();
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
