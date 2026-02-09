using System;
using System.Collections.Generic;
using Config.Player;
using Core.Config;
using Core.Controllers.Save;
using Game.Cards;

namespace Game.Player {
	public interface IPlayer {
		public PlayerCards      Cards                       { get; }
		public PlayerTowers     Towers                      { get; }
		public PlayerMines      Mines                       { get; }
		public PlayerGrandChest GrandChests                 { get; }
		public PlayerFlags      Flags                       { get; }
		public PlayerStats      Stats                       { get; }
		public PlayerCurrency   Currency                    { get; }
		public PlayerConfig     Config                      { get; }
		public List<CardInfo>   CollectedCards              { get; }
		public Action           OnChangeCoinsCount          { get; set; }
		public Action           OnChangeCurrencyCount       { get; set; }
		public Action           OnAddCard                   { get; set; }
		public Action<float>    OnChangeGameBoardCoinsCount { get; set; }
		public int              UnlockedTowersCount         { get; }
		public List<TowerInfo>  UnlockedTowers              { get; }
		public CardType GetCardForShow(bool needRemove = true);
		public void Initialize(List<PlayerStatsData> data);
		public void Load(SaveData data);
		public void ClearSave();
		public List<StatInfo> GetAttackStats();
		public List<StatInfo> GetDefenceStats();
		public List<StatInfo> GetLootStats();
		public void UnlockStat(PlayerStatType type);
		public void UpgradeStats(PlayerStatType type, PlayerStatConfig config);
		public bool IsHaveCoins(int coinsCount);
		public void ChangeCoinsCount(float delta, string source = "");
		public void AddCard(CardType type);
		public void ApplayCard(CardType type);
		public void RemoveCard(CardType type);
		public CardInfo GetCardLevelUpped();
		public void UnlockCardSlot(int slotId);
		public List<CardType> GetActiveCards();
		public bool IsCardMaxLevelUp(CardType type);
		public CardInfo GetCard(CardType type);
		public CardInfo GetActiveCardByType(CardType type);
		public Dictionary<string, int> GetStatsLevelUp();
		public Dictionary<string, float> GetStatsValue();
		public Dictionary<string, string> GetActiveCardsInfo();
		public void UnlockTower(int id);
		public bool IsTowerUnlocked(int id);
		public TowerInfo GetTowerInfo(int id);
		public void SetTowerMaxCompleteWave(int id, int waveId);
		public void UnlockGrandChest(string id);
		public GrandChestInfo GetGrandChestInfo(string id);
		public bool IsGrandChestUnlocked(string id);
		public void UnlockMine(int id, int levelUp);
		public bool IsMineUnlocked(int id);
		public MineInfo GetMineInfo(int id);
		public void ChangeCurrency(PlayerCurrencyType type, int delta, string source = "", bool isBought = false);
		public void CollectChest(string chestId);
		public bool IsChestCollected(string chestId);
		public bool IsHaveCurrency(PlayerCurrencyType type, int count);
		public void IncreaseTowerPlayedCount(int towerId);
		public int GetTowerPlayedCount(int towerId);
		public void ClearNeedShowCards();
	}
}
