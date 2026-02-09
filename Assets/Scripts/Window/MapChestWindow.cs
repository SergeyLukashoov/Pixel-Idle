using System.Collections.Generic;
using Core.Config.Chests;
using Core.Config.Tutorial;
using Core.Controllers;
using Core.Controllers.Tutorial;
using Core.Controllers.Windows;
using Game.Map;
using Game.Player;
using Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class MapChestWindow : BaseWindow<MapChestWS> {
		public override WindowType Type => WindowType.MapChestWindow;

		[SerializeField] private Image  _buttonAdsDoubleRewardImage;
		[SerializeField] private Image  _buttonShowAdsImage;
		[SerializeField] private Sprite _buttonDefaultSP;
		[SerializeField] private Sprite _buttonDefaultPressedSP;
		[SerializeField] private Sprite _buttonDisabledSP;
		[SerializeField] private Sprite _buttonDisabledPressedSP;

		[SerializeField] private Button              _buttonTutorialGet;
		[SerializeField] private Button              _buttonAdsDoubleReward;
		[SerializeField] private Button              _buttonShowAds;
		[SerializeField] private Button              _buttonClose;
		[SerializeField] private Button              _buttonCloseIcon;
		[SerializeField] private Transform           _rewardsRoot;
		[SerializeField] private Transform           _popUpRoot;
		[SerializeField] private GameObject          _rewardObj;
		[SerializeField] private GameObject          _tapToContinueObj;
		[SerializeField] private List<RectTransform> _layoutsForRebuild = new List<RectTransform>();

		private IPlayer            _player;
		private TutorialController _tutorialController;

		private bool _onShowAds = false;
		
		private void Start() {
			_player             = GameController.Instance.Player;
			_tutorialController = GameController.Instance.TutorialController;

			InitButtons();
			InitRewards();
			UpdateView();

			for (var i = 0; i < _layoutsForRebuild.Count; ++i) {
				LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutsForRebuild[i]);
			}
		}

		private void UpdateView() {
			var isDefaultChest = _settings.Chest.Type == MapChestType.Default;

			_buttonCloseIcon.gameObject.SetActive(!isDefaultChest);
			_tapToContinueObj.SetActive(isDefaultChest);
			_popUpRoot.gameObject.SetActive(isDefaultChest);
		}

		private void CheckNeedTutorials() {
			if (_tutorialController.NeedChestWindow &&
			    !_tutorialController.IsTutorialComplete(TutorialId.FirstChestCollect) &&
			    _tutorialController.IsTutorialComplete(TutorialId.FirstChest)) {
				_tutorialController.StartTutorial(TutorialId.FirstChestCollect, FindTargetForTutor);
				_tutorialController.NeedChestWindow = false;
			}
		}

		private GameObject FindTargetForTutor() {
			var currentStepTargetId = _tutorialController.CurrentStepTargetId;
			if (currentStepTargetId == "ButtonCollect") {
				return _buttonTutorialGet.gameObject;
			}

			return null;
		}

		private void InitButtons() {
			_buttonClose.onClick.AddListener(OnCloseClick);
			_buttonCloseIcon.onClick.AddListener(OnCloseClick);
			_buttonTutorialGet.onClick.AddListener(OnCloseClick);
			_buttonAdsDoubleReward.onClick.AddListener(OnX2CollectClick);
			_buttonShowAds.gameObject.SetActive(false);

			var isDefaultChest = _settings.Chest.Type == MapChestType.Default;
			var showX2 = isDefaultChest && _tutorialController.IsTutorialComplete(TutorialId.FirstChestCollect);
			_buttonAdsDoubleReward.gameObject.SetActive(showX2);

			if (!_tutorialController.IsTutorialComplete(TutorialId.FirstChestCollect) && isDefaultChest) {
				_buttonAdsDoubleReward.gameObject.SetActive(false);
				_buttonTutorialGet.gameObject.SetActive(true);
				_tapToContinueObj.SetActive(false);
				_popUpRoot.gameObject.SetActive(false);
			}
			else {
				_buttonTutorialGet.gameObject.SetActive(false);
			}
		}

		private void OnCloseClick() {
			if (_onShowAds)
				return;
			
			if (_settings.Chest.Type == MapChestType.Default)
				_settings.Chest.CollectChest();
			else if (_settings.Chest.Type == MapChestType.RewardedAds)
				_settings.Chest.CollectAdChest();
			
			Hide();
		}

		private void OnX2CollectClick() {
			if (_onShowAds)
				return;
			_settings.Chest.CollectChest(2);
			Hide();
		}
		
		private void SetButtonSprites(Button btn, bool isAdAvailable) {
			var sp        = isAdAvailable ? _buttonDefaultSP : _buttonDisabledSP;
			var spPressed = isAdAvailable ? _buttonDefaultPressedSP : _buttonDisabledPressedSP;

			var spState = btn.spriteState;
			spState.highlightedSprite = sp;
			spState.disabledSprite    = sp;
			spState.selectedSprite    = sp;
			spState.pressedSprite     = spPressed;
			btn.spriteState           = spState;
		}

		private void OnAdsGetReward() {
			_onShowAds = false;
			_settings.Chest.CollectAdChest();
			Hide();
		}

		private void AddCards(int count) {
			var cardController = GameController.Instance.CardsController;
			for (var i = 0; i < count; ++i) {
				var cardType = cardController.DropCard(_settings.Chest.ChestData.CardsList);
				_player.AddCard(cardType);
			}
		}

		private void InitRewards() {
			for (var i = 0; i < _settings.Reward.Count; ++i) {
				var reward = _settings.Reward[i];

				var rewardObj = Instantiate(_rewardObj, _rewardsRoot);

				var needAdsReward      = _settings.Chest.Type == MapChestType.Default;
				var isTutorialComplete = _tutorialController.IsTutorialComplete(TutorialId.FirstChestCollect);

				rewardObj.GetComponent<RewardObject>().Initialize(reward.Type, reward.Count, needAdsReward && isTutorialComplete);
				rewardObj.GetComponent<RewardObject>().SetAdsReward(reward.Count * 2);

				AddSmallRewardIcon(reward.Type);
			}
		}

		private void AddSmallRewardIcon(PlayerCurrencyType type) {
			var currency = GameController.Instance.DB.CurrencyDB.GetCurrencyInfo(type);
			if (currency != null) {
				var iconObj = Instantiate(new GameObject("Icon"), _popUpRoot);
				var img     = iconObj.AddComponent<Image>();
				img.sprite = currency.IconSmall;
				img.SetNativeSize();
			}
		}

		private void Update() {
			if (OnShow)
				return;

			CheckNeedTutorials();
		}
	}

	public class MapChestWS : BaseWindowSettings {
		public string           ID;
		public List<RewardInfo> Reward;
		public MapChestObject   Chest;
	}
}
