using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Game.Player;
using UnityEngine;

namespace Config.Player {
	public class PlayerConfig {
		private List<PlayerStatConfig> _statInfos;

		public PlayerConfig(List<PlayerStatsData> data) {
			Create(data);
		}

		private void Create(List<PlayerStatsData> data) {
			_statInfos = new List<PlayerStatConfig>();
			for (var i = 0; i < data.Count; ++i) {
				var statConfig = new PlayerStatConfig(data[i]);
				_statInfos.Add(statConfig);
			}
		}

		public PlayerStatConfig GetStatInfoByType(PlayerStatType type) {
			var statInfo = _statInfos.FirstOrDefault(x => x.Type == type);
			if (statInfo == null) {
				Debug.LogError($"Stat {type} type not found");
				return null;
			}

			return statInfo;
		}
	}
}
