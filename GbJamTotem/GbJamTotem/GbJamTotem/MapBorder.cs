using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
    public class MapBorder : GameObject
    {

        public Sprite pixelPlayer;
        public Sprite pixelTotem;
        const int m_mapFloor = 60;
        const int m_mapCeiling = 120;
        float topTotem;

        MoveToStaticAction m_slideToScreen;
        MoveToStaticAction m_slideOutOfScreen;
        Vector2 positionOnScreen = new Vector2(152, 72);
        Vector2 positionOutOfScreen = new Vector2(168, 72);
        const float durationSlide = 0.25f;

        SingleActionManager m_actionManager;

        public void setTopTotem()
        {
            if (Game1.CurrentTotem.Top != 0)
				topTotem = Game1.CurrentTotem.Top;
        }

        public MapBorder()
        {
            m_actionManager = new SingleActionManager();

            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("score_background"), m_transform);
            m_transform.Position = new Vector2(168, 72);

            // Get the highest point of totem at start
            //
			if (Game1.CurrentTotem.Top != 0)
				topTotem = Game1.CurrentTotem.Top;

            pixelPlayer = new Sprite(Program.TheGame, TextureLibrary.PixelSpriteSheet, new Transform(m_transform, true));
            pixelPlayer.Colour = new Vector4((float)255/(float)255, (float)251/(float)255, (float)240/(float)255, 1);
            pixelPlayer.Transform.PosX = -4;
            pixelPlayer.Transform.Scale = new Vector2(2, 2);

            pixelTotem = new Sprite(Program.TheGame, TextureLibrary.PixelSpriteSheet, new Transform(m_transform, true));
            pixelTotem.Colour = new Vector4((float)255 / (float)255, (float)251 / (float)255, (float)240 / (float)255, 1);
            pixelTotem.Origin = new Vector2(0.5f, 1);
            pixelTotem.Transform.PosY = m_mapFloor;
            pixelTotem.Transform.Scale = new Vector2(2, m_mapCeiling);

            m_slideToScreen = new MoveToStaticAction(Program.TheGame, m_transform, positionOnScreen, 1);
            m_slideToScreen.Interpolator = new PSmoothstepInterpolation();
            m_slideToScreen.Timer.Interval = durationSlide;

            m_slideOutOfScreen = new MoveToStaticAction(Program.TheGame, m_transform, positionOutOfScreen, 1);
            m_slideOutOfScreen.Interpolator = new PSmoothstepInterpolation();
            m_slideOutOfScreen.StartPosition = positionOnScreen;
            m_slideOutOfScreen.Timer.Interval = durationSlide;
        }

        public void Slide(bool onScreen)
        {
            if (onScreen)
                m_actionManager.StartNew(m_slideToScreen);

            if (!onScreen)
                m_actionManager.StartNew(m_slideOutOfScreen);

        }

        public override void Update()
        {
            // TODO
            // Mettre à jour la tour de totem ainsi que sa hauteur
            //

            float ratioPlayerPosition = Game1.player.Transform.PosY / topTotem;
			float ratioTotemState = Game1.CurrentTotem.Top / topTotem;

            pixelPlayer.Transform.PosY = (ratioPlayerPosition * -m_mapCeiling) + m_mapFloor;
            pixelTotem.Transform.SclY = (ratioTotemState * m_mapCeiling);

            m_actionManager.Update();
        }

        public override void Draw()
        {
            m_sprite.Draw();
            pixelPlayer.Draw();
            pixelTotem.Draw();
        }
    }
}
