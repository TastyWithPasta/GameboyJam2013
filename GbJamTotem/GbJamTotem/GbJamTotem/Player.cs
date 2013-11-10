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
		float m_offset;
		int m_factor = 1;
		bool m_reverse = false;

		public PBounceInterpolation(float offset)
		{
			m_offset = offset;
		}
		public PBounceInterpolation(float offset, int factor)
		{
			m_offset = offset;
			m_factor = factor;
		}
		public PBounceInterpolation(float offset, int factor, bool reverse)
		{
			m_offset = offset;
			m_factor = factor;
			m_reverse = reverse;
		}
		public float GetInterpolation(float from, float to, float ratio)
		{
			ratio = 1 - (float)Math.Abs(Math.Sin((ratio + m_offset) * Math.PI));
			ratio = (float)Math.Pow(ratio, m_factor);
			if (m_reverse)
				ratio = 1 - ratio;//dirty
			return from + (to - from) * ratio;
		}
	}
		
    public class Player : GameObject
    {
		public const float DistanceFromTotemCenter = -40;

		const float BasePlayerSpeed = 60.0f;
		const float BasePushForce = 4.0f;
		const float SlashDuration = 0.3f;
		const float CollisionDelayRatio = 0.5f;
		const float CollisionDelayDuration = SlashDuration * CollisionDelayRatio;
		const float SpeedMultiplierIncrement = 1.05f;
		const float MaxSpeedMultiplier = 2.5f;
		const int DeltaAboveClimbingAltitude = -100;
		const int DecelerationPointFromBaseX = -100;

		Totem m_totemInstance;

        MoveToTransform m_walkingToTotem;
        const int walkingDistance = 100;
        const float walkingDuration = 5f;

		Concurrent m_slashLR;
		MoveToTransform m_movementLR;
		MoveToTransform m_bounceMovementLL;
		DelayAction m_slashDelayLR;

		Concurrent m_slashRL;
		MoveToTransform m_movementRL;
		MoveToTransform m_bounceMovementRR;
		DelayAction m_slashDelayRL;

		Concurrent m_slashBounceLR;
		Concurrent m_slashBounceRL;
		MoveToTransform m_metalBounceLR; //Pas la même chose que slashBounce: metalBounce interrompt l'animation normale de slash
		MoveToTransform m_metalBounceRL;
		SpriteSheetAnimation m_spritAnimLR;
		SpriteSheetAnimation m_spritAnimRL;
		SpriteSheetAnimation m_spritAnimRR;
		SpriteSheetAnimation m_spritAnimLL;
		SpriteSheetAnimation m_ready;

        MoveToTransform m_climbing;

        SingleActionManager m_actionManager;
		SingleActionManager m_spriteAnimation;

        Transform m_spriteTransform;
		Transform m_soulHotspot;
        Transform m_soulAbsorptionPosition;
        Transform m_leftTransform;
        Transform m_rightTransform;
		Transform m_bounceTransform;

        bool isToLeft;
        bool canClimb;
        bool isFalling;
		bool isVisible;

		float m_speedMultiplier = 1; //Permet d'accélérer le rythme d'action du joueur

        int comboCount;

		public bool IsVisible
		{
			get { return isVisible; }
			set { isVisible = value; }
		}
        public bool IsFalling
        {
            get { return isFalling; }
        }

        public int ComboCount
        {
            get { return comboCount; }
            set {
				if (value > Game1.scoreBorder.ScoreMultiplierMax)
					comboCount = Game1.scoreBorder.ScoreMultiplierMax;
				else
					comboCount = value;
				Program.TheGame.UpdateComboEffects();
			}
        }

		public Transform SpriteTransform
		{
			get { return m_spriteTransform; }
		}

		public Transform SoulHotspot
		{
			get { return m_soulHotspot; }
		}

        public Transform SoulAbsorptionPosition
        {
            get { return m_soulAbsorptionPosition; }
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
				m_bounceMovementLL.Timer.Interval = SlashDuration / value;
				m_bounceMovementRR.Timer.Interval = SlashDuration / value;
				m_spritAnimLL.Timer.Interval = SlashDuration / value;
				m_spritAnimRR.Timer.Interval = SlashDuration / value;
				m_spritAnimRL.Timer.Interval = SlashDuration / value;
				m_spritAnimLR.Timer.Interval = SlashDuration / value;
			}
		}


        public Player()
            : base()
        {
            isToLeft = true;
            canClimb = true;
            isFalling = false;
            comboCount = 0;
			
            m_actionManager = new SingleActionManager();
			m_spriteAnimation = new SingleActionManager();

            m_spriteTransform = new Transform(m_transform, true);
            m_leftTransform = new Transform(m_transform, true);
            m_rightTransform = new Transform(m_transform, true);
			m_bounceTransform = new Transform(m_transform, true);
			m_soulHotspot = new Transform(m_spriteTransform, true);
			m_soulHotspot.Position = new Vector2(10, -10);
            m_soulAbsorptionPosition = new Transform(m_spriteTransform, true);
            m_soulAbsorptionPosition.Position = new Vector2(0, 1);

            m_transform.PosX = -walkingDistance;
			m_spriteTransform.PosX = DistanceFromTotemCenter;
            m_leftTransform.Position = new Vector2(DistanceFromTotemCenter, 0);
            m_rightTransform.Position = new Vector2(-DistanceFromTotemCenter, 0);

            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("player", 4, 8), m_spriteTransform);


            // Mouvement walkingToTotem
            // Mouvement d'introduction au jeu : on voit le personnage à distance, puis
            // quand on commence, il se rapproche du totem
            //
            m_walkingToTotem = new MoveToTransform(Program.TheGame, m_transform, m_transform, new Transform(), 1) ;
            m_walkingToTotem.Interpolator = new PSmoothstepInterpolation();
            m_walkingToTotem.Timer.Interval = walkingDuration;

			#region Slash Animations
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

			#endregion

			#region Bounce Animations
			//
			// Mouvement de rebond volontaire gauche
			//
			m_bounceMovementLL = new MoveToTransform(Program.TheGame, m_spriteTransform, m_leftTransform, m_bounceTransform, 1);
			m_bounceMovementLL.Timer.Interval = SlashDuration;
			m_bounceMovementLL.Interpolator = new PBounceInterpolation(0.5f);
			MethodAction actionL = new MethodAction(
				delegate()
				{
					m_bounceTransform.PosY = 0;
					m_bounceTransform.PosX = m_leftTransform.PosX + (m_rightTransform.PosX - m_leftTransform.PosX) * CollisionDelayRatio;
				});
			m_slashBounceLR = new Concurrent(new PastaGameLibrary.Action[] { actionL, slashActionLR, m_bounceMovementLL });

			//
			// Mouvement de rebond volontaire droite
			//
			m_bounceMovementRR = new MoveToTransform(Program.TheGame, m_spriteTransform, m_rightTransform, m_bounceTransform, 1);
			m_bounceMovementRR.Timer.Interval = SlashDuration;
			m_bounceMovementRR.Interpolator = new PBounceInterpolation(0.5f);
			MethodAction actionR = new MethodAction(
				delegate()
				{
					m_bounceTransform.PosY = 0;
					m_bounceTransform.PosX = m_rightTransform.PosX - (m_rightTransform.PosX - m_leftTransform.PosX) * CollisionDelayRatio;
				});
			m_slashBounceRL = new Concurrent(new PastaGameLibrary.Action[] { actionR, slashActionRL, m_bounceMovementRR });

			m_metalBounceRL = new MoveToTransform(Program.TheGame, m_spriteTransform, m_bounceTransform, m_rightTransform, 1);
			m_metalBounceRL.Timer.Interval = SlashDuration - CollisionDelayDuration; //Le reste de temps après la collision

			#endregion

			m_spritAnimLR = new SpriteSheetAnimation(m_sprite, 0, 7, SlashDuration, 1);
			m_spritAnimRL = new SpriteSheetAnimation(m_sprite, 8, 15, SlashDuration, 1);
			m_spritAnimLL = new SpriteSheetAnimation(m_sprite, 16, 23, SlashDuration, 1);
			m_spritAnimRR = new SpriteSheetAnimation(m_sprite, 23, 30, SlashDuration, 1);
			m_ready = new SpriteSheetAnimation(m_sprite, 0, 4, SlashDuration, 1);
			m_ready.Timer.Interval = 0.5f;

		}

		public void Initialise(Totem totem)
		{ 
			m_totemInstance = totem;
			m_transform.PosX = totem.Transform.PosX;
			m_transform.PosY = totem.Top + DeltaAboveClimbingAltitude;
			isVisible = false;
		}

		public void GetReady()
		{
			//isFalling = true;
			//Game1.startingCountdown.activateCountdown();
			m_spriteAnimation.StartNew(m_ready);
		}

		public void StartCountDown()
		{
			isFalling = true;
			Game1.startingCountdown.activateCountdown();
			Game1.isInGameplay = true;
			isVisible = true;
		}

		//Pour debug
		public void StartDebug(Totem totem)
		{
			m_totemInstance = totem;
			m_transform.PosX = 0;
			m_transform.PosY = totem.Top + DeltaAboveClimbingAltitude;
			isFalling = true;
			Game1.startingCountdown.activateCountdown();
			Game1.isInGameplay = true;
			isVisible = true;
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
					SpeedMultiplier = Math.Min(SpeedMultiplier * SpeedMultiplierIncrement, MaxSpeedMultiplier);
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
		public void HitSpikes()
		{
			Game1.isInGameplay = false;
			isVisible = false;
			isFalling = false;
			Cutscenes.cutscenePlayer.HitSpikes();
			Game1.spikeHitSound.Play();
		}

		public void FinishTotem()
		{
			isFalling = false;
			isVisible = false;
		}

		public override void Update()
        {
			bool animationIsActive = m_slashLR.IsActive || m_slashRL.IsActive 
				|| m_slashBounceLR.IsActive || m_slashBounceLR.IsActive
				|| m_metalBounceLR.IsActive || m_metalBounceRL.IsActive;


            // Introduction animation (walking to totem)
            //
			//if (Game1.kbs.IsKeyDown(Keys.S) && Game1.old_kbs.IsKeyUp(Keys.S))
			//{
			//    m_actionManager.StartNew(m_walkingToTotem);
			//}

            // TODO
            // faire un compteur
            // 1 - le perso décolle
            // 2 - Il arrive à l'arc de son vol et commence à redescendre. => le décompte commence
            // 3 - Vers la fin du compteur, les slides apparaissent
            // 4 - Fin du compteur, le joueur atteint le top du totem et le joueur a le controle
            //

            // Unlock commands if falling and countdownFinished are true
            //
            if (isFalling)
            {
                if (Game1.startingCountdown.CountdownHasFinished)
                {
                    if (Game1.kbs.IsKeyDown(Keys.LeftAlt) && Game1.old_kbs.IsKeyUp(Keys.LeftAlt) && !animationIsActive)
                    {
                        Game1.swordSlashSound.Play();

                        if (isToLeft)
                        {
                            m_actionManager.StartNew(m_slashBounceLR);
							m_spriteAnimation.StartNew(m_spritAnimLL);
							//m_spritAnimLL.Timer.Interval = 
                            isToLeft = true;
                        }
                        else
                        {
                            m_actionManager.StartNew(m_slashBounceRL);
							m_spriteAnimation.StartNew(m_spritAnimRR);
                            isToLeft = false;
                        }
                    }

                    if (Game1.kbs.IsKeyDown(Keys.Space) && Game1.old_kbs.IsKeyUp(Keys.Space) && !animationIsActive)
                    {
                        Game1.swordSlashSound.Play();

                        if (isToLeft)
                        {
                            Game1.moveLeftToRightSound.Play();
                            m_actionManager.StartNew(m_slashLR);
							m_spriteAnimation.StartNew(m_spritAnimLR);
                            isToLeft = false;
                            Game1.comboCounter.RestartScrolling();
                        }
                        else
                        {
                            Game1.moveRightToLeftSound.Play();
                            m_actionManager.StartNew(m_slashRL);
							m_spriteAnimation.StartNew(m_spritAnimRL);
                            isToLeft = true;
                            Game1.comboCounter.RestartScrolling();
                        }
                    }

					if (!animationIsActive)
						if (isToLeft)
							m_sprite.SetFrame(4);
						else
							m_sprite.SetFrame(12);
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

				if (Transform.PosY > m_totemInstance.Base)
				{
					Cutscenes.FinishTotem();
				}
            }

            m_actionManager.Update();
			m_spriteAnimation.Update();
            Game1.old_kbs = Game1.kbs;
        }
        public override void Draw()
        {
			if (!isVisible)
				return;
            m_sprite.Draw();
        }
    }
}
