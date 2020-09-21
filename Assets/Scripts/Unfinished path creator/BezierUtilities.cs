using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierUtilities
{
	//Get the point on a quadratic curve
	public static Vector3 GetPointOnQuadratic(Vector3 a, Vector3 b, Vector3 c, float t)
	{
		Vector3 p0 = Vector3.Lerp(a, b, t);
		Vector3 p1 = Vector3.Lerp(a, c, t);
		return Vector3.Lerp(p0, p1, t);
	}

	//Get the point on a cubic curve
	//So vectors a + d are the two anchors
	//And vectors b + c are the two points
	public static Vector3 GetPointOnCubic(Vector3 b, Vector3 a, Vector3 d, Vector3 c, float t)
	{
		Vector3 p0 = GetPointOnQuadratic(a, b, c, t);
		Vector3 p1 = GetPointOnQuadratic(b, c, d, t);
		return Vector3.Lerp(p0, p1, t);
	}

	//The forward axis of the point on the curve
	public static Vector3 GetTangentOnCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
	{
		Vector3 p0 = GetPointOnQuadratic(a, b, c, t);
		Vector3 p1 = GetPointOnQuadratic(b, c, d, t);
		return (p1 - p0).normalized;
	}

	//The up axis of the point on the curve
	public static Vector3 GetNormalOnCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t, Vector3 up)
	{
		Vector3 tangent = GetTangentOnCubic(a, b, c, d, t);
		Vector3 binormal = Vector3.Cross(up, tangent).normalized; //The side axis of the point on the curve
		return Vector3.Cross(tangent, binormal);
	}

	//Expressing the tangent/normal/binormal stuff as a quaternion, so it incorporates all the vectors.
	public static Quaternion GetOrientationOnCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t, Vector3 up)
	{
		Vector3 tangent = GetTangentOnCubic(a, b, c, d, t);
		Vector3 normal = GetNormalOnCubic(a, b, c, d, t, up);
		return Quaternion.LookRotation(tangent, normal);
	}
}