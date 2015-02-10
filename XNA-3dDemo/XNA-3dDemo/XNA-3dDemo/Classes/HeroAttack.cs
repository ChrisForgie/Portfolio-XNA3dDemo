using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

/* This class is an object of a hero attack, once the
 * object is created, it maintains a bounding sphere for the object
 * and checks to make sure the object is still within the game (current
 * room). Collision between the attack and enemies is dealt within the
 * Game1 class.
 * 
 * Chris Forgie 2015
 * XNA3dDemo
 */ 

namespace XNA3dDemo
{
    internal class HeroAttack : Microsoft.Xna.Framework.Game
    {
        //This will be the primary attack of the hero character (the player)
        private Vector3 position, rotation;

        private float power, speed, scale;
        private bool isActive;
        private Model attackModel;
        private BoundingSphere colSphere;

        //Encapsulation methods
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

        public new bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public BoundingSphere ColSphere
        {
            get { return colSphere; }
        }

        //Constructor for the attack
        public HeroAttack(Vector3 ha_position, Vector3 ha_rotation, Model attackMod)
        {
            //Link the attack to a 3D model
            attackModel = attackMod;

            //Set up the attack
            position = ha_position;
            rotation = ha_rotation;
            power = GameConstants.heroAttackPower;
            scale = 0.04f;
            isActive = true;

            speed = GameConstants.heroAttackSpeed;

            //Raise the z-position so the bullets are "flying" across the floor
            position.Z += 5.0f;
            //Change rotation (Object rotation not initially straight)
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
                    //Remove the attack
                    isActive = false;
                }

                //If we're in the final room, fix a glitch with not being able to fire from the back of the room
                //so extend the vertical boundary back a little
                float backWallBoundary = GameConstants.vertBoundaryBack;
                if (GameConstants.currentRoom == 3)
                {
                    backWallBoundary += 5;
                }

                if ((position.Y > (GameConstants.vertBoundary + ((GameConstants.currentRoom - 1) * 175))) || (position.Y < (-backWallBoundary + ((GameConstants.currentRoom - 1) * 175))))
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
                    effect.TextureEnabled = true;
                    effect.Texture = GameConstants.heroAttackTexure;

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