using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
    public class PowerUp : GameObject
    {

        bool isPickedUp;
        bool isToLeft;

        public PowerUp(bool isToLeft)
        {
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("power_up"), m_transform);
            isPickedUp = false;
            this.isToLeft = isToLeft;
        }

        public override void Update()
        {
            if (Game1.player.IsFalling && !isPickedUp)
            {
                if ( (isToLeft == Game1.player.IsToLeft || !isToLeft != Game1.player.IsToLeft)
                    && Math.Abs(this.m_transform.PosY - Game1.player.Transform.PosY) < 5)
                {
                    m_transform.Scale = new Vector2(0);
                    Game1.player.IsPoweredUp = true;
                    isPickedUp = true;
                }
            }
        }

        public override void Draw()
        {
            m_sprite.Draw();
        }

    }
}
