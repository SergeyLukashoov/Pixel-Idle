using System;
using System.Collections.Generic;
using Core.Config.Tutorial;
using Core.Controllers;
using Core.Controllers.Windows;
using UnityEngine;
using Window;

namespace Ui.Hud {
	public enum HudButtonType {
		Menu    = 1,
		Upgrade = 2,
		Cards   = 3,
		Locked  = 9,
	}

	[Serializable]
	public class HudButtonInfo {
		public HudButtonType _type;
		public HudButtonView _hudButton;
		public int           _screenIdx;
	}

	public class HudBottom : MonoBehaviour {
		[SerializeField] private List<HudButtonInfo> _hudButtons = new List<HudButtonInfo>();
		[SerializeField] private ScreensController   _screensController;

		public void UnlockButton(HudButtonType type) {
			var hudButtonInfo = _hudButtons.Find(x => x._type == type);
			hudButtonInfo._hudButton.SetLockedState(false);
		}
		
		private void Start() {
			InitButtons();
		}

		private void InitButtons() {
			for (var i = 0; i < _hudButtons.Count; ++i) {
				if (_hudButtons[i]._type == HudButtonType.Locked) {
					_hudButtons[i]._hudButton.Btn.onClick.AddListener(OnLockButtonClick);
					continue;
				}

				if (_hudButtons[i]._type == HudButtonType.Menu)
					_hudButtons[i]._hudButton.Btn.onClick.AddListener(OnMenuButtonClick);
				else if (_hudButtons[i]._type == HudButtonType.Upgrade) {
					_hudButtons[i]._hudButton.Btn.onClick.AddListener(OnUpgradeButtonClick);
					
					if (!GameController.Instance.TutorialController.IsTutorialComplete(TutorialId.UpgradeScreen))
						_hudButtons[i]._hudButton.SetLockedState(true);
				}
				else if (_hudButtons[i]._type == HudButtonType.Cards) {
					_hudButtons[i]._hudButton.Btn.onClick.AddListener(OnCardsButtonClick);
					
					if (!GameController.Instance.TutorialController.IsTutorialComplete(TutorialId.NewCard))
						_hudButtons[i]._hudButton.SetLockedState(true);
				}
			}

			SetButtonActive(HudButtonType.Menu, false);
		}

		private void OnLockButtonClick() {
			var settings = new InfoHeaderWS {
				HeaderStr   = "",
				InfoStr     = GameController.Instance.GetGameText("lock_menu_button_info"),
				NeedHeader  = false,
				NeedContent = false,
			};

			GameController.Instance.WindowsController.Show(WindowType.InfoHeaderWindow, settings);
		}

		public void SetButtonActive(HudButtonType type, bool needTween) {
			for (var i = 0; i < _hudButtons.Count; ++i) {
				if (type == _hudButtons[i]._type) {
					_hudButtons[i]._hudButton.SetActive(true);
					_screensController.MoveToScreen(_hudButtons[i]._screenIdx, needTween);
				}
				else {
					_hudButtons[i]._hudButton.SetActive(false);
				}
			}
		}

		private void OnMenuButtonClick() {
			if (_screensController.OnChangeScreen)
				return;

			SetButtonActive(HudButtonType.Menu, true);
		}

		private void OnUpgradeButtonClick() {
			if (_screensController.OnChangeScreen)
				return;

			var buttonInfo = _hudButtons.Find(x => x._type == HudButtonType.Upgrade);
			if (buttonInfo._hudButton.Locked) {
				StartInfoWindow("upgrade_screen_lock_info");
			}
			else {
				SetButtonActive(HudButtonType.Upgrade, true);
			}
		}

		private void OnCardsButtonClick() {
			if (_screensController.OnChangeScreen)
				return;

			var buttonInfo = _hudButtons.Find(x => x._type == HudButtonType.Cards);
			if (buttonInfo._hudButton.Locked) {
				StartInfoWindow("cards_screen_lock_info");
			}
			else {
				SetButtonActive(HudButtonType.Cards, true);
			}
		}

		private void StartInfoWindow(string infoStrId) {
			var settings = new InfoHeaderWS {
				HeaderStr = "",
				InfoStr     = GameController.Instance.GetGameText(infoStrId),
				NeedHeader  = false,
				NeedContent = false
			};
			
			GameController.Instance.WindowsController.Show(WindowType.InfoHeaderWindow, settings);
		}
		
		public GameObject FindTargetForTutor(string currentStepTargetId) {
			if (currentStepTargetId == "UpgradeScreen") {
				var hudBtnInfo = _hudButtons.Find(x => x._type == HudButtonType.Upgrade);
				if (hudBtnInfo != null)
					return hudBtnInfo._hudButton.gameObject;
			}
			if (currentStepTargetId == "CardsScreen") {
				var hudBtnInfo = _hudButtons.Find(x => x._type == HudButtonType.Cards);
				if (hudBtnInfo != null)
					return hudBtnInfo._hudButton.gameObject;
			}
			if (currentStepTargetId == "ButtonBattle") {
				var hudBtnInfo = _hudButtons.Find(x => x._type == HudButtonType.Menu);
				if (hudBtnInfo != null)
					return hudBtnInfo._hudButton.gameObject;
			}

			return null;
		}
	}
}
