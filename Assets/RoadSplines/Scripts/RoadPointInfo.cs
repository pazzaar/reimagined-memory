using System.Collections.Generic;
using UnityEngine;
using Utils;

public class RoadPointInfo : List<Vector3>
{
	public List<Vector3> evenRoadPoints;
	public List<Pair<Vector3>> roadVerticies;
	public List<List<ShapePoint>> innerVerticies;
	public List<List<ShapePoint>> outerVerticies;

	public RoadPointInfo(List<Vector3> even = null, List < Pair<Vector3>> road = null, List<List<ShapePoint>> inner = null, List<List<ShapePoint>> outer = null)
	{
		
		roadVerticies = road != null ? road : new List<Pair<Vector3>>();
		outerVerticies = outer != null ? outer : new List<List<ShapePoint>>();
		innerVerticies = inner != null ? inner : new List<List<ShapePoint>>();
		evenRoadPoints = even != null ? even : new List<Vector3>();
	}
}
