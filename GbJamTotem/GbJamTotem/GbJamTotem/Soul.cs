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
		MoveToStaticAction m_moveToExplosionPoint;
		MoveToTransform m_moveToAnimation;

		public Soul(Vector2 initialPosition, Player player)
		{
			//m_transform.ParentTransform = player.Transform;
			m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("soul_temp"), m_transform);
			Vector2 explodePosition;

			if (player.IsToLeft)
				explodePosition.X = Program.Random.Next(-72, -20) + 0.5f;
			else
				explodePosition.X = Program.Random.Next(20, 73) + 0.5f;
			explodePosition.Y = Program.Random.Next((int)player.Transform.PosY + 50, (int)Math.Min(10, player.Transform.PosY + 100)) + 0.5f;
			m_moveToExplosionPoint = new MoveToStaticAction(Program.TheGame, m_transform, explodePosition, 1);
			m_moveToExplosionPoint.StartPosition = initialPosition;
			m_moveToExplosionPoint.Timer.Interval = BaseExplosionDuration / player.SpeedMultiplier;
			m_moveToExplosionPoint.Interpolator = new PSmoothstepInterpolation();
			m_moveToExplosionPoint.Start();
			//Transform start = new Transform(player.Transform, true);
			//start.Position = initialPosition - player.Transform.PositionGlobal;
			//m_moveToAnimation = new MoveToTransform(Program.TheGame, m_transform, start, player.SpriteTransform, 1);
			//m_moveToAnimation.Interpolator = new PSmoothstepInterpolation();
			//m_moveToAnimation.Timer.Interval = BaseDuration / player.SpeedMultiplier;
			//m_moveToAnimation.Start();
		}

		public override void Update()
		{
			m_moveToExplosionPoint.Update();
			//m_moveToAnimation.Update();
		}

		public override void Draw()
		{
			//if (m_moveToAnimation.IsActive)
				m_sprite.Draw();
		}

		public bool RemoveMe()
		{
			return false;
			//return !m_moveToAnimation.IsActive;
		}
	}
}
