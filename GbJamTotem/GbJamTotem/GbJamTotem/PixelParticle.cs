using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
	public class PixelParticle : IParticle
	{
		Transform m_transform;
		Sprite m_sprite;
		PhysicsComponent m_physics;

		public PixelParticle(Vector2 startPosition, float fx, float fy)
		{
			m_transform = new Transform();
			m_transform.Position = startPosition;
			m_physics = new PhysicsComponent(Program.TheGame, m_transform);
			m_sprite = new Sprite(Program.TheGame, TextureLibrary.PixelSpriteSheet, m_transform);
			m_sprite.Colour = new Vector4(24.0f / 255, 52.0f / 255, 66.0f/ 255, 1);
			m_physics.Throw(fx, fy, 0);
		}

		public bool RemoveMe()
		{
			return !m_physics.IsProjected;
		}

		public void Update()
		{
			m_physics.Update();
		}

		public void Draw()
		{
			m_sprite.Draw();
		}
	}
}
