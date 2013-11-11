using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GbJamTotem
{
    public class Tutorial : GameObject
    {
        bool tutorialMetal;
        bool tutorialSpike;
        bool tutorialPowerUp;

        public Tutorial(bool activated)
        {
            tutorialMetal = activated;
            tutorialSpike = activated;
            tutorialPowerUp = activated;
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Draw()
        {
            throw new NotImplementedException();
        }


    }
}
