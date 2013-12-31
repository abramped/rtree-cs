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

namespace SpatioTextual {
    public interface IPoint
    {
    	IComparable this[int coord] { get; }
    	int Dimension { get; }
	float Distance(IPoint q);
	IPoint MoveTo(int coord, IComparable new_val);
	IPoint Duplicate();
	string ToString();
    }

    public interface Database<TPoint> where TPoint:IPoint {
	void Load(IEnumerable<TPoint> data);
	IEnumerable<TPoint> Points { get; }
	TPoint kNN(TPoint p, int k);
	//IEnumerable<TPoint> RkNN(TPoint p, int k);
    }

    public abstract class MBR<TPoint> where TPoint: class, IPoint {
	private TPoint bl = null, tr = null;

	public TPoint BL 
	{
	    get { return bl; }
	    set { this.bl = value; }
	}

	public TPoint TR
	{
	    get { return tr; }
	    set { this.tr = value; }
	}

	public MBR () { }

	public MBR (TPoint p)
	{
	    this.BL = this.TR = p;
	}

	public MBR (TPoint bl, TPoint tr)
	{
	    this.BL = bl;
	    this.TR = tr;
	}

	public MBR (MBR<TPoint> mbr)
	{
	    this.BL = mbr.BL;
	    this.TR = mbr.TR;
	}

	public override string ToString()
	{
	    return String.Format("[{0},{1}]", this.BL, this.TR);
	}

	public abstract MBR<TPoint> Duplicate();

        public bool Contains(MBR<TPoint> in_mbr)
        {
	    MBR<TPoint> out_mbr = this;
            int dim = out_mbr.BL.Dimension;
            if (out_mbr.TR.Dimension != dim ||
                in_mbr.BL.Dimension != dim ||
                in_mbr.TR.Dimension != dim)
                throw new ArgumentException(String.Format("Dimension mismatch ({0},{1}) vs ({2},{3})",
                    out_mbr.BL.Dimension,
                    out_mbr.TR.Dimension,
                    in_mbr.BL.Dimension,
                    in_mbr.TR.Dimension));

            for (int i=0; i < dim; ++ i) {
                if (in_mbr.BL[i].CompareTo(out_mbr.BL[i]) < 0)
                    return false;
                if (in_mbr.TR[i].CompareTo(out_mbr.TR[i]) > 0)
                    return false;
            }
            return true;
        }

	public bool Expand(TPoint point)
	{
	    return Expand(point, point);
	}

	public bool Expand(MBR<TPoint> extra_mbr)
	{
	    return Expand(extra_mbr.BL, extra_mbr.TR);
	}

	public bool Expand(TPoint extra_mbr_BL, TPoint extra_mbr_TR)
	{
	    if (this.BL == null) {
		this.BL = extra_mbr_BL;
		this.TR = extra_mbr_TR;
		return true;
	    }

            // Increase orig_mbr to accomodate extra_mbr
            // For BL, use lower value for all coordinates
	    // FOR TR, use higher value for all coordinates
	    bool extended = false;
	    //Console.WriteLine("Expand " + this + " with " + extra_mbr);
            for (int i=0; i<this.BL.Dimension; ++i) {
		//Console.WriteLine("Comparing BL -- this:" + extra_mbr_BL[i] + " vs " + this.BL[i]);
                if (extra_mbr_BL[i].CompareTo(this.BL[i]) < 0) {
                    this.BL = (TPoint) this.BL.MoveTo(i, extra_mbr_BL[i]);
		    //Console.WriteLine("Updating BL to " + this.BL[i]);
		    extended = true;
		}
		//Console.WriteLine("Comparing TR -- this:" + extra_mbr_TR[i] + " vs " + this.TR[i]);
                if (extra_mbr_TR[i].CompareTo(this.TR[i]) > 0) {
                    this.TR = (TPoint) this.TR.MoveTo(i, extra_mbr_TR[i]);
		    //Console.WriteLine("Updating TR to " + this.TR[i]);
		    extended = true;
		}
            }

	    //Console.WriteLine("Expanded MBR:" + this);
	    return extended;
	}

	public abstract float MinDistance(TPoint p);
    }

}
