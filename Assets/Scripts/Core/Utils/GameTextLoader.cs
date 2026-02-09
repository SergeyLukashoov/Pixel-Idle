using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.GameTextSpace;
using UnityEditor;
using UnityEngine;

namespace Core.Utils {
	public static class GameTextLoader {
		private const string URLFormat = @"https://docs.google.com/spreadsheets/d/{0}/gviz/tq?tqx=out:csv";
		
		public static async void DownloadGameText() {
			var url       = string.Format(URLFormat, "1qFCAqtgS449Sdm_DcjxZ_0elJn9tV7uWTSEQfeqDAYU");
			var webClient = new WebClient();

			Task<string> request;
			try {
				request = webClient.DownloadStringTaskAsync(url);
			}
			catch (WebException) {
				Debug.LogError($"Bad URL '{url}'");
				throw;
			}

			while (!request.IsCompleted) {
				await Task.Delay(100);
			}

			var rawTable = Regex.Split(request.Result, "\r\n|\r|\n");
			request.Dispose();
			
			ParseGameText(rawTable);
			Debug.Log("GameText downloaded!");
		}

		private static void ParseGameText(string[] data) {
			var gameText = Resources.Load<GameText>("GameText/GameText");
			if (!gameText) {
				Debug.LogError("Can't load GameText object!");
				return;
			}
			
			gameText.ClearTexts();
			
			for (var i = 1; i < data.Length; ++i) {
				var fixedStr = Utility.FixData(data[i]);
				gameText.AddText(fixedStr);
			}
			
#if UNITY_EDITOR
			EditorUtility.SetDirty(gameText);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
#endif
		}
	}
}
