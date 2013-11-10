using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace GbJamTotem
{
    public class DynamicMusic
    {

        // TODO
        // - Généraliser à X couches (Rappel : limite de 16 en 16 bits)
        // 

        List<SoundEffectInstance> layerList;

        public enum dynamicMusicState{
            PLAYING,
            PAUSED,
            STOPPED
        }

        dynamicMusicState state;

        public dynamicMusicState State
        {
            get { return state; }
        }



        public DynamicMusic(SoundEffectInstance musicLayer1, SoundEffectInstance musicLayer2, SoundEffectInstance musicLayer3, SoundEffectInstance musicLayer4)
        {
            layerList = new List<SoundEffectInstance>();

            layerList.Add(musicLayer1);
            layerList.Add(musicLayer2);
            layerList.Add(musicLayer3);
            layerList.Add(musicLayer4); 

            for (int i = 0; i < layerList.Count; i++)
            {
                layerList[i].IsLooped = true;

                if (i != 0 )
                    layerList[i].Volume = 0f;
            }

            state = dynamicMusicState.STOPPED;

        }

        public void PlayDynamicMusic()
        {

            for (int i = 0; i < layerList.Count; i++)
            {
                layerList[i].Play();
            }

            state = dynamicMusicState.PLAYING;

        }

        public void PauseDynamicMusic()
        {
            for (int i = 0; i < layerList.Count; i++)
            {
                layerList[i].Pause();
            }

            state = dynamicMusicState.PAUSED;

        }

        public void StopDynamicMusic()
        {
            for (int i = 0; i < layerList.Count; i++)
            {
                layerList[i].Stop();
            }

            state = dynamicMusicState.STOPPED;
        }

        // Enable layers with 1,2,3... etc
        // (and not 0,1,2...)

        public void EnableLayer(int index)
        {
            if (index ==0)
                index = 1;

            if (index > layerList.Count)
                index = layerList.Count;

            layerList[index - 1].Volume = 1f;
        }

        public void DisableLayer(int index)
        {
            if (index ==0)
                index = 1;

            if (index > layerList.Count)
                index = layerList.Count;

            layerList[index - 1].Volume = 0f;
        }
        
        public void ResetSecondaryLayers(){
            for (int i = 1; i < layerList.Count; i++){
                layerList[i].Volume = 0f;
            }
        }

    }
}
