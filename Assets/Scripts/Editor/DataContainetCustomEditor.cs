using System.Collections.Generic;
using Core.Config;
using Core.Config.Chests;
using Core.Config.Towers;
using Core.Utils;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DataContainer), true)]
public class DataContainerCustomEditor : Editor {
	bool                     foldout = true;
	Dictionary<string, bool> pagesToggles;

	public override void OnInspectorGUI() {
		var container = (DataContainer) target;

		foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "Import settings");
		if (foldout)
			DrawGUI(container);

		EditorGUILayout.EndFoldoutHeaderGroup();

		EditorGUILayout.Space(16);

		base.OnInspectorGUI();
	}

	void DrawGUI(DataContainer container) {
		EditorGUILayout.BeginVertical("box");

		#region Draw controll buttons

		EditorGUILayout.BeginVertical();

		if (GUILayout.Button("Import Character Data")) {
			GoogleTableImporter.DownloadConfig(container.GetDocId, "CharacterParameters", OnComplete);
		}

		if (GUILayout.Button("Import NPC Data")) {
			GoogleTableImporter.DownloadConfig(container.GetDocId, "NPCParameters", OnComplete);
		}

		if (GUILayout.Button("Import Wave Data")) {
			ParseWaveSettings(container);
		}
		
		if (GUILayout.Button("Import Towers Data")) {
			GoogleTableImporter.DownloadConfig(container.GetDocId, "TowersParameters", OnComplete);
		}
		
		if (GUILayout.Button("Import Chests Data")) {
			GoogleTableImporter.DownloadConfig(container.GetDocId, "ChestsParameters", OnComplete);
		}

		EditorGUILayout.EndVertical();

		#endregion

		EditorGUILayout.EndVertical();
	}

	private void ParseWaveSettings(DataContainer container) {
		container.ClearWaveData();
		
		var terrainCount = 5;
		for (var i = 1; i <= terrainCount; ++i) {
			var pageName = $"WavesTerrein{i}";
			GoogleTableImporter.DownloadConfig(container.GetDocId, pageName, OnComplete);
		}
	}
	
	private void OnComplete(string pageName, string[] result) {
		var container = (DataContainer) target;
		if (pageName == "CharacterParameters") {
			container.ParseCharacterData(result);
		}
		else if (pageName == "NPCParameters") {
			container.ParseNpcData(result);
		}
		else if (pageName.Contains("WavesTerrein")) {
			var maskStr = "WavesTerrein";
			var idStr   = pageName.Substring(maskStr.Length, pageName.Length - maskStr.Length);
			
			if (int.TryParse(idStr, out var wavePageId))
				container.ParseWaveData(result, wavePageId);
		}
		else if (pageName == "TowersParameters") {
			var towersDB = Resources.Load<TowersDB>("Config/Towers/TowersDB");
			container.ParseTowersData(result, towersDB);
			EditorUtility.SetDirty(towersDB);
		}
		else if (pageName == "ChestsParameters") {
			var chestsDB = Resources.Load<ChestsDB>("Config/Chests/ChestsDB");
			container.ParseChestsData(result, chestsDB);
			EditorUtility.SetDirty(chestsDB);
		}

		EditorUtility.SetDirty(container);
	}
}
