using System;
using Core.Controllers;
using Core.Controllers.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class ReviveWindow : BaseWindow<ReviveWS> {
		public override WindowType Type => WindowType.ReviveWindow;

		[SerializeField] private Button          _buttonClose;
		[SerializeField] private Button          _buttonIconClose;
		[SerializeField] private Button          _buttonRevive;
		[SerializeField] private Image           _progressFill;
		[SerializeField] private TextMeshProUGUI _timerLabel;

		private float _maxTimer = 10f;
		private float _currentTimer;

		private bool _needUpdateTimer = true;
		private bool _onShowAds       = false;
		
		private void Awake() {
			InitButtons();

			_currentTimer = _maxTimer;
			UpdateTimerVisual();
		}

		private void InitButtons() {
			_buttonRevive.onClick.AddListener(OnReviveClick);
			_buttonClose.onClick.AddListener(OnCloseClick);
			_buttonIconClose.onClick.AddListener(OnCloseClick);
		}

		private void OnReviveClick() {
			if (!GameController.Instance.IsInternetAvailable) {
				GameController.Instance.WindowsController.ShowNoInternetWindow();
				return;
			}
			
			if (OnClose)
				return;
			
			_needUpdateTimer = false;
			_onShowAds       = true;
		}

		private void OnGetReward() {
			_onShowAds = false;
			_settings.OnRevive?.Invoke();
			Hide();
		}
		
		private void OnSkipReward() {
			_onShowAds       = false;
			_needUpdateTimer = true;
		}
		
		private void OnCloseClick() {
			if (_onShowAds)
				return;
			
			OnCloseAction();
		}

		private void OnCloseAction() {
			_onShowAds = false;
			_settings.OnClose?.Invoke();
			Hide();
		}

		private void Update() {
			UpdateTimer();
		}

		private void UpdateTimer() {
			if (GameController.Instance.WindowsController.OpenedWindowCount > 1)
				return;
			
			if (!_needUpdateTimer)
				return;
			
			if (_currentTimer <= 0f)
				return;

			_currentTimer -= Time.deltaTime;

			if (_currentTimer <= 0f) {
				_currentTimer = 0f;
				OnCloseAction();
			}

			UpdateTimerVisual();
		}

		private void UpdateTimerVisual() {
			_timerLabel.text = ((int) _currentTimer).ToString();
			var progress = _currentTimer / _maxTimer;
			_progressFill.fillAmount = progress;
		}
	}

	public class ReviveWS : BaseWindowSettings {
		public Action OnRevive;
		public Action OnClose;
	}
}
