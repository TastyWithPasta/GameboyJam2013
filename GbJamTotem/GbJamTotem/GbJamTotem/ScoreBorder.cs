using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
    public class ScoreBorder : GameObject
    {
        int score;
        int scoreBarHeight;
        public Sprite m_graphicScore;

        MoveToStaticAction m_slideToScreen;
        MoveToStaticAction m_slideOutOfScreen;
        Vector2 positionOnScreen = new Vector2(8, 72);
        Vector2 positionOutOfScreen = new Vector2(-8, 72);
        const float durationSlide = 0.25f;

        SingleActionManager m_actionManager;

        public int Score
        {
            get { return score; }
            set { score = value; }
        }

        public ScoreBorder(int heightMax)
        {
            m_actionManager = new SingleActionManager();

            score = 0;
            scoreBarHeight = heightMax;
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("score_background"), m_transform);
            m_transform.Position = new Vector2(-8, 72);

            m_graphicScore = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("score_foreground"), new Transform(m_transform, true));
            m_graphicScore.Transform.Position = new Vector2(0,72);
            m_graphicScore.Transform.SclY = 0f;

            m_slideToScreen = new MoveToStaticAction(Program.TheGame, m_transform, positionOnScreen, 1);
            m_slideToScreen.Interpolator = new PSmoothstepInterpolation();
            m_slideToScreen.Timer.Interval = durationSlide;

            m_slideOutOfScreen = new MoveToStaticAction(Program.TheGame, m_transform, positionOutOfScreen, 1);
            m_slideOutOfScreen.StartPosition = positionOnScreen;
            m_slideOutOfScreen.Interpolator = new PSmoothstepInterpolation();
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
            m_graphicScore.Transform.SclY = ((float)score / ((float)Game1.m_totem.TotalAmountOfSections * (float)Game1.normalTotemValue))*2;
            m_actionManager.Update();
        }

        public override void Draw()
        {
           m_sprite.Draw();
           m_graphicScore.Draw();
        }


    }
}
