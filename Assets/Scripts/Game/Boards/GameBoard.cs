using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core;
using Core.Config;
using Core.Config.Tutorial;
using Core.Config.Wave;
using Core.Controllers;
using Core.Controllers.Tutorial;
using Core.Controllers.Windows;
using Core.Utils;
using DevToDev.Analytics;
using Game.Ability;
using Game.Back;
using Game.Bullet;
using Game.Cards;
using Game.Enemy;
using Game.Player;
using Game.Wave;
using TMPro;
using Ui;
using Ui.Hud;
using Ui.Screens.Upgrade;
using UnityEngine;
using UnityEngine.UI;
using Window;
using Random = UnityEngine.Random;

namespace Game.Boards {
	internal enum GameBoardState {
		None,
		Pause,
		Play,
		EndRun,
	}

	public class GameBoard : BaseBoard {
		[Header("GameBoard")]
		[SerializeField] private Button _exitButton;
		[SerializeField] private Button          _attackSpeedButton;
		[SerializeField] private PlayerView      _playerView;
		[SerializeField] private BackContainer   _backContainer;
		[SerializeField] private GameObject      _rangeObject;
		[SerializeField] private GameHudBottom   _gameHudBottom;
		[SerializeField] private Transform       _gameHudTop;
		[SerializeField] private Transform       _minionsLayer;
		[SerializeField] private Transform       _gameLayer;
		[SerializeField] private Transform       _overGameLayer;
		[SerializeField] private TextMeshProUGUI _stateLabel;
		[SerializeField] private TextMeshProUGUI _coinsLabel;
		[SerializeField] private TextMeshProUGUI _expLabel;
		[SerializeField] private TextMeshProUGUI _attackSpeedCooldownLabel;
		[SerializeField] private TextMeshProUGUI _enemyBaseAttack;
		[SerializeField] private TextMeshProUGUI _enemyBaseHealth;
		[SerializeField] private TextMeshProUGUI _playerAttackLabel;
		[SerializeField] private TextMeshProUGUI _playerRegenLabel;
		[SerializeField] private TextMeshProUGUI _playerCoinsMultLabel;
		[SerializeField] private TextMeshProUGUI _currentWaveIdLabel;
		[SerializeField] private TextMeshProUGUI _frameRateLabel;
		[SerializeField] private TextMeshProUGUI _dps1Label;
		[SerializeField] private TextMeshProUGUI _dps2Label;
		[SerializeField] private GameObject      _dpsRoot;
		[SerializeField] private ProgressBar     _waveProgressBar;
		[SerializeField] private GameObject      _dropViewPrefab;
		[SerializeField] private GameObject      _reviveIcon;
		[SerializeField] private GameObject      _bulletPrefab;
		[SerializeField] private GameObject      _cardDropPrefab;
		[SerializeField] private GameObject      _completeWaveCountPrefab;
		[SerializeField] private GameObject      _multishotCardPrefab;
		[SerializeField] private SpawnPoint      _specialSpawnPoint;
		[SerializeField] private GameObject      _vampireBiteFxPrefab;

		private GameBoardState _state     = GameBoardState.None;
		private GameBoardState _prevState = GameBoardState.None;
		
		private IAbilityController   _abilityController;
		private ISpawnWaveController _spawnWaveController;
		private TutorialController   _tutorialController;
		private EnemyDB              _enemyDB;
		private WaveDB               _waveDB;
		private PlayerFlags          _playerFlags;

		private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
		private List<EnemyView>  _enemies     = new List<EnemyView>();
		private List<BulletView> _bullets     = new List<BulletView>();

		private int   _killedEnemiesCount = 0;
		private int   _prevSpawnPointIdx  = -100;
		private int   _currentRangeSteps  = 0;
		private int   _droppedCardsCount  = 0;
		private float _expCount           = 0;
		private float _coinsCount         = 0;
		private float _backAddSizeStepX;
		private float _backAddSizeStepY;
		private float _scaleStepFromRange;
		private float _attackRadius;
		private float _timeToShowEndRunWindow = 4f;
		private bool  _needShowEndRunWindow   = true;
		private bool  _needShowReviveWindow   = true;
		private bool  _isFocus                = true;

		private Vector2 _neededGameRootScale;
		private LTDescr _rangeSizeTween;
		private LTDescr _gameRootSizeTween;
		private LTDescr _backSizeTween;

		public Action OnChangeExpCount;

		public PlayerView PlayerView          => _playerView;
		public Transform  OverGameLayer       => _overGameLayer;
		public GameObject DropViewPrefab      => _dropViewPrefab;
		public EnemyDB    EnemyDB             => _enemyDB;
		public bool       IsPause             => _state == GameBoardState.Pause;
		public GameObject VampireBiteFxPrefab => _vampireBiteFxPrefab;
		public int        CurrentWaveId       => _spawnWaveController.WaveIdxForDraw;
		public float      AttackRadius        => _attackRadius;

		public void PlayWaveSkipAnim() {
			_playerView.PlayWaveSkipAnim();
		}

		public override void ApplaySafeArea() {
			var hudPos = _gameHudTop.localPosition;
			hudPos.y                  -= GameController.Instance.SafeAreaHudShift;
			_gameHudTop.localPosition =  hudPos;
		}

		private void OnDestroy() {
			GameController.Instance.Player.OnChangeGameBoardCoinsCount -= OnChangeCoinsCount;
		}

		private void OnApplicationFocus(bool hasFocus) {
			if (_isFocus != hasFocus) {
				if (!hasFocus)
					OnLostFocus();

				_isFocus = hasFocus;
			}
		}

		private void OnLostFocus() {
			if (GameController.Instance.WindowsController.OpenedWindowCount > 0)
				return;

			ShowSettingsWindow(false);
		}

