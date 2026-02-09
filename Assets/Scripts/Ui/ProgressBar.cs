using UnityEngine;
using UnityEngine.UI;

namespace Ui {
	public class ProgressBar : MonoBehaviour {
		[SerializeField] private Image _fillImage;

		public void SetProgress(float progress, bool needTween = false) {
			if (needTween) {
				LeanTween.value(gameObject, _fillImage.fillAmount, progress, 0.1f).setOnUpdate(
					val => { _fillImage.fillAmount = val; });
			}
			else
				_fillImage.fillAmount = progress;
		}
	}
}
