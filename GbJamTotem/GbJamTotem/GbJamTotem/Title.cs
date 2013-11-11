using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
	public class Title : GameObject
	{
		const float TitleX = Game1.GameboyWidth * 0.5f;
		const float TitleY = 40;

		SpriteSheetAnimation m_animation;
		MoveToStaticAction m_moveTo;
		PhysicsComponent m_physics;

		public Title()
			: base()
		{
			m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("titre_anime", 4, 1), m_transform);
			m_animation = new SpriteSheetAnimation(m_sprite, 0, 3, 1.0f, -1);
			m_animation.Start();
			m_physics = new PhysicsComponent(Program.TheGame, m_transform);
			m_physics.GroundLevel = (int)TitleY;
			m_physics.Mass = 4;
			m_moveTo = new MoveToStaticAction(Program.TheGame, m_transform, new Vector2(TitleX, -100), 1);
			m_moveTo.Interpolator = new PSmoothstepInterpolation();
			m_moveTo.Timer.Interval = 0.4f;
			m_transform.Position = new Vector2(TitleX, -100);
		}

		public void Appear()
		{
			m_transform.PosX = TitleX;
			m_transform.PosY = -50;
			m_physics.Throw(0, 0, 0);
		}
		public void Dissappear()
		{
			m_moveTo.StartPosition = m_transform.Position;
			m_moveTo.Start();
		}

		public override void Update()
		{
			m_animation.Update();
			m_physics.Update();
			m_moveTo.Update();
		}
		public override void Draw()
		{
			m_sprite.Draw();
		}
	}
}
