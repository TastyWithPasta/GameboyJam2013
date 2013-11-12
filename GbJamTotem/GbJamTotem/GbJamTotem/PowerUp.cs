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
        int indexTotemToPlacePowerUp;

        public bool IsPickedUp
        {
            get { return isPickedUp; }
        }

        public bool IsToLeft
        {
            get { return isToLeft; }
        }

        public int IndexTotemToPlacePowerUp
        {
            get { return indexTotemToPlacePowerUp; }
        }

        public PowerUp(int index, bool isToLeft)
        {
            indexTotemToPlacePowerUp = index;
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("power_up"), m_transform);
            isPickedUp = false;
            this.isToLeft = isToLeft;
        }

        public override void Update()
        {
            if (Game1.player.IsFalling && !isPickedUp)
            {
                if ( ( (Game1.player.SpriteTransform.PosX > 0 && m_transform.PosX > 0)
                    || (Game1.player.SpriteTransform.PosX < 0 && m_transform.PosX < 0)
                    )
                    && Math.Abs(this.m_transform.PosY - Game1.player.Transform.PosY) < 7)
                {
                    this.Hide();
                    Game1.player.IsPoweredUp = true;
					Game1.feedback_powerUp.Play();
                    isPickedUp = true;
                }
            }
        }

        public void Show()
        {
            m_transform.Scale = new Vector2(1);
        }

        public void Hide()
        {
            m_transform.Scale = new Vector2(0);
        }

        public override void Draw()
        {
            m_sprite.Draw();
        }

    }
}
