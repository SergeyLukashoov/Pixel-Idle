using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Config.Towers {
	[Serializable]
	public class TowerData {
		public int   Id;
		public int   TerrainId;
		public int   UnlockPrice;
		public int   StartWaveId;
		public float CurrencyMult;
		public float EnemiesHealthMult;
		public float EnemiesDamageMult;
	}

	[Serializable, CreateAssetMenu(fileName = "TowersDB", menuName = "DB/Create TowersDB", order = 57)]
	public class TowersDB : ScriptableObject {
		[SerializeField] private List<TowerData> _towersData = new List<TowerData>();

		public void AddTowerData(TowerData data) {
			_towersData.Add(data);
		}
		
		public TowerData GetTowerData(int id) {
			var towerData = _towersData.FirstOrDefault(x => x.Id == id);
			if (towerData != null)
				return towerData;
			
			Debug.LogError("Wrong Tower Id!!!");
			return null;
		}

		public void Clear() {
			_towersData.Clear();
		}
	}
}
