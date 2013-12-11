
	
using System.Drawing;
using System;
using System.Collections.Generic;

namespace Delaunay
{
	
	
	public  class Site : ICoord
	{
		private static List<Site> _pool = new List<Site>();
		public static Site create(PointF p, int index, float weight, uint color)
		{
			if (_pool.Count > 0)
			{
				return _pool.Pop().init(p, index, weight, color);
			}
			else
			{
				return new Site(typeof(PrivateConstructorEnforcer), p, index, weight, color);
			}
		}
		
		internal static void sortSites(List<Site> sites)
		{
			sites.SortFunc(Site.compare);
		}

		/**
		 * sort sites on y, then x, coord
		 * also change each site's _siteIndex to match its new position in the list
		 * so the _siteIndex can be used to identify the site for nearest-neighbor queries
		 * 
		 * haha "also" - means more than one responsibility...
		 * 
		 */
		private static float compare(Site s1, Site s2)
		{
			int returnValue = (int)Voronoi.compareByYThenX(s1, s2);
			
			// swap _siteIndex values if necessary to match new ordering:
			int tempIndex;
			if (returnValue == -1)
			{
				if (s1._siteIndex > s2._siteIndex)
				{
					tempIndex = (int)s1._siteIndex;
					s1._siteIndex = s2._siteIndex;
					s2._siteIndex = (uint)tempIndex;
				}
			}
			else if (returnValue == 1)
			{
				if (s2._siteIndex > s1._siteIndex)
				{
					tempIndex = (int)s2._siteIndex;
					s2._siteIndex = s1._siteIndex;
					s1._siteIndex = (uint)tempIndex;
				}
				
			}
			
			return returnValue;
		}


		private static readonly float EPSILON = .005f;
		private static bool closeEnough(PointF p0, PointF p1)
		{
			return Utilities.Distance(p0, p1) < EPSILON;
		}
				
		private PointF _coord;

        public PointF coord() {
            //get {
                return _coord;
            //}
        }
		
		internal uint color;
		internal float weight;
		
		private uint _siteIndex;
		
		// the edges that define this Site's Voronoi region:
		private List<Edge> _edges;

        internal List<Edge> edges {
            get {
                return _edges;
            }
        }
		// which end of each edge hooks up with the previous edge in _edges:
		private List<LR> _edgeOrientations;
		// ordered list of PointFs that define the region clipped to bounds:
		private List<PointF> _region;

		public  Site(Type pce, PointF p, int index, float weight, uint color)
		{
			if (pce != typeof(PrivateConstructorEnforcer))
			{
				throw new Exception("Site static readonlyructor is private");
			}
			init(p, index, weight, color);
		}
		
		private Site init(PointF p, int index, float weight, uint color)
		{
			_coord = p;
			_siteIndex = (uint)index;
			this.weight = weight;
			this.color = color;
			_edges = new List<Edge>();
			_region = null;
			return this;
		}
		
		public override string ToString()
		{
			return "Site " + _siteIndex + ": " + coord().ToString();
		}
		
		private void move(PointF p)
		{
			clear();
			_coord = p;
		}
		
		public void dispose()
		{
			_coord = PointF.Empty;
			clear();
			_pool.Add(this);
		}
		
		private void clear()
		{
			if (_edges != null)
			{
				_edges.Clear();
				_edges = null;
			}
			if (_edgeOrientations != null)
			{
				_edgeOrientations.Clear();
				_edgeOrientations = null;
			}
			if (_region != null)
			{
				_region.Clear();
				_region = null;
			}
		}
		
		internal void addEdge(Edge edge)
		{
			_edges.Add(edge);
		}
		
		internal Edge nearestEdge()
		{
			_edges.SortFunc(Edge.compareSitesDistances);
			return _edges[0];
		}
		
		internal List<Site> neighborSites()
		{
			if (_edges == null || _edges.Count == 0)
			{
				return new List<Site>();
			}
			if (_edgeOrientations == null)
			{ 
				reorderEdges();
			}
			List<Site> list = new List<Site>();
			foreach (Edge edge in _edges)
			{
				list.Add(neighborSite(edge));
			}
			return list;
		}
			
