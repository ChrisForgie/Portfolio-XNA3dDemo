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
    public class GameConstants : Microsoft.Xna.Framework.Game
    {
        //Global constant variables for the whole game
        public static int currentRoom = 1;

        //Constants for whole game
        public static int gameWindowHeight = 600;
        public static int gameWindowWidth = 800;
        public static int horBoundary = 80;
        public static int vertBoundary = 110;
        public static int vertBoundaryBack = 50;
        public static int numRoom1Enemies = 3;
        public static int numRoom2Enemies = 5;
        public static int numRoom3Enemies = 3;

        //Constants for the hero character
        public static Vector3 heroStartPosition = new Vector3(0, -40, 0);
        public static Vector3 heroStartRotation = new Vector3(MathHelper.ToRadians(90), 0, MathHelper.ToRadians(180));
        public static float heroInitRotAngle = 180;
        public static float heroRotSpeed = 25;
        public static float heroMoveSpeed = 0.55f;
        public static float heroStartScale = 1f;
        public static float heroHealth = 100f;
        public static int heroNumAttacks = 3;

        //Constants for the hero's attack
        public static float heroAttackPower = 20f;
        public static float heroAttackSpeed = 1f;
        public static Texture2D heroAttackTexure; //Defined in Game1

        //Constants for the boss' attack
        public static float bossAttackPower = 10f;
        public static float bossAttackSpeed = 0.8f;
        public static int bossNumAttacks = 1;
        public static Texture2D bossAttackTexure; //Defined in Game1

        //Enemy variables will usually vary and will be defined within the Game1 class
    }
}
