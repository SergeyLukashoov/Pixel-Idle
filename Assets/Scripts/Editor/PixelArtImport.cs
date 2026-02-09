using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor {
	/// <summary>
	/// Copies generated pixel art PNGs from a folder (e.g. Cursor assets) into Assets/Images_PixelArt
	/// so they can be used or later swapped into Assets/Images. Menu: Tools / Pixel Art / Import from folder...
	/// </summary>
	public static class PixelArtImport {
		// Filename -> subpath under Assets/Images_PixelArt (Ui, Map, etc.)
		private static readonly Dictionary<string, string> FileToSubpath = new Dictionary<string, string> {
			{ "window_bg.png", "Ui/Windows" },
			{ "circle_back.png", "Ui" },
			{ "circle_back_1.png", "Ui" },
			{ "circle_back_2.png", "Ui" },
			{ "stats_hp_progress_back.png", "Ui" },
			{ "stats_hp_progress_fill.png", "Ui" },
			{ "wave_progress_back.png", "Ui" },
			{ "wave_progress_fill.png", "Ui" },
			{ "enemy_hp_progress_back.png", "Ui" },
			{ "enemy_hp_progress_fill.png", "Ui" },
			{ "chest_icon.png", "Ui" },
			{ "grand_chest_icon.png", "Ui" },
			{ "plus.png", "Ui" },
			{ "tooltip_bg.png", "Ui" },
			{ "cost_bg.png", "Ui" },
			{ "toggle_back.png", "Ui" },
			{ "core_HUD_stats_bg.png", "Ui" },
			{ "tutor_target.png", "Ui" },
			{ "wait_icon.png", "Ui" },
			{ "wind_icon_lock.png", "Ui" },
			{ "wind_icon_plus.png", "Ui" },
			{ "wind_icon_placement.png", "Ui/Windows" },
			{ "Grass.png", "Map/TerrainZone2" },
			{ "Rock.png", "Map/TerrainZone2" },
			{ "Stone1.png", "Map/TerrainZone2" },
			{ "Stone2.png", "Map/TerrainZone2" },
			{ "Tree1.png", "Map/TerrainZone2" },
			{ "Tree2.png", "Map/TerrainZone2" },
			{ "Garbage.png", "Map/TerrainZone2" },
			{ "Chest.png", "Map/Chest" },
			{ "red_crystal.png", "Map/Mine" },
			{ "green_crystal.png", "Map/Mine" },
			{ "bubble.png", "Map/Mine" },
			{ "mine_progress_bg.png", "Map/Mine" },
			{ "mine_progress_red.png", "Map/Mine" },
			{ "mine_progress_green.png", "Map/Mine" },
			{ "mine_progress_blue.png", "Map/Mine" },
			{ "progress_back.png", "Map/BigChest" },
			{ "progress_fill.png", "Map/BigChest" },
			{ "Path.png", "Map" },
			{ "Path_dot.png", "Map" },
			{ "sea_river_tr.png", "Map/Water" },
		};

		private const string PixelArtRoot = "Assets/Images_PixelArt";

		[MenuItem("Tools/Pixel Art/Import from folder...")]
		public static void ImportFromFolder() {
			string source = EditorUtility.OpenFolderPanel("Select folder with generated PNGs", "", "");
			if (string.IsNullOrEmpty(source)) return;

			var dir = new DirectoryInfo(source);
			if (!dir.Exists) {
				EditorUtility.DisplayDialog("Import Pixel Art", "Folder not found.", "OK");
				return;
			}

			int copied = 0;
			foreach (FileInfo fi in dir.GetFiles("*.png", SearchOption.TopDirectoryOnly)) {
				string name = fi.Name;
				if (!FileToSubpath.TryGetValue(name, out string subpath))
					subpath = "Ui"; // default
				string destDir = Path.Combine(PixelArtRoot, subpath);
				string destPath = Path.Combine(destDir, name);
				if (!Directory.Exists(destDir))
					Directory.CreateDirectory(destDir);
				File.Copy(fi.FullName, Path.Combine(Application.dataPath, "..", destPath), overwrite: true);
				copied++;
			}

			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Import Pixel Art", $"Copied {copied} PNG(s) to {PixelArtRoot}. Rebuild atlases: Tools / Sprite Atlas / Build Atlases.", "OK");
		}

		[MenuItem("Tools/Pixel Art/Use Pixel Art in project (replace Images)")]
		public static void ReplaceImagesWithPixelArt() {
			if (!EditorUtility.DisplayDialog("Replace Images", "Copy all files from Images_PixelArt into Images (overwrite)? Backup Images first if needed.", "Copy", "Cancel"))
				return;
			string pixelRoot = Path.Combine(Application.dataPath, "Images_PixelArt");
			string imagesRoot = Path.Combine(Application.dataPath, "Images");
			if (!Directory.Exists(pixelRoot)) {
				EditorUtility.DisplayDialog("Replace", "Images_PixelArt not found.", "OK");
				return;
			}
			CopyDirectory(pixelRoot, imagesRoot);
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Replace", "Done. Run Tools / Sprite Atlas / Build Atlases.", "OK");
		}

		private static void CopyDirectory(string src, string dst) {
			foreach (string path in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories)) {
				string rel = path.Substring(src.Length + 1);
				string dest = Path.Combine(dst, rel);
				string destDir = Path.GetDirectoryName(dest);
				if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
				File.Copy(path, dest, overwrite: true);
			}
		}
	}
}
