using System;
using System.Collections.Generic;

namespace Game.Player {
	[Serializable]
	public class TowerInfo {
		public int _towerId;
		public int _maxWaveId;
		public int _rangeStepId;
		public int _playedCount;
	}

	[Serializable]
	public class PlayerTowers {
		public List<TowerInfo> _unlockedTowers = new List<TowerInfo>();
	}
}
