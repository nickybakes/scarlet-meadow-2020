using System;

namespace Scarlet_Meadow_Monogame
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class. "I bust, therefore I brown." - Buster Brown
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
#endif
}
