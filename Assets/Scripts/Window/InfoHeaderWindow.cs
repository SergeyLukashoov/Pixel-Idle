using System.Collections.Generic;
using Core.Config.Chests;
using Core.Controllers;
using Core.Controllers.Windows;
using TMPro;
using Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class InfoHeaderWindow : BaseWindow<InfoHeaderWS> {
		public override WindowType Type => WindowType.InfoHeaderWindow;

		[SerializeField] private Button              _buttonClose;
		[SerializeField] private Button              _buttonIconClose;
		[SerializeField] private Button              _buttonInfo;
		[SerializeField] private TextMeshProUGUI     _labelHeader;
		[SerializeField] private TextMeshProUGUI     _labelInfo;
		[SerializeField] private List<RectTransform> _layersForRebuild;
		[SerializeField] private GameObject          _contentRoot;
		[SerializeField] private GameObject          _rewardPrefab;

		private void Start() {
			_buttonClose.onClick.AddListener(Hide);
			_buttonIconClose.onClick.AddListener(Hide);
			_buttonInfo.onClick.AddListener(OnInfoClick);

			_labelHeader.text = _settings.HeaderStr.ToUpper();
			_labelInfo.text   = _settings.InfoStr;

			if (!_settings.NeedHeader)
				_labelHeader.gameObject.SetActive(false);

			BuildContent();

			for (var i = 0; i < _layersForRebuild.Count; ++i)
				LayoutRebuilder.ForceRebuildLayoutImmediate(_layersForRebuild[i]);
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

		private void BuildContent() {
			if (!_settings.NeedContent) {
				_contentRoot.SetActive(false);
				return;
			}

			InitRewards();
		}

		private void InitRewards() {
			for (var i = 0; i < _settings.ChestContent.Count; ++i) {
				var reward = _settings.ChestContent[i];

				var rewardObj = Instantiate(_rewardPrefab, _contentRoot.transform);
				rewardObj.GetComponent<RewardObject>().Initialize(reward.Type, reward.Count);
			}
		}
	}

	public class InfoHeaderWS : BaseWindowSettings {
		public string           HeaderStr;
		public string           InfoStr;
		public bool             NeedHeader;
		public bool             NeedContent;
		public List<RewardInfo> ChestContent;
	}
}
