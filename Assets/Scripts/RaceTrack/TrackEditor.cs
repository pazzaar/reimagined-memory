using UnityEditor;
using UnityEngine;

/*
public class TrackEditor : Editor
{
	private TrackCreator track;

	// Start is called before the first frame update
	void Start()
    {
		track = new TrackCreator(200, 160, 20);
		System.Random rnd = new System.Random();
		track.GenerateTrack(rnd);
	}

    // Update is called once per frame
    void Update()
    {
		var curve = track.curve;
		var oldCurvePoint = curve.First();

		foreach (var newCurvePoint in curve.Skip(1))
		{
			Debug.DrawLine(oldCurvePoint, newCurvePoint, Color.red);
			oldCurvePoint = newCurvePoint;
		}

		var points = track.points;
		var oldPoint = points.First();

		foreach (var newPoint in points.Skip(1))
		{
			Debug.DrawLine(oldPoint, newPoint, Color.green);
			oldPoint = newPoint;
		}

    }
}
*/

[CustomEditor(typeof(TrackCreator))]
public class TrackEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		TrackCreator track = (TrackCreator)target;

		if (GUILayout.Button("Generate highway points"))
		{
			System.Random rnd = new System.Random();
			track.GenerateHighway(rnd);
		}

		if (GUILayout.Button("Generate road points"))
		{
			System.Random rnd = new System.Random();
			track.GenerateTrack(rnd);
		}

		if (GUILayout.Button("Generate road points akima"))
		{
			System.Random rnd = new System.Random();
			track.GenerateTrackAkima(rnd);
		}

		if (GUILayout.Button("Generate road sections"))
		{
			track.GenerateRoadSection();
		}
	}
}

