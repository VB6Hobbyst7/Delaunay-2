using System.Collections.Generic;
	
using System.Drawing;

namespace Delaunay
{
    public class visibleLineSegmentsClass
    {
        static internal List<LineSegment> visibleLineSegments(List<Edge> edges)
        {
            List<LineSegment> segments = new List<LineSegment>();

            foreach (Edge edge in edges)
            {
                if (edge.visible)
                {
                    PointF p1 = edge.clippedEnds[LR.LEFT];
                    PointF p2 = edge.clippedEnds[LR.RIGHT];
                    segments.Add(new LineSegment(p1, p2));
                }
            }

            return segments;
        }
    }

}