using Core;
using Core.Controllers;
using Game.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui {
	public class CheaterPanel : MonoBehaviour {
		[SerializeField] private Button _buttonClose;
		[SerializeField] private Button _buttonShowEnemiesStats;
		[SerializeField] private Button _buttonFreeUpgrades;
		[SerializeField] private Button _buttonIsGodMode;
		[SerializeField] private Button _buttonPlayTestAcceleration;
		[SerializeField] private Button _buttonSaveReset;
		[SerializeField] private Button _buttonAddRedCrystal;
		[SerializeField] private Button _buttonAddGreenCrystal;
		[SerializeField] private Button _buttonAddCoins;

		[SerializeField] private TMP_InputField _redCrystalCountInput;
		[SerializeField] private TMP_InputField _greenCrystalCountInput;
		[SerializeField] private TMP_InputField _coinsCountInput;
		
		[SerializeField] private TextMeshProUGUI _labelShowEnemiesStats;
		[SerializeField] private TextMeshProUGUI _labelFreeUpgrades;
		[SerializeField] private TextMeshProUGUI _labelGodMode;
		[SerializeField] private TextMeshProUGUI _labelPlayTestAcceleration;

		public bool IsShowed { get; set; }

		private void Awake() {
			InitButtons();
		}

		private void InitButtons() {
			_buttonClose.onClick.AddListener(OnCloseClick);
			_buttonSaveReset.onClick.AddListener(OnSaveResetClick);

			_buttonShowEnemiesStats.onClick.AddListener(OnShowEnemiesStatsClick);
			_labelShowEnemiesStats.text = CheaterController.NeedShowEnemyStats ? "on" : "off";

			_buttonFreeUpgrades.onClick.AddListener(OnFreeUpgradesClick);
			_labelFreeUpgrades.text = CheaterController.NeedFreeUpgrades ? "on" : "off";

			_buttonIsGodMode.onClick.AddListener(OnGodModeClick);
			_labelGodMode.text = CheaterController.IsGodMode ? "on" : "off";

			_buttonPlayTestAcceleration.onClick.AddListener(OnPlayTestAccelerationClick);
			_labelPlayTestAcceleration.text = MyTime.maxAcceleration == 5 ? "on" : "off";
			
			_redCrystalCountInput.text = "0";
			_buttonAddRedCrystal.onClick.AddListener(OnAddRedCrystalClick);
			
			_greenCrystalCountInput.text = "0";
			_buttonAddGreenCrystal.onClick.AddListener(OnAddGreenCrystalClick);
			
			_coinsCountInput.text = "0";
			_buttonAddCoins.onClick.AddListener(OnAddCoinsClick);
		}

		private void OnAddCoinsClick() {
			var str = _coinsCountInput.text;
			if (int.TryParse(str, out var count)) {
				GameController.Instance.Player.ChangeCoinsCount(count, "cheater");
				_coinsCountInput.text = "0";
			}
		}
		
		private void OnAddRedCrystalClick() {
			var str   = _redCrystalCountInput.text;
			if (int.TryParse(str, out var count)) {
				GameController.Instance.Player.ChangeCurrency(PlayerCurrencyType.RedCrystal, count, "cheater");
				_redCrystalCountInput.text = "0";
			}
		}
		
		private void OnAddGreenCrystalClick() {
			var str = _greenCrystalCountInput.text;
			if (int.TryParse(str, out var count)) {
				GameController.Instance.Player.ChangeCurrency(PlayerCurrencyType.GreenCrystal, count, "cheater");
				_greenCrystalCountInput.text = "0";
			}
		}
		
		private void OnGodModeClick() {
			var godMode = CheaterController.IsGodMode;
			CheaterController.IsGodMode = !godMode;
			_labelGodMode.text          = CheaterController.IsGodMode ? "on" : "off";
		}

		private void OnPlayTestAccelerationClick() {
			var maxAcceleration = MyTime.maxAcceleration;
			if (maxAcceleration == 2)
				MyTime.maxAcceleration = 5;
			else
				MyTime.maxAcceleration = 2;

			_labelPlayTestAcceleration.text = MyTime.maxAcceleration == 5 ? "on" : "off";
		}

		private void OnCloseClick() {
			Hide();
		}

		private void OnSaveResetClick() {
			Hide();
			GameController.Instance.ClearSave();
			SceneController.LoadMenu();
		}

		public void Show() {
			IsShowed = true;
			gameObject.SetActive(true);
		}

		public void Hide() {
			IsShowed = false;
			gameObject.SetActive(false);
		}

		private void OnShowEnemiesStatsClick() {
			var needShow = CheaterController.NeedShowEnemyStats;
			CheaterController.NeedShowEnemyStats = !needShow;
			_labelShowEnemiesStats.text          = CheaterController.NeedShowEnemyStats ? "on" : "off";
		}

		private void OnFreeUpgradesClick() {
			var needFree = CheaterController.NeedFreeUpgrades;
			CheaterController.NeedFreeUpgrades = !needFree;
			_labelFreeUpgrades.text            = CheaterController.NeedFreeUpgrades ? "on" : "off";

			GameController.Instance.Board.UpdateUpgradeButtons();
		}
	}
}
