using System;
using System.Collections.Generic;
using Core.Config;
using Core.Config.Store;
using Core.Controllers.Save;
using Core.Controllers.Tutorial;
using Core.Controllers.Windows;
using Core.Controllers.Audio;
using Core.Fade;
using Core.GameTextSpace;
using Game;
using Game.Boards;
using Game.Cards;
using Game.Player;
using Ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Controllers {
	public class GameController : MonoBehaviour {
		private static GameController _instance;
		public static  GameController Instance => _instance;

		[SerializeField] private Camera              _mainCamera;
		[SerializeField] private CheaterPanel        _cheaterPanel;
		[SerializeField] private Transform           _cheaterLayer;
		[SerializeField] private WindowsController   _windowsController;
		[SerializeField] private TutorialController  _tutorialController;
		[SerializeField] private GlobalFade          _globalFade;
		[SerializeField] private MusicController     _musicController;
		[SerializeField] private SoundController     _soundController;

		[Header("Resources")]
		[SerializeField] private GameText _gameText;
		[SerializeField] private DB _db;

		private IPlayer          _player;
		private ISaveController  _saveController;
		private ICardsController _cardsController;
		private MainConfig       _config;
		private PlayedTowerInfo  _playedTowerInfo;

		private bool _isBlockInput             = false;
		private bool _isFocus                  = true;
		private bool _isInitialized            = false;
		private bool _isNeedSave               = false;
		private bool _needCheckScreenSafeArea  = true;
		private bool _needApplaySafeArea       = false;
		private bool _needSendAnalyticsOnStart = true;

		private float _safeAreaHudShift = 0f;
		private float _gamePlayTime     = 0f;

		public ScreensController   ScreensController    { get; set; }
		public MusicController     MusicController      => _musicController;
		public SoundController     SoundController      => _soundController;
		public PlayedTowerInfo     PlayedTowerInfo      => _playedTowerInfo;
		public ICardsController    CardsController      => _cardsController;
		public WindowsController   WindowsController    => _windowsController;
		public TutorialController  TutorialController   => _tutorialController;
		public IPlayer             Player               => _player;
		public DB                  DB                   => _db;
		public CheaterPanel        CheaterPanel         => _cheaterPanel;
		public MainConfig          Config               => _config;
		public BaseBoard           Board                { get; set; }
		public GlobalFade          GlobalFade           => _globalFade;
		public bool                IsInitialized        => _isInitialized;
		public bool                IsBlockInput         => _isBlockInput;
		public float               SafeAreaHudShift     => _safeAreaHudShift;
		public bool                IsInternetAvailable  => Application.internetReachability != NetworkReachability.NotReachable;
		public bool                NeedShowNoAdsWindow  { get; set; } = false;
		public bool                NeedShowRateUsWindow { get; set; } = false;

		public bool IsCanShowInterstitial() {
			if (_player.Flags.NonConsumableItems.Contains(StoreProductNames.NO_ADS))
				return false;

			return true;
		}

		public bool IsCanClick() {
			if (IsBlockInput)
				return false;

			if (IsPointerOverUI())
				return false;

			if (!ScreensController.IsOnMap())
				return false;

			if (_windowsController.OpenedWindowCount > 0)
				return false;

			return true;
		}

		private bool IsPointerOverUI() {
			var eventData = new PointerEventData(EventSystem.current) {
				position = Input.mousePosition,
			};

			var results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventData, results);
			return results.Count > 0;
		}

		public void BlockInput(bool needBlock) {
			_isBlockInput = needBlock;

			Board.MainCG.interactable   = !needBlock;
			Board.MainCG.blocksRaycasts = !needBlock;
		}

		public string GetGameText(string id) {
			return _gameText.GetGameText(id);
		}

		private void OnDestroy() {
			OnDestroySave();
			OnGameCloseAnalyticSend();
		}

		private void OnGameCloseAnalyticSend() {
			if (Board && Board.Type == BoardType.Game) {
				Board.GetComponent<GameBoard>().SendLevelFinishEvent("game_closed", "");
			}
		}

		private void OnDestroySave() {
			Save(false);
		}

		private void OnApplicationFocus(bool hasFocus) {
			if (Board == null || _player == null)
				return;

			if (_isFocus != hasFocus) {
				if (!hasFocus)
					OnDestroySave();

				_isFocus = hasFocus;
			}
		}

		public void InitCheaterLayer() {
			_cheaterLayer.GetComponent<CanvasScaler>().referenceResolution = new Vector2(
				Config.GameWidth, Config.GameHeight);
		}

		private void Awake() {
			Init();
		}

		private void Start() {
#if VOODOO
			TinySauce.SubscribeOnInitFinishedEvent(OnTinySauceConsentGiven);
#endif
		}

		private void OnTinySauceConsentGiven(bool adConsent, bool analyticsConsent) {
			Debug.Log($"[TinySauce] Consent given: AdConsent={adConsent}; AnalyticsConsent={analyticsConsent}");
		}

		private void Init() {
			if (!_instance)
				_instance = this;

			LoadResources();

			Application.targetFrameRate  = 60;
			Screen.sleepTimeout          = SleepTimeout.NeverSleep;
			_mainCamera.orthographicSize = _config.GameHeight / 2f;

			CreatePlayer();
			InitControllers();
			InitLanguage();
			DontDestroyOnLoad(this);

			_gamePlayTime  = _player.Flags.GamePlayTime;
			_isInitialized = true;

			MusicController.StartMusic("track_1", true);
		}

		private void LoadResources() {
#if HARDCORE
			_config = Resources.Load("Config/MainConfig_HC") as MainConfig;
#else
			_config = Resources.Load("Config/MainConfig") as MainConfig;
#endif
		}

		private void InitControllers() {
			_saveController = new SaveController();
			_saveController.Initialize(_player);
			_saveController.Load();

			_cardsController = new CardsController();
			_cardsController.Initialize(_player);

			MyTime.InitAcceleration(_player.Flags.GameAcceleration);

			_playedTowerInfo = new PlayedTowerInfo();

			_musicController.Initialize();
			_soundController.Initialize();
		}

		private void InitLanguage() {
			_gameText.CurrentLanguage = Application.systemLanguage;
		}

		private void CreatePlayer() {
			_player = new Player();
			_player.Initialize(_config.GameData.CharacterParameters);
		}

		public void Save(bool delay = true) {
			if (delay) {
				_isNeedSave = true;
				return;
			}

			_isNeedSave = false;
			_saveController.Save();
		}

		private void Update() {
			if (!_isFocus)
				return;

			if (_needCheckScreenSafeArea)
				TryInitSafeArea();

			if (_needApplaySafeArea)
				UpdateAplaySafeArea();

			UpdatePlayTime();
			UpdateNeedSave();
		}

		private void UpdateNeedSave() {
			if (_isNeedSave)
				Save(false);
		}

		private void UpdatePlayTime() {
			_gamePlayTime              += Time.deltaTime;
			_player.Flags.GamePlayTime =  (int) _gamePlayTime;
		}

		private void TryInitSafeArea() {
#if !UNITY_EDITOR
			if (Screen.width != Screen.safeArea.width)
				return;
#endif

			_needCheckScreenSafeArea = false;

			_safeAreaHudShift = Math.Abs((Screen.safeArea.height - Screen.height) / 2.0f);
			if (SafeAreaHudShift > 50)
				_safeAreaHudShift = 50;

			_needApplaySafeArea = true;
		}

		private void UpdateAplaySafeArea() {
			if (Board == null)
				return;

			if (Board.Type == BoardType.Loader)
				return;

			_needApplaySafeArea = false;

			Board.ApplaySafeArea();
		}

		public void ClearSave() {
			_player.ClearSave();
			_saveController.Save();
		}
	}
}
