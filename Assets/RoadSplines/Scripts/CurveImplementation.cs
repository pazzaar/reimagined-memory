using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Utils;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode()]
public class CurveImplementation : MonoBehaviour
{
	List<Vector3> ControlPoints;
	List<Pair<Vector3>> exitControlPoints;
	public TrackCreator trackMaker;
	public List<List<Vector3>> connections = new List<List<Vector3>>();

	public List<List<Pair<Vector3>>> highways = new List<List<Pair<Vector3>>>();
	public List<List<Pair<Vector3>>> roads = new List<List<Pair<Vector3>>>();

	public List<Vector3> firstRoad;
	public List<Vector3> secondRoad;

	public bool drawEdges = true;
	[Tooltip("The alpha is a catmull-rom spline specific thing, will make the curve's tangents more or less agressive")]
	public float alpha = .5f;

	public bool closedLoop = true;

	public int CurveResolution = 10;
	public float MeshResolution = 5;

    public float extrude = 10;

    public float edgeWidth = 2;
    public float thickness = 1;

	public float tangentAggression = .5f; //Higher is wider turns, lower is tighter turns

    public bool debug = true;

    [HideInInspector]
    public List<Vector3> waypoints = new List<Vector3>();

    public bool generateWaypoints = true;
    public float waypointResolution = 5;

	public List<Vector3> debugPoints;
	public List<float> debugAngles;

	void Awake()
	{
		Debug.Log("I am awake");
		trackMaker = gameObject.GetComponent(typeof(TrackCreator)) as TrackCreator;
	}

    void Update()
    {
		if (trackMaker == null)
			trackMaker = gameObject.GetComponent(typeof(TrackCreator)) as TrackCreator;

		//ControlPoints = trackMaker.points;
		//DrawSpline(false);
	}

	public List<Vector3> MakeSpline(List<Vector3> controlPoints, bool closedLoop)
	{
		ControlPoints = controlPoints;

		List<Vector3> points = new List<Vector3>(); //All points of the spline
		
		int closedAdjustment = closedLoop ? 0 : 1;

		// First for loop goes through each individual control point and connects it to the next, so 0-1, 1-2, 2-3 and so on
		for (int i = 0; i < controlPoints.Count - closedAdjustment; i++)
		{
			//The 4 points on my catmull spline
			Vector3 point1, point2, point3, point4;

			//The two points to interpolate between
			point2 = controlPoints[i];
			point3 = (closedLoop == true && i == controlPoints.Count - 1) ? controlPoints[0] : controlPoints[i + 1];

			//The first handle/anchor thingy
			if (i == 0 && !closedLoop)
				//If its the first point, make the point up
				point1 = point3 - point2;
			else
				//This will loop back round
				point1 = controlPoints[((i - 1) + controlPoints.Count) % controlPoints.Count];

			//The second handle/anchor thingy
			if (i >= controlPoints.Count - 2 && !closedLoop)
				//If we're on the last point, make it up
				point4 = point3 - point2;
			else
				point4 = controlPoints[(i + 2) % controlPoints.Count];

			float pointStep = 1.0f / CurveResolution;

			if (i == controlPoints.Count - 1 && closedLoop)
				pointStep = 1.0f / (CurveResolution - 1);

			// Second loop actually creates the spline for this particular segment
			for (int j = 0; j < CurveResolution; j++)
			{
				float t = j * pointStep;
				Vector3 position = CatmullRom.GetCatmullRomPosition(point1, point2, point3, point4, t, out var tangent, alpha);
				points.Add(position);
			}
		}

		return SplitCurveEvenly(points);
	}

	void OnDrawGizmos()
    {
        if (debug && ControlPoints != null && ControlPoints.Count > 1)
        {
            if (generateWaypoints)
            {
                //Waypoints
                for (int i = 0; i < waypoints.Count; i++)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(waypoints[i] + new Vector3(0, 25, 0), 2);

                    if (i == waypoints.Count - 1)
                    {
                        Debug.DrawLine(waypoints[i] + new Vector3(0, 25, 0), waypoints[0] + new Vector3(0, 25, 0), Color.blue);
                    }
                    else
                    {
                        Debug.DrawLine(waypoints[i] + new Vector3(0, 25, 0), waypoints[i + 1] + new Vector3(0, 25, 0), Color.blue);
                    }
                }
            }

            //Control Point Gizmo
            Gizmos.color = Color.cyan;
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                Gizmos.DrawSphere(ControlPoints[i], extrude/2);
            }

