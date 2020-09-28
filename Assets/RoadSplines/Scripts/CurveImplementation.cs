using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Utils;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode()]
public class CurveImplementation : MonoBehaviour
{
	private List<CurvePoint> curvePoints = new List<CurvePoint>();

	private List<Vector3> points = new List<Vector3>(); //All points of the spline
	private List<Vector3> inner = new List<Vector3>(); //All points of the spline
    private List<Vector3> outer = new List<Vector3>(); //All points of the spline

	private List<Vector3> evenPoints = new List<Vector3>(); //All points of the spline
	private List<Vector3> evenInner = new List<Vector3>(); //All points of the spline
	private List<Vector3> evenOuter = new List<Vector3>(); //All points of the spline

	private TrackCreator trackMaker;

	public bool drawEdges = true;
	public float alpha = .5f;
	private Vector3[] CurveCoordinates;
    private Vector3[] Tangents;

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

    public void DrawSpline (bool store)
    {
        if (store)
        {
            points.Clear();
            inner.Clear();
            outer.Clear();

            if (generateWaypoints)
                waypoints.Clear();
        }

        Vector3 p0;
        Vector3 p1;
        Vector3 m0;
        Vector3 m1;

        int pointsToMake;

        if (ClosedLoop == true)
        {
            pointsToMake = (CurveResolution) * (ControlPoints.Count);
        }
        else
        {
            pointsToMake = (CurveResolution) * (ControlPoints.Count - 1);
        }

        if (pointsToMake > 0) //Prevent Number Overflow
        {
            CurveCoordinates = new Vector3[pointsToMake];
            Tangents = new Vector3[pointsToMake];

            int closedAdjustment = ClosedLoop ? 0 : 1;

			// First for loop goes through each individual control point and connects it to the next, so 0-1, 1-2, 2-3 and so on
			for (int i = 0; i < ControlPoints.Count - closedAdjustment; i++)
            {
                p0 = ControlPoints[i];
                p1 = (ClosedLoop == true && i == ControlPoints.Count - 1) ? ControlPoints[0] : ControlPoints[i + 1];

                // Tangent calculation for each control point
                // Tangent M[k] = (P[k+1] - P[k-1]) / 2
                // With [] indicating subscript

                // m0
                if (i == 0)
                {
					m0 = ClosedLoop ? ControlPoints[ControlPoints.Count - 1] : p1 - p0;
					//m0 = ClosedLoop ? tangentAggression * (p1 - ControlPoints[ControlPoints.Count - 1]) : p1 - p0;
				}
				else
                {
					m0 = ControlPoints[i - 1];
					//m0 = tangentAggression * (p1 - ControlPoints[i - 1]);
				}

                // m1
                if (ClosedLoop)
                {
					if (i == ControlPoints.Count - 1) //At the end
					{
						m1 = ControlPoints[1];
						//m1 = tangentAggression * (ControlPoints[(i + 2) % ControlPoints.Count] - p0);
					}
					else if (i == ControlPoints.Count - 2)
					{
						m1 = ControlPoints[0];
						//m1 = tangentAggression * (ControlPoints[(i + 2) % ControlPoints.Count] - p0);
					}
					else if (i == 0) //At the start
					{
						m1 = ControlPoints[i + 2];
						//m1 = tangentAggression * (ControlPoints[i + 2] - p0);
					}
					else
					{
						m1 = ControlPoints[i + 2];
						//m1 = tangentAggression * (ControlPoints[(i + 2) % ControlPoints.Count] - p0);
					}
                }
                else
                {
                    if (i < ControlPoints.Count - 2)
                    {
                        m1 = tangentAggression * (ControlPoints[(i + 2) % ControlPoints.Count] - p0);
                    }
                    else
                    {
                        m1 = p1 - p0;
                    }
                }

				Vector3 position;
                float t;
                float pointStep = 1.0f / CurveResolution;

                if ((i == ControlPoints.Count - 2 && ClosedLoop == false) || (i == ControlPoints.Count - 1 && ClosedLoop))
                {
                    pointStep = 1.0f / (CurveResolution - 1);
                    // last point of last segment should reach p1
                }
                // Second for loop actually creates the spline for this particular segment
                for (int j = 0; j < CurveResolution; j++)
                {
                    t = j * pointStep;
                    Vector3 tangent;
                    position = CatmullRom.GetCatmullRomPosition(m0, p0, p1, m1, t, out tangent, alpha);
                    CurveCoordinates[i * CurveResolution + j] = position;
                    Tangents[i * CurveResolution + j] = tangent;

					if (debug) //Normals
                    {
                        Debug.DrawLine(position + Vector3.Cross(tangent, Vector3.up).normalized * extrude / 2 + transform.position, position - Vector3.Cross(tangent, Vector3.up).normalized * extrude / 2 + transform.position, Color.red);
                        Debug.DrawLine(position + Vector3.Cross(tangent, Vector3.up).normalized * extrude / 2 + transform.position, position + Vector3.Cross(tangent, Vector3.up).normalized * (extrude / 2 + edgeWidth) + transform.position, Color.green); //Edge +
                        Debug.DrawLine(position - Vector3.Cross(tangent, Vector3.up).normalized * extrude / 2 + transform.position, position - Vector3.Cross(tangent, Vector3.up).normalized * (extrude / 2 + edgeWidth) + transform.position, Color.green); //Edge -
                    }

                    if (store)
                    {
                        //points.Add(position - Vector3.Cross(tangent, Vector3.up).normalized * extrude / 2);
                        points.Add(position);
                        //points.Add(position + Vector3.Cross(tangent, Vector3.up).normalized * extrude / 2);

                        inner.Add(position - Vector3.Cross(tangent, Vector3.up).normalized * extrude / 2);
                        inner.Add(position - Vector3.Cross(tangent, Vector3.up).normalized * (extrude / 2 + edgeWidth));

                        outer.Add(position + Vector3.Cross(tangent, Vector3.up).normalized * extrude / 2);
                        outer.Add(position + Vector3.Cross(tangent, Vector3.up).normalized * (extrude / 2 + edgeWidth));
                    }
                }				

				for (int j = 0; j < waypointResolution; j++)
                {
                    t = j * 1.0f / waypointResolution;
                    Vector3 tangent;
                    position = CatmullRom.GetCatmullRomPosition(m0, p0, p1, m1, t, out tangent, alpha);
                
                    if (debug && generateWaypoints) //Waypoints
                    {
                        Debug.DrawLine(position, position + new Vector3(0, 25, 0), Color.blue);
                    }

                    if (store && generateWaypoints)
                    {
                        waypoints.Add(position);
                    }
                }
            }

			if (store)
			{
				//this.path.AddRange(path);
				evenPoints = SplitCurveEvenly(points);
			}

			//Curve line
			for (int i = 0; i < CurveCoordinates.Length - 1; ++i)
            {
                Debug.DrawLine(CurveCoordinates[i] + transform.position, CurveCoordinates[i + 1] + transform.position, Color.cyan);
            }

        }
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
		Mesh mesh = MeshMaker.RoadMeshAlongPath(roadInfo.roadVerticies, "road");
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
	public static List<Vector3> SplitCurveEvenly(List<Vector3> points)
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

	public static float GetDistanceOfCurve(List<Vector3> points)
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
	public static RoadPointInfo GenerateTangentPointsFromPath(List<Vector3> path, float width, float edgeWidth, float thickness)
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
		}

		return roadInfo;
	}
}