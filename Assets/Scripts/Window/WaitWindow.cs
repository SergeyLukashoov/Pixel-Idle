using Core.Controllers.Windows;
using UnityEngine;

namespace Window {
	public class WaitWindow : BaseWindow<WaitWS>
	{
		public override WindowType Type => WindowType.WaitWindow;

		[SerializeField] private Transform _waitIcon;

		private void Update() {
			UpdateWaitIcon();
		}

		private void UpdateWaitIcon() {
			_waitIcon.Rotate(new Vector3(0f, 0f, 1f), -5f);
		}
	}
	
	public class WaitWS : BaseWindowSettings {
	}
}
