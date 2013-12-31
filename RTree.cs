/**
  * Copyright (C) 2013-2014 IIIT-Delhi <http://www.iiitd.ac.in>
  * Author: D Bera <dbera.web@gmail.com>
  * 
  * Distributed under MIT License:
  * Permission is hereby granted, free of charge, to any person obtaining a
  * copy of this software and associated documentation files (the
  * "Software"), to deal in the Software without restriction,
  * including without limitation the rights to use, copy, modify, merge,
  * publish, distribute, sublicense, and/or sell copies of the Software, and to
  * permit persons to whom the Software is furnished to do so, subject to the
  * following conditions:
  * 
  * The above copyright notice and this permission notice shall be included in
  * all copies or substantial portions of the Software.
  * 
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
  * SIMON TATHAM BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
  * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
  * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  * SOFTWARE. 
  */

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Diagnostics;
using SpatioTextual;

namespace SpatioTextual
{
    public class RTree<TPoint> : Database<TPoint> where TPoint : PointBase  {

	private class RTreeNode
        {
            public static readonly int MIN_LOAD = 6;
            public static readonly int MAX_LOAD = 10;

            private int fill = 0;
            public virtual int Load { get { return fill; }}

            private RTreeNode[] children;
            public RTreeNode[] Children { get { return children; }}

            public MBR<TPoint> MBR { get; set; }

            public RTreeNode() : this(false) { }
            public RTreeNode(bool leaf)
            {
                if (!leaf)
                    children = new RTreeNode[RTreeNode.MAX_LOAD];
                this.fill = 0;
                this.MBR = null;
            }
            
            public virtual Boolean IsLeaf { get { return false; } }

            public RTreeNode this[int i]
            {
                get
                {
                    if (i >= fill)
                        throw new IndexOutOfRangeException ("Not enough children: " + i);
                    return children[i];
                }
            }

            // @returns False if Node is full after adding, true otherwise
            internal bool AddEntryReturnSpaceLeft (RTreeNode entry)
            {
                //Console.WriteLine(String.Format("Add Entry {0}, current load = {1}({2})", entry, fill, MAX_LOAD));
                if (fill == RTreeNode.MAX_LOAD)
                    throw new IndexOutOfRangeException ("Node is full.");

                //Console.WriteLine("Previous MBR:" + this.mbr);
                // Recompute (update) MBR
                if (Load == 0)
                    this.MBR = entry.MBR.Duplicate();//this.mbr.Duplicate(entry.MBR);
                else
                    this.MBR.Expand(entry.MBR);
                //Console.WriteLine("New MBR:" + this.mbr);

                // Add entry to the children list
                children[fill] = entry;
                fill++;

                //Console.WriteLine(String.Format("After add Entry {0}, current load = {1}({2})", entry, fill, MAX_LOAD));
                return !(Load == RTreeNode.MAX_LOAD);
            }

            public override string ToString()
            {
                if (this.Load == 0)
                    return "[]";
                return this.MBR.ToString() + "(" + this.Load + ")";
            }
        }

        class LeafNode : RTreeNode
        {
            private TPoint point;
	    public TPoint Point { get { return point; }}

            public LeafNode (TPoint point) : base(true)
            {
                this.point = point;
                this.MBR = new MBRSpatial<TPoint>(point);
		//this.MBR.Expand(point);
            }
        
            public override int Load { get { return 1; }}

            public override Boolean IsLeaf { get { return true; } }

            public override String ToString ()
            {
                return String.Join (",", point);
            }

            public class LeafNodeComparer:IComparer <RTreeNode>
            {
                int coord;              // dimension \in {0,1,2,...}

                public LeafNodeComparer (int coord) { this.coord = coord; }
                
                public int Compare (RTreeNode L1, RTreeNode L2)
                {
                    if (! (L1 is LeafNode) || ! (L2 is LeafNode))
                        throw new ArgumentException("L1 and L2 should be both LeafNodes");
                    return ((LeafNode)L1).point[coord].CompareTo (((LeafNode)L2).point[coord]);
                }
            }
        }

        private int dim;
        private RTreeNode root;
        public int Dimension { get { return dim; }}

        public RTree(int d) { this.dim = d;}

