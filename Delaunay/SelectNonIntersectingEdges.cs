using System.Drawing;
using System.Collections.Generic;

namespace Delaunay
{
    public class selectNonIntersectingEdgesClass
    {
        static BitmapData _keepOutMask;
        internal static List<Edge> selectNonIntersectingEdges(BitmapData keepOutMask, List<Edge> edgesToTest)
        {
            if (keepOutMask == null)
            {
                return edgesToTest;
            }
            _keepOutMask = keepOutMask;

            return edgesToTest.Filter(myTest);
        }
        static PointF zeroPointF = new PointF();

        static bool myTest(Edge edge, int index, List<Edge> vector)
        {
            BitmapData delaunayLineBmp = edge.makeDelaunayLineBmp();
            bool notIntersecting = !(_keepOutMask.hitTest(zeroPointF, 1, delaunayLineBmp, zeroPointF, 1));
            delaunayLineBmp.dispose();
            return notIntersecting;
        }
    }
}