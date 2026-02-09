using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Config.Player;
using Core;
using Core.Config.Tutorial;
using Core.Controllers;
using Core.Controllers.Tutorial;
using Game.Boards;
using Game.Player;
using TMPro;
using Ui.Screens.Upgrade;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Hud {
	public class GameHudBottom : MonoBehaviour {
		[SerializeField] private List<UpgradeTabInfo> _tabsInfo = new List<UpgradeTabInfo>();
		[SerializeField] private GameObject           _statsRoot;
		[SerializeField] private GameObject           _upgradeStatViewPrefab;
		[SerializeField] private GameObject           _unlockStatInfoPrefab;
		[SerializeField] private GameBoard            _gameBoard;
		[SerializeField] private GameObject           _adsEffectRoot;
		[SerializeField] private GameObject           _accelerateRoot;
		[SerializeField] private GameObject           _currencyRoot;
		[SerializeField] private Button               _buttonAccelerateUp;
		[SerializeField] private Button               _buttonAccelerateDown;
		[SerializeField] private Button               _buttonAdsAccelerate;
		[SerializeField] private TextMeshProUGUI      _labelAccelerate;
		[SerializeField] private TextMeshProUGUI      _labelAccelerateTimer;
		[SerializeField] private Image                _back;

		private IPlayer            _player;
		private TutorialController _tutorialController;
		private TabType            _activeTabType     = TabType.None;
		private bool               _isStatsShow       = false;
		private bool               _isOnShowHideStats = false;
		private bool               _needAdsButton     = false;

		private List<UpgradeStatView> _upgradeStatViews = new List<UpgradeStatView>();

		private void Awake() {
			_player             = GameController.Instance.Player;
			_tutorialController = GameController.Instance.TutorialController;

			_gameBoard.OnChangeExpCount += UpdateUpgradeButtons;
		}

		private void Start() {
			InitButtons();
			InitUpgradeStatsView();
			SetTabActive(TabType.Attack);

			UpdateUI();
		}

		private void UpdateUI() {
			var needAccelerateRoot = _tutorialController.IsTutorialComplete(TutorialId.Acceleration);
			_accelerateRoot.SetActive(needAccelerateRoot);

			var needAdsEffect = _player.Flags.AdsAccelerationTime > 0f;
			_adsEffectRoot.SetActive(needAdsEffect);
		}

		public void UpdateAccelerateTimer(int time) {
			var min = time / 60;
			var sec = time - min * 60;

			_labelAccelerateTimer.text = $"{min:00}:{sec:00}";
		}

		public void ShowUpgradePanel() {
			var statsRootPos = _statsRoot.transform.localPosition;
			statsRootPos.y                     += 550;
			_statsRoot.transform.localPosition =  statsRootPos;

			SetTabButtonsInteractable(true);
		}

		public void ShowAccelerationRoot() {
			_accelerateRoot.SetActive(true);
		}

		private void SetTabButtonsInteractable(bool isInteractable) {
			for (var i = 0; i < _tabsInfo.Count; ++i) {
				_tabsInfo[i]._btn.interactable = isInteractable;
			}
		}

		private void OnDestroy() {
			_gameBoard.OnChangeExpCount -= UpdateUpgradeButtons;
		}

		private void InitButtons() {
			for (var i = 0; i < _tabsInfo.Count; ++i) {
				if (_tabsInfo[i]._type == TabType.Attack)
					_tabsInfo[i]._btn.onClick.AddListener(OnAttackTabClick);
				else if (_tabsInfo[i]._type == TabType.Defence)
					_tabsInfo[i]._btn.onClick.AddListener(OnDefenceTabClick);
				else if (_tabsInfo[i]._type == TabType.Loot)
					_tabsInfo[i]._btn.onClick.AddListener(OnLootTabClick);
			}

			_buttonAccelerateUp.onClick.AddListener(OnAccelerateUpClick);
			_buttonAccelerateDown.onClick.AddListener(OnAccelerateDownClick);
			_buttonAdsAccelerate.onClick.AddListener(OnAdsAccelerationClick);
			
			UpdateAccelerateBtns();
			UpdateAccelerateLabel();
		}

		private void OnAdsAccelerationClick() {
			if (!GameController.Instance.IsInternetAvailable) {
				GameController.Instance.WindowsController.ShowNoInternetWindow();
				return;
			}
			
		}
		
		private void OnAccelerateUpClick() {
			MyTime.SetAcceleration(0.5f);
			GameController.Instance.Player.Flags.GameAcceleration = MyTime.acceleration;
			GameController.Instance.Save();

			UpdateAccelerateBtns();
			UpdateAccelerateLabel();
			
			UpdateUI();
			
			_gameBoard.UpdateAcceleration();
		}

		private void OnAccelerateDownClick() {
			MyTime.SetAcceleration(-0.5f);
			GameController.Instance.Player.Flags.GameAcceleration = MyTime.acceleration;
			GameController.Instance.Save();

			UpdateAccelerateBtns();
			UpdateAccelerateLabel();

			_gameBoard.UpdateAcceleration();
		}

		public void SetAdsAcceleration() {
			MyTime.SetMaxAcceleration();

			GameController.Instance.Player.Flags.GameAcceleration = MyTime.acceleration;
			GameController.Instance.Save();

			UpdateAccelerateBtns();
			UpdateAccelerateLabel();
			UpdateUI();
			
			_gameBoard.UpdateAcceleration();
		}

		public void SetDefaultAcceleration() {
			MyTime.maxAcceleration = MyTime.maxDefaultAcceleration;
			MyTime.SetMaxAcceleration();

			GameController.Instance.Player.Flags.GameAcceleration = MyTime.acceleration;
			GameController.Instance.Save();

			UpdateAccelerateBtns();
			UpdateAccelerateLabel();
			UpdateUI();

			_gameBoard.UpdateAcceleration();
		}

		private void UpdateAccelerateBtns() {
			if (MyTime.acceleration <= 1.5f)
				_buttonAccelerateDown.interactable = false;
			else
				_buttonAccelerateDown.interactable = true;

			if (MyTime.acceleration >= MyTime.maxAcceleration) {
				_buttonAccelerateUp.interactable = false;
				_needAdsButton = true;
			}
			else {
				_buttonAccelerateUp.interactable = true;
				_needAdsButton = false;
			}

			CheckNeedAdsAccelerate();
		}

		public void CheckNeedAdsAccelerate() {
			var needAdsAccelerateBtn =
				_player.Flags.PlayedCount >= 2 && GameController.Instance.IsInternetAvailable &&
				_player.Flags.AdsAccelerationTime <= 0f && _needAdsButton;

			_buttonAdsAccelerate.gameObject.SetActive(needAdsAccelerateBtn);
		}

		private void UpdateAccelerateLabel() {
			var acceleration  = MyTime.acceleration - 0.5f;
			var accelerateStr = acceleration.ToString(CultureInfo.InvariantCulture);
			_labelAccelerate.text = $"{accelerateStr}x";
		}

		private void OnAttackTabClick() {
			if (_isOnShowHideStats)
				return;

			SetTabActive(TabType.Attack);
		}

		private void OnDefenceTabClick() {
			if (_isOnShowHideStats)
				return;

			SetTabActive(TabType.Defence);
		}

		private void OnLootTabClick() {
			if (_isOnShowHideStats)
				return;

			SetTabActive(TabType.Loot);
		}

		public void SetTabActive(TabType type) {
			_activeTabType = type;

			for (var i = 0; i < _tabsInfo.Count; ++i) {
				var isActive = _tabsInfo[i]._type == type;
				_tabsInfo[i]._root.gameObject.SetActive(isActive);
				_tabsInfo[i]._btn.GetComponent<ButtonStatsGroup>().SetActive(isActive);

				if (isActive)
					_back.color = _tabsInfo[i]._color;
			}
		}

		private void InitUpgradeStatsView() {
			InitStatsView(TabType.Attack);
			InitStatsView(TabType.Defence);
			InitStatsView(TabType.Loot);
		}

		private void InitStatsView(TabType type) {
			var tabInfo = _tabsInfo.FirstOrDefault(x => x._type == type);
			if (tabInfo == null)
				return;

			var statsInfos = new List<StatInfo>();
			if (type == TabType.Attack)
				statsInfos = _gameBoard.PlayerView.Stats.GetAttackStats();
			else if (type == TabType.Defence)
				statsInfos = _gameBoard.PlayerView.Stats.GetDefenceStats();
			else if (type == TabType.Loot)
				statsInfos = _gameBoard.PlayerView.Stats.GetLootStats();

			var statsViewOnPage = new List<UpgradeStatView>();
			for (var i = 0; i < statsInfos.Count; ++i) {
				if (!statsInfos[i]._unlocked)
					continue;

				var statViewObj = Instantiate(_upgradeStatViewPrefab, tabInfo._grid.transform);
				var statView    = statViewObj.GetComponent<UpgradeStatView>();
				statView.OnUpgradeAction = UpgradeStat;
				statView.GameBoard       = _gameBoard;
				statView.Initialize(statsInfos[i], PlayerCurrencyType.Exp);

				_upgradeStatViews.Add(statView);
				statsViewOnPage.Add(statView);
			}

			if (statsViewOnPage.Count == 0) {
				var unlockStatView = Instantiate(_unlockStatInfoPrefab, tabInfo._root.transform);
				unlockStatView.GetComponent<UnlockStatInfoView>().SetColor(
					new Color(1f, 1f, 1f, 1f),
					new Color(254 / 255f, 255 / 255f, 246 / 255f, 1f));

				var rt = unlockStatView.GetComponent<RectTransform>();
				rt.anchorMin        = new Vector2(0.5f, 1f);
				rt.anchorMax        = new Vector2(0.5f, 1f);
				rt.anchoredPosition = Vector2.up;
			}
		}

		private void UpgradeStat(PlayerStatType type, PlayerStatConfig statConfig) {
			_gameBoard.PlayerView.Stats.UpgradeStats(type, statConfig);
			_gameBoard.UpdateDpsLabel();

			if (type == PlayerStatType.Damage || type == PlayerStatType.HealthRegen || type == PlayerStatType.MetaCurrencyMult)
				_gameBoard.UpdatePlayerStatLabel(type);

			if (type == PlayerStatType.Health)
				_gameBoard.UpdatePlayerHealth();
			else if (type == PlayerStatType.Range)
				_gameBoard.UpdateRange();
			else if (type == PlayerStatType.ShotsPerSec)
				_gameBoard.UpdateShotPerSec();
		}

		public void UpdateUpgradeButtons() {
			for (var i = 0; i < _upgradeStatViews.Count; ++i) {
				_upgradeStatViews[i].UpdateView();
			}
		}

		public GameObject FindTargetForTutor(string targetId) {
			GameObject target = null;
			if (targetId == "AttackUpgrade") {
				var statView = _upgradeStatViews.FirstOrDefault(x => x.Type == PlayerStatType.Damage);
				if (statView)
					target = statView.ButtonUpgrade.gameObject;
			}
			else if (targetId == "HealthUpgrade") {
				var statView = _upgradeStatViews.FirstOrDefault(x => x.Type == PlayerStatType.Health);
				if (statView)
					target = statView.ButtonUpgrade.gameObject;
			}
			else if (targetId == "DefenceTab") {
				var tabInfo = _tabsInfo.Find(x => x._type == TabType.Defence);
				if (tabInfo != null)
					target = tabInfo._btn.gameObject;
			}
			else if (targetId == "AccelerateRoot") {
				target = _accelerateRoot;
			}
			else if (targetId == "CurrencyObject") {
				target = _currencyRoot;
			}

			return target;
		}

		public GameObject FindClickTargetForTutor(string targetId) {
			GameObject target = null;
			if (targetId == "AccelerateRoot") {
				target = _buttonAccelerateUp.gameObject;
			}

			return target;
		}
	}
}
