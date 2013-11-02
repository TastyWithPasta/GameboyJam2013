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

        MoveToTransform m_leftToRight;
        MoveToTransform m_rightToLeft;
        MoveToTransform m_climbing;
        SingleActionManager m_actionManager;

        Transform m_playerTransform;
        Transform m_leftTransform;
        Transform m_rightTransform;

        bool isToLeft;
        bool canClimb;
        bool isPlaying;

        Vector2 m_initialPosition;
        Transform m_climbingPosition;

        public Vector2 InitialPosition
        {
            get { return m_initialPosition; }
            set { m_initialPosition = value; }
        }

        public Player(Vector2 initialPosition, Transform climbingPosition)
            : base()
        {
            m_initialPosition = initialPosition;
            m_climbingPosition = climbingPosition;
            isToLeft = true;
            canClimb = true;
            isPlaying = false;

            m_actionManager = new SingleActionManager();

            m_playerTransform = new Transform(m_transform, true);
            m_leftTransform = new Transform(m_transform, true);
            m_rightTransform = new Transform(m_transform, true);

            m_playerTransform.PosX = initialPosition.X;
            m_leftTransform.Position = initialPosition;
            m_rightTransform.Position = -initialPosition;

            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("player_sprite"), m_playerTransform);

            m_leftToRight = new MoveToTransform(Program.TheGame, m_playerTransform, m_leftTransform, m_rightTransform, 1);
            m_leftToRight.Interpolator = new PSmoothstepInterpolation();
            m_leftToRight.Timer.Interval = 0.2f;

            m_rightToLeft = new MoveToTransform(Program.TheGame, m_playerTransform, m_rightTransform, m_leftTransform, 1);
            m_rightToLeft.Interpolator = new PSmoothstepInterpolation();
            m_rightToLeft.Timer.Interval = 0.2f;

            m_climbing = new MoveToTransform(Program.TheGame, m_transform, m_transform, m_climbingPosition, 1);
           
            m_climbing.Interpolator = new PSmoothstepInterpolation();
            m_climbing.Timer.Interval = 0.2f;
        }

        public override void Update()
        {

            if (Game1.kbs.IsKeyDown(Keys.Space) && Game1.old_kbs.IsKeyUp(Keys.Space)
                && !m_leftToRight.IsActive && !m_rightToLeft.IsActive && isToLeft)
            {
                m_actionManager.StartNew(m_leftToRight);
                isToLeft = false;
            }

            if (Game1.kbs.IsKeyDown(Keys.Space) && Game1.old_kbs.IsKeyUp(Keys.Space)
                && !m_rightToLeft.IsActive && !m_leftToRight.IsActive && !isToLeft)
            {
                m_actionManager.StartNew(m_rightToLeft);
                isToLeft = true;
            }

            // Activate climbing animation
            //
            if (Game1.kbs.IsKeyDown(Keys.C)
                && !m_climbing.IsActive && canClimb)
            {
                m_actionManager.StartNew(m_climbing);
                canClimb = false;
            }

            if (!canClimb && this.Transform.PosY >= m_climbingPosition.PosY)
            {
                isPlaying = true;
            }

            if (isPlaying)
            {
                this.Transform.PosY = this.Transform.PosY + 1;

                if (this.Transform.PosY == 0)
                {
                    isPlaying = false;
                    canClimb = true;
                }
            }


            m_actionManager.Update();

            Game1.old_kbs = Game1.kbs;

        }

        public override void Draw()
        {
            m_sprite.Draw();
        }

    }
}
