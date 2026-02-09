using System;
using Core.Controllers;
using Game.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace Ui {
	public class PassiveSkillIcon : MonoBehaviour {
		[SerializeField] private Image  _icon;
		[SerializeField] private Image  _iconAdd;
		[SerializeField] private Button _buttonSelf;

		[SerializeField] private Sprite _lockIconSp;
		[SerializeField] private Sprite _plusIconSp;

		public Action OnSkillClick { get; set; }

		private void Awake() {
			_buttonSelf.onClick.AddListener(OnClick);
		}

		private void OnClick() {
			OnSkillClick?.Invoke();
		}

		public void Initialize(CardType type) {
			if (type == CardType.Empty) {
				_icon.gameObject.SetActive(false);
				_iconAdd.sprite = _plusIconSp;
				_iconAdd.SetNativeSize();
			}
			else {
				_iconAdd.gameObject.SetActive(false);
				
				var cardData = GameController.Instance.DB.CardsDB.GetCardConfig(type);
				if (cardData != null)
					_icon.sprite = cardData._icon;
			}
		}

		public void InitLockedState() {
			_icon.gameObject.SetActive(false);
			
			_iconAdd.sprite = _lockIconSp;
			_iconAdd.SetNativeSize();
		}
	}
}
