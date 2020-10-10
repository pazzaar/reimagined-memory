using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Utils;

public class MeshMaker : MonoBehaviour
{  
	public static Mesh RoadMeshAlongPath(List<Pair<Vector3>> points, string meshName, bool closedLoop)
	{
		Mesh mesh = new Mesh();
		mesh.name = meshName;

		int closedAdjustment = closedLoop ? 0 : 1;

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

			//Second vertex + uvs and norms
			verts.Add(points[i].Second);
			norms.Add(Vector3.up);

			//Uvs
			float uvProgression = i / (float)(points.Count() - 1 - closedAdjustment);
			uvs.Add(new Vector2(0.0f, uvProgression));
			uvs.Add(new Vector2(1.0f, uvProgression));

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
			else if (closedLoop)
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

	public static Mesh ExtrudeShapeMeshAlongPath(List<List<ShapePoint>> points, string meshName, bool loopPath = true, bool loopExtrudeShape = true)
	{
		Mesh mesh = new Mesh();
		mesh.name = meshName;

		List<int> triangles = new List<int>();
		List<Vector3> verts = new List<Vector3>();
		List<Vector3> norms = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();

		for (int i = 0; i < points.Count(); i++)
		{
			int uvIndex = i % 2;
			int pointsInExtrudeShape = 0;
			foreach (var p in points.First())
			{
				pointsInExtrudeShape += p.SoftNormal ? 1 : 2; //If its a hard normal, you need 2 verticies
			}

			int vertsIndex = (i * pointsInExtrudeShape);
			for (int j = 0; j < points[i].Count; j++)
			{

				/**Add verticies**/

				//If its a hard normal, add another vertex
				if (!points[i][j].SoftNormal)
				{
					vertsIndex += 1;
					verts.Add(points[i][j].Point);
				}

				verts.Add(points[i][j].Point);


				//if (points[i][j].SoftNormal == false)
				//{
				//	//Hard point, add 2 verticies at the same point
				//	verts.Add(points[i][j].Point);
				//	verts.Add(points[i][j].Point);

				//	/**Calculate normals for the face**/
				//	//Not the last point in the path
				//	if (i < points.Count() - 1)
				//	{
				//		//Not the last point in the extrude shape
				//		if (j < points[i].Count() - 1)
				//		{
				//			norms.Add(-Vector3.Cross(points[i][j + 1].Point - points[i][j].Point, points[i + 1][j].Point - points[i][j].Point).normalized);
				//			norms.Add(-Vector3.Cross(points[i][j + 1].Point - points[i][j].Point, points[i + 1][j].Point - points[i][j].Point).normalized);
				//		}
				//		//If the extrude shape is connected (like a cylinder), connect the last point up to the first point
				//		else if (loopExtrudeShape)
				//		{
				//			norms.Add(-Vector3.Cross(points[i][0].Point - points[i][j].Point, points[i + 1][j].Point - points[i][j].Point).normalized);
				//			norms.Add(-Vector3.Cross(points[i][0].Point - points[i][j].Point, points[i + 1][j].Point - points[i][j].Point).normalized);
				//		}
				//	}
				//	//If we're looping, connect back to the start of the path (so point[0])
				//	else if (loopPath)
				//	{
				//		//Not the last point in the extrude shape
				//		if (j < points[i].Count() - 1)
				//		{
				//			norms.Add(-Vector3.Cross(points[i][j + 1].Point - points[i][j].Point, points[0][j].Point - points[i][j].Point).normalized);
				//			norms.Add(-Vector3.Cross(points[i][j + 1].Point - points[i][j].Point, points[0][j].Point - points[i][j].Point).normalized);
				//		}
				//		//If the extrude shape is connected (like a cylinder), connect the last point up to the first point
				//		else if (loopExtrudeShape)
				//		{
				//			norms.Add(-Vector3.Cross(points[i][0].Point - points[i][j].Point, points[0][j].Point - points[i][j].Point).normalized);
				//			norms.Insert(0, -Vector3.Cross(points[i][0].Point - points[i][j].Point, points[0][j].Point - points[i][j].Point).normalized);
				//		}
				//	}
				//	vertsIndex += 1; //Offset
				//}
				//else
				//{
				//	//Soft point, add 1 vertex
				//	verts.Add(points[i][j].Point);

				//	//Calculate normals
				//	if (i < points.Count() - 1 && j < points[i].Count() - 1)
				//	{
				//		norms.Add(Vector3.Cross(points[i][j + 1].Point - points[i][j].Point, points[i + 1][j].Point - points[i][j].Point).normalized);
				//	}
				//	else if (i < points.Count() && j < points[i].Count() - 1)
				//	{
				//		norms.Add(Vector3.Cross(points[i][j + 1].Point - points[i][j].Point, points[0][j].Point - points[i][j].Point).normalized);
				//	}
				//	else if (i < points.Count() - 1 && j < points[i].Count())
				//	{
				//		norms.Add(Vector3.Cross(points[i][0].Point - points[i][j].Point, points[i + 1][j].Point - points[i][j].Point).normalized);
				//	}
				//	else
				//	{
				//		norms.Add(Vector3.Cross(points[i][0].Point - points[i][j].Point, points[0][j].Point - points[i][j].Point).normalized);
				//	}
				//}


				//Make sure we're not on the last extrude shape in the path
				if (i < points.Count - 1)
				{
					//Make sure we're not on the last point in the extrude shape
					if (j < points[i].Count - 1)
					{
						//First Triangle
						triangles.Add(vertsIndex);
						triangles.Add(vertsIndex + pointsInExtrudeShape);
						triangles.Add(vertsIndex + 1);

						//Second Triangle
						triangles.Add(vertsIndex + 1);
						triangles.Add(vertsIndex + pointsInExtrudeShape);
						triangles.Add(vertsIndex + pointsInExtrudeShape + 1);
					}
					//The extrude shape makes a closed shape (like a cylinder instead of a flat road or something)
					//So connect it up to the first point in the extrude shape
					else if (loopExtrudeShape)
					{
						//First triangle
						triangles.Add(vertsIndex);
						triangles.Add(vertsIndex + pointsInExtrudeShape);
						triangles.Add(i * pointsInExtrudeShape + 0);

						//Second Triangle
						triangles.Add(i * pointsInExtrudeShape + 0);
						triangles.Add(vertsIndex + pointsInExtrudeShape);
						triangles.Add(i * pointsInExtrudeShape + pointsInExtrudeShape + 0);
					}
				}
				//Do the last point to loop it
				else if (loopPath)
				{
					if (j < points[i].Count - 1)
					{
						var lastPointVertsIndex = vertsIndex - ((points.Count - 1) * pointsInExtrudeShape);
						//First Triangle
						triangles.Add(vertsIndex);
						triangles.Add(lastPointVertsIndex); //makes it clearer
						triangles.Add(vertsIndex + 1);

						//Second Triangle
						triangles.Add(vertsIndex + 1);
						triangles.Add(lastPointVertsIndex); //makes it clearer
						triangles.Add(lastPointVertsIndex + 1); //makes it clearer
					}
					else if (loopExtrudeShape)
					{
						var lastPointVertsIndex = vertsIndex - ((points.Count - 1) * pointsInExtrudeShape);
						//First triangle
						triangles.Add(vertsIndex);
						triangles.Add(lastPointVertsIndex);
						triangles.Add(i * pointsInExtrudeShape + 0);

						//Second triangle
						triangles.Add(i * pointsInExtrudeShape + 0);
						triangles.Add(lastPointVertsIndex);
						triangles.Add(0); //Back to the first vertex added
					}
				}

				vertsIndex += 1;
			}
		}

		mesh.vertices = verts.ToArray();
		mesh.normals = norms.ToArray();
		mesh.triangles = triangles.ToArray();
		//mesh.uv = uvs.ToArray();
		mesh.name = meshName;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		return mesh;
	}

	public static Mesh ExtrudeShapeAlongPath(List<Vector3> points, List<TemplatePoint> template, string meshName, bool loopPath = true, bool loopExtrudeShape = true)
	{
		Mesh mesh = new Mesh();
		mesh.name = meshName;

		int closedAdjustment = loopPath ? 0 : 1;

		List<int> triangles = new List<int>();
		List<Vector3> verts = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();

		for (int i = 0; i < points.Count(); i++)
		{
			float uvProgression = i / (float)(points.Count() - 1 - closedAdjustment);
			int vertsIndex = (i * template.CountNormals());

			for (int j = 0; j < template.Count(); j++)
			{
				/**Add verticies and UVs**/
				//If its a hard normal, add another vertex + uv
				if (!template[j].SoftNormal)
				{
					vertsIndex += 1;
					verts.Add(points.GetShapePoint(i, template[j].Point));
					uvs.Add(new Vector2(template[j].UMap, uvProgression));
				}
				verts.Add(points.GetShapePoint(i, template[j].Point));
				uvs.Add(new Vector2(template[j].UMap, uvProgression));

				/**Add triangles**/
				//Make sure we're not on the last extrude shape in the path
				if (i < points.Count - 1)
				{
					//Make sure we're not on the last point in the extrude shape
					if (j < template.Count() - 1)
					{
						//First Triangle
						triangles.Add(vertsIndex);
						triangles.Add(vertsIndex + template.CountNormals());
						triangles.Add(vertsIndex + 1);

						//Second Triangle
						triangles.Add(vertsIndex + 1);
						triangles.Add(vertsIndex + template.CountNormals());
						triangles.Add(vertsIndex + template.CountNormals() + 1);
					}
					//The extrude shape makes a closed shape (like a cylinder instead of a flat road or something)
					//So connect it up to the first point in the extrude shape
					else if (loopExtrudeShape)
					{
						//First triangle
						triangles.Add(vertsIndex);
						triangles.Add(vertsIndex + template.CountNormals());
						triangles.Add(i * template.CountNormals() + 0);

						//Second Triangle
						triangles.Add(i * template.CountNormals() + 0);
						triangles.Add(vertsIndex + template.CountNormals());
						triangles.Add(i * template.CountNormals() + template.CountNormals() + 0);
					}
				}
				//Do the last point to loop it
				else if (loopPath)
				{
					if (j < template.Count() - 1)
					{
						var lastPointVertsIndex = vertsIndex - ((points.Count - 1) * template.CountNormals());
						//First Triangle
						triangles.Add(vertsIndex);
						triangles.Add(lastPointVertsIndex); //makes it clearer
						triangles.Add(vertsIndex + 1);

						//Second Triangle
						triangles.Add(vertsIndex + 1);
						triangles.Add(lastPointVertsIndex); //makes it clearer
						triangles.Add(lastPointVertsIndex + 1); //makes it clearer
					}
					else if (loopExtrudeShape)
					{
						var lastPointVertsIndex = vertsIndex - ((points.Count - 1) * template.CountNormals());
						//First triangle
						triangles.Add(vertsIndex);
						triangles.Add(lastPointVertsIndex);
						triangles.Add(i * template.CountNormals() + 0);

						//Second triangle
						triangles.Add(i * template.CountNormals() + 0);
						triangles.Add(lastPointVertsIndex);
						triangles.Add(0); //Back to the first vertex added
					}
				}

				vertsIndex += 1;
			}
		}

		mesh.vertices = verts.ToArray();
		mesh.normals = new List<Vector3>().ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.name = meshName;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		return mesh;
	}

	public static Mesh FourWayIntersection(List<Pair<Vector3>> outerPoints, List<Vector3> innerPoints, string name)
	{
		Mesh mesh = new Mesh();

		List<int> triangles = new List<int>();
		List<Vector3> verts = new List<Vector3>();
		List<Vector3> norms = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();


		for (int i = 0; i < 4; i++)
		{
			verts.Add(innerPoints[i]);
			norms.Add(Vector3.up);
		}

		triangles.Add(0);
		triangles.Add(2);
		triangles.Add(3);

		triangles.Add(0);
		triangles.Add(1);
		triangles.Add(2);

		int innerVertsIndex = 4;
		int outerVertsIndex = 0;

		for (int i = 0; i < 4; i++)
		{
			//First vertex + uvs and norms
			verts.Add(outerPoints[i].First);
			norms.Add(Vector3.up);

			//Second vertex + uvs and norms
			verts.Add(outerPoints[i].Second);
			norms.Add(Vector3.up);

			triangles.Add(innerVertsIndex + outerVertsIndex);
			triangles.Add((i + 1) % 4);
			triangles.Add(i);

			triangles.Add(innerVertsIndex + outerVertsIndex);
			triangles.Add(innerVertsIndex + outerVertsIndex + 1);
			triangles.Add((i + 1) % 4);

			triangles.Add(innerVertsIndex + outerVertsIndex + 1);
			triangles.Add(innerVertsIndex + ((outerVertsIndex + 2) % 8));
			triangles.Add((i + 1) % 4);

			outerVertsIndex += 2;
		}

		mesh.vertices = verts.ToArray();
		mesh.normals = norms.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = UvCalculator.CalculateUVs(verts.ToArray(), 100);
		mesh.name = name;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		return mesh;
	}
}