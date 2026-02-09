using Core.Controllers;
using Game.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui {
	public class DropView : MonoBehaviour {
		[SerializeField] private Image           _icon;
		[SerializeField] private TextMeshProUGUI _countLabel;
		[SerializeField] private CanvasGroup     _cg;

		public void Init(int count, PlayerCurrencyType type) {
			_countLabel.text = $"{count}";
			_cg.alpha        = 0f;
			_icon.sprite     = GameController.Instance.DB.CurrencyDB.GetCurrencyInfo(type).Icon;
		}
		
		public void StartAnim(float animDelay) {
			var posToMove = transform.position;
			posToMove.y += 100;

			LeanTween.alphaCanvas(_cg, 1f, 0.2f).setDelay(animDelay);
			LeanTween.moveY(gameObject, posToMove.y, 0.8f).setOnComplete(
				() => {
					LeanTween.alphaCanvas(_cg, 0f, 0.1f).setOnComplete(() => { Destroy(gameObject);});
				}).setDelay(animDelay);
		}
	}
}
