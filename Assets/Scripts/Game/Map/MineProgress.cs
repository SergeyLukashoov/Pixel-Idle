using Game.Player;
using UnityEngine;

namespace Game.Map {
	public class MineProgress : MonoBehaviour {
		[SerializeField] private SpriteRenderer _fillSprite;

		[SerializeField] private Sprite _greenFill;
		[SerializeField] private Sprite _redFill;

		private Vector2 _maxSize;
		
		public void Initialize(PlayerCurrencyType type) {
			_maxSize = _fillSprite.size;
			
			var neededSprite = _redFill;
			if (type == PlayerCurrencyType.GreenCrystal)
				neededSprite = _greenFill;

			_fillSprite.sprite = neededSprite;
		} 
		
		public void SetProgress(float progress) {
			var currSize = _maxSize.x * progress;
			_fillSprite.size = new Vector2(currSize, _maxSize.y);
		}
	}
}
