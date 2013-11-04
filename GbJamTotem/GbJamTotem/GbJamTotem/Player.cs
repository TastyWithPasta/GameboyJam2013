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
		const float BasePlayerSpeed = 50.0f;
		const float BasePushForce = 4.0f;
		const float SlashDuration = 0.2f;
		const float CollisionDelayDuration = SlashDuration * 0.5f;
		const float MaxSpeedMultiplier = 3.0f;

		Totem m_totemInstance;

		Concurrent m_slashLR;
		MoveToTransform m_movementLR;
		DelayAction m_slashDelayLR;

		Concurrent m_slashRL;
		MoveToTransform m_movementRL;
		DelayAction m_slashDelayRL;

		MoveToTransform m_bounceLR;
		MoveToTransform m_bounceRL;

        MoveToTransform m_climbing;
        SingleActionManager m_actionManager;

        Transform m_playerTransform;
        Transform m_leftTransform;
        Transform m_rightTransform;
		Transform m_bounceTransform;

        bool isToLeft;
        bool canClimb;
        bool isPlaying;

		float m_speedMultiplier = 1; //Permet d'accélérer le rythme d'action du joueur

        Vector2 m_initialPosition;
        Transform m_climbingPosition;

		public float SpeedMultiplier
		{
			get { return m_speedMultiplier; }
			set { 
				//Changer le multiplicateur de vitesse implique que les animations vont plus vite
				m_speedMultiplier = value;
				m_movementLR.Timer.Interval = SlashDuration / value;
				m_slashDelayLR.Timer.Interval = CollisionDelayDuration / value;
				m_movementRL.Timer.Interval = SlashDuration / value;
				m_slashDelayRL.Timer.Interval = CollisionDelayDuration / value;
			}
		}

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
			m_bounceTransform = new Transform(m_transform, true);

            m_playerTransform.PosX = initialPosition.X;
            m_leftTransform.Position = initialPosition;
            m_rightTransform.Position = -initialPosition;

            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("perso_destroy"), m_playerTransform);

			///
			///	Slash left to right.
			///	Le mouvement est exécuté en parallèle avec une séquence délai + action.
			///	Autrement dit, vers la moitié de l'animation, il y a test de collision.
			///	Comme ça, le bloc peut être éjecté à un moment très précis, et il n'y a pas de collisions intempestives.
			///	On a: Concurrent(Mouvement + Sequence(delai => test collision))
			///
			m_movementLR = new MoveToTransform(Program.TheGame, m_playerTransform, m_leftTransform, m_rightTransform, 1);
			m_movementLR.Interpolator = new PSmoothstepInterpolation();
			m_movementLR.Timer.Interval = SlashDuration;
			m_slashDelayLR = new DelayAction(Program.TheGame, CollisionDelayDuration);
			MethodAction collisionLR = new MethodAction(delegate() { DoCollisionWithSections(false);  });
			Sequence slashActionLR = new Sequence(1);
			slashActionLR.AddAction(m_slashDelayLR);
			slashActionLR.AddAction(collisionLR);
			m_slashLR = new Concurrent(new PastaGameLibrary.Action[] { slashActionLR, m_movementLR });

			m_bounceLR = new MoveToTransform(Program.TheGame, m_playerTransform, m_bounceTransform, m_leftTransform, 1);
			m_bounceLR.Timer.Interval = SlashDuration - CollisionDelayDuration; //Le reste de temps après la collision

			///
			/// Slash Right To Left
			/// Même principe que l'autre sens
			///
			m_movementRL = new MoveToTransform(Program.TheGame, m_playerTransform, m_rightTransform, m_leftTransform, 1);
			m_movementRL.Interpolator = new PSmoothstepInterpolation();
			m_movementRL.Timer.Interval = SlashDuration;
			m_slashDelayRL = new DelayAction(Program.TheGame, CollisionDelayDuration);
			MethodAction collisionRL = new MethodAction(delegate() { DoCollisionWithSections(true);  });
			Sequence slashActionRL = new Sequence(1);
			slashActionRL.AddAction(m_slashDelayRL);
			slashActionRL.AddAction(collisionRL);
			m_slashRL = new Concurrent(new PastaGameLibrary.Action[] { slashActionRL, m_movementRL });

			m_bounceRL = new MoveToTransform(Program.TheGame, m_playerTransform, m_bounceTransform, m_rightTransform, 1);
			m_bounceRL.Timer.Interval = SlashDuration - CollisionDelayDuration; //Le reste de temps après la collision

            m_climbing = new MoveToTransform(Program.TheGame, m_transform, m_transform, m_climbingPosition, 1);
           
            m_climbing.Interpolator = new PSmoothstepInterpolation();
            m_climbing.Timer.Interval = 0.2f;
        }

		public void Initialise(Totem totem)
		{ 
			m_totemInstance = totem;
		}

		/// <summary>
		/// Détection et réponse à la collision entre le joueur et une section.
		/// </summary>
		/// <param name="toTheLeft"></param>
		private void DoCollisionWithSections(bool toTheLeft)
		{
			int direction = toTheLeft ? -1 : 1;
			List<TotemSection> sections = m_totemInstance.AttachedSections;
			Vector2 playerPos = m_transform.PositionGlobal;
			Vector2 totemPos = m_totemInstance.Transform.PositionGlobal;
			for (int i = 0; i < sections.Count; ++i)
			{
				//La détection est faite purement grâce à la position verticale.
				if (playerPos.Y > sections[i].Top + totemPos.Y
					& playerPos.Y < sections[i].Bottom + totemPos.Y)
				{
					sections[i].OnHit(toTheLeft, this, (float)(BasePushForce * SpeedMultiplier * direction));
					SpeedMultiplier = Math.Min(SpeedMultiplier *1.05f, MaxSpeedMultiplier);
				}
			}
		}
		public void Bounce(bool toTheLeft)
		{
			m_bounceTransform.Position = m_playerTransform.Position;
			if (toTheLeft)
			{
				m_actionManager.StartNew(m_bounceRL);
				isToLeft = false;
			}
			else
			{
				m_actionManager.StartNew(m_bounceLR);
				isToLeft = true;
			}
		}

		public override void Update()
        {
            if (Game1.kbs.IsKeyDown(Keys.Space) && Game1.old_kbs.IsKeyUp(Keys.Space)
				&& !m_slashLR.IsActive && !m_slashRL.IsActive && isToLeft)
            {
				m_actionManager.StartNew(m_slashLR);
                isToLeft = false;
            }

            if (Game1.kbs.IsKeyDown(Keys.Space) && Game1.old_kbs.IsKeyUp(Keys.Space)
				&& !m_slashRL.IsActive && !m_slashLR.IsActive && !isToLeft)
            {
                m_actionManager.StartNew(m_slashRL);
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

            if (!canClimb && this.Transform.PosY <= m_climbingPosition.PosY)
            {
                isPlaying = true;
            }

            if (isPlaying)
            {
				this.Transform.PosY = this.Transform.PosY + (float)(BasePlayerSpeed * SpeedMultiplier * Program.TheGame.ElapsedTime);

                if (this.Transform.PosY >= 0)
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
