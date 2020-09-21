using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
	public Path path;

	public void Start()
	{
		CreatePath();
	}

	public void Upate()
	{
		//draw the path
		

		for (int i = 0; i < path.NumberOfSegments; i++)
		{
			var startOfCurve = path[i];
			var endOfCurve = path[i + 1];

			//var oldPoint = BezierUtilities.GetPointOnCubic(
			//for (int j = 1; i < path.steps; i++)
			//{
				//var newPoint = BezierUtilities.GetPointOnCubic(path[
			//}
		}
		
	}

	public void CreatePath()
	{
		path = new Path(transform.position);
		path.steps = 20;
	}
}
