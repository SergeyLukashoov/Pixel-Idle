using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Config.Chests;
using Core.Config.Towers;
using Core.Config.Wave;
using UnityEngine;
using Core.Utils;
using Game.Cards;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.Experimental.Audio;

namespace Core.Config {
	[Serializable]
	public class PlayerStatsData {
		public string SkillId;

		public float BaseValue;
		public float MaxValue;
		public float ValueStep;

#if HARDCORE
		public List<UpgradePriceInfo> UpgradeInfos;
#else
		public List<int> UpgradeInfos;
#endif

		public bool LockedOnStart;
		public int  ShowAfterTower = -1;
		public int  UnlockPrice;
	}

	[Serializable]
	public class NPCStatsData {
		public string NPCID;
		public int    AttackRange;

#if HARDCORE
		public float                  BaseHealth;
		public List<UpgradePriceInfo> UpgradeHealthInfo;

		public float                  BaseDamage;
		public List<UpgradePriceInfo> UpgradeDamageInfo;

		public float BaseSpeed;
		public float SpeedInc;
		public float SpeedMax;
		public float BaseCoreCurrency;
		public float CoreCurrencyInc;
		public float BaseMetaCurrency;
		public float MetaCurrencyInc;
#else
		public List<float> UpgradeHealthInfo;
		public List<float> UpgradeDamageInfo;
		public List<float> UpgradeSpeedInfo;
		public List<float> BaseCoreCurrency;
		public List<float> BaseMetaCurrency;
#endif
		public float TimeToAttack;
	}

	[Serializable]
	public class WaveDataByTerrain {
		public int            TerrainId;
		public List<WaveData> WaveDatas;
	}

	[Serializable]
	public class WaveData {
		public int WaveId;

		public float TimeToSpawn;
		public float DelayBeforeWave;

		public bool NextWaveAfterDefeat;

		public List<SpawnEnemyInfo> EnemiesInfo;
		public List<SpawnPointInfo> EnemiesByPointsIds;
		public List<int>            PointsIds;
	}

	[CreateAssetMenu(fileName = "GoogleTableDataContainer", menuName = "DB/Create GoogleTableDataContainer")]
	public class DataContainer : ScriptableObject {
		private NumberFormatInfo NumberFormatInfo = new NumberFormatInfo{ CurrencyDecimalSeparator = ","};
		
#if HARDCORE
		private string DocumentID = "1lqj4la85I0OaCiMQ3ujGQEeK9qt_psQtXRJZpBNZ3Ds";
#else
		private string DocumentID = "1_QvkRfFM5acZzM1zP60kdUNUcZiT-fTDcos5A9G4B6M";
#endif

		public List<PlayerStatsData>   CharacterParameters;
		public List<NPCStatsData>      NPCParameters;
		public List<WaveData>          WaveParametersTower1;
		public List<WaveData>          WaveParametersTower2;
		public List<WaveDataByTerrain> WaveParametersByTerrain = new List<WaveDataByTerrain>();

		public string GetDocId => DocumentID;

		public WaveDataByTerrain GetWaveDataByTerrain(int terrainId) {
			var waveDataBuTerrain = WaveParametersByTerrain.FirstOrDefault(x => x.TerrainId == terrainId);
			if (waveDataBuTerrain == null) {
				Debug.LogError("Wrong terrain id!");
				return null;
			}

			return waveDataBuTerrain;
		}

		#region Parse Character Data

		public void ParseCharacterData(string[] data) {
			CharacterParameters.Clear();

			for (var i = 1; i < data.Length; ++i) {
				ParseCharacterDataRow(data[i]);
			}

			Debug.Log("Parse Characters Data success!");
		}

