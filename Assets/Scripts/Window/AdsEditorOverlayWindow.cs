using System;
using Core.Controllers.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class AdsEditorOverlayWindow : BaseWindow<AdsEditorOverlayWS> {
		public override WindowType Type => WindowType.AdsEditorOverlayWindow;

		[SerializeField] private Button _buttonShowed;
		[SerializeField] private Button _buttonSkip;

		private void Awake() {
			InitButtons();
		}

		private void InitButtons() {
			_buttonShowed.onClick.AddListener(OnShowedClick);
			_buttonSkip.onClick.AddListener(OnSkipClick);
		}

		private void OnShowedClick() {
			Hide();
			_settings.OnShowed?.Invoke();
		}

		private void OnSkipClick() {
			Hide();

			if (_settings.IsInterstitial)
				_settings.OnShowed?.Invoke();
			else
				_settings.OnSkip?.Invoke();
		}
	}

	public class AdsEditorOverlayWS : BaseWindowSettings {
		public bool   IsInterstitial;
		public Action OnShowed;
		public Action OnSkip;
	}
}
