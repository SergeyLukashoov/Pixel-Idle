using System;
using Game.Player;

namespace Core.Controllers.Save {
	[Serializable]
	public class SaveData {
		public PlayerStats      PlayerStats;
		public PlayerCurrency   PlayerCurrency;
		public PlayerFlags      PlayerFlags;
		public PlayerCards      PlayerCards;
		public PlayerTowers     PlayerTowers;
		public PlayerMines      PlayerMines;
		public PlayerGrandChest PlayerGrandChests;
	}
}
