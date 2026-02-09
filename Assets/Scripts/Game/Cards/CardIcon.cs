using System.Collections.Generic;
using Core.Controllers;
using Core.Controllers.Windows;
using TMPro;
using Ui.Screens.Cards;
using UnityEngine;
using UnityEngine.UI;
using Window;

namespace Game.Cards {
	public class CardIcon : MonoBehaviour {
		[SerializeField] private Button _buttonHint;
		[SerializeField] private Button _buttonApplay;
		[SerializeField] private Button _buttonRemove;

		[SerializeField] private Image _rarityFrame;
		[SerializeField] private Image _rarityAddBack;

		[SerializeField] private GameObject _iconActive;
		[SerializeField] private Image      _cardIcon;

		[SerializeField] private List<Image> _starsImg  = new List<Image>();

		[SerializeField] private TextMeshProUGUI _cardNameLabel;

		private CardSlotView _cardSlot;

		private void Awake() {
			InitButtons();
		}

		private void InitButtons() {
			_buttonHint.onClick.AddListener(OnHintClick);
			_buttonApplay.onClick.AddListener(OnApplayClick);
			_buttonRemove.onClick.AddListener(OnRemoveClick);
		}

		private void OnRemoveClick() {
			GameController.Instance.CardsController.RemoveCard(_cardSlot.Config._type);
			var collectionSlot = _cardSlot.CardsScreen.GetCardSlotInCollection(_cardSlot.Config._type);
			if (collectionSlot)
				collectionSlot.Initialize(0, _cardSlot.Config._type, true, true);

			_cardSlot.Initialize(0, CardType.Empty, false, true);
		}

		private void OnHintClick() {
			var settings = new CardHintWS {
				_cardSlot = _cardSlot
			};

			GameController.Instance.WindowsController.Show(WindowType.CardHintWindow, settings);
		}

		public void OnApplayClick() {
			if (_cardSlot.IsActive)
				return;

			var emptyActiveSlot = _cardSlot.CardsScreen.GetEmptyActiveSlot();
			if (!emptyActiveSlot)
				return;

			GameController.Instance.CardsController.ApplayCard(_cardSlot.Config._type);
			emptyActiveSlot.Initialize(0, _cardSlot.Config._type, false, true);
			_cardSlot.Initialize(0, _cardSlot.Config._type, true, true);
		}

		public void Init(CardSlotView cardSlot) {
			_cardSlot = cardSlot;

			UpdateView();
			UpdateButtons();
		}

		private void UpdateButtons() {
			_buttonHint.gameObject.SetActive((_cardSlot.IsInCollection && !_cardSlot.IsActive) || !_cardSlot.IsInCollection);
			_buttonApplay.gameObject.SetActive(_cardSlot.IsInCollection && !_cardSlot.IsActive);
			_buttonRemove.gameObject.SetActive(!_cardSlot.IsInCollection && _cardSlot.IsActive);
		}

		private void UpdateView() {
			_cardIcon.sprite    = _cardSlot.Config._icon;
			_cardNameLabel.text = GameController.Instance.GetGameText($"{_cardSlot.Config._type}_name");
			_iconActive.SetActive(_cardSlot.IsActive && _cardSlot.IsInCollection);

			InitStars();
			InitRarity();
		}

		private void InitRarity() {
			var rarityInfo = GameController.Instance.DB.CardsDB.GetCardRarityInfo(_cardSlot.Config._rarity);

			_rarityFrame.color   = rarityInfo._frameColor;
			_rarityAddBack.color = rarityInfo._backColor;
		}
		
		private void InitStars() {
			var currentLevelUp = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(_cardSlot.Info._count);
			for (var i = 0; i < _starsImg.Count; ++i) {
				var isActive = i < currentLevelUp;
				
				var c = _starsImg[i].color;
				c.a = isActive ? 1f : 0.2f;
				_starsImg[i].color = c;
			}
		}

		public void SetHintState() {
			_buttonHint.gameObject.SetActive(false);
			_buttonApplay.gameObject.SetActive(false);
			_iconActive.SetActive(false);
		}
	}
}
