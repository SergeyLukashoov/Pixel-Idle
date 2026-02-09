using Config.Player;
using Core.Controllers;
using Core.Controllers.Windows;
using Game.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class StatHintWindow : BaseWindow<StatHintWS> {
		public override WindowType Type => WindowType.StatHintWindow;

		[SerializeField] private Button _closeButton;
		[SerializeField] private Button _iconCloseButton;

		[SerializeField] private TextMeshProUGUI _headerLabel;
		[SerializeField] private TextMeshProUGUI _infoLabel;
		[SerializeField] private TextMeshProUGUI _currentLevelLabel;
		[SerializeField] private TextMeshProUGUI _maxLevelLabel;

		private void Awake() {
			_closeButton.onClick.AddListener(Hide);
			_iconCloseButton.onClick.AddListener(Hide);
		}

		private void Start() {
			InitLabels();
		}

		private void InitLabels() {
			_headerLabel.text       = GameController.Instance.GetGameText(_settings?.StatInfo._type + "_name");
			_infoLabel.text         = GameController.Instance.GetGameText(_settings?.StatInfo._type + "_hint_info");
			_currentLevelLabel.text = GameController.Instance.GetGameText("hint_window_current_level") + (_settings?.StatInfo._levelUp + 1);

			var maxLvl = _settings?.StatConfig.MaxValue / _settings?.StatConfig.UpgradeStep; 
			_maxLevelLabel.text     = GameController.Instance.GetGameText("hint_window_max_level") + maxLvl;

			if (maxLvl == 0)
				_maxLevelLabel.gameObject.SetActive(false);
		}
	}

	public class StatHintWS : BaseWindowSettings {
		public StatInfo         StatInfo;
		public PlayerStatConfig StatConfig;
	}
}
