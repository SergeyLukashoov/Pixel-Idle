using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Tools.Editor {
	public class EditorTools : MonoBehaviour {
		[MenuItem("Scenes/Loader")]
		private static void OpenLoaderScene() {
			EditorSceneManager.OpenScene("Assets/Scenes/Loader.unity");
		}

		[MenuItem("Scenes/Menu")]
		private static void OpenLoaderMenu() {
			EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");
		}

		[MenuItem("Scenes/Game")]
		private static void OpenLoaderGame() {
			EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
		}
		
		#region Build_Android_Default

		[MenuItem("Tools/Builder/Android/Default/Build Prod")]
		private static void BuildAndroidProdHardcore() {
			var depList = Utility.GetDefaultDependencies();

			Builder.Builder.BuildAndroidProd(depList.ToArray());
		}

		[MenuItem("Tools/Builder/Android/Default/Build Cheater")]
		private static void BuildAndroidCheaterHardcore() {
			var depList = Utility.GetDefaultDependencies(true);
			Builder.Builder.BuildAndroidCheater(depList.ToArray());
		}
		
		[MenuItem("Tools/Builder/Android/Default/Build For Creatives")]
		private static void BuildAndroidCreativesHardcore() {
			var depList = Utility.GetDefaultDependencies(true);
			depList.Add("REC_VIDEO");
			
			Builder.Builder.BuildAndroidCheater(depList.ToArray());
		}
		#endregion
		
		#region Build_Android_Azur

		[MenuItem("Tools/Builder/Android/Azur/Build Prod")]
		private static void BuildAndroidProdAzur() {
			var depList = Utility.GetDefaultDependencies();
			depList.Add("APPMETRICA");

			Builder.Builder.BuildAndroidProd(depList.ToArray());
		}

		[MenuItem("Tools/Builder/Android/Azur/Build Cheater")]
		private static void BuildAndroidCheaterAzur() {
			var depList = Utility.GetDefaultDependencies(true);
			depList.Add("APPMETRICA");
			
			Builder.Builder.BuildAndroidCheater(depList.ToArray());
		}
		
		[MenuItem("Tools/Builder/Android/Azur/Build No Ads")]
		private static void BuildAndroidCheaterAzurNoAds() {
			var depList = Utility.GetDefaultDependencies(true);
			depList.Add("APPMETRICA");
			depList.Add("FAKE_ADS");

			Builder.Builder.BuildAndroidCheater(depList.ToArray());
		}

		#endregion

		#region Build_Android_Voodoo

		[MenuItem("Tools/Builder/Android/Voodoo/Build Prod")]
		private static void BuildAndroidProdVoodoo() {
			var depList = Utility.GetDefaultDependencies();
			depList.Add("APPSFLYER");
			depList.Add("GAMEANALYTICS");
			depList.Add("APPMETRICA");
			depList.Add("VOODOO");
			
			Builder.Builder.BuildAndroidProd(depList.ToArray());
		}

		[MenuItem("Tools/Builder/Android/Voodoo/Build Cheater")]
		private static void BuildAndroidCheaterVoodoo() {
			var depList = Utility.GetDefaultDependencies(true);
			depList.Add("APPSFLYER");
			depList.Add("GAMEANALYTICS");
			depList.Add("APPMETRICA");
			depList.Add("VOODOO");
			
			Builder.Builder.BuildAndroidCheater(depList.ToArray());
		}

		#endregion

    #region Build_Android_Boombit

		[MenuItem("Tools/Builder/Android/Boombit/Build Prod")]
		private static void BuildAndroidProdBoombit() {
			var depList = Utility.GetDefaultDependencies();
			depList.Add("APPSFLYER");
			depList.Add("GAMEANALYTICS");
			depList.Add("BOOMBIT");
			
			Builder.Builder.BuildAndroidProd(depList.ToArray());
		}

		[MenuItem("Tools/Builder/Android/Boombit/Build Cheater")]
		private static void BuildAndroidCheaterBoombit() {
			var depList = Utility.GetDefaultDependencies(true);
			depList.Add("APPSFLYER");
			depList.Add("GAMEANALYTICS");
			depList.Add("BOOMBIT");
			
			Builder.Builder.BuildAndroidCheater(depList.ToArray());
		}

		#endregion

		#region Tools

		[MenuItem("Tools/Load Game Text")]
		private static void LoadGameText() {
			GameTextLoader.DownloadGameText();
		}

		[MenuItem("Tools/Clear Save")]
		private static void ClearSave() {
			var savePath = Path.Combine(Application.persistentDataPath + "/profile.txt");
#if UNITY_EDITOR
			if (File.Exists(savePath)) {
				File.Delete(savePath);
			}
#endif
		}
		
		[MenuItem("Tools/Sprite Atlas/Build Atlases")]
		public static void BuildSpriteAtlases() {
			var imagesPath    = Application.dataPath + "/Images";
			var imagesDirInfo = new DirectoryInfo(imagesPath);
			var allImagesDir  = imagesDirInfo.GetDirectories();
			for (var i = 0; i < allImagesDir.Length; ++i) {
				BuildAtlas(allImagesDir[i]);
			}

			Debug.Log("Build atlases complete!");
		}

		private static void BuildAtlas(DirectoryInfo info) {
			var spriteAtlas = Resources.Load($"Atlases/{info.Name}") as SpriteAtlas;
			if (spriteAtlas) {
				//Clear atlas
				var allPackables = spriteAtlas.GetPackables();
				spriteAtlas.Remove(allPackables);

				//Add items to atlas
				var objList = new List<Object>();
				var path    = $"Assets/Images/{info.Name}";
				var files   = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
				for (var i = 0; i < files.Length; ++i) {
					var asset = AssetDatabase.LoadAssetAtPath(files[i], typeof(Object));
					objList.Add(asset);
				}

				spriteAtlas.Add(objList.ToArray());
			}
		}

		#endregion
	}
}
