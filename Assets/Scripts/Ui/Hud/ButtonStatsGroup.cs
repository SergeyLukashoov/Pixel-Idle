using UnityEngine;
using UnityEngine.UI;

namespace Ui.Hud {
	public class ButtonStatsGroup : MonoBehaviour {
		[SerializeField] private Image  _image;
		[SerializeField] private Sprite _activeSp;
		[SerializeField] private Sprite _disabledSp;

		public void SetActive(bool isActive) {
			_image.sprite = isActive ? _activeSp : _disabledSp;
		}
	}
}
