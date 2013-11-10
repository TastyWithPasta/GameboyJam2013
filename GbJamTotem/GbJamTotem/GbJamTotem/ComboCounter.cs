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
        Vector2 comboPositionLeft = new Vector2(37, 72);
        Vector2 comboPositionRight = new Vector2(113, 72);
        int comboScale = 2;
        int comboScrollSpeed = 2;
        int initialWidth;

        public ComboCounter(Player player)
        {
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("multiplier_x1"), m_transform);
            m_sprite.Transform.Position = new Vector2(Game1.GameboyWidth / 2, Game1.GameboyHeight / 2);
            m_sprite.Transform.Scale = new Vector2(0);

            this.player = player;
            initialWidth = 32;
            m_sprite.SourceRectangle = new Rectangle(0, 0, 0, (int)m_sprite.Height);
            m_sprite.Origin = new Vector2(0f, 0.5f);
        }

        public override void Update()
        {
            if (Game1.kbs.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
            {
                if ((int)m_sprite.Width <= initialWidth)
                    m_sprite.SourceRectangle = new Rectangle(0,0, (int)(m_sprite.Width+comboScrollSpeed), (int)m_sprite.Height);
            }
			if (player.ComboCount == 0)
			{
				m_transform.SclX = 0;
				return;
			}

			if (player.ComboCount > Game1.scoreBorder.ScoreMultiplierMax)
                player.ComboCount = Game1.scoreBorder.ScoreMultiplierMax;

            string comboIndex = "multiplier_x" + player.ComboCount.ToString();

            // TODO Limiter le counter par rapport au sprite
            //
            if (player.ComboCount > Game1.scoreBorder.ScoreMultiplierMax)
            {
                m_sprite.SpriteSheet = TextureLibrary.GetSpriteSheet("multiplier_x"+Game1.scoreBorder.ScoreMultiplierMax);
            }
            else
            {
                m_sprite.SpriteSheet = TextureLibrary.GetSpriteSheet(comboIndex);
            }
        }

        public override void Draw()
        {
            if (player.ComboCount > 1 && player.IsFalling)
            {
                m_sprite.Transform.Scale = new Vector2(comboScale);

                if (player.IsToLeft)
                {
                   //m_sprite.Transform.Position = comboPositionRight;
                    m_sprite.Origin = new Vector2(0, 0.5f);
                }
                else
                {
                   //m_sprite.Transform.Position = comboPositionLeft;
                    m_sprite.Origin = new Vector2(1, 0.5f);
                }

                m_sprite.Draw();
            }
        }

    }
}
