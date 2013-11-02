using System;

namespace GbJamTotem
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
		/// 

		public static Random Random = new Random();
		public static Game1 TheGame = new Game1();

        static void Main(string[] args)
        {
			TheGame.Run();
        }
    }
#endif
}

