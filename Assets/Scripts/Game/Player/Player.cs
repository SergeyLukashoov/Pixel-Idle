using System;
using System.Collections.Generic;
using System.Linq;
using Config.Player;
using Core.Config;
using Core.Controllers;
using Core.Controllers.Save;
using Game.Boards;
using Game.Cards;
using UnityEngine;

namespace Game.Player {
	public class Player : IPlayer {
		private PlayerFlags      _playerFlags;
		private PlayerStats      _playerStats;
		private PlayerConfig     _playerConfig;
		private PlayerCurrency   _playerCurrency;
		private PlayerCards      _playerCards;
		private PlayerTowers     _playerTowers;
		private PlayerMines      _playerMines;
		private PlayerGrandChest _playerGrandChests;

		private List<CardType> _neededShowCards = new List<CardType>();

		public PlayerGrandChest GrandChests                 => _playerGrandChests;
		public PlayerMines      Mines                       => _playerMines;
		public PlayerTowers     Towers                      => _playerTowers;
		public PlayerCards      Cards                       => _playerCards;
		public PlayerFlags      Flags                       => _playerFlags;
		public PlayerStats      Stats                       => _playerStats;
		public PlayerCurrency   Currency                    => _playerCurrency;
		public PlayerConfig     Config                      => _playerConfig;
		public List<CardInfo>   CollectedCards              => _playerCards._collectedCards;
		public int              UnlockedTowersCount         => _playerTowers._unlockedTowers.Count;
		public List<TowerInfo>  UnlockedTowers              => _playerTowers._unlockedTowers;
		public Action           OnChangeCoinsCount          { get; set; }
		public Action           OnChangeCurrencyCount       { get; set; }
		public Action           OnAddCard                   { get; set; }
		public Action<float>    OnChangeGameBoardCoinsCount { get; set; }

		public CardInfo GetCardLevelUpped() {
			for (var i = 0; i < CollectedCards.Count; ++i) {
				var cardLvlUp = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(CollectedCards[i]._count);
				if (cardLvlUp > 1)
					return CollectedCards[i];
			}

			return null;
		}

		public void Initialize(List<PlayerStatsData> data) {
			_playerFlags       = new PlayerFlags();
			_playerConfig      = new PlayerConfig(data);
			_playerCurrency    = new PlayerCurrency(GameController.Instance.Config.StartCoins);
			_playerCards       = new PlayerCards();
			_playerStats       = new PlayerStats();
			_playerTowers      = new PlayerTowers();
			_playerMines       = new PlayerMines();
			_playerGrandChests = new PlayerGrandChest();
			_playerStats.CreateDefault(_playerConfig);
		}

		public void Load(SaveData data) {
			if (data == null)
				return;

			_playerStats       = data.PlayerStats;
			_playerCurrency    = data.PlayerCurrency;
			_playerFlags       = data.PlayerFlags;
			_playerCards       = data.PlayerCards;
			_playerTowers      = data.PlayerTowers;
			_playerMines       = data.PlayerMines;
			_playerGrandChests = data.PlayerGrandChests;
		}

		public bool IsHaveCoins(int coinsCount) {
			if (CheaterController.NeedFreeUpgrades)
				return true;

			return coinsCount <= _playerCurrency.CoinsValue;
		}

		public void ChangeCoinsCount(float delta, string source = "") {
			if (delta < 0 && CheaterController.NeedFreeUpgrades)
				return;

			if (delta > 0) {

			}

			_playerCurrency.CoinsValue += delta;
			GameController.Instance.Save();

			OnChangeCoinsCount?.Invoke();
			OnChangeGameBoardCoinsCount?.Invoke(delta);
		}

		public void UpgradeStats(PlayerStatType type, PlayerStatConfig config) {
			_playerStats.UpgradeStats(type, config);
			GameController.Instance.Save();
		}

		public void UnlockStat(PlayerStatType type) {
			_playerStats.UnlockStat(type);
			GameController.Instance.Save();
		}

		public List<StatInfo> GetAttackStats() {
			return _playerStats.GetAttackStats();
		}

		public List<StatInfo> GetDefenceStats() {
			return _playerStats.GetDefenceStats();
		}

		public List<StatInfo> GetLootStats() {
			return _playerStats.GetLootStats();
		}

