using System.Collections.Generic;
using Core.Controllers;
using Core.Controllers.Windows;
using Game.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class UpgradeCardWindow : BaseWindow<UpgradeCardWS> {
		public override WindowType Type => WindowType.UpgradeCardWindow;

		[SerializeField] private Button          _buttonClose;
		
		[SerializeField] private TextMeshProUGUI _headerLabel;
		[SerializeField] private TextMeshProUGUI _nameLabel;
		[SerializeField] private TextMeshProUGUI _infoLabel;
		[SerializeField] private TextMeshProUGUI _levelUpLabel;
		
		[SerializeField] private Image _cardIcon;
		
		[SerializeField] private List<GameObject> _starsObj  = new List<GameObject>();
		[SerializeField] private List<GameObject> _starsLine = new List<GameObject>();

		private void Awake() {
			_buttonClose.onClick.AddListener(Hide);
		}

		private void Start() {
			_nameLabel.text    = GameController.Instance.GetGameText($"{_settings.Info._type}_name");

			var levelUp  = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(_settings.Info._count);
			var headerId = levelUp == 1 ? "card_unlocked" : "card_upgraded_header";
			_headerLabel.text  = GameController.Instance.GetGameText(headerId);
			
			var infoId = levelUp == 1 ? "card_unlocked_info" : "card_upgraded_info";
			_infoLabel.text = GameController.Instance.GetGameText(infoId);
			
			_levelUpLabel.text = GameController.Instance.GetGameText("common_level") + levelUp;

			var cardConfig = GameController.Instance.DB.CardsDB.GetCardConfig(_settings.Info._type);
			_cardIcon.sprite = cardConfig._icon;
			
			InitStars(levelUp);			
		}
		
		private void InitStars(int levelUp) {
			var neededLines    = 1;
			if (levelUp > 5)
				neededLines = 2;

			for (var i = 0; i < _starsLine.Count; ++i) {
				var isActive = i < neededLines;
				_starsLine[i].SetActive(isActive);
			}

			for (var i = 0; i < _starsObj.Count; ++i) {
				var isActive = i < levelUp;
				_starsObj[i].SetActive(isActive);
			}
		}
	}

	public class UpgradeCardWS : BaseWindowSettings {
		public CardInfo Info;
	}
}
