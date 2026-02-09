using System.Collections.Generic;
using Core.Config.Tutorial;
using Game.Map;
using Ui.Hud;
using UnityEngine;

namespace Core.Controllers {
	public class ScreensController : MonoBehaviour {
		[SerializeField] private RectTransform    _mainCanvas;
		[SerializeField] private MapObject        _mapObject;
		[SerializeField] private HudTop           _hudTopObject;
		[SerializeField] private List<GameObject> _screens = new List<GameObject>();
		

		private int  _currentScreenId = 0;
		private bool _onChangeScreen  = false;
		public  bool OnChangeScreen  => _onChangeScreen;
		public  int  CurrentScreenId => _currentScreenId;

		private void Start() {
			BuildContainer();
			MoveToScreen(1, false);
		}

		private void BuildContainer() {
			var containerWidth  = _mainCanvas.sizeDelta.x * _screens.Count;
			var containerHeight = _mainCanvas.sizeDelta.y;
			transform.GetComponent<RectTransform>().sizeDelta = new Vector2(containerWidth, containerHeight);

			SetScreensSize();
			SetScreensPos(containerWidth);
		}

		private void SetScreensSize() {
			for (var i = 0; i < _screens.Count; ++i) {
				var rt = _screens[i].GetComponent<RectTransform>();
				rt.sizeDelta = _mainCanvas.sizeDelta;
			}
		}

		private void SetScreensPos(float containerWidth) {
			var neededPos = -containerWidth / 2f;
			for (var i = 0; i < _screens.Count; ++i) {
				var rt = _screens[i].GetComponent<RectTransform>();
				_screens[i].transform.localPosition =  new Vector3(neededPos + rt.sizeDelta.x / 2f, 0, 0f);
				neededPos                           += rt.sizeDelta.x;
			}
		}

		public void MoveToScreen(int screenIdx, bool needTween) {
			if (screenIdx >= _screens.Count || screenIdx < 0) {
				Debug.LogError($"Wrong screen id = {screenIdx}!");
				return;
			}

			_onChangeScreen = true;

			var neededScreen    = _screens[screenIdx];
			var neededScreenPos = neededScreen.transform.position;
			var currentPos      = transform.position;

			_hudTopObject.SetZoomButtonVisible(screenIdx == 1);
			_hudTopObject.SetNoAdsButtonVisible(screenIdx == 1);

			var mapShiftDelta = _currentScreenId - screenIdx;
			var mapMoveDelta  = 100f * mapShiftDelta;
			//if (neededScreenPos.x > 0f)
			//mapMoveDelta = -100f;

			if (needTween) {
				LeanTween.moveX(gameObject, currentPos.x - neededScreenPos.x, 0.1f).setOnComplete(
					() => {
						_onChangeScreen  = false;
						_currentScreenId = screenIdx;

						TryShowInterstitial();
						TryOpenTowerWindow();
					});

				MoveMapObj(mapMoveDelta);
			}
			else {
				currentPos.x       -= neededScreenPos.x;
				transform.position =  currentPos;
				_onChangeScreen    =  false;
				_currentScreenId   =  screenIdx;
			}
		}

		private void TryOpenTowerWindow() {
			if (_currentScreenId != 1)
				return;
			
			_mapObject.TryOpenNeededTowerWindow();
		}
		
		private void MoveMapObj(float delta) {
			var pos = _mapObject.transform.position;
			pos.x += delta;

			LeanTween.moveX(_mapObject.gameObject, pos.x, 0.1f);
		}

		public bool IsOnMap() {
			return _currentScreenId == 1;
		}

		private void TryShowInterstitial() {
			if (!GameController.Instance.IsCanShowInterstitial())
				return;

			if (_currentScreenId == 1)
				return;

			if (!GameController.Instance.TutorialController.IsTutorialComplete(TutorialId.NewCard))
				return;
		}

		private void CheckNeedShowNoAdsWindow() {
			if (GameController.Instance.Player.Flags.PlayedCount < 3)
				return;
		}
	}
}
