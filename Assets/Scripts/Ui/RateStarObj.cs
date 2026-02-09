using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ui {
	public class RateStarObj : MonoBehaviour {
		[SerializeField] private Button     _btnSelf;
		[SerializeField] private GameObject _starObj;

		public Action<RateStarObj> OnClickAction;

		private void Start() {
			_starObj.SetActive(false);
			_btnSelf.onClick.AddListener(OnClick);
		}

		private void OnClick() {
			SetActiveState(true);
			OnClickAction?.Invoke(this);
		}

		public void SetActiveState(bool isActive) {
			_starObj.SetActive(isActive);
		}
	}
}