		private void OnChangeCoinsCount(float delta) {
			_coinsCount += delta;
			UpdateCoinsLabel();
		}

		public void SetReviveIconActive(bool isActive) {
			_reviveIcon.SetActive(isActive);
		}

		public override void UpdateUpgradeButtons() {
			_gameHudBottom.UpdateUpgradeButtons();
		}

		public void UpdateAcceleration() {
			_playerView.SetAnimationCurrentSpeed();
			for (var i = 0; i < _enemies.Count; ++i)
				_enemies[i].SetAnimationCurrentSpeed();
		}

		public bool IsHaveExp(int expCount) {
			return expCount <= _expCount;
		}

		public void ChangeExp(float delta) {
			_expCount += delta;
			OnChangeExpCount?.Invoke();
			UpdateExpLabel();
		}

		public void SetStateLabel(SpawnState state) {
			var waveIdx = _spawnWaveController.WaveIdxForDraw;
			_stateLabel.text = $"Wave: {waveIdx + 1} | State: {state}";

#if REC_VIDEO
			_stateLabel.gameObject.SetActive(false);
#endif
		}

		public void AttackTarget(Transform targetForShoot, EnemyView target, float damage, bool isCrit) {
			CreateBullet(targetForShoot, target, damage, isCrit);

			var activeCard = GameController.Instance.Player.GetActiveCardByType(CardType.Multishot);
			if (activeCard != null) {
				var cardConfig = GameController.Instance.DB.CardsDB.GetCardConfig(activeCard._type);
				var currLevel  = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(activeCard._count);

				var bulletCount = (int) (cardConfig.GetCardAddValueByLvlUp(currLevel - 1) - 1);
				var probVal     = cardConfig.GetCardValueByLvlUp(currLevel - 1);

				var shotProb = new RandomProb();
				shotProb.AddValue("yes", (int) probVal);
				shotProb.AddValue("no", 100 - (int) probVal);

				var randVal = shotProb.GetRandomValue();
				if (randVal == "yes") {
					var targetsInRange = GetEnemiesInRange(_playerView.transform.position, target);
					if (bulletCount > targetsInRange.Count)
						bulletCount = targetsInRange.Count;

					if (bulletCount > 0)
						ShowMultishotLabel();

					for (var i = 0; i < bulletCount; ++i) {
						var randTargetIdx = Random.Range(0, targetsInRange.Count);
						var randTarget    = targetsInRange[randTargetIdx];
						targetsInRange.RemoveAt(randTargetIdx);

						CreateBullet(targetForShoot, randTarget, damage, isCrit);
					}

					Debug.Log($"[Cards] {activeCard._type} applayed!");
				}
			}
		}

		private void ShowMultishotLabel() {
			var pos = _playerView.transform.position;
			pos.y += 50;

			var endPos = pos;
			endPos.y += 200;

			var countObject = Instantiate(_multishotCardPrefab, _overGameLayer);
			countObject.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
			countObject.transform.position   = pos;

			var countCg = countObject.GetComponent<CanvasGroup>();
			countCg.alpha = 0f;

			LeanTween.alphaCanvas(countCg, 1f, 0.2f);
			LeanTween.scale(countObject, Vector3.one, 0.4f).setEase(LeanTweenType.easeOutBack);
			LeanTween.move(countObject, endPos, 1f);
			LeanTween.alphaCanvas(countCg, 0f, 0.1f).setDelay(0.9f).setOnComplete(() => { Destroy(countObject); });
		}

		private void CreateBullet(Transform targetForShoot, EnemyView target, float damage, bool isCrit) {
			var bulletObj = Instantiate(_bulletPrefab, _overGameLayer);
			bulletObj.transform.localScale = target.transform.lossyScale;
			bulletObj.transform.position   = targetForShoot.position;

			var dir   = (target.transform.position - targetForShoot.position).normalized;
			var angle = Vector2.Angle(new Vector2(1, 0), dir);
			if (dir.y < 0)
				angle = -angle;

			var bulletAngles = bulletObj.transform.eulerAngles;
			bulletAngles.z                  = angle;
			bulletObj.transform.eulerAngles = bulletAngles;

			var bullet = bulletObj.GetComponent<BulletView>();
			if (target.CurrentHealth - damage <= 0f)
				target.SkipFromSearch = true;

			bullet.IsCrit        = isCrit;
			bullet.Damage        = damage;
			bullet.Target        = target;
			bullet.IsVampireBite = IsNeedVampireBite();
			bullet.Move(OnBulletDestroy);

			_bullets.Add(bullet);
		}

		public void CreateBulletForPlayer(EnemyView shooter, PlayerView target, float damage) {
			var bulletObj = Instantiate(_bulletPrefab, _overGameLayer);
			bulletObj.transform.localScale = shooter.transform.lossyScale;
			bulletObj.transform.position   = shooter.transform.position;

			var dir   = (shooter.transform.position - target.transform.position).normalized;
			var angle = Vector2.Angle(new Vector2(1, 0), dir);
			if (dir.y < 0)
				angle = -angle;

			var bulletAngles = bulletObj.transform.eulerAngles;
			bulletAngles.z                  = angle;
			bulletObj.transform.eulerAngles = bulletAngles;

			var bullet = bulletObj.GetComponent<BulletView>();
			if (target.CurrentHealth - damage <= 0f)
				target.WasDead = true;

			bullet.IsCrit        = false;
			bullet.Damage        = damage;
			bullet.Player        = target;
			bullet.IsVampireBite = false;
			bullet.Move(OnBulletDestroy);

			_bullets.Add(bullet);
		}

