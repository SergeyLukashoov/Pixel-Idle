using TMPro;
using UnityEngine;

namespace Ui {
	public class DamageView : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI _damageLabel;
		[SerializeField] private CanvasGroup     _cg;

		public void Init(string dmg, Color color) {
			_damageLabel.text  = dmg;
			_damageLabel.color = color;

			_cg.alpha = 0f;
		}

		public void StartAnim() {
			var posToMove = transform.position;
			posToMove.y += 150;

			LeanTween.alphaCanvas(_cg, 1f, 0.2f);
			LeanTween.moveY(gameObject, posToMove.y, 0.5f).setOnComplete(
				() => { LeanTween.alphaCanvas(_cg, 0f, 0.1f).setOnComplete(() => { Destroy(gameObject); }); });
		}
	}
}
