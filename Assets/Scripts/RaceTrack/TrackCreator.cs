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
	public List<Vector3> curve = new List<Vector3>();
	public List<Vector3> points = new List<Vector3>();

	public List<Vector3> bottom = new List<Vector3>();
	public List<Vector3> right = new List<Vector3>();
	public List<Vector3> top = new List<Vector3>();
	public List<Vector3> left = new List<Vector3>();

	public int highwayIntersections = 1;
	public float highwayMaxDeviation = 50;

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

	public void GenerateHighway(System.Random rnd = null)
	{
		if (rnd == null)
			rnd = new System.Random();

		this.rnd = rnd;

		var points = GenerateRandomHighway(trackWidth, trackHeight, highwayIntersections, highwayMaxDeviation); //Get some points

		var pointsConverted = new List<Vector3>();
		//Convert to Vector3 and make sure its in center of the scene
		pointsConverted = points.Select(p => new Vector3(p.x - (trackWidth / 2), 0, p.y - (trackHeight / 2))).ToList();

		this.points = pointsConverted;
	}

	public void GenerateRoadSection()
	{
		var bottom = GenerateRandomRoadSection(trackWidth, trackHeight, highwayIntersections, highwayMaxDeviation, Side.Bottom); //Get some points
		var right = GenerateRandomRoadSection(trackWidth, trackHeight, highwayIntersections, highwayMaxDeviation, Side.Right); //Get some points
		var top = GenerateRandomRoadSection(trackWidth, trackHeight, highwayIntersections, highwayMaxDeviation, Side.Top); //Get some points
		var left = GenerateRandomRoadSection(trackWidth, trackHeight, highwayIntersections, highwayMaxDeviation, Side.Left); //Get some points

		this.bottom = bottom.Select(p => new Vector3(p.x - (trackWidth / 2), 0, p.y - (trackHeight / 2))).ToList();
		this.right = right.Select(p => new Vector3(p.x - (trackWidth / 2), 0, p.y - (trackHeight / 2))).ToList();
		this.top = top.Select(p => new Vector3(p.x - (trackWidth / 2), 0, p.y - (trackHeight / 2))).ToList();
		this.left = left.Select(p => new Vector3(p.x - (trackWidth / 2), 0, p.y - (trackHeight / 2))).ToList();
	}

	public void GenerateTrack(System.Random rnd = null)
	{
		if (rnd == null)
			rnd = new System.Random();
		this.rnd = rnd;

		var points = GenerateRandomTrack(trackWidth, trackHeight, trackPoints); //Get some points
		

		int startIndex = GetMaxSegmentIndex(points);
		startIndex = (int)((startIndex) * 2000 / (trackPoints + 1)); // ??????????????????
		var curve = AkimaSpline(points, 2000).ToList(); //Curvify the ordered list, 2000 is the resolution of the curve.

		var curveConverted = new List<Vector3>();
		//Convert to Vector3 and make sure its in center of the scene
		curveConverted = curve.Select(p => new Vector3(p.x - (trackWidth / 2), 0, p.y - (trackHeight / 2))).ToList();

		this.curve = curveConverted;

		//Remove any weird kinks which can get into the track
		//TODO

		//Remove the last point, its duplicated for the curve above
		points = points.Take(points.Count() - 1);
		if (IsClockwise(points.ToList()))
			points = points.Reverse();

		var pointsConverted = new List<Vector3>();
		//Convert to Vector3 and make sure its in center of the scene
		pointsConverted = points.Select(p => new Vector3(p.x - (trackWidth / 2), 0, p.y - (trackHeight / 2))).ToList();

		this.points = pointsConverted;
	}

	public void GenerateTrackAkima(System.Random rnd = null)
	{
		if (rnd == null)
			rnd = new System.Random();
		this.rnd = rnd;

		var points = GenerateRandomTrack(trackWidth, trackHeight, trackPoints); //Get some points

		int startIndex = GetMaxSegmentIndex(points);
		startIndex = (int)((startIndex) * 2000 / (trackPoints + 1)); // ??????????????????
		var curve = AkimaSpline(points, 2000).ToList(); //Curvify the ordered list, 2000 is the resolution of the curve.
		this.curve = curve.Select(c => new Vector3(c.x, 0, c.y)).ToList();
	}

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

		return best;
	}

	private IEnumerable<Vector2> GenerateRandomHighway(float width, float height, int intersections, float maxDeviation)
	{
		var start = UnityEngine.Random.Range(0f, 1f);
		var end = UnityEngine.Random.Range(0f, 1f);

		List<Vector2> points = new List<Vector2>();

		Vector2 highwayStart;
		Vector2 highwayEnd;

		//Coinflip if it is going across or down the map
		if (UnityEngine.Random.Range(0f, 1f) > .5f)
		{
			highwayStart = new Vector2(start * width, 0);
			highwayEnd = new Vector2(end * width, height);
		}
		else
		{
			highwayStart = new Vector2(0, start * height);
			highwayEnd = new Vector2(width, end * height);
		}

		//Split evenly with no. of intersections
		//Add 2 to account for the start and end postion
		for (int i = 0; i <= intersections + 1; i++)
		{
			points.Add(Vector2.Lerp(highwayStart, highwayEnd, i / (float)(intersections + 1)));
		}

		//Work out the normal to the two points
		var normal = Vector2.Perpendicular(highwayEnd - highwayStart).normalized;

		//Move each point a certain amount along the normal by the maxDeviation * some random amount (except the first and last point)
		//Start at i = 1 and end at count - 1 so the first and last point stay the same
		for (int i = 1; i < points.Count - 1; i++)
		{
			//Make sure it stays in the bounds of the height & width
			do
			{
				var seed = UnityEngine.Random.Range(-1f, 1f);
				points[i] = points[i] + (normal * (maxDeviation * seed));
			}
			while (points[i].x > width || points[i].y > height || points[i].x < 0 || points[i].y < 0);
		}

		return points;
	}

	public enum Side {
		Bottom,
		Right,
		Top,
		Left
	}

	private IEnumerable<Vector2> GenerateRandomRoadSection(float width, float height, int intersections, float maxDeviation, Side side)
	{
		List<Vector2> points = new List<Vector2>();

		var startSeed = UnityEngine.Random.Range(0f, 1f);
		var endSeed = UnityEngine.Random.Range(0f, 1f);
		Vector2 roadStart = new Vector2();
		Vector2 roadEnd = new Vector2();

		switch (side)
		{
			case Side.Bottom:
				{
					roadStart = new Vector2(0, startSeed * .3f * height);
					roadEnd = new Vector2(width, endSeed * .3f * height);
				}
				break;
			case Side.Right:
				{
					roadStart = new Vector2(width - (startSeed * .3f * width), 0);
					roadEnd = new Vector2(width - (endSeed * .3f * width), height);
				}
				break;
			case Side.Top:
				{
					roadStart = new Vector2(0, height - (startSeed * .3f * height));
					roadEnd = new Vector2(width, height - (endSeed * .3f * height));
				}
				break;
			case Side.Left:
				{
					roadStart = new Vector2(startSeed * .3f * width, 0);
					roadEnd = new Vector2(endSeed * .3f * width, height);
				}
				break;
		}

		//Split evenly with no. of intersections
		//Add 2 to account for the start and end postion
		for (int i = 0; i <= intersections + 1; i++)
		{
			points.Add(Vector2.Lerp(roadStart, roadEnd, i / (float)(intersections + 1)));
		}

		//Work out the normal to the two points
		var normal = Vector2.Perpendicular(roadStart - roadEnd).normalized;

		//Move each point a certain amount along the normal by the maxDeviation * some random amount (except the first and last point)
		//Start at i = 1 and end at count - 1 so the first and last point stay the same
		for (int i = 1; i < points.Count - 1; i++)
		{
			//Make sure it stays in the bounds of the height & width
			do
			{
				var seed = UnityEngine.Random.Range(-1f, 1f);
				points[i] = points[i] + (normal * (maxDeviation * seed));
			}
			while (points[i].x > width || points[i].y > height || points[i].x < 0 || points[i].y < 0);
		}

		return points;
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

	public static IEnumerable<Vector2> AkimaSpline(IEnumerable<Vector2> points, int count)
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

	public bool IsClockwise(IList<Vector2> vertices)
	{
		double sum = 0.0;
		for (int i = 0; i < vertices.Count; i++)
		{
			Vector2 v1 = vertices[i];
			Vector2 v2 = vertices[(i + 1) % vertices.Count];
			sum += (v2.x - v1.x) * (v2.y + v1.y);
		}
		return sum > 0.0;
	}

	public double length = 0;
	public double roadWidth = 2;
}
