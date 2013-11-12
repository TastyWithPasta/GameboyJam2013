using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
	public class ScoreDisplay 
	{
		Transform m_transform;
		float m_value = 0;
		MoveToStaticAction m_slideIn, m_slideOut;
		static Vector2 inPos = new Vector2(64, -4);
		static Vector2 outPos = new Vector2(160, -4);
		SingleActionManager m_actions;

		public float Value
		{
			get { return m_value; }
			set { m_value = value; }
		}

		public ScoreDisplay()
		{
			m_transform = new Transform();

			m_slideIn = new MoveToStaticAction(Program.TheGame, m_transform, inPos, 1);
			m_slideIn.StartPosition = outPos;
			m_slideIn.Timer.Interval = 0.25f;
			m_slideIn.Interpolator = new PSmoothstepInterpolation();

			m_slideOut = new MoveToStaticAction(Program.TheGame, m_transform, outPos, 1);
			m_slideOut.StartPosition = inPos;
			m_slideOut.Timer.Interval = 0.25f;
			m_slideOut.Interpolator = new PSmoothstepInterpolation();

			m_actions = new SingleActionManager();
			m_transform.Position = outPos;
		}

		public void Initialise()
		{
			m_value = 0;
		}

		public void SlideIn()
		{
			m_actions.StartNew(m_slideIn);
		}

		public void SlideOut()
		{
			m_actions.StartNew(m_slideOut);
		}

		public void Draw()
		{
			m_actions.Update();
			Vector2 position = new Vector2((int)m_transform.PosX, (int)m_transform.PosY);
			Program.TheGame.SpriteBatch.DrawString(Game1.menuText, m_value.ToString(), position, Color.Black);
			Program.TheGame.SpriteBatch.DrawString(Game1.menuText, m_value.ToString(), position, Color.Black);
			Program.TheGame.SpriteBatch.DrawString(Game1.menuText, m_value.ToString(), position, Color.Black);
		}
	}
}
