using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackCreator : MonoBehaviour
{
	private System.Random rnd = new System.Random();
	public float trackWidth = 200;
	public float trackHeight = 160;
	public int trackPoints = 20;
	private List<Vector2> curve = new List<Vector2>();
	public List<Vector3> points = new List<Vector3>();

	static int GetMaxSegmentIndex(IEnumerable<Vector2> Curve)
	{
		int segmentIndex = 0;
		double segmentDistance = 0;
		int index = 0;
		Vector2 prevPoint = new Vector2();

		foreach (var point in Curve)
		{
			if (index != 0)
			{
				var curDistance = Distance(point, prevPoint);
				if (curDistance > segmentDistance)
				{
					segmentIndex = index - 1;
					segmentDistance = curDistance;
				}
			}
			index++;
			prevPoint = point;
		}
		return segmentIndex;
	}

	public void GenerateTrack(System.Random rnd = null)
	{
		if (rnd == null)
			rnd = new System.Random();
		this.rnd = rnd;

		var points = GenerateRandomTrack(trackWidth, trackHeight, trackPoints); //Get some points

		int startIndex = GetMaxSegmentIndex(points);
		startIndex = (int)((startIndex) * 2000 / (trackPoints + 1)); // ??????????????????
		var curve = Spline(points, 2000).ToList(); //Curvify the ordered list, 2000 is the resolution of the curve.

		var p1 = curve[startIndex];
		var p2 = p1 - curve[startIndex + 1];

		startPosition = p1;
		startOrientation = -Math.Atan2(p2.x, p2.y) * 180 / Math.PI;
		this.length = MeasureLength(curve);
		this.curve = curve;

		var pointsConverted = new List<Vector3>();

		foreach (var p in points)
		{
			pointsConverted.Add(new Vector3(p.x, 0, p.y));
		}

		this.points = pointsConverted;
	}

	private double startOrientation = 0;

	private Vector2 startPosition = new Vector2();

	private IEnumerable<Vector2> GenerateRandomTrack(float width, float height, int points)
	{
		IEnumerable<Vector2> best = null;
		var list = Enumerable.Range(0, points - 1).Select(p => new Vector2((float)rnd.NextDouble() * width, (float)rnd.NextDouble() * height));
		list = list.Concat(list.Take(1));
		int cnt = list.Count() - 1;
		double bestLen = double.PositiveInfinity;

		/// Iteration for the Simulated Annealing algorithm.
		/// Using Simulated Annealing algorithm to solve traveling salesman problem.
		/// Alistairs note - I cba to look up how this works, but it somehow returns a list of points in order of how to traverse them in some sensible way
		for (int i = 0; i < 2000; i++)
		{
			double len = MeasureLength(list); //Length of whole track
			int id1, id2;

			id1 = rnd.Next(cnt); //Get a new random number thats less than the number of points total in the list
			do id2 = rnd.Next(cnt); while (id2 == id1); //Same, but make sure other random number isnt same as the first

			if (id2 < id1) //If id2 less than id1, swap values?
				Swap(ref id1, ref id2);

			var newPoints = ReversePoints(list, id1, id2); //Ok?
			double newLen = MeasureLength(newPoints);

			double k = 100;
			double T = ((1.0 + k) / (i + 1 + k)) / 2;
			double p = Math.Exp(-(newLen - len) / (T * 100)) / 2;

			if (newLen > len && rnd.NextDouble() > p)
			{
			}
			else
			{
				list = newPoints.ToList(); // We have to convert it to list to get enumeration faster
				if (newLen < bestLen)
				{
					bestLen = newLen;
					best = list;
				}
			}
		}

		return best.Take(best.Count() - 1);
	}

	public static void Swap<T>(ref T lhs, ref T rhs)
	{
		T temp = lhs;
		lhs = rhs;
		rhs = temp;
	}

	private static IEnumerable<Vector2> ReversePoints(IEnumerable<Vector2> points, int start, int stop)
	{
		var count = points.Count();
		var l1 = points.Take(start);
		var l2 = points.Skip(start).Take(stop - start + 1).Reverse();
		var l3 = points.Skip(stop + 1);
		var np = l1.Concat(l2).Concat(l3);
		// Copy first point to the end
		return np.Take(count - 1).Concat(np.Take(1));
	}

	public static IEnumerable<Vector2> Spline(IEnumerable<Vector2> points, int count)
	{
		var points1 = new List<Vector2>(points);
		points1.Add(points1[1]);
		points1.Insert(0, points1[points1.Count - 3]);

		var distances = new List<double>();
		double distance = 0;
		for (int i = 0; i < points1.Count; i++)
		{
			if (i > 0)
				distance += Distance(points1[i], points1[i - 1]);
			distances.Add(distance);
		}

		var splineX = Interpolate.CubicSplineRobust(distances, points1.Select(v => (double)v.x));
		var splineY = Interpolate.CubicSplineRobust(distances, points1.Select(v => (double)v.y));

		// Distances without first and last points
		var dst = Enumerable.Range(0, count).Select(v => distances[1] + (distances[distances.Count - 2] - distances[1]) * v / (count - 1));
		return dst.Select(v => new Vector2((float)splineX.Interpolate(v), (float)splineY.Interpolate(v)));
	}



	private static double Distance(Vector2 p1, Vector2 p2)
	{
		return Vector2.Distance(p1, p2);
	}

	//Find length of the whole track
	//So I take Points and Points.Skip(1), and because list I'm comparing to is offset by 1 I'm getting the length between a point and the next point
	//in the list, using that .Zip.... very clever.
	//Then add the length between the last and the first point, as we won't have done that one.
	public static double MeasureLength(IEnumerable<Vector2> Points)
	{
		return Points.Zip(Points.Skip(1), Distance).Sum() + Distance(Points.First(), Points.Last());
	}

	public double length = 0;
	public double roadWidth = 2;
}
