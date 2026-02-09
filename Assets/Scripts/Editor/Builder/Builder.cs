using System.IO;
using System.Linq;
using System.Net;
using Core.Config;
using Core.Utils;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor.Builder {
	public static class Builder {
		public static void BuildAndroidProd(string[] defines) {
#if UNITY_EDITOR
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
			BuildAndroid("prod");
#endif
		}

		public static void BuildAndroidCheater(string[] defines) {
#if UNITY_EDITOR
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
			BuildAndroid("cheater");
#endif
		}

		private static void BuildAndroid(string postfix) {
#if UNITY_EDITOR
			GameTextLoader.DownloadGameText();
			EditorTools.BuildSpriteAtlases();

			var buildConfig = Resources.Load<BuilderConfig>("Config/BuildConfig");
			if (!buildConfig) {
				Debug.LogError("Can't load build config!");
				return;
			}

			var version               = $"{buildConfig.GlobalVersion}.{buildConfig.BundleVersion}_{postfix}";
			var buildBundleIdentifier = $"TD_prototype_ver_{version}";
			
			var extentions = "aab";
			if (!buildConfig.NeedBuildAppBundle)
				extentions = "apk";

			var folder = Directory.CreateDirectory($"builds/android/{buildConfig.GlobalVersion}.{buildConfig.BundleVersion}");
			var buildPath = $"builds/android/{folder.Name}/{buildBundleIdentifier}.{extentions}";

			PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
			PlayerSettings.applicationIdentifier = "pixel.archer";

			PlayerSettings.Android.useCustomKeystore = true;
			PlayerSettings.bundleVersion             = buildConfig.GlobalVersion;
			PlayerSettings.Android.bundleVersionCode = buildConfig.BundleVersion;
			PlayerSettings.Android.keystoreName      = buildConfig.KeystoreName;
			PlayerSettings.Android.keyaliasName      = buildConfig.AliasName;
			PlayerSettings.Android.keystorePass              = buildConfig.KeystorePass;
			PlayerSettings.Android.keyaliasPass              = buildConfig.AliasPass;

			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
			EditorUserBuildSettings.androidCreateSymbols         = AndroidCreateSymbols.Debugging;
			EditorUserBuildSettings.androidBuildSystem           = AndroidBuildSystem.Gradle;
			EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
			EditorUserBuildSettings.buildAppBundle               = buildConfig.NeedBuildAppBundle;

			buildConfig.IncBundleVersion();
			EditorUtility.SetDirty(buildConfig);

			BuildPipeline.BuildPlayer(GetEnabledScenePaths(), buildPath, BuildTarget.Android, BuildOptions.None);
			Debug.Log($"<color=green>[Builder]</color> Build version: {version} complete!");
#endif
		}

		private static string[] GetEnabledScenePaths() {
			return EditorBuildSettings.scenes.Select(e => e.path).ToArray();
		}
	}
}
