using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public struct Pair<T>
	{
		public T First { get; set; }

		public T Second { get; set; }

		public Pair(T first, T second)
		{
			First = first;
			Second = second;
		}

		public override string ToString()
		{
			return "[" + First + "," + Second + "]";
		}

		public Pair<T> Flip()
		{
			return new Pair<T>(Second, First);
		}
	}

	public struct Triple<T>
	{
		public T First { get; set; }

		public T Second { get; set; }

		public T Third { get; set; }

		public Triple(T first, T second, T third)
		{
			First = first;
			Second = second;
			Third = third;
		}

		public override string ToString()
		{
			return "[" + First + "," + Second + "," + Third + "]";
		}
	}

	public struct ShapePoint
	{
		public Vector3 Point { get; set; }

		public bool SoftNormal { get; set; }

		public ShapePoint(Vector3 point, bool softNormal = false)
		{
			Point = point;
			SoftNormal = softNormal;
		}

		public override string ToString()
		{
			return "[" + Point + "," + SoftNormal + "]";
		}
	}

	public struct TemplatePoint
	{
		public Vector2 Point { get; set; }

		public bool SoftNormal { get; set; }

		//This is for UV mapping.. but the V is the direction down the path, the U is the direction across the path (so the shape).
		//So only care about the U map
		public float UMap { get; set; }

		public TemplatePoint(Vector2 point, float uMap, bool softNormal = false)
		{
			Point = point;
			SoftNormal = softNormal;
			UMap = uMap;
		}

		public float x
		{
			get
			{
				return Point.x;
			}
		}

		public float y
		{
			get
			{
				return Point.y;
			}
		}

		public override string ToString()
		{
			return "[" + Point + "," + SoftNormal + "]";
		}
	}

	//Sourced from https://pastebin.com/iQDhQTFN
	public class LineEquation
	{
		public LineEquation(Vector2 start, Vector2 end)
		{
			Start = start;
			End = end;

			A = End.y - Start.y;
			B = Start.x - End.x;
			C = A * Start.x + B * Start.y;
		}
		public Vector2 Start { get; private set; }
		public Vector2 End { get; private set; }

		public float A { get; private set; }
		public float B { get; private set; }
		public float C { get; private set; }

		public Vector2? GetIntersectionWithLine(LineEquation otherLine)
		{
			double determinant = A * otherLine.B - otherLine.A * B;

			if (determinant > double.MaxValue - 10 || determinant < -(double.MaxValue - 10)) //lines are parallel
				return new Vector2();

			//Cramer's Rule
			double x = (otherLine.B * C - B * otherLine.C) / determinant;
			double y = (A * otherLine.C - otherLine.A * C) / determinant;

			Vector2 intersectionPoint = new Vector2((float)x, (float)y);

			return intersectionPoint;
		}
	}

	public static class Utils
	{
		public static Vector2 To2D(this Vector3 v)
		{
			return new Vector2(v.x, v.z);
		}

		public static Vector3 To3D(this Vector2 v)
		{
			return new Vector3(v.x, 0, v.y);
		}

		//Look at vector in front and behind and get average tangent of both
		public static Vector3 GetTangent(this List<Vector3> list, int i)
		{
			Vector3 forward = Vector3.zero;
			if (i < list.Count - 1)
			{
				forward += list[i + 1] - list[i];
			}

			if (i > 0)
			{
				forward += list[i] - list[i - 1];
			}
			forward.Normalize();

			return Vector3.Cross(forward, Vector3.up).normalized;
		}

		public static List<ShapePoint> GetShape(this List<Vector3> list, int i, List<TemplatePoint> extrudeShape)
		{
			var result = new List<ShapePoint>();
			var t = list.GetTangent(i);

			foreach (var point in extrudeShape)
			{
				var v = new ShapePoint((list[i] + t * point.x) + new Vector3(0, point.y, 0));
				result.Add(v);
			}

			return result;
		}

		public static Vector3 GetShapePoint(this List<Vector3> list, int i, Vector2 templatePoint)
		{
			var t = list.GetTangent(i);
			var result = (list[i] + t * templatePoint.x) + new Vector3(0, templatePoint.y, 0);
			return result;
		}

		public static Vector3 GetRoadPoint(this List<Vector3> list, int i, float pos)
		{
			var t = list.GetTangent(i);
			var result = list[i] + t * pos;
			return result;
		}

		public static int CountNormals(this List<TemplatePoint> list)
		{
			int result = 0;

			foreach (var p in list)
			{
				result += p.SoftNormal ? 1 : 2; //If its a hard normal, you need 2 verticies
			}

			return result;
		}
	}
}