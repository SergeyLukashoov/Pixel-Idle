using System;
using Core.Config.Tutorial;
using Core.Controllers;
using Core.Controllers.Tutorial;
using Core.Controllers.Windows;
using Core.Utils;
using Game.Cards;
using Game.Map;
using Game.Player;
using Ui.Hud;
using Ui.Screens.Cards;
using Ui.Screens.Upgrade;
using UnityEngine;
using UnityEngine.Playables;
using Window;

namespace Game.Boards {
	public class MenuBoard : BaseBoard {
		[SerializeField] private UpgradeScreen     _upgradeScreen;
		[SerializeField] private CardsScreen       _cardsScreen;
		[SerializeField] private HudBottom         _hudBottom;
		[SerializeField] private HudTop            _hudTop;
		[SerializeField] private MapObject         _mapObject;
		[SerializeField] private ScreensController _screensController;

		private IPlayer            _player;
		private TutorialController _tutorialController;
		private WindowsController  _windowsController;

		//private float _timeToShowHand = 0f;

		public HudBottom HudBottom      => _hudBottom;
		public bool      CanShowNewCard { get; set; } = false;

		public override void ApplaySafeArea() {
			var hudPos = _hudTop.transform.localPosition;
			hudPos.y                        -= GameController.Instance.SafeAreaHudShift;
			_hudTop.transform.localPosition =  hudPos;
		}

		private void Start() {
			_player             = GameController.Instance.Player;
			_tutorialController = GameController.Instance.TutorialController;
			_windowsController  = GameController.Instance.WindowsController;

			GameController.Instance.GlobalFade.StartFadeToAlpha(0f, null);
			GameController.Instance.ScreensController = _screensController;

			_mapObject.Initialize();
		}

		private void CheckNeedStartTutorial() {
			if (_windowsController.OpenedWindowCount > 0)
				return;

			if (_screensController.OnChangeScreen)
				return;

			if (!_tutorialController.IsCanUpdateTutors())
				return;

			if (!_tutorialController.IsTutorialComplete(TutorialId.FirstTowerClick)) {
				_tutorialController.StartTutorial(TutorialId.FirstTowerClick, FindTargetForTutor);
			}
			else if (_tutorialController.NeedUpgradeScreen && !_tutorialController.IsTutorialComplete(TutorialId.UpgradeScreen)) {
				GameController.Instance.Player.ChangeCoinsCount(250);
				_hudBottom.UnlockButton(HudButtonType.Upgrade);
				_tutorialController.StartTutorial(TutorialId.UpgradeScreen, FindTargetForTutor);
			}
			else if (!_tutorialController.IsTutorialComplete(TutorialId.FirstMetaUpgrade) &&
			         _tutorialController.IsTutorialComplete(TutorialId.UpgradeScreen) &&
			         _screensController.CurrentScreenId == 0) {
				_tutorialController.StartTutorial(TutorialId.FirstMetaUpgrade, FindTargetForTutor, FindClickTargetForTutor);
			}
			else if (_tutorialController.NeedFirstChest &&
			         !_tutorialController.IsTutorialComplete(TutorialId.FirstChest)) {
				_tutorialController.StartTutorial(TutorialId.FirstChest, FindTargetForTutor);
				_tutorialController.NeedFirstChest  = false;
				_tutorialController.NeedChestWindow = true;
			}
			else if (_tutorialController.NeedNewCard &&
			         !_tutorialController.IsTutorialComplete(TutorialId.NewCard)) {
				_hudBottom.UnlockButton(HudButtonType.Cards);
				
				GameController.Instance.Player.ClearNeedShowCards();
				CanShowNewCard = false;
				
				_tutorialController.StartTutorial(TutorialId.NewCard, FindTargetForTutor);
			}
			else if (!_tutorialController.IsTutorialComplete(TutorialId.NewCardActivate) &&
			         _tutorialController.IsTutorialComplete(TutorialId.NewCard) &&
			         _screensController.CurrentScreenId == 2) {
				_tutorialController.StartTutorial(TutorialId.NewCardActivate, FindTargetForTutor, FindClickTargetForTutor);
			}
		}