		private void ParseCharacterDataRow(string row) {
			var data      = Utility.FixData(row);
			var statsData = new PlayerStatsData();

			statsData.SkillId = data[0];

			if (!float.TryParse(data[1], NumberStyles.Any, NumberFormatInfo, out statsData.BaseValue))
				Debug.LogError($"Skill ID = {statsData.SkillId}; Error parsing BaseValue = {data[1]}");

			if (!float.TryParse(data[2], NumberStyles.Any, NumberFormatInfo, out statsData.MaxValue))
				Debug.LogError($"Skill ID = {statsData.SkillId}; Error parsing MaxValue = {data[2]}");

			if (!float.TryParse(data[3], NumberStyles.Any, NumberFormatInfo, out statsData.ValueStep))
				Debug.LogError($"Skill ID = {statsData.SkillId}; Error parsing ValueStep = {data[3]}");

#if HARDCORE
			statsData.UpgradeInfos = ParseUpgradePriceInfos(data[4]);
#else
			statsData.UpgradeInfos = new List<int>();
			var strArr = data[4].Split(' ');
			for (var i = 0; i < strArr.Length; ++i) {
				if (!int.TryParse(strArr[i], out var intVal)) {
					Debug.LogError("Error parsing stats upgradePrice");
					continue;
				}

				statsData.UpgradeInfos.Add(intVal);
			}
#endif

			if (!bool.TryParse(data[5], out statsData.LockedOnStart))
				Debug.LogError($"Skill ID = {statsData.SkillId}; Error parsing LockedOnStart = {data[5]}");

			if (!int.TryParse(data[6], out statsData.ShowAfterTower))
				Debug.LogError($"Skill ID = {statsData.SkillId}; Error parsing ShowAfterTower = {data[6]}");

			if (!int.TryParse(data[7], out statsData.UnlockPrice))
				Debug.LogError($"Skill ID = {statsData.SkillId}; Error parsing UnlockPrice = {data[7]}");

			CharacterParameters.Add(statsData);
		}

		#endregion

		#region Parse NPC data

		public void ParseNpcData(string[] data) {
			NPCParameters.Clear();

			for (var i = 1; i < data.Length; ++i) {
				ParseNpcDataRow(data[i]);
			}

			Debug.Log("Parse NPC Data success!");
		}

		private void ParseNpcDataRow(string row) {
			var data    = Utility.FixData(row);
			var npcData = new NPCStatsData();

			npcData.NPCID = data[0];

#if HARDCORE
			if (!float.TryParse(data[1], NumberStyles.Any, NumberFormatInfo, out npcData.BaseHealth))
				Debug.LogError($"Error parsing BaseHealth = {data[1]}");

			npcData.UpgradeHealthInfo = ParseUpgradePriceInfos(data[2]);

			if (!float.TryParse(data[3], NumberStyles.Any, NumberFormatInfo, out npcData.BaseSpeed))
				Debug.LogError($"Error parsing BaseSpeed = {data[3]}");

			if (!float.TryParse(data[4], NumberStyles.Any, NumberFormatInfo, out npcData.SpeedInc))
				Debug.LogError($"Error parsing SpeedInc = {data[4]}");

			if (!float.TryParse(data[5], NumberStyles.Any, NumberFormatInfo, out npcData.SpeedMax))
				Debug.LogError($"Error parsing SpeedMax = {data[5]}");

			if (!float.TryParse(data[6], NumberStyles.Any, NumberFormatInfo, out npcData.BaseDamage))
				Debug.LogError($"Error parsing BaseDamage = {data[6]}");

			npcData.UpgradeDamageInfo = ParseUpgradePriceInfos(data[7]);

			if (!float.TryParse(data[8], NumberStyles.Any, NumberFormatInfo, out npcData.BaseCoreCurrency))
				Debug.LogError($"Error parsing BaseCoreCurrency = {data[8]}");

			if (!float.TryParse(data[9], NumberStyles.Any, NumberFormatInfo, out npcData.CoreCurrencyInc))
				Debug.LogError($"Error parsing CoreCurrencyInc = {data[9]}");

			if (!float.TryParse(data[10], NumberStyles.Any, NumberFormatInfo, out npcData.BaseMetaCurrency))
				Debug.LogError($"Error parsing BaseMetaCurrency = {data[10]}");

			if (!float.TryParse(data[11], NumberStyles.Any, NumberFormatInfo, out npcData.MetaCurrencyInc))
				Debug.LogError($"Error parsing MetaCurrencyInc = {data[11]}");

			if (!float.TryParse(data[12], NumberStyles.Any, NumberFormatInfo, out npcData.TimeToAttack))
				Debug.LogError($"Error parsing TimeToAttack = {data[12]}");
#else
			npcData.UpgradeHealthInfo = ParseHardBalanceValues(data[1]);
			npcData.UpgradeDamageInfo = ParseHardBalanceValues(data[2]);
			npcData.UpgradeSpeedInfo = ParseHardBalanceValues(data[3]);
			npcData.BaseCoreCurrency = ParseHardBalanceValues(data[4]);
			npcData.BaseMetaCurrency = ParseHardBalanceValues(data[5]);
			//npcData.TimeToAttack = ParseHardBalanceValues(data[6]);
#endif
			NPCParameters.Add(npcData);
		}