		private Site neighborSite(Edge edge)
		{
			if (this == edge.leftSite)
			{
				return edge.rightSite;
			}
			if (this == edge.rightSite)
			{
				return edge.leftSite;
			}
			return null;
		}
		
		internal List<PointF> region(RectangleF clippingBounds)
		{
			if (_edges == null || _edges.Count == 0)
			{
				return new List<PointF>();
			}
			if (_edgeOrientations == null)
			{ 
				reorderEdges();
				_region = clipToBounds(clippingBounds);
				if ((new Polygon(_region)).winding() == Winding.CLOCKWISE)
				{
					_region.Reverse();
				}
			}
			return _region;
		}
		
		private void reorderEdges()
		{
			//trace("_edges:", _edges);
			EdgeReorderer reorderer = new EdgeReorderer(_edges, typeof(Vertex));
			_edges = reorderer.edges;
			//trace("reordered:", _edges);
			_edgeOrientations = reorderer.edgeOrientations;
			reorderer.dispose();
		}
		
		private List<PointF> clipToBounds(RectangleF bounds)
		{
			List<PointF> PointFs = new List<PointF>();
			int n = _edges.Count;
			int i = 0;
			Edge edge;
			while (i < n && ((_edges[i] as Edge).visible == false))
			{
				++i;
			}
			
			if (i == n)
			{
				// no edges visible
				return new List<PointF>();
			}
			edge = _edges[i];
			LR orientation = _edgeOrientations[i];
			PointFs.Add(edge.clippedEnds[orientation]);
			PointFs.Add(edge.clippedEnds[LR.other(orientation)]);
			
			for (int j = i + 1; j < n; ++j)
			{
				edge = _edges[j];
				if (edge.visible == false)
				{
					continue;
				}
				connect(PointFs, j, bounds);
			}
			// close up the polygon by adding another corner PointF of the bounds if needed:
			connect(PointFs, i, bounds, true);
			
			return PointFs;
		}
		
