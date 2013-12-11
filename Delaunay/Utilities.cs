using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delaunay
{
    class Utilities
    {
        public static float Distance(PointF one, PointF two)
        {
            float x = (two.X - one.X) * (two.X - one.X);
            float y = (two.Y - one.Y) * (two.Y - one.Y);
            return (float)Math.Sqrt(x + y);
        }
    }
}
