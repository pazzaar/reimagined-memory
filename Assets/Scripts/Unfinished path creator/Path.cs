using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Path
{
	List<BezierPoint> points;
	public int steps; //the resolution of the curve. Higher number = smoother curve

	public Path(Vector3 curveStart)
	{
		points = new List<BezierPoint>
		{
			//Make 2 new points, one on the left and one on the right to get the curve started
			new BezierPoint
			{
				center = curveStart + Vector3.left, //This point starts on the left
				anchor_1 = ((curveStart + Vector3.left) * 2) - (curveStart + (Vector3.right + Vector3.up) * .5f), //anchor 1 is equal to the central point x2 - anchor 2
				anchor_2 = curveStart + (Vector3.right + Vector3.up) * .5f, //This anchor starts to the top right of the center
			},
			new BezierPoint
			{
				center = curveStart + Vector3.right, //This point starts on the right
				anchor_1 = curveStart + (Vector3.right + Vector3.down) * .5f, //This anchor starts to the bottom left of the center
				anchor_2 = ((curveStart + Vector3.right) * 2) - (curveStart + (Vector3.right + Vector3.down) * .5f) //anchor 2 is equal to the central point x2 - anchor 1
			}
		};
	}

	public void AddPoint(Vector3 centerPosition)
	{
		var previousPoint = points[points.Count - 1];

		points.Add(
			new BezierPoint
			{
				anchor_1 = (previousPoint.anchor_2 + centerPosition) / 2,
				center = centerPosition,
				anchor_2 = (centerPosition * 2) - ((previousPoint.anchor_2 + centerPosition) / 2)
			}
		);
	}

	public void AutoSetAnchorPoints(int index)
	{
		BezierPoint centerPoint = points[index];
		Vector3 dir = Vector3.zero;

		int neighbour_1 = index - 1;
		float neighbour_1_distance = 1f;
		int neighbour_2 = index + 1;
		float neighbour_2_distance = 1f;

		if (neighbour_1 >= 0)
		{
			Vector3 offset = points[neighbour_1].center - centerPoint.center;
			dir += offset.normalized;
			neighbour_1_distance = offset.magnitude;
		}

		if (neighbour_2 >= 0)
		{
			Vector3 offset = points[neighbour_2].center - centerPoint.center;
			dir -= offset.normalized;
			neighbour_2_distance = -offset.magnitude;
		}

		dir.Normalize();

		centerPoint.anchor_1 = centerPoint.center + dir * neighbour_1_distance * .5f;
		centerPoint.anchor_2 = centerPoint.center + dir * neighbour_2_distance * .5f;
	}

	public void AutoSetEndAnchorPoints()
	{
		points[0].anchor_2 = (points[0].center + points[1].anchor_1) * .5f;
		points[points.Count - 1].anchor_1 = (points[points.Count - 1].center + points[points.Count - 2].anchor_2) * .5f;
	}

	public void MovePoint(int i, Vector3 newPos)
	{
		points[i].center = newPos;
	}

	public BezierPoint this[int i]
	{
		get
		{
			return points[i];
		}
	}

	public int NumberOfSegments
	{
		get
		{
			return points.Count - 1;
		}
	}

	public int NumberOfPoints
	{
		get
		{
			return points.Count;
		}
	}
}