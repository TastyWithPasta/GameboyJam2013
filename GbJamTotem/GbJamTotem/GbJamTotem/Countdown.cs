using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GbJamTotem
{
    public class Countdown : GameObject
    {
        float timer;
        float timeCounter = 3;
        bool activeTimer;

        public Countdown()
        {
            timer = 0;
            activeTimer = false;
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("counter3"), m_transform);
            m_sprite.Transform.Position = new Vector2(Game1.GameboyWidth/2, Game1.GameboyHeight/2);
            m_sprite.Transform.Scale = new Vector2();
        }


        public void activateCountdown()
        {
            activeTimer = true;
        }

        public void resetCountdown()
        {
            timer = 0;
            activeTimer = false;
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("counter3"), m_transform);
        }

        public override void Update()
        {
            // TODO
            // Activer le timer en dehors du countdown
            //
            if (Game1.kbs.IsKeyDown(Keys.T) && Game1.old_kbs.IsKeyUp(Keys.T))
            {
                activeTimer = true;
                m_sprite.Transform.Scale = new Vector2(1);
            }

            if(activeTimer){

                timer = (float)Program.TheGame.ElapsedTime;

                //timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                timeCounter -= timer;

                if (timeCounter < 2)
                {
                    m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("counter2"), m_transform);
                }

                if (timeCounter < 1)
                {
                    m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("counter1"), m_transform);
                }

                if (timeCounter < 0)
                {
                    activeTimer = false;
                    m_sprite.Transform.Scale = new Vector2(0);
                }
            }

        }

        public override void Draw()
        {
            m_sprite.Draw();
        }

    }
}
