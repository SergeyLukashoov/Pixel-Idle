using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Cards;
using Game.Player;
using UnityEngine;

namespace Core.Config.Chests {
	[Serializable]
	public class ChestData {
		public string           ChestId;
		public List<RewardInfo> Rewards;
		public List<CardType>   CardsList;
	}

	[Serializable]
	public class RewardInfo {
		public PlayerCurrencyType Type;
		public int                Count;
	}

	[Serializable, CreateAssetMenu(fileName = "ChestsDB", menuName = "DB/Create ChestsDB", order = 58)]
	public class ChestsDB : ScriptableObject {
		[SerializeField] private List<ChestData> _chestsData = new List<ChestData>();

		public void AddChestData(ChestData data) {
			_chestsData.Add(data);
		}

		public ChestData GetChestData(string id) {
			var chestsData = _chestsData.FirstOrDefault(x => x.ChestId == id);
			if (chestsData != null)
				return chestsData;

			Debug.LogError("Wrong Chest Id!!!");
			return null;
		}

		public void Clear() {
			_chestsData.Clear();
		}
	}
}