			Gizmos.color = Color.red;
			/*
			foreach (var p in evenPoints)
			{
				//Gizmos.DrawSphere(p, .5f);
			}
			*/
        }

		if (debugPoints != null)
		{
			foreach (var d in debugPoints)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(d, 10);
			}
		}
    }

	public void GenerateRoadMesh(List<Vector3> evenPoints, string name, bool closedLoop)
	{
		//var roadInfo = GenerateTangentPointsFromPath(evenPoints, extrude, edgeWidth, thickness);

		if (name == "bottom")
		{
			firstRoad = evenPoints;
		}
		else if (name == "right")
		{
			secondRoad = evenPoints;
		}

		//Mesh mesh = MeshMaker.RoadMeshAlongPath(roadInfo.roadVerticies, name, closedLoop);

		//Make the road template... just 2 points on either side of the center point
		List<TemplatePoint> template = new List<TemplatePoint>();
		template.Add(new TemplatePoint(new Vector2(extrude / 2, 0), 0));
		template.Add(new TemplatePoint(new Vector2(-extrude / 2, 0), 1));

		Mesh mesh = MeshMaker.ExtrudeShapeAlongPath(evenPoints, template, "road", closedLoop, false);

		//GetComponent<MeshFilter>().sharedMesh = mesh;
		//GetComponent<MeshCollider>().sharedMesh = mesh;

		DestroyImmediate(GameObject.Find(name));
		GameObject road = new GameObject(name);
		road.transform.SetParent(transform);
		road.transform.position = transform.position;
		road.AddComponent<MeshFilter>().sharedMesh = mesh;
		road.AddComponent<MeshCollider>().sharedMesh = mesh;

		MeshRenderer rend = road.AddComponent<MeshRenderer>();

		Material material = (Material)Resources.Load("URP Shaders/Shader Graphs_road", typeof(Material));
		rend.sharedMaterial = material;
		rend.sharedMaterial.color = Color.white;

		if (drawEdges)
        {
           // DrawInner(roadInfo.innerVerticies, closedLoop);
           // DrawOuter(roadInfo.outerVerticies, closedLoop);
        }
    }

	public void GenerateMesh(List<Vector3> evenPoints)
	{
		var info = MakeExitTest(evenPoints, extrude);
		Mesh mesh = MeshMaker.RoadMeshAlongPath(info, "exit", false);

		DestroyImmediate(GameObject.Find("exit"));
		GameObject exit = new GameObject("exit");
		exit.transform.SetParent(transform);
		exit.transform.position = transform.position;
		exit.AddComponent<MeshFilter>().sharedMesh = mesh;

		MeshRenderer rend = exit.AddComponent<MeshRenderer>();
		rend.sharedMaterial = new Material(Shader.Find("Shader Graphs_road"));
		rend.sharedMaterial.color = Color.white;
	}

	private void DrawInner(List<List<ShapePoint>> innerVerticies, bool closedLoop)
    {
		Mesh mesh = MeshMaker.ExtrudeShapeMeshAlongPath(innerVerticies, "inner", closedLoop, true);

		DestroyImmediate(GameObject.Find("Inner Edge"));
		GameObject piece = new GameObject("Inner Edge");
        piece.transform.SetParent(transform);
        piece.transform.position = transform.position;
        piece.AddComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer rend = piece.AddComponent<MeshRenderer>();
        rend.sharedMaterial = new Material(Shader.Find("Standard"));
        rend.sharedMaterial.color = Color.white;
    }

    private void DrawOuter(List<List<ShapePoint>> outerVerticies, bool closedLoop)
    {
		Mesh mesh = MeshMaker.ExtrudeShapeMeshAlongPath(outerVerticies, "outer", closedLoop, true);

		DestroyImmediate(GameObject.Find("Outer Edge"));
		GameObject piece = new GameObject("Outer Edge");
        piece.transform.SetParent(transform);
        piece.transform.position = transform.position;
        piece.AddComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer rend = piece.AddComponent<MeshRenderer>();
        rend.sharedMaterial = new Material(Shader.Find("Standard"));
        rend.sharedMaterial.color = Color.white;
    }

	//A sort of dirt way to evenly spread out the points in the curve. Basically iterate through the curve and make rougly evenly space out points
	//Ideally, I'd want to mathmatically work out how much to increment t so it proceeds down the curve at a regular rate - but this will do.
	public List<Vector3> SplitCurveEvenly(List<Vector3> points)
	{
		List<Vector3> evenlySpacedPoints = new List<Vector3>();

		Vector3 prevPointOnPath = points[0];
		evenlySpacedPoints.Add(prevPointOnPath);
		float dstSinceLastPoint = 0;

		//float estimatedSegmentLength = GetDistanceOfCurve(points);
		//float estimatedSpacing = estimatedSegmentLength / points.Count;

		float estimatedSpacing = MeshResolution;

		//Skip first point
		for (int i = 1; i < points.Count; i++)
		{
			Vector3 pointOnPath = points[i];
			dstSinceLastPoint += Vector3.Distance(pointOnPath, prevPointOnPath);

			while (dstSinceLastPoint >= estimatedSpacing)
			{
				float overshootDst = dstSinceLastPoint - estimatedSpacing;
				Vector3 newEvenlySpacedPoint = pointOnPath + (prevPointOnPath - pointOnPath).normalized * overshootDst;

				evenlySpacedPoints.Add(newEvenlySpacedPoint);
				dstSinceLastPoint = overshootDst;
				prevPointOnPath = newEvenlySpacedPoint;
			}

			prevPointOnPath = pointOnPath;
		}

		return evenlySpacedPoints;
	}

	public float GetDistanceOfCurve(List<Vector3> points)
	{
		var totalLength = 0f;
		var prevPoint = points[0];

		//Skip first point
		for (int i = 1; i < points.Count; i++)
		{
			var length = Vector3.Distance(points[i], prevPoint);
			totalLength += length;
			prevPoint = points[i];
		}

		return totalLength;
	}

	//Return pairs of points to contruct the mesh for the road
	public RoadPointInfo GenerateTangentPointsFromPath(List<Vector3> path, float width, float edgeWidth, float thickness)
	{
		RoadPointInfo roadInfo = new RoadPointInfo(path);

		for (int i = 0; i < path.Count; i++)
		{
			Vector3 forward = Vector3.zero;
			if (i < path.Count - 1)
			{
				forward += path[i + 1] - path[i];
			}

			if (i > 0)
			{
				forward += path[i] - path[i - 1];
			}
			forward.Normalize();

			//Get the orientation of the point (where it point towards the next point in the path)
			Vector3 t = Vector3.Cross(forward, Vector3.up).normalized;

			//Stick two points on either side of the original point, in line with the orientation
			Pair<Vector3> roadPair = new Pair<Vector3>(path[i] + t * width / 2, path[i] - t * width / 2);

			//right sidewalk shape
			List<ShapePoint> outer = new List<ShapePoint>();
			outer.Add(new ShapePoint(path[i] - t * width / 2));
			outer.Add(new ShapePoint((path[i] - t * width / 2) + new Vector3(0, thickness, 0)));
			outer.Add(new ShapePoint((path[i] - t * (width / 2 + edgeWidth)) + new Vector3(0, thickness, 0)));
			outer.Add(new ShapePoint((path[i] - t * (width / 2 + edgeWidth))));

			//left sidewalk shape
			List<ShapePoint> inner = new List<ShapePoint>();
			inner.Add(new ShapePoint((path[i] + t * (width / 2 + edgeWidth))));
			inner.Add(new ShapePoint((path[i] + t * (width / 2 + edgeWidth)) + new Vector3(0, thickness, 0)));
			inner.Add(new ShapePoint((path[i] + t * width / 2) + new Vector3(0, thickness, 0)));
			inner.Add(new ShapePoint(path[i] + t * width / 2));
			
			roadInfo.roadVerticies.Add(roadPair);
			roadInfo.innerVerticies.Add(inner);
			roadInfo.outerVerticies.Add(outer);
		}

		return roadInfo;
	}

	public List<Pair<Vector3>> MakeExitTest(List<Vector3> path, float width)
	{
		List<Pair<Vector3>> exitTest = new List<Pair<Vector3>>();
		exitControlPoints = new List<Pair<Vector3>>();

		float exitLength = 20;
		Vector3 tangent = new Vector3();

		for (int i = 0; i < 40; i++)
		{
			Vector3 forward = Vector3.zero;
			if (i < path.Count - 1)
			{
				forward += path[i + 1] - path[i];
			}

			if (i > 0)
			{
				forward += path[i] - path[i - 1];
			}
			forward.Normalize();

			//Get the orientation of the point (where it point towards the next point in the path)
			Vector3 t = Vector3.Cross(forward, Vector3.up).normalized;

			var innerOrigin = 0;
			var outerOrigin = width / 2;

			var innerTarget = width / 2;
			var outerTarget = width;

			var innerSmooth = Mathf.SmoothStep(innerOrigin, innerTarget, i / exitLength);
			var outerSmooth = Mathf.SmoothStep(outerOrigin, outerTarget, i / exitLength);

			if (i < exitLength)
			{
				Pair<Vector3> pair = new Pair<Vector3>(path[i] - t * innerSmooth, path[i] - t * outerSmooth);
				exitTest.Add(pair);
			}
			else if (i < 40)
			{
				Pair<Vector3> pair = new Pair<Vector3>(path[i] - t * innerTarget, path[i] - t * outerTarget);
				exitTest.Add(pair);
				if (i == 39)
				{
					exitControlPoints.Add(pair);
					tangent = t;
				}
			}
		}

		var firstPoint = exitTest.Last().First;
		var secondPoint = exitTest.Last().Second;

		var radius = 100;
		var origin = firstPoint - tangent * radius;

		for (int i = 0; i < 90; i = i + 5)
		{
			var angleToRotate = Quaternion.Euler(0, i, 0);
			var rotatedFirstPoint = RotatePointAroundPivot(firstPoint, origin, angleToRotate);
			var rotatedSecondPoint = RotatePointAroundPivot(secondPoint, origin, angleToRotate);
			exitTest.Add(new Pair<Vector3>(rotatedFirstPoint, rotatedSecondPoint));
		}

		//Move all points down .1f so it doesn't clip with the highway
		for (int i = 0; i < exitTest.Count; i++)
		{
			exitTest[i] = new Pair<Vector3>(
				new Vector3(exitTest[i].First.x, exitTest[i].First.y - .1f, exitTest[i].First.z), 
				new Vector3(exitTest[i].Second.x, exitTest[i].Second.y - .1f, exitTest[i].Second.z)
				);
		}

		return exitTest;
	}

	public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
	{
		return rotation * (point - pivot) + pivot;
	}

	public void DetectClosedPoints()
	{
		debugPoints = new List<Vector3>();
		debugAngles = new List<float>();

		var highway = highways.First();
		var road = roads.First();

		var threshholdDistance = 5;

		var highwayPointIndexs = new List<int>();
		var roadPointIndexs = new List<int>();

		var exitedRecently = 0;

		for (int i = 0; i < highway.Count; i++)
		{
			if (exitedRecently == 0)
			{
				for (int j = 0; j < road.Count; j++)
				{
					if (exitedRecently == 0)
					{
						var d = Vector3.Distance(road[j].First, highway[i].First);
						if (d < threshholdDistance)
						{
							highwayPointIndexs.Add(i);
							roadPointIndexs.Add(j);
							exitedRecently = 20;
						}
					}
				}
			}
			else
				exitedRecently--;
		}

		

		for (int i = 0; i < highwayPointIndexs.Count; i++)
		{
			debugPoints.Add(highway[highwayPointIndexs[i]].First);
			var something = Vector3.SignedAngle(highway[highwayPointIndexs[i] + 1].First - highway[highwayPointIndexs[i]].First, road[roadPointIndexs[i] + 1].First - road[roadPointIndexs[i]].First, Vector3.up);
			debugAngles.Add(something);
			//debugPoints.Add(road[roadPointIndexs[i]].First);
		}

		//var h = highway[highwayPointIndex - 30].First;
		//var r = road[roadPointIndex - 30].First;

		//debugPoints.Add(highwayPoint.First);
		//debugPoints.Add(r);
		//debugPoints.Add(h);
		//debugPoints.Add(highway[highwayPointIndex - 30].First);
		//debugPoints.Add(road[roadPointIndex - 30].First);
	}

	public void CreateIntersection(List<Vector3> roadA, List<Vector3> roadB, float width, int size)
	{
		debugPoints = new List<Vector3>();

		var firstList = roadA;
		var secondList = roadB;

		int firstListIndex = 0;
		int secondListIndex = 0;

		float distance = float.MaxValue;

		//For a curve, need to fine intersection with brute force
		for (int i = 0; i < firstList.Count; i++)
		{
			for (int j = 0; j < secondList.Count; j++)
			{
				var d = Vector3.Distance(firstList[i], secondList[j]);
				if (d < distance)
				{
					firstListIndex = i;
					secondListIndex = j;
					distance = d;
				}
			}
		}

		connections = new List<List<Vector3>>();

		var firstListA = firstList.GetRange(0, firstListIndex);
		firstListA.Reverse();
		connections.Add(firstListA.Skip(size).ToList());
		var firstListB = firstList.GetRange(firstListIndex - 1, firstList.Count() - firstListIndex);
		connections.Add(firstListB.Skip(size).ToList());

		//var lineA1 = new Pair<Vector3>(firstListA.GetRoadPoint(size, -width / 2), firstListB.GetRoadPoint(size, width / 2));
		//var lineA2 = new Pair<Vector3>(firstListA.GetRoadPoint(size, width / 2), firstListB.GetRoadPoint(size, -width / 2));

		var secondListA = secondList.GetRange(0, secondListIndex);
		secondListA.Reverse();
		connections.Add(secondListA.Skip(size).ToList());
		var secondListB = secondList.GetRange(secondListIndex - 1, secondList.Count() - secondListIndex);
		connections.Add(secondListB.Skip(size).ToList());

		//var lineB1 = new Pair<Vector3>(secondListA.GetRoadPoint(size, -width / 2), secondListB.GetRoadPoint(size, width / 2));
		//var lineB2 = new Pair<Vector3>(secondListA.GetRoadPoint(size, width / 2), secondListB.GetRoadPoint(size, -width / 2));

		var side1 = new Pair<Vector3>(firstListA.GetRoadPoint(size, width / 2), firstListA.GetRoadPoint(size, -width / 2));
		var side3 = new Pair<Vector3>(firstListB.GetRoadPoint(size, width / 2), firstListB.GetRoadPoint(size, -width / 2));

		var side4 = new Pair<Vector3>(secondListA.GetRoadPoint(size, width / 2), secondListA.GetRoadPoint(size, -width / 2));
		var side2 = new Pair<Vector3>(secondListB.GetRoadPoint(size, width / 2), secondListB.GetRoadPoint(size, -width / 2));

		var outerPoints = new List<Pair<Vector3>>();

		outerPoints.Add(side1);

		//debugPoints.Add(side2.First);
		//debugPoints.Add(side4.Second);

		outerPoints.Add(side2);
		outerPoints.Add(side3);
		outerPoints.Add(side4);

		LineEquation line1 = new LineEquation(side1.First.To2D(), side3.Second.To2D());
		LineEquation line2 = new LineEquation(side1.Second.To2D(), side3.First.To2D());
		LineEquation line3 = new LineEquation(side2.First.To2D(), side4.Second.To2D());
		LineEquation line4 = new LineEquation(side2.Second.To2D(), side4.First.To2D());

		var innerPoints = new List<Vector3>();

		innerPoints.Add(line1.GetIntersectionWithLine(line3).Value.To3D());
		innerPoints.Add(line2.GetIntersectionWithLine(line3).Value.To3D());
		innerPoints.Add(line2.GetIntersectionWithLine(line4).Value.To3D());
		innerPoints.Add(line1.GetIntersectionWithLine(line4).Value.To3D());

		//debugPoints.Add(line1.GetIntersectionWithLine(line3).Value.To3D());
		//debugPoints.Add(line1.GetIntersectionWithLine(line4).Value.To3D());
		//debugPoints.Add(line2.GetIntersectionWithLine(line3).Value.To3D());
		//debugPoints.Add(line2.GetIntersectionWithLine(line4).Value.To3D());

		var name = "fourwayintersection";

		Mesh mesh = MeshMaker.FourWayIntersection(outerPoints, innerPoints, name);

		DestroyImmediate(GameObject.Find(name));
		GameObject intersection = new GameObject(name);
		intersection.transform.SetParent(transform);
		intersection.transform.position = transform.position;
		intersection.AddComponent<MeshFilter>().sharedMesh = mesh;

		MeshRenderer rend = intersection.AddComponent<MeshRenderer>();
		Material material = (Material)Resources.Load("Materials/test_mat", typeof(Material));
		rend.sharedMaterial = material;
		rend.sharedMaterial.color = Color.white;
	}

	public Vector3 FindIntersectionOfTwoLines(Pair<Vector3> lineA, Pair<Vector3> lineB, float moveAmount = .1f)
	{
		float distance = float.MaxValue;
		var intersectionPoint = new Vector3();

		var pointOnA = lineA.First;
		var pointOnB = lineB.First;

		int escape = 0;

		while (pointOnA != lineA.Second && escape != 1000000000)
		{
			while (pointOnB != lineB.Second && escape != 1000000000)
			{
				pointOnB = Vector3.MoveTowards(pointOnB, lineB.Second, moveAmount);

				var d = Vector3.Distance(pointOnA, pointOnB);
				if (d < distance)
				{
					intersectionPoint = Vector3.Lerp(pointOnA, pointOnB, .5f);
					distance = d;
				}

				escape++;
			}
			escape++;
			//Reset B to start
			pointOnB = lineB.First;
			//Move alone A
			pointOnA = Vector3.MoveTowards(pointOnA, lineA.Second, moveAmount);
		}
		
		return intersectionPoint;
	}
}