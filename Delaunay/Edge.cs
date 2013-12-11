using System;
using System.Collections.Generic;
using System.Drawing;
	

namespace Delaunay
{
	
	
	/**
	 * The line segment connecting the two Sites is part of the Delaunay triangulation;
	 * the line segment connecting the two Vertices is part of the Voronoi diagram
	 * @author ashaw
	 * 
	 */
	public  class Edge
	{
		private static List<Edge> _pool = new List<Edge>();

		/**
		 * This is the only way to create a new Edge 
		 * @param site0
		 * @param site1
		 * @return 
		 * 
		 */
		internal static Edge createBisectingEdge(Site site0, Site site1)
		{
			float dx, dy, absdx, absdy;
			float a, b, c;
		
			dx = site1.x - site0.x;
			dy = site1.y - site0.y;
			absdx = dx > 0 ? dx : -dx;
			absdy = dy > 0 ? dy : -dy;
			c = site0.x * dx + site0.y * dy + (dx * dx + dy * dy) * 0.5f;
			if (absdx > absdy)
			{
				a = 1.0f; b = dy/dx; c /= dx;
			}
			else
			{
				b = 1.0f; a = dx/dy; c /= dy;
			}
			
			Edge edge = Edge.create();
		
			edge.leftSite = site0;
			edge.rightSite = site1;
			site0.addEdge(edge);
			site1.addEdge(edge);
			
			edge._leftVertex = null;
			edge._rightVertex = null;
			
			edge.a = a; edge.b = b; edge.c = c;
			//trace("createBisectingEdge: a ", edge.a, "b", edge.b, "c", edge.c);
			
			return edge;
		}

		private static Edge create()
		{
            Edge edge;
			if (_pool.Count > 0)
			{
				edge = _pool.Pop();
				edge.init();
			}
			else
			{
				edge = new Edge(typeof(PrivateConstructorEnforcer));
			}
			return edge;
		}
		
        //private static readonly Sprite LINESPRITE = new Sprite();
        //private static readonly Graphics GRAPHICS = LINESPRITE.graphics;
		
		private BitmapData _delaunayLineBmp;
        internal BitmapData delaunayLineBmp {
            get{
            
			if (_delaunayLineBmp == null)
			{
				_delaunayLineBmp = makeDelaunayLineBmp();
			}
			return _delaunayLineBmp;
        }
        }
		
		// making this available to Voronoi; running out of memory in AIR so I cannot cache the bmp
		internal BitmapData makeDelaunayLineBmp()
		{
            throw new NotImplementedException();
            //var p0 = leftSite.coord;
            //PointF p1 = rightSite.coord;
			
            //GRAPHICS.clear();
            //// clear() resets line style back to undefined!
            //GRAPHICS.lineStyle(0, 0, 1.0, false, LineScaleMode.NONE, CapsStyle.NONE);
            //GRAPHICS.moveTo(p0.x, p0.y);
            //GRAPHICS.lineTo(p1.x, p1.y);
						
            //int w = int(Math.Ceiling(Math.Max(p0.x, p1.x)));
            //if (w < 1)
            //{
            //    w = 1;
            //}
            //int h = int(Math.Ceiling(Math.Max(p0.y, p1.y)));
            //if (h < 1)
            //{
            //    h = 1;
            //}
            //BitmapData bmp = new BitmapData(w, h, true, 0);
            //bmp.draw(LINESPRITE);
            //return bmp;
		}

		public LineSegment delaunayLine()
		{
			// draw a line connecting the input Sites for which the edge is a bisector:
			return new LineSegment(leftSite.coord(), rightSite.coord());
		}

                public LineSegment voronoiEdge()
                {
                  if (!visible) return new LineSegment(PointF.Empty, PointF.Empty);
                  return new LineSegment(_clippedVertices[LR.LEFT],
                                         _clippedVertices[LR.RIGHT]);
                }

		private static int _nedges = 0;
		
		internal static readonly Edge DELETED = new Edge(typeof(PrivateConstructorEnforcer));
		
		// the equation of the edge: ax + by = c
		internal float a, b, c;
		
		// the two Voronoi vertices that the edge connects
		//		(if one of them is null, the edge extends to infinity)
		private Vertex _leftVertex;
		internal Vertex leftVertex
		{
    get {
			return _leftVertex;
}
		}
		private Vertex _rightVertex;
		internal Vertex rightVertex
		{
    get {
			return _rightVertex;
}
}
		internal Vertex vertex(LR leftRight)
		{
			return (leftRight == LR.LEFT) ? _leftVertex : _rightVertex;
		}
		internal void setVertex(LR leftRight, Vertex v)
		{
			if (leftRight == LR.LEFT)
			{
				_leftVertex = v;
			}
			else
			{
				_rightVertex = v;
			}
		}
		
		internal bool isPartOfConvexHull()
		{
			return (_leftVertex == null || _rightVertex == null);
		}
		
		public float sitesDistance()
		{
			return Utilities.Distance(leftSite.coord(), rightSite.coord());
		}
		
		public static float compareSitesDistances_MAX(Edge edge0,  Edge edge1)
		{
			float length0 = edge0.sitesDistance();
			float length1 = edge1.sitesDistance();
			if (length0 < length1)
			{
				return 1;
			}
			if (length0 > length1)
			{
				return -1;
			}
			return 0;
		}
		
		public static float compareSitesDistances(Edge edge0, Edge edge1)
		{
			return - compareSitesDistances_MAX(edge0, edge1);
		}
		