		#endregion

		#region Parse Waves Data

		public void ClearWaveData() {
			WaveParametersTower1.Clear();
			WaveParametersTower2.Clear();
			WaveParametersByTerrain.Clear();
		}

		public void ParseWaveData(string[] data, int waveTerrainId) {
			var waveDataByTerrain = new WaveDataByTerrain();
			waveDataByTerrain.TerrainId = waveTerrainId;
			waveDataByTerrain.WaveDatas = new List<WaveData>();
			
			for (var i = 1; i < data.Length; ++i) {
				var waveData = ParseWaveDataRow(data[i], waveTerrainId);
				
				if (waveTerrainId == 1) {
					if (waveData.WaveId <= 40)
						WaveParametersTower1.Add(waveData);
					else if (waveData.WaveId <= 80)
						WaveParametersTower2.Add(waveData);
					else
						waveDataByTerrain.WaveDatas.Add(waveData);
				}
				else {
					waveDataByTerrain.WaveDatas.Add(waveData);
				}
			}

			if (waveDataByTerrain.WaveDatas.Count > 0)
				WaveParametersByTerrain.Add(waveDataByTerrain);
			
			Debug.Log("Parse Wave Data success!");
		}

		private WaveData ParseWaveDataRow(string row, int waveTerrainId) {
			var data     = Utility.FixData(row);
			var waveData = new WaveData();

			if (!int.TryParse(data[0], out waveData.WaveId))
				Debug.LogError($"Error parsing WaveId = {data[0]}");

			if (!float.TryParse(data[1], NumberStyles.Any, NumberFormatInfo, out waveData.TimeToSpawn))
				Debug.LogError($"Error parsing TimeToSpawn = {data[1]}");

			if (!float.TryParse(data[2], NumberStyles.Any, NumberFormatInfo, out waveData.DelayBeforeWave))
				Debug.LogError($"Error parsing DelayBeforeWave = {data[2]}");

			waveData.EnemiesInfo = new List<SpawnEnemyInfo>();
			var spawnEnemiesArr = data[3].Split(' ');
			for (var i = 0; i < spawnEnemiesArr.Length; ++i) {
				var infoArr = spawnEnemiesArr[i].Split('|');
				if (infoArr.Length != 2) {
					Debug.LogError($"Wrong enemies info format: {spawnEnemiesArr[i]}. Must be a|b! Wave id = {waveData.WaveId}");
					continue;
				}

				var info = new SpawnEnemyInfo();
				Enum.TryParse(infoArr[0], out info._type);
				int.TryParse(infoArr[1], out info._count);
				waveData.EnemiesInfo.Add(info);
			}

			waveData.PointsIds = new List<int>();
			if (data[4] != "0") {
				var pointsArr = data[4].Split(' ');
				for (var i = 0; i < pointsArr.Length; ++i) {
					int.TryParse(pointsArr[i], out var value);
					waveData.PointsIds.Add(value);
				}
			}

			waveData.EnemiesByPointsIds = new List<SpawnPointInfo>();
			if (data[5] != "0") {
				var spawnPointInfoArr = data[5].Split(' ');
				for (var i = 0; i < spawnPointInfoArr.Length; ++i) {
					var infoArr = spawnPointInfoArr[i].Split('|');
					if (infoArr.Length != 2) {
						Debug.LogError($"Wrong points info format: {spawnPointInfoArr[i]}. Must be a|b");
						continue;
					}

					var info = new SpawnPointInfo();
					Enum.TryParse(infoArr[0], out info._type);

					info._pointId = new List<int>();
					var pointsIdArr = infoArr[1].Split(',');
					for (var j = 0; j < pointsIdArr.Length; ++j) {
						int.TryParse(pointsIdArr[j], out var value);
						info._pointId.Add(value);
					}

					waveData.EnemiesByPointsIds.Add(info);
				}
			}

			if (!bool.TryParse(data[6], out waveData.NextWaveAfterDefeat))
				Debug.LogError($"Error parsing NextWaveAfterDefeat = {data[6]}");

			return waveData;
		}

		#endregion

		#region Parse Towers Data

		public void ParseTowersData(string[] data, TowersDB towersDB) {
			towersDB.Clear();

			for (var i = 1; i < data.Length; ++i) {
				var towerData = ParseTowerDataRow(data[i]);
				towersDB.AddTowerData(towerData);
			}

			Debug.Log("Parse Towers Data success!");
		}

