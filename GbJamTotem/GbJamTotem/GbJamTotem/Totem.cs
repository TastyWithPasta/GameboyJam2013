using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GbJamTotem
{
	public enum SectionType
	{
 		Left,
		Right,
		Unilateral,
	}

	public class SectionData
	{
		Type m_typeToSpawn;
		int m_amountLeft, m_amountRight, m_amountBoth;

		public SectionData(Type typeToSpawn, int amountLeft, int amountRight, int amountBoth)
		{
			m_typeToSpawn = typeToSpawn;
			m_amountLeft = amountLeft;
			m_amountRight = amountRight;
			m_amountBoth = amountBoth;
		}
		public List<TotemSection> BuildSections()
		{
			List<TotemSection> result = new List<TotemSection>();
			for (int i = 0; i < m_amountLeft; ++i)
				result.Add((TotemSection)Activator.CreateInstance(m_typeToSpawn, SectionType.Left));
			for (int i = 0; i < m_amountRight; ++i)
				result.Add((TotemSection)Activator.CreateInstance(m_typeToSpawn, SectionType.Right));
			for (int i = 0; i < m_amountBoth; ++i)
				result.Add((TotemSection)Activator.CreateInstance(m_typeToSpawn, SectionType.Unilateral));
			return result;
		}
	}


    /* Class totem
     * 
     * Build a totem with differents type of totem sections (normal, metal left, etc.)
     * 
     * When adding a new type of totem section :
     * 1) Add member (+ getters & setters) amount of the new type of totem ;
     * 2) Add the new amount in TotalAmountSections ;
     * 3) Add a new for cycle for the new totem sections ;
     * 4) Add comboCount++ for good hits. (Resets done in Player.Bounce)
     * 
     * */
	public class Totem : GameObject
	{
		List<TotemSection> m_sectionsToPlace = new List<TotemSection>();

		List<TotemSection> m_allSections = new List<TotemSection>();
		List<TotemSection> m_attachedSections = new List<TotemSection>();
		List<TotemSection> m_detachedSections = new List<TotemSection>();
		int m_amountOfNormalSections = 10;

        int m_amoutOfLeftMetalSections = 10;
        int m_amoutOfRightMetalSections = 10;
        int m_amoutOfBothMetalSections = 10;


        #region ACCESSORS & MUTATORS

        public int TotalAmountOfSections
		{
			get { return AmountOfNormalSections 
                + AmoutOfLeftMetalSections
                + AmoutOfRightMetalSections
                + AmoutOfBothMetalSections; }
		}	
		public int AmountOfNormalSections
		{
			get { return m_amountOfNormalSections; }
			set { m_amountOfNormalSections = value; }
		}

        public int AmoutOfLeftMetalSections
        {
            get { return m_amoutOfLeftMetalSections; }
            set { m_amoutOfLeftMetalSections = value; }
        }

        public int AmoutOfRightMetalSections
        {
            get { return m_amoutOfRightMetalSections; }
            set { m_amoutOfRightMetalSections = value; }
        }

        public int AmoutOfBothMetalSections
        {
            get { return m_amoutOfBothMetalSections; }
            set { m_amoutOfBothMetalSections = value; }
        }

		public float Top
		{
			get {
				if (m_attachedSections.Count == 0)
					return 0;
				return m_attachedSections[m_attachedSections.Count - 1].Top; }
		}
		public List<TotemSection> AttachedSections
		{
			get { return m_attachedSections; }
		}
		public List<TotemSection> DetachedSections
		{
			get { return m_detachedSections; }
		}

        #endregion

		public Totem()
		{
			
		}

		public void AddSections(SectionData sectionsToAdd)
		{
			m_sectionsToPlace.AddRange(sectionsToAdd.BuildSections());
		}

		public void Build()
		{
			//Set the order of the totem sections
			int amountOfSections = m_sectionsToPlace.Count;
			for (int i = 0; i < amountOfSections; ++i)
			{
				int index = Program.Random.Next(0, m_sectionsToPlace.Count);
				m_allSections.Add(m_sectionsToPlace[index]);
				m_attachedSections.Add(m_sectionsToPlace[index]);
				m_sectionsToPlace.RemoveAt(index);
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
	public abstract class TotemSection : GameObject
	{
		const float Mass = 7.0f;
		const float Bounciness = 0.5f;
		const int PerspectiveOffset = 5;

        public static Vector2 spriteOrigin = new Vector2(0.5f, 1.0f);

		protected SectionType m_type;
		protected PhysicsComponent m_physics;
		protected Totem m_totemInstance = null;
		protected TotemSection m_above = null;
		protected TotemSection m_below = null;

		public SectionType Type
		{
			get { return m_type; }
		}
		public float Top
		{
			get { return m_transform.PosY - m_sprite.Height * m_transform.SclY * m_sprite.Origin.Y + PerspectiveOffset; }
		}
		public float Bottom
		{
			get { return m_transform.PosY + m_sprite.Height * m_transform.SclY * (1- m_sprite.Origin.Y);  }
		}

		public TotemSection(SectionType type)
			: base()
		{
			m_type = type;
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

		public abstract void OnHit(bool toTheLeft, Player player, float pushForce);

		protected void Push(float force)
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

	public class NormalSection : TotemSection
	{
		public NormalSection(SectionType type)
			: base(SectionType.Unilateral)
		{
			m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_temp"), m_transform);
            m_sprite.Origin = TotemSection.spriteOrigin;
		}
		public override void OnHit(bool toTheLeft, Player player, float pushForce)
		{
			Push(pushForce);
            player.ComboCount++;
		}
	}

    public class MetalSection : TotemSection
    {
        Sprite m_metalSpriteLeft = null;
        Sprite m_metalSpriteRight = null;

		public MetalSection(SectionType type)
			:base(type)
        {
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_temp"), m_transform);
            m_sprite.Origin = TotemSection.spriteOrigin;

			bool left = type == SectionType.Left || type == SectionType.Unilateral;
			bool right = type == SectionType.Right || type == SectionType.Unilateral;
			if (left)
			{
				m_metalSpriteLeft = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_metal_left"), new Transform(m_transform, true));
				m_metalSpriteLeft.Origin = TotemSection.spriteOrigin;
			}
			if (right)
			{
				m_metalSpriteRight = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_metal_right"), new Transform(m_transform, true));
				m_metalSpriteRight.Origin = TotemSection.spriteOrigin;
			}
        }

		public override void OnHit(bool toTheLeft, Player player, float pushForce)
		{
			if ((toTheLeft && m_type == SectionType.Left)
				|| (!toTheLeft && m_type == SectionType.Right))
			{
				Push(pushForce);
                player.ComboCount++;
			}
			else
			{
				player.Bounce(toTheLeft);
                player.ComboCount = 0;
				Push(pushForce);
			}
		}

        public override void Draw()
        {
            base.Draw();
			if(m_metalSpriteLeft != null)
				m_metalSpriteLeft.Draw();
			if(m_metalSpriteRight != null)
				m_metalSpriteRight.Draw();
        }
    }

    public class SpikeSection : TotemSection
    {
        Sprite m_spikeSpriteLeft = null;
        Sprite m_spikeSpriteRight = null;

        public SpikeSection(SectionType type)
            : base(type)
        {
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_temp"), m_transform);
            m_sprite.Origin = TotemSection.spriteOrigin;

            bool left = type == SectionType.Left || type == SectionType.Unilateral;
            bool right = type == SectionType.Right || type == SectionType.Unilateral;

            if (left)
            {
                m_spikeSpriteLeft = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_spikes_left"), new Transform(m_transform, true));
				m_spikeSpriteLeft.Transform.PosX = -4;
				m_spikeSpriteLeft.Origin = TotemSection.spriteOrigin;
            }
            if (right)
            {
                m_spikeSpriteRight = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_spikes_right"), new Transform(m_transform, true));
                m_spikeSpriteRight.Origin = TotemSection.spriteOrigin;
				m_spikeSpriteRight.Transform.PosX = 4;
            }
        }

        public override void OnHit(bool toTheLeft, Player player, float pushForce)
        {
			if ((toTheLeft && m_type == SectionType.Left)
				|| (!toTheLeft && m_type == SectionType.Right))
			{
				Push(pushForce);
                player.ComboCount++;
			}
			else
			{
				player.Bounce(toTheLeft);
                player.ComboCount = 0; // TODO Utile ici?
			}
        }

        public override void Draw()
        {
            base.Draw();
            if (m_spikeSpriteLeft != null)
                m_spikeSpriteLeft.Draw();
            if (m_spikeSpriteRight != null)
                m_spikeSpriteRight.Draw();
        }
    }

}
