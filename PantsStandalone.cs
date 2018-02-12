#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Net;
using System.Collections;
using System.Reflection;


//WHAT I NEED TO STILL DO:
//
//ADD DYNAMIC STORAGE REPRESENTATIONS
//BUILTIN-TEXTURE MAKER

/*
 * This program was written by a 15 year old named BlueFireExplosion. If you put your mind to it,
 * you can do anything. :) 
 * 
 * Credit to Nelson Sexton for Unturned and the modding community. Awesome game, my friend! 
 * */
public class PantsStandalone : EditorWindow
{
	//The variable named "selection" below is legacy, and was part of my attempts to integrate Nelson's bundling system. It failed.
	//private Object[] selection;
	UnityEditor.EditorWindow window;
	public RARENESS pantsRarity;
	public RenderTexture PantsScenePreview;
	string itemName = "Blue Jeans";
	string itemDescription = "Faded blue jeans.";
	//This path is private to maintain integrity, and static to be defined as a type and not an object.
	private static string path, customNameString = ""; //url = "https://cdn.discordapp.com/attachments/296241012483817473/411961984620167168/Pants.unitypackage";
	private static int maxTags = 10000, maxLayers = 31;
	//These are the various variables set by the user in the .dat file settings.
	int itemID, armorLevel = 1, itemWidth, itemHeight, itemStorageWidth, itemStorageHeight;
	double actualArmor;
	bool generateDats, createdItem = false, fireResistant = false, waterproof = false,
	armor = false, standardBlueprints = true, customItemSize = false, custFileName = false, 
	cleanProject = false, KeepManifests = false, devFold = false, importdialog = false, displayErrors = false, hasRan = false;
	static bool isOpen = false;
	//The two Texture2Ds used to show previews of the Image Texture and InGame preview.
	Texture2D PantsTex, PantsPreview, PantsEm, PantsMet;
	//The float representing the rotation of Holder, which controls the camera angle.
	float camRotation = 0f, smoothness, metallicLevel;
	GameObject holder, cam, clone;
	Camera camera;
	Material mat;
	//Just defining this string so I can use it before being "officially" defined.
	string itemNameFlat = "";
	//The below bools are legacy, and were planned for use when I was making a dynamic storage visualizer.
	//bool storageFold = true, sizeFold = true;
	//This should be coming soon
	//Texture2D PantsEm;
	[MenuItem("Window/Pants Creator")]
	static void Init()
	{
		// Get existing open window or if none, make a new one:
		UnityEditor.EditorWindow window = GetWindow(typeof(PantsStandalone));
		isOpen = true;
		//Show the window when it is called on
		window.minSize = new Vector2(360,1100);
		window.Show();
		/*if (!(AssetDatabase.IsValidFolder ("Pants_Assets"))) {
			WebClient client = new WebClient();
			client.DownloadFile (url, Application.persistentDataPath + "/" + "Pants.unitypackage");
		}*/
	}
	void OnEnable()
	{
		//PantsEm = new Texture2D (256, 256);
		//Loading the icon for the window.
		Texture icon = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/ClothingCreator/Icons/zombie.png");
		if (AssetDatabase.IsValidFolder ("Assets/Pants_Assets")) {
		} else {
			AssetDatabase.ImportPackage ("Assets/ClothingCreator/PKGs/Pants.unitypackage", importdialog);
		}
		//Setting the content of the window, i.e the title and icon.
		GUIContent stitleContent = new GUIContent ("Pants Creator", icon);
		PantsStandalone window = (PantsStandalone)EditorWindow.GetWindow(typeof(PantsStandalone));
		window.titleContent = stitleContent;
	}

