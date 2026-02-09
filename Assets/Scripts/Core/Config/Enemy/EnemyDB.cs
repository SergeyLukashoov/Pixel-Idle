using System.Collections.Generic;
using System.Linq;
using Core.Config;

namespace Game.Enemy {
	public class EnemyDB {
		private List<EnemyInfo> _enemyInfos;

		public EnemyDB(List<NPCStatsData> data) {
			_enemyInfos = new List<EnemyInfo>();
			for (var i = 0; i < data.Count; ++i) {
				var enemyInfo = new EnemyInfo(data[i]);
				_enemyInfos.Add(enemyInfo);
			}
		}
		
		public EnemyInfo GetEnemyByType(EnemyType type) {
			return _enemyInfos.FirstOrDefault(x => x.Type == type);
		}
	}
}
