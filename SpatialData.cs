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
using System.Diagnostics;
using System.Collections.Generic;
using SpatioTextual;

namespace SpatioTextual {

    public abstract class Point : IPoint {
	internal double[] p;
	protected int dim;

	public Point(int dim)
	{
	    this.dim = dim;
	    p = new double[dim];
	    //Console.WriteLine("Constructing Point with dimension:" + this.Dimension + " (" + this.dim + ")");
	}

        public virtual IComparable this[int coord]
        {
	        get {
		        if (coord >= dim)
		            throw new IndexOutOfRangeException("Point1D: coord = " + coord);
        		return this.p[coord];
	        }
	        protected set {
        		if (coord >= dim)
		            throw new IndexOutOfRangeException("Point: coord = " + coord);
        		if (! (value is double))
		            throw new ArgumentException("val of type:" + value.GetType());
        		this.p[coord] = (double) value;
	        }
        }

        public virtual int Dimension { get { return dim; }}

	public double Distance(IPoint p)
	{
	    Point q = p as Point;
	    if (q == null)
		throw new ArgumentException("IPoint is not of type Point");
	    double dist = 0;
	    for (int i=0; i<dim; ++i)
		dist += ((this.p[i] - q.p[i])*(this.p[i] - q.p[i]));
	    return Math.Sqrt(dist);
	}

	public IPoint MoveTo(int coord, IComparable val)
	{
	    if (! (val is double))
		throw new ArgumentException("val of type:" + val.GetType());
	    Point newp = Duplicate() as Point;
	    newp.p[coord] = (double) val;
	    return newp;
	}

	public abstract IPoint Duplicate();

	public override string ToString()
	{
	    return "(" + String.Join(",", this.p) + ")";
	}

    }

    public class PointBase : Point
    {
	public static PointBase Create(double[] pt)
	{
	    if (pt.Length == 1) return new Point1D(pt[0]);
	    if (pt.Length == 2) return new Point2D(pt[0], pt[1]);
	    if (pt.Length == 3) return new Point3D(pt[0], pt[1], pt[2]);
	    throw new ArgumentException("Invalid array size");
	}

	public PointBase(double[] pt) : base(pt.Length)
	{
	    pt.CopyTo(this.p, 0);
	}

	public PointBase(int dim, double p=0, double q=0, double r=0) : base(dim)
	{
	    if (dim >= 1) this.p[0] = p;
	    if (dim >= 2) this.p[1] = q;
	    if (dim >= 3) this.p[2] = r;
	}

	public override IPoint Duplicate()
	{
	    return null; // empty & dummy
	}

	public IPoint Duplicate(PointBase pt)
	{
	    for (int i=0; i < pt.dim; ++i)
		pt.p[i] = this.p[i]; // TODO Do a deep copy
	    return pt;
	}
    }

    public class Point1D : PointBase
    {
        public Point1D (double p) : base (1, p) { }
	public Point1D (double[] p) : base (p) { }
	public override IPoint Duplicate() { return Duplicate(new Point1D(0)); }
    }

    public class Point2D : PointBase
    {
        public Point2D (double p, double q) : base(2, p, q) { }
	public Point2D (double[] p) : base (p) { }
	public override IPoint Duplicate() { return Duplicate(new Point2D(0,0)); }
    }
    
    public class Point3D : PointBase
    {
        public Point3D (double p, double q, double r):base (3, p, q, r) { }
	public Point3D (double[] p) : base (p) { }
	public override IPoint Duplicate() { return Duplicate(new Point3D(0,0,0)); }
    }

    public class MBRSpatial<T> : MBR<T> where T:PointBase
    {
	public MBRSpatial(T p) : base(p) { }
	public MBRSpatial(T p, T q) : base(p, q) { }
	public MBRSpatial(MBRSpatial<T> mbr) : base(mbr) { }
	public MBRSpatial(Circle<T> circle) : base(circle.Center)
	{
	    for (int i=0; i < circle.Center.Dimension; ++i) {
		BL.p[i] = (BL.p[i] - circle.Radius);
		TR.p[i] = (TR.p[i] + circle.Radius);
	    }
	    Console.WriteLine("MBR({0}) = {1}", circle, this);
	}

	public override MBR<T> Duplicate()
	{
	    return new MBRSpatial<T>(this);
	}

	public override double MinDistance(T pt)
	{
	    double[] BL = this.BL.p;
	    double[] TR = this.TR.p;
	    double[] p = pt.p;
	    double dist = 0.0;
	    for (int i=0; i < this.BL.Dimension; ++i) {
		if (p[i].CompareTo(BL[i]) < 0)
		    dist += ((BL[i] - p[i])*(BL[i] - p[i]));
		else if(p[i] > TR[i])
		    dist += ((TR[i] - p[i])*(TR[i] - p[i]));
	    }

	    return Math.Sqrt(dist);
	}

	public override double MaxDistance(T pt)
	{
	    double[] BL = this.BL.p;
	    double[] TR = this.TR.p;
	    double[] p = pt.p;
	    double dist = 0.0;
	    for (int i=0; i < this.BL.Dimension; ++i) {
		if (p[i].CompareTo((BL[i]+TR[i])/2) < 0)
		    dist += ((TR[i] - p[i])*(TR[i] - p[i]));
		else
		    dist += ((BL[i] - p[i])*(BL[i] - p[i]));
	    }

	    return Math.Sqrt(dist);
	}

