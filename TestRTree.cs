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

namespace SpatioTextual
{
    public class TestRTree
    {
	public static void Main(string[] args)
	{
            //TestSortOneOneDimension();
            //TestSortOneTwoDimension();
            //TestSortTwoThreeDimension();
	    TestLoad();
	    //TestKNN();
	    //Test_TopKQueue();
	}

        private static void TestLoad()
        {
            List<Point1D> list = new List<Point1D>();
            list.Add (new Point1D(27));
            list.Add (new Point1D(5));
            list.Add (new Point1D(3));
            list.Add (new Point1D(37));
            list.Add (new Point1D(6));
            list.Add (new Point1D(89));
            list.Add (new Point1D(2));
            list.Add (new Point1D(9));
            list.Add (new Point1D(67));
            list.Add (new Point1D(87));
            list.Add (new Point1D(1));
            list.Add (new Point1D(79));
            list.Add (new Point1D(72));
            list.Add (new Point1D(17));

            List <string> sorted_list = new List<string>();
            sorted_list.Add ("1");
            sorted_list.Add ("2");
            sorted_list.Add ("3");
            sorted_list.Add ("5");
            sorted_list.Add ("6");
            sorted_list.Add ("9");
            sorted_list.Add ("17");
            sorted_list.Add ("27");
            sorted_list.Add ("37");
            sorted_list.Add ("67");
            sorted_list.Add ("72");
            sorted_list.Add ("79");
            sorted_list.Add ("87");
            sorted_list.Add ("89");

            //RTree<Point1D>.Test_SortLeaves(list, 1, 1, sorted_list);
            RTree<Point1D> rt = new RTree<Point1D>(1);
            rt.Load(list);
            Console.Out.WriteLine(rt);
	    Console.WriteLine("-------- Testing Points --------");
	    foreach(Point1D point in rt.Points)
		Console.WriteLine(point);
	    Console.WriteLine("-------- Testing Points --------");
        }

        private static void TestSortOneOneDimension ()
        {
            List<Point1D> list = new List<Point1D>();

            list.Add (new Point1D(5));
            list.Add (new Point1D(3));
            list.Add (new Point1D(6));
            list.Add (new Point1D(1));
            list.Add (new Point1D(12));
            list.Add (new Point1D(21));
            list.Add (new Point1D(41));
            list.Add (new Point1D(14));
            list.Add (new Point1D(16));
            list.Add (new Point1D(2));
            list.Add (new Point1D(9));
            list.Add (new Point1D(7));
            list.Add (new Point1D(8));
            
            List <string> sorted_list = new List<string>();
            sorted_list.Add ("1");
            sorted_list.Add ("2");
            sorted_list.Add ("3");
            sorted_list.Add ("5");
            sorted_list.Add ("6");
            sorted_list.Add ("7");
            sorted_list.Add ("8");
            sorted_list.Add ("9");
            sorted_list.Add ("12");
            sorted_list.Add ("14");
            sorted_list.Add ("16");
            sorted_list.Add ("21");
            sorted_list.Add ("41");
            
            RTree<Point1D>.Test_SortLeaves(list, 1, 1, sorted_list);
        }
                
        private static void TestSortOneTwoDimension ()
        {
            List <Point2D> list = new List <Point2D> ();
            list.Add (new Point2D (5, 27));
            list.Add (new Point2D (3, 12));
            list.Add (new Point2D (6, 82));
            list.Add (new Point2D (1, 42));
            list.Add (new Point2D (2, 2));
            list.Add (new Point2D (9, 67));
            list.Add (new Point2D (7, 23));

            List<string> sorted_list = new List<string>();
            sorted_list.Add("1,42");
            sorted_list.Add("2,2");
            sorted_list.Add("3,12");
            sorted_list.Add("5,27");
            sorted_list.Add("6,82");
            sorted_list.Add("7,23");
            sorted_list.Add("9,67");

            RTree<Point2D>.Test_SortLeaves(list, 2, 1, sorted_list);
        }
            
        private static void TestSortTwoThreeDimension ()
        {
            List <Point3D> list = new List <Point3D> ();
            list.Add (new Point3D (5, 27, 3));
            list.Add (new Point3D (3, 12, 12));
            list.Add (new Point3D (6, 82, 6));
            list.Add (new Point3D (1, 42, 19));
            list.Add (new Point3D (2, 2, 16));
            list.Add (new Point3D (9, 67, 2));
            list.Add (new Point3D (7, 23, 1));
            list.Add (new Point3D (8, 11, 9));

            List<string> sorted_list = new List<string>();
            sorted_list.Add("2,2,16");
            sorted_list.Add("8,11,9");
            sorted_list.Add("3,12,12");
            sorted_list.Add("7,23,1");
            sorted_list.Add("5,27,3");
            sorted_list.Add("1,42,19");
            sorted_list.Add("9,67,2");
            sorted_list.Add("6,82,6");

            RTree<Point3D>.Test_SortLeaves(list, 3, 2, sorted_list);
        }

        private static void TestKNN()
        {
            List<Point1D> list = new List<Point1D>();
            list.Add (new Point1D(25));
            list.Add (new Point1D(5));
            list.Add (new Point1D(3));
            list.Add (new Point1D(37));
            list.Add (new Point1D(6));
            list.Add (new Point1D(89));
            list.Add (new Point1D(2));
            list.Add (new Point1D(9));
            list.Add (new Point1D(67));
            list.Add (new Point1D(87));
            list.Add (new Point1D(1));
            list.Add (new Point1D(79));
            list.Add (new Point1D(72));
            list.Add (new Point1D(17));

            RTree<Point1D> rt = new RTree<Point1D>(1);
            rt.Load(list);
	    Debug.Assert(rt.kNN(new Point1D(8), 1).ToString() == "(9)", "1-NN of 8 is not 9");
	    Debug.Assert(rt.kNN(new Point1D(8), 2).ToString() == "(25)", "2-NN of 8 is not 25");
	}

	private static void Test_TopKQueue()
	{
	    RTree<Point1D>.TopKQueue queue = new RTree<Point1D>.TopKQueue(3, true);
	    queue.Add(5, new Point1D(5));
	    queue.Add(15, new Point1D(15));
	    queue.Add(8, new Point1D(8));
	    Debug.Assert(queue.TopK.ToString() == "(15)");
	    queue.Add(4, new Point1D(4));
	    Debug.Assert(queue.TopK.ToString() == "(8)");
	    queue.Add(4, new Point1D(40));
	    Debug.Assert(queue.TopK.ToString() == "(5)");
	    queue.Add(5, new Point1D(14));
	    queue.Add(2, new Point1D(2));
	    Debug.Assert(queue.TopK.ToString() == "(40)");
	}
    }
}
