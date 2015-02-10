using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/* This class is used to create an enemy, handle 
 * collision and if the enemy is a boss then update
 * when he can attack. Enemies are created within the Game1
 * class, that class also handles whether they have collided
 * with an attack of the player.
 * 
 * Chris Forgie 2015
 * XNA3dDemo
 */

namespace XNA3dDemo
{
    internal class Enemy : Microsoft.Xna.Framework.Game
    {
        //Enemy character setup
        private Vector3 position, rotation;

        private float scale, health, initHealth, speed;
        private BossAttack[] onScreenBossAttacks;
        private bool isAlive, isBoss;
        private Model charModel;
        private Model bossAttackMod;
        private Texture2D charTex;

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
        public Enemy(Model m_charModel, Vector3 v3_position, Vector3 v3_rotation, float f_scale, float f_health, float f_speed, Texture2D t2_charTex, Model bossAttack)
        {
            //Assign the model of this character class to the one which was passed in
            charModel = m_charModel;
            position = v3_position;
            rotation = v3_rotation;
            scale = f_scale;
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

            //If the enemy is a boss, then an attack model will be passed in
            if (bossAttack != null)
            {
                bossAttackMod = bossAttack;
            }

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
            colSphere.Radius = scale * 5f;
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
                    onScreenBossAttacks[0] = new BossAttack(position, rotation, bossAttackMod);

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

                    effect.EnableDefaultLighting();
                    effect.LightingEnabled = true;
                    effect.AmbientLightColor = new Vector3(10, 10, 10);

                    world = Matrix.CreateTranslation(new Vector3(0, 0, 0)) * Matrix.CreateScale(scale) * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateTranslation(position);

                    effect.World = world;
                    effect.View = cam.viewMatrix;
                    effect.Projection = cam.projectionMatrix;
                }

                mesh.Draw();
            }
        }
    }
}