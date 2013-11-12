using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
    public class ComboCounter : GameObject
    {
        Player player;
        Vector2 comboPositionLeft = new Vector2(37, 78);
        Vector2 comboPositionRight = new Vector2(113, 78);
        int comboScale = 2;
        int comboScrollSpeed = 5;
        int initialWidth;

        bool scrolling = true;
		ScaleToAction m_bump;

        public ComboCounter(Player player)
        {
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("multiplier_x1"), m_transform);
            m_sprite.Transform.Position = new Vector2(Game1.GameboyWidth / 2, Game1.GameboyHeight / 2 + 20);
            m_sprite.Transform.Scale = new Vector2(0);

			m_bump = new ScaleToAction(Program.TheGame, m_transform, new Vector2(2.3f, 2.3f), 1);
			m_bump.Interpolator = new PBounceInterpolation(0);
			m_bump.Timer.Interval = 0.2f;


            this.player = player;
            initialWidth = 37;
            m_sprite.SourceRectangle = new Rectangle(0, 0, 0, (int)m_sprite.Height);
            m_sprite.Origin = new Vector2(0f, 0.5f);
        }

        public void RestartScrolling()
        {
            scrolling = true;
            m_sprite.SourceRectangle = new Rectangle(0, 0, 0, (int)m_sprite.Height);
        }

		public void SetCounter(int combo, bool triggerAnimation)
		{
			if (combo == 0)
			{
				m_transform.SclX = 0;
				return;
			}

			if (combo > Game1.scoreBorder.ScoreMultiplierMax)
				combo = Game1.scoreBorder.ScoreMultiplierMax;

			string comboIndex = "multiplier_x" + player.ComboCount.ToString();

			// TODO Limiter le counter par rapport au sprite
			//
			if (combo > Game1.scoreBorder.ScoreMultiplierMax)
			{
				m_sprite.SpriteSheet = TextureLibrary.GetSpriteSheet("multiplier_x" + Game1.scoreBorder.ScoreMultiplierMax);
			}
			else
			{
				m_sprite.SpriteSheet = TextureLibrary.GetSpriteSheet(comboIndex);
			}

			if (triggerAnimation)
			{
				m_bump.StartScale = new Vector2(2.1f, 2.1f);
				m_bump.Restart();
			}
		}

        public override void Update()
        {
            if (scrolling)
            {
                if (! player.IsToLeft)
                {
                    if ((int)m_sprite.Width < initialWidth)
                        m_sprite.SourceRectangle = new Rectangle(0, 0, (int)(m_sprite.Width + comboScrollSpeed), (int)m_sprite.Height);
                    else
                        scrolling = false;
                }
                else
                {
                    if ((int)m_sprite.Width < initialWidth)
                        m_sprite.SourceRectangle = new Rectangle(m_sprite.SpriteSheet.Texture.Width-(int)(m_sprite.Width + comboScrollSpeed), 0, (int)(m_sprite.Width + comboScrollSpeed), (int)m_sprite.Height);
                    else
                        scrolling = false;
                }
            }

			m_bump.Update();

        }

        public override void Draw()
        {
            if (player.ComboCount > 1 && player.IsFalling)
            {
                if (player.IsToLeft)
                {
                   //m_sprite.Transform.Position = comboPositionRight;
                    m_sprite.Origin = new Vector2(0.2f, 0.5f);
                }
                else
                {
                   //m_sprite.Transform.Position = comboPositionLeft;
                    m_sprite.Origin = new Vector2(0.95f, 0.5f);
                }

                m_sprite.Draw();
            }
        }

    }
}
