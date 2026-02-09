using System;
using System.Collections.Generic;
using Core.Controllers;
using Core.Controllers.Windows;
using Game.Player;
using Ui;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.UI;

namespace Window {
	public class RateUsWindow : BaseWindow<RateUsWS> {
		public override WindowType Type => WindowType.RateUsWindow;

		[SerializeField] private Button _buttonClose;
		[SerializeField] private Button _buttonRate;
		[SerializeField] private Button _buttonContinue;

		[SerializeField] private Image  _buttonRateImage;
		[SerializeField] private Sprite _activeSp;
		[SerializeField] private Sprite _activePressedSp;
		[SerializeField] private Sprite _disabledSp;
		[SerializeField] private Sprite _disabledPressedSp;

		[SerializeField] private List<RateStarObj> _starObjButtons;

		private IPlayer _player;

		private int _rateStarsCount;

		private void Start() {
			_player = GameController.Instance.Player;

			_player.Flags.RateUsShowCount++;
			_player.Flags.RateUsShowTime = (long) TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;

			InitButtons();
			InitRateStars();
		}

		private void InitRateStars() {
			for (var i = 0; i < _starObjButtons.Count; ++i) {
				_starObjButtons[i].OnClickAction = OnStarClick;
			}
		}

		private void OnStarClick(RateStarObj starObj) {
			var starObjId = _starObjButtons.IndexOf(starObj);
			for (var i = 0; i < _starObjButtons.Count; ++i) {
				if (i <= starObjId) {
					_starObjButtons[i].SetActiveState(true);
					_rateStarsCount++;
				}
				else
					_starObjButtons[i].SetActiveState(false);
			}

			SetRateButtonState(_rateStarsCount > 0);
		}

		private void InitButtons() {
			_buttonClose.onClick.AddListener(Hide);
			_buttonContinue.onClick.AddListener(Hide);

			_buttonRate.onClick.AddListener(OnRateClick);
			SetRateButtonState(false);
		}

		private void SetRateButtonState(bool active) {
			_buttonRate.interactable = active;
			_buttonRateImage.sprite  = active ? _activeSp : _disabledSp;;
			
			var spriteStates = _buttonRate.spriteState;
			spriteStates.highlightedSprite = active ? _activeSp : _disabledSp;
			spriteStates.pressedSprite     = active ? _activePressedSp : _disabledPressedSp;
			spriteStates.selectedSprite    = active ? _activeSp : _disabledSp;
			spriteStates.disabledSprite    = active ? _activeSp : _disabledSp;
			_buttonRate.spriteState        = spriteStates;
		}

		private void OnRateClick() {
			if (_rateStarsCount <= 4)
				Hide();
			else {
#if UNITY_ANDROID
				Application.OpenURL($"market://details?id={Application.identifier}");
#elif UNITY_IOS
				Device.RequestStoreReview();
				//Application.OpenURL($"market://details?id={Application.identifier}");
#endif
				Hide();
			}
		}
	}

	public class RateUsWS : BaseWindowSettings {
	}
}
