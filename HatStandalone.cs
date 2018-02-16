#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Net;
using System.Collections;
using System.Reflection;
//using Witty.Joke.Except.ImNotFunny;

/*
 * This program was written by a 15 year old named BlueFireExplosion. If you put your mind to it,
 * you can do anything. :) 
 * 
 * Credit to Nelson Sexton for Unturned and the modding community. Awesome game, my friend! 
 * */
public class HatStandalone : EditorWindow
{
	bool importdialog = false;

	[MenuItem("Window/Hat Creator")]
	static void Init()
	{
		HatStandalone window = (HatStandalone)EditorWindow.GetWindow(typeof(HatStandalone));

	}
	void OnEnable()
	{
		if (AssetDatabase.IsValidFolder ("Assets/Hat_Assets")) {
		} else {
			//AssetDatabase.ImportPackage ("Assets/ClothingCreator/PKGs/Hat.unitypackage", importdialog);
		}
	}
	void OnGUI()
	{
		GUILayout.Label ("Currently in Development", EditorStyles.boldLabel);
	}
	void OnDestroy()
	{

	}
	void OnInspectorUpdate()
	{

	}
	void bundleAssets()
	{

	}

}

#endif