using System;
using System.Collections.Generic;
using Core.Controllers;
using Core.Controllers.Windows;
using Game.Player;
using TMPro;
using Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class NotEnoughCrystalWindow : BaseWindow<NotEnoughCrystalWS> {
		public override WindowType Type => WindowType.NotEnoughCrystalWindow;

		[SerializeField] private Button          _buttonClose;
		[SerializeField] private Button          _buttonIconClose;
		[SerializeField] private Button          _buttonBuyCrystals;
		[SerializeField] private Transform       _resRoot;
		[SerializeField] private GameObject      _resPrefab;
		[SerializeField] private TextMeshProUGUI _labelPrice;

		[SerializeField] private List<RectTransform> _layoutsForRebuild;

		private IPlayer _player;
		private int     _coinsPrice;

		private void Start() {
			_player = GameController.Instance.Player;

			InitButtons();
			InitRes();

			for (var i = 0; i < _layoutsForRebuild.Count; ++i)
				LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutsForRebuild[i]);
		}

		private void InitRes() {
			if (_settings.RedCrystalsCount > 0) {
				var crystalResObj = Instantiate(_resPrefab, _resRoot);
				crystalResObj.GetComponent<RewardObject>().Initialize(PlayerCurrencyType.RedCrystal, _settings.RedCrystalsCount);
			}

			if (_settings.GreenCrystalsCount > 0) {
				var crystalResObj = Instantiate(_resPrefab, _resRoot);
				crystalResObj.GetComponent<RewardObject>().Initialize(PlayerCurrencyType.GreenCrystal, _settings.GreenCrystalsCount);
			}
		}

		private void InitButtons() {
			_buttonClose.onClick.AddListener(Hide);
			_buttonIconClose.onClick.AddListener(Hide);

			_coinsPrice = (_settings.RedCrystalsCount + _settings.GreenCrystalsCount) *
			              GameController.Instance.Config.OneCrystalCoinsPrice;
			_labelPrice.text = _coinsPrice.ToString();

			var isHaveCoins = GameController.Instance.Player.IsHaveCoins(_coinsPrice);
			_buttonBuyCrystals.interactable = isHaveCoins;
			_buttonBuyCrystals.onClick.AddListener(OnBuyClick);
		}

		private void OnBuyClick() {
			if (OnClose)
				return;
			
			_player.ChangeCoinsCount(-_coinsPrice);
			_player.ChangeCurrency(PlayerCurrencyType.RedCrystal, _settings.RedCrystalsCount, "not_enough", true);
			_player.ChangeCurrency(PlayerCurrencyType.GreenCrystal, _settings.GreenCrystalsCount, "not_enough", true);

			Hide();
			_settings.OnBuyClick?.Invoke();
		}
	}

	public class NotEnoughCrystalWS : BaseWindowSettings {
		public int    RedCrystalsCount;
		public int    GreenCrystalsCount;
		public Action OnBuyClick;
	}
}
