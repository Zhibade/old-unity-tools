using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class LightmapBatchBaking : EditorWindow {
	
	private static bool gOverrideLightmapSettings = false;
	
	private static readonly string[] gLightmapModeOptions = new string[3]{"Single Lightmaps", "Dual Lightmaps", "Directional Lightmaps"};
	private static int gLightmapModeIndex = 0;
	
	private static readonly string[] gLightmapQualityOptions = new string[2]{"High", "Low"};
	private static int gLightmapQualityIndex = 0;
	
	private static readonly string[] gLightmapBouncesOptions = new string[5]{"0","1","2","3","4"};
	private static int gLightmapBouncesIndex = 1;
	
	private static Color gSkyLightColor = new Color(0.86f, 0.93f, 1f, 1f);
	private static float gSkyLightIntensity = 0f;
	
	private static readonly float gBounceBoostMin = 0f;
	private static readonly float gBounceBoostMax = 4f;
	private static float gBounceBoostValue = 1f;
	
	private static readonly float gBounceIntensityMin = 0f;
	private static readonly float gBounceIntensityMax = 5f;
	private static float gBounceIntensityValue = 1f;
	
	private static int gFinalGatherRays = 1000;
	
	private static readonly float gContrastThresholdMin = 0f;
	private static readonly float gContrastThresholdMax = 0.5f;
	private static float gContrastThresholdValue = 0.05f;
	
	private static readonly float gInterpolationMin = 0f;
	private static readonly float gInterpolationMax = 1;
	private static float gInterpolationValue = 0f;
	
	private static readonly int gInterpolationPointsMin = 15;
	private static readonly int gInterpolationPointsMax = 30;
	private static int gInterpolationPointsValue = 15;
	
	private static readonly float gAoMin = 0f;
	private static readonly float gAoMax = 1;
	private static float gAoValue = 1f;
	
	private static float gAoMaxDistance = 0f;
	
	private static readonly float gAoContrastMin = 0f;
	private static readonly float gAoContrastMax = 2f;
	private static float gAoContrastValue = 1f;
	
	private static bool gLockAtlas = false;
	
	private static int gResolution = 50;
	private static int gPadding = 0;
	
	private static readonly string[] gMaxLightmapSizeOptions = new string[6]{"128", "256", "512", "1024", "2048", "4096"};
	private static int gMaxLightmapSizeIndex = 3;
	
	private static bool gSceneFoldout = false;
	private static string[] gSceneList;
	private static bool[] gSceneCheckboxes;
	
	private static bool batchStarted = false;
	
	private static int currentScene = 0;
	
	[MenuItem ("Custom Tools/Lightmap Batch Baking Tool")]
	private static void Init()
	{
		EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
		
		List<string> sceneList = new List<string>();
		
		foreach (EditorBuildSettingsScene scene in scenes)
		{
			sceneList.Add(scene.path);
		}
		
		gSceneList = sceneList.ToArray();
		gSceneCheckboxes = new bool[gSceneList.Length];
		
		for (int i = 0; i < gSceneCheckboxes.Length; i++)
		{
			gSceneCheckboxes[i] = true;
		}
		
		batchStarted = false;
		
		EditorWindow.GetWindow(typeof(LightmapBatchBaking), true, "Lightmap Batch Baking Tool");
	}
	
	private static bool PreCheck()
	{
		if (Lightmapping.isRunning || batchStarted)
		{
			Debug.LogError("Beast is currently baking lightmaps. Please cancel the baking process to run the tool");
			return false;
		}
		
		return true;
	}
	
	private static void Run()
	{
		batchStarted = true;
		
		EditorApplication.OpenScene(gSceneList[currentScene]);
		BakeCurrentScene();
	}
	
	private static void Cancel()
	{
		Lightmapping.Cancel();
		
		Debug.Log(string.Format("Cancelled bake. {0} scene lightmap bake not finished.", gSceneList[currentScene]));
		
		currentScene = 0;
		batchStarted = false;
	}
	
	private static void BakeCurrentScene()
	{
		LightmapEditorSettings.maxAtlasHeight = Convert.ToInt32(gMaxLightmapSizeOptions[gMaxLightmapSizeIndex]);
		LightmapEditorSettings.maxAtlasWidth = Convert.ToInt32(gMaxLightmapSizeOptions[gMaxLightmapSizeIndex]);
		
		if (gOverrideLightmapSettings)
		{
			LightmapsMode tempMode = LightmapSettings.lightmapsMode;
			LightmapBakeQuality tempQuality = LightmapEditorSettings.quality;
			int tempBounces = LightmapEditorSettings.bounces;
			Color tempSkyColor = LightmapEditorSettings.skyLightColor;
			float tempSkyIntensity = LightmapEditorSettings.skyLightIntensity;
			float tempBounceBoost = LightmapEditorSettings.bounceBoost;
			float tempBounceIntensity = LightmapEditorSettings.bounceIntensity;
			int tempFinalGatherRays = LightmapEditorSettings.finalGatherRays;
			float tempContrastThreshold = LightmapEditorSettings.finalGatherContrastThreshold;
			float tempInterpolation = LightmapEditorSettings.finalGatherGradientThreshold;
			int tempInterpolationPoints = LightmapEditorSettings.finalGatherInterpolationPoints;
			float tempAo = LightmapEditorSettings.aoAmount;
			float tempAoMaxDistance = LightmapEditorSettings.aoMaxDistance;
			float tempAoContrast = LightmapEditorSettings.aoContrast;
			bool tempLockAtlas = LightmapEditorSettings.lockAtlas;
			float tempResolution = LightmapEditorSettings.resolution;
			int tempPadding = LightmapEditorSettings.padding;
			
			LightmapSettings.lightmapsMode = (LightmapsMode)gLightmapModeIndex;
			LightmapEditorSettings.quality = (LightmapBakeQuality)gLightmapQualityIndex;
			LightmapEditorSettings.bounces = gLightmapBouncesIndex;
			LightmapEditorSettings.skyLightColor = gSkyLightColor;
			LightmapEditorSettings.skyLightIntensity = gSkyLightIntensity;
			LightmapEditorSettings.bounceBoost = gBounceBoostValue;
			LightmapEditorSettings.bounceIntensity = gBounceIntensityValue;
			LightmapEditorSettings.finalGatherRays = gFinalGatherRays;
			LightmapEditorSettings.finalGatherContrastThreshold = gContrastThresholdValue;
			LightmapEditorSettings.finalGatherGradientThreshold = gInterpolationValue;
			LightmapEditorSettings.finalGatherInterpolationPoints = gInterpolationPointsValue;
			LightmapEditorSettings.aoAmount = gAoValue;
			LightmapEditorSettings.aoMaxDistance = gAoMaxDistance;
			LightmapEditorSettings.aoContrast = gAoContrastValue;
			LightmapEditorSettings.lockAtlas = gLockAtlas;
			LightmapEditorSettings.resolution = gResolution;
			LightmapEditorSettings.padding = gPadding;
			
			Lightmapping.BakeAsync();

			LightmapSettings.lightmapsMode = tempMode;
			LightmapEditorSettings.quality = tempQuality;
			LightmapEditorSettings.bounces = tempBounces;
			LightmapEditorSettings.skyLightColor = tempSkyColor;
			LightmapEditorSettings.skyLightIntensity = tempSkyIntensity;
			LightmapEditorSettings.bounceBoost = tempBounceBoost;
			LightmapEditorSettings.bounceIntensity = tempBounceIntensity;
			LightmapEditorSettings.finalGatherRays = tempFinalGatherRays;
			LightmapEditorSettings.finalGatherContrastThreshold = tempContrastThreshold;
			LightmapEditorSettings.finalGatherGradientThreshold = tempInterpolation;
			LightmapEditorSettings.finalGatherInterpolationPoints = tempInterpolationPoints;
			LightmapEditorSettings.aoAmount = tempAo;
			LightmapEditorSettings.aoMaxDistance = tempAoMaxDistance;
			LightmapEditorSettings.aoContrast = tempAoContrast;
			LightmapEditorSettings.lockAtlas = tempLockAtlas;
			LightmapEditorSettings.resolution = tempResolution;
			LightmapEditorSettings.padding = tempPadding;
		}
		else
		{
			Lightmapping.BakeAsync();
		}
		
		Debug.Log(string.Format("Baking {0}...", gSceneList[currentScene]));
	}
	
	void OnGUI()
	{
		EditorGUILayout.BeginVertical();
		
		EditorGUILayout.Space();
		
		gOverrideLightmapSettings = EditorGUILayout.Toggle("Override Scene Settings", gOverrideLightmapSettings);
		
		EditorGUILayout.Space();
		
		if (gOverrideLightmapSettings == false)
		{
			GUI.enabled = false;
		}
		
		gLightmapModeIndex = EditorGUILayout.Popup("Mode", gLightmapModeIndex, gLightmapModeOptions);
		gLightmapQualityIndex = EditorGUILayout.Popup("Quality", gLightmapQualityIndex, gLightmapQualityOptions);
		gLightmapBouncesIndex = EditorGUILayout.Popup("Bounces", gLightmapBouncesIndex, gLightmapBouncesOptions);
		
		gSkyLightColor = EditorGUILayout.ColorField("Sky Light Color", gSkyLightColor);
		gSkyLightIntensity = EditorGUILayout.FloatField("Sky Light Intensity", gSkyLightIntensity);
		
		gSkyLightIntensity = (gSkyLightIntensity < 0f) ? 0f : gSkyLightIntensity;
		
		gBounceBoostValue = EditorGUILayout.Slider("Bounce Boost", gBounceBoostValue, gBounceBoostMin, gBounceBoostMax);
		gBounceIntensityValue = EditorGUILayout.Slider("Bounce Intensity", gBounceIntensityValue, gBounceIntensityMin, gBounceIntensityMax);
		
		gFinalGatherRays = EditorGUILayout.IntField("Final Gather Rays", gFinalGatherRays);
		
		gFinalGatherRays = (gFinalGatherRays < 1) ? 1 : gFinalGatherRays;
		
		gContrastThresholdValue = EditorGUILayout.Slider("Contrast Threshold", gContrastThresholdValue, gContrastThresholdMin, gContrastThresholdMax);
		gInterpolationValue = EditorGUILayout.Slider("Interpolation", gInterpolationValue, gInterpolationMin, gInterpolationMax);
		gInterpolationPointsValue = EditorGUILayout.IntSlider("Interpolation Points", gInterpolationPointsValue, gInterpolationPointsMin, gInterpolationPointsMax);
		
		gAoValue = EditorGUILayout.Slider("Ambient Occlusion", gAoValue, gAoMin, gAoMax);
		
		if (gAoValue > 0f)
		{
			EditorGUI.indentLevel++;
			
			gAoMaxDistance = EditorGUILayout.FloatField("Max Distance", gAoMaxDistance);
			
			gAoMaxDistance = (gAoMaxDistance < 0f) ? 0f : gAoMaxDistance;
			
			gAoContrastValue = EditorGUILayout.Slider("Contrast", gAoContrastValue, gAoContrastMin, gAoContrastMax);
			
			EditorGUI.indentLevel--;
		}
		
		EditorGUILayout.Space();
		
		gLockAtlas = EditorGUILayout.Toggle("Lock Atlas", gLockAtlas);
		
		if (gLockAtlas)
		{
			GUI.enabled = false;
		}
		
		gResolution = EditorGUILayout.IntField("Resolution", gResolution);
		
		gResolution = (gResolution < 0) ? 0 : gResolution;
		
		gPadding = EditorGUILayout.IntField("Padding", gPadding);
		
		if (gLockAtlas)
		{
			GUI.enabled = true;
		}
		
		EditorGUILayout.Space();
		
		if (gOverrideLightmapSettings == false)
		{
			GUI.enabled = true;
		}
		
		gMaxLightmapSizeIndex = EditorGUILayout.Popup("Max Lightmap Size", gMaxLightmapSizeIndex, gMaxLightmapSizeOptions);
		
		EditorGUILayout.Space();
		
		gSceneFoldout = EditorGUILayout.Foldout(gSceneFoldout, "Scene List");
		
		if (gSceneFoldout)
		{
			EditorGUI.indentLevel++;
			
			for (int i = 0; i < gSceneCheckboxes.Length; i++)
			{
				string sceneName = Path.GetFileNameWithoutExtension(gSceneList[i]);
				
				gSceneCheckboxes[i] = EditorGUILayout.Toggle(sceneName, gSceneCheckboxes[i]);
			}
			
			EditorGUI.indentLevel--;
		}
		
		EditorGUILayout.Space();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		if (batchStarted == false)
		{
			if (GUILayout.Button("Batch Bake"))
			{
				if (PreCheck())
				{
					Run();
				}
			}
		}
		else
		{
			if (GUILayout.Button("Cancel"))
			{
				Cancel();
			}
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		EditorGUILayout.EndVertical();
	}
	
	void Update()
	{
		if (Lightmapping.isRunning == false && batchStarted)
		{
			EditorApplication.SaveScene();
			
			Debug.Log(string.Format("Finished baking {0}.", gSceneList[currentScene]));
			
			currentScene++;
			
			if (currentScene < gSceneList.Length)
			{
				EditorApplication.OpenScene(gSceneList[currentScene]);
				BakeCurrentScene();
			}
			else
			{
				batchStarted = false;
			}
		}
	}
}