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
 * 
 * Credit to Nelson Sexton for Unturned and the modding community. Awesome game, my friend! 
 * */
public enum HATMESHTYPE
{
	Custom = 0,
	Beret = 1,
	Bowler = 2,
	Coalition = 3,
	Engineer = 4,
	Farmer = 5,
	Fedora = 6,
	Fez = 7,
	Fireman = 8,
	Fishing = 9,
	Gladiator = 10,
	Headphones = 11,
	Jester = 12,
	Magician = 13,
	Military = 14,
	News = 15,
	Pilot = 16,
	Police = 17,
	RCMP = 18,
	Togue = 19,
	Ushanka = 20,
	Ushanka_Ruski = 21,
	Viking = 22,
	Witch = 23,


}
public class HatStandalone : EditorWindow
{
	public HATMESHTYPE hmt;
	bool importdialog = false;
	Texture2D meshTex;
	public Mesh hatMesh;
	GameObject hat, item;
	[MenuItem("Window/Hat Creator")]
	static void Init()
	{
		HatStandalone window = (HatStandalone)EditorWindow.GetWindow(typeof(HatStandalone));

	}
	void OnEnable()
	{
		if (AssetDatabase.IsValidFolder ("Assets/Hat_Assets")) {
		} else {
			AssetDatabase.ImportPackage ("Assets/ClothingCreator/PKGs/Hat.unitypackage", importdialog);
		}
		if (GameObject.Find ("Hat") == null) {
			UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath ("Assets/Hat_Assets/Resources/Bundles/Items/Hats/Beret_Forest/Hat.prefab", typeof(GameObject));
				hat = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
			hat.name = "Hat";
		}
		if (GameObject.Find ("Item") == null) {
			UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath ("Assets/Hat_Assets/Resources/Bundles/Items/Hats/Beret_Forest/Item.prefab", typeof(GameObject));
			item = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
			item.name = "Item";
		}
	}
	void OnGUI()
	{
		GUILayout.Label ("Currently in Development", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox ("Select the model you would like to use for your hat below. Choose 'Custom' if you would like to input your own model.", MessageType.Info);
		hmt = (HATMESHTYPE)EditorGUILayout.EnumPopup ("Hat Type:", hmt);
		if (hmt == HATMESHTYPE.Custom) {
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel ("Custom Mesh");
			if (GUILayout.Button ("Select", GUILayout.Width (200))) {
				string path = EditorUtility.OpenFilePanel ("Select Mesh", "", "obj,fbx");
				if (path.Length != 0) 
				{
					if (path.Contains (".fbx")) {
						File.Copy (path, Application.dataPath + "/Hat_Assets/Game/Sources/Models/Items/Hats/Beret_Forest/Model_0.fbx");
						hatMesh = (Mesh)AssetDatabase.LoadAssetAtPath ("Assets/Hat_Assets/Game/Sources/Models/Items/Hats/Beret_Forest/Model_0.fbx", typeof(Mesh));
					}
					else if (path.Contains (".obj")) {
						File.Copy (path, Application.dataPath + "/Hat_Assets/Game/Sources/Models/Items/Hats/Beret_Forest/Model_0.obj");
						hatMesh = (Mesh)AssetDatabase.LoadAssetAtPath ("Assets/Hat_Assets/Game/Sources/Models/Items/Hats/Beret_Forest/Model_0.obj", typeof(Mesh));
					}
				}
				//MeshFilter itemMeshFilter = item.GetComponent<MeshFilter> ();
				item.GetComponent<MeshFilter>().mesh = hatMesh;
				hat.GetComponentInChildren<MeshFilter> ().mesh = hatMesh;
				UnityEditor.AssetDatabase.Refresh ();
				
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.HelpBox (".blend files are currently unsupported. Please export them into FBX.", MessageType.Info);
		}
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