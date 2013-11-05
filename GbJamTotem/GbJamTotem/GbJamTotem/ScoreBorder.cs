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

        public int Score
        {
            get { return score; }
            set { score = value; }
        }

        public ScoreBorder(int heightMax)
        {
            score = 0;
            scoreBarHeight = heightMax;
            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("score_background"), m_transform);
            m_sprite.Transform.Position = new Vector2(8,72);

            m_graphicScore = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("score_foreground"), new Transform(m_transform, true));
            m_graphicScore.Transform.Position = new Vector2(0,72);
            m_graphicScore.Transform.SclY = 0f;

        }

        public override void Update()
        {
           m_graphicScore.Transform.SclY = ((float)score / ((float)Game1.m_totem.TotalAmountOfSections * (float)Game1.normalTotemValue))*2;
        }

        public override void Draw()
        {
            m_sprite.Draw();
            m_graphicScore.Draw();
        }


    }
}