        public void Load (IEnumerable<TPoint> data)
	{
	    LoadSTR(data);
	}

        private void LoadSTR (IEnumerable<TPoint> data)
        {
            /* 0. Create LeafNodes
             * 1. Sort by y, and make sqrt(N/Load) groups of size sqrt(NL) : G1 G2 ...
             * 2. For each group G1, sort by x and make sqrt(N/Load) groups of size L : G11 G12 ...
             */

            // 1. Create all leafnodes
            List <RTreeNode> leafnodes = new List <RTreeNode> ();
            int N = 0;

            foreach (TPoint point in data)
            {
                if (point.Dimension != Dimension) throw new ArgumentException(String.Format("Data dimension {0} does not match RTree dimension {1}", point.Dimension, Dimension));
                N++;
                leafnodes.Add (new LeafNode (point));
            }

            // 2. Sort leafnodes in slices and groups
            for (int k = 1; k <= Dimension; k++)
            {
                SortLeaves (leafnodes, Dimension, k);
            }

            // 3. Create RTreeNode in groups of MAX_LOAD
            IList<RTreeNode> newlist = new List<RTreeNode>(); // Holder for rtreenodes until added to root
            IList<RTreeNode> curlist = leafnodes;
            RTreeNode parent_node = new RTreeNode();

            int depth = 0;

            do {
                //Console.WriteLine("Processing depth = from leaf to " + depth);
                foreach (RTreeNode node in curlist) {
                    //Console.WriteLine("Adding node: " + node);
                    //Console.WriteLine("To parent_node: " + parent_node);
                    if (! parent_node.AddEntryReturnSpaceLeft(node)) {
                        //Console.WriteLine("Node filled:" + parent_node);
                        newlist.Add(parent_node);
                        parent_node = new RTreeNode();
                    }
                }
                //Console.WriteLine("No more node in curlist for this depth");
                if (parent_node.Load > 0) {
                    //Console.WriteLine("Adding leftover nodes");
                    newlist.Add(parent_node);
                    parent_node = new RTreeNode();
                }

                curlist = newlist;
                newlist = new List<RTreeNode>();
                //Console.WriteLine("Curlist length:" + curlist.Count);
                depth ++;
            } while(curlist.Count >= 2);

            this.root = curlist[0];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("----- [RTree] -----");
            sb.Append(Environment.NewLine);
            ToStringRecursive(root, 1, sb);
            return sb.ToString();
        }

        private void ToStringRecursive(RTreeNode node, int indent, StringBuilder sb)
        {
            if (node == null)
                return;

            //Console.WriteLine("Inspecing {0}", node);

            for (int i=0; i < indent; ++i) sb.Append('\t');
            sb.Append(node.ToString());
            sb.Append('\n');

            if (node.Children == null)
                return;

            foreach (RTreeNode childnode in node.Children)
                ToStringRecursive(childnode, indent+1, sb);
        }

        // Sort the leafnodes based on dimension
        /*
         * Generalization of the following:
         * Initially, the number of leaf nodes is determined, which is P = N/C .
         * Let S = sqrt(P) . The
         * rectangles are sorted with respect to the x coordinate of the centroids, and S slices
         * are created. Each slice contains S·C rectangles, which are consecutive in the sorted
         * list. In each slice, the objects are sorted by the y coordinate of the centroids and
         * are packed into nodes (placing C objects in a node). The method is applied until
         * all R-tree levels are formulated.
         * For a general case when sorting in the k-th dimension out of d dimension, the width of
         * slice should be N^{1-k/d}/L^{k/d} (d=1..., k=0...). Initially, the width is entire set.
         */
        private static void SortLeaves (List<RTreeNode> leafnodes,
                                        int totaldim, int dim)
        {
            int begin = 0, N = leafnodes.Count;
            int S = (int) Math.Ceiling(Math.Pow ((float) N / (float) RTreeNode.MAX_LOAD,
                                    (float) (dim-1) / (float) totaldim));
            //Console.WriteLine("{0} {1} {2} {3} {4}", S, N, RTreeNode.MAX_LOAD, dim, totaldim);
            int W = N / S;

            //Console.Out.WriteLine (N + "," + S + "," + W);
            LeafNode.LeafNodeComparer comparer = new LeafNode.LeafNodeComparer (dim-1);
            while (begin < N)
            {
                leafnodes.Sort (begin, (begin + W - 1 < N ? W : N - begin),
                                comparer);
                begin += W;
            }
        }

