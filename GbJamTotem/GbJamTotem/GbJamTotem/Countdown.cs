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
		const float TimerMaxValue = 1.5f;

        float timer;
		float timeCounter = TimerMaxValue;
        bool activeTimer;
        bool countdownHasFinished;

        public bool CountdownHasFinished
        {
            get { return countdownHasFinished; }
        }

        public Countdown()
        {
            timer = 0;
            activeTimer = false;
            countdownHasFinished = false;
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("counter3"), m_transform);
            m_sprite.Transform.Position = new Vector2(Game1.GameboyWidth/2, Game1.GameboyHeight/2);
            m_sprite.Transform.Scale = new Vector2();
        }


        public void activateCountdown()
        {
            m_sprite.Transform.Scale = new Vector2(1);
            activeTimer = true;
        }

        public void resetCountdown()
        {
            timer = 0;
			timeCounter = TimerMaxValue;
            activeTimer = false;
            countdownHasFinished = false;
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("counter3"), m_transform);
        }

        public override void Update()
        {

            if(activeTimer){

                timer = (float)Program.TheGame.ElapsedTime;

                //timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                timeCounter -= timer;

                if (timeCounter < 1.0f)
                {
                    m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("counter2"), m_transform);
                }

                if (timeCounter < 0.5f)
                {
                    m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("counter1"), m_transform);
                }

                if (timeCounter < 0)
                {
                    activeTimer = false;
                    m_sprite.Transform.Scale = new Vector2(0);
                    countdownHasFinished = true;
                    Game1.scoreBorder.Slide(true);
                    Game1.mapBorder.Slide(true);
					
					OnFinish();
                }
            }

        }
		private void OnFinish()
		{
			Game1.dynamicMusic.PlayDynamicMusic();
		}
        public override void Draw()
        {
            m_sprite.Draw();
        }

    }
}
