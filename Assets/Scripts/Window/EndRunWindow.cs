using System.Collections.Generic;
using Core.Controllers;
using Core.Controllers.Windows;
using Game.Enemy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class EndRunWindow : BaseWindow<EndRunWS> {
		public override WindowType Type => WindowType.EndRunWindow;

		[SerializeField] private Button _buttonShowAds;
		[SerializeField] private Button _buttonOk;
		[SerializeField] private Image  _buttonShowAdsImage;
		[SerializeField] private Button _buttonClose;
		[SerializeField] private Sprite _buttonDefaultSP;
		[SerializeField] private Sprite _buttonDefaultPressedSP;
		[SerializeField] private Sprite _buttonDisabledSP;
		[SerializeField] private Sprite _buttonDisabledPressedSP;

		[SerializeField] private GameObject _adsRewardInfo;
		[SerializeField] private GameObject _tapToContinue;

		[SerializeField] private TextMeshProUGUI _currentWaveLabel;
		[SerializeField] private TextMeshProUGUI _maxWaveLabel;
		[SerializeField] private TextMeshProUGUI _killedByLabel;
		[SerializeField] private TextMeshProUGUI _coinsCountLabel;
		[SerializeField] private TextMeshProUGUI _coinsCountBonusAdsLabel;
		[SerializeField] private TextMeshProUGUI _cardsCountLabel;
		[SerializeField] private TextMeshProUGUI _buttonLabel;

		[SerializeField] private GameObject          _cardsRoot;
		[SerializeField] private List<RectTransform> _layoutsForRebuild = new List<RectTransform>();

		private bool _onShowAds = false;
		
		private void Awake() {
			InitButtons();

			_buttonLabel.text = $"{GameController.Instance.Config.AdsEndRunBonus * 100 - 100}% " +
			                    GameController.Instance.GetGameText("common_bonus");

			UpdateView();
		}

		private void InitButtons() {
			_buttonShowAds.onClick.AddListener(OnOkClick);
			_buttonClose.onClick.AddListener(OnCloseClick);
			_buttonOk.onClick.AddListener(OnCloseClick);

			var sp = IsAdsAvailable() ? _buttonDefaultSP : _buttonDisabledSP;
			_buttonShowAdsImage.sprite = sp;

			var spPressed = IsAdsAvailable() ? _buttonDefaultPressedSP : _buttonDisabledPressedSP;

			var spState = _buttonShowAds.spriteState;
			spState.highlightedSprite  = sp;
			spState.disabledSprite     = sp;
			spState.selectedSprite     = sp;
			spState.pressedSprite      = spPressed;
			_buttonShowAds.spriteState = spState;
		}

		private void UpdateView() {
			_buttonOk.gameObject.SetActive(true);
			_buttonShowAds.gameObject.SetActive(false);
			_adsRewardInfo.SetActive(false);
			_tapToContinue.SetActive(false);
		}

		private bool IsAdsAvailable() {
			if (GameController.Instance.Player.Flags.PlayedCount < 2)
				return false;

			return true;
		}

		private void OnCloseClick() {
			if (_onShowAds)
				return;
			
			Hide();
		}

		private void OnOkClick() {
			if (OnClose)
				return;
		}

		private void OnAdsGetReward() {
			_onShowAds = false;
			GameController.Instance.Player.ChangeCoinsCount(
				_settings.CoinsCount * GameController.Instance.Config.AdsEndRunBonus - _settings.CoinsCount);

			Hide();
		}
		
		private void OnAdsSkipReward() {
			_onShowAds = false;
		}

		private void Start() {
			InitLabels();

			if (_settings?.CardsCount == 0)
				_cardsRoot.SetActive(false);

			for (var i = 0; i < _layoutsForRebuild.Count; ++i)
				LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutsForRebuild[i]);
		}

		private void InitLabels() {
			_currentWaveLabel.text = GameController.Instance.GetGameText("EndRunWindow_current_wave") + _settings?.WaveId;
			_maxWaveLabel.text     = GameController.Instance.GetGameText("EndRunWindow_max_wave") + _settings?.HighestWaveId;
			_killedByLabel.text = GameController.Instance.GetGameText("EndRunWindow_killed_by") +
			                      GameController.Instance.GetGameText(_settings?.KilledEnemyType + "_name");

			_cardsCountLabel.text = GameController.Instance.GetGameText("common_cards") + _settings?.CardsCount;
			_coinsCountLabel.text = GameController.Instance.GetGameText("common_coins") + _settings?.CoinsCount;
			_coinsCountBonusAdsLabel.text = GameController.Instance.GetGameText("common_coins") +
			                                _settings?.CoinsCount * GameController.Instance.Config.AdsEndRunBonus;

			_killedByLabel.gameObject.SetActive(_settings.NeedKilledBy);
		}
	}

	public class EndRunWS : BaseWindowSettings {
		public int       WaveId;
		public int       HighestWaveId;
		public int       CoinsCount;
		public int       CardsCount;
		public EnemyType KilledEnemyType;
		public bool      NeedKilledBy;
	}
}
