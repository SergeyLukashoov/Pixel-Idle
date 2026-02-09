using System;

namespace Game.Player {
	[Serializable]
	public class PlayerCurrency {
		public float CoinsValue;
		public int   RedCrystal;
		public int   GreenCrystal;

		public PlayerCurrency(float coinsCount) {
			CoinsValue = coinsCount;
		}
	}
}
