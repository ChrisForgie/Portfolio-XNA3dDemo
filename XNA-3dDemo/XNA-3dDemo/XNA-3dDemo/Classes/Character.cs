using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/* This class is used to create the character, define
 * his collision (bounding sphere), handle the firing of his
 * attacks (collision checking of this is done in Game1), update
 * his movement by checking keyboard/gamepad entries and to make
 * sure he doesn't move outside the confines of the game area.
 * 
 * Chris Forgie 2015
 * XNA3dDemo
 */ 

namespace XNA3dDemo
{
    internal class Character : Microsoft.Xna.Framework.Game
    {
        //Character setup
        private Vector3 position, rotation;

        private float scale, health, rotAngle, moveSpeed;
        private int numAttacks;
        private HeroAttack[] onScreenAttacks;
        private Model charModel, heroAttack;

        //Bounding Sphere
        private BoundingSphere colSphere;

        //Keyboard States
        private KeyboardState oldKeyState, currentKeyState;

        //GamePad States (Xbox360 Controller)
        private GamePadState oldPadState, currentPadState;

        //Boundary position for movement
        //Since game takes place on a square, the same position would be used for left and right limits
        private int horBoundLimit, verBoundLimit, verBoundLimitBack;

        //Door opened or closed for movement north of room
        private bool doorOpen;

        //Getter and Setter Methods
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public float Health
        {
            get { return health; }
            set { health = value; }
        }

        public int NumAttacks
        {
            get { return numAttacks; }
            set { numAttacks = value; }
        }

        public HeroAttack[] OnScreenAttacks
        {
            get { return onScreenAttacks; }
            set { onScreenAttacks = value; }
        }

        public BoundingSphere ColSphere
        {
            get { return colSphere; }
        }

        public bool DoorOpen
        {
            get { return doorOpen; }
            set { doorOpen = value; }
        }

        //Constructor for the character
        public Character(Model m_charModel, Vector3 v3_position, Vector3 v3_rotation, float f_scale, Model heroAttackMod)
        {
            //Assign the model of this character class to the one which was passed in
            charModel = m_charModel;
            position = v3_position;
            rotation = v3_rotation;
            //Initial rotation of the Z-axis, the only axis that should rotate during run-time
            rotAngle = GameConstants.heroInitRotAngle;
            scale = f_scale;
            //The model for the attack the hero uses
            heroAttack = heroAttackMod;

            //Speed at which the player moves
            moveSpeed = GameConstants.heroMoveSpeed;

            //Health will always be a starting value of 100 for the hero character so no need to pass this in
            health = GameConstants.heroHealth;

            //Number of attacks which the player has currently fired. MAX: GameConstants.heroNumAttacks;
            numAttacks = 0;
            onScreenAttacks = new HeroAttack[GameConstants.heroNumAttacks];

            //Set the horizontal boundary limit
            horBoundLimit = GameConstants.horBoundary;
            verBoundLimit = GameConstants.vertBoundary;
            verBoundLimitBack = GameConstants.vertBoundaryBack; //The very back wall of game

            //Create a bounding sphere for the character
            createBoundingSphere();
        }

        //Create a bounding sphere around the character
        public void createBoundingSphere()
        {
            colSphere = new BoundingSphere();

            foreach (ModelMesh mesh in charModel.Meshes)
            {
                if (colSphere.Radius == 0)
                {
                    colSphere = mesh.BoundingSphere;
                }
                else
                {
                    colSphere = BoundingSphere.CreateMerged(colSphere, mesh.BoundingSphere);
                }
            }

            //Centre the sphere, also scale as character isnt a 1:1 scale
            colSphere.Center = (position + new Vector3(0, 0, 5));
            colSphere.Radius = scale * 2.5f;
        }

        //Update method
        public void Update()
        {
            //Update the movement
            UpdateMovement();

            //Check if we have fired any attacks
            FireAttack();

            //Handle the old keyboard/gamepad state
            oldKeyState = currentKeyState;
            oldPadState = currentPadState;

            //Update bounding sphere position and scale
            colSphere.Center = (position + new Vector3(0, 0, 5));
        }