		private bool IsNeedVampireBite() {
			var activeCard = GameController.Instance.Player.GetActiveCardByType(CardType.VampireBite);
			if (activeCard != null) {
				var cardConfig = GameController.Instance.DB.CardsDB.GetCardConfig(activeCard._type);
				var currLevel  = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(activeCard._count);
				var probVal    = cardConfig.GetCardValueByLvlUp(currLevel - 1);

				var biteProb = new RandomProb();
				biteProb.AddValue("yes", probVal);
				biteProb.AddValue("no", 100 - probVal);

				var randVal = biteProb.GetRandomValue();
				if (randVal == "yes") {
					Debug.Log($"[Cards] {activeCard._type} applayed!");
					return true;
				}
			}

			return false;
		}

		private void OnBulletDestroy(BulletView view) {
			if (_bullets.Contains(view))
				_bullets.Remove(view);
		}

		private void Start() {
			GameController.Instance.GlobalFade.StartFadeToAlpha(0f, null);
			GameController.Instance.Player.OnChangeGameBoardCoinsCount += OnChangeCoinsCount;
			GameController.Instance.Player.IncreaseTowerPlayedCount(GameController.Instance.PlayedTowerInfo.Id);

			_enemyDB     = new EnemyDB(GameController.Instance.Config.GameData.NPCParameters);
			_playerFlags = GameController.Instance.Player.Flags;
			
			_playerFlags.PlayedCount++;
			if (_playerFlags.PlayedCount >= 3 && !GameController.Instance.NeedShowRateUsWindow)
				GameController.Instance.NeedShowRateUsWindow = true;
			
			_tutorialController  = GameController.Instance.TutorialController;

			if (_tutorialController.IsTutorialComplete(TutorialId.FirstBattleUpgrade))
				_expCount = GameController.Instance.Config.StartCoreExp;

			InitButtons();
			InitWaveDB();
			InitSpawnPoints();
			InitRangeObject();
			InitBackSize();
			InitPlayerView();
			InitSpawnWavesController();
			UpdateCoinsLabel();
			UpdateExpLabel();
			InitAbilityController();
			UpdateEnemyBaseStatsLabels();
			UpdateStatsInfoLabels();
			UpdateDpsLabel();
			InitAcceleration();
#if UNITY_IOS
			ApplaySafeArea();
#endif

			_attackSpeedCooldownLabel.gameObject.SetActive(false);

			_backContainer.CheckObjectsVisible();

#if !CHEATER
			_stateLabel.gameObject.SetActive(false);
			_dpsRoot.SetActive(false);
#endif

			SendLevelStartEvent();
			_frameRateLabel.gameObject.SetActive(false);
		}

		private void InitAcceleration() {
			if (_playerFlags.AdsAccelerationTime > 0f)
				MyTime.maxAcceleration = GameController.Instance.Config.AdsAccelerateReward;
		}

		private void UpdateFrameRateLabel(int fps) {
			_frameRateLabel.text = fps.ToString();
		}

		public void UpdateDpsLabel() {
			var damage      = _playerView.Stats.GetStatByType(PlayerStatType.Damage)._value;
			var critChance  = _playerView.Stats.GetStatByType(PlayerStatType.CritChance)._value / 100f;
			var critMult    = _playerView.Stats.GetStatByType(PlayerStatType.CritMult)._value;
			var attackSpeed = _playerView.Stats.GetStatByType(PlayerStatType.ShotsPerSec)._value;
			var range       = _playerView.Stats.GetStatByType(PlayerStatType.Range)._value;

			var addDmgPerRangeInfo = _playerView.Stats.GetStatByType(PlayerStatType.AddDmgPerRange);
			var addDmgPerRange     = 0f;
			if (addDmgPerRangeInfo._unlocked)
				addDmgPerRange = addDmgPerRangeInfo._value;

			var superCritChanceInfo = _playerView.Stats.GetStatByType(PlayerStatType.SuperCritChance);
			var superCritChance     = 0f;
			if (superCritChanceInfo._unlocked)
				superCritChance = superCritChanceInfo._value / 100f;

			var superCritMultInfo = _playerView.Stats.GetStatByType(PlayerStatType.SuperCritMult);
			var superCritMult     = 1f;
			if (superCritMultInfo._unlocked)
				superCritMult = superCritChanceInfo._value;

			var dps1Val = ((1 - critChance) * damage + critChance * critMult * damage) *
			              (1 + critChance * superCritChance * superCritMult) * attackSpeed;
			_dps1Label.text = dps1Val.ToString();

			var dps2Val = dps1Val * (1 + (range * addDmgPerRange) / 100f);
			_dps2Label.text = dps2Val.ToString();
		}

		private void SendLevelStartEvent() {
			var towerId     = GameController.Instance.PlayedTowerInfo.Id;
			var playedCount = GameController.Instance.Player.GetTowerPlayedCount(GameController.Instance.PlayedTowerInfo.Id);
		}

		public void SendLevelFinishEvent(string finishStatus, string killedBy) {
			var towerId     = GameController.Instance.PlayedTowerInfo.Id;
			var playedCount = GameController.Instance.Player.GetTowerPlayedCount(GameController.Instance.PlayedTowerInfo.Id);
		}

		public void SendStageStartEvent() {
			var towerId     = GameController.Instance.PlayedTowerInfo.Id;
			var playedCount = GameController.Instance.Player.GetTowerPlayedCount(GameController.Instance.PlayedTowerInfo.Id);
			var waveId      = _spawnWaveController.WaveIdxForDraw;
		}

		public void SendStageFinishEvent(string finishStatus, string killedBy) {
			var towerId     = GameController.Instance.PlayedTowerInfo.Id;
			var playedCount = GameController.Instance.Player.GetTowerPlayedCount(GameController.Instance.PlayedTowerInfo.Id);
			var waveId      = _spawnWaveController.WaveIdxForDraw;
		}

