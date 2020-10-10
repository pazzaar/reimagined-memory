using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(CurveImplementation))]
public class SplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();   

        CurveImplementation curve = (CurveImplementation)target;
        
        if (GUILayout.Button ("Instantiate"))
        {
            var points = curve.MakeSpline(curve.trackMaker.points, curve.closedLoop);
            curve.GenerateRoadMesh(points, "test", curve.closedLoop);
			//curve.GenerateMesh(points);
		}

		if (GUILayout.Button("Make highway"))
		{
			var points = curve.MakeSpline(curve.trackMaker.points, false);
			curve.GenerateRoadMesh(points, "highway", false);
			//curve.GenerateMesh(points);
		}

		if (GUILayout.Button("Make road"))
		{
			var points = curve.MakeSpline(curve.trackMaker.points, curve.closedLoop);
			curve.GenerateRoadMesh(points, "road", curve.closedLoop);
			//curve.GenerateMesh(points);
		}

		if (GUILayout.Button("Make akima road"))
		{
			curve.GenerateRoadMesh(curve.trackMaker.curve, "akima road", curve.closedLoop);
			//curve.GenerateMesh(points);
		}

		if (GUILayout.Button("Make road sections"))
		{
			curve.GenerateRoadMesh(curve.MakeSpline(curve.trackMaker.bottom, false), "bottom", false);
			curve.GenerateRoadMesh(curve.MakeSpline(curve.trackMaker.right, false), "right", false);
			curve.GenerateRoadMesh(curve.MakeSpline(curve.trackMaker.top, false), "top", false);
			curve.GenerateRoadMesh(curve.MakeSpline(curve.trackMaker.left, false), "left", false);
			//curve.GenerateMesh(points);
		}

		if (GUILayout.Button("Highlight closest points"))
		{
			curve.DetectClosedPoints();
		}

		if (GUILayout.Button("Find closest points"))
		{
			curve.CreateIntersection(curve.firstRoad, curve.secondRoad, curve.extrude, 8);
			var i = 0;
			foreach (var c in curve.connections)
			{
				curve.GenerateRoadMesh(c, $"connection {i}", false);
				i++;
			}
		}
	}
}
