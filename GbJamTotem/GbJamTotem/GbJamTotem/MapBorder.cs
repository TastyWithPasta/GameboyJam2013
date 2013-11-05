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

        public Sprite pixelMap;
        const int m_mapFloor = 30;
        const int m_mapCeiling = 120;
        float topTotem;


        public MapBorder()
        {

            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("score_background"), m_transform);
            m_sprite.Transform.Position = new Vector2(152, 72);

            if (Game1.m_totem.Top != 0)
                topTotem = Game1.m_totem.Top;

            pixelMap = new Sprite(Program.TheGame, TextureLibrary.PixelSpriteSheet, new Transform(m_transform, true));
            pixelMap.Origin = new Vector2(0, -m_mapFloor);
            pixelMap.Transform.Position = Game1.player.Transform.Position;
            pixelMap.Transform.Scale = new Vector2(2, 2);

        }

        public override void Update()
        {
            float ratio = Game1.player.Transform.PosY / topTotem;
            //pixelMap.Transform.PosY = -(Game1.player.Transform.PosY * m_mapScrollLimit)/Game1.m_totem.Top;
            pixelMap.Transform.PosY = ratio * -m_mapCeiling;
        }

        public override void Draw()
        {
            m_sprite.Draw();
            pixelMap.Draw();
        }
    }
}
