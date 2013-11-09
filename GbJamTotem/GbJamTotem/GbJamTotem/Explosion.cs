using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
    class Explosion : GameObject, IParticle
    {

        const float BaseExplosionDuration = 0.5f;
		const float BaseDuration = 0.5f;
		const float ExplosionBreadth = 75;

		Player m_playerInstance;
		MoveToStaticAction m_moveToPlayer;
		Sequence m_animation;

        // TODO NOTE
        // Si besoin de créer des particules, il faut un générateur de particules ( ParticleGenerator<ClasseDeParticle> )
        /*
         * "J'ai un système de particules statique deans Game1, Il te faut un générateur de particules pour créer des particules"
         */

        public Explosion(Vector2 initialPosition, Player player)
		{
			//m_transform.ParentTransform = player.Transform;
			m_playerInstance = player;
			m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("soul"), m_transform);
			Vector2 explodePosition;

			/*if (player.IsToLeft)
				explodePosition.X = Program.Random.Next(-72, -20) + 0.5f;
			else
				explodePosition.X = Program.Random.Next(20, 73) + 0.5f;*/

            explodePosition.X = Program.Random.Next((int)player.Transform.PosX + 50, (int)(player.Transform.PosX + 50 + ExplosionBreadth * m_playerInstance.SpeedMultiplier)) + 0.5f;
			explodePosition.Y = Program.Random.Next((int)player.Transform.PosY + 50, (int)(player.Transform.PosY + 50 + ExplosionBreadth * m_playerInstance.SpeedMultiplier)) + 0.5f;

			MoveToStaticAction moveToExplosionPoint = new MoveToStaticAction(Program.TheGame, m_transform, explodePosition, 1);
			moveToExplosionPoint.StartPosition = initialPosition;
            moveToExplosionPoint.Timer.Interval = BaseExplosionDuration / player.SpeedMultiplier;
			moveToExplosionPoint.Interpolator = new PSmoothstepInterpolation();

            
			m_animation = new Sequence(1);
			m_animation.AddAction(moveToExplosionPoint);
			m_animation.AddAction(new DelayAction(Program.TheGame, (float)(Program.Random.NextDouble() * 0.5f + 0.1f) / m_playerInstance.SpeedMultiplier));
			//m_animation.AddAction(m_moveToPlayer);
			m_animation.Start();
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
