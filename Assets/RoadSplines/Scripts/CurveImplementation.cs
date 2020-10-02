using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Utils;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode()]
public class CurveImplementation : MonoBehaviour
{
	private List<Vector3> points = new List<Vector3>(); //All points of the spline
	private List<Vector3> evenPoints = new List<Vector3>(); //All points of the spline
	private List<Pair<Vector3>> exitTest = new List<Pair<Vector3>>(); //All points of the spline

	private TrackCreator trackMaker;

	public bool drawEdges = true;
	public float alpha = .5f;

    public List<Vector3> ControlPoints;
    public int CurveResolution = 10;
    public float extrude = 10;

    public float edgeWidth = 2;
    public float thickness = 1;

	public float tangentAggression = .5f; //Higher is wider turns, lower is tighter turns

    public bool debug = true;

    public bool ClosedLoop = true;

    [HideInInspector]
    public List<Vector3> waypoints = new List<Vector3>();

    public bool generateWaypoints = true;
    public float waypointResolution = 5;

	void Awake()
	{
		Debug.Log("I am awake");
		trackMaker = gameObject.GetComponent(typeof(TrackCreator)) as TrackCreator;
	}

    void Update()
    {
		if (trackMaker == null)
			trackMaker = gameObject.GetComponent(typeof(TrackCreator)) as TrackCreator;

		ControlPoints = trackMaker.points;
		//DrawSpline(false);
    }

	public void DrawSpline(bool store)
	{
		if (store)
			points.Clear();
		
		int closedAdjustment = ClosedLoop ? 0 : 1;

		// First for loop goes through each individual control point and connects it to the next, so 0-1, 1-2, 2-3 and so on
		for (int i = 0; i < ControlPoints.Count - closedAdjustment; i++)
		{
			//The 4 points on my catmull spline
			Vector3 point1, point2, point3, point4;

			//The two points to interpolate between
			point2 = ControlPoints[i];
			point3 = (ClosedLoop == true && i == ControlPoints.Count - 1) ? ControlPoints[0] : ControlPoints[i + 1];

			//The first handle/anchor thingy
			if (i == 0 && !ClosedLoop)
				//If its the first point, make the point up
				point1 = point3 - point2;
			else
				//This will loop back round
				point1 = ControlPoints[((i - 1) + ControlPoints.Count) % ControlPoints.Count];

			//The second handle/anchor thingy
			if (i >= ControlPoints.Count - 2 && !ClosedLoop)
				//If we're on the last point, make it up
				point4 = point3 - point2;
			else
				point4 = ControlPoints[(i + 2) % ControlPoints.Count];

			float pointStep = 1.0f / CurveResolution;

			if (i == ControlPoints.Count - 1 && ClosedLoop)
				pointStep = 1.0f / (CurveResolution - 1);

			// Second loop actually creates the spline for this particular segment
			for (int j = 0; j < CurveResolution; j++)
			{
				float t = j * pointStep;
				Vector3 position = CatmullRom.GetCatmullRomPosition(point1, point2, point3, point4, t, out var tangent, alpha);

				if (store)
					points.Add(position);
			}
		}

		if (store)
			evenPoints = SplitCurveEvenly(points);
	}

	void OnDrawGizmos()
    {
        if (debug && ControlPoints.Count > 1)
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
			foreach (var p in evenPoints)
			{
				//Gizmos.DrawSphere(p, .5f);
			}
        }
    }

    public void GenerateMesh ()
    {
		var roadInfo = GenerateTangentPointsFromPath(evenPoints, extrude, edgeWidth, thickness);
		Mesh mesh = MeshMaker.RoadMeshAlongPath(roadInfo.roadVerticies, "road", ClosedLoop);
		GetComponent<MeshFilter>().sharedMesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;

        if (drawEdges)
        {
            DrawInner(roadInfo.innerVerticies);
            DrawOuter(roadInfo.outerVerticies);
        }
    }

    private void DrawInner(List<List<ExtrudeShapePoint>> innerVerticies)
    {
		Mesh mesh = MeshMaker.ExtrudeShapeMeshAlongPath(innerVerticies, "inner");

		DestroyImmediate(GameObject.Find("Inner Edge"));
		GameObject piece = new GameObject("Inner Edge");
        piece.transform.SetParent(transform);
        piece.transform.position = transform.position;
        piece.AddComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer rend = piece.AddComponent<MeshRenderer>();
        rend.sharedMaterial = new Material(Shader.Find("Standard"));
        rend.sharedMaterial.color = Color.white;
    }

    private void DrawOuter(List<List<ExtrudeShapePoint>> outerVerticies)
    {
		Mesh mesh = MeshMaker.ExtrudeShapeMeshAlongPath(outerVerticies, "outer");

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

		float estimatedSegmentLength = GetDistanceOfCurve(points);
		float estimatedSpacing = estimatedSegmentLength / points.Count;

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
		RoadPointInfo roadInfo = new RoadPointInfo();

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
			List<ExtrudeShapePoint> outer = new List<ExtrudeShapePoint>();
			outer.Add(new ExtrudeShapePoint(path[i] - t * width / 2));
			outer.Add(new ExtrudeShapePoint((path[i] - t * width / 2) + new Vector3(0, thickness, 0)));
			outer.Add(new ExtrudeShapePoint((path[i] - t * (width / 2 + edgeWidth)) + new Vector3(0, thickness, 0)));
			outer.Add(new ExtrudeShapePoint((path[i] - t * (width / 2 + edgeWidth))));

			//left sidewalk shape
			List<ExtrudeShapePoint> inner = new List<ExtrudeShapePoint>();
			inner.Add(new ExtrudeShapePoint((path[i] + t * (width / 2 + edgeWidth))));
			inner.Add(new ExtrudeShapePoint((path[i] + t * (width / 2 + edgeWidth)) + new Vector3(0, thickness, 0)));
			inner.Add(new ExtrudeShapePoint((path[i] + t * width / 2) + new Vector3(0, thickness, 0)));
			inner.Add(new ExtrudeShapePoint(path[i] + t * width / 2));
			
			roadInfo.roadVerticies.Add(roadPair);
			roadInfo.innerVerticies.Add(inner);
			roadInfo.outerVerticies.Add(outer);


			if (i == 0 || i == 10)
			{
				var exit = new Pair<Vector3>(path[i] - t * width / 4, path[i] - t * width / 2);
				exitTest.Add(exit);
			}
			if (i == 20 || i == 30)
			{
				var exit = new Pair<Vector3>(path[i] - t * width / 2, path[i] - t * ((3 * width) / 4));
				exitTest.Add(exit);
			}

		}

		return roadInfo;
	}
}