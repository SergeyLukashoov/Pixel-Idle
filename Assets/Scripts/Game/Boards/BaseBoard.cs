using Core.Controllers;
using Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Boards {
	public enum BoardType {
		Menu,
		Game,
		Loader,
	}

	public class BaseBoard : MonoBehaviour {
		[SerializeField] private BoardType _type;
		[Header("BaseBoard")]
		[SerializeField] private CanvasScaler _canvasScaler;
		[SerializeField] private Button _cheaterButton;

		[Header("Layers")]
		[SerializeField] protected RectTransform _mainCanvasRT;
		[SerializeField] protected CanvasGroup _mainCG;
		[SerializeField] private   Transform   _windowsLayer;
		[SerializeField] protected Transform   _uiLayer;

		private CheaterPanel   _cheaterPanel;
		private GameController _gameController;
		private Camera         _mainCamera;

		public BoardType     Type         => _type;
		public RectTransform MainCanvasRT => _mainCanvasRT;
		public CanvasGroup   MainCG       => _mainCG;
		public Camera        MainCamera   => _mainCamera;

		public virtual void UpdateUpgradeButtons() {

		}

		public virtual void ApplaySafeArea() {

		}

		private void Awake() {
			Init();
		}

		private void Init() {
			_gameController       = GameController.Instance;
			_gameController.Board = this;

			var width  = _gameController.Config.GameWidth;
			var height = _gameController.Config.GameHeight;

			_mainCamera = Camera.main;
			if (_mainCamera)
				_mainCamera.orthographicSize = height / 2f;
			else
				Debug.LogWarning("MainCamera not found!");

			_canvasScaler.referenceResolution = new Vector2(width, height);

			InitCheaterPanel();
			InitButtons();

			_gameController.WindowsController.WindowsLayer = _windowsLayer;
			_gameController.GlobalFade.SetMainCamera(_mainCamera);
		}

		private void InitButtons() {
			if (_type == BoardType.Loader)
				return;

			_cheaterButton.onClick.AddListener(OnCheaterClick);

#if CHEATER
			_cheaterButton.gameObject.SetActive(true);
#if REC_VIDEO
			_cheaterButton.GetComponent<CanvasGroup>().alpha = 0f;
#endif
#else
			_cheaterButton.gameObject.SetActive(false);
#endif
		}

		private void InitCheaterPanel() {
			GameController.Instance.InitCheaterLayer();
			_cheaterPanel = GameController.Instance.CheaterPanel;
		}

		private void OnCheaterClick() {
			GameController.Instance.CheaterPanel.Show();
		}

		protected void UpdateButtonsClick() {
#if CHEATER
			if (Input.GetKeyDown("`")) {
				if (_cheaterPanel.IsShowed)
					_cheaterPanel.Hide();
				else
					_cheaterPanel.Show();
			}
#endif
		}
	}
}
