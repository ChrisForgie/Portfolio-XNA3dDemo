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
    class BossAttack : Microsoft.Xna.Framework.Game
    {
        //This will be the attack of the final boss (Room 3 - Giant Robot)
        //This class is very similar to HeroAttack
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
        public BossAttack(Vector3 ba_position, Vector3 ba_rotation)
        {
            //Link the attack to a 3D model
            //Getting around this, by assigning (loading) the model in Game1 and just calling it here
            attackModel = Game1.bossAttack;

            //Set up the attack
            position = ba_position;
            rotation = ba_rotation;
            power = GameConstants.bossAttackPower;
            scale = 2f;
            isActive = true;

            speed = GameConstants.bossAttackSpeed;

            //Raise the z-position so the attacks are "flying" across the floor
            position.Z -= 20.0f;
            rotation.Z -= MathHelper.ToRadians(90);

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
                if ((position.Y > (GameConstants.vertBoundary + ((GameConstants.currentRoom - 1) * 175))) || (position.Y < (-GameConstants.vertBoundaryBack + ((GameConstants.currentRoom - 1) * 175))))
                {
                    //Outside either side of the screeen
                    //Remove the attack
                    isActive = false;
                }
                if (position.Y < 300)
                {
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
                    //Error with texture co-ordinates so texture enabled is false
                    effect.TextureEnabled = false;
                    effect.Texture = GameConstants.bossAttackTexure;

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
