using UnityEngine;
using UnityEngine.UI;

namespace Ui.Hud {
	public class HudButtonView : MonoBehaviour {
		[SerializeField] private bool          _locked;
		[SerializeField] private RectTransform _rt;
		[SerializeField] private Image         _back;
		[SerializeField] private Sprite        _activeSp;
		[SerializeField] private Sprite        _disabledSp;
		[SerializeField] private Vector2       _activeSize;
		[SerializeField] private Vector2       _disableSize;
		[SerializeField] private Button        _button;
		[SerializeField] private Image         _icon;
		[SerializeField] private Sprite        _activeIcon;
		[SerializeField] private Sprite        _lockIcon;

		private Vector3 _defaultIconPos;
		private Vector3 _activeIconPos;

		public Button Btn    => _button;
		public bool   Locked => _locked;
		private void Awake() {
			if (_locked)
				_button.interactable = false;
		}

		public void SetActive(bool isActive) {
			_button.interactable = !isActive;
			_back.sprite         = isActive ? _activeSp : _disabledSp;
			_rt.sizeDelta        = isActive ? _activeSize : _disableSize;
		}

		public void SetLockedState(bool isLocked) {
			_locked = isLocked;
			
			var sp = isLocked ? _lockIcon : _activeIcon;
			_icon.sprite = sp;
		}
	}
}
