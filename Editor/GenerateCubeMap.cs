using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class GenerateCubeMap : EditorWindow {
	
	private static GameObject referenceGameObject;
	private static Cubemap cubemap;
	
	private static readonly string[] gDropDownOptions = new string[8]{"32", "64", "128", "256", "512", "1024", "2048", "4096"};
	private static int gDropDownIndex = 0;
	
	[MenuItem ("Custom Tools/Generate Cubemap")]
	private static void Init()
	{
		EditorWindow.GetWindow(typeof(GenerateCubeMap), true, "Generate Cubemap Tool");
	}
	
	private static void RenderCubemap()
	{
		if (referenceGameObject.GetComponent<Camera>() == null)
		{
			referenceGameObject.AddComponent<Camera>();
		}
		
		Camera cam = referenceGameObject.GetComponent<Camera>();
		
		string targetPath = AssetDatabase.GetAssetPath(cubemap);
		int cubemapSize = Convert.ToInt32(gDropDownOptions[gDropDownIndex]);

		Cubemap temp = new Cubemap(cubemapSize, TextureFormat.ARGB32, true);	
		
		AssetDatabase.CreateAsset(temp, targetPath);
		cubemap = (Cubemap)AssetDatabase.LoadAssetAtPath(targetPath, typeof(Cubemap));
		
		cam.RenderToCubemap(cubemap);
	}
	
	private static bool InputCheck()
	{
	if (referenceGameObject == null || cubemap == null)
		{
			Debug.LogError("Please assign the required objects first.");
			return false;
		}
		else
		{
			return true;
		}
	}
	
	void OnGUI()
	{
		EditorGUILayout.BeginVertical();
		referenceGameObject = (GameObject)EditorGUILayout.ObjectField("Cubemap position:", referenceGameObject, typeof(GameObject), true);
		cubemap = (Cubemap)EditorGUILayout.ObjectField("Target Cubemap:", cubemap, typeof(Cubemap), true);
		
		gDropDownIndex = EditorGUILayout.Popup("Cubemap Size: ", gDropDownIndex, gDropDownOptions);
		
		EditorGUILayout.Space();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		if (GUILayout.Button("Generate"))
		{
			if (InputCheck())
			{
				RenderCubemap();
			}
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		EditorGUILayout.EndVertical();
	}
}