        //Check for collisions between the character and enemies/other objects (Called in Game1)
        public bool CollisionCheck(BoundingSphere collision)
        {
            if (collision.Intersects(colSphere))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Checks the keyboard input for any attacks that the hero has fired
        //Fires attack if correct key is pressed/ammo is available
        public void FireAttack()
        {
            //Current key state for this frame called in UpdateMovement()
            if(keyPressed(Keys.Space, Buttons.RightTrigger, false))
            {
                //If we have ammo left
                if (numAttacks < GameConstants.heroNumAttacks)
                {
                    //Fire Attack
                    onScreenAttacks[numAttacks] = new HeroAttack(position, rotation, heroAttack);
                    numAttacks++;

                    //Play Sound Effect
                    Game1.heroAttackSound.Play();
                }
            }
        }

        //Handle movement for the hero character
        public void UpdateMovement()
        {
            //Get the new keyboard state
            currentKeyState = Keyboard.GetState();
            currentPadState = GamePad.GetState(PlayerIndex.One);

            Vector3 charMovement = Vector3.Zero;
            bool moveUp = false, moveDown = false, moveLeft = false, moveRight = false;
            float currentCharRotation = rotation.Z;
            int goalRotationAngle = (int)rotation.Z;

            if (keyPressed(Keys.Up, Buttons.LeftThumbstickUp, true)) moveUp = true;
            if (keyPressed(Keys.Down, Buttons.LeftThumbstickDown, true)) moveDown = true;
            if (keyPressed(Keys.Left, Buttons.LeftThumbstickLeft, true)) moveLeft = true;
            if (keyPressed(Keys.Right, Buttons.LeftThumbstickRight, true)) moveRight = true;

            if ((moveUp) && !(moveDown))
            {
                if (!(moveLeft || moveRight))
                {
                    goalRotationAngle = 180;
                    charMovement.Y += moveSpeed;
                }
                else
                {
                    if (moveLeft)
                    {
                        goalRotationAngle = 225;
                        charMovement.X -= moveSpeed;
                        charMovement.Y += moveSpeed;
                    }
                    else if (moveRight)
                    {
                        goalRotationAngle = 135;
                        charMovement.X += moveSpeed;
                        charMovement.Y += moveSpeed;
                    }
                }
            }

            if ((moveDown) && !(moveUp))
            {
                if (!(moveLeft || moveRight))
                {
                    goalRotationAngle = 0;
                    charMovement.Y -= moveSpeed;
                }
                else
                {
                    if (moveLeft)
                    {
                        goalRotationAngle = 315;
                        charMovement.X -= moveSpeed;
                        charMovement.Y -= moveSpeed;
                    }
                    else if (moveRight)
                    {
                        goalRotationAngle = 45;
                        charMovement.X += moveSpeed;
                        charMovement.Y -= moveSpeed;
                    }
                }
            }

            if ((moveLeft || moveRight) && (!moveUp && !moveDown))
            {
                if (moveLeft)
                {
                    goalRotationAngle = 275;
                    charMovement.X -= moveSpeed;
                }
                else if (moveRight)
                {
                    goalRotationAngle = 90;
                    charMovement.X += moveSpeed;
                }
            }

            //Move the character
            if (charMovement != Vector3.Zero)
            {
                if (currentCharRotation != MathHelper.ToRadians(goalRotationAngle))
                {
                    rotation.Z += MathHelper.ToRadians(angleTurn(MathHelper.ToDegrees(currentCharRotation), goalRotationAngle));
                }

                //CheckBoundaries
                if (checkBoundaries(charMovement))
                {
                    position += charMovement;
                }
            }
        }

        //Work out the smallest distance between two angles and produce speed in return
        private float angleTurn(float initAngle, float finalAngle)
        {
            //Work out difference in angles from current to goal
            //target angle will return between -180 and 180
            float targetAngle = (((finalAngle - initAngle) + 180) % 360) - 180;
            float speed = targetAngle / GameConstants.heroRotSpeed;

            return speed;
        }

        //Check to make sure the player doesnt move outside of the boundaries of the game
        private bool checkBoundaries(Vector3 charMovement)
        {
            //The new position the hero will be in if he moves
            Vector3 newPosition = position + charMovement;

            //Check horizontal limits
            if (newPosition.X > horBoundLimit || newPosition.X < -horBoundLimit)
            {
                return false;
            }

            //Check vertial limits
            int endRoomPosition = ((GameConstants.currentRoom) * GameConstants.roomSize) - verBoundLimitBack;
            int endBuffer = (GameConstants.currentRoom - 1) * 20;
            int frontBuffer = endBuffer - (GameConstants.currentRoom - 1) * 3;

            if ((newPosition.Y < (endRoomPosition - GameConstants.roomSize + frontBuffer)) || (newPosition.Y > endRoomPosition + endBuffer))
            {
                //The character cant move out the room unless the door is opened
                if (doorOpen && (newPosition.Y > endRoomPosition))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        //Check the health of the character, called from the update method within Game1
        //If the hero character has 0 or less health then return true, else return false
        public bool checkHealth()
        {
            if (health <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Check to see if a key or button has been pressed
        private bool keyPressed(Keys keyPress, Buttons buttonPress, bool keyHeld)
        {
            //keyHeld will be passed as true if we want to check movement for holding the key down and not just 
            //pressing it once, it will be passed in false otherwise.
            if (keyHeld)
            {
                if (currentKeyState.IsKeyDown(keyPress) || currentPadState.IsButtonDown(buttonPress))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if ((currentKeyState.IsKeyDown(keyPress) && oldKeyState.IsKeyUp(keyPress)) || (currentPadState.IsButtonDown(buttonPress) && oldPadState.IsButtonUp(buttonPress)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Draw Model
        public void DrawModel(Matrix world, Matrix view, Matrix projection, Camera cam)
        {
            foreach (ModelMesh mesh in charModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //Lighting
                    effect.LightingEnabled = true;
                    //Ambient Light - All round light source
                    effect.AmbientLightColor = new Vector3(30, 30, 30);

                    //Apply the world, view and projection matrices.
                    //Update world view depending on the characters values
                    //Translate to origin first then scale, then rotate then translate back
                    world = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateScale(scale) * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateTranslation(position);

                    effect.World = world;
                    effect.View = cam.viewMatrix;
                    effect.Projection = cam.projectionMatrix;
                }

                mesh.Draw();
            }
        }
    }
}