using Core.Controllers;
using Core.Controllers.Windows;
using UnityEngine;

namespace Window {
	public class BaseWindow<T> : MonoBehaviour where T : BaseWindowSettings {
		[SerializeField] protected GameObject _root;

		public bool       OnShow { get; set; }
		public bool       OnClose { get; set; }
		public GameObject Root   => _root;

		protected T _settings;

		public virtual WindowType Type => WindowType.EndRunWindow;

		public virtual void BeforeShow() {

		}

		public void SetSettings(T settings) {
			_settings = settings;
		}

		public void Hide() {
			if (OnShow)
				return;

			OnClose = true;
			GameController.Instance.WindowsController.HideAnim(_root, () => {
				_settings.OnHide?.Invoke();
					
				Destroy(gameObject);
				GameController.Instance.WindowsController.RemoveWindow(gameObject);
			});
		}
	}
}
