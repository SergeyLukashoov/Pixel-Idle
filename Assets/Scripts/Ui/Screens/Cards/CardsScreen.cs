using System;
using System.Collections.Generic;
using System.Linq;
using Core.Controllers;
using Game.Cards;
using UnityEngine;

namespace Ui.Screens.Cards {
	public class CardsScreen : MonoBehaviour {
		[SerializeField] private GameObject    _cardSlotPrefab;
		[SerializeField] private Transform     _activeSlotsRoot;
		[SerializeField] private Transform     _collectionRoot;
		[SerializeField] private RectTransform _backRT;

		private List<CardSlotView> _activeSlots     = new List<CardSlotView>();
		private List<CardSlotView> _collectionSlots = new List<CardSlotView>();

		private void Awake() {
			BuildActiveSlots();
			BuildCollectionSlots();
		}

		private void Start() {
			_backRT.sizeDelta                        =  new Vector2(Screen.width, Screen.height);
			GameController.Instance.Player.OnAddCard += UpdateCardsScreen;
		}

		private void OnDestroy() {
			GameController.Instance.Player.OnAddCard -= UpdateCardsScreen;
		}

		private void UpdateCardsScreen() {
			UpdateActiveSlotsView();
			UpdateCollectionSlotsView();
		}

		private void BuildActiveSlots() {
			var unlockedSlots    = GameController.Instance.Player.Flags.UnlockedCardSlotsId;
			var activeSlotsCount = GameController.Instance.Config.ActiveSlotsCount;
			var activeCards      = GameController.Instance.Player.GetActiveCards();
			for (var i = 0; i < activeSlotsCount; ++i) {
				var isUnlocked = unlockedSlots.Contains(i);
				var slot       = Instantiate(_cardSlotPrefab, _activeSlotsRoot);
				var slotView   = slot.GetComponent<CardSlotView>();

				var cardType = CardType.Empty;
				if (isUnlocked && i < activeCards.Count)
					cardType = activeCards[i];

				slotView.Initialize(i, cardType, false, isUnlocked);
				slotView.CardsScreen             = this;
				slotView.UpdateAfterUnlockAction = UpdateActiveSlotsView;
				_activeSlots.Add(slotView);

				if (!isUnlocked)
					break;
			}
		}

		private void UpdateActiveSlotsView() {
			for (var i = 0; i < _activeSlots.Count; ++i)
				Destroy(_activeSlots[i].gameObject);

			_activeSlots.Clear();

			BuildActiveSlots();
		}

		private void UpdateCollectionSlotsView() {
			for (var i = 0; i < _collectionSlots.Count; ++i)
				Destroy(_collectionSlots[i].gameObject);

			_collectionSlots.Clear();

			BuildCollectionSlots();
		}

		private void BuildCollectionSlots() {
			var collectedCards = GameController.Instance.Player.CollectedCards;
			for (var i = 0; i < 9; ++i) {
				var slot     = Instantiate(_cardSlotPrefab, _collectionRoot);
				var slotView = slot.GetComponent<CardSlotView>();

				var cardType = CardType.Empty;
				if (i < collectedCards.Count)
					cardType = collectedCards[i]._type;

				slotView.Initialize(i, cardType, true, true);
				slotView.CardsScreen             = this;
				slotView.UpdateAfterUnlockAction = null;
				_collectionSlots.Add(slotView);
			}
		}

		public CardSlotView GetEmptyActiveSlot() {
			var emptySlot = _activeSlots.FirstOrDefault(x => x.IsUnlocked && x.IsEmpty);
			return emptySlot;
		}

		public CardSlotView GetCardSlotInCollection(CardType type) {
			for (var i = 0; i < _collectionSlots.Count; ++i) {
				if (_collectionSlots[i].Type == type)
					return _collectionSlots[i];
			}

			return null;
		}

		public GameObject FindTargetForTutor(string currentStepTargetId) {
			if (currentStepTargetId == "FirstActiveSlot") {
				return _activeSlots[0].gameObject;
			}
			else if (currentStepTargetId == "FirstCollectionSlot") {
				return _collectionSlots[0].gameObject;
			}

			return null;
		}

		public GameObject FindClickTargetForTutor(string currentStepTargetId) {
			if (currentStepTargetId == "FirstActiveSlot") {
				return _activeSlots[0].gameObject;
			}
			else if (currentStepTargetId == "FirstCollectionSlot") {
				return _collectionSlots[0].gameObject;
			}

			return null;
		}
	}
}
