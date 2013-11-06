using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GbJamTotem 
{
	class PBounceInterpolation : IPInterpolation<float>
	{
		public float GetInterpolation(float from, float to, float ratio)
		{
			ratio = 1 - (float)Math.Abs(Math.Sin((ratio + 0.5f) * Math.PI));
			return from + (to - from) * ratio;
		}
	}

    public class Player : GameObject
    {
		const float BasePlayerSpeed = 60.0f;
		const float BasePushForce = 4.0f;
		const float SlashDuration = 0.2f;
		const float CollisionDelayRatio = 0.5f;
		const float CollisionDelayDuration = SlashDuration * CollisionDelayRatio;
		const float MaxSpeedMultiplier = 2.75f;

		Totem m_totemInstance;

        MoveToTransform m_walkingToTotem;
        const int walkingDistance = 100;
        const float walkingDuration = 5f;

        // Cannot be greather than 7, otherwise climbing animation will not
        // switch at time with coun
        //
        const float climbingDuration = 20f;
        const int deltaAboveClimbingAltitude = -200;

		Concurrent m_slashLR;
		MoveToTransform m_movementLR;
		DelayAction m_slashDelayLR;

		Concurrent m_slashRL;
		MoveToTransform m_movementRL;
		DelayAction m_slashDelayRL;

		Concurrent m_slashBounceLR;
		Concurrent m_slashBounceRL;
		MoveToTransform m_metalBounceLR; //Pas la même chose que slashBounce: metalBounce interrompt l'animation normale de slash
		MoveToTransform m_metalBounceRL;

        MoveToTransform m_climbing;
        SingleActionManager m_actionManager;

        Transform m_spriteTransform;
		Transform m_soulHotspot;
        Transform m_leftTransform;
        Transform m_rightTransform;
		Transform m_bounceTransform;

        Vector2 m_initialPosition;
        Transform m_climbingPosition;
       
        bool isToLeft;
        bool canClimb;
        bool isFalling;

		float m_speedMultiplier = 1; //Permet d'accélérer le rythme d'action du joueur

        int comboCount;

        public int ComboCount
        {
            get { return comboCount; }
            set { comboCount = value; }
        }

        public Transform PlayerTransform
        {
            get { return m_spriteTransform; }
        }

		public Transform SpriteTransform
		{
			get { return m_spriteTransform; }
		}

		public Transform SoulHotspot
		{
			get { return m_soulHotspot; }
		}

		public bool IsToLeft
		{
			get { return isToLeft; }
		}

		public float SpeedMultiplier
		{
			get { return m_speedMultiplier; }
			set { 
				//Changer le multiplicateur de vitesse implique que les animations vont plus vite

				if (m_speedMultiplier > MaxSpeedMultiplier)
					m_speedMultiplier = MaxSpeedMultiplier;
				
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

            // Add delta for starting higher on the totem
            //
            m_climbingPosition = climbingPosition;
            m_climbingPosition.PosY = m_climbingPosition.PosY + deltaAboveClimbingAltitude;

            isToLeft = true;
            canClimb = true;
            isFalling = false;
            comboCount = 0;
			
            m_actionManager = new SingleActionManager();

            m_spriteTransform = new Transform(m_transform, true);
            m_leftTransform = new Transform(m_transform, true);
            m_rightTransform = new Transform(m_transform, true);
			m_bounceTransform = new Transform(m_transform, true);
			m_soulHotspot = new Transform(m_spriteTransform, true);
			m_soulHotspot.Position = new Vector2(10, -10);

            m_transform.PosX = -walkingDistance;
            m_spriteTransform.PosX = initialPosition.X;
            m_leftTransform.Position = initialPosition;
            m_rightTransform.Position = -initialPosition;

            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("perso_destroy"), m_spriteTransform);


            // Mouvement walkingToTotem
            // Mouvement d'introduction au jeu : on voit le personnage à distance, puis
            // quand on commence, il se rapproche du totem
            //
            m_walkingToTotem = new MoveToTransform(Program.TheGame, m_transform, m_transform, new Transform(), 1) ;
            m_walkingToTotem.Interpolator = new PSmoothstepInterpolation();
            m_walkingToTotem.Timer.Interval = walkingDuration;

			///
			///	Slash left to right.
			///	Le mouvement est exécuté en parallèle avec une séquence délai + action.
			///	Autrement dit, vers la moitié de l'animation, il y a test de collision.
			///	Comme ça, le bloc peut être éjecté à un moment très précis, et il n'y a pas de collisions intempestives.
			///	On a: Concurrent(Mouvement + Sequence(delai => test collision))
			///
			m_movementLR = new MoveToTransform(Program.TheGame, m_spriteTransform, m_leftTransform, m_rightTransform, 1);
			m_movementLR.Interpolator = new PSmoothstepInterpolation();
			m_movementLR.Timer.Interval = SlashDuration;
			m_slashDelayLR = new DelayAction(Program.TheGame, CollisionDelayDuration);
			MethodAction collisionLR = new MethodAction(delegate() { DoCollisionWithSections(false);  });
			Sequence slashActionLR = new Sequence(1);
			slashActionLR.AddAction(m_slashDelayLR);
			slashActionLR.AddAction(collisionLR);
			m_slashLR = new Concurrent(new PastaGameLibrary.Action[] { slashActionLR, m_movementLR });

			m_metalBounceLR = new MoveToTransform(Program.TheGame, m_spriteTransform, m_bounceTransform, m_leftTransform, 1);
			m_metalBounceLR.Timer.Interval = SlashDuration - CollisionDelayDuration; //Le reste de temps après la collision

			///
			/// Slash Right To Left
			/// Même principe que l'autre sens
			///
			m_movementRL = new MoveToTransform(Program.TheGame, m_spriteTransform, m_rightTransform, m_leftTransform, 1);
			m_movementRL.Interpolator = new PSmoothstepInterpolation();
			m_movementRL.Timer.Interval = SlashDuration;



			m_slashDelayRL = new DelayAction(Program.TheGame, CollisionDelayDuration);
			MethodAction collisionRL = new MethodAction(delegate() { DoCollisionWithSections(true);  });
			Sequence slashActionRL = new Sequence(1);
			slashActionRL.AddAction(m_slashDelayRL);
			slashActionRL.AddAction(collisionRL);
			m_slashRL = new Concurrent(new PastaGameLibrary.Action[] { slashActionRL, m_movementRL });

			//
			// Mouvement de rebond volontaire gauche
			//
			MoveToTransform bounceMovementL = new MoveToTransform(Program.TheGame, m_spriteTransform, m_leftTransform, m_bounceTransform, 1);
			bounceMovementL.Timer.Interval = SlashDuration;
			bounceMovementL.Interpolator = new PBounceInterpolation();
			MethodAction actionL = new MethodAction(
				delegate()
				{
					m_bounceTransform.PosY = 0;
					m_bounceTransform.PosX = m_leftTransform.PosX + (m_rightTransform.PosX - m_leftTransform.PosX) * CollisionDelayRatio;
				});
			m_slashBounceLR = new Concurrent(new PastaGameLibrary.Action[] { actionL, slashActionLR, bounceMovementL });

			//
			// Mouvement de rebond volontaire droite
			//
			MoveToTransform bounceMovementR = new MoveToTransform(Program.TheGame, m_spriteTransform, m_rightTransform, m_bounceTransform, 1);
			bounceMovementR.Timer.Interval = SlashDuration;
			bounceMovementR.Interpolator = new PBounceInterpolation();
			MethodAction actionR = new MethodAction(
				delegate()
				{
					m_bounceTransform.PosY = 0;
					m_bounceTransform.PosX = m_rightTransform.PosX - (m_rightTransform.PosX - m_leftTransform.PosX) * CollisionDelayRatio;
				});
			m_slashBounceRL = new Concurrent(new PastaGameLibrary.Action[] { actionR, slashActionRL, bounceMovementR });

			m_metalBounceRL = new MoveToTransform(Program.TheGame, m_spriteTransform, m_bounceTransform, m_rightTransform, 1);
			m_metalBounceRL.Timer.Interval = SlashDuration - CollisionDelayDuration; //Le reste de temps après la collision

            m_climbing = new MoveToTransform(Program.TheGame, m_transform, m_transform, m_climbingPosition, 1);
            m_climbing.Interpolator = new PSmoothstepInterpolation();
            m_climbing.Timer.Interval = climbingDuration;
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
					SpeedMultiplier = Math.Min(SpeedMultiplier * 1.05f, MaxSpeedMultiplier);
				}
			}
		}
		public void Bounce(bool toTheLeft)
		{
			m_bounceTransform.Position = m_spriteTransform.Position;
			if (toTheLeft)
			{
				m_actionManager.StartNew(m_metalBounceRL);
				isToLeft = false;
			}
			else
			{
				m_actionManager.StartNew(m_metalBounceLR);
				isToLeft = true;
			}

            //Reset combo
            //
            SpeedMultiplier = 1;
            comboCount = 0;

		}
		public override void Update()
        {
			bool animationIsActive = m_slashLR.IsActive || m_slashRL.IsActive 
				|| m_slashBounceLR.IsActive || m_slashBounceLR.IsActive
				|| m_metalBounceLR.IsActive || m_metalBounceRL.IsActive;


            // Introduction animation (walking to totem)
            //
            if (Game1.kbs.IsKeyDown(Keys.S) && Game1.old_kbs.IsKeyUp(Keys.S))
            {
                m_actionManager.StartNew(m_walkingToTotem);
            }

            // Activate climbing animation
            //
            if (Game1.kbs.IsKeyDown(Keys.C)
                && !m_climbing.IsActive && canClimb)
            {
                m_actionManager.StartNew(m_climbing);
                canClimb = false;
            }

            // TODO
            // faire un compteur
            // 1 - le perso décolle
            // 2 - Il arrive à l'arc de son vol et commence à redescendre. => le décompte commence
            // 3 - Vers la fin du compteur, les slides apparaissent
            // 4 - Fin du compteur, le joueur atteint le top du totem et le joueur a le controle
            //

            // Stop animation in order to skip end of interpolation
            //
            if (m_climbing.IsActive && this.Transform.PosY <= m_climbingPosition.PosY - (deltaAboveClimbingAltitude/50))
                m_actionManager.Stop();

            // Start falling
            // PosY <= m_climbing... becasue Y axis is negative
            //
            if (!canClimb && !isFalling && this.Transform.PosY <= m_climbingPosition.PosY - (deltaAboveClimbingAltitude/50) && !m_climbing.IsActive)
            {
                isFalling = true;
                Game1.startingCountdown.activateCountdown();
            }

            // Unlock commands if falling and countdownFinished are true
            //
            if (isFalling)
            {
                if (Game1.startingCountdown.CountdownHasFinished)
                {
                    if (Game1.kbs.IsKeyDown(Keys.LeftAlt) && Game1.old_kbs.IsKeyUp(Keys.LeftAlt) && !animationIsActive)
                    {
                        if (isToLeft)
                        {
                            m_actionManager.StartNew(m_slashBounceLR);
                            isToLeft = true;
                        }
                        else
                        {
                            m_actionManager.StartNew(m_slashBounceRL);
                            isToLeft = false;
                        }
                    }

                    if (Game1.kbs.IsKeyDown(Keys.Space) && Game1.old_kbs.IsKeyUp(Keys.Space) && !animationIsActive)
                    {
                        if (isToLeft)
                        {
                            m_actionManager.StartNew(m_slashLR);
                            isToLeft = false;
                        }
                        else
                        {
                            m_actionManager.StartNew(m_slashRL);
                            isToLeft = true;
                        }
                    }
                }

                // Stop falling if on the floor 
                //
                if (this.Transform.PosY >= 0)
                {
                    isFalling = false;
                    canClimb = true;

                    Game1.scoreBorder.Slide(false);
                    Game1.mapBorder.Slide(false);
                    Game1.startingCountdown.resetCountdown();
                }

                // Update falling
                //
                this.Transform.PosY = this.Transform.PosY + (float)(BasePlayerSpeed * SpeedMultiplier * Program.TheGame.ElapsedTime);

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
