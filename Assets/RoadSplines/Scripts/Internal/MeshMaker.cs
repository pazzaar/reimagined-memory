using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MIConvexHull;
using Utils;

public class MeshMaker : MonoBehaviour
{
    /// <summary>
    /// Generates a rock from a given set of points. 
    /// </summary>       
    public static Mesh MeshFromPoints(IEnumerable<Vector3> points)
    {
        Mesh m = new Mesh();
        m.name = "Mesh";

        List<int> triangles = new List<int>();

        var vertices = points.Select(x => new Vertex(x)).ToList();

        var result = ConvexHull.Create(vertices);

        m.vertices = result.Points.Select(x => x.ToVec()).ToArray();

        var resultPoints = result.Points.ToList();

        foreach (var face in result.Faces)
        {
            triangles.Add(resultPoints.IndexOf(face.Vertices[0]));
            triangles.Add(resultPoints.IndexOf(face.Vertices[1]));
            triangles.Add(resultPoints.IndexOf(face.Vertices[2]));
        }

        m.triangles = triangles.ToArray();
        m.RecalculateNormals();

        //m = LowPolyConverter.Convert(m); //Converts the generated mesh to low poly

        return m;
    }

	public static void SidewalkMeshFromPoints(List<Vector3> path, float roadWidth, string meshName)
	{
	}

	public static Mesh RoadMeshFromPoints(List<Pair<Vector3>> points, string meshName)
	{
		Mesh mesh = new Mesh();
		mesh.name = meshName;

		List<int> triangles = new List<int>();
		List<Vector3> verts = new List<Vector3>();
		List<Vector3> norms = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();

		for (int i = 0; i < points.Count(); i++)
		{
			int vertsIndex = i * 2;
			int uvIndex = i % 2;

			//First vertex + uvs and norms
			verts.Add(points[i].First);
			norms.Add(Vector3.up);
			if (uvIndex == 0)
			{
				uvs.Add(new Vector2(0.0f, 0.0f));
			}
			else
			{
				uvs.Add(new Vector2(1.0f, 0.0f));
			}

			//Second vertex + uvs and norms
			verts.Add(points[i].Second);
			norms.Add(Vector3.up);
			if (uvIndex == 0)
			{
				uvs.Add(new Vector2(0.0f, 1.0f));
			}
			else
			{
				uvs.Add(new Vector2(1.0f, 1.0f));
			}

			//Make triangles
			if (i < points.Count() - 1)
			{
				//First Triangle
				triangles.Add(vertsIndex);
				triangles.Add(vertsIndex + 2);
				triangles.Add(vertsIndex + 1);

				//Second Triangle
				triangles.Add(vertsIndex + 1);
				triangles.Add(vertsIndex + 2);
				triangles.Add(vertsIndex + 3);
			}
			//Do the last point to loop it
			else 
			{
				//First Triangle
				triangles.Add(vertsIndex);
				triangles.Add(0);
				triangles.Add(vertsIndex + 1);

				//Second Triangle
				triangles.Add(vertsIndex + 1);
				triangles.Add(0);
				triangles.Add(1);
			}
		}

		mesh.vertices = verts.ToArray();
		mesh.normals = norms.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.name = meshName;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		return mesh;
	}
}