using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GbJamTotem
{
    public class Player : GameObject
    {

        MoveToStaticAction m_leftToRight;
        MoveToStaticAction m_RightToLeft;

        bool isToLeft;

        Vector2 m_initialPosition;

        public Vector2 InitialPosition
        {
            get { return m_initialPosition; }
            set { m_initialPosition = value; }
        }

        public Player(Vector2 initialPosition)
            : base()
        {
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("player_sprite"), m_transform);
            m_initialPosition = initialPosition;
            this.Transform.Position = initialPosition;
            isToLeft = true;

            m_leftToRight = new MoveToStaticAction(Program.TheGame, m_transform, new Vector2(-m_initialPosition.X, this.Transform.PosY), 1);
            m_leftToRight.StartPosition = new Vector2(m_initialPosition.X, m_initialPosition.Y);
            m_leftToRight.Interpolator = new PSmoothstepInterpolation();
            m_leftToRight.Timer.Interval = 0.2f;

            m_RightToLeft = new MoveToStaticAction(Program.TheGame, m_transform, new Vector2(m_initialPosition.X, this.Transform.PosY), 1);
            m_RightToLeft.StartPosition = new Vector2(-m_initialPosition.X, m_initialPosition.Y);
            m_RightToLeft.Interpolator = new PSmoothstepInterpolation();
            m_RightToLeft.Timer.Interval = 0.2f;
        }

        public override void Update()
        {

            if (Game1.kbs.IsKeyDown(Keys.Space) && Game1.old_kbs.IsKeyUp(Keys.Space)
                && !m_leftToRight.IsActive && !m_RightToLeft.IsActive && isToLeft)
            {
                m_leftToRight.Restart();
                isToLeft = false;
            }

            if (Game1.kbs.IsKeyDown(Keys.Space) && Game1.old_kbs.IsKeyUp(Keys.Space)
                && !m_RightToLeft.IsActive && !m_leftToRight.IsActive && !isToLeft)
            {
                m_RightToLeft.Restart();
                isToLeft = true;
            }


            m_leftToRight.Update();
            m_RightToLeft.Update();

            Game1.old_kbs = Game1.kbs;

        }

        public override void Draw()
        {
            m_sprite.Draw();
        }

    }
}
