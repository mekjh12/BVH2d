using System;
using System.Drawing;

namespace BVH2d
{
    class RndBrush
    {
        private static Random rand = new Random();

        public static Brush GetRandomBrush
        {
            get
            {
                Brush brush = new SolidBrush(Color.FromArgb((byte)rand.Next(0, 256), (byte)rand.Next(0, 256), (byte)rand.Next(0, 256)));
                return brush;
            }
        }
    }
}
