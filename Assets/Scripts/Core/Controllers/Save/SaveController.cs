using System;
using System.IO;
using System.Xml;
using Game.Player;
using UnityEngine;

namespace Core.Controllers.Save {
	public class SaveController : ISaveController {
		private IPlayer _player;

		public void Initialize(IPlayer player) {
			_player = player;
		}

		public void Save() {
			var saveData = new SaveData {
				PlayerStats       = _player.Stats,
				PlayerCurrency    = _player.Currency,
				PlayerFlags       = _player.Flags,
				PlayerCards       = _player.Cards,
				PlayerTowers      = _player.Towers,
				PlayerMines       = _player.Mines,
				PlayerGrandChests = _player.GrandChests,
			};

			var saveString = JsonUtility.ToJson(saveData, true);
#if UNITY_EDITOR
			File.WriteAllText(Application.persistentDataPath + "/profile.txt", saveString);
#else
      PlayerPrefs.SetString("profile_txt", saveString);
      PlayerPrefs.Save();
#endif
		}

		public void Load() {
			var savePath = Path.Combine(Application.persistentDataPath + "/profile.txt");
			Debug.Log($"Save path = {savePath}");
#if UNITY_EDITOR
			if (File.Exists(savePath)) {
				var saveStr  = File.ReadAllText(savePath);
				var saveData = JsonUtility.FromJson<SaveData>(saveStr);
				_player.Load(saveData);
			}
			else {
				Debug.LogError($"Save file {savePath} not found!");
			}
#else
      try
      {
          var saveStr = PlayerPrefs.GetString("profile_txt");
          var saveData = JsonUtility.FromJson<SaveData>(saveStr);
          _player.Load(saveData);
      }
      catch (XmlException exception)
      {
          Debug.LogError("playerprefs xml broken!!! " + exception.Message);
      }
#endif
		}
	}
}
