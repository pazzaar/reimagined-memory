using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode()]
public class CatmullRom
{
	//Old
    public enum Uniformity
    {
        Uniform,
        Centripetal,
        Chordal
    }

	//Old
    public static Vector3 Interpolate(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t)
    {
        // Catmull-Rom splines are Hermite curves with special tangent values.
        // Hermite curve formula:
        // (2t^3 - 3t^2 + 1) * p0 + (t^3 - 2t^2 + t) * m0 + (-2t^3 + 3t^2) * p1 + (t^3 - t^2) * m1
        // For points p0 and p1 passing through points m0 and m1 interpolated over t = [0, 1]
        // Tangent M[k] = (P[k+1] - P[k-1]) / 2
        // With [] indicating subscript
        Vector3 position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * start
            + (t * t * t - 2.0f * t * t + t) * tanPoint1
            + (-2.0f * t * t * t + 3.0f * t * t) * end
            + (t * t * t - t * t) * tanPoint2;

        return position;
    }

	//Old
    public static Vector3 Interpolate(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t, out Vector3 tangent)
    {
        // Calculate tangents
        // p'(t) = (6t² - 6t)p0 + (3t² - 4t + 1)m0 + (-6t² + 6t)p1 + (3t² - 2t)m1
        tangent = (6 * t * t - 6 * t) * start
            + (3 * t * t - 4 * t + 1) * tanPoint1
            + (-6 * t * t + 6 * t) * end
            + (3 * t * t - 2 * t) * tanPoint2;
        return Interpolate(start, end, tanPoint1, tanPoint2, t);
    }

	public static Vector3 GetCatmullRomPosition(Vector3 tanPoint1, Vector3 start, Vector3 end, Vector3 tanPoint2, float t, out Vector3 tangent, float alpha = 0.5f)
	{
		float dt0 = GetTime(tanPoint1, start, alpha);
		float dt1 = GetTime(start, end, alpha);
		float dt2 = GetTime(end, tanPoint2, alpha);

		Vector3 t1 = ((start - tanPoint1) / dt0) - ((end - tanPoint1) / (dt0 + dt1)) + ((end - start) / dt1);
		Vector3 t2 = ((end - start) / dt1) - ((tanPoint2 - start) / (dt1 + dt2)) + ((tanPoint2 - end) / dt2);

		t1 *= dt1;
		t2 *= dt1;

		Vector3 c0 = start;
		Vector3 c1 = t1;
		Vector3 c2 = (3 * end) - (3 * start) - (2 * t1) - t2;
		Vector3 c3 = (2 * start) - (2 * end) + t1 + t2;
		Vector3 pos = CalculatePosition(t, c0, c1, c2, c3);

		tangent = CalculateTangent(t, c1, c2, c3);
		return pos;
	}

	private static float GetTime(Vector3 p0, Vector3 p1, float alpha)
	{
		if (p0 == p1)
			return 1;
		return Mathf.Pow((p1 - p0).sqrMagnitude, 0.5f * alpha);
	}

	private static Vector3 CalculatePosition(float t, Vector3 c0, Vector3 c1, Vector3 c2, Vector3 c3)
	{
		float t2 = t * t;
		float t3 = t2 * t;
		return c0 + c1 * t + c2 * t2 + c3 * t3;
	}

	//CalculatePosition() but differentiated from cubic to quadratic
	private static Vector3 CalculateTangent(float t, Vector3 c1, Vector3 c2, Vector3 c3)
	{
		float t2 = t * t;
		return c1 + 2 * c2 * t + 3 * c3 * t2;
	}

}