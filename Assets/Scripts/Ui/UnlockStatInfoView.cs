using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui {
	public class UnlockStatInfoView : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI _label;
		[SerializeField] private Image           _image;

		public void SetColor(Color backColor, Color labelColor) {
			_label.color = labelColor;
			_image.color = backColor;
		}
	}
}
