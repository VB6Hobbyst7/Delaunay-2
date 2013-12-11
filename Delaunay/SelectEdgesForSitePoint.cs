using System.Drawing;

using System.Collections.Generic;


namespace Delaunay
{
	public class selectEdgesForSitePointFClass {
        static PointF _coord;
	internal static List<Edge> selectEdgesForSitePointF(PointF coord, List<Edge> edgesToTest)
	{
        _coord = coord;
		return edgesToTest.Filter(myTest);
	}

    static bool myTest(Edge edge, int index, List<Edge> vector)
		{
			return ((edge.leftSite != null && edge.leftSite.coord() == _coord)
			||  (edge.rightSite != null && edge.rightSite.coord() == _coord));
		}
}
}