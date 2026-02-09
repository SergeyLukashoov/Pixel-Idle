using System;
using Core.Controllers;
using Core.Controllers.Windows;
using Game.Player;
using TMPro;
using Tools.Editor.Builder;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class SettingsWindow : BaseWindow<SettingsWS> {
		public override WindowType Type => WindowType.SettingsWindow;

		[SerializeField] private Button          _buttonClose;
		[SerializeField] private Button          _buttonIconClose;
		[SerializeField] private Button          _buttonRestorePurchase;
		[SerializeField] private Button          _buttonMusic;
		[SerializeField] private Button          _buttonHome;
		[SerializeField] private Button          _buttonResume;
		[SerializeField] private GameObject      _buttonsRoot;
		[SerializeField] private GameObject      _labelOn;
		[SerializeField] private GameObject      _labelOff;
		[SerializeField] private Image           _toggleMusicBack;
		[SerializeField] private GameObject      _toggleMusicObject;
		[SerializeField] private Color           _toggleMusicColorActive;
		[SerializeField] private Color           _toggleMusicColorDisable;
		[SerializeField] private RectTransform   _layoutForRebuild;
		[SerializeField] private TextMeshProUGUI _versionLabel;

		private IPlayer _player;

		private void Start() {
			_player = GameController.Instance.Player;

			InitButtons();
			InitVersionLabel();
			UpdateView();
			
			LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutForRebuild);
		}

		private void UpdateView() {
			var needBtns = _settings.NeedButtons;
#if UNITY_IOS
			if (!needBtns) {
				needBtns = true;
				_buttonHome.gameObject.SetActive(false);
				_buttonResume.gameObject.SetActive(false);
			}
#endif
			_buttonRestorePurchase.gameObject.SetActive(false);
			_buttonsRoot.SetActive(needBtns);
		}
		
		private void InitVersionLabel() {
			var buildConfig = Resources.Load<BuilderConfig>("Config/BuildConfig");
			if (!buildConfig) {
				Debug.LogError("Can't load build config!");
				return;
			}

#if UNITY_ANDROID
			_versionLabel.text = $"ver. {buildConfig.GlobalVersion}.{buildConfig.BundleVersion - 1}";
#elif UNITY_IOS
			_versionLabel.text = $"ver. {buildConfig.IosBuildVersion}.{buildConfig.IosBundleVersion}";
#endif
		}

		private void InitButtons() {
			_buttonClose.onClick.AddListener(OnResumeClick);
			_buttonIconClose.onClick.AddListener(OnResumeClick);
			_buttonMusic.onClick.AddListener(OnMusicClick);
			_buttonHome.onClick.AddListener(OnHomeClick);
			_buttonResume.onClick.AddListener(OnResumeClick);
			_buttonRestorePurchase.onClick.AddListener(OnRestorePurchaseClick);

			SetMusicButtonState(_player.Flags.NeedMusic);
		}

		private void OnRestorePurchaseClick() {
			if (GameController.Instance.IsInternetAvailable) {
			}
			else {
				GameController.Instance.WindowsController.ShowNoInternetWindow();
			}
		}
		
		private void OnHomeClick() {
			Hide();
			_settings.OnHomeClick?.Invoke();
		}

		private void OnResumeClick() {
			Hide();
		}

		private void SetMusicButtonState(bool needMusic) {
			_labelOn.SetActive(needMusic);
			_labelOff.SetActive(!needMusic);

			_toggleMusicBack.color = needMusic ? _toggleMusicColorActive : _toggleMusicColorDisable;

			var toggleBackSize = _toggleMusicBack.GetComponent<RectTransform>().sizeDelta;
			var toggleSize     = _toggleMusicObject.GetComponent<RectTransform>().sizeDelta;

			var pos = _toggleMusicObject.transform.localPosition;
			if (needMusic)
				pos.x = toggleBackSize.x / 2f - toggleSize.x / 2f;
			else
				pos.x = -toggleBackSize.x / 2f + toggleSize.x / 2f;

			pos.y                                      = 0;
			_toggleMusicObject.transform.localPosition = pos;
		}

		private void OnMusicClick() {
			_player.Flags.NeedMusic = !_player.Flags.NeedMusic;
			SetMusicButtonState(_player.Flags.NeedMusic);
			GameController.Instance.MusicController.UpdateNeedPlay(_player.Flags.NeedMusic);
		}
	}

	public class SettingsWS : BaseWindowSettings {
		public bool   NeedButtons;
		public Action OnHomeClick;
		public Action OnRestoreClick;
	}
}
