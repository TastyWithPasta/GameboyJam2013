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
		//MoveToStaticAction m_moveToPlayer;
		Sequence m_animation;
		PhysicsComponent m_physics;

        // TODO NOTE
        // Si besoin de créer des particules, il faut un générateur de particules ( ParticleGenerator<ClasseDeParticle> )
        /*
         * "J'ai un système de particules statique deans Game1, Il te faut un générateur de particules pour créer des particules"
         */

		static SpriteSheet[] particleTextures = new SpriteSheet[4];
		public static void InitialiseIndexes()
		{
			particleTextures[0] = TextureLibrary.GetSpriteSheet("totem_particle_1");
			particleTextures[1] = TextureLibrary.GetSpriteSheet("totem_particle_2");
			particleTextures[2] = TextureLibrary.GetSpriteSheet("totem_particle_3");
			particleTextures[3] = TextureLibrary.GetSpriteSheet("totem_particle_4");
		}

        public Explosion(Vector2 initialPosition, Player player)
		{
			//m_transform.ParentTransform = player.Transform;
			m_playerInstance = player;
			m_transform.Position = player.Transform.PositionGlobal;
			m_sprite = new Sprite(Program.TheGame, particleTextures[Program.Random.Next(0, 4)], m_transform);
			m_physics = new PhysicsComponent(Program.TheGame, m_transform);
			m_physics.Mass = 2 + (float)Program.Random.NextDouble();
			m_physics.GroundLevel = Program.Random.Next(0, 10);

			float fx = 1 + (float)Program.Random.NextDouble() * 2;
			float fy = -1 - (float)Program.Random.NextDouble() * 2;
			if (player.SpriteTransform.PosX < 0)
				m_physics.Throw(fx, fy, 1.0f);
			else
				m_physics.Throw(-fx, fy, 1.0f);
		}

		public override void Update()
		{
			m_physics.Update();
		}

		public override void Draw()
		{
			//if (m_moveToAnimation.IsActive)
				m_sprite.Draw();
		}

		public bool RemoveMe()
		{
			return !m_physics.IsProjected;
			//return !m_moveToAnimation.IsActive;
		}
	}

}
