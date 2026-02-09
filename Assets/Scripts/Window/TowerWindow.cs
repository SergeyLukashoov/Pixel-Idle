using System;
using System.Collections.Generic;
using Core.Config.Tutorial;
using Core.Controllers;
using Core.Controllers.Tutorial;
using Core.Controllers.Windows;
using Core.Utils;
using Game.Cards;
using TMPro;
using Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class TowerWindow : BaseWindow<TowerWS> {
		public override WindowType Type => WindowType.TowerWindow;

		[SerializeField] private Button                 _buttonClose;
		[SerializeField] private Button                 _buttonIconClose;
		[SerializeField] private Button                 _buttonPlay;
		[SerializeField] private Button                 _buttonInfo;
		[SerializeField] private Image                  _buttonPlayImage;
		[SerializeField] private Sprite                 _buttonPlayDisabledSp;
		[SerializeField] private TextMeshProUGUI        _labelInfo;
		[SerializeField] private TextMeshProUGUI        _labelButtonPlay;
		[SerializeField] private GameObject             _addLabelObj;
		[SerializeField] private GameObject             _coinIconObj;
		[SerializeField] private GameObject             _passiveSkillsRoot;
		[SerializeField] private List<RectTransform>    _forceUpdateLayouts;
		[SerializeField] private List<PassiveSkillIcon> _passiveSkills;

		private TutorialController _tutorialController;
		private bool               _needRebuildLayers = true;

		private void Awake() {
			InitButtons();
		}

		private void Start() {
			_tutorialController = GameController.Instance.TutorialController;

			UpdateView();
			CheckNeedTutorials();
		}

		private void InitPassiveSkillsView() {
			if (!GameController.Instance.TutorialController.IsTutorialComplete(TutorialId.NewCard)) {
				_passiveSkillsRoot.SetActive(false);
				return;
			}
			
			var unlockedCardSlots = GameController.Instance.Player.Flags.UnlockedCardSlotsId;
			var activeCards       = GameController.Instance.Player.GetActiveCards();

			for (var i = 0; i < _passiveSkills.Count; ++i) {
				_passiveSkills[i].OnSkillClick = OnSkillClick;
				
				if (i < unlockedCardSlots.Count) {
					var cardType = i < activeCards.Count ? activeCards[i] : CardType.Empty;
					_passiveSkills[i].Initialize(cardType);
				}
				else {
					_passiveSkills[i].InitLockedState();
				}
			}
		}

		private void OnSkillClick() {
			_settings.OnHide = _settings.OnSkillClick;
			Hide();
		}
		
		private void CheckNeedTutorials() {
			if (!_tutorialController.IsTutorialComplete(TutorialId.FirstTowerUnlock)) {
				_tutorialController.StartTutorial(TutorialId.FirstTowerUnlock, FindTargetForTutor);
			}
			else if (_settings.TowerId == 2 &&
			         !_tutorialController.IsTutorialComplete(TutorialId.AfterRangeInfoBattle) &&
			         _tutorialController.IsTutorialComplete(TutorialId.FirstInfoRange)) {
				_tutorialController.StartTutorial(TutorialId.AfterRangeInfoBattle, FindTargetForTutor);
			}
		}

		private GameObject FindTargetForTutor() {
			var currentStepTargetId = _tutorialController.CurrentStepTargetId;
			if (currentStepTargetId == "ButtonUnlock" || currentStepTargetId == "ButtonPlay") {
				return _buttonPlay.gameObject;
			}

			return null;
		}

		private void UpdateView() {
			var infoLabel    = "";
			var needAddLabel = false;
			var needCoinIcon = false;

			if (_settings.IsTowerUnlocked) {
				infoLabel    = BuildUnlockedTowerInfo();
				needAddLabel = true;

				_labelButtonPlay.text = GameController.Instance.GetGameText("common_battle");

				InitPassiveSkillsView();
			}
			else {
				infoLabel    = GameController.Instance.GetGameText("tower_window_available_info");
				needCoinIcon = true;

				var priceStr = FormatNumHelper.GetNumStr(_settings.UnlockPrice);
				if (_settings.UnlockPrice <= 0) {
					priceStr     = GameController.Instance.GetGameText("common_free");
					needCoinIcon = false;
				}

				_labelButtonPlay.text = priceStr;
				_buttonInfo.gameObject.SetActive(false);
				_passiveSkillsRoot.SetActive(false);
			}

			_addLabelObj.SetActive(needAddLabel);
			_coinIconObj.SetActive(needCoinIcon);
			_labelInfo.text = infoLabel;

			if (!IsBattleAvailable()) {
				_buttonPlayImage.sprite  = _buttonPlayDisabledSp;
				_buttonPlay.interactable = false;
			}
		}

		private void LateUpdate() {
			RebuildLayers();
		}

		private void RebuildLayers() {
			if (!_needRebuildLayers)
				return;

			_needRebuildLayers = false;
			for (var i = 0; i < _forceUpdateLayouts.Count; ++i)
				LayoutRebuilder.ForceRebuildLayoutImmediate(_forceUpdateLayouts[i]);
		}

		private bool IsBattleAvailable() {
			if (_settings.IsTowerUnlocked)
				return true;

			if (GameController.Instance.Player.IsHaveCoins(_settings.UnlockPrice))
				return true;

			return false;
		}

		private void InitButtons() {
			_buttonClose.onClick.AddListener(Hide);
			_buttonIconClose.onClick.AddListener(Hide);
			_buttonPlay.onClick.AddListener(OnPlayClick);
			_buttonInfo.onClick.AddListener(OnInfoClick);
		}

		private void OnInfoClick() {
			var settings = new InfoHeaderWS {
				HeaderStr   = GameController.Instance.GetGameText("tower_window_range_info_header"),
				InfoStr     = GameController.Instance.GetGameText("tower_window_range_info"),
				NeedHeader  = true,
				NeedContent = false,
			};

			GameController.Instance.WindowsController.Show(WindowType.InfoHeaderWindow, settings);
		}

		private string BuildUnlockedTowerInfo() {
			var towerInfo = GameController.Instance.Player.GetTowerInfo(_settings.TowerId);

			var rangeStepId = towerInfo._rangeStepId;
			if (rangeStepId > 3)
				rangeStepId = 3;

			var infoStr = GameController.Instance.GetGameText("tower_window_range");
			infoStr += $"{rangeStepId}/3";
			infoStr += "\n";
			infoStr += GameController.Instance.GetGameText("tower_window_max_wave");
			infoStr += $"{towerInfo._maxWaveId}";
			infoStr += "\n";
			infoStr += GameController.Instance.GetGameText("tower_window_mult");
			infoStr += $"x{_settings.CurrencyMultiplier}";

			return infoStr;
		}

		private void OnPlayClick() {
			GameController.Instance.PlayedTowerInfo.Id                    = _settings.TowerId;
			GameController.Instance.PlayedTowerInfo.TerrainId             = _settings.TerrainId;
			GameController.Instance.PlayedTowerInfo.StartWaveId           = _settings.StartWaveId;
			GameController.Instance.PlayedTowerInfo.CurrencyMultiplier    = _settings.CurrencyMultiplier;
			GameController.Instance.PlayedTowerInfo.EnemyDamageMultiplier = _settings.EnemyDamageMultiplier;
			GameController.Instance.PlayedTowerInfo.EnemyHealthMultiplier = _settings.EnemyHealthMultiplier;

			if (!_settings.IsTowerUnlocked) {
				GameController.Instance.Player.ChangeCoinsCount(-_settings.UnlockPrice);
				GameController.Instance.Player.UnlockTower(_settings.TowerId);

				_settings.OnUnlockAction?.Invoke();
			}
			else {
			
				SceneController.LoadGame();
			}

			Hide();
		}
	}

	public class TowerWS : BaseWindowSettings {
		public int    TowerId;
		public int    UnlockPrice;
		public int    TerrainId;
		public int    StartWaveId;
		public float  CurrencyMultiplier;
		public bool   IsTowerUnlocked;
		public Action OnUnlockAction;
		public float  EnemyHealthMultiplier;
		public float  EnemyDamageMultiplier;
		public Action OnSkillClick;
	}
}
