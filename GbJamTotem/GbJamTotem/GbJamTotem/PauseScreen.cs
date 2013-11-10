using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GbJamTotem
{
    public class PauseScreen : GameObject
    {

        bool isGamePaused;

        public bool IsGamePaused
        {
            get { return isGamePaused; }
            //set { isGamePaused = value; }
        }

        public PauseScreen()
        {
            isGamePaused = false;
        }

        public override void Update()
        {
            if (Game1.old_kbs.IsKeyDown(Keys.Enter) && Game1.kbs.IsKeyUp(Keys.Enter))
                isGamePaused = !isGamePaused;
        }

        public override void Draw()
        {
            if (isGamePaused)
            {
                Program.TheGame.SpriteBatch.Begin();
                Program.TheGame.SpriteBatch.DrawString(Game1.debugText, "PAUSE",
                    new Vector2(Program.TheGame.ScreenWidth / 2 - Game1.debugText.MeasureString("PAUSE").X / 2,
                        Program.TheGame.ScreenHeight / 2 - Game1.debugText.MeasureString("PAUSE").Y / 2),
                        Color.Black);
                Program.TheGame.SpriteBatch.End();
            }
        }


    }
}