		// Once clipVertices() is called, this Dictionary will hold two PointFs
		// representing the clipped coordinates of the left and right ends...
        private Dictionary<LR, PointF> _clippedVertices;
        internal Dictionary<LR, PointF> clippedEnds
		{
            get
            {
                return _clippedVertices;
            }
		}
		// unless the entire Edge is outside the bounds.
		// In that case visible will be false:
		internal bool visible
		{
            get
            {
                return _clippedVertices != null;
            }
		}
		
		// the two input Sites for which this Edge is a bisector:
		private Dictionary<LR, Site> _sites;

internal Site leftSite {
set {
_sites[LR.LEFT] = value;
}
get {
return _sites[LR.LEFT];
}
}
internal Site rightSite {
set {
_sites[LR.RIGHT] = value;
}
get {
return _sites[LR.RIGHT];
}
}
		internal Site site(LR leftRight)
		{
			return _sites[leftRight] as Site;
		}
		
		private int _edgeIndex;
		
		public void dispose()
		{
			if (_delaunayLineBmp != null)
			{
				_delaunayLineBmp.dispose();
				_delaunayLineBmp = null;
			}
			_leftVertex = null;
			_rightVertex = null;
			if (_clippedVertices != null)
			{
				_clippedVertices[LR.LEFT] = PointF.Empty;
				_clippedVertices[LR.RIGHT] = PointF.Empty;
				_clippedVertices = null;
			}
			_sites[LR.LEFT] = null;
			_sites[LR.RIGHT] = null;
			_sites = null;
			
			_pool.Add(this);
		}

		public Edge(Type pce)
		{
			if (pce != typeof(PrivateConstructorEnforcer))
			{
				throw new Exception("Edge: static readonlyructor is private");
			}
			
			_edgeIndex = _nedges++;
			init();
		}
		
		private void init()
		{
            _sites = new Dictionary<LR, Site>();
		}
		
		public override string ToString()
		{
			return "Edge " + _edgeIndex + "; sites " + _sites[LR.LEFT] + ", " + _sites[LR.RIGHT]
					+ "; endVertices " + (_leftVertex != null ? _leftVertex.vertexIndex.ToString() : "null") + ", "
					 + (_rightVertex != null ? _rightVertex.vertexIndex.ToString() : "null") + "::";
		}

		/**
		 * Set _clippedVertices to contain the two ends of the portion of the Voronoi edge that is visible
		 * within the bounds.  If no part of the Edge falls within the bounds, leave _clippedVertices null. 
		 * @param bounds
		 * 
		 */
		internal void clipVertices(RectangleF bounds)
		{
			float xmin = bounds.X;
			float ymin = bounds.Y;
			float xmax = bounds.Right;
			float ymax = bounds.Bottom;

            Vertex vertex0, vertex1;
			float x0, x1, y0, y1;
			
			if (a == 1.0 && b >= 0.0)
			{
				vertex0 = _rightVertex;
				vertex1 = _leftVertex;
			}
			else 
			{
				vertex0 = _leftVertex;
				vertex1 = _rightVertex;
			}
		
			if (a == 1.0)
			{
				y0 = ymin;
				if (vertex0 != null && vertex0.y > ymin)
				{
					 y0 = vertex0.y;
				}
				if (y0 > ymax)
				{
					return;
				}
				x0 = c - b * y0;
				
				y1 = ymax;
				if (vertex1 != null && vertex1.y < ymax)
				{
					y1 = vertex1.y;
				}
				if (y1 < ymin)
				{
					return;
				}
				x1 = c - b * y1;
				
				if ((x0 > xmax && x1 > xmax) || (x0 < xmin && x1 < xmin))
				{
					return;
				}
				
				if (x0 > xmax)
				{
					x0 = xmax; y0 = (c - x0)/b;
				}
				else if (x0 < xmin)
				{
					x0 = xmin; y0 = (c - x0)/b;
				}
				
				if (x1 > xmax)
				{
					x1 = xmax; y1 = (c - x1)/b;
				}
				else if (x1 < xmin)
				{
					x1 = xmin; y1 = (c - x1)/b;
				}
			}
			else
			{
				x0 = xmin;
				if (vertex0 != null && vertex0.x > xmin)
				{
					x0 = vertex0.x;
				}
				if (x0 > xmax)
				{
					return;
				}
				y0 = c - a * x0;
				
				x1 = xmax;
				if (vertex1 != null && vertex1.x < xmax)
				{
					x1 = vertex1.x;
				}
				if (x1 < xmin)
				{
					return;
				}
				y1 = c - a * x1;
				
				if ((y0 > ymax && y1 > ymax) || (y0 < ymin && y1 < ymin))
				{
					return;
				}
				
				if (y0 > ymax)
				{
					y0 = ymax; x0 = (c - y0)/a;
				}
				else if (y0 < ymin)
				{
					y0 = ymin; x0 = (c - y0)/a;
				}
				
				if (y1 > ymax)
				{
					y1 = ymax; x1 = (c - y1)/a;
				}
				else if (y1 < ymin)
				{
					y1 = ymin; x1 = (c - y1)/a;
				}
			}

			_clippedVertices = new Dictionary<LR, PointF>();
			if (vertex0 == _leftVertex)
			{
				_clippedVertices[LR.LEFT] = new PointF(x0, y0);
				_clippedVertices[LR.RIGHT] = new PointF(x1, y1);
			}
			else
			{
				_clippedVertices[LR.RIGHT] = new PointF(x0, y0);
				_clippedVertices[LR.LEFT] = new PointF(x1, y1);
			}
		}

	}
}