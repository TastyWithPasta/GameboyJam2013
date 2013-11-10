using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace GbJamTotem
{
	public enum SectionType
	{
 		Left,
		Right,
		Bilateral,
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
				result.Add((TotemSection)Activator.CreateInstance(m_typeToSpawn, SectionType.Bilateral));
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

        #region ACCESSORS & MUTATORS

        public int TotalAmountOfSections
		{
            get
            {
                return m_allSections.Count;
            }
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

		public void BuildFromFile(string filename)
		{
			string[] lines = System.IO.File.ReadAllLines("Content/Levels/" + filename + ".txt");
			List<TotemSection> sectionsToAdd = new List<TotemSection>();
			for(int i = lines.Length - 1; i > -1; --i)
			{
				if (lines[i].StartsWith("||"))
					sectionsToAdd.Add(new NormalSection(SectionType.Bilateral));
				if (lines[i].StartsWith("[|"))
					sectionsToAdd.Add(new MetalSection(SectionType.Left));
				if (lines[i].StartsWith("|]"))
					sectionsToAdd.Add(new MetalSection(SectionType.Right));
				if (lines[i].StartsWith("[]"))
					sectionsToAdd.Add(new MetalSection(SectionType.Bilateral));
				if (lines[i].StartsWith("{|"))
					sectionsToAdd.Add(new SpikeSection(SectionType.Left));
				if (lines[i].StartsWith("|}"))
					sectionsToAdd.Add(new SpikeSection(SectionType.Right));
				if (lines[i].StartsWith("{}"))
					sectionsToAdd.Add(new SpikeSection(SectionType.Bilateral));
			}
			TotemBase totemBase  = new TotemBase();
			m_allSections.Add(totemBase);
			m_attachedSections.Add(totemBase);
			m_allSections.AddRange(sectionsToAdd);
			m_attachedSections.AddRange(sectionsToAdd);
		}

		public void BuildRandom()
		{
			TotemBase totemBase = new TotemBase();
			m_allSections.Add(totemBase);
			m_attachedSections.Add(totemBase);

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
		const int PerspectiveOffset = 8;

        public static Vector2 spriteOrigin = new Vector2(0.5f, 1.0f);

		protected SectionType m_type;
		protected PhysicsComponent m_physics;
		protected Totem m_totemInstance = null;
		protected TotemSection m_above = null;
		protected TotemSection m_below = null;
		private ParticleGenerator<Soul> m_generator;

        private ParticleGenerator<Explosion> m_explosion;

        public virtual int GetSoulCount()
        {
            return 1;
        }
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
            m_generator = new ParticleGenerator<Soul>(Program.TheGame, Game1.Souls);

            m_explosion = new ParticleGenerator<Explosion>(Program.TheGame, Game1.Explosions);
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

		protected void Push(Player player, float force)
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

            if (Game1.old_kbs.IsKeyDown(Keys.Space))
            {
                m_transform.Scale = new Vector2(0);
                m_explosion.Generate(10, new Object[] { m_transform.PositionGlobal, player });
            }

            if (player.ComboCount >= Game1.scoreBorder.ScoreMultiplierMax)
            {
                m_generator.Generate(Game1.scoreBorder.ScoreMultiplierMax, new object[] { m_transform.PositionGlobal, player });
            }
            else
            {
                m_generator.Generate(player.ComboCount, new object[] { m_transform.PositionGlobal, player });
            }
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

	public class TotemBase : TotemSection
	{
		public TotemBase()
			: base(SectionType.Bilateral)
		{
			m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_base"), m_transform);
			m_sprite.Origin = TotemSection.spriteOrigin;
		}
		public override void OnHit(bool toTheLeft, Player player, float pushForce)
		{
			return;
		}
	}

	public class NormalSection : TotemSection
	{
		public NormalSection(SectionType type)
			: base(SectionType.Bilateral)
		{
			m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_sprite_1"), m_transform);
            m_sprite.Origin = TotemSection.spriteOrigin;
		}
		public override void OnHit(bool toTheLeft, Player player, float pushForce)
		{
			Push(player, pushForce);
            if (Game1.normalTotemCollisionSound_Channel1.State == SoundState.Playing)
            {
                Game1.normalTotemCollisionSound_Channel2.Play();
            }
            else
            {
                Game1.normalTotemCollisionSound_Channel1.Play();
            }
            player.ComboCount++;
		}
	}

    public class MetalSection : TotemSection
    {
        Sprite m_metalSpriteLeft = null;
        Sprite m_metalSpriteRight = null;

        public override int GetSoulCount()
        {
            if (m_type == SectionType.Bilateral) 
                return 0;
            return 1;
        }

		public MetalSection(SectionType type)
			:base(type)
        {
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_sprite_1"), m_transform);
            m_sprite.Origin = TotemSection.spriteOrigin;

			bool left = type == SectionType.Left || type == SectionType.Bilateral;
			bool right = type == SectionType.Right || type == SectionType.Bilateral;
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
				Push(player, pushForce);
                if (Game1.normalTotemCollisionSound_Channel1.State == SoundState.Playing)
                {
                    Game1.normalTotemCollisionSound_Channel2.Play();
                }
                else
                {
                    Game1.normalTotemCollisionSound_Channel1.Play();
                }
                player.ComboCount++;
			}
			else
			{
				player.Bounce(toTheLeft);
                if (Game1.metalTotemCollisionSound_Channel1.State == SoundState.Playing)
                {
                    Game1.metalTotemCollisionSound_Channel2.Play();
                }
                else
                {
                    Game1.metalTotemCollisionSound_Channel1.Play();
                }
                player.ComboCount = 0;
				//Push(player, pushForce);
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

        public override int GetSoulCount()
        {
            if (m_type == SectionType.Bilateral)
                return 0;
            return 1;
        }

        public SpikeSection(SectionType type)
            : base(type)
        {
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_sprite_1"), m_transform);
            m_sprite.Origin = TotemSection.spriteOrigin;

            bool left = type == SectionType.Left || type == SectionType.Bilateral;
            bool right = type == SectionType.Right || type == SectionType.Bilateral;

            if (left)
            {
                m_spikeSpriteLeft = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_sprite_spikes_left_1"), new Transform(m_transform, true));
				//m_spikeSpriteLeft.Transform.PosX = -4;
                m_spikeSpriteLeft.Transform.PosX = -19;
				m_spikeSpriteLeft.Origin = TotemSection.spriteOrigin;
            }
            if (right)
            {
                m_spikeSpriteRight = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("totem_sprite_spikes_right_1"), new Transform(m_transform, true));
                m_spikeSpriteRight.Origin = TotemSection.spriteOrigin;
				//m_spikeSpriteRight.Transform.PosX = 4;
                m_spikeSpriteRight.Transform.PosX = 19;
            }
        }

        public override void OnHit(bool toTheLeft, Player player, float pushForce)
        {
			if ((toTheLeft && m_type == SectionType.Left)
				|| (!toTheLeft && m_type == SectionType.Right))
			{
				Push(player, pushForce);
                if (Game1.normalTotemCollisionSound_Channel1.State == SoundState.Playing)
                {
                    Game1.normalTotemCollisionSound_Channel2.Play();
                }
                else
                {
                    Game1.normalTotemCollisionSound_Channel1.Play();
                }
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
