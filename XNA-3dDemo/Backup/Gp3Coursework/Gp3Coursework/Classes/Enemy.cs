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
    class Enemy : Microsoft.Xna.Framework.Game
    {
        //Variables for the main character
        private Vector3 position, rotation;
        private float scale, health, initHealth, speed;
        private BossAttack[] onScreenBossAttacks;
        private bool isAlive, isBoss;
        private Model charModel;
        private Texture2D charTex;

        private BoundingSphere colSphere;

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
        public float InitHealth
        {
            get { return initHealth; }
        }
        public float Speed
        {
            get { return speed; }
        }
        public bool IsAlive
        {
            get { return isAlive; }
            set { isAlive = value; }
        }
        public bool IsBoss
        {
            get { return isBoss; }
            set { isBoss = value; }
        }
        public BossAttack[] OnScreenBossAttacks
        {
            get { return onScreenBossAttacks; }
            set { onScreenBossAttacks = value; }
        }

        public BoundingSphere ColSphere
        {
            get { return colSphere; }
        }

        //Constructor for the character
        public Enemy(Model m_charModel, Vector3 v3_position, Vector3 v3_rotation, float f_scale, float f_health, float f_speed, Texture2D t2_charTex)
        {
            //Assign the model of this character class to the one which was passed in
            charModel = m_charModel;
            //Assign position and rotation
            position = v3_position;
            rotation = v3_rotation;
            //Assign scale
            scale = f_scale;
            
            //Speed of enemy - passed in
            speed = f_speed;

            //Assign Texture
            charTex = t2_charTex;
            
            //Health can vary betwen enemy types so assign it to the health that we pass in
            //Set the initial health as well
            initHealth = f_health;
            health = initHealth;

            isAlive = true;

            //If this enemy is the final boss then we will declare that in Game1 (LoadRoom3)
            isBoss = false;
            onScreenBossAttacks = new BossAttack[GameConstants.bossNumAttacks];

            //Create a bounding sphere for the enemy
            createBoundingSphere();
        }

        //Create a bounding sphere around the enemy
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
            colSphere.Radius = scale * 4f;
        }

        //Check for collisions between the enemy and the hero's attacks
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


        public void Update()
        {
            //NOTE: Enemy movement done in Game1

            //Update bounding sphere position and scale
            colSphere.Center = (position + new Vector3(0, 0, 5));

            //Fire his attack and update his collision sphere
            if (isBoss)
            {
                //Fire Attack
                if (onScreenBossAttacks[0] == null)
                {
                    onScreenBossAttacks[0] = new BossAttack(position, rotation);

                    //Play sound effect
                    Game1.bossAttackSound.Play();
                }

                //Update the collision sphere to better fit the boss
                colSphere.Center = (position + new Vector3(0, 0, -20));
                colSphere.Radius = scale * 5;
            }
        }

        // Draw Model
        public void DrawModel(Matrix world, Matrix view, Matrix projection, Camera cam)
        {
            foreach (ModelMesh mesh in charModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.Texture = charTex;

                    //effect.EnableDefaultLighting();
                    effect.LightingEnabled = true; // turn on the lighting subsystem.
                    //Ambient Light - All round light source
                    effect.AmbientLightColor = new Vector3(30, 30, 30);

                    //Fog
                    //effect.FogEnabled = true;
                    //effect.FogColor = Color.CornflowerBlue.ToVector3(); // For best results, ake this color whatever your background is.
                    //effect.FogStart = 14.75f;
                    //effect.FogEnd = 15.75f;

                    //Apply the world, view and projection matrices.

                    //Update world view depending on the characters values
                    //Translate to origin first then scale, then rotate then translate back
                    world = Matrix.CreateTranslation(new Vector3(0, 0, 0)) * Matrix.CreateScale(scale) * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateTranslation(position);

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