		private void UpdateStatsInfoLabels() {
			UpdatePlayerStatLabel(PlayerStatType.Damage);
			UpdatePlayerStatLabel(PlayerStatType.HealthRegen);
			UpdatePlayerStatLabel(PlayerStatType.MetaCurrencyMult);
		}

		public void UpdatePlayerStatLabel(PlayerStatType type) {
			var statInfo = _playerView.Stats.GetStatByType(type);
			var roundVal = statInfo._value;
			if (type == PlayerStatType.HealthRegen)
				roundVal = (float) Math.Round(statInfo._value, 3);

			var statStr = Utility.GetStringForStats(type, roundVal);

			if (type == PlayerStatType.Damage)
				_playerAttackLabel.text = statStr;
			else if (type == PlayerStatType.HealthRegen)
				_playerRegenLabel.text = statStr;
			else if (type == PlayerStatType.MetaCurrencyMult) {
				var coinsMultVal = statInfo._value;

				var activeCard = GameController.Instance.Player.GetActiveCardByType(CardType.CoinsBoost);
				if (activeCard != null) {
					var cardConfig = GameController.Instance.DB.CardsDB.GetCardConfig(activeCard._type);
					var currLevel  = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(activeCard._count);
					var currVal    = cardConfig.GetCardValueByLvlUp(currLevel - 1);
					coinsMultVal += currVal;
				}

				_playerCoinsMultLabel.text = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", coinsMultVal);
			}
		}

		public void UpdateEnemyBaseStatsLabels() {
			var waveIdx = _spawnWaveController.WaveIdx;
			var info    = _enemyDB.GetEnemyByType(EnemyType.Common);

#if HARDCORE
			var addDamage = info.AddDamageByWave(waveIdx);
			var attack    = info.Damage + addDamage;
			attack += (int) (attack * GameController.Instance.PlayedTowerInfo.EnemyDamageMultiplier);

			var addHealth = info.AddHealthByWave(waveIdx);
			var health    = info.Health + addHealth;
			health += (int) (health * GameController.Instance.PlayedTowerInfo.EnemyHealthMultiplier);

#else
			var attack = info.GetEnemyDamage(waveIdx);
			var health = info.GetEnemyHealth(waveIdx);
#endif

			_enemyBaseAttack.text = attack.ToString();
			_enemyBaseHealth.text = health.ToString();
		}

		private void InitAbilityController() {
			_abilityController = new AbilityController();
			_abilityController.Initialize(_playerView, GameController.Instance.DB.AbilityDB);
		}

		private void InitWaveDB() {
			_waveDB = new WaveDB();

			var gameData = GameController.Instance.Config.GameData;
			var waveData = new List<WaveData>();

			var terrainId = GameController.Instance.PlayedTowerInfo.TerrainId;
			if (terrainId == 1) {
				if (GameController.Instance.PlayedTowerInfo.Id == 1)
					waveData = gameData.WaveParametersTower1;
				else if (GameController.Instance.PlayedTowerInfo.Id == 2)
					waveData = gameData.WaveParametersTower2;
				else
					waveData = gameData.GetWaveDataByTerrain(terrainId).WaveDatas;
			}
			else {
				waveData = gameData.GetWaveDataByTerrain(terrainId).WaveDatas;
			}

			_waveDB.Initialize(waveData);
		}

		private void InitBackSize() {
			_backContainer.BuildBack(GameController.Instance.PlayedTowerInfo.TerrainId);

			_neededGameRootScale   =  Vector3.one;
			_neededGameRootScale.x -= _currentRangeSteps * _scaleStepFromRange;
			_neededGameRootScale.y -= _currentRangeSteps * _scaleStepFromRange;

			_backContainer.BackRoot.localScale = _neededGameRootScale;
			_minionsLayer.transform.localScale = _neededGameRootScale;
		}

		private void UpdateCoinsLabel() {
			_coinsLabel.text = FormatNumHelper.GetNumStr((int) _coinsCount);
		}

		private void UpdateExpLabel() {
			_expLabel.text = FormatNumHelper.GetNumStr((int) _expCount);
		}

		private void InitButtons() {
			_exitButton.onClick.AddListener(OnExitClick);
			_attackSpeedButton.onClick.AddListener(OnAttackSpeedClick);

			_attackSpeedButton.gameObject.SetActive(false);

			if (!_tutorialController.IsTutorialComplete(TutorialId.Acceleration))
				_exitButton.gameObject.SetActive(false);
		}

		public void OnAdsAccelerationReward() {
			GameController.Instance.BlockInput(false);
			_playerFlags.AdsAccelerationTime = GameController.Instance.Config.AdsAccelerateTime;
			MyTime.maxAcceleration           = GameController.Instance.Config.AdsAccelerateReward;

			_gameHudBottom.SetAdsAcceleration();

			SetPauseState(false);
		}

		private void OnAttackSpeedClick() {
			_abilityController.TryActivateAbility(AbilityType.AddAttackSpeed);
		}

		private void OnExitClick() {
			ShowSettingsWindow(true);
		}

		private void ShowSettingsWindow(bool needAnim) {
			SetPauseState(true);

			var settings = new SettingsWS {
				NeedButtons = true,
				OnHomeClick = OnExitConfirmed,
				OnHide      = OnExitWndHide
			};

			GameController.Instance.WindowsController.Show(WindowType.SettingsWindow, settings, needAnim);
		}

		private void OnExitConfirmed() {
			SendStageFinishEvent("leave", "null");
			SendLevelFinishEvent("leave", "null");

			if (IsCanShowInterstitial()) {
				ShowInterstitial();
			}
			else {
				if (!_tutorialController.IsTutorialComplete(TutorialId.UpgradeScreen))
					_tutorialController.NeedUpgradeScreen = true;

				SceneController.LoadMenu();
			}
		}

