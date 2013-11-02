using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
	class GameboyDrawer
	{
		RenderTarget2D m_renderTarget;
		Rectangle m_drawingRectangle;
		MyGame m_theGame;

		public GameboyDrawer(MyGame game)
		{
			m_theGame = game;
			m_drawingRectangle = new Rectangle(0, 0, game.ScreenWidth, game.ScreenHeight);
			m_renderTarget = new RenderTarget2D(m_theGame.GraphicsDevice, 160, 144);
		}

		public void SetRenderTarget()
		{
			m_theGame.GraphicsDevice.SetRenderTarget(m_renderTarget);
		}

		public void Draw()
		{
			//m_theGame.GraphicsDevice.Clear(m_bgColor);
			m_theGame.GraphicsDevice.SetRenderTarget(null);
			m_theGame.SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null);
			m_theGame.SpriteBatch.Draw(m_renderTarget, m_drawingRectangle, Color.White);
			m_theGame.SpriteBatch.End();
		}
	}
}
