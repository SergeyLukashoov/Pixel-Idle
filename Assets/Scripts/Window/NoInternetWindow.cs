using Core.Controllers.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class NoInternetWindow : BaseWindow<NoInternetWS> {
		public override WindowType Type => WindowType.NoInternetWindow;

		[SerializeField] private Button _buttonOk;
		[SerializeField] private Button _buttonClose;

		private void Awake() {
			InitButtons();
		}

		private void InitButtons() {
			_buttonOk.onClick.AddListener(Hide);
			_buttonClose.onClick.AddListener(Hide);
		}
	}

	public class NoInternetWS : BaseWindowSettings {
	}
}
