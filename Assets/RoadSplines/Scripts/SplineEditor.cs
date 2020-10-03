using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CurveImplementation))]
public class SplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();   

        CurveImplementation curve = (CurveImplementation)target;
        
        if (GUILayout.Button ("Instantiate"))
        {
            var points = curve.MakeSpline(curve.trackMaker.points, true);
            curve.GenerateRoadMesh(points, true);
			curve.GenerateMesh(points);
		}
    }
}
