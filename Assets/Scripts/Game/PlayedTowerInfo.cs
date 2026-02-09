namespace Game {
	public class PlayedTowerInfo {
		public int   Id                    { get; set; } = -1;
		public int   TerrainId             { get; set; }
		public int   StartWaveId           { get; set; }
		public float CurrencyMultiplier    { get; set; }
		public float EnemyHealthMultiplier { get; set; }
		public float EnemyDamageMultiplier { get; set; }
		public bool  IsCardDrop            { get; set; }
	}
}