	public IEnumerable<T> Vertices {
	    get {
		int dim = BL.Dimension;
		int num = (int) Math.Pow(2, dim);

		for (int v=0; v<num; ++v) {
		    //Point vertex = BL.Duplicate() as T; // create new point
		    double[] vertex = new double[dim];
		    for (int bit=0; bit<dim; ++bit) {
			if ((v & (1 << bit))==(1<<bit))
			    vertex[bit] = (double) TR.p[bit];
			else
			    vertex[bit] = (double) BL.p[bit];
		    }
		    yield return PointBase.Create(vertex) as T;
		}
	    }
	}

	/*
	public override void ExpandToCircle(T center, double radius)
	{
	    for (int i=1; i <= center.dimension; ++i) {
		BL[i] = (BL[i] - radius);
		TR[i] = (TR[i] + radius);
	    }
	}*/

    }

    public class Circle<TPoint> where TPoint: PointBase {
	TPoint center;
        public TPoint Center { get { return center; }}

	double radius;
	public double Radius { get { return radius; }}

	public Circle(TPoint p, double r)
	{
	    this.center = p;
	    this.radius = r;
	}

	public bool Overlaps(TPoint pt)
	{
	    return (Center.Distance(pt) <= Radius);
	}

	// Check if circle overlaps with mbr, and if overlaps, then
	// whether the circle contains the mbr
	public bool Overlaps(MBRSpatial<TPoint> mbr, out bool contains)
	{
	    /*
	     * 4 cases:
	     *   a. Circle contains MBR -- contains (iff all vertices of MBR are in circle)
	     *   b. MBR contains Circle -- overlap (iff MBR(Circle) is contained in MBR)
	     *   c. MBR and Circle are partially overlapping -- overlap (iff some vertex of MBR is contained in Circle)
	     *   d. O/W disjoint
	     */

	    bool some_overlap = false;
	    contains = true;
	    foreach (TPoint vertex in mbr.Vertices) {
		bool overlap = Overlaps(vertex);
		contains = contains && overlap;
		some_overlap = some_overlap || overlap;
	    }

	    Console.WriteLine("Checking if {0} Overlaps {1}: contains = {2}, some_overlap = {3}", this, mbr, contains, some_overlap);
	    if (contains || some_overlap)
		    return true;

	    // Circle could be contained in MBR, which also counts as overlap
	    return mbr.Contains(new MBRSpatial<TPoint>(this));
	}

	public override string ToString()
	{
	    return string.Format("Circle({0},{1})", Center, Radius);
	}
    }

    public class Test {
	// Need to: export MONO_TRACE_LISTENER=Console.Out and compile using: dmcs -d:DEBUG 
	public static void Main(string[] args)
	{
	    TestContains();
	}

	private static void TestDistance()
	{
	    double d1 = (new Point1D(3)).Distance(new Point1D(16));
	    Debug.Assert(d1 == 13, String.Format("Distance between 3 and 16 is not 13 but {0}", d1));
	}

        private static void TestContains()
        {
            Debug.Assert(new MBRSpatial<Point1D>(new Point1D(3), new Point1D(10)).Contains(new MBRSpatial<Point1D>(new Point1D(4))), "[3,10] does not contain [4]!");
            Debug.Assert(new MBRSpatial<Point1D>(new Point1D(3), new Point1D(10)).Contains(new MBRSpatial<Point1D>(new Point1D(4), new Point1D(7))), "[3,10] does not contain [4,7]!");
            Debug.Assert(! new MBRSpatial<Point1D>(new Point1D(3), new Point1D(10)).Contains(new MBRSpatial<Point1D>(new Point1D(4), new Point1D(77))), "[3,10] contains [4,77]!");
            Debug.Assert(new MBRSpatial<Point2D>(new Point2D(3,5), new Point2D(10,10)).Contains(new MBRSpatial<Point2D>(new Point2D(4,6), new Point2D(5,7))), "[(3,5),(10,10)] does not contain [(4,6),(5,7)]");
            Debug.Assert(new MBRSpatial<Point3D>(new Point3D(3,5,5), new Point3D(10,10,12)).Contains(new MBRSpatial<Point3D>(new Point3D(4,6,6), new Point3D(5,7,11))), "[(3,5,5)(10,10,12)] does not contain [(4,6,6)(5,7,11)]");
        }

	private static void TestExpand()
    	{
    	    MBRSpatial<Point1D> mbr = new MBRSpatial<Point1D>(new Point1D(3), new Point1D(10));
    	    bool ret = mbr.Expand(new MBRSpatial<Point1D>(new Point1D(4)));
    	    Debug.Assert(ret == false, "[3,10] expands after adding [4])!");
    	    Debug.Assert(mbr.ToString() == "[(3),(10)]", "[(3),(10)] expands after adding [(4)]) to " + mbr);
    	
    	    Point3D p1 = new Point3D(3,5,5);
    	    Point3D p2 = new Point3D(10,10,12);
    	    MBRSpatial<Point3D> m = new MBRSpatial<Point3D>(p1, p2);
    	    Point3D ap1 = new Point3D(4,6,6);
    	    Point3D ap2 = new Point3D(5,17,11);
    	    MBRSpatial<Point3D> am = new MBRSpatial<Point3D>(ap1, ap2);
    	
    	    ret = m.Expand(am);
    	    Debug.Assert(ret == true, "[(3,5,5)(10,10,12)] does not expand after adding [(4,6,6)(5,17,11)]");
    	    Debug.Assert(m.ToString() == "[(3,5,5),(10,17,12)]", "MBR Expand error: [(3,5,5),(10,10,12)] + [(4,6,6),(5,7,11)] = " + m);
    	    Debug.Assert(p1.ToString() + p2.ToString() == "(3,5,5)(10,10,12)", "MBR Expand changes original points");
    	}
    }
}
