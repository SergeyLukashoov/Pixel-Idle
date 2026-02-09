using System;
using Core.Controllers;
using Core.Controllers.Windows;
using Game.Map;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Window;

namespace Ui.Hud {
	public class HudTop : MonoBehaviour {
		[SerializeField] private Button          _buttonSettings;
		[SerializeField] private Button          _buttonMapZoom;
		[SerializeField] private Button          _buttonNoAds;
		[SerializeField] private MapObject       _mapObject;
		[SerializeField] private TextMeshProUGUI _coinsLabel;
		[SerializeField] private TextMeshProUGUI _greenCrystalLabel;
		[SerializeField] private TextMeshProUGUI _redCrystalLabel;
		[SerializeField] private Image           _plusMinusIcon;
		[SerializeField] private Sprite          _plusSp;
		[SerializeField] private Sprite          _minusSp;
		[SerializeField] private GameObject      _currencyRoot;

		public  GameObject NoAdsButtonObj => _buttonNoAds.gameObject;
		public  GameObject CurrencyRoot   => _currencyRoot;
		private bool       _needZoomOut = true;

		public void SetZoomButtonVisible(bool visible) {
			_buttonMapZoom.gameObject.SetActive(visible);
		}

		public bool IsNeedAdsButton() {
			return false;
		}

		public void SetNoAdsButtonVisible(bool visible) {
			if (!IsNeedAdsButton())
				return;

			_buttonNoAds.gameObject.SetActive(visible);
		}

		private void Awake() {
			UpdateCoinsLabel();
			UpdateCrystals();
			SetZoomSprite();

			GameController.Instance.Player.OnChangeCoinsCount    += UpdateCoinsLabel;
			GameController.Instance.Player.OnChangeCurrencyCount += UpdateCrystals;

			_buttonSettings.onClick.AddListener(OpenSettingsWindow);
			_buttonMapZoom.onClick.AddListener(OnMapZoomClick);

			InitNoAdsButton();
		}

		private void Start() {
			UpdateCurrencyRootPos();
		}

		private void UpdateCurrencyRootPos() {
#if UNITY_IOS
			var pos = _currencyRoot.transform.localPosition;
			pos.y                                 -= GameController.Instance.SafeAreaHudShift * 0.5f;
			_currencyRoot.transform.localPosition =  pos;
#endif
		}

		private void InitNoAdsButton() {
			_buttonNoAds.onClick.AddListener(OnNoAdsClick);

			var needBtn = IsNeedAdsButton();
			_buttonNoAds.gameObject.SetActive(needBtn);
		}

		private void OnNoAdsClick() {
			GameController.Instance.WindowsController.ShowNoAdsWindow(_buttonNoAds.gameObject);
		}

		private void OpenSettingsWindow() {
			var settings = new SettingsWS {
				NeedButtons    = false,
				OnRestoreClick = () => { _buttonNoAds.gameObject.SetActive(false); }
			};

			GameController.Instance.WindowsController.Show(WindowType.SettingsWindow, settings);
		}

		private void OnMapZoomClick() {
			var neededScale = _needZoomOut ? 0.5f : 1f;
			_needZoomOut = !_needZoomOut;

			_mapObject.SetMapZoom(neededScale);
			SetZoomSprite();
		}

		private void SetZoomSprite() {
			var neededIconSp = _needZoomOut ? _minusSp : _plusSp;
			_plusMinusIcon.sprite = neededIconSp;
		}

		private void OnDestroy() {
			GameController.Instance.Player.OnChangeCoinsCount    -= UpdateCoinsLabel;
			GameController.Instance.Player.OnChangeCurrencyCount -= UpdateCrystals;
		}

		private void UpdateCoinsLabel() {
			_coinsLabel.text = ((int) GameController.Instance.Player.Currency.CoinsValue).ToString();
		}

		private void UpdateCrystals() {
			_greenCrystalLabel.text = GameController.Instance.Player.Currency.GreenCrystal.ToString();
			_redCrystalLabel.text   = GameController.Instance.Player.Currency.RedCrystal.ToString();
		}
	}
}
