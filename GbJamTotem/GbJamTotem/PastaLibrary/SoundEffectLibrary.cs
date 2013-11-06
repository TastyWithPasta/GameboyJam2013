using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace PastaGameLibrary
{
	public static class SoundEffectLibrary
	{
		private static Dictionary<string, SoundEffect> m_soundEffects = new Dictionary<string, SoundEffect>();
		//private string m_basePath = "Content/";

		public static void LoadContent(ContentManager Content, string path)
		{
			string fullpath;
			if (path == "")
			{
				fullpath = Content.RootDirectory;
			}
			else
			{
				fullpath = Content.RootDirectory + "/" + path;
				path += "/";
			}

			DirectoryInfo di = new DirectoryInfo(fullpath);
			FileInfo[] files = di.GetFiles();
			int length = files.Length;

			for (int i = 0; i < length; ++i)
			{
				string name = files[i].Name.Substring(0, files[i].Name.Length - 4);
				m_soundEffects.Add(name, Content.Load<SoundEffect>(path + name));
			}
		}

		public static void UnloadContent()
		{
			m_soundEffects.Clear();
		}

		public static SoundEffect Get(string name)
		{
			return m_soundEffects[name];
		}
	}
}
