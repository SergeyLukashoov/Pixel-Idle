using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.GameTextSpace {
	[Serializable]
	public class LocalizedText {
		public string _id;
		public string _ru;
		public string _en;
	}

	[CreateAssetMenu(fileName = "GameText", menuName = "DB/Create GameText")]
	public class GameText : ScriptableObject {
		[SerializeField][HideInInspector] private List<LocalizedText> _gameText = new List<LocalizedText>();

		public SystemLanguage CurrentLanguage { private get; set; }

		public void ClearTexts() {
			_gameText.Clear();
		}

		public void AddText(List<string> text) {
			var id = text[0];
			
			if (_gameText.FirstOrDefault(x => x._id == id) != null) {
				Debug.LogError($"ID = {id} is already added!");
				return;
			}

			var localizedText = new LocalizedText {
				_id = id,
				_ru = text[1],
				_en = text[2],
			};

			_gameText.Add(localizedText);
		}

		public string GetGameText(string id) {
			var locText = _gameText.FirstOrDefault(x => x._id == id);
			if (locText == null) {
				Debug.LogError("Wrong text id!");
				return id;
			}
			
			return locText._en;
			
			/*if (CurrentLanguage == SystemLanguage.Russian)
				return locText._ru;
			else {
				return locText._en;
			}*/
		}
	}
}
