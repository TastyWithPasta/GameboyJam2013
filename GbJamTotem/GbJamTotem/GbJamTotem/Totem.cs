using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;

namespace GbJamTotem
{
	class Totem : GameObject
	{
		List<GameObject> m_sections;
		int m_amountOfNormalSections;

		public int AmountOfNormalSections
		{
			get { return m_amountOfNormalSections; }
		}

		public Totem()
		{
			
		}
		public void Build()
		{ }

		public override void Update()
		{ }
		public override void Draw()
		{ }
	}
	class TotemSection : GameObject
	{
		PhysicsComponent m_physics;

		public TotemSection()
			: base()
		{
			m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totemsection"), m_transform);
			m_physics = new PhysicsComponent(Program.TheGame, m_transform);
		}

		public override void Update()
		{
			m_physics.Update();
		}
		public override void Draw()
		{
			m_sprite.Draw();
		}
	}
}
