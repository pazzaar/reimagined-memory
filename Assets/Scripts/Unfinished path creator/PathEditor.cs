using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
	PathCreator creator;
	Path path;

	private void OnSceneGUI()
	{
		Draw();
	}

	void Draw()
	{
		Handles.color = Color.red;

	}

	// Update is called once per frame
	void OnEnable()
    {
		creator = (PathCreator)target;
		if (creator.path == null)
		{
			creator.CreatePath();
		}
		path = creator.path;
    }
}
