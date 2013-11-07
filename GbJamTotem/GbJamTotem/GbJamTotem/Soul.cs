using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
	public class Soul : GameObject, IParticle
	{
		const float BaseExplosionDuration = 0.5f;
		const float BaseDuration = 0.5f;
		const float MoveToPlayerTime = 0.3f;

		Sequence m_animation;

        // TODO NOTE
        // Si besoin de créer des particules, il faut un générateur de particules ( ParticleGenerator<ClasseDeParticle> )
        /*
         * "J'ai un système de particules statique deans Game1, Il te faut un générateur de particules pour créer des particules"
         */

        public Soul(Vector2 initialPosition, Player player)
		{
			//m_transform.ParentTransform = player.Transform;
			m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("soul_temp"), m_transform);
			Vector2 explodePosition;

			if (player.IsToLeft)
				explodePosition.X = Program.Random.Next(-72, -20) + 0.5f;
			else
				explodePosition.X = Program.Random.Next(20, 73) + 0.5f;
			explodePosition.Y = Program.Random.Next((int)player.Transform.PosY + 50, (int)player.Transform.PosY + 100) + 0.5f;

			MoveToStaticAction moveToExplosionPoint = new MoveToStaticAction(Program.TheGame, m_transform, explodePosition, 1);
			moveToExplosionPoint.StartPosition = initialPosition;
			moveToExplosionPoint.Timer.Interval = BaseExplosionDuration / player.SpeedMultiplier;
			moveToExplosionPoint.Interpolator = new PSmoothstepInterpolation();
			MoveToStaticAction moveToPlayer = new MoveToStaticAction(Program.TheGame, m_transform, explodePosition, 1);
			moveToPlayer.Interpolator = new PSquareInterpolation(1);
			moveToPlayer.Timer.Interval = MoveToPlayerTime;
			MethodAction positionRefresh = new MethodAction(delegate() { moveToPlayer.Target = player.SoulAbsorptionPosition.PositionGlobal; });

			m_animation = new Sequence(1);
			m_animation.AddAction(moveToExplosionPoint);
			m_animation.AddAction(new Concurrent(new PastaGameLibrary.Action[]{ positionRefresh, moveToPlayer }));
			m_animation.Start();
		}

		private void UpdateMoveToPlayer()
		{ 
		}

		public override void Update()
		{
			m_animation.Update();
		}

		public override void Draw()
		{
			//if (m_moveToAnimation.IsActive)
				m_sprite.Draw();
		}

		public bool RemoveMe()
		{
			return !m_animation.IsActive;
			//return !m_moveToAnimation.IsActive;
		}
	}
}
