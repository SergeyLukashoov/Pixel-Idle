using UnityEngine;

namespace Ui.Screens.Upgrade {
	public class UpgradeTabBackSize : MonoBehaviour {
		[SerializeField] private RectTransform _backRT;

		private void Start() {
			_backRT.sizeDelta = new Vector2(Screen.width, Screen.height);
		}
	}
}
