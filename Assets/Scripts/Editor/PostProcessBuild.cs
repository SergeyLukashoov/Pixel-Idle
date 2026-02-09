using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class PostProcessBuild {
#if UNITY_IOS
	//must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
	[PostProcessBuildAttribute(45)]
	public static void PostProcessFixFB(BuildTarget buildTarget, string path) {
		if (buildTarget == BuildTarget.iOS) {
			var podFile = File.ReadAllText(path + "/Podfile");
			podFile = podFile.Replace("use_frameworks! :linkage => :static", "use_frameworks!");
			File.WriteAllText(path + "/Podfile", podFile);
		}
	}
	
	[PostProcessBuild]
	public static void OnPostProcessBuildIOS(BuildTarget buildTarget, string path) {
		if (buildTarget != BuildTarget.iOS)
			return;

		Debug.Log("[Post Processing Build]: Start");

		//PlistDocument-------------
		var infoPlist = new PlistDocument();
		infoPlist.ReadFromFile(path + "/Info.plist");
		infoPlist.root.SetString("NSUserTrackingUsageDescription", 
			"Your data will be used to deliver personalized ads to you. App needs to access IDFA to analyze game data and restore player profile in case it is lost or broken.");
		infoPlist.root.SetString("AppTrackingTransparency", "yes");
		infoPlist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

		var exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
		if (infoPlist.root.values.ContainsKey(exitsOnSuspendKey)) {
			infoPlist.root.values.Remove(exitsOnSuspendKey);
			Debug.Log("[Post Processing Build]: Remove UIApplicationExitsOnSuspend");
		}

		var nsAppTransportSecurity = infoPlist.root.CreateDict("NSAppTransportSecurity");
		nsAppTransportSecurity.SetBoolean("NSAllowsArbitraryLoads", true);

		infoPlist.WriteToFile(path + "/Info.plist");
		//--------------------------

		//PBXProject-------------
		var projPath = PBXProject.GetPBXProjectPath(path);
		var proj     = new PBXProject();
		proj.ReadFromString(File.ReadAllText(projPath));

		var target = proj.GetUnityFrameworkTargetGuid();
		proj.AddFrameworkToProject(target, "libz.tbd", false);
		proj.AddFrameworkToProject(target, "AppTrackingTransparency.framework", false);

		proj.SetBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

		File.WriteAllText(projPath, proj.WriteToString());
		Debug.Log("[Post Processing Build]: Success");
	}
#endif
}
