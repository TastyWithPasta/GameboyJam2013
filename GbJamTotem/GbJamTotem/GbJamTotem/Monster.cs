using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
	public class Monster : GameObject
	{
		const float MonsterPosX = 400;
		const float MonsterPosY = 80;

		SpriteSheetAnimation m_idle, m_openMouth, m_closeMouth;
		SingleActionManager m_actionManager;
		SpriteSheet m_spriteSheetNomNom, m_spriteSheetIdle;

		public Monster() : base()
		{
			m_spriteSheetNomNom = TextureLibrary.GetSpriteSheet("anim_monstre_miam", 8, 1);
			m_spriteSheetIdle = TextureLibrary.GetSpriteSheet("anim_monstre_neutre", 5, 1);
			m_sprite = new Sprite(Program.TheGame, m_spriteSheetIdle, m_transform);
			m_sprite.PixelCorrection = true;
			m_idle = new SpriteSheetAnimation(m_sprite, 0, 4, 3.0f, -1);
			m_actionManager = new SingleActionManager();
			m_transform.Position = new Vector2(MonsterPosX, MonsterPosY);
		}

		public void OpenMouth()
		{
			m_sprite.SpriteSheet = m_spriteSheetNomNom;
			m_actionManager.StartNew(m_openMouth);
		}
		public void CloseMouth()
		{
			m_sprite.SpriteSheet = m_spriteSheetNomNom;
			m_actionManager.StartNew(m_closeMouth);
		}
		public void Idle()
		{
			m_sprite.SpriteSheet = m_spriteSheetIdle;
			m_actionManager.StartNew(m_idle);
		}

		public override void Update()
		{
			m_actionManager.Update();
		}
		public override void Draw()
		{
			m_sprite.Draw();
		}
	}
}
