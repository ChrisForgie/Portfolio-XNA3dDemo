using Microsoft.Xna.Framework;

/* This class handles the cameras in the game, there
 * are two - one for top down (main view) and one for
 * third person. Once the cameras are set up with
 * their position, target, view and projection matrices;
 * this class handles the update. 
 * 
 * Chris Forgie 2015
 * XNA3dDemo
 */

namespace XNA3dDemo
{
    internal class Camera : Microsoft.Xna.Framework.Game
    {
        //Camera position and target
        private Vector3 position, target, thirdPersonPosition, thirdPersonTarget;

        public Matrix viewMatrix, projectionMatrix;
        public bool thirdPersonCam;
        private Character heroCharacter;

        //Getter/Setter Methods
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }

        public Camera(bool thirdPerson, Character heroChar)
        {
            if (thirdPerson)
            {
                thirdPersonCam = true;
                setUpThirdPersonCamera(heroChar);
            }
            else
            {
                //Default Camera (Main)
                thirdPersonCam = false;
                setUpCamera();
            }
        }

        public void setUpCamera()
        {
            position = new Vector3(0, -45, 55);
            target = new Vector3(0, 0, 0);

            viewMatrix = Matrix.Identity;
            //Paramaters of projectionMatrix:  Field Of View/Apsect Ratio/Near Plane/Far Plane
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(100.0f), 16 / 9, .5f, 300f);
        }

        public void setUpThirdPersonCamera(Character heroChar)
        {
            heroCharacter = heroChar;

            thirdPersonPosition = heroCharacter.Position - new Vector3(-5, 20, -10);
            thirdPersonTarget = heroCharacter.Position - new Vector3(-5, 0, 0);

            viewMatrix = Matrix.Identity;
            //Paramaters of projectionMatrix:  Field Of View/Apsect Ratio/Near Plane/Far Plane
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(100.0f), 16 / 9, .5f, 500f);
        }

        public void UpdateTarget(Vector3 newPos, Vector3 newTarget)
        {
            position = newPos;
            target = newTarget;
        }

        public void Update()
        {
            if (thirdPersonCam)
            {
                thirdPersonPosition = heroCharacter.Position - new Vector3(-5, 20, -10);
                thirdPersonTarget = heroCharacter.Position - new Vector3(-5, 0, 0);
            }

            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            if (thirdPersonCam)
            {
                viewMatrix = Matrix.CreateLookAt(thirdPersonPosition, thirdPersonTarget, Vector3.Up);
            }
            else
            {
                viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
            }
        }
    }
}