using System;
using Core.Config.Tutorial;
using Core.Controllers.Windows;
using Game.Boards;
using Game.Player;
using UnityEngine;
using Window;

namespace Core.Controllers.Tutorial {
	public class TutorialController : MonoBehaviour {
		[SerializeField] private TutorialDB _db;

		private IPlayer           _player;
		private WindowsController _windowsController;
		private TutorialInfo      _currentTutorialInfo;

		public Func<GameObject> OnFindTarget;
		public Func<GameObject> OnFindClickTarget;

		private int _stepIdx;

		public GameObject FirstChestObject  { get; set; }
		public bool       NeedUpgradeScreen { get; set; } = false;
		public bool       NeedNewCard       { get; set; } = false;
		public bool       NeedFirstChest    { get; set; } = false;
		public bool       NeedChestWindow   { get; set; } = false;

		public int    StepIdx             => _stepIdx;
		public string CurrentStepTargetId => _currentTutorialInfo.TutorialStepInfo[_stepIdx].TargetId;

		private void Start() {
			_player            = GameController.Instance.Player;
			_windowsController = GameController.Instance.WindowsController;
		}

		public bool IsCanUpdateTutors() {
			if (NeedChestWindow)
				return false;

			return true;
		}
		
		public bool IsTutorialComplete(TutorialId tutorialId) {
#if REC_VIDEO
			return true;
#endif

			return _player.Flags.IsTutorialComplete(tutorialId.ToString());
		}

		public void StartTutorial(TutorialId id, Func<GameObject> onFindTarget, Func<GameObject> onFindClickTarget = null) {
			GameController.Instance.BlockInput(true);

			_currentTutorialInfo = _db.GetTutorialById(id);
			if (_currentTutorialInfo == null) {
				GameController.Instance.BlockInput(false);
				return;
			}

			_player.Flags.SetTutorialComplete(_currentTutorialInfo.TutorialId.ToString());
			
			OnFindTarget      = onFindTarget;
			OnFindClickTarget = onFindClickTarget;

			_stepIdx = 0;

			var settings = new TutorialWS {
				Info = _currentTutorialInfo,
			};

			_windowsController.Show(WindowType.TutorialWindow, settings);
		}

		public void ChangeStep() {
			_stepIdx++;
			if (_stepIdx > _currentTutorialInfo.TutorialStepInfo.Count - 1) {
				CompeteTutorial();
			}
		}

		private void CompeteTutorial() {
			_currentTutorialInfo = null;

			var tutorialWindow = _windowsController.GetActiveWindow<TutorialWS>(WindowType.TutorialWindow);
			if (tutorialWindow)
				tutorialWindow.Hide();

			var gameBoard = GameController.Instance.Board as GameBoard;
			if (gameBoard && gameBoard.IsPause)
				gameBoard.SetPauseState(false);
		}
	}
}
