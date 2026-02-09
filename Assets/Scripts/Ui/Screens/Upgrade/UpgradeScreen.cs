using System;
using System.Collections.Generic;
using System.Linq;
using Config.Player;
using Core.Controllers;
using Game.Player;
using Ui.Hud;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Screens.Upgrade {
	public enum TabType {
		None,
		Attack,
		Defence,
		Loot,
	}

	[Serializable]
	public class UpgradeTabInfo {
		public TabType    _type;
		public Button     _btn;
		public GameObject _root;
		public GameObject _grid;
		public Color      _color;
		public GameObject _backElements;
	}

	public class UpgradeScreen : MonoBehaviour {
		[SerializeField] private List<UpgradeTabInfo> _tabsInfo = new List<UpgradeTabInfo>();
		[SerializeField] private GameObject           _currentBackElements;
		[SerializeField] private GameObject           _upgradeStatViewPrefab;
		[SerializeField] private GameObject           _unlockStatInfoPrefab;
		[SerializeField] private Image                _backColor;

		private List<GameObject>      _unlockStatsViewInfos = new List<GameObject>();
		private List<UpgradeStatView> _upgradeStatViews      = new List<UpgradeStatView>();

		private GameObject _attackTabInfoObj; 
		private IPlayer    _player;

		private void Awake() {
			_player                       =  GameController.Instance.Player;
			_player.OnChangeCoinsCount    += UpdateStatsView;
			_player.OnChangeCurrencyCount += UpdateStatsView;

			InitButtons();
			InitUpgradeStatsView();
			SetTabActive(TabType.Attack);
		}

		private void OnDestroy() {
			_player.OnChangeCoinsCount    -= UpdateStatsView;
			_player.OnChangeCurrencyCount -= UpdateStatsView;
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
		}

		private void OnAttackTabClick() {
			SetTabActive(TabType.Attack);
		}

		private void OnDefenceTabClick() {
			SetTabActive(TabType.Defence);
		}

		private void OnLootTabClick() {
			SetTabActive(TabType.Loot);
		}

		private void SetTabActive(TabType type) {
			Destroy(_currentBackElements);
			
			for (var i = 0; i < _tabsInfo.Count; ++i) {
				var isActive = _tabsInfo[i]._type == type;
				if (isActive) {
					_backColor.color = _tabsInfo[i]._color;

					_currentBackElements                         = Instantiate(_tabsInfo[i]._backElements, _backColor.transform);
					_currentBackElements.transform.localPosition = Vector3.zero;
				}

				_tabsInfo[i]._root.SetActive(isActive);
				_tabsInfo[i]._btn.interactable = !isActive;
				_tabsInfo[i]._btn.GetComponent<ButtonStatsGroup>().SetActive(isActive);
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
				statsInfos = _player.GetAttackStats();
			else if (type == TabType.Defence)
				statsInfos = _player.GetDefenceStats();
			else if (type == TabType.Loot)
				statsInfos = _player.GetLootStats();

			var needInfoLabel = false;
			for (var i = 0; i < statsInfos.Count; ++i) {
				var statConfig = _player.Config.GetStatInfoByType(statsInfos[i]._type);
				if (!_player.IsTowerUnlocked(statConfig.ShowAfterTower)) {
					needInfoLabel = true;
					continue;
				}

				AddStatView(statsInfos[i], tabInfo);
			}

			if (needInfoLabel) {
				var unlockStatView = Instantiate(_unlockStatInfoPrefab, tabInfo._root.transform);
				unlockStatView.GetComponent<UnlockStatInfoView>().SetColor(
					new Color(113 / 255f, 133 / 255f, 148 / 255f, 1f),
					new Color(13 / 255f, 53 / 255f, 74 / 255f, 0.75f));

				if (type == TabType.Attack)
					_attackTabInfoObj = unlockStatView;
				
				_unlockStatsViewInfos.Add(unlockStatView);
			}
		}

		private void AddStatView(StatInfo info, UpgradeTabInfo tabInfo) {
			var statViewObj = Instantiate(_upgradeStatViewPrefab, tabInfo._grid.transform);
			var statView    = statViewObj.GetComponent<UpgradeStatView>();
			statView.OnUpgradeAction = UpgradeStat;
			statView.Initialize(info, PlayerCurrencyType.Coins);
			statView.UpgradeScreen = this; 

			_upgradeStatViews.Add(statView);
		}

		private void UpgradeStat(PlayerStatType type, PlayerStatConfig statConfig) {
			GameController.Instance.Player.UpgradeStats(type, statConfig);
		}

		public void UpdateStatsView() {
			for (var i = 0; i < _upgradeStatViews.Count; ++i)
				_upgradeStatViews[i].UpdateView();
		}

		public GameObject FindTargetForTutor(string currentStepTargetId) {
			if (currentStepTargetId == "AttackMeta") {
				var statInfo = _upgradeStatViews.Find(x => x.Type == PlayerStatType.Damage);
				if (statInfo != null)
					return statInfo.gameObject;	
			}
			else if (currentStepTargetId == "DefeatMoreWaves") {
				return _attackTabInfoObj;
			}

			return null;
		}
		
		public GameObject FindClickTargetForTutor(string currentStepTargetId) {
			if (currentStepTargetId == "AttackMeta") {
				var statInfo = _upgradeStatViews.Find(x => x.Type == PlayerStatType.Damage);
				if (statInfo != null)
					return statInfo.ButtonUpgrade.gameObject;
			}

			return null;
		}

		public void UpdateScreen() {
			for (var i = 0; i < _upgradeStatViews.Count; ++i)
				Destroy(_upgradeStatViews[i].gameObject);
			
			for (var i = 0; i < _unlockStatsViewInfos.Count; ++i)
				Destroy(_unlockStatsViewInfos[i].gameObject);
			
			_upgradeStatViews.Clear();
			_unlockStatsViewInfos.Clear();
			
			InitUpgradeStatsView();
		}
	}
}
