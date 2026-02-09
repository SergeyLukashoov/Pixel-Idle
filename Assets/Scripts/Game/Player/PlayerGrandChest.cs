using System;
using System.Collections.Generic;

namespace Game.Player {
	[Serializable]
	public class GrandChestInfo {
		public string _id;
		public float  _currentProduceTime;
		public float  _produceTime;
		public int    _resourceLeft;
	}

	[Serializable]
	public class PlayerGrandChest {
		public List<GrandChestInfo> _grandChestInfos = new List<GrandChestInfo>();
	}
}
