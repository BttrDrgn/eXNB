using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eXNB
{
    internal class Program
    {
        public static Game1 game = new Game1();
        static void Main(string[] args)
        {
            using (game)
            {
                game.Run();
            }
        }
    }
}
