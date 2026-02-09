using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Utils {
	public static class GoogleTableImporter {
		private const string URLFormat = @"https://docs.google.com/spreadsheets/d/{0}/gviz/tq?tqx=out:csv&sheet={1}";

		public static async void DownloadConfig(string docId, string pageName, Action<string, string[]> onComplete) {
			var url       = string.Format(URLFormat, docId, pageName);
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
			
			onComplete?.Invoke(pageName, rawTable);
		}
	}
}
