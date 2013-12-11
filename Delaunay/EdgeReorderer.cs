using System;
using System.Collections.Generic;
using System.Drawing;

namespace Delaunay
{
	
	
	internal  class EdgeReorderer
	{
		private List<Edge> _edges;
		private List<LR> _edgeOrientations;
		public List<Edge> edges
		{
            get {
			return _edges;
            }
		}
		public List<LR> edgeOrientations
		{
            get {
			return _edgeOrientations;
            }
		}
		
		public EdgeReorderer( List<Edge> origEdges, Type criterion)
		{
			if (criterion != typeof(Vertex) && criterion != typeof(Site))
			{
				throw new ArgumentException("Edges: criterion must be Vertex or Site");
			}
			_edges = new List<Edge>();
			_edgeOrientations = new List<LR>();
			if (origEdges.Count > 0)
			{
				_edges = reorderEdges(origEdges, criterion);
			}
		}
		
		public void dispose()
		{
			_edges = null;
			_edgeOrientations = null;
		}

		private List<Edge> reorderEdges(List<Edge> origEdges, Type criterion)
		{
			int i;
			int j;
			int n = origEdges.Count;
			Edge edge;
			// we're going to reorder the edges in order of traversal
			List<bool> done = new List<bool>(n);
			int nDone = 0;
			for (int o = 0; o < n; o++)
			{
				done.Add(false);
			}
			List<Edge> newEdges = new List<Edge>();
			
			i = 0;
			edge = origEdges[i];
			newEdges.Add(edge);
			_edgeOrientations.Add(LR.LEFT);
			ICoord firstPointF = null;
            ICoord lastPointF = null;
            if ((criterion == typeof(Vertex)))
            {
                firstPointF = edge.leftVertex;
                lastPointF = edge.rightVertex;
            }
            else
            {
                firstPointF = edge.leftSite;
                lastPointF = edge.rightSite;
            }
			
			if (firstPointF == Vertex.VERTEX_AT_INFINITY || lastPointF == Vertex.VERTEX_AT_INFINITY)
			{
				return new List<Edge>();
			}
			
			done[i] = true;
			++nDone;
			
			while (nDone < n)
			{
				for (i = 1; i < n; ++i)
				{
					if (done[i])
					{
						continue;
					}
					edge = origEdges[i];
                    ICoord leftPointF = null;
                    ICoord rightPointF = null;
                    if ((criterion == typeof(Vertex)))
                    {
                        leftPointF = edge.leftVertex;
                        rightPointF = edge.rightVertex;
                    }
                    else
                    {
                        leftPointF = edge.leftSite;
                        rightPointF = edge.rightSite;
                    }
					if (leftPointF == Vertex.VERTEX_AT_INFINITY || rightPointF == Vertex.VERTEX_AT_INFINITY)
					{
						return new List<Edge>();
					}
					if (leftPointF == lastPointF)
					{
						lastPointF = rightPointF;
						_edgeOrientations.Add(LR.LEFT);
						newEdges.Add(edge);
						done[i] = true;
					}
					else if (rightPointF == firstPointF)
					{
						firstPointF = leftPointF;
                        _edgeOrientations.Unshift(LR.LEFT);
                        newEdges.Unshift(edge);
						done[i] = true;
					}
					else if (leftPointF == firstPointF)
					{
						firstPointF = rightPointF;
                        _edgeOrientations.Unshift(LR.RIGHT);
                        newEdges.Unshift(edge);
						done[i] = true;
					}
					else if (rightPointF == lastPointF)
					{
						lastPointF = leftPointF;
						_edgeOrientations.Add(LR.RIGHT);
						newEdges.Add(edge);
						done[i] = true;
					}
					if (done[i])
					{
						++nDone;
					}
				}
			}
			
			return newEdges;
		}

	}
}