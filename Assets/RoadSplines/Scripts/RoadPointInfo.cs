using System.Collections.Generic;
using UnityEngine;
using Utils;

public class RoadPointInfo
{
	public List<Pair<Vector3>> roadVerticies;
	public List<List<ExtrudeShapePoint>> innerVerticies;
	public List<List<ExtrudeShapePoint>> outerVerticies;

	public RoadPointInfo(List<Pair<Vector3>> road = null, List<List<ExtrudeShapePoint>> inner = null, List<List<ExtrudeShapePoint>> outer = null)
	{
		roadVerticies = road != null ? road : new List<Pair<Vector3>>();
		outerVerticies = outer != null ? outer : new List<List<ExtrudeShapePoint>>();
		innerVerticies = inner != null ? inner : new List<List<ExtrudeShapePoint>>();
	}
}
