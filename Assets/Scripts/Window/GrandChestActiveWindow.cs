using System;
using System.Collections;
using System.Collections.Generic;
using Core.Controllers;
using Core.Controllers.Windows;
using Core.Utils;
using TMPro;
using Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class GrandChestActiveWindow : BaseWindow<GrandChestActiveWS> {
		public override WindowType Type => WindowType.GrandChestActiveWindow;

		[SerializeField] private Button          _buttonClose;
		[SerializeField] private Button          _buttonIconClose;
		[SerializeField] private ProgressBar     _capacityProgress;
		[SerializeField] private TextMeshProUGUI _capacityLabel;
		[SerializeField] private TextMeshProUGUI _incomeLabel;
		[SerializeField] private TextMeshProUGUI _timeLeftLabel;

		private float _currentTime;
		private float _produceTime;

		private void Start() {
			_currentTime = _settings.CurrentTime;
			_produceTime = _settings.ProduceTime;

			InitButtons();
			InitLabels();
			UpdateProgress();
			UpdateTimeLeftLabel();
		}

		private void UpdateProgress() {
			var progress = _settings.ResourcesLeft / (float)GameController.Instance.Config.GrandChestMaxResources;
			_capacityProgress.SetProgress(progress);
		}

		private void UpdateTimeLeftLabel() {
			var secondsLeft = _produceTime - _currentTime;
			_timeLeftLabel.text = Utility.GetTimeStr((int) secondsLeft);
		}

		private void Update() {
			UpdateTime();
		}

		private void UpdateTime() {
			if (_currentTime >= _produceTime)
				return;

			_currentTime += Time.deltaTime;
			if (_currentTime >= _produceTime) {
				_currentTime = _produceTime;
			}

			UpdateTimeLeftLabel();
		}

		private void InitButtons() {
			_buttonClose.onClick.AddListener(Hide);
			_buttonIconClose.onClick.AddListener(Hide);
		}

		private void InitLabels() {
			_capacityLabel.text = GameController.Instance.GetGameText("mine_window_capacity") +
			                      $"{_settings.ResourcesLeft}/{GameController.Instance.Config.GrandChestMaxResources}";

			_incomeLabel.text = GameController.Instance.GetGameText("mine_window_income") +
			                    $"{GameController.Instance.Config.GrandChestIncomeCount}/" +
			                    string.Format(GameController.Instance.GetGameText("common_per_custom_hour"), _produceTime / 60 / 60);
		}
	}

	public class GrandChestActiveWS : BaseWindowSettings {
		public int   ResourcesLeft;
		public float CurrentTime;
		public float ProduceTime;
	}
}
