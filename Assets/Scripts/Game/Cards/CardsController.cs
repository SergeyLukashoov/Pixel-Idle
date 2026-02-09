using System;
using System.Collections.Generic;
using System.Linq;
using Core.Controllers;
using Core.Utils;
using Game.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Cards {
	public class CardsController : ICardsController {
		private IPlayer _player;
		private CardsDB _cardsDb;

		public void Initialize(IPlayer player) {
			_player  = player;
			_cardsDb = GameController.Instance.DB.CardsDB;
		}

		public CardType CheckNeedDropCard(int towerId, int bossWaveId) {
			var needDropCard = false;
			if (GameController.Instance.Player.Flags.IsBossKilled(towerId, bossWaveId)) {
				var needDropProb = new RandomProb();
				needDropProb.AddValue("yes", 10);
				needDropProb.AddValue("no", 90);

				var canDrop = needDropProb.GetRandomValue();
				if (canDrop == "yes") {
					needDropCard = true;
				}
			}
			else {
				needDropCard = true;
			}

			if (needDropCard) {
				var card = DropCard();
				_player.AddCard(card);
				return card;
			}

			return CardType.Empty;
		}

		public CardType DropCard(List<CardType> forceCardTypes = null) {
			var cardList = _cardsDb.GetAllCards();
			if (forceCardTypes != null && forceCardTypes.Count > 0)
				cardList = forceCardTypes;

			var cardType     = GetCardForDrop(cardList);
			var droppedCards = new List<CardType>();
			droppedCards.Add(cardType);

			var collectedCard = _player.CollectedCards.FirstOrDefault(x => x._type == cardType);
			if (collectedCard != null) {
				cardList.Remove(collectedCard._type);

				var anotherCardType = GetCardForDrop(cardList);
				droppedCards.Add(anotherCardType);
			}

			var cardRandId = Random.Range(0, droppedCards.Count);
			var randCard   = droppedCards[cardRandId];

			return randCard;
		}

		private CardType GetCardForDrop(List<CardType> cardTypes, bool needSkipMaxLevel = true) {
			var cardProb = new RandomProb();
			for (var i = 0; i < cardTypes.Count; ++i) {
				if (_player.IsCardMaxLevelUp(cardTypes[i]) && needSkipMaxLevel)
					continue;

				var cardConfig = _cardsDb.GetCardConfig(cardTypes[i]);
				var probVal    = _cardsDb.GetCardRarityValue(cardConfig._rarity);
				cardProb.AddValue(cardTypes[i].ToString(), probVal);
			}

			if (cardProb.Count == 0)
				return GetCardForDrop(cardTypes, false);

			var randVal = cardProb.GetRandomValue();
			if (!Enum.TryParse(randVal, out CardType cardType)) {
				Debug.LogError("Wrong card type!");
				return CardType.Empty;
			}

			return cardType;
		}

		public void ApplayCard(CardType card) {
			_player.ApplayCard(card);
		}

		public void RemoveCard(CardType card) {
			_player.RemoveCard(card);
		}

		public float GetCoinsBoostVal() {
			var activeCard = _player.GetActiveCardByType(CardType.CoinsBoost);
			if (activeCard != null) {
				var cardConfig = GameController.Instance.DB.CardsDB.GetCardConfig(activeCard._type);
				var currLevel  = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(activeCard._count);
				var currVal    = cardConfig.GetCardValueByLvlUp(currLevel - 1);

				return currVal / 100f;
			}

			return 0f;
		}
	}
}