		private void OnHideInterstitial() {
		}

		private void OnExitWndHide() {
			SetPauseState(false);
		}

		private void InitSpawnWavesController() {
			_spawnWaveController = new SpawnWaveController();
			_spawnWaveController.Initialize(_waveDB, this);

			UpdateWaveIdLabel(_spawnWaveController.WaveIdxForDraw);
			_waveProgressBar.SetProgress(1f);
		}

		public void UpdateWaveIdLabel(int waveId) {
			_currentWaveIdLabel.text = (waveId + 1).ToString();
		}

		private void InitSpawnPoints() {
			var spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
			if (spawnPoints == null) {
				Debug.LogError("Can't find spawn points!");
				return;
			}

			for (var i = 0; i < spawnPoints.Length; ++i) {
				var point = spawnPoints[i].GetComponent<SpawnPoint>();
				point.ID = i;

				_spawnPoints.Add(point);
			}
		}

		private void InitRangeObject() {
			var rangeConfig = GameController.Instance.Player.Config.GetStatInfoByType(PlayerStatType.Range);
			var rangeInfo   = _playerView.Stats.GetStatByType(PlayerStatType.Range);
			var rangeVal    = rangeInfo._value;

			var startVal = rangeConfig.Value;
			var maxVal   = rangeConfig.MaxValue;
			_currentRangeSteps = (int) ((rangeVal - startVal) / rangeConfig.UpgradeStep);

			// Шаг на который изменяется скейл при прокачки ренжа. Обычный скайл 1, минимальный 0.2. Отсюда разница 0.8 
			_scaleStepFromRange = 0.8f / ((maxVal - startVal) / rangeConfig.UpgradeStep);

			var rangeObjSize = _rangeObject.GetComponent<RectTransform>().sizeDelta;
			_attackRadius = (rangeObjSize.x / 2 + rangeObjSize.y / 2) / 2f;
		}

		private void InitPlayerView() {
			_playerView.Board              = this;
			_playerView.transform.position = _rangeObject.transform.position;

			var reviveCard = GameController.Instance.Player.GetActiveCardByType(CardType.Revive);
			SetReviveIconActive(reviveCard != null);
		}

		private void Update() {
			//var avgFrameRate = Time.frameCount / Time.time;
			//UpdateFrameRateLabel((int)avgFrameRate);

			UpdateButtonsClick();

			if (_state == GameBoardState.None)
				Play();
			else if (_state == GameBoardState.Play)
				UpdatePlayState();
			else if (_state == GameBoardState.EndRun)
				UpdateEndRunState();
		}

		public void SetEndRunState() {
			_uiLayer.GetComponent<CanvasGroup>().interactable = false;
			_state                                            = GameBoardState.EndRun;

			SendStageFinishEvent("lose", _playerView.KilledByType.ToString());
			SendLevelFinishEvent("lose", _playerView.KilledByType.ToString());
		}

		private void UpdateEndRunState() {
			if (!_needShowEndRunWindow)
				return;

			_timeToShowEndRunWindow -= MyTime.deltaTime;
			if (_timeToShowEndRunWindow <= 0f) {
				_needShowEndRunWindow = false;

				if (IsCanReviveAds())
					ShowReviveWindow();
				else
					ShowEndRunWindow();
			}
		}

		private bool IsCanReviveAds() {
			if (_playerFlags.PlayedCount < 2)
				return false;

			if (!_needShowReviveWindow)
				return false;

			return true;
		}

		private void ShowReviveWindow() {
			_needShowReviveWindow = false;

			var settings = new ReviveWS {
				OnClose  = () => { ShowEndRunWindow(true, 0.5f); },
				OnRevive = OnReviveAdsReward,
			};

			GameController.Instance.WindowsController.Show(WindowType.ReviveWindow, settings);
		}

		private void OnReviveAdsReward() {
			_state = GameBoardState.Play;

			_uiLayer.GetComponent<CanvasGroup>().interactable = true;
			GameController.Instance.BlockInput(false);

			ResumeEnemiesState();
			_playerView.SetInvincibleAfterAds(3f);

			_needShowEndRunWindow   = true;
			_timeToShowEndRunWindow = 4f;
		}

		private void ShowEndRunWindow(bool needKilledBy = true, float delay = 0f) {
			if (!_tutorialController.IsTutorialComplete(TutorialId.UpgradeScreen))
				_tutorialController.NeedUpgradeScreen = true;

			var towerInfo = GameController.Instance.Player.GetTowerInfo(GameController.Instance.PlayedTowerInfo.Id);

			var settings = new EndRunWS {
				WaveId          = _spawnWaveController.WaveIdxForDraw + 1,
				HighestWaveId   = towerInfo._maxWaveId,
				CoinsCount      = (int) _coinsCount,
				KilledEnemyType = _playerView.KilledByType,
				CardsCount      = _droppedCardsCount,
				OnHide          = CheckNeedShowCards,
				NeedKilledBy    = needKilledBy
			};

			GameController.Instance.WindowsController.Show(
				WindowType.EndRunWindow, settings, true, delay);
		}

		private void CheckNeedShowCards() {
			var showCardType = GameController.Instance.Player.GetCardForShow();
			if (showCardType != CardType.Empty && _tutorialController.IsTutorialComplete(TutorialId.NewCard)) {
				var cardInfo = GameController.Instance.Player.GetCard(showCardType);
				var settings = new UpgradeCardWS {
					Info   = cardInfo,
					OnHide = CheckNeedShowCards
				};

				GameController.Instance.WindowsController.Show(WindowType.UpgradeCardWindow, settings, true, 0.3f);
			}
			else {
				if (IsCanShowInterstitial()) {
					ShowInterstitial();
				}
				else {
					SceneController.LoadMenu();
				}
			}
		}

