using System;
using UnityEngine;

namespace Core.Fade {
	public class GlobalFade : MonoBehaviour {
		[SerializeField] private CanvasGroup _fadeCG;
		[SerializeField] private Canvas      _canvas;

		public void SetMainCamera(Camera cam) {
			_canvas.worldCamera = cam;
		}
		
		public void SetAlpha(float alpha) {
			_fadeCG.alpha = alpha;
		}

		public void StartFadeToAlpha(float alpha, Action onComplete) {
			LeanTween.alphaCanvas(_fadeCG, alpha, 0.2f).setOnComplete(
				() => { onComplete?.Invoke(); });
		}
	}
}
