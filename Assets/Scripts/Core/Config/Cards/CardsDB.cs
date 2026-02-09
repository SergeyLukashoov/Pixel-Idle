using System;
using System.Collections.Generic;
using System.Linq;
using Core.Controllers;
using Core.Utils;
using UnityEngine;

namespace Game.Cards {
	[Serializable]
	public class RarityInfo {
		public CardRarity _rarity;
		public int        _dropChance;
		public Color      _frameColor;
		public Color      _backColor;
	}

	[Serializable]
	public class CardConfig {
		public CardRarity     _rarity;
		public CardType       _type;
		public CardApplayType _applayType;
		public Sprite         _icon;
		public Sprite         _gameIcon;
		public List<float>    _value;
		public List<float>    _addValue;

		public float GetCardValueByLvlUp(int lvlUp) {
			if (lvlUp >= _value.Count)
				return _value[_value.Count - 1];

			return _value[lvlUp];
		}

		public float GetCardAddValueByLvlUp(int lvlUp) {
			if (_addValue.Count == 0)
				return 0f;
			
			if (lvlUp >= _addValue.Count)
				return _addValue[_addValue.Count - 1];

			return _addValue[lvlUp];
		}
	}

	[Serializable, CreateAssetMenu(fileName = "CardsDB", menuName = "DB/Create CardsDB", order = 60)]
	public class CardsDB : ScriptableObject {
		[SerializeField] private List<CardConfig> _cardConfigs         = new List<CardConfig>();
		[SerializeField] private List<int>        _upgradeCount        = new List<int>();
		[SerializeField] private List<RarityInfo> _rarityInfos         = new List<RarityInfo>();
		[SerializeField] private List<int>        _cardSlotUnlockPrice = new List<int>();

		public List<CardConfig> Cards               => _cardConfigs;
		public List<int>        CardSlotUnlockPrice => _cardSlotUnlockPrice;

		public CardConfig GetCardConfig(CardType type) {
			return _cardConfigs.FirstOrDefault(x => x._type == type);
		}

		public List<CardType> GetAllCards() {
			var cardTypes = new List<CardType>();
			for (var i = 0; i < _cardConfigs.Count; ++i) {
				cardTypes.Add(_cardConfigs[i]._type);
			}

			return cardTypes;
		}

		public int GetCurrentLevelUp(int cardCount) {
			var cardsCountForLvlUp = 0;
			var lvlUp              = 0;

			for (var i = 0; i < _upgradeCount.Count; ++i) {
				cardsCountForLvlUp += _upgradeCount[i];
				if (cardCount >= cardsCountForLvlUp) {
					lvlUp = i;
				}
				else {
					break;
				}
			}

			return lvlUp + 1;
		}

		public bool IsCardMaxLevelUp(int cardCount) {
			var maxCardCount = 0;
			for (var i = 0; i < _upgradeCount.Count; ++i)
				maxCardCount += _upgradeCount[i];

			if (cardCount >= maxCardCount)
				return true;

			return false;
		}

		public int GetCurrentLevelUpCount(int levelUp) {
			if (levelUp - 1 >= _upgradeCount.Count)
				return _upgradeCount[_upgradeCount.Count - 1];

			return _upgradeCount[levelUp - 1];
		}
		
		public int GetAllLevelUpCount(int levelUp) {
			var allCount = 0;
			for (var i = 0; i < _upgradeCount.Count; ++i) {
				if (i + 1 <= levelUp)
					allCount += _upgradeCount[i];
			}

			return allCount;
		}

		public int GetCardRarityValue(CardRarity rarity) {
			var rarityInfo = _rarityInfos.FirstOrDefault(x => x._rarity == rarity);
			if (rarityInfo != null)
				return rarityInfo._dropChance;

			Debug.Log("[CardDrop] Wrong card rarity!");
			return 100;
		}
		
		public RarityInfo GetCardRarityInfo(CardRarity rarity) {
			return _rarityInfos.FirstOrDefault(x => x._rarity == rarity);
		}

		public List<CardConfig> GetCardsByRarity(CardRarity rarity) {
			return _cardConfigs.FindAll(x => x._rarity == rarity);
		}

		public List<CardType> BuildMaxLvlCards() {
			var maxLvlCards = new List<CardType>();
			for (var i = 0; i < _cardConfigs.Count; ++i) {
				var cardConfig = GameController.Instance.Player.GetCard(_cardConfigs[i]._type);
				if (cardConfig != null && IsCardMaxLevelUp(cardConfig._count)) {
					maxLvlCards.Add(cardConfig._type);
				}
			}
			
			return maxLvlCards;
		}
	}
}