		private void connect(List<PointF>PointFs, int j, RectangleF bounds)
        {
            connect(PointFs, j, bounds, false);
        }
		private void connect(List<PointF>PointFs, int j, RectangleF bounds, bool closingUp)
		{
			PointF rightPointF = PointFs[PointFs.Count - 1];
			Edge newEdge = _edges[j] as Edge;
			LR newOrientation = _edgeOrientations[j];
			// the PointF that  must be connected to rightPointF:
			PointF newPointF = newEdge.clippedEnds[newOrientation];
			if (!closeEnough(rightPointF, newPointF))
			{
				// The PointFs do not coincide, so they must have been clipped at the bounds;
				// see if they are on the same border of the bounds:
				if (rightPointF.X != newPointF.X
				&&  rightPointF.Y != newPointF.Y)
				{
					// They are on different borders of the bounds;
					// insert one or two corners of bounds as needed to hook them up:
					// (NOTE this will not be corRectangleF if the region should take up more than
					// half of the bounds RectangleF, for then we will have gone the wrong way
					// around the bounds and included the smaller part rather than the larger)
					int rightCheck = BoundsCheck.check(rightPointF, bounds);
					int newCheck = BoundsCheck.check(newPointF, bounds);
					float px, py;
                    //throw new NotImplementedException("Modified, might not work");
					if (rightCheck == BoundsCheck.RIGHT)
					{
						px = bounds.Right;
                        if (newCheck == BoundsCheck.BOTTOM)
						{
							py = bounds.Bottom;
							PointFs.Add(new PointF(px, py));
						}
                        else if (newCheck == BoundsCheck.TOP)
						{
							py = bounds.Top;
							PointFs.Add(new PointF(px, py));
						}
                        else if (newCheck == BoundsCheck.LEFT)
						{
							if (rightPointF.Y - bounds.Y + newPointF.Y - bounds.Y < bounds.Height)
							{
								py = bounds.Top;
							}
							else
							{
								py = bounds.Bottom;
							}
							PointFs.Add(new PointF(px, py));
							PointFs.Add(new PointF(bounds.Left, py));
						}
					}
                    else if (rightCheck == BoundsCheck.LEFT)
					{
						px = bounds.Left;
                        if (newCheck == BoundsCheck.BOTTOM)
						{
							py = bounds.Bottom;
							PointFs.Add(new PointF(px, py));
						}
                        else if (newCheck == BoundsCheck.TOP)
						{
							py = bounds.Top;
							PointFs.Add(new PointF(px, py));
						}
                        else if (newCheck == BoundsCheck.RIGHT)
						{
							if (rightPointF.Y - bounds.Y + newPointF.Y - bounds.Y < bounds.Height)
							{
								py = bounds.Top;
							}
							else
							{
								py = bounds.Bottom;
							}
							PointFs.Add(new PointF(px, py));
							PointFs.Add(new PointF(bounds.Right, py));
						}
					}
                    else if (rightCheck == BoundsCheck.TOP)
					{
						py = bounds.Top;
                        if (newCheck == BoundsCheck.RIGHT)
						{
							px = bounds.Right;
							PointFs.Add(new PointF(px, py));
						}
                        else if (newCheck == BoundsCheck.LEFT)
						{
							px = bounds.Left;
							PointFs.Add(new PointF(px, py));
						}
                        else if (newCheck == BoundsCheck.BOTTOM)
						{
							if (rightPointF.X - bounds.X + newPointF.X - bounds.X < bounds.Width)
							{
								px = bounds.Left;
							}
							else
							{
								px = bounds.Right;
							}
							PointFs.Add(new PointF(px, py));
							PointFs.Add(new PointF(px, bounds.Bottom));
						}
					}
                    else if (rightCheck == BoundsCheck.BOTTOM)
					{
						py = bounds.Bottom;
                        if (newCheck == BoundsCheck.RIGHT)
						{
							px = bounds.Right;
							PointFs.Add(new PointF(px, py));
						}
                        else if (newCheck == BoundsCheck.LEFT)
						{
							px = bounds.Left;
							PointFs.Add(new PointF(px, py));
						}
                        else if (newCheck == BoundsCheck.TOP)
						{
							if (rightPointF.X - bounds.X + newPointF.X - bounds.X < bounds.Width)
							{
								px = bounds.Left;
							}
							else
							{
								px = bounds.Right;
							}
							PointFs.Add(new PointF(px, py));
							PointFs.Add(new PointF(px, bounds.Top));
						}
					}
				}
				if (closingUp)
				{
					// newEdge's ends have already been added
					return;
				}
				PointFs.Add(newPointF);
			}
            PointF newRightPointF = newEdge.clippedEnds[LR.other(newOrientation)];
			if (!closeEnough(PointFs[0], newRightPointF))
			{
				PointFs.Add(newRightPointF);
			}
		}
								

internal float x
{
get {
return _coord.X;
}
}

internal float y
{
get {
return _coord.Y;
}
}
		
		internal float dist(ICoord p)
		{
			return Utilities.Distance(p.coord(), this._coord);
		}

	}
}


	
	 class BoundsCheck
	{
		public static readonly int TOP = 1;
		public static readonly int BOTTOM = 2;
		public static readonly int LEFT = 4;
		public static readonly int RIGHT = 8;
		
		/**
		 * 
		 * @param PointF
		 * @param bounds
		 * @return an int with the appropriate bits set if the PointF lies on the corresponding bounds lines
		 * 
		 */
		public static int check(PointF point, RectangleF bounds)
		{
            int value = 0;
			if (point.X == bounds.Left)
			{
				value |= LEFT;
			}
			if (point.X == bounds.Right)
			{
				value |= RIGHT;
			}
			if (point.Y == bounds.Top)
			{
				value |= TOP;
			}
			if (point.Y == bounds.Bottom)
			{
				value |= BOTTOM;
			}
			return value;
		}
		
		public BoundsCheck()
		{
			throw new Exception("BoundsCheck static readonlyructor unused");
		}

	}