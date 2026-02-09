using System;
using System.Collections.Generic;
using Core.Config.Store;
using UnityEngine;
using Window;

namespace Core.Controllers.Windows {
	public class WindowsController : MonoBehaviour {
		private const string           WindowPath     = "Ui/Windows/";
		private       List<GameObject> _openedWindows = new List<GameObject>();

		public Transform WindowsLayer      { get; set; }
		public int       OpenedWindowCount => _openedWindows.Count;

		public void Show<T>(WindowType type, T settings, bool needAnim = true, float delay = 0f) where T : BaseWindowSettings {
			GameController.Instance.BlockInput(true);

			var windowPrefab = Resources.Load(WindowPath + type) as GameObject;

			var windowObj = Instantiate(windowPrefab, WindowsLayer);
			windowObj.transform.localPosition = Vector3.zero;
			windowObj.transform.localScale    = Vector3.one;

			var window = windowObj.GetComponent<BaseWindow<T>>();
			window.SetSettings(settings);
			window.BeforeShow();
			window.OnShow = true;

			if (needAnim)
				ShowAnim(window, delay);
			else {
				GameController.Instance.BlockInput(false);
				window.OnShow = false;
			}

			_openedWindows.Add(windowObj);
		}

		private void ShowAnim<T>(BaseWindow<T> window, float delay) where T : BaseWindowSettings {
			if (window.Root) {
				window.gameObject.SetActive(false);
				window.Root.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
				LeanTween.scale(window.Root, Vector3.one, 0.2f).setEase(LeanTweenType.easeOutBack)
					.setDelay(delay)
					.setOnStart(() => { window.gameObject.SetActive(true); })
					.setOnComplete(() => {
						GameController.Instance.BlockInput(false);
						window.OnShow = false;
					});
			}
			else {
				GameController.Instance.BlockInput(false);
				window.OnShow = false;
			}
		}

		public void HideAnim(GameObject root, Action onComplete) {
			if (root) {
				LeanTween.scale(root, new Vector3(0.9f, 0.9f, 1f), 0.2f).setEase(LeanTweenType.easeInBack).setOnComplete(
					() => { onComplete?.Invoke(); });
			}
			else {
				onComplete?.Invoke();
			}
		}

		public void RemoveWindow(GameObject window) {
			if (_openedWindows.Contains(window))
				_openedWindows.Remove(window);
		}

		public BaseWindow<T> GetActiveWindow<T>(WindowType type) where T : BaseWindowSettings {
			for (var i = 0; i < _openedWindows.Count; ++i) {
				var window = _openedWindows[i].GetComponent<BaseWindow<T>>();
				if (window && window.Type == type)
					return window;
			}

			return null;
		}

		public void ShowNoInternetWindow() {
			var settings = new NoInternetWS();
			Show(WindowType.NoInternetWindow, settings);
		}

		public void ShowNoAdsWindow(GameObject noAdsButton) {
			if (GameController.Instance.Player.Flags.NonConsumableItems.Contains(StoreProductNames.NO_ADS))
				return;
			
			var settings = new NoAdsWS();
			settings.NoAdsIcon = noAdsButton;
			
			Show(WindowType.NoAdsWindow, settings);
		}
		
		public void ShowWaitWindow() {
			var settings = new WaitWS();
			Show(WindowType.WaitWindow, settings, false);
		}
		
		public void CloseWaitWindow() {
			var waitWindow = GetActiveWindow<WaitWS>(WindowType.WaitWindow);
			if (waitWindow == null) {
				return;
			}
			
			waitWindow.Hide();
		}

		public void ShowInfoDialog(string infoId) {
			var settings = new InfoHeaderWS {
				HeaderStr   = "",
				InfoStr     = GameController.Instance.GetGameText(infoId),
				NeedHeader  = false,
				NeedContent = false,
			};

			GameController.Instance.WindowsController.Show(WindowType.InfoHeaderWindow, settings);
		}
		
		public void ShowInfoDialogWHeader(string headerId, string infoId) {
			var settings = new InfoHeaderWS {
				HeaderStr   = GameController.Instance.GetGameText(headerId),
				InfoStr     = GameController.Instance.GetGameText(infoId),
				NeedHeader  = true,
				NeedContent = false,
			};

			GameController.Instance.WindowsController.Show(WindowType.InfoHeaderWindow, settings);
		}
		
		public void ShowRateUsDialog() {
			var settings = new RateUsWS {
			};

			GameController.Instance.WindowsController.Show(WindowType.RateUsWindow, settings);
		}
	}
}