		public void AddCard(CardType type) {
			var cardInfo = _playerCards._collectedCards.FirstOrDefault(x => x._type == type);
			if (cardInfo == null) {
				cardInfo = new CardInfo {
					_type  = type,
					_count = 1,
				};

				_playerCards._collectedCards.Add(cardInfo);

				if (!_neededShowCards.Contains(type))
					_neededShowCards.Add(type);
			}
			else {
				var prevLvlUp = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(cardInfo._count);
				cardInfo._count++;

				var currentLvlUp = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(cardInfo._count);
				if (currentLvlUp > prevLvlUp) {
					if (!_neededShowCards.Contains(type))
						_neededShowCards.Add(type);
				}
			}

			OnAddCard?.Invoke();
			GameController.Instance.Save();
		}

		public CardType GetCardForShow(bool needRemove = true) {
			if (_neededShowCards.Count == 0)
				return CardType.Empty;

			var cardType = _neededShowCards[0];
			if (needRemove)
				_neededShowCards.RemoveAt(0);

			return cardType;
		}

		public void ClearNeedShowCards() {
			_neededShowCards.Clear();
		}

		public bool IsCardMaxLevelUp(CardType type) {
			var cardInfo = GetCard(type);
			if (cardInfo == null)
				return false;

			return GameController.Instance.DB.CardsDB.IsCardMaxLevelUp(cardInfo._count);
		}

		public CardInfo GetCard(CardType type) {
			return _playerCards._collectedCards.FirstOrDefault(x => x._type == type);
		}

		public void ApplayCard(CardType type) {
			_playerCards._activeCards.Add(type);
			GameController.Instance.Save();
		}

		public List<CardType> GetActiveCards() {
			return _playerCards._activeCards;
		}

		public CardInfo GetActiveCardByType(CardType type) {
			if (_playerCards._activeCards.Contains(type)) {
				return GetCard(type);
			}

			return null;
		}

		public void RemoveCard(CardType type) {
			_playerCards._activeCards.Remove(type);
			GameController.Instance.Save();
		}

		public void UnlockCardSlot(int slotId) {
			_playerFlags.UnlockSlot(slotId);
			GameController.Instance.Save();
		}

		public void ClearSave() {
			Initialize(GameController.Instance.Config.GameData.CharacterParameters);
		}

		public Dictionary<string, int> GetStatsLevelUp() {
			var statslevelUpInfo = new Dictionary<string, int>();

			for (var i = 0; i < _playerStats._statInfos.Count; ++i) {
				var type    = _playerStats._statInfos[i]._type.ToString();
				var levelUp = _playerStats._statInfos[i]._levelUp;

				statslevelUpInfo.Add(type, levelUp);
			}

			return statslevelUpInfo;
		}

		public Dictionary<string, float> GetStatsValue() {
			var statsvalueInfo = new Dictionary<string, float>();

			for (var i = 0; i < _playerStats._statInfos.Count; ++i) {
				var type    = _playerStats._statInfos[i]._type.ToString();
				var levelUp = _playerStats._statInfos[i]._value;

				statsvalueInfo.Add(type, levelUp);
			}

			return statsvalueInfo;
		}

		public Dictionary<string, string> GetActiveCardsInfo() {
			var cardsInfo = new Dictionary<string, string>();

			var activeSlotsCount = GameController.Instance.Config.ActiveSlotsCount;
			var unlockedSlots    = GameController.Instance.Player.Flags.UnlockedCardSlotsId;

			for (var i = 0; i < activeSlotsCount; ++i) {
				var slotValStr = "";
				var isUnlocked = unlockedSlots.Contains(i);
				if (isUnlocked) {
					if (i < _playerCards._activeCards.Count)
						slotValStr = _playerCards._activeCards[i].ToString();
					else
						slotValStr = "null";
				}
				else {
					slotValStr = "no_buy";
				}

				cardsInfo.Add($"slot_{i + 1}", slotValStr);
			}

			return cardsInfo;
		}

		public void UnlockTower(int id) {
			var info = _playerTowers._unlockedTowers.Find(x => x._towerId == id);
			if (info != null) {
				Debug.Log($"Tower {id} already unlocked!");
				return;
			}

			var newInfo = new TowerInfo {
				_towerId     = id,
				_maxWaveId   = 0,
				_rangeStepId = 0
			};

			_playerTowers._unlockedTowers.Add(newInfo);
			GameController.Instance.Save();
		}

