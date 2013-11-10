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
        int scoreBarMaxValue;
        int scoreMultiplierMax = 9;

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

        public int ScoreBarMaxValue
        {
            get { return scoreBarMaxValue; }
        }

        public int ScoreMultiplierMax
        {
            get { return scoreMultiplierMax; }
        }

        public ScoreBorder()
        {
            m_actionManager = new SingleActionManager();

            score = 0;

            scoreBarMaxValue = calculateScoreMax();

            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("score_background"), m_transform);
            m_transform.Position = new Vector2(-8, 72);

            m_graphicScore = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("score_foreground"), new Transform(m_transform, true));
            m_graphicScore.Origin = new Vector2(0.5f, 1);
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

        public int calculateScoreMax()
        {
            int nbSoulsCollectible = 0;
            int multiplierFactor = 0;

			for (int i = 0; i < Game1.testTotem.AttachedSections.Count; i++)
            {
				nbSoulsCollectible += Game1.testTotem.AttachedSections[i].GetSoulCount();

                // Count n first souls, else add scoreMultiplierMax
                //
                if (multiplierFactor < scoreMultiplierMax)
                {
                    multiplierFactor += i;
                }
                else
                {
                    multiplierFactor += scoreMultiplierMax;
                }

            }

            return nbSoulsCollectible + multiplierFactor;
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
            //m_graphicScore.Transform.SclY = ((float)score / ((float)Game1.m_totem.TotalAmountOfSections * (float)Game1.normalTotemValue));
            m_graphicScore.Transform.SclY = (float)score/(float)scoreBarMaxValue;
            m_actionManager.Update();
        }

        public override void Draw()
        {
           m_sprite.Draw();
           m_graphicScore.Draw();
        }


    }
}