		private GameObject FindTargetForTutor() {
			var currentStepTargetId = _tutorialController.CurrentStepTargetId;
			if (currentStepTargetId == "Tower1") {
				return _mapObject.GetTowerById(1);
			}
			else if (currentStepTargetId == "UpgradeScreen" || currentStepTargetId == "CardsScreen" ||
			         currentStepTargetId == "ButtonBattle") {
				return _hudBottom.FindTargetForTutor(currentStepTargetId);
			}
			else if (currentStepTargetId == "AttackMeta" || currentStepTargetId == "DefeatMoreWaves") {
				return _upgradeScreen.FindTargetForTutor(currentStepTargetId);
			}
			else if (currentStepTargetId == "FirstActiveSlot" || currentStepTargetId == "FirstCollectionSlot") {
				return _cardsScreen.FindTargetForTutor(currentStepTargetId);
			}
			else if (currentStepTargetId == "Chest") {
				return _tutorialController.FirstChestObject;
			}
			else if (currentStepTargetId == "HudTop") {
				return _hudTop.CurrencyRoot;
			}

			return null;
		}

		private GameObject FindClickTargetForTutor() {
			var currentStepTargetId = _tutorialController.CurrentStepTargetId;
			if (currentStepTargetId == "AttackMeta") {
				return _upgradeScreen.FindClickTargetForTutor(currentStepTargetId);
			}
			else if (currentStepTargetId == "FirstActiveSlot" || currentStepTargetId == "FirstCollectionSlot") {
				return _cardsScreen.FindClickTargetForTutor(currentStepTargetId);
			}
			else if (currentStepTargetId == "ButtonBattle") {
				return _hudBottom.FindTargetForTutor(currentStepTargetId);
			}

			return null;
		}

		public override void UpdateUpgradeButtons() {
			_upgradeScreen.UpdateStatsView();
		}

		public void UpdateUpgradeScreen() {
			_upgradeScreen.UpdateScreen();
		}

		private void Update() {
			UpdateButtonsClick();
			UpdateNeedHand();
		}

		private void UpdateNeedHand() {
			/*if (_screensController.OnChangeScreen ||
			    _screensController.CurrentScreenId != 1) {
				_timeToShowHand = 0f;
				return;
			}

			if (GameController.Instance.WindowsController.OpenedWindowCount > 0)
				return;

			if (GameController.Instance.Player.UnlockedTowersCount > 3)
				return;

			if (_timeToShowHand >= 2f)
				return;

			_timeToShowHand += Time.deltaTime;
			if (_timeToShowHand >= 2f) {
				_mapObject.ShowHand();
			}*/
		}

		private void LateUpdate() {
			CheckNeedStartTutorial();
			UpdateNeedShowNewCards();
			CheckNeedShowWindowsOnStart();
			//CheckNeedShowRateUsWindow();
		}

		private void CheckNeedShowWindowsOnStart() {
			if (_windowsController.OpenedWindowCount > 0)
				return;

			if (GameController.Instance.NeedShowNoAdsWindow) {
				GameController.Instance.NeedShowNoAdsWindow = false;
				_windowsController.ShowNoAdsWindow(_hudTop.NoAdsButtonObj);
				return;
			}
		}

		private void CheckNeedShowRateUsWindow() {
			if (_windowsController.OpenedWindowCount > 0)
				return;
			
			if (_player.Flags.RateUsShowCount >= 25 || _player.Flags.IsWasRate)
				return;
			
			if (_player.Flags.PlayedCount < 3)
				return;

			if (Utility.IsNextDay(_player.Flags.RateUsShowTime)) {
				if (GameController.Instance.NeedShowRateUsWindow) {
					GameController.Instance.NeedShowRateUsWindow = false;
					
					_windowsController.ShowRateUsDialog();
				}
			}
		}

		private void UpdateNeedShowNewCards() {
			if (!CanShowNewCard)
				return;

			if (_windowsController.OpenedWindowCount > 0)
				return;

			if (_screensController.OnChangeScreen)
				return;

			if (_screensController.CurrentScreenId != 1)
				return;

			CheckNeedShowCards();
		}

		private void CheckNeedShowCards() {
			var showCardType = GameController.Instance.Player.GetCardForShow();
			if (showCardType != CardType.Empty && _tutorialController.IsTutorialComplete(TutorialId.NewCard)) {
				CanShowNewCard = false;

				var cardInfo = GameController.Instance.Player.GetCard(showCardType);
				var settings = new UpgradeCardWS {
					Info   = cardInfo,
					OnHide = CheckNeedShowCards
				};

				_windowsController.Show(WindowType.UpgradeCardWindow, settings, true, 0.3f);
			}
		}
	}
}
