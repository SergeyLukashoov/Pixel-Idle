using Core.Controllers;
using Core.Controllers.Windows;
using Game.Cards;
using TMPro;
using Ui.Screens.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class CardHintWindow : BaseWindow<CardHintWS> {
		public override WindowType Type => WindowType.StatHintWindow;

		[SerializeField] private Button   _buttonClose;
		[SerializeField] private Button   _buttonIconClose;
		[SerializeField] private CardIcon _cardIcon;

		[SerializeField] private TextMeshProUGUI _cardRarityLabel;
		[SerializeField] private TextMeshProUGUI _cardHintLabel;
		[SerializeField] private TextMeshProUGUI _currentLevelLabel;
		[SerializeField] private TextMeshProUGUI _currentLevelValueLabel;
		[SerializeField] private TextMeshProUGUI _nextLevelValueLabel;
		[SerializeField] private TextMeshProUGUI _cardsCountLabel;

		private void Awake() {
			_buttonClose.onClick.AddListener(Hide);
			_buttonIconClose.onClick.AddListener(Hide);
		}

		private void Start() {
			_cardIcon.Init(_settings._cardSlot);
			_cardIcon.SetHintState();

			_cardRarityLabel.text = _settings._cardSlot.Config._rarity.ToString();

			var currCount = _settings._cardSlot.Info._count;

			var currentLevel = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(currCount);
			var currentValue = _settings._cardSlot.Config.GetCardValueByLvlUp(currentLevel - 1);
			var addValue = _settings._cardSlot.Config.GetCardAddValueByLvlUp(currentLevel - 1);
			var nextValue    = _settings._cardSlot.Config.GetCardValueByLvlUp(currentLevel);
			var isMaxLevel   = GameController.Instance.DB.CardsDB.IsCardMaxLevelUp(currCount);
			var hintInfo     = GameController.Instance.GetGameText($"{_settings._cardSlot?.Config._type}_hint");

			var hintLabelStr = "";
			if (_settings._cardSlot?.Config._type == CardType.Multishot)
				hintLabelStr = string.Format(hintInfo, addValue, currentValue);
			else
				hintLabelStr = string.Format(hintInfo, currentValue);

			_cardHintLabel.text          = hintLabelStr;
			_currentLevelLabel.text      = GameController.Instance.GetGameText("card_hint_window_current_level") + currentLevel;
			_currentLevelValueLabel.text = GetValueStr(currentValue);
			_nextLevelValueLabel.text    = isMaxLevel ? "Max." : GetValueStr(nextValue);

			var nextLevelCount = GameController.Instance.DB.CardsDB.GetCurrentLevelUpCount(currentLevel + 1);
			var nextLevelAllCount = GameController.Instance.DB.CardsDB.GetAllLevelUpCount(currentLevel + 1);
			_cardsCountLabel.text = isMaxLevel ? "Max." : $"{nextLevelCount - (nextLevelAllCount - currCount)}/{nextLevelCount}";
		}

		private string GetValueStr(float value) {
			if (_settings._cardSlot.Config._type == CardType.CoinsBoost || _settings._cardSlot.Config._type == CardType.WaveSkip ||
			    _settings._cardSlot.Config._type == CardType.AttackUp || _settings._cardSlot.Config._type == CardType.HealthUp ||
			    _settings._cardSlot.Config._type == CardType.Multishot || _settings._cardSlot.Config._type == CardType.VampireBite || 
			    _settings._cardSlot.Config._type == CardType.Fortification)
				return $"{value}%";
			else if (_settings._cardSlot.Config._type == CardType.Revive)
				return $"{value} s.";

			return value.ToString();
		}
	}

	public class CardHintWS : BaseWindowSettings {
		public CardSlotView _cardSlot;
	}
}
