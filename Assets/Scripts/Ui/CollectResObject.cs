using TMPro;
using UnityEngine;

namespace Ui {
	public class CollectResObject : MonoBehaviour {
		[SerializeField] private SpriteRenderer _resIcon;
		[SerializeField] private TextMeshPro    _labelResCount;

		public void Initialize(Sprite resIcon, int count) {
			_resIcon.sprite     = resIcon;
			_labelResCount.text = $"+{count}";
		}
	}
}
