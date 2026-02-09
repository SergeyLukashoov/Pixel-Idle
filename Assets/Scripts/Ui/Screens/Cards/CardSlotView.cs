using System;
using Core.Controllers;
using Core.Utils;
using Game.Cards;
using Game.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Screens.Cards {
	public class CardSlotView : MonoBehaviour {
		[SerializeField] private Button _buttonUnlock;

		[SerializeField] private GameObject _activeEmptyRoot;
		[SerializeField] private GameObject _activeLockedRoot;
		[SerializeField] private GameObject _collectionEmptyRoot;
		[SerializeField] private CardIcon   _cardIcon;

		[SerializeField] private TextMeshProUGUI _unlockPriceLabel;

		private CardType   _type;
		private CardConfig _config;
		private CardInfo   _cardInfo;

		private int _idx;
		private int _unlockPrice;

		private bool _isInCollection;
		private bool _isUnlocked;
		private bool _isActive;

		public CardType   Type           => _type;
		public bool       IsEmpty        => _type == CardType.Empty;
		public CardConfig Config         => _config;
		public CardInfo   Info           => _cardInfo;
		public bool       IsInCollection => _isInCollection;
		public bool       IsActive       => _isActive;
		public bool       IsUnlocked     => _isUnlocked;

		public CardsScreen CardsScreen             { get; set; }
		public Action      UpdateAfterUnlockAction { get; set; }

		private void Awake() {
			InitButtons();
		}

		private void InitButtons() {
			_buttonUnlock.onClick.AddListener(OnUnlockClick);
		}

		public void OnUnlockClick() {
			if (GameController.Instance.Player.IsHaveCoins(_unlockPrice)) {
				GameController.Instance.Player.ChangeCoinsCount(-_unlockPrice);
				GameController.Instance.Player.UnlockCardSlot(_idx);

				_isUnlocked = true;
				UpdateAfterUnlockAction?.Invoke();
			}
		}

		public void OnApplayClick() {
			_cardIcon.OnApplayClick();
		}

		public void Initialize(int idx, CardType type, bool inCollection, bool unlocked) {
			_idx            = idx;
			_type           = type;
			_isInCollection = inCollection;
			_isUnlocked     = unlocked;
			_config         = GameController.Instance.DB.CardsDB.GetCardConfig(_type);
			_cardInfo       = GameController.Instance.Player.GetCard(_type);

			CheckIsCardActive();
			CalcUnlockPrice();
			UpdateView();
			UpdateButtons();
		}

		private void CheckIsCardActive() {
			var activeCards = GameController.Instance.Player.GetActiveCards();
			_isActive = activeCards.Contains(_type);
		}

		private void UpdateView() {
			var isEmpty = _type == CardType.Empty;

			_collectionEmptyRoot.SetActive(_isInCollection && isEmpty && _isUnlocked);
			_activeEmptyRoot.SetActive(!_isInCollection && isEmpty && _isUnlocked);
			_activeLockedRoot.SetActive(!_isInCollection && !_isUnlocked);
			_cardIcon.gameObject.SetActive(!isEmpty);

			if (!isEmpty)
				InitCard();

			_unlockPriceLabel.text = FormatNumHelper.GetNumStr(_unlockPrice);
		}

		private void InitCard() {
			_cardIcon.Init(this);
		}

		private void UpdateButtons() {
			_buttonUnlock.gameObject.SetActive(!_isUnlocked);
		}

		private void CalcUnlockPrice() {
			if (_isInCollection)
				return;

			var cardSlotUnlockPrice = GameController.Instance.DB.CardsDB.CardSlotUnlockPrice;
			_unlockPrice = cardSlotUnlockPrice[_idx];
		}
	}
}
