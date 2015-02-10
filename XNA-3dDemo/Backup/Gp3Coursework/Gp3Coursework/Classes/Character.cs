using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gp3Coursework
{
    class Character : Microsoft.Xna.Framework.Game
    {
        //Variables for the main character
        private Vector3 position, rotation;
        private float scale, health, rotAngle, moveSpeed;
        private int numAttacks;
        private HeroAttack[] onScreenAttacks;
        private Model charModel;
        //Bounding Sphere
        private BoundingSphere colSphere;

        //Keyboard States
        KeyboardState oldKeyState, currentKeyState;
        //GamePad States (Xbox360 Controller)
        GamePadState oldPadState, currentPadState;

        //Boundary position for movement
        //Since game takes place on a square, the same position would be used for left and right limits
        private int horBoundLimit, verBoundLimit;

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

        //Constructor for the character
        public Character(Model m_charModel, Vector3 v3_position, Vector3 v3_rotation, float f_scale)
        {
            //Assign the model of this character class to the one which was passed in
            charModel = m_charModel;
            //Assign position and rotation
            position = v3_position;
            rotation = v3_rotation;
            //Initial rotation of the Z-axis, the only axis that should rotate during run-time
            rotAngle = GameConstants.heroInitRotAngle;
            //Assign scale
            scale = f_scale;

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
            //Check if we have enough health to continue
            //Update the movement
            UpdateMovement();

            //Check if we have fired any attacks
            FireAttack();

            //Handle the old keyboard/gamepad state
            oldKeyState = currentKeyState;
            oldPadState = currentPadState;

            //Update bounding sphere position and scale
            colSphere.Center = (position + new Vector3(0,0,5));
        }


        //Check for collisions between the character and enemies/other objects (Called in Game1)
        public bool CollisionCheck(BoundingSphere collision)
        {
            if(collision.Intersects(colSphere)){
                return true;
            }else{
                return false;
            }
        }


        //Checks the keyboard input for any attacks that the hero has fired
        //Fires attack if correct key is pressed/ammo is available
        public void FireAttack()
        {
            //Current key state for this frame called in UpdateMovement()
            if ((currentKeyState.IsKeyDown(Keys.Space) && oldKeyState.IsKeyUp(Keys.Space)) || (currentPadState.IsButtonDown(Buttons.RightTrigger) && oldPadState.IsButtonUp(Buttons.RightTrigger)))
            {
                //If we have ammo left
                if (numAttacks < GameConstants.heroNumAttacks)
                {
                    //Fire Attack
                    onScreenAttacks[numAttacks] = new HeroAttack(position, rotation);
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

            //Move left and right
            if ((currentKeyState.IsKeyDown(Keys.Right)) || (currentPadState.IsButtonDown(Buttons.LeftThumbstickRight)))
            {
                //Rotate character to face this direction
                if (rotAngle != 90)
                {
                    //Work out difference in angles from current to goal
                    float speed = angleTurn(rotAngle, 90);
                    rotAngle += speed;
                    rotation.Z += MathHelper.ToRadians(speed);
                }
                if (position.X < horBoundLimit)
                {
                    position.X += moveSpeed;
                }
            }
            else if ((currentKeyState.IsKeyDown(Keys.Left)) || (currentPadState.IsButtonDown(Buttons.LeftThumbstickLeft)))
            {
                //Rotate character to face this direction
                if (rotAngle != 270)
                {
                    //Work out difference in angles from current to goal
                    float speed = angleTurn(rotAngle, 270);
                    rotAngle += speed;
                    rotation.Z += MathHelper.ToRadians(speed);
                }
                if (position.X > -horBoundLimit)
                {
                    position.X -= moveSpeed;
                }
            }

            //Move up and down
            if ((currentKeyState.IsKeyDown(Keys.Up)) || (currentPadState.IsButtonDown(Buttons.LeftThumbstickUp)))
            {
                //Rotate character to face this direction
                if (rotAngle != 180)
                {
                    //Work out difference in angles from current to goal
                    float speed = angleTurn(rotAngle, 180);
                    rotAngle += speed;
                    rotation.Z += MathHelper.ToRadians(speed);
                }

                //If we're moving left and right then move slower going up and down
                if ((currentKeyState.IsKeyDown(Keys.Right) || currentKeyState.IsKeyDown(Keys.Left)) || (currentPadState.IsButtonDown(Buttons.LeftThumbstickRight) || currentPadState.IsButtonDown(Buttons.LeftThumbstickLeft)))
                {
                    if (position.Y < (verBoundLimit + ((GameConstants.currentRoom - 1) * 175)))
                    {
                        position.Y += moveSpeed - 0.15f;
                    }
                }
                else
                {
                    if (position.Y < (verBoundLimit + ((GameConstants.currentRoom - 1) * 175)))
                    {
                        position.Y += moveSpeed;
                    }
                }
            }
            else if ((currentKeyState.IsKeyDown(Keys.Down)) || (currentPadState.IsButtonDown(Buttons.LeftThumbstickDown)))
            {
                //Rotate character to face this direction
                if (rotAngle != 360)
                {
                    //Work out difference in angles from current to goal
                    float speed = angleTurn(rotAngle, 360);
                    rotAngle += speed;
                    rotation.Z += MathHelper.ToRadians(speed);
                }

                //If we're moving left and right then move slower going up and down
                if ((currentKeyState.IsKeyDown(Keys.Right) || currentKeyState.IsKeyDown(Keys.Left)) || (currentPadState.IsButtonDown(Buttons.LeftThumbstickRight) || currentPadState.IsButtonDown(Buttons.LeftThumbstickLeft)))
                {
                    //Some issues with the back wall in room 3
                    if (GameConstants.currentRoom != 3)
                    {
                        if (position.Y > (-GameConstants.vertBoundaryBack + ((GameConstants.currentRoom - 1) * 175)))
                        {
                            position.Y -= moveSpeed - 0.15f;
                        }
                    }
                    else
                    {
                        if (position.Y > 310)
                        {
                            position.Y -= moveSpeed - 0.15f;
                        }
                    }
                }
                else
                {
                    //Fixes an issue with the back wall in room 3
                    if (GameConstants.currentRoom != 3)
                    {
                        if (position.Y > (-GameConstants.vertBoundaryBack + ((GameConstants.currentRoom - 1) * 175)))
                        {
                            position.Y -= moveSpeed;
                        }
                    }
                    else
                    {
                        if (position.Y > 310)
                        {
                            position.Y -= moveSpeed;
                        }
                    }
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


        // Draw Model
        public void DrawModel(Matrix world, Matrix view, Matrix projection, Camera cam)
        {
            foreach (ModelMesh mesh in charModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //effect.TextureEnabled = false; 

                    //Lighting
                    //effect.EnableDefaultLighting();
                    effect.LightingEnabled = true; // turn on the lighting subsystem.
                    //Ambient Light - All round light source
                    effect.AmbientLightColor = new Vector3(30, 30, 30);

                    //Fog
                    //effect.FogEnabled = true;
                    //effect.FogColor = Color.CornflowerBlue.ToVector3(); // For best results, ake this color whatever your background is.
                    //effect.FogStart = 9.75f;
                    //effect.FogEnd = 10.75f;

                    //Apply the world, view and projection matrices.
                    //Update world view depending on the characters values
                    //Translate to origin first then scale, then rotate then translate back
                    world = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateScale(scale) * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateTranslation(position);

                    effect.World = world;
                    //effect.View = view;
                    //effect.Projection = projection;
                    effect.View = cam.viewMatrix;
                    effect.Projection = cam.projectionMatrix;
                }

                mesh.Draw();
            }
        }


    }
}