	void OnGUI()
	{
		//Debug.Log (Application.dataPath);
		GUILayout.Label("General Settings", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox ("Select the texture with the button below. Emission & Metallic textures are optional. Metallic shaders are experimental, expect issues.", MessageType.Info);
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.PrefixLabel ("Image Texture");
		//This little void allows me to open a file prompt looking for .png files (Must support transparency at the moment)
		if (GUILayout.Button ("Select", GUILayout.Width (200))) 
		{
			string path = EditorUtility.OpenFilePanel("Select Texture", "", "png");
			if (path.Length != 0)
			{
				//The magical and still confusing process of reading bytes from the file into a var.
				var fileContent = File.ReadAllBytes(path);
				//Loading that magical var into the Texture2D's image slot.
				PantsTex.LoadImage(fileContent);
				//This is the most important, as it applies the changes we have made to the texture.
				PantsTex.Apply ();
				AssetDatabase.Refresh ();
			}
		}
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.PrefixLabel ("Emission Texture");
		if (GUILayout.Button ("Select", GUILayout.Width (200))) 
		{
			string path = EditorUtility.OpenFilePanel("Select Emission Texture", "", "png,jpeg,jpg");
			if (path.Length != 0)
			{
				var fileContent = File.ReadAllBytes(path);
				PantsEm.LoadImage (fileContent);
				PantsEm.Apply ();
				mat.SetColor ("_EmissionColor", Color.white);
				AssetDatabase.Refresh ();
			}
		}
		/*Honestly most of the following doesn't need explaining, it's just buttons and switches and sliders. 
		 * If you'd like to learn about how they work, I'd recommend searching "EditorGUILayout" and "GUILayout" and reading the unity documentation. 
		 * It's very helpful.
		 * */
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.PrefixLabel ("Metallic Texture");
		if (GUILayout.Button ("Select", GUILayout.Width (200))) 
		{
			string path = EditorUtility.OpenFilePanel("Select Metallic Texture", "", "png");
			if (path.Length != 0)
			{
				var fileContent = File.ReadAllBytes(path);
				PantsMet.LoadImage (fileContent);
				PantsMet.Apply ();
				mat.SetTexture ("_MetallicGlossMap", PantsMet);
				AssetDatabase.ImportAsset ("Assets/Pants_Assets/Resources/Bundles/Items/Pants/Jeans_Work/Material.mat");
				AssetDatabase.Refresh ();
			}
			UnityEditor.AssetDatabase.Refresh();
		}
		EditorGUILayout.EndHorizontal ();
		smoothness = EditorGUILayout.Slider ("Smoothness", smoothness, 0, 1, GUILayout.Width (350));
		/*Honestly most of the following doesn't need explaining, it's just buttons and switches and sliders. 
		 * If you'd like to learn about how they work, I'd recommend searching "EditorGUILayout" and "GUILayout" and reading the unity documentation. 
		 * It's very helpful.
		 * */
		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Reset Emission", GUILayout.Width (173))) {
			mat.SetColor ("_EmissionColor", Color.black);
		}
		if (GUILayout.Button ("Reset Metallic", GUILayout.Width (173))) {
			mat.SetTexture ("_MetallicGlossMap", null);
			mat.SetFloat ("_Metallic", 0);
			AssetDatabase.ImportAsset ("Assets/Pants_Assets/Resources/Bundles/Items/Pants/Jeans_Work/Material.mat");
			AssetDatabase.Refresh ();
		}
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.PrefixLabel ("Texture Preview");
		GUILayout.Label ("In-Game Preview", GUILayout.Width(150));
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Box (AssetPreview.GetAssetPreview (PantsTex));
		GUILayout.Box (PantsScenePreview);
		EditorGUILayout.EndHorizontal ();
		camRotation = EditorGUILayout.Slider ("Rotate Preview", camRotation, 0, 360, GUILayout.Width (350));
		generateDats = EditorGUILayout.BeginToggleGroup("Generate .DAT Files", generateDats);
		EditorGUILayout.HelpBox ("These are the stats files for your item.", MessageType.Info);
		itemName = EditorGUILayout.TextField("Item Name", itemName, GUILayout.Width (350));
		itemDescription = EditorGUILayout.TextField("Item Description", itemDescription, GUILayout.Width (350));
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.PrefixLabel ("ID Number");
		itemID = EditorGUILayout.IntSlider (itemID, 2000, 65000, GUILayout.Width (200));
		EditorGUILayout.EndHorizontal ();
		pantsRarity = (RARENESS)EditorGUILayout.EnumPopup ("Item Rarity", pantsRarity, GUILayout.Width (350));
		/*
		 * Currently unimplemented dynamic preview system for Width and Height of Item and storage size. Hopefully will be implemented at a later date.
		 * 
		 * storageFold = EditorGUILayout.Foldout(storageFold, "Storage Preview");
		 * sizeFold = EditorGUILayout.Foldout (sizeFold, "Size Preview");
		*/
		fireResistant = EditorGUILayout.Toggle("Fire Resistant", fireResistant, GUILayout.Width (350));
		waterproof = EditorGUILayout.Toggle("Waterproof", waterproof, GUILayout.Width (350));
		//Remind me to add a visual blueprint creator to this at some point
		standardBlueprints = EditorGUILayout.Toggle ("Add Standard Blueprints", standardBlueprints, GUILayout.Width (350));
		EditorGUILayout.HelpBox ("The Standard Blueprints option adds the blueprint to scrap the pants into cloth, and the blueprint to repair it with cloth.", MessageType.Info);
		customItemSize = EditorGUILayout.BeginToggleGroup ("Custom Item Size", customItemSize);
		itemWidth = EditorGUILayout.IntSlider ("Custom Item Width", itemWidth, 1, 5, GUILayout.Width (350));
		itemHeight = EditorGUILayout.IntSlider ("Custom Item Height", itemHeight, 1, 5, GUILayout.Width (350));
		EditorGUILayout.HelpBox ("This is how large in grid squares your item will take up. Disabling Custom Item Size will reset to the default size, 3 squares wide by 2 squares tall.", MessageType.Info);
		EditorGUILayout.EndToggleGroup ();
		armor = EditorGUILayout.BeginToggleGroup("Armor", armor);
		armorLevel = EditorGUILayout.IntSlider("Damage Absorption", armorLevel, 1, 100, GUILayout.Width (350));
		GUILayout.Label (armorLevel + "% of any damage to the torso will be absorbed.");
		EditorGUILayout.EndToggleGroup();
		GUILayout.Label("Item Storage Slots", EditorStyles.boldLabel);
		itemStorageWidth = EditorGUILayout.IntSlider ("Storage Width", itemStorageWidth, 1, 15, GUILayout.Width (350));
		itemStorageHeight = EditorGUILayout.IntSlider ("Storage Height", itemStorageHeight, 1, 15, GUILayout.Width (350));
		EditorGUILayout.HelpBox ("This is how large in grid squares your item's storage slots will be when equipped. A good average is around 4-5 squares wide and tall.", MessageType.Info);
		EditorGUILayout.EndToggleGroup();
		customNameString = EditorGUILayout.TextField ("Custom File Name", customNameString, GUILayout.Width (350));
		EditorGUILayout.HelpBox ("This option is for advanced users who want to customize the name of mod files created. The standard option automatically removes spaces from the item name and uses that. If you aren't happy with that name being used as the File Name, use this to change it to a more preferable option.", MessageType.Info);
		if (itemNameFlat.Contains(" "))
		{
			EditorGUILayout.HelpBox ("Please note that any spaces added to the File Name input box will be automatically removed with spaces when processed.", MessageType.Warning);
		}
		KeepManifests = EditorGUILayout.Toggle ("Keep Manifests", KeepManifests);
		cleanProject = EditorGUILayout.Toggle ("Clean Project After Bundle", cleanProject);
		EditorGUILayout.HelpBox ("This option allows your project to be automatically set up for the next item, i.e removing any objects currently in scene and deleting the Pants_Assets file. Don't use this if you plan on making more pantss after this!", MessageType.Info);
		if (GUILayout.Button ("Create Mod Files", GUILayout.Width(350))) 
		{
			if (generateDats == false && customNameString == "" || itemName == "" && customNameString == "") {
				EditorGUILayout.HelpBox ("You must specify a name for the mod, if you choose not to generate .DAT files. Enter one in the Custom File Name slot.", MessageType.Warning);
			} else
			{
				//The following line allows for the user inputted Item Name to have a dual purpose system, in which it is used to name the file and the in-game item. It removes any spaces with underlines, to maintain styling with SDG assets and to prevent any errors. 
				//itemNameFlat = itemName.Replace (" ", "_");
				/* Because I opted for an ease of use system in which the armor slider is shown as a percentage of damage absorbed, I have to convert it to the method used in the dat files. Basically, in the dat files it uses a multiplier like 0.55, which would multiply any incoming damage by 0.55.
			 * This multiplier would essentially negate 45% of the damage. In order to make it easier to understand for inexperienced modders, I simply have to subtract
			 * the float value I created earlier out of 100 to get the percentage of armor negated, then divide by 100 to get a decimal. Oh, and since you're here and reading this, I just wanted to say congrats to you for either attempting to learn programming or wanting to see how this works. You're
			 * going to go far, kid. :)
			 * */
				Debug.Log (100 - armorLevel);
				actualArmor = (double)(100 - armorLevel) / 100;
				if (customNameString != "") {
					itemNameFlat = customNameString.Replace (" ", "_");
					Debug.Log ("cust name:" + itemNameFlat);
					Debug.Log (customNameString != "");
					Debug.Log (customNameString != null);
				} else {
					itemNameFlat = itemName.Replace (" ", "_");
				}
				//This is the new weird and wonky Unity 5+ asset bundling system. While I've never coded in Unity 4, this system definitely seems more complex. The function below gets the path to the Pants_Assets folder and marks it as a "unity3d" bundle. This removes the need for user based renaming.
				AssetImporter.GetAtPath ("Assets/Pants_Assets").SetAssetBundleNameAndVariant ("temp", "unity3d");
				//Prompt for the user to select what folder they would like to save to. The window is called "Select Bundle Folder", the path is the string it returns, and "Mod Folder" is the default name shown when the window opens.
				path = EditorUtility.SaveFolderPanel ("Select Bundle Folder", path, "Mod Folder");
				System.IO.Directory.CreateDirectory (Application.dataPath + "/" + itemNameFlat + "/test");
				Debug.Log (Application.dataPath + "/" + itemNameFlat + "/test");
				System.IO.Directory.Delete (Application.dataPath + "/" + itemNameFlat + "/test", true);
				//Checking whether or not the user enabled creation of .dat files
				if (generateDats) {
					using (StreamWriter sw = File.CreateText (Application.dataPath + "/" + itemNameFlat + "/" + itemNameFlat + ".dat")) {
						Debug.Log (Application.dataPath + "/" + itemNameFlat + "/" + itemNameFlat + ".dat");
						sw.WriteLine ("Type Pants");
						sw.WriteLine ("Rarity " + pantsRarity);
						sw.WriteLine ("Useable Clothing");
						sw.WriteLine ("ID " + itemID);
						//This is added to make it look more human, human written dat files feature some spaces between lines
						sw.WriteLine (" ");
						if (customItemSize) {
							sw.WriteLine ("Size_X " + itemWidth);
							sw.WriteLine ("Size_Y " + itemHeight);
							sw.WriteLine ("Size_Z 0.6");
						}
						if (!customItemSize) {
							//If the user did not toggle custom item sizes, the default 3x2 will be used instead.
							sw.WriteLine ("Size_X 3");
							sw.WriteLine ("Size_Y 2");
							sw.WriteLine ("Size_Z 0.6");
						}
						sw.WriteLine (" ");
						sw.WriteLine ("Width " + itemStorageWidth);
						sw.WriteLine ("Height " + itemStorageHeight);
						sw.WriteLine (" ");
						if (armor) {
							sw.WriteLine ("Armor " + actualArmor);
							sw.WriteLine (" ");
						}
						if (fireResistant)
							sw.WriteLine ("Proof_Fire");
						if (waterproof)
							sw.WriteLine ("Proof_Water");
						if (standardBlueprints) {
							sw.WriteLine (" ");
							sw.WriteLine ("Blueprints 2");
							sw.WriteLine ("Blueprint_0_Type Apparel");
							sw.WriteLine ("Blueprint_0_Supply_0_ID 1421");
							sw.WriteLine ("Blueprint_0_Product 66");
							sw.WriteLine ("Blueprint_0_Products 3");
							sw.WriteLine ("Blueprint_0_Build 32");
							sw.WriteLine ("Blueprint_1_Type Repair");
							sw.WriteLine ("Blueprint_1_Supplies 1");
							sw.WriteLine ("Blueprint_1_Supply_0_ID 66");
							sw.WriteLine ("Blueprint_1_Supply_0_Amount 3");
							sw.WriteLine ("Blueprint_1_Build 32");
						}
					}	
					using (StreamWriter sx = File.CreateText (Application.dataPath + "/" + itemNameFlat + "/English.dat")) {
						sx.WriteLine ("Name " + itemName);
						sx.WriteLine ("Description " + itemDescription);
					}
					/*
				 * NOTICE: THIS IS THE BROKEN AND LEGACY WAY I ATTEMPTED TO WRITE TO DAT FILES WITH. IT DOES NOT WORK AND IS ONLY BEING SAVED FOR THE SAKE OF POSTERITY. DO NOT ATTEMPT TO USE AS IT IS COMPLETELY BROKEN.
				 * Basically, what I'm doing here is checking what the user requested to be added to any .dat files and writing it. The dat file is generated automatically if it doesn't exist (which it shouldn't) when the first WriteAllText is ran. 
				System.IO.File.WriteAllText (path + "/English.dat", "Name " + itemName + "\n" + "Description " + itemDescription);
				System.IO.File.WriteAllText (path + "/" + itemNameFlat + ".dat", "\nType Pants \n Rarity " + pantsRarity + "\n Useable Clothing \n ID " + itemID + "\n");
				if (customItemSize) {
					System.IO.File.WriteAllText (path + "/" + itemNameFlat + ".dat", "Size_X " + itemWidth + "\n Size_Y " + itemHeight + "\n Size_Z 0.6 \n");
				}
				if (!customItemSize) {
					//If the user did not toggle custom item sizes, the default 3x2 will be used instead.
					System.IO.File.WriteAllText (path + "/" + itemNameFlat + ".dat", "Size_X 3\n Size_Y 2\n Size_Z 0.6 \n");
				}
				System.IO.File.WriteAllText (path + "/" + itemNameFlat + ".dat", "Width " + itemStorageWidth + "\n Height " + itemHeight);
				if (armor)
					System.IO.File.WriteAllText (path + "/" + itemNameFlat + ".dat", "\n Armor " + actualArmor);
				if (fireResistant)
					//In case you're wondering, \n is what's called an Escape Sequence. Basically, it allows you to escape the quotes and add extra stuff. I.e you can use \n to make a new line, \" to add a quote, and "\ " to add spaces when not in quotes.
					System.IO.File.WriteAllText (path + "/" + itemNameFlat + ".dat", "\n Proof_Fire");
				if (waterproof)
					System.IO.File.WriteAllText (path + "/" + itemNameFlat + ".dat", "\n Proof_Water");
				if (standardBlueprints) {
					System.IO.File.WriteAllText (path + "/" + itemNameFlat + ".dat", "\nBlueprints 2 \nBlueprint_0_Type Apparel\nBlueprint_0_Supply_0_ID 1421\nBlueprint_0_Product 66\nBlueprint_0_Products 3\nBlueprint_0_Build 32\nBlueprint_1_Type Repair\nBlueprint_1_Supplies 1\nBlueprint_1_Supply_0_ID 66\nBlueprint_1_Supply_0_Amount 3\nBlueprint_1_Build 32\n");
				}	
				*/
				}
				//Bundle the Unity3d file and place it in the same path folder mentioned early marked path. This isn't a reusable script as it doesn't carry over the parameters like a reusable void would, i.e getting the local class variable rather than bundleAssets(String path);
				bundleAssets ();
				UnityEditor.AssetDatabase.Refresh ();
				File.Delete (Application.dataPath + "/" + itemNameFlat + ".meta"); 
				File.Move (Application.dataPath + "/" + itemNameFlat + "/temp.unity3d", Application.dataPath + "/" + itemNameFlat + "/" + itemNameFlat + ".unity3d");
				File.Move (Application.dataPath + "/" + itemNameFlat, path + "/" + itemNameFlat);
				File.Delete (path + "/" + itemNameFlat + "/English.dat.meta"); 
				File.Delete (path + "/" + itemNameFlat + "/temp.unity3d.manifest"); 
				File.Delete (path + "/" + itemNameFlat + "/temp.unity3d.manifest.meta"); 
				File.Delete (path + "/" + itemNameFlat + "/temp.unity3d.meta"); 
				File.Delete (path + "/" + itemNameFlat + "/" + itemNameFlat); 
				File.Delete (path + "/" + itemNameFlat + "/" + itemNameFlat + ".manifest"); 
				File.Delete (path + "/" + itemNameFlat + "/" + itemNameFlat + ".meta"); 
				File.Delete (path + "/" + itemNameFlat + "/" + itemNameFlat + ".manifest.meta"); 
				File.Delete (path + "/" + itemNameFlat + "/" + itemNameFlat + ".dat.meta"); 

			}
		}
		if (generateDats == false && customNameString == "" || itemName == "" && customNameString == "") {
			EditorGUILayout.HelpBox ("You must specify a name for the mod, in either the Item Name slot or in the Custom Name slot if you have chosen not to generate .DATs", MessageType.Error);
		}
		devFold = EditorGUILayout.Foldout (devFold, "Dev Tests");
		if (devFold) {
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Clean Tags/Layers", GUILayout.Width (150))) 
			{
				RemoveTag ("Logic");
				RemoveTag ("Enemy");
				RemoveTag ("Viewmodel");
				RemoveTag ("Debris");
				RemoveTag ("Item");
				RemoveTag ("Resource");
				RemoveTag ("Large");
				RemoveTag ("Medium");
				RemoveTag ("Small");
				RemoveTag ("Sky");
				RemoveTag ("Environment");
				RemoveTag ("Water");
				RemoveTag ("Ground");
				RemoveTag ("Clip");
				RemoveTag ("Navmesh");
				RemoveTag ("Zombie");
				RemoveTag ("Agent");
				RemoveTag ("Ladder");
				RemoveTag ("Vehicle");
				RemoveTag ("Barricade");
				RemoveTag ("Structure");
				RemoveTag ("Tire");
				RemoveTag ("Trap");
				RemoveTag ("Ground2");
				RemoveTag ("Animal");
				RemoveTag ("UI");
				RemoveTag ("Border");
				RemoveLayer ("Logic");
				RemoveLayer ("Player");
				RemoveLayer ("Enemy");
				RemoveLayer ("Viewmodel");
				RemoveLayer ("Debris");
				RemoveLayer ("Item");
				RemoveLayer ("Resource");
				RemoveLayer ("Large");
				RemoveLayer ("Medium");
				RemoveLayer ("Small");
				RemoveLayer ("Sky");
				RemoveLayer ("Environment");
				RemoveLayer ("Ground");
				RemoveLayer ("Clip");
				RemoveLayer ("Navmesh");
				RemoveLayer ("Zombie");
				RemoveLayer ("Agent");
				RemoveLayer ("Ladder");
				RemoveLayer ("Vehicle");
				RemoveLayer ("Barricade");
				RemoveLayer ("Structure");
				RemoveLayer ("Tire");
				RemoveLayer ("Trap");
				RemoveLayer ("Ground2");
				RemoveLayer ("Animal");
				RemoveLayer ("UI");
				RemoveLayer ("Border");
				RemoveLayer ("Entity");

			}
			if (GUILayout.Button ("Create Tags/Layers", GUILayout.Width (150))) 
			{
				AddTag ("Logic");
				AddTag ("Enemy");
				AddTag ("Viewmodel");
				AddTag ("Debris");
				AddTag ("Item");
				AddTag ("Resource");
				AddTag ("Large");
				AddTag ("Medium");
				AddTag ("Small");
				AddTag ("Sky");
				AddTag ("Environment");
				AddTag ("Water");
				AddTag ("Ground");
				AddTag ("Clip");
				AddTag ("Navmesh");
				AddTag ("Zombie");
				AddTag ("Agent");
				AddTag ("Ladder");
				AddTag ("Vehicle");
				AddTag ("Barricade");
				AddTag ("Structure");
				AddTag ("Tire");
				AddTag ("Trap");
				AddTag ("Ground2");
				AddTag ("Animal");
				AddTag ("UI");
				AddTag ("Border");
				AddLayer ("Logic");
				AddLayer ("Player");
				AddLayer ("Enemy");
				AddLayer ("Viewmodel");
				AddLayer ("Debris");
				AddLayer ("Item");
				AddLayer ("Resource");
				AddLayer ("Large");
				AddLayer ("Medium");
				AddLayer ("Small");
				AddLayer ("Sky");
				AddLayer ("Environment");
				AddLayer ("Ground");
				AddLayer ("Clip");
				AddLayer ("Navmesh");
				AddLayer ("Zombie");
				AddLayer ("Agent");
				AddLayer ("Ladder");
				AddLayer ("Vehicle");
				AddLayer ("Barricade");
				AddLayer ("Structure");
				AddLayer ("Tire");
				AddLayer ("Trap");
				AddLayer ("Ground2");
				AddLayer ("Animal");
				AddLayer ("UI");
				AddLayer ("Border");
				AddLayer ("Entity");

			}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Reset Scene", GUILayout.Width (150))) {
				AssetDatabase.DeleteAsset ("Assets/Pants_Assets");
				GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>() ;
				foreach (GameObject go in allObjects)
					if (go.activeInHierarchy && (go.name != "Main Camera") && (go.name != "Directional Light")) {
						DestroyImmediate (go);
					}
				Debug.Log ("Forgot to tell you, this crashes the window. Just reopen it, and ignore any errors.");
				window.Close ();
				UnityEditor.AssetDatabase.Refresh ();
			}
			if (GUILayout.Button ("Reimport PKGs", GUILayout.Width (150))) {
				AssetDatabase.ImportPackage ("Assets/ClothingCreator/Pants.unitypackage", importdialog);
			}
			EditorGUILayout.EndHorizontal ();;
			importdialog = EditorGUILayout.ToggleLeft ("Import Dialogue", importdialog);
			displayErrors = EditorGUILayout.ToggleLeft ("Display Errors", displayErrors);
		}
	}
	/*OLD VERSION OF INSPECTOR UPDATE: IT'S REALLY BADLY MADE
	void OnInspectorUpdate()
	{
		//If the mod database exists, set both the Texture and InGame preview to what they currently look like. This basically refreshes previews every 10 frames or so. 
		if (AssetDatabase.IsValidFolder ("Assets/Pants_Assets")) {
			PantsTex = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Pants_Assets/Resources/Bundles/Items/Pants/Jeans_Work/Pants.png");
			PantsEm = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Pants_Assets/Resources/Bundles/Items/Pants/Jeans_Work/Emission.png");
			PantsPreview = AssetPreview.GetAssetPreview (PantsTex);

		} else {
			//If the mod data folder is missing/damaged, this will display the null icon for the PantsPreview instead rather than displaying nothing.
			PantsPreview = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/ClothingCreator/Icons/Null.png");
			//If I'm being honest, I'm unsure why this is here. It clearly won't work outside of the OnGui void in it's current state, but yet I kept it here anyways. Hmmm
			Debug.Log("ERROR");
		}
		/*This is my pitiful attempt to check whether or not the .unitypackage had imported without using those gay-ass IEnumerator thingies. It's such a pain in the ass to make a new void just for waiting, so I said fuck it and just made this.
		 * It checks whether or not the folder that should be present once the package has imported exists, and if it does it runs a one-time setup process to create the InGame preview window basis, as well as potentially the emission map when I
		 * add it. I'm using a boolean to check whether or not to run the void again, and once the void has been run once, it turns the boolean true so it can never run again.
		 * 
		if (AssetDatabase.IsValidFolder ("Assets/Pants_Assets")) {
			UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath ("Assets/Pants_Assets/Resources/Bundles/Items/Pants/Jeans_Work/Item.prefab", typeof(GameObject));
			if (!clone.activeInHierarchy) {
				clone = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
				clone.name = "New Cam";
				clone.transform.Rotate (-90, 0, 0);
			}
			/*MeshRenderer meshrend = clone.GetComponent<MeshRenderer> ();
			if (PantsEm != null) {
				meshrend.material.SetTexture ("_EmissionMap", PantsEm);
			}
			//Make a copy of the Main Camera and place it 2 units above the origin.
			if (!cam.activeInHierarchy) {
				cam = Instantiate (GameObject.FindWithTag ("MainCamera"), Vector3.zero, Quaternion.identity) as GameObject;
				cam.transform.Translate (0, 2, 0);
				cam.transform.Rotate (90, 90, 0);
			}
			//Set the InGame preview's render texture to a 210x128 pixel texture with a depth of 16. I actually have no fucking idea what the RenderTextureFormat does, but it was on the docs so I assumed it was probably necessary.
			if (PantsScenePreview.Equals (null)) {
				PantsScenePreview = new RenderTexture (210, 128, 16, RenderTextureFormat.ARGB32);
				//Without this, the texture will never be uploaded to the GPU. It's literally essential or else your texture won't work.
				PantsScenePreview.Create ();
			}
			//Registering the actual camera component of the Main Camera duplicate we made earlier.
			if (camera.Equals (null)) {
				camera = cam.GetComponent<Camera> ();
				camera.targetTexture = PantsScenePreview;
			}
			//Creating a new GameObject called Holder that will serve as the point of rotation for the Camera, so it can rotate around the pants properly.
			if (!holder.activeInHierarchy)
			{
			holder = new GameObject ("Holder");
			//Set the camera GameObject's parent to Holder, so it will follow the Holder's rotation.
			cam.transform.parent = holder.transform;
			//Guarantee that this void will never run again, as this will create an endless stream of new GameObjects (don't remove me or you will lag & or crash)
			}
		}
		//Updating the rotation of the Holder depending on the rotation setting on the Rotate Preview slider.
		holder.transform.eulerAngles = new Vector3 (camRotation, 0, 0);
		//Redraw the window, updating any previews about every 10 frames.
		Repaint ();
	}*/
	void OnInspectorUpdate()
	{
		if (PantsScenePreview == null) {
			PantsScenePreview = new RenderTexture (210, 128, 16, RenderTextureFormat.ARGB32);
			//Without this, the texture will never be uploaded to the GPU. It's literally essential or else your texture won't work.
			PantsScenePreview.Create ();
		}
		if (!displayErrors) {
			ClearLogConsole ();

		}
		if (AssetDatabase.IsValidFolder ("Assets/Pants_Assets")) {
			mat = AssetDatabase.LoadAssetAtPath<Material> ("Assets/Pants_Assets/Resources/Bundles/Items/Pants/Jeans_Work/Material.mat");
			PantsTex = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Pants_Assets/Resources/Bundles/Items/Pants/Jeans_Work/Pants.png");
			PantsEm = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Pants_Assets/Resources/Bundles/Items/Pants/Jeans_Work/Emission.png");
			PantsMet = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Pants_Assets/Resources/Bundles/Items/Pants/Jeans_Work/Metallic.png");
			mat.SetFloat ("_GlossMapScale", smoothness);
			mat.SetFloat ("_Glossiness", smoothness);
			mat.SetFloat ("_Metallic", 0);
			PantsPreview = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Pants_Assets/Resources/Bundles/Items/Pants/Jeans_Work/Pants.png");
			UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath ("Assets/Pants_Assets/Resources/Bundles/Items/Pants/Jeans_Work/Item.prefab", typeof(GameObject));
			if (GameObject.Find ("Pants") == null) {
				clone = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
				clone.name = "Pants";
				clone.transform.Rotate (-90, 0, 0);

			}
			if (GameObject.Find ("View Cam") == null) {
				cam = Instantiate (GameObject.FindWithTag ("MainCamera"), Vector3.zero, Quaternion.identity) as GameObject;
				cam.transform.Translate (0, 2, 0);
				cam.transform.Rotate (90, 90, 0);
				cam.name = "View Cam";
				camera = cam.GetComponent<Camera> ();
				camera.targetTexture = PantsScenePreview;
				camera.orthographic = true;
				camera.orthographicSize = 0.6f;
			}
			else if (GameObject.Find("View Cam"))
			{
				cam = GameObject.Find ("View Cam");
				camera = cam.GetComponent<Camera> ();
				camera.targetTexture = PantsScenePreview;
			}
			if (GameObject.Find ("Holder") != null) {
				holder = GameObject.Find ("Holder");
				cam.transform.parent = holder.transform;
			}

		}
		if (GameObject.Find ("Holder") == null) {
			holder = new GameObject ("Holder");
			cam.transform.parent = holder.transform;
		}
		else if (!AssetDatabase.IsValidFolder ("Assets/Pants_Assets")){
			PantsPreview = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/ClothingCreator/Icons/Null.png");
		}
		if (AssetDatabase.IsValidFolder ("Assets/Pants_Assets") && hasRan == false) 
		{
			mat.SetColor ("_EmissionColor", Color.black);
			mat.SetTexture ("_MetallicGlossMap", null);
			hasRan = true;
		}
		if (holder != null) {
			holder.transform.eulerAngles = new Vector3 (camRotation, 0, 0);
		} else 
		{
			if (GameObject.Find ("Holder")) {
				holder = GameObject.Find ("Holder");
			} else
				window.EndWindows ();
		}
		//Redraw the window, updating any previews about every 10 frames.
		Repaint ();

	}
	private void bundleAssets()
	{
		//This was Nelson Sexton's way of bundling legacy, I couldn't figure out how to use it without the Selection feature (which was necessary to automate it) so I just went with the newer version in the end.
		/*Debug.Log (selection.Length);
		if(path.Length > 0)
		{
			#pragma warning disable 0618

			//if(!BuildPipeline.BuildAssetBundle(AssetDatabase.LoadMainAssetAtPath("Assets/Pants_Assets",typeof(UnityEngine.Object)), AssetDatabase.LoadAllAssetsAtPath("Assets/Pants_Assets"), path, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows));
			BuildPipeline.BuildAssetBundle(selection[0], selection, path, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
			#pragma warning restore 0618

			Debug.Log("Successfully built bundle!");
		}*/
		BuildPipeline.BuildAssetBundles (Application.dataPath + "/" + itemNameFlat, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows64);
		if (cleanProject) {
			AssetDatabase.DeleteAsset ("Assets/Pants_Assets");
			DestroyImmediate (cam);
			DestroyImmediate (holder);
			DestroyImmediate (clone);
			UnityEditor.AssetDatabase.Refresh();

		}
	}
	/// <summary>
	/// Adds the tag.
	/// </summary>
	/// <returns><c>true</c>, if tag was added, <c>false</c> otherwise.</returns>
	/// <param name="tagName">Tag name.</param>
	public static bool AddTag (string tagName) {
		// Open tag manager
		SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);
		// Tags Property
		SerializedProperty tagsProp = tagManager.FindProperty ("tags");
		if (tagsProp.arraySize >= maxTags) {
			Debug.Log("No more tags can be added to the Tags property. You have " + tagsProp.arraySize + " tags");
			return false;
		}
		// if not found, add it
		if (!PropertyExists (tagsProp, 0, tagsProp.arraySize, tagName)) {
			int index = tagsProp.arraySize;
			// Insert new array element
			tagsProp.InsertArrayElementAtIndex(index);
			SerializedProperty sp = tagsProp.GetArrayElementAtIndex(index);
			// Set array element to tagName
			sp.stringValue = tagName;
			Debug.Log ("Tag: " + tagName + " has been added");
			// Save settings
			tagManager.ApplyModifiedProperties ();
			return true;
		} else {
			//Debug.Log ("Tag: " + tagName + " already exists");
		}
		return false;
	}
	/// <summary>
	/// Removes the tag.
	/// </summary>
	/// <returns><c>true</c>, if tag was removed, <c>false</c> otherwise.</returns>
	/// <param name="tagName">Tag name.</param>
	public static bool RemoveTag(string tagName) {

		// Open tag manager
		SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);

		// Tags Property
		SerializedProperty tagsProp = tagManager.FindProperty ("tags");

		if (PropertyExists (tagsProp, 0, tagsProp.arraySize, tagName)) {
			SerializedProperty sp;

			for(int i = 0, j = tagsProp.arraySize; i < j; i++) {

				sp = tagsProp.GetArrayElementAtIndex (i);
				if(sp.stringValue == tagName) {
					tagsProp.DeleteArrayElementAtIndex (i);
					Debug.Log("Tag: " + tagName + " has been removed");
					// Save settings
					tagManager.ApplyModifiedProperties ();
					return true;
				}

			}
		}

		return false;

	}
	/*
	 * Not my code, just consolidating this so I can minimize the number of files needed. Going to attempt to have a script download the unitypackage file rather than have it on hand 
	 * to completely eliminate the need for more than one file.
	 * */
	/// <summary>
	/// Checks to see if tag exists.
	/// </summary>
	/// <returns><c>true</c>, if tag exists, <c>false</c> otherwise.</returns>
	/// <param name="tagName">Tag name.</param>
	public static bool TagExists(string tagName) {
		// Open tag manager
		SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);

		// Layers Property
		SerializedProperty tagsProp = tagManager.FindProperty ("tags");
		return PropertyExists (tagsProp, 0, maxTags, tagName);
	}
	/// <summary>
	/// Adds the layer.
	/// </summary>
	/// <returns><c>true</c>, if layer was added, <c>false</c> otherwise.</returns>
	/// <param name="layerName">Layer name.</param>
	public static bool AddLayer (string layerName) {
		// Open tag manager
		SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);
		// Layers Property
		SerializedProperty layersProp = tagManager.FindProperty ("layers");
		if (!PropertyExists (layersProp, 0, maxLayers, layerName)) {
			SerializedProperty sp;
			// Start at layer 9th index -> 8 (zero based) => first 8 reserved for unity / greyed out
			for (int i = 8, j = maxLayers; i < j; i++) {
				sp = layersProp.GetArrayElementAtIndex (i);
				if (sp.stringValue == "") {
					// Assign string value to layer
					sp.stringValue = layerName;
					Debug.Log ("Layer: " + layerName + " has been added");
					// Save settings
					tagManager.ApplyModifiedProperties ();
					return true;
				}
				if (i == j)
					Debug.Log ("All allowed layers have been filled");
			}
		} else {
			//Debug.Log ("Layer: " + layerName + " already exists");
		}
		return false;
	}

	/// <summary>
	/// Removes the layer.
	/// </summary>
	/// <returns><c>true</c>, if layer was removed, <c>false</c> otherwise.</returns>
	/// <param name="layerName">Layer name.</param>
	public static bool RemoveLayer(string layerName) {

		// Open tag manager
		SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);

		// Tags Property
		SerializedProperty layersProp = tagManager.FindProperty ("layers");

		if (PropertyExists (layersProp, 0, layersProp.arraySize, layerName)) {
			SerializedProperty sp;

			for(int i = 0, j = layersProp.arraySize; i < j; i++) {

				sp = layersProp.GetArrayElementAtIndex (i);

				if(sp.stringValue == layerName) {
					sp.stringValue = "";
					Debug.Log ("Layer: " + layerName + " has been removed");
					// Save settings
					tagManager.ApplyModifiedProperties ();
					return true;
				}

			}
		}

		return false;

	}
	/// <summary>
	/// Checks to see if layer exists.
	/// </summary>
	/// <returns><c>true</c>, if layer exists, <c>false</c> otherwise.</returns>
	/// <param name="layerName">Layer name.</param>
	public static bool LayerExists(string layerName) {
		// Open tag manager
		SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);

		// Layers Property
		SerializedProperty layersProp = tagManager.FindProperty ("layers");
		return PropertyExists (layersProp, 0, maxLayers, layerName);
	}
	/// <summary>
	/// Checks if the value exists in the property.
	/// </summary>
	/// <returns><c>true</c>, if exists was propertyed, <c>false</c> otherwise.</returns>
	/// <param name="property">Property.</param>
	/// <param name="start">Start.</param>
	/// <param name="end">End.</param>
	/// <param name="value">Value.</param>
	private static bool PropertyExists(SerializedProperty property, int start, int end, string value) {
		for (int i = start; i < end; i++) {
			SerializedProperty t = property.GetArrayElementAtIndex (i);
			if (t.stringValue.Equals (value)) {
				return true;
			}
		}
		return false;
	}
	public static void ClearLogConsole() {
		Assembly assembly = Assembly.GetAssembly (typeof(SceneView));
		Type logEntries = assembly.GetType ("UnityEditorInternal.LogEntries");
		MethodInfo clearConsoleMethod = logEntries.GetMethod ("Clear");
		clearConsoleMethod.Invoke (new object (), null);
	}
	void OnDestroy()
	{
		GameObject pants = GameObject.Find ("Pants");
		DestroyImmediate (pants);
		isOpen = false;
	}

}
#endif