		private bool IsCanShowInterstitial() {
			if (_playerFlags.PlayedCount < 2)
				return false;

			return false;
		}

		private void ShowInterstitial() {
			SetPauseState(true);
		
		}

		private void Play() {
			_state = GameBoardState.Play;
			_playerView.SetAttackState();
			_spawnWaveController.StartSpawn();

			SendLevelStartEvent();
		}

		private void UpdatePlayState() {
			if (_playerView.IsDead)
				return;

			_spawnWaveController.Update();
			_abilityController.Update();

			UpdateCooldownLabels();
			UpdateWaveProgress();
			UpdateAdsAccelerationTime();
		}

		private void UpdateAdsAccelerationTime() {
			if (IsPause)
				return;

			if (_playerFlags.AdsAccelerationTime <= 0f)
				return;

			_playerFlags.AdsAccelerationTime -= Time.deltaTime;
			if (_playerFlags.AdsAccelerationTime <= 0f) {
				_playerFlags.AdsAccelerationTime = 0f;

				_gameHudBottom.SetDefaultAcceleration();
				_gameHudBottom.CheckNeedAdsAccelerate();
			}

			_gameHudBottom.UpdateAccelerateTimer((int) _playerFlags.AdsAccelerationTime);
		}

		private void LateUpdate() {
			UpdateTutorials();
		}

		private void UpdateTutorials() {
			if (GameController.Instance.WindowsController.OpenedWindowCount > 0)
				return;

			if (_killedEnemiesCount == 1 && !_tutorialController.IsTutorialComplete(TutorialId.FirstBattleUpgrade)) {
				_gameHudBottom.SetTabActive(TabType.Attack);
				ChangeExp(GameController.Instance.Config.StartCoreExp);
				_tutorialController.StartTutorial(TutorialId.FirstBattleUpgrade, FindTargetForTutor);

				SetPauseState(true);
			}
			else if (_spawnWaveController.WaveIdx > 1 && !_tutorialController.IsTutorialComplete(TutorialId.Acceleration)) {
				_gameHudBottom.ShowAccelerationRoot();
				_exitButton.gameObject.SetActive(true);
				_tutorialController.StartTutorial(TutorialId.Acceleration, FindTargetForTutor, FindClickTargetForTutor);

				SetPauseState(true);
			}
		}

		private GameObject FindTargetForTutor() {
			var currentStepTargetId = _tutorialController.CurrentStepTargetId;
			if (currentStepTargetId == "AttackUpgrade" || currentStepTargetId == "DefenceTab" || currentStepTargetId == "HealthUpgrade" ||
			    currentStepTargetId == "AccelerateRoot" || currentStepTargetId == "CurrencyObject")
				return _gameHudBottom.FindTargetForTutor(currentStepTargetId);

			return null;
		}

		private GameObject FindClickTargetForTutor() {
			var currentStepTargetId = _tutorialController.CurrentStepTargetId;
			if (currentStepTargetId == "AccelerateRoot")
				return _gameHudBottom.FindClickTargetForTutor(currentStepTargetId);

			return null;
		}

		public void SetPauseState(bool isPause) {
			if (isPause) {
				_prevState = _state;
				_state     = GameBoardState.Pause;
			}
			else {
				_state = _prevState;
			}

			_playerView.SetPauseState(isPause);
			for (var i = 0; i < _enemies.Count; ++i) {
				_enemies[i].SetPauseState(isPause);
			}

			for (var i = 0; i < _bullets.Count; ++i) {
				_bullets[i].SetPauseState(isPause);
			}
		}

		private void UpdateWaveProgress() {
			var progress = _spawnWaveController.GetCurrentWaveProgress();
			_waveProgressBar.SetProgress(1 - progress);
		}

		private void UpdateCooldownLabels() {
			UpdateAttackSpeedCooldown();
		}

		private void UpdateAttackSpeedCooldown() {
			var cooldown = _abilityController.GetCooldown(AbilityType.AddAttackSpeed);
			if (cooldown >= 0) {
				if (!_attackSpeedCooldownLabel.gameObject.activeSelf)
					_attackSpeedCooldownLabel.gameObject.SetActive(true);

				_attackSpeedCooldownLabel.text = ((int) cooldown).ToString();
			}
			else if (cooldown < 0 && _attackSpeedCooldownLabel.gameObject.activeSelf)
				_attackSpeedCooldownLabel.gameObject.SetActive(false);
		}

		public void SpawnEnemy(EnemyType enemyType, int pointId, int waveIdx) {
			var enemyInfo = _enemyDB.GetEnemyByType(enemyType);
			if (enemyInfo == null) {
				Debug.LogError("Enemy not found in DB!");
				return;
			}

			UpdateSpawnPointsCooldown();

			var        spawnPoints = GetSpawnPoints();
			SpawnPoint neededPoint = null;
			if (pointId == -1) {
				var neededPointId = Random.Range(0, spawnPoints.Count);
				neededPoint = spawnPoints[neededPointId];
			}
			else {
				neededPoint = _spawnPoints.FirstOrDefault(x => x.ID == pointId);
			}

			if (_playerFlags.NeedTutorialWave) {
				_playerFlags.NeedTutorialWave = false;
				neededPoint                   = _specialSpawnPoint;
			}

			if (neededPoint == null) {
				Debug.LogError($"Wrong point ID! id = {pointId}");
				return;
			}

			_prevSpawnPointIdx = neededPoint.ID;
			SetSpawnPointsCooldown();

			var enemyObj = Instantiate(enemyInfo.Prefab, _minionsLayer);
			enemyObj.transform.position   = neededPoint.transform.position;
			enemyObj.transform.localScale = Vector3.one;

			var enemy = enemyObj.GetComponent<EnemyView>();
			enemy.Initialize(enemyInfo, _playerView, waveIdx);
			enemy.SetDirection();
			enemy.OnDieAction = OnEnemyDie;
			enemy.GameBoard   = this;
			enemy.Spawn();

			_enemies.Add(enemy);
		}

