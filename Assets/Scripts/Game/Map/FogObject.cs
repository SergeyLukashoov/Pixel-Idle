using UnityEngine;

namespace Game.Map {
	public class FogObject : MonoBehaviour {
		[SerializeField] private SpriteRenderer _spriteRenderer;

		public bool SetAlpha(float alpha, bool needTween = false, float delay = 0f) {
			var c = _spriteRenderer.color;
			if (c.a < alpha)
				return false;

			if (needTween) {
				LeanTween.value(gameObject, c.a, alpha, 0.1f).setOnUpdate(
					val => {
						c.a                   = val;
						_spriteRenderer.color = c;
					}).setDelay(delay);
			}
			else {
				c.a                   = alpha;
				_spriteRenderer.color = c;
			}

			return true;
		}
	}
}
