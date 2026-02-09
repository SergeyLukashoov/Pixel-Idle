using System;
using System.Collections.Generic;
using System.Linq;
using Config.Player;
using UnityEngine;

namespace Game.Player {
	[Serializable]
	public class StatInfo {
		public PlayerStatType _type;
		public float          _value;
		public int            _levelUp;
		public bool           _unlocked;

		public StatInfo(PlayerStatType type, float value, int levelUp, bool unlocked) {
			_type     = type;
			_value    = value;
			_levelUp  = levelUp;
			_unlocked = unlocked;
		}
	}

	[Serializable]
	public class PlayerStats {
		public List<StatInfo> _statInfos = new List<StatInfo>();

		public void CreateDefault(PlayerConfig config) {
			var typesVal = (PlayerStatType[]) Enum.GetValues(typeof(PlayerStatType));
			foreach (var type in typesVal) {
				var statInfo = config.GetStatInfoByType(type);
				var unlocked = !statInfo.LockedOnStart;

				_statInfos.Add(new StatInfo(type, statInfo.Value, 0, unlocked));
			}
		}

		public void CreateInGameStats(PlayerStats stats) {
			_statInfos = new List<StatInfo>();
			for (var i = 0; i < stats._statInfos.Count; ++i) {
				var info = stats._statInfos[i];
				_statInfos.Add(new StatInfo(info._type, info._value, 0, info._unlocked));
			}
		}

		public StatInfo GetStatByType(PlayerStatType type) {
			var statInfo = _statInfos.FirstOrDefault(x => x._type == type);
			if (statInfo == null) {
				Debug.LogError("Stat type not found");
				return null;
			}

			return statInfo;
		}

		public List<StatInfo> GetAttackStats() {
			var attackStats = new List<StatInfo>();
			for (var i = 0; i < _statInfos.Count; ++i) {
				if (_statInfos[i]._type == PlayerStatType.Damage || _statInfos[i]._type == PlayerStatType.ShotsPerSec ||
				    _statInfos[i]._type == PlayerStatType.CritChance || _statInfos[i]._type == PlayerStatType.CritMult ||
				    _statInfos[i]._type == PlayerStatType.Range || _statInfos[i]._type == PlayerStatType.AddDmgPerRange ||
				    _statInfos[i]._type == PlayerStatType.SuperCritChance || _statInfos[i]._type == PlayerStatType.SuperCritMult) {
					attackStats.Add(_statInfos[i]);
				}
			}

			return attackStats;
		}

		public List<StatInfo> GetDefenceStats() {
			var defenceStats = new List<StatInfo>();
			for (var i = 0; i < _statInfos.Count; ++i) {
				if (_statInfos[i]._type == PlayerStatType.Health || _statInfos[i]._type == PlayerStatType.HealthRegen ||
				    _statInfos[i]._type == PlayerStatType.PhysicalResist || _statInfos[i]._type == PlayerStatType.AbsoluteResist) {
					defenceStats.Add(_statInfos[i]);
				}
			}

			return defenceStats;
		}

		public List<StatInfo> GetLootStats() {
			var lootStats = new List<StatInfo>();
			for (var i = 0; i < _statInfos.Count; ++i) {
				if (_statInfos[i]._type == PlayerStatType.CoreCurrencyMult || _statInfos[i]._type == PlayerStatType.CoreCurrencyForWave ||
				    _statInfos[i]._type == PlayerStatType.MetaCurrencyMult || _statInfos[i]._type == PlayerStatType.MetaCurrencyForWave) {
					lootStats.Add(_statInfos[i]);
				}
			}

			return lootStats;
		}

		public void UnlockStat(PlayerStatType type) {
			var neededStat = _statInfos.FirstOrDefault(x => x._type == type);
			if (neededStat == null) {
				Debug.LogError($"Stat {type} not found!");
				return;
			}

			neededStat._unlocked = true;
		}

		public void UpgradeStats(PlayerStatType type, PlayerStatConfig config) {
			var neededStat = _statInfos.FirstOrDefault(x => x._type == type);
			if (neededStat == null) {
				Debug.LogError($"Stat {type} not found!");
				return;
			}

			neededStat._value += config.UpgradeStep;
			neededStat._levelUp++;
		}
	}
}
