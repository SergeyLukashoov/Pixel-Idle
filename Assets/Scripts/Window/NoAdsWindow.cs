using System;
using System.Collections.Generic;
using Core.Config.Chests;
using Core.Config.Store;
using Core.Controllers;
using Core.Controllers.Windows;
using Game.Player;
using TMPro;
using Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class NoAdsWindow : BaseWindow<NoAdsWS> {
		public override WindowType Type => WindowType.NoAdsWindow;

		[SerializeField] private Button          _buttonClose;
		[SerializeField] private Button          _buttonCloseIcon;
		[SerializeField] private Button          _buttonBuy;
		[SerializeField] private GameObject      _rewardPrefab;
		[SerializeField] private Transform       _rewardRoot;
		[SerializeField] private TextMeshProUGUI _priceLabel;
		[SerializeField] private TextMeshProUGUI _oldPriceLabel;

		private StoreDB            _storeDB;
		private ProductData        _product;
		private ProductData        _productFakePrice;

		private List<RewardInfo> _rewards;

		private void Awake() {
			_rewards            = GameController.Instance.Config.NoAdsReward;
			_storeDB            = GameController.Instance.DB.StoreDB;
			_product            = _storeDB.GetProductBySKU(StoreProductNames.NO_ADS);
			_productFakePrice   = _storeDB.GetProductBySKU(StoreProductNames.NO_ADS_FAKE_PRICE);

			InitButtons();
		}

		private void Start() {
			InitRewards();
			InitPrice();
		}

		private void InitRewards() {
			for (var i = 0; i < _rewards.Count; ++i) {
				var rewardObj = Instantiate(_rewardPrefab, _rewardRoot);
				rewardObj.GetComponent<RewardObject>().Initialize(_rewards[i].Type, _rewards[i].Count);
			}
		}

		private void InitPrice() {
			if (_product == null || _productFakePrice == null) {
				Debug.LogError($"Product {StoreProductNames.NO_ADS} or {StoreProductNames.NO_ADS_FAKE_PRICE} not found!");
				return;
			}

			_priceLabel.text    = _product._localizedPriceStr;
			_oldPriceLabel.text = _productFakePrice._localizedPriceStr;
		}

		private void InitButtons() {
			_buttonClose.onClick.AddListener(Hide);
			_buttonCloseIcon.onClick.AddListener(Hide);
		}
		

		private void OnPurchaseComplete() {
			_settings.NoAdsIcon.SetActive(false);

			AddReward();
			Hide();
		}

		private void AddReward() {
			var player = GameController.Instance.Player;

			player.Flags.NonConsumableItems.Add(_product._name);

			for (var i = 0; i < _rewards.Count; ++i) {
				var rewardInfo = _rewards[i];
				if (rewardInfo.Type == PlayerCurrencyType.Coins)
					player.ChangeCoinsCount(rewardInfo.Count, "buy_no_ads");
				else if (rewardInfo.Type is
				         PlayerCurrencyType.CardCommon or
				         PlayerCurrencyType.CardRare or
				         PlayerCurrencyType.CardEpic)
					AddCards(rewardInfo.Count);
				else
					player.ChangeCurrency(rewardInfo.Type, rewardInfo.Count, "buy_no_ads");
			}
		}

		private void AddCards(int count) {
			var cardController = GameController.Instance.CardsController;
			for (var i = 0; i < count; ++i) {
				var cardType = cardController.DropCard();
				GameController.Instance.Player.AddCard(cardType);
			}
		}
	}

	public class NoAdsWS : BaseWindowSettings {
		public GameObject NoAdsIcon;
	}
}
