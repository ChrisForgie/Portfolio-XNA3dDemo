using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/* This class allows for a default menu screen to be created.
 * Each screen is created with 3 buttons in mind although
 * through the constructor, they can be set up to be 1-3
 * buttons which is sufficient for each screen in the game.
 * This class also checks for keyboard/mouse/gamepad input and
 * passes back which if any buttons have been pressed to the Game1 
 * class which will act accordingly.
 * 
 * Chris Forgie 2015
 * XNA3dDemo
 */ 

namespace XNA3dDemo
{
    internal class MenuScreen
    {
        //Activity monitor
        private bool screenActive;

        private int optionSelected, numOptions;
        private int buttonSelected;

        //Textures
        private Texture2D menuBackground;

        private Texture2D button1, button1Hover;
        private Texture2D button2, button2Hover;
        private Texture2D button3, button3Hover;

        //Button placements
        private Rectangle button1Place, button2Place, button3Place;

        //Keyboard, gamepad and mouse states
        private KeyboardState oldKeyState, currentKeyState;

        private MouseState oldMouseState, currentMouseState;
        private GamePadState oldPadState, currentPadState;

        //Constructor to set up a menu screen
        public MenuScreen(Texture2D menuBackground, Texture2D button1, Texture2D button1Hover, Texture2D button2, Texture2D button2Hover, Texture2D button3, Texture2D button3Hover)
        {
            this.menuBackground = menuBackground;
            this.button1 = button1;
            this.button1Hover = button1Hover;
            this.button2 = button2;
            this.button2Hover = button2Hover;
            this.button3 = button3;
            this.button3Hover = button3Hover;

            screenActive = true;
            buttonSelected = 0; //Default - no button clicked
            optionSelected = 1;
            numOptions = 1;

            //Set the number of options to how many buttons are available
            if (button2 != null) numOptions++;
            if ((button2 != null) && (button3 != null)) numOptions++;

            //Get keyboard, gamepad and mouse states
            oldKeyState = Keyboard.GetState();
            oldPadState = GamePad.GetState(PlayerIndex.One);
            oldMouseState = Mouse.GetState();
        }

        public void setButtonPlace(Rectangle rect1, Rectangle rect2, Rectangle rect3)
        {
            if (rect1 != null) button1Place = rect1;
            if (!rect2.IsEmpty) button2Place = rect2;
            if (!rect3.IsEmpty) button3Place = rect3;
        }

        //Get the state of the screen i.e. active or not
        public bool ScreenActive
        {
            get { return screenActive; }
            set { screenActive = value; }
        }

        //Return which button has been chosen if one is selected
        public int ButtonSelected
        {
            get { return buttonSelected; }
        }

        public void Update(KeyboardState kCurrState, KeyboardState kOldState, GamePadState gCurrState, GamePadState gOldState, MouseState mCurrState, MouseState mOldState)
        {
            //Take the key, pad and mouse states from the Game1 class
            currentKeyState = kCurrState;
            oldKeyState = kOldState;
            currentPadState = gCurrState;
            oldPadState = gOldState;
            currentMouseState = mCurrState;
            oldMouseState = mOldState;

            if (screenActive)
            {
                if (buttonSelected != 0) buttonSelected = 0;
                checkInputChange();
            }
        }

        //Check for any input in the menu
        private void checkInputChange()
        {
            //We have more than option to choose from in the menu
            if (numOptions > 1)
            {
                //Keyboard/Gamepad checking
                if (keyPressed(Keys.Down, Buttons.LeftThumbstickDown))
                {
                    if (optionSelected == numOptions)
                    {
                        optionSelected = 1;
                    }
                    else
                    {
                        optionSelected++;
                    }
                }

                if (keyPressed(Keys.Up, Buttons.LeftThumbstickUp))
                {
                    if (optionSelected == 1)
                    {
                        optionSelected = numOptions;
                    }
                    else
                    {
                        optionSelected--;
                    }
                }

                if ((keyPressed(Keys.Enter, Buttons.Start)) || (keyPressed(Keys.Space, Buttons.A)))
                {
                    buttonSelected = optionSelected;
                }

                //Mouse Checking
                Point mouseLocation = new Point(currentMouseState.X, currentMouseState.Y);

                if (button1Place.Contains(mouseLocation))
                {
                    optionSelected = 1;

                    if (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                    {
                        buttonSelected = optionSelected;
                    }
                }
                else if (button2Place.Contains(mouseLocation))
                {
                    optionSelected = 2;

                    if (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                    {
                        buttonSelected = optionSelected;
                    }
                }
                else if ((button3 != null) && button3Place.Contains(mouseLocation))
                {
                    optionSelected = 3;

                    if (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                    {
                        buttonSelected = optionSelected;
                    }
                }
            }
            else
            {
                //There is only 1 button
                if ((keyPressed(Keys.Enter, Buttons.Start)) || (keyPressed(Keys.Space, Buttons.A)))
                {
                    buttonSelected = optionSelected;
                }

                //Mouse Checking
                Point mouseLocation = new Point(currentMouseState.X, currentMouseState.Y);

                if (button1Place.Contains(mouseLocation))
                {
                    optionSelected = 1;

                    if (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                    {
                        buttonSelected = optionSelected;
                    }
                }
            }
        }

        //Check to see if a key or button has been pressed
        private bool keyPressed(Keys keyPress, Buttons buttonPress)
        {
            if ((currentKeyState.IsKeyDown(keyPress) && oldKeyState.IsKeyUp(keyPress)) || (currentPadState.IsButtonDown(buttonPress) && oldPadState.IsButtonUp(buttonPress)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(menuBackground, new Vector2(0, 0), Color.White);
            switch (optionSelected)
            {
                case 1:
                    spriteBatch.Draw(button1Hover, button1Place, Color.White);
                    if (button2 != null) spriteBatch.Draw(button2, button2Place, Color.White);
                    if (button3 != null) spriteBatch.Draw(button3, button3Place, Color.White);
                    break;

                case 2:
                    spriteBatch.Draw(button1, button1Place, Color.White);
                    if (button2 != null) spriteBatch.Draw(button2Hover, button2Place, Color.White);
                    if (button3 != null) spriteBatch.Draw(button3, button3Place, Color.White);
                    break;

                case 3:
                    spriteBatch.Draw(button1, button1Place, Color.White);
                    if (button2 != null) spriteBatch.Draw(button2, button2Place, Color.White);
                    if (button3 != null) spriteBatch.Draw(button3Hover, button3Place, Color.White);
                    break;
            }
        }
    }
}