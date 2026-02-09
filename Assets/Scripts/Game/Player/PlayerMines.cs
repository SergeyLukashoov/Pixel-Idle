using System;
using System.Collections.Generic;

namespace Game.Player {
	[Serializable]
	public class MineInfo {
		public int   _id;
		public int   _levelUp;
		public int   _producedCount;
		public float _produceTime;
	}

	[Serializable]
	public class PlayerMines {
		public List<MineInfo> _minesInfos = new List<MineInfo>();
	}
}
