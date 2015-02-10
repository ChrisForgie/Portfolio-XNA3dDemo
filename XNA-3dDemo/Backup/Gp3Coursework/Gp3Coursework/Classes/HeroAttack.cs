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
    class HeroAttack : Microsoft.Xna.Framework.Game
    {
        //This will be the primary attack of the hero character (the player)
        //Variables for this attack
        private Vector3 position, rotation;
        private float power, speed, scale;
        private bool isActive;
        private Model attackModel;
        private BoundingSphere colSphere;

        //Getter/Setter Methods
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
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }
        public BoundingSphere ColSphere
        {
            get { return colSphere; }
        }


        //Constructor for the attack
        public HeroAttack(Vector3 ha_position, Vector3 ha_rotation)
        {
            //Link the attack to a 3D model
            //Can't seem to solve error with below code
            //attackModel = Content.Load<Model>("Models\\BulletAttack");
            //Getting around this, by assigning (loading) the model in Game1 and just calling it here
            attackModel = Game1.heroAttack;

            //Set up the attack
            position = ha_position;
            rotation = ha_rotation;
            power = GameConstants.heroAttackPower;
            scale = 0.04f;
            isActive = true;

            speed = GameConstants.heroAttackSpeed;

            //Raise the z-position so the bullets are "flying" across the floor
            position.Z += 5.0f;
            //Fix rotation (Object rotation not initially straight)
            rotation.Z -= MathHelper.ToRadians(90f);

            //Create a bounding sphere for the attack
            createBoundingSphere();
        }

        //Create a bounding sphere around the character
        public void createBoundingSphere()
        {
            colSphere = new BoundingSphere();

            foreach (ModelMesh mesh in attackModel.Meshes)
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
            colSphere.Center = position;
            colSphere.Radius = 5f;
        }

        //Update function for the attack
        public void Update()
        {
            //Make sure the bullet is still active
            if (isActive)
            {
                //Check if we are still within the boundaries
                if ((position.X > GameConstants.horBoundary) || (position.X < -GameConstants.horBoundary))
                {
                    //Outside either side of the screeen
                    //Remove the attack
                    isActive = false;
                }

                //If we're in the final room, fix a glitch with not being able to fire from the back of the room
                //so extend the vertical boundary back a little
                if (GameConstants.currentRoom == 3)
                {
                    GameConstants.vertBoundaryBack += 5;
                }
                if ((position.Y > (GameConstants.vertBoundary +((GameConstants.currentRoom - 1) * 175))) || (position.Y < (-GameConstants.vertBoundaryBack + +((GameConstants.currentRoom - 1) * 175))))
                {
                    //Outside either side of the screeen
                    //Remove the attack
                    isActive = false;
                }

                //Move the attack in a straight line from it's rotation
                position.X += speed * ((float)Math.Cos(rotation.Z));
                position.Y += speed * ((float)Math.Sin(rotation.Z));

                //Update bounding sphere position
                colSphere.Center = position;
            }

        }

        // Draw the attack
        public void DrawModel(Matrix world, Matrix view, Matrix projection, Camera cam)
        {
            foreach (ModelMesh mesh in attackModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //effect.TextureEnabled = false; 
                    effect.TextureEnabled = true;
                    effect.Texture = GameConstants.heroAttackTexure;

                    //Lighting
                    //effect.EnableDefaultLighting();
                    //effect.LightingEnabled = true; // turn on the lighting subsystem.
                    //Ambient Light - All round light source
                    //effect.AmbientLightColor = new Vector3(30, 30, 30);

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