		private List<SpawnPoint> GetSpawnPoints() {
			var availablePoints = _spawnPoints.FindAll(x => x.SpawnCooldown <= 0);
			return availablePoints;
		}

		private void SetSpawnPointsCooldown() {
			var leftPointId = _prevSpawnPointIdx - 1;
			if (leftPointId < 0)
				leftPointId = _spawnPoints.Count - 1;

			var rightPointId = _prevSpawnPointIdx + 1;
			if (rightPointId < 0)
				rightPointId = 0;

			for (var i = 0; i < _spawnPoints.Count; ++i) {
				if (_spawnPoints[i].ID == _prevSpawnPointIdx ||
				    _spawnPoints[i].ID == leftPointId ||
				    _spawnPoints[i].ID == rightPointId)
					_spawnPoints[i].SpawnCooldown = GameController.Instance.Config.SpawnPointsCooldown;
			}
		}

		private void UpdateSpawnPointsCooldown() {
			for (var i = 0; i < _spawnPoints.Count; ++i) {
				if (_spawnPoints[i].SpawnCooldown > 0)
					_spawnPoints[i].SpawnCooldown--;
			}
		}

		private void DropCardAction(Vector3 pos) {
			var cardObj = Instantiate(_cardDropPrefab, _overGameLayer);
			cardObj.GetComponent<Canvas>().sortingLayerName = "OverGameLayer";
			cardObj.GetComponent<Canvas>().sortingOrder     = 1000;
			cardObj.transform.position                      = pos;

			var anim = cardObj.GetComponent<Animator>();
			anim.enabled = false;

			var localPos = cardObj.transform.localPosition;
			localPos.z                      = 0;
			cardObj.transform.localPosition = localPos;
			cardObj.transform.localScale    = new Vector3(0.8f, 0.8f, 1f);

			var shiftDir = 1;
			if (pos.x < 0)
				shiftDir = -1;

			var startPos = localPos;
			var endPos   = startPos;
			endPos.x += shiftDir * 50;

			var flyPath = Utility.BuildFlyCurve(startPos, endPos, shiftDir * 80, 1f);
			LeanTween.moveLocal(cardObj, flyPath, 0.4f);

			var playerPos = _playerView.transform.position;
			LeanTween.move(cardObj, playerPos, 1.2f)
				.setDelay(1f)
				.setEase(LeanTweenType.easeOutCubic)
				.setOnStart(() => { anim.enabled = true; })
				.setOnComplete(() => { Destroy(cardObj, 2); });
		}

		private float ApplayCoinsMultiplier(float coinsCount) {
			coinsCount *= GameController.Instance.PlayedTowerInfo.CurrencyMultiplier;

			var playerCoinsMult = _playerView.Stats.GetStatByType(PlayerStatType.MetaCurrencyMult);
			var cardCoinsMult   = GameController.Instance.CardsController.GetCoinsBoostVal();

			coinsCount *= (playerCoinsMult._value + cardCoinsMult);
			return coinsCount;
		}

		private float ApplayExpMultiplier(float expCount) {
			var playerExpMult = _playerView.Stats.GetStatByType(PlayerStatType.CoreCurrencyMult);
			expCount *= playerExpMult._value;
			return expCount;
		}

		private void OnEnemyDie(EnemyView enemy, float coinsCount, float expCount) {
			SpawnDeathFX(enemy);

			if (_killedEnemiesCount < 100000)
				_killedEnemiesCount++;

			_enemies.Remove(enemy);

			if (enemy.Type == EnemyType.Boss || enemy.Type == EnemyType.BossSimple) {
				var playedTowerId = GameController.Instance.PlayedTowerInfo.Id;
				var currentWaveId = enemy.WaveIdx;

				var cardTypeForDrop = GameController.Instance.CardsController.CheckNeedDropCard(playedTowerId, currentWaveId);
				if (cardTypeForDrop != CardType.Empty) {
					GameController.Instance.PlayedTowerInfo.IsCardDrop = true;
					DropCardAction(enemy.transform.position);
					_droppedCardsCount++;
				}

				var killedBossInfo = new KilledBossInfo {
					TowerId = playedTowerId,
					WaveId  = currentWaveId
				};

				_playerFlags.SetBossKilled(killedBossInfo);
			}

			if (enemy.Type == EnemyType.Explode) {
				if (IsPlayerInExplodeRange(enemy))
					_playerView.TakeDamage(enemy.Damage, enemy.Type);
			}

			if (enemy.WaveIdx == _spawnWaveController.WaveIdx &&
			    _enemies.Count == 0 &&
			    _spawnWaveController.IsWaveEnd &&
			    _spawnWaveController.NextSpawnAfterDefeat) {
				AddCurrencyForWave();
				_spawnWaveController.CheckNeedDelayState();
			}

			coinsCount = ApplayCoinsMultiplier(coinsCount);
			GameController.Instance.Player.ChangeCoinsCount(coinsCount, "enemy");

			expCount = ApplayExpMultiplier(expCount);
			ChangeExp(expCount);
		}

		private bool IsPlayerInExplodeRange(EnemyView enemy) {
			var playerPos    = _playerView.transform.position;
			var playerRadius = _playerView.GetRadius();
			var enemyPos     = enemy.transform.position;

			var scale = _minionsLayer.lossyScale;
			var dist  = (playerPos - enemyPos).magnitude;
			if (dist - playerRadius * scale.x <= GameController.Instance.Config.ExplodeRange * scale.x)
				return true;

			return false;
		}

