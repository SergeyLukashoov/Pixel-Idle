using System.Collections.Generic;
using Core.Config.Tutorial;
using Core.Controllers;
using Core.Controllers.Tutorial;
using Core.Controllers.Windows;
using Core.Utils;
using Game.Player;
using TMPro;
using Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class MineWindow : BaseWindow<MineWS> {
		public override WindowType Type => WindowType.MineWindow;

		[SerializeField] private Button              _buttonClose;
		[SerializeField] private Button              _buttonIconClose;
		[SerializeField] private Button              _buttonInfo;
		[SerializeField] private GameObject          _timerRoot;
		[SerializeField] private GameObject          _conditionsRoot;
		[SerializeField] private ProgressBar         _capacityProgress;
		[SerializeField] private Image               _crystalIcon;
		[SerializeField] private TextMeshProUGUI     _levelUpLabel;
		[SerializeField] private TextMeshProUGUI     _capacityLabel;
		[SerializeField] private TextMeshProUGUI     _incomeLabel;
		[SerializeField] private TextMeshProUGUI     _condition1Label;
		[SerializeField] private TextMeshProUGUI     _condition2Label;
		[SerializeField] private TextMeshProUGUI     _timeLeftLabel;
		[SerializeField] private TextMeshProUGUI     _labelNotControlHeader;
		[SerializeField] private Image               _condition1Icon;
		[SerializeField] private Image               _condition2Icon;
		[SerializeField] private Sprite              _conditionDoneSp;
		[SerializeField] private Sprite              _conditionNoneSp;
		[SerializeField] private Color               _conditionDoneColor;
		[SerializeField] private Color               _conditionNoneColor;
		[SerializeField] private List<RectTransform> _forceUpdateLayouts;

		private TutorialController _tutorialController;
		
		private float _currentTime;
		private float _produceTime;
		private int   _currentCapacity;

		private void Start() {
			_tutorialController = GameController.Instance.TutorialController;
			_currentTime        = _settings.CurrentTime;
			_produceTime        = _settings.ProduceTime;
			_currentCapacity    = _settings.CurrentCapacity;

			InitButtons();
			UpdateView();
			CheckNeedTutorials();
		}

		private void CheckNeedTutorials() {
			if (!_tutorialController.IsTutorialComplete(TutorialId.FirstMineWindow) && 
			    _tutorialController.IsTutorialComplete(TutorialId.FirstMine)) {
				_tutorialController.StartTutorial(TutorialId.FirstMineWindow, FindTargetForTutor, FindClickTargetForTutor);
			}
		}
		
		private GameObject FindTargetForTutor() {
			var currentStepTargetId = _tutorialController.CurrentStepTargetId;
			if (currentStepTargetId == "MineTimer") {
				return _timerRoot.gameObject;
			}

			return null;
		}
		
		private GameObject FindClickTargetForTutor() {
			return null;
		}
		
		private void InitButtons() {
			_buttonClose.onClick.AddListener(Hide);
			_buttonIconClose.onClick.AddListener(Hide);
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
		
		private void UpdateView() {
			UpdateTimerView();
			
			var currencyInfo = GameController.Instance.DB.CurrencyDB.GetCurrencyInfo(_settings.CrystalType);
			if (currencyInfo != null)
				_crystalIcon.sprite = currencyInfo.Icon;

			UpdateProgress();
			InitInfoLabels();
			InitConditions();

			for (var i = 0; i < _forceUpdateLayouts.Count; ++i) {
				LayoutRebuilder.ForceRebuildLayoutImmediate(_forceUpdateLayouts[i]);
			}
		}

		private void UpdateTimerView() {
			var needTimer = _settings.IsUnlocked && _currentCapacity < _settings.MaxCapacity;
			_timerRoot.SetActive(needTimer);
		}
		
		private void UpdateProgress() {
			var progress = _currentCapacity / (float) _settings.MaxCapacity;
			_capacityProgress.SetProgress(progress);
		}

		private void InitInfoLabels() {
			_levelUpLabel.text = GameController.Instance.GetGameText("common_level") + $"{_settings.LevelUp}";


			_incomeLabel.text = GameController.Instance.GetGameText("mine_window_income") +
			                    $"{_settings.IncomeCount}/" +
			                    GameController.Instance.GetGameText("common_per_min");

			_labelNotControlHeader.text = GameController.Instance.GetGameText("mine_window_not_control").ToUpper();
			
			UpdateCapacityLabel();
		}

		private void UpdateCapacityLabel() {
			var capacityStr = GameController.Instance.GetGameText("mine_window_capacity");
			if (!_settings.IsUnlocked)
				capacityStr += $"0/{_settings.MaxCapacity}";
			else
				capacityStr += $"{_currentCapacity}/{_settings.MaxCapacity}";

			_capacityLabel.text = capacityStr;
		}

		private void Update() {
			UpdateTime();
		}

		private void UpdateTime() {
			if (_currentCapacity >= _settings.MaxCapacity)
				return;
			
			_currentTime += Time.deltaTime;
			if (_currentTime >= _produceTime) {
				_currentTime     =  0f;
				_currentCapacity += _settings.IncomeCount;
				
				UpdateCapacityLabel();
				UpdateProgress();
				UpdateTimerView();
			}

			UpdateTimeLeftLabel();
		}

		private void UpdateTimeLeftLabel() {
			var secondsLeft = _produceTime - _currentTime;
			_timeLeftLabel.text = Utility.GetTimeStr((int) secondsLeft);
		}

		private void InitConditions() {
			if (_settings.IsUnlocked) {
				_conditionsRoot.SetActive(false);
				ForceDoneConditions();
				return;
			}

			var condition1Icon  = _settings.IsAvailable ? _conditionDoneSp : _conditionNoneSp;
			var condition1Color = _settings.IsAvailable ? _conditionDoneColor : _conditionNoneColor;
			_condition1Icon.sprite = condition1Icon;
			_condition1Label.text  = GameController.Instance.GetGameText("mine_window_condition_1");
			_condition1Label.color = condition1Color;

			_condition2Icon.sprite = _conditionNoneSp;
			_condition2Label.text  = GameController.Instance.GetGameText("mine_window_condition_2");
		}
		
		private void ForceDoneConditions() {
			if (!_settings.NeedDoneConditions)
				return;
			
			_conditionsRoot.SetActive(true);
			
			_condition1Icon.sprite = _conditionDoneSp;
			_condition1Label.text  = GameController.Instance.GetGameText("mine_window_condition_1");
			_condition1Label.color = _conditionDoneColor;
			
			_condition2Icon.sprite = _conditionDoneSp;
			_condition2Label.text  = GameController.Instance.GetGameText("mine_window_condition_2");
			_condition2Label.color = _conditionDoneColor;
			
			_labelNotControlHeader.text  = GameController.Instance.GetGameText("mine_window_control");
			_labelNotControlHeader.color = _conditionDoneColor;
			
			_buttonInfo.gameObject.SetActive(false);
		}
	}

	public class MineWS : BaseWindowSettings {
		public bool               IsUnlocked;
		public bool               IsAvailable;
		public int                LevelUp;
		public int                IncomeCount;
		public int                CurrentCapacity;
		public int                MaxCapacity;
		public float              CurrentTime;
		public float              ProduceTime;
		public PlayerCurrencyType CrystalType;
		public bool               NeedDoneConditions;
	}
}