		private TowerData ParseTowerDataRow(string row) {
			var data      = Utility.FixData(row);
			var towerData = new TowerData();

			if (!int.TryParse(data[0], out towerData.Id))
				Debug.LogError($"Error parsing Id = {data[0]}");

			if (!int.TryParse(data[1], out towerData.TerrainId))
				Debug.LogError($"Error parsing TerrainId = {data[1]}");

			if (!int.TryParse(data[2], out towerData.UnlockPrice))
				Debug.LogError($"Error parsing UnlockPrice = {data[2]}");

			if (!int.TryParse(data[3], out towerData.StartWaveId))
				Debug.LogError($"Error parsing StartWaveId = {data[3]}");

			if (!float.TryParse(data[4], NumberStyles.Any, NumberFormatInfo, out towerData.CurrencyMult))
				Debug.LogError($"Error parsing CurrencyMult = {data[4]}");

			if (!float.TryParse(data[5], NumberStyles.Any, NumberFormatInfo, out towerData.EnemiesHealthMult))
				Debug.LogError($"Error parsing EnemiesHealthMult = {data[5]}");

			if (!float.TryParse(data[6], NumberStyles.Any, NumberFormatInfo, out towerData.EnemiesDamageMult))
				Debug.LogError($"Error parsing EnemiesDamageMult = {data[6]}");

			return towerData;
		}

		#endregion

		#region ChestsParameters

		public void ParseChestsData(string[] data, ChestsDB chestsDB) {
			chestsDB.Clear();

			for (var i = 1; i < data.Length; ++i) {
				var chestData = ParseChestDataRow(data[i]);
				chestsDB.AddChestData(chestData);
			}

			Debug.Log("Parse Chests Data success!");
		}

		private ChestData ParseChestDataRow(string row) {
			var data      = Utility.FixData(row);
			var chestData = new ChestData();

			chestData.ChestId = data[0];

			chestData.Rewards = new List<RewardInfo>();
			var rewardsArr = data[1].Split(' ');
			for (var i = 0; i < rewardsArr.Length; ++i) {
				var infoArr = rewardsArr[i].Split('|');
				if (infoArr.Length != 2) {
					Debug.LogError($"Wrong rewards info format: {rewardsArr[i]}. Must be a|b! Chest id = {chestData.ChestId}");
					continue;
				}

				var info = new RewardInfo();
				Enum.TryParse(infoArr[0], out info.Type);
				int.TryParse(infoArr[1], out info.Count);
				chestData.Rewards.Add(info);
			}

			chestData.CardsList = new List<CardType>();
			if (data.Count > 2) {
				var cardsArr = data[2].Split(' ');
				for (var i = 0; i < cardsArr.Length; ++i) {
					Enum.TryParse(cardsArr[i], out CardType type);
					chestData.CardsList.Add(type);
				}
			}

			return chestData;
		}

		#endregion

		#region Tools

		private List<float> ParseHardBalanceValues(string dataStr) {
			var valList = new List<float>();
			var strArr  = dataStr.Split(' ');
			for (var i = 0; i < strArr.Length; ++i) {
				if (!float.TryParse(strArr[i], NumberStyles.Any, NumberFormatInfo, out var floatVal)) {
					Debug.LogError("Error parsing value!");
					continue;
				}

				valList.Add(floatVal);
			}

			return valList;
		}

		private List<UpgradePriceInfo> ParseUpgradePriceInfos(string dataStr) {
			var upgradeInfosList = new List<UpgradePriceInfo>();
			var upgradeInfos     = Regex.Split(dataStr, " ");
			for (var i = 0; i < upgradeInfos.Length; ++i) {
				var splitStr = upgradeInfos[i].Split('|');
				if (splitStr.Length != 2) {
					Debug.LogError($"Wrong upgrade info format: {upgradeInfos[i]}. Must be a|b");
					continue;
				}

				var info = new UpgradePriceInfo();
				if (!float.TryParse(splitStr[0], NumberStyles.Any, NumberFormatInfo, out info._price))
					Debug.LogError($"Error parsing UpgradePrice = {splitStr[0]}");

				if (!int.TryParse(splitStr[1], out info._level))
					Debug.LogError($"Error parsing UpgradeLevel = {splitStr[1]}");

				upgradeInfosList.Add(info);
			}

			return upgradeInfosList;
		}

		#endregion
	}
}
