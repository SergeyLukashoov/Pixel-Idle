using System;
using UnityEngine;

namespace Game.Map {
	public class MineCountLabel : MonoBehaviour {
		public Action OnClickAction { get; set; }

		public void OnMouseDown() {
			OnClickAction?.Invoke();
		}
	}
}
