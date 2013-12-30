using System.Collections.Generic;
using System.Drawing;

namespace Delaunay
{
    public class selectNonIntersectingEdgesClass
    {
        static BitmapData _keepOutMask;
        static PointF zeroPointF = new PointF();

        internal static List<Edge> SelectNonIntersectingEdges(BitmapData keepOutMask, List<Edge> edgesToTest)
        {
            if (keepOutMask == null)
            {
                return edgesToTest;
            }
            _keepOutMask = keepOutMask;
            return edgesToTest.Filter(MyTest);
        }

        static bool MyTest(Edge edge, int index, List<Edge> vector)
        {
            BitmapData delaunayLineBmp = edge.MakeDelaunayLineBmp();
            bool notIntersecting = !(_keepOutMask.hitTest(zeroPointF, 1, delaunayLineBmp, zeroPointF, 1));
            delaunayLineBmp.dispose();
            return notIntersecting;
        }
    }
}