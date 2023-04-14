using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eXNB
{
    internal class Program
    {
        public static Game1 game;
        static void Main(string[] args)
        {
            game = new Game1(args);

            using (game)
            {
                game.Run();
            }
        }
    }
}