		public void SetTowerMaxCompleteWave(int id, int waveId) {
			var info = _playerTowers._unlockedTowers.Find(x => x._towerId == id);
			if (info != null) {
				if (waveId > info._maxWaveId) {
					info._maxWaveId = waveId;
					GameController.Instance.Save();
				}
			}
			else
				Debug.Log($" Wrong tower {id}!");
		}

		public bool IsTowerUnlocked(int id) {
			if (id == 0)
				return true;

			var info = _playerTowers._unlockedTowers.Find(x => x._towerId == id);
			if (info != null)
				return true;

			return false;
		}

		public TowerInfo GetTowerInfo(int id) {
			var info = _playerTowers._unlockedTowers.Find(x => x._towerId == id);
			if (info != null)
				return info;

			return null;
		}

		public void UnlockGrandChest(string id) {
			var info = _playerGrandChests._grandChestInfos.Find(x => x._id == id);
			if (info != null) {
				Debug.Log($"Grand chest {id} already unlocked!");
				return;
			}

			var newInfo = new GrandChestInfo {
				_id                 = id,
				_currentProduceTime = 0f,
				_produceTime        = GameController.Instance.Config.GrandChestStartProduceTime,
				_resourceLeft       = GameController.Instance.Config.GrandChestMaxResources,
			};

			_playerGrandChests._grandChestInfos.Add(newInfo);
			GameController.Instance.Save();
		}

		public GrandChestInfo GetGrandChestInfo(string id) {
			var info = _playerGrandChests._grandChestInfos.Find(x => x._id == id);
			if (info != null)
				return info;

			return null;
		}

		public bool IsGrandChestUnlocked(string id) {
			var info = _playerGrandChests._grandChestInfos.Find(x => x._id == id);
			if (info != null)
				return true;

			return false;
		}

		public void UnlockMine(int id, int levelUp) {
			var info = _playerMines._minesInfos.Find(x => x._id == id);
			if (info != null) {
				Debug.Log($"Mine {id} already unlocked!");
				return;
			}

			var newInfo = new MineInfo {
				_id            = id,
				_levelUp       = levelUp,
				_producedCount = 0,
				_produceTime   = 0f,
			};

			_playerMines._minesInfos.Add(newInfo);
			GameController.Instance.Save();
		}

		public MineInfo GetMineInfo(int id) {
			var info = _playerMines._minesInfos.Find(x => x._id == id);
			if (info != null)
				return info;

			return null;
		}

		public bool IsMineUnlocked(int id) {
			var info = _playerMines._minesInfos.Find(x => x._id == id);
			if (info != null)
				return true;

			return false;
		}

		public bool IsHaveCurrency(PlayerCurrencyType type, int count) {
			if (type == PlayerCurrencyType.RedCrystal) {
				if (_playerCurrency.RedCrystal >= count)
					return true;
			}
			else if (type == PlayerCurrencyType.GreenCrystal) {
				if (_playerCurrency.GreenCrystal >= count)
					return true;
			}

			return false;
		}

		public void ChangeCurrency(PlayerCurrencyType type, int delta, string source = "", bool isBought = false) {

			if (type == PlayerCurrencyType.RedCrystal)
				_playerCurrency.RedCrystal += delta;
			else if (type == PlayerCurrencyType.GreenCrystal)
				_playerCurrency.GreenCrystal += delta;

			OnChangeCurrencyCount?.Invoke();
			GameController.Instance.Save();
		}

		public void CollectChest(string chestId) {
			if (!_playerFlags.CollectedChests.Contains(chestId))
				_playerFlags.CollectedChests.Add(chestId);

			GameController.Instance.Save();
		}

		public bool IsChestCollected(string chestId) {
			if (_playerFlags.CollectedChests.Contains(chestId))
				return true;

			return false;
		}

		public void IncreaseTowerPlayedCount(int towerId) {
			var towerInfo = GetTowerInfo(towerId);
			towerInfo._playedCount++;

			GameController.Instance.Save();
		}

		public int GetTowerPlayedCount(int towerId) {
			var towerInfo = GetTowerInfo(towerId);
			return towerInfo._playedCount;
		}
	}
}
