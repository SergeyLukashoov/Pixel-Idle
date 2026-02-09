using System;
using System.Collections.Generic;
using Game.Enemy;

namespace Core.Config.Wave {
	[Serializable]
	public class SpawnPointInfo {
		public EnemyType _type;
		public List<int> _pointId;
	}

	[Serializable]
	public class SpawnEnemyInfo {
		public EnemyType _type;
		public int       _count;
	}

	public class WaveInfo {
		public int                  _waveId;
		public List<int>            _spawnPointsIds  = new List<int>();
		public List<SpawnPointInfo> _spawnPointInfos = new List<SpawnPointInfo>();
		public List<SpawnEnemyInfo> _spawnEnemyInfos = new List<SpawnEnemyInfo>();
		public float                _timeToSpawnEnemy;
		public float                _pauseToNextWave;
		public bool                 _nextWaveAfterDefeat;
	}
}
