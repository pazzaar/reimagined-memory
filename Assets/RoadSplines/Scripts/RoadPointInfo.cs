using System.Collections.Generic;
using UnityEngine;
using Utils;

public class RoadPointInfo
{
	public List<Pair<Vector3>> roadVerticies;
	public List<Pair<Vector3>> innerVerticies;
	public List<Pair<Vector3>> outerVerticies;

	public RoadPointInfo(List<Pair<Vector3>> road = null, List<Pair<Vector3>> inner = null, List<Pair<Vector3>> outer = null)
	{
		roadVerticies = road != null ? road : new List<Pair<Vector3>>();
		innerVerticies = inner != null ? inner : new List<Pair<Vector3>>();
		outerVerticies = outer != null ? outer : new List<Pair<Vector3>>();
	}
}
