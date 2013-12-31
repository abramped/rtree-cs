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
	internal int[] p;
	protected int dim;

	public Point(int dim)
	{
	    this.dim = dim;
	    p = new int[dim];
	    //Console.WriteLine("Constructing Point with dimension:" + this.Dimension + " (" + this.dim + ")");
	}

        public virtual IComparable this[int coord]
        {
	        get {
		        if (coord > dim)
		            throw new IndexOutOfRangeException("Point1D: coord = " + coord);
        		return this.p[coord];
	        }
	        protected set {
        		if (coord > dim)
		            throw new IndexOutOfRangeException("Point: coord = " + coord);
        		if (! (value is int))
		            throw new ArgumentException("val of type:" + value.GetType());
        		this.p[coord] = (int) value;
	        }
        }

        public virtual int Dimension { get { return dim; }}

	public float Distance(IPoint p)
	{
	    Point q = p as Point;
	    if (q == null)
		throw new ArgumentException("IPoint is not of type Point");
	    float dist = 0;
	    for (int i=0; i<dim; ++i)
		dist += ((this.p[i] - q.p[i])*(this.p[i] - q.p[i]));
	    return (float)Math.Sqrt(dist);
	}

	public IPoint MoveTo(int coord, IComparable val)
	{
	    if (! (val is int))
		throw new ArgumentException("val of type:" + val.GetType());
	    Point newp = Duplicate() as Point;
	    newp.p[coord] = (int) val;
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
	public PointBase(int dim, int p=0, int q=0, int r=0) : base(dim)
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
        public Point1D (int p) : base (1, p) { }
	public override IPoint Duplicate() { return Duplicate(new Point1D(0)); }
    }

    public class Point2D : PointBase
    {
        public Point2D (int p, int q) : base(2, p, q) { }
	public override IPoint Duplicate() { return Duplicate(new Point2D(0,0)); }
    }
    
    public class Point3D : PointBase
    {
        public Point3D (int p, int q, int r):base (3, p, q, r) { }
	public override IPoint Duplicate() { return Duplicate(new Point3D(0,0,0)); }
    }

    public class MBRSpatial<T> : MBR<T> where T:PointBase
    {
	public MBRSpatial(T p) : base(p) { }
	public MBRSpatial(T p, T q) : base(p, q) { }
	public MBRSpatial(MBRSpatial<T> mbr) : base(mbr) { }

	public override MBR<T> Duplicate()
	{
	    return new MBRSpatial<T>(this);
	}

	public override float MinDistance(T pt)
	{
	    int[] BL = this.BL.p;
	    int[] TR = this.TR.p;
	    int[] p = pt.p;
	    float dist = 0.0f;
	    for (int i=0; i < this.BL.Dimension; ++i) {
		if (p[i].CompareTo(BL[i]) < 0)
		    dist += ((BL[i] - p[i])*(BL[i] - p[i]));
		else if(p[i] > TR[i])
		    dist += ((TR[i] - p[i])*(TR[i] - p[i]));
	    }

	    return (float)Math.Sqrt(dist);
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
	    float d1 = (new Point1D(3)).Distance(new Point1D(16));
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
