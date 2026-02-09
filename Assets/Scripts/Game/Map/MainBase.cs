using System;
using System.Collections.Generic;
using Core.Controllers;
using Core.Controllers.Windows;
using UnityEngine;
using UnityEngine.EventSystems;
using Window;

namespace Game.Map {
	public class MainBase : MonoBehaviour {
		[SerializeField] private int _currentViewRange;

		private List<GameObject> _linkedFog = new List<GameObject>();
		
		private void OnMouseDown() {
			if (!GameController.Instance.IsCanClick())
				return;

			ShowInfoWindow();
		}

		private void ShowInfoWindow() {
			var settings = new InfoHeaderWS {
				HeaderStr   = GameController.Instance.GetGameText("main_base_header"),
				InfoStr     = GameController.Instance.GetGameText("main_base_info"),
				NeedContent = false,
				NeedHeader  = true
			};
			
			GameController.Instance.WindowsController.Show(WindowType.InfoHeaderWindow, settings);
		}
		
		public void RemoveFog() {
			var delta = GameController.Instance.Config.ViewRangeDeltaForFog;

			for (var i = 0; i < _linkedFog.Count; ++i) {
				var alpha = 0.7f;
				if (IsObjectInViewRange(_linkedFog[i].transform.position, delta / 2))
					alpha = 0.6f;

				if (IsObjectInViewRange(_linkedFog[i].transform.position)) {
					_linkedFog[i].SetActive(false);
					continue;
				}

				_linkedFog[i].GetComponent<FogObject>().SetAlpha(alpha);
			}
		}

		public bool IsObjectInViewRange(Vector3 objPos, int delta = 0) {
			var length = (transform.position - objPos).magnitude;
			if (length <= _currentViewRange + delta)
				return true;

			return false;
		}

		public void LinkFogObject(GameObject fogObject) {
			_linkedFog.Add(fogObject);
		}
	}
}
