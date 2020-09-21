using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class for representing a point on the curve
//A point has one main central point and two anchor points either side
public class BezierPoint
{
	public Vector3 center;
	public Vector3 anchor_1;
	public Vector3 anchor_2;
}
