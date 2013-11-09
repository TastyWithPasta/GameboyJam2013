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

        public ComboCounter(Player player)
        {
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("multiplier_x1"), m_transform);
            m_sprite.Transform.Scale = new Vector2(0);
            this.player = player; 
        }

        public override void Update()
        {
            if (Game1.kbs.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
            {
                m_sprite.SourceRectangle = new Rectangle(0,0, (int)(m_sprite.Width/2), (int)m_sprite.Height);
            }
			if (player.ComboCount == 0)
			{
				m_transform.SclX = 0;
				return;
			}

			string comboIndex = "multiplier_x" + player.ComboCount.ToString();
			if (player.ComboCount > 3)
				player.ComboCount = 3;
			m_sprite.SpriteSheet = TextureLibrary.GetSpriteSheet("multiplier_x1");
        }

        public override void Draw()
        {
            if (player.ComboCount > 1 && player.IsFalling)
            {
                m_sprite.Transform.Scale = new Vector2(2);

                if (player.IsToLeft)
                {
                   m_sprite.Transform.Position = new Vector2(110, 72);
                }
                else
                {
                   m_sprite.Transform.Position = new Vector2(50, 72);
                }

                m_sprite.Draw();
            }
        }

    }
}
