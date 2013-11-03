using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GbJamTotem
{
	public class Totem : GameObject
	{
		List<TotemSection> m_allSections = new List<TotemSection>();
		List<TotemSection> m_attachedSections = new List<TotemSection>();
		List<TotemSection> m_detachedSections = new List<TotemSection>();
		int m_amountOfNormalSections = 10;

		public int TotalAmountOfSections
		{
			get { return AmountOfNormalSections; }
		}	
		public int AmountOfNormalSections
		{
			get { return m_amountOfNormalSections; }
			set { m_amountOfNormalSections = value; }
		}

		public float Top
		{
			get { return m_attachedSections[m_attachedSections.Count - 1].Top; }
		}
		public List<TotemSection> AttachedSections
		{
			get { return m_attachedSections; }
		}
		public List<TotemSection> DetachedSections
		{
			get { return m_detachedSections; }
		}

		public Totem()
		{
			
		}
			
		public void Build()
		{
			//Builds list of all totem parts (not placed)
			List<TotemSection> sectionsToPlace = new List<TotemSection>();
			for (int i = 0; i < m_amountOfNormalSections; ++i){
				sectionsToPlace.Add(new TotemSection());
			}

			//Set the order of the totem sections
			for (int i = 0; i < TotalAmountOfSections; ++i)
			{
				int index = Program.Random.Next(0, sectionsToPlace.Count);
				m_allSections.Add(sectionsToPlace[index]);
				m_attachedSections.Add(sectionsToPlace[index]);
				sectionsToPlace.RemoveAt(index);
			}

			//Place the totem sections in the world
			TotemSection above = null, below = null;
			for (int i = 0; i < m_allSections.Count; ++i)
			{
				
				if (i == 0)
				{
					below = null;
					m_allSections[i].Transform.PosY = 0;
				}
				else
				{
					below = m_allSections[i - 1];
					m_allSections[i].Transform.PosY = below.Top;
				}

				if (i == m_allSections.Count - 1)
				{
					above = null;
				}
				else
				{
					above = m_allSections[i + 1];
				}

				m_allSections[i].Transform.ParentTransform = m_transform;
				m_allSections[i].PlaceOnTotem(this, above, below);
			}
		}

		internal void Detach(TotemSection section)
		{
			m_attachedSections.Remove(section);
			m_detachedSections.Add(section);
		}

		public override void Update()
		{
			for (int i = 0; i < m_allSections.Count; ++i)
				m_allSections[i].Update();
		}
		public override void Draw()
		{
			for (int i = 0; i < m_allSections.Count; ++i)
				m_allSections[i].Draw();
		}
	}
	public class TotemSection : GameObject
	{
		const float Mass = 7.0f;
		const float Bounciness = 0.5f;
		const int PerspectiveOffset = 5;

		PhysicsComponent m_physics;
		Totem m_totemInstance = null;
		TotemSection m_above = null;
		TotemSection m_below = null;

		public float Top
		{
			get { return m_transform.PosY - m_sprite.Height * m_transform.SclY * m_sprite.Origin.Y + PerspectiveOffset; }
		}
		public float Bottom
		{
			get { return m_transform.PosY + m_sprite.Height * m_transform.SclY * (1- m_sprite.Origin.Y);  }
		}

		public TotemSection()
			: base()
		{
			m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_temp"), m_transform);
			m_sprite.Origin = new Vector2(0.5f, 1.0f);
			m_physics = new PhysicsComponent(Program.TheGame, m_transform);
			m_physics.Mass = Mass;
			m_physics.Restitution = Bounciness;
			m_transform.PosX = 0.5f;
		}

		public void PlaceOnTotem(Totem totem, TotemSection sectionAbove, TotemSection sectionBelow)
		{
			m_totemInstance = totem;
			m_above = sectionAbove;
			m_below = sectionBelow;
		}

		private void ChainPush()
		{
			m_physics.Throw(0, 0, 0);
			if (m_above != null)
				m_above.ChainPush();
		}

		public void Push(float force)
		{
			m_physics.Throw(force, -5, (float)Program.Random.NextDouble());
			m_physics.GroundLevel = 0;
			m_totemInstance.Detach(this);

			if(m_below != null)
				m_below.m_above = m_above;
			if (m_above != null)
			{
				m_above.ChainPush();
				m_above.m_below = m_below;
			}

			float previous = Top;
			m_sprite.Origin = new Vector2(0.5f, 0.5f);
			float next = Top;
			m_transform.Position += m_transform.ParentTransform.PositionGlobal;
			m_transform.PosY -= next - previous;
			m_transform.ParentTransform = null;
			m_totemInstance = null;
			m_below = null;
			m_above = null;
		}

		public override void Update()
		{
			m_physics.Update();
			if (m_below != null)
				m_physics.GroundLevel = (int)m_below.Top;
			else
				m_physics.GroundLevel = 0;
		}
		public override void Draw()
		{
			m_sprite.Draw();
		}
	}
}