		public void AddRewardForSkippedWave(float coinsCount, float expCount) {
			expCount = ApplayExpMultiplier(expCount);
			ChangeExp(expCount);

			var coinsForAllEnemies = ApplayCoinsMultiplier(coinsCount);
			GameController.Instance.Player.ChangeCoinsCount(coinsForAllEnemies, "skip_wave");

			AddCurrencyForWave((int) coinsForAllEnemies, (int) expCount);
		}

		public void AddCurrencyForWave(int coinsCount = 0, int expCount = 0) {
			var coinsForWave    = _playerView.Stats.GetStatByType(PlayerStatType.MetaCurrencyForWave);
			var coinsForWaveVal = ApplayCoinsMultiplier(coinsForWave._value);
			GameController.Instance.Player.ChangeCoinsCount(coinsForWaveVal, "complete_wave");

			var expForWave    = _playerView.Stats.GetStatByType(PlayerStatType.CoreCurrencyForWave);
			var expForWaveVal = ApplayExpMultiplier(expForWave._value);
			ChangeExp(expForWaveVal);

			var coinsForDraw = coinsForWaveVal + coinsCount;
			var expForDraw   = expForWaveVal + expCount;
			StartEndWaveAnim((int) coinsForDraw, (int) expForDraw);
		}

		private void StartEndWaveAnim(int coinsCount, int expCount) {
			var pos = _playerView.transform.position;
			pos.y += 50;

			var endPos = pos;
			endPos.y += 200;

			var countObject = Instantiate(_completeWaveCountPrefab, _overGameLayer);
			countObject.GetComponent<WaveCompleteCount>().Initialize(expCount, coinsCount);
			countObject.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
			countObject.transform.position   = pos;

			var countCg = countObject.GetComponent<CanvasGroup>();
			countCg.alpha = 0f;

			LeanTween.alphaCanvas(countCg, 1f, 0.2f);
			LeanTween.scale(countObject, Vector3.one, 0.4f).setEase(LeanTweenType.easeOutBack);
			LeanTween.move(countObject, endPos, 1f);
			LeanTween.alphaCanvas(countCg, 0f, 0.1f).setDelay(0.9f).setOnComplete(() => { Destroy(countObject); });
		}

		private List<EnemyView> GetEnemiesInRange(Vector3 pos, EnemyView excludeEnemy) {
			var enemiesInRange = new List<EnemyView>();
			var rangeInPixels  = GetRangeInPixels();
			for (var i = 0; i < _enemies.Count; ++i) {
				if (excludeEnemy == _enemies[i])
					continue;

				if (_enemies[i].IsDead || _enemies[i].SkipFromSearch)
					continue;

				var enemyPos = _enemies[i].transform.position;
				var dist     = (enemyPos - pos).magnitude;

				if (dist <= rangeInPixels) {
					enemiesInRange.Add(_enemies[i]);
				}
			}

			return enemiesInRange;
		}

		public EnemyView GetNearestEnemy(Vector3 pos) {
			EnemyView nearestEnemy = null;
			var       nearestDist  = 1000000f;

			for (var i = 0; i < _enemies.Count; ++i) {
				if (_enemies[i].IsDead || _enemies[i].SkipFromSearch)
					continue;

				var enemyPos = _enemies[i].transform.position;
				var dist     = (enemyPos - pos).magnitude;

				if (dist < nearestDist) {
					nearestDist  = dist;
					nearestEnemy = _enemies[i];
				}
			}

			var rangeInPixels = GetRangeInPixels();
			if (nearestDist <= rangeInPixels)
				return nearestEnemy;

			return null;
		}

		public float GetRangeInPixels() {
			return _attackRadius * _mainCanvasRT.localScale.x;
		}

		public void UpdatePlayerHealth() {
			_playerView.UpdateHealth();
		}

		public void UpdateRange() {
			SetGameRootScale();
		}

		public void UpdateShotPerSec() {
			_playerView.CalcTimeToShot();
		}

		private void SetGameRootScale() {
			if (_gameRootSizeTween != null)
				LeanTween.cancel(_gameRootSizeTween.uniqueId);

			if (_backSizeTween != null)
				LeanTween.cancel(_backSizeTween.uniqueId);

			_neededGameRootScale.x -= _scaleStepFromRange;
			_neededGameRootScale.y -= _scaleStepFromRange;

			_gameRootSizeTween = LeanTween.scale(_gameLayer.gameObject, _neededGameRootScale, 0.5f).setOnUpdate(
				(float val) => {
					var pos = _rangeObject.transform.position;
					_playerView.transform.position = pos;
				}).setOnComplete(
				() => { _gameRootSizeTween = null; });

			_backSizeTween = LeanTween.scale(_backContainer.BackRoot.gameObject, _neededGameRootScale, 0.5f).setOnUpdate(
				(float val) => { _backContainer.CheckObjectsVisible(); }).setOnComplete(
				() => { _backSizeTween = null; });
		}

		private void SpawnDeathFX(EnemyView enemy) {
			var deathFx = Instantiate(enemy.DeathFx, _overGameLayer);
			deathFx.transform.position = enemy.transform.position;

			Utility.SetParticleSorting(deathFx, "OverGameLayer", 100);
			Utility.SetParticlesScale(deathFx, enemy.transform.lossyScale.x);

			Destroy(deathFx, 1.5f);
		}

		public void SetEnemiesIdleState() {
			for (var i = 0; i < _enemies.Count; ++i) {
				var delay = Random.Range(0f, 0.2f);
				_enemies[i].SetIdleState(delay);
			}
		}

		private void ResumeEnemiesState() {
			for (var i = 0; i < _enemies.Count; ++i) {
				_enemies[i].ResumeState();
			}
		}
	}
}
