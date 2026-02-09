using System;
using Core.Controllers;
using Game.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Cards {
	public class CardView : MonoBehaviour {
		[SerializeField] private Image           _cardIcon;
		[SerializeField] private TextMeshProUGUI _labelCount;
		[SerializeField] private Button          _buttonApplay;
		[SerializeField] private Button          _buttonRemove;

		private CardInfo   _info;
		private CardConfig _config;

		private Action<CardView> OnApplayAction;
		private Action<CardView> OnRemoveAction;

		public CardType Type => _info._type;
		
		private void Awake() {
			_buttonApplay.onClick.AddListener(OnApplayClick);
			_buttonRemove.onClick.AddListener(OnRemoveClick);
		}

		private void OnApplayClick() {
			GameController.Instance.CardsController.ApplayCard(_info._type);
			OnApplayAction?.Invoke(this);
		}

		private void OnRemoveClick() {
			GameController.Instance.CardsController.RemoveCard(_info._type);
			OnRemoveAction?.Invoke(this);
		}

		public void Initialize(CardInfo info, Action<CardView> onApplayAction, Action<CardView> onRemoveAction) {
			_info          = info;
			_config        = GameController.Instance.DB.CardsDB.GetCardConfig(_info._type);
			OnApplayAction = onApplayAction;
			OnRemoveAction = onRemoveAction;

			UpdateView();
		}

		private void UpdateView() {
			_cardIcon.sprite = _config._icon;
			_labelCount.text = _info._count.ToString();
		}

		public void SetInCollectionState(bool isHaveFreeSlot) {
			_buttonApplay.gameObject.SetActive(isHaveFreeSlot);
			_buttonRemove.gameObject.SetActive(false);
		}
		
		public void SetInActiveState() {
			_buttonApplay.gameObject.SetActive(false);
			_buttonRemove.gameObject.SetActive(true);
		}
	}
}