        internal static void Test_SortLeaves(List<TPoint> list, int totaldim, int dim, IEnumerable<string> sorted_list)
        {
            List <RTreeNode> newlist = new List <RTreeNode>();
            foreach (TPoint t in list)
                newlist.Add(new LeafNode(t));

            SortLeaves(newlist, totaldim, dim);
            Console.WriteLine("---- Testing SortLeaves on {0}/{1} ----", dim, totaldim);

            IEnumerator<string> sorted_enumerator = sorted_list.GetEnumerator();

            foreach (LeafNode e in newlist) {
                sorted_enumerator.MoveNext();
                Console.WriteLine("    {0} vs {1}", e, sorted_enumerator.Current);
            }
            Console.WriteLine("---------------------");
        }

	private class kNN_NodeComparer : IComparer<RTreeNode>
	{
	    private TPoint q;

	    public kNN_NodeComparer(TPoint q) { this.q = q; }

	    // Return -1 if n1.mbr is closer to q than n2.mbr is
	    public int Compare(RTreeNode n1, RTreeNode n2)
	    {
		float d1 = n1.MBR.MinDistance(q);
		float d2 = n2.MBR.MinDistance(q);
		return (int)(d2-d1);
	    }
	}

	internal class TopKQueue : C5.IntervalHeap<Tuple<double,TPoint>>
	{
	    private class TopKQueueComparer : IComparer<Tuple<double,TPoint>>
	    {
		//private bool descending;
		//public TopKQueueComparer(bool descending) { this.descending = descending; }
		public int Compare(Tuple<double,TPoint> t1, Tuple<double,TPoint> t2)
		{
		    //if (descending)
		    return (int)(t1.Item1-t2.Item1);
		}
	    }

	    int k;
	    bool topk_min;
	    public TopKQueue(int k, bool topk_min) : base(k+1,new TopKQueueComparer())
	    {
		this.k = k; this.topk_min = topk_min;
	    }

	    public void Add(double val, TPoint point)
	    {
		base.Add(Tuple.Create<double,TPoint>(val, point));
		if (Count <= k)
		    return;
		// Else
		if(topk_min)
		    DeleteMax();
		else
		    DeleteMin();
	    }

	    public TPoint TopK
	    {
		get {
		    if (Count < k)
		        throw new Exception("Insufficient data");
		    if (topk_min)
		        return FindMax().Item2;
		    else
		        return FindMin().Item2;
		}
	    }
	}

	public TPoint kNN(TPoint q, int k)
	{
	    TopKQueue min_k = new TopKQueue(k,true);
	    C5.IntervalHeap<RTreeNode> pq = new C5.IntervalHeap<RTreeNode>(new kNN_NodeComparer(q));

	    pq.Add(root);
	    while(! pq.IsEmpty)
	    {
		RTreeNode node = pq.DeleteMin();
		float dist = node.MBR.MinDistance(q);
		if (node.IsLeaf) {
		    //Console.WriteLine(node + ":" + node.GetType());
		    min_k.Add(dist,((LeafNode)node).Point);
		} else {
		    foreach (RTreeNode child in node.Children)
			pq.Add(child);
		}
	    }

	    return min_k.TopK;
	}

	public IEnumerable<TPoint> Points
	{
	    get {
		Queue<RTreeNode> BFS_queue = new Queue<RTreeNode>();
		BFS_queue.Enqueue(root);

		while(BFS_queue.Count > 0) {
		    RTreeNode node = BFS_queue.Dequeue();
		    if (node == null)
			continue;
		    //Console.WriteLine("Checking:" + node);
		    if (! node.IsLeaf) {
			foreach(RTreeNode child in node.Children)
			    BFS_queue.Enqueue(child);
			continue;
		    }

		    // If node is leafnode
		    yield return ((LeafNode)node).Point;
		}
	    }
	}

	/*
	public IEnumerable<TPoint> RkNN(TPoint p, int k)
	{
	    // Default
	}
	*/
    }
}
