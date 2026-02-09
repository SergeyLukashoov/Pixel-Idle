using System.Collections.Generic;
using Core.Controllers;

namespace Core.Config.Wave {
	public class WaveDB {
		List<WaveInfo> _waveInfos = new List<WaveInfo>();

		public void Initialize(List<WaveData> data) {
			for (var i = 0; i < data.Count; ++i) {
				var waveInfo = new WaveInfo {
					_waveId = data[i].WaveId,
					_timeToSpawnEnemy = data[i].TimeToSpawn,
					_pauseToNextWave = data[i].DelayBeforeWave,
					_spawnEnemyInfos = data[i].EnemiesInfo,
					_spawnPointsIds = data[i].PointsIds,
					_spawnPointInfos = data[i].EnemiesByPointsIds,
					_nextWaveAfterDefeat = data[i].NextWaveAfterDefeat
				};

				_waveInfos.Add(waveInfo);
			}
		}

		public WaveInfo GetWaveById(int idx) {
			var waveId = idx;
			if (waveId >= _waveInfos.Count)
				return null;

			return _waveInfos[waveId];
		}

		public bool IsHaveWave(int idx) {
			var waveId = idx;
			if (waveId >= _waveInfos.Count)
				return false;

			return true;
		}
	}
}
