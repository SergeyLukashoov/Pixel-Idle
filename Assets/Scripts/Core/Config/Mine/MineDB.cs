using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Config.Mine {
	[Serializable]
	public class MineProduceInfo {
		public int LevelUp;
		public int ProduceCount;
		public int ProduceTime;
		public int MaxCapacity;
	}
	
	[Serializable, CreateAssetMenu(fileName = "MineDB", menuName = "DB/Create MineDB", order = 56)]
	public class MineDB : ScriptableObject {
		[SerializeField] private List<MineProduceInfo> _mineInfos = new List<MineProduceInfo>();

		public MineProduceInfo GetMineProduceInfo(int levelUp) {
			return _mineInfos.FirstOrDefault(x => x.LevelUp == levelUp);
		}
	}
}
