using System.Collections.Generic;
using Core.Config;
using Core.Config.Towers;
using Core.Config.Tutorial;
using Core.Controllers;
using Core.Controllers.Tutorial;
using Core.Controllers.Windows;
using Core.Utils;
using Game.Player;
using Ui.Hud;
using UnityEngine;
using Window;
using Utility = Core.Utils.Utility;

namespace Game.Map {
	public class TowerObject : MonoBehaviour {
		[SerializeField] private int                    _id;
		[SerializeField] private List<int>              _unlockedTowerIds = new List<int>();
		[SerializeField] private List<MineObject>       _linkedMines      = new List<MineObject>();
		[SerializeField] private List<MapChestObject>   _linkedChests     = new List<MapChestObject>();
		[SerializeField] private MapGrandChestObject    _linkedGrandChest;
		[SerializeField] private List<BezierPathObject> _linkedPaths = new List<BezierPathObject>();

		[SerializeField] private BoxCollider2D  _clickCollider;
		[SerializeField] private GameObject     _towerEmpty;
		[SerializeField] private GameObject     _towerPlace;
		[SerializeField] private GameObject     _towerMain;
		[SerializeField] private SpriteRenderer _range;
		[SerializeField] private GameObject     _rangeViewTmp;
		[SerializeField] private GameObject     _callToActionAnim;

		private List<GameObject> _linkedFog = new List<GameObject>();

		private TowerData          _towerData;
		private TutorialController _tutorialController;
		private WindowsController  _windowController;
		private IPlayer            _player;
		private MapObject          _mapObject;
		private MainConfig         _mainConfig;

		private bool  _canRotateRange = true;
		private bool  _unlocked;
		private float _viewRange;
		private float _lockedViewRange;

		public int  ID            => _id;
		public bool Unlocked      => _unlocked;
		public int  UnlockedPrice => _towerData.UnlockPrice;

		private void Start() {
			_tutorialController = GameController.Instance.TutorialController;
			_windowController   = GameController.Instance.WindowsController;
			_player             = GameController.Instance.Player;
			_mainConfig         = GameController.Instance.Config;
		}

		public void RemoveFog(bool needTween) {
			if (!IsTowerAvailable())
				return;

			var delta = _mainConfig.ViewRangeDeltaForFog;
			for (var i = 0; i < _linkedFog.Count; ++i) {
				var alpha = 0.8f;
				if (IsObjectInViewRange(_linkedFog[i].transform.position, delta, true))
					alpha = 0.7f;

				if (IsObjectInViewRange(_linkedFog[i].transform.position, delta / 2, true))
					alpha = 0.6f;

				if (IsObjectInViewRange(_linkedFog[i].transform.position, 0, true)) {
					_linkedFog[i].SetActive(false);
					continue;
				}

				_linkedFog[i].GetComponent<FogObject>().SetAlpha(alpha, needTween);
			}
		}

		public void Initialize(MapObject mapObject) {
			_mapObject = mapObject;
			_towerData = GameController.Instance.DB.TowersDB.GetTowerData(_id);

			UpdateView();
		}

		public void UpdateView() {
			UpdateUnlockedState();
			UpdateViewRange();
			InitView();
			InitRange();
			InitChests();
			LinkTowerToMines();
		}

		private void Update() {
			UpdateRangeAnim();
		}

		private void UpdateRangeAnim() {
			if (!_unlocked)
				return;

			if (!_canRotateRange)
				return;

			var angles = _range.transform.eulerAngles;
			angles.z                     += 10 * Time.deltaTime;
			_range.transform.eulerAngles =  angles;
		}

		private void UpdateUnlockedState() {
			_unlocked = _player.IsTowerUnlocked(_id);
		}

		private void UpdateViewRange() {
			_viewRange       = _mainConfig.TowerViewRange;
			_lockedViewRange = _mainConfig.TowerLockedViewRange;

			_rangeViewTmp.gameObject.SetActive(false);
		}

		private void InitChests() {
			for (var i = 0; i < _linkedChests.Count; ++i) {
				_linkedChests[i].Initialize(this);

				var isChestCollected = _player.IsChestCollected(_linkedChests[i].ID);
				_linkedChests[i].gameObject.SetActive(_unlocked && !isChestCollected);
			}

			if (_linkedGrandChest) {
				_linkedGrandChest.SetActive(_unlocked);
				_linkedGrandChest.UpdateView();
			}
		}

		private void InitView() {
			var isAvailable = IsTowerAvailable();

			var needCallToAction = !(!isAvailable || _unlocked);
			_callToActionAnim.SetActive(needCallToAction);

			_towerEmpty.SetActive(!_unlocked && !isAvailable);
			_towerPlace.SetActive(!_unlocked && isAvailable);
			_towerMain.SetActive(_unlocked);

			var colliderSize = new Vector2(150, 150);
			if (_unlocked)
				colliderSize = new Vector2(120, 120);

			_clickCollider.size = colliderSize;
		}

		private void InitRange() {
			_range.gameObject.SetActive(_unlocked);

			if (!_unlocked)
				return;

			_range.drawMode = SpriteDrawMode.Sliced;

			var info      = GameController.Instance.Player.GetTowerInfo(_id);
			var minRange  = _mainConfig.MinTowerRange;
			var maxRange  = _mainConfig.MaxTowerRange;
			var rangeStep = _mainConfig.TowerRangeStep;

			var rangeStepId = info._maxWaveId / 10;
			var currentSize = minRange + rangeStep * info._rangeStepId;
			var neededSize  = minRange + rangeStep * rangeStepId;

			if (currentSize > maxRange)
				currentSize = maxRange;

			_range.size = new Vector2(currentSize * 2, currentSize * 2);

			if (neededSize > maxRange)
				neededSize = maxRange;

			var needAnim = neededSize != currentSize;
			if (needAnim) {
				CheckNeedUnlockMines(neededSize);

				info._rangeStepId = rangeStepId;

				GameController.Instance.BlockInput(true);
				LeanTween.rotateZ(_range.gameObject, 180, 1f);
				LeanTween.value(_range.gameObject, currentSize, neededSize, 1f).setOnUpdate(
					(range) => { _range.size = new Vector2(range * 2, range * 2); }).setOnComplete(
					() => {
						GameController.Instance.BlockInput(false);
						CheckNeedUnlockChests(neededSize);
						CheckNeedNewCardTutor();
					});
			}
			else {
				CheckNeedNewCardTutor();
			}
		}

		private void CheckNeedNewCardTutor() {
			if (_id != GameController.Instance.PlayedTowerInfo.Id)
				return;

			if (GameController.Instance.PlayedTowerInfo.IsCardDrop &&
			    !_tutorialController.IsTutorialComplete(TutorialId.NewCard)) {
				_tutorialController.NeedNewCard = true;
				_player.ClearNeedShowCards();
			}
			else {
				_mapObject.MenuBoard.CanShowNewCard = true;
			}
		}

		private void LinkTowerToMines() {
			for (var i = 0; i < _linkedMines.Count; ++i)
				_linkedMines[i].AddTower(this);
		}

		private void OnMouseDown() {
			if (!GameController.Instance.IsCanClick())
				return;

			OnTowerClick();
		}

		public void OnTowerClick() {
			if (IsTowerAvailable()) {
#if REC_VIDEO
				if (!_unlocked) {
					GameController.Instance.Player.UnlockTower(_id);
					UnlockTowerAction();
				}
				else {
					StartTowerWindow(_unlocked);
				}
#else
				StartTowerWindow(_unlocked);
#endif
			}
		}

		public void StartTowerWindow(bool unlocked) {
			var settings = new TowerWS {
				TowerId               = _id,
				UnlockPrice           = _towerData.UnlockPrice,
				TerrainId             = _towerData.TerrainId,
				StartWaveId           = _towerData.StartWaveId,
				CurrencyMultiplier    = _towerData.CurrencyMult,
				IsTowerUnlocked       = unlocked,
				OnUnlockAction        = UnlockTowerAction,
				EnemyDamageMultiplier = _towerData.EnemiesDamageMult,
				EnemyHealthMultiplier = _towerData.EnemiesHealthMult,
				OnSkillClick          = OnSkillClick
			};

			_windowController.Show(WindowType.TowerWindow, settings);
		}

		private void OnSkillClick() {
			_mapObject.MenuBoard.HudBottom.SetButtonActive(HudButtonType.Cards, true);
			_mapObject.NeedOpenTowerId = _id;
		}

		private void UnlockTowerAction() {
			GameController.Instance.BlockInput(true);

			var focusInfo = new FocusInfo {
				Object          = gameObject.transform,
				OnExecuteAction = UnlockTowerAnim
			};

			_mapObject.MenuBoard.UpdateUpgradeScreen();
			_mapObject.FocusController.AddQueue(focusInfo);
			_mapObject.FocusController.StartQueue();
		}

		private void ShowFirstRangeAction() {
			_range.gameObject.SetActive(true);
			_range.size = Vector2.zero;

			var minRange = _mainConfig.MinTowerRange;
			LeanTween.rotateZ(_range.gameObject, 180, 1f);
			LeanTween.value(_range.gameObject, 0f, minRange, 1f).setOnUpdate(
				range => { _range.size = new Vector2(range * 2, range * 2); }).setDelay(0.2f);
		}

		private void UnlockTowerAnim() {
			UpdateViewRange();
			UpdateUnlockedState();
			InitView();

			var delta           = _mainConfig.ViewRangeDeltaForFog;
			var delay           = 0f;
			var startLevelDelay = 0f;
			var showTime        = 0.1f;

			var shuffledList = Utility.ShuffleList(_linkedFog);
			for (var i = 0; i < shuffledList.Count; ++i) {
				if (!shuffledList[i].activeSelf)
					continue;

				var alpha = 0.7f;
				if (IsObjectInViewRange(shuffledList[i].transform.position, delta / 2))
					alpha = 0.6f;

				if (IsObjectInViewRange(shuffledList[i].transform.position)) {
					LeanTween.scale(shuffledList[i], Vector3.zero, 0.1f).setDelay(delay);
					startLevelDelay += showTime;
					delay           += 0.01f;
					continue;
				}

				var wasSet = shuffledList[i].GetComponent<FogObject>().SetAlpha(alpha, true, delay);
				if (wasSet) {
					startLevelDelay += showTime;
					delay           += 0.01f;
				}
			}

			LeanTween.delayedCall(showTime + delay + 0.5f,
				() => {
					for (var i = 0; i < _linkedPaths.Count; ++i) {
						_linkedPaths[i].gameObject.SetActive(true);
						_linkedPaths[i].UpdateNextTower();
						_linkedPaths[i].InitPath();
						_linkedPaths[i].ShowPath();
					}

					ShowFirstRangeAction();
					ShowChests(1.2f);
				});
		}

		private void ShowChests(float delay) {
			for (var i = 0; i < _linkedChests.Count; ++i) {
				_linkedChests[i].gameObject.SetActive(true);
				_linkedChests[i].SetDisableSkin();
				_linkedChests[i].transform.localScale = Vector3.zero;

				LeanTween.scale(_linkedChests[i].gameObject, Vector3.one, 0.3f).setDelay(delay).setEase(LeanTweenType.easeOutBack);
				delay += 0.3f + 0.05f;
			}

			if (_linkedGrandChest && !_linkedGrandChest.IsActive) {
				_linkedGrandChest.SetActive(true);
				_linkedGrandChest.transform.localScale = Vector3.zero;
				LeanTween.scale(_linkedGrandChest.gameObject, Vector3.one, 0.3f).setDelay(delay).setEase(LeanTweenType.easeOutBack);
				delay += 0.3f;
			}

			LeanTween.delayedCall(delay + 0.5f,
				() => {
					GameController.Instance.BlockInput(false);

#if !REC_VIDEO
					if (!CheckNeedTutor())
						StartTowerWindow(_unlocked);
#endif
				});
		}

		private bool CheckNeedTutor() {
			if (_id != 2)
				return false;

			if (!_tutorialController.IsTutorialComplete(TutorialId.FirstInfoRange)) {
				_tutorialController.StartTutorial(TutorialId.FirstInfoRange, FindTargetForTutor, FindClickTargetForTutor);
				return true;
			}

			return false;
		}

		private GameObject FindTargetForTutor() {
			var currentStepTargetId = _tutorialController.CurrentStepTargetId;
			if (currentStepTargetId == "Mine")
				return _linkedMines[0].gameObject;
			else if (currentStepTargetId == "Tower2")
				return gameObject;

			return null;
		}

		private GameObject FindClickTargetForTutor() {
			var currentStepTargetId = _tutorialController.CurrentStepTargetId;
			if (currentStepTargetId == "Tower2")
				return gameObject;

			return null;
		}

		private void CheckNeedUnlockMines(int neededSize) {
			for (var i = 0; i < _linkedMines.Count; ++i) {
				var minePos       = _linkedMines[i].transform.localPosition;
				var nearestLength = (transform.localPosition - minePos).magnitude;
				if (nearestLength <= neededSize) {
					if (_player.IsMineUnlocked(_linkedMines[i].ID))
						continue;

					GameController.Instance.Player.UnlockMine(_linkedMines[i].ID, _linkedMines[i].StartLevelUp);
					_mapObject.FocusController.AddQueue(_linkedMines[i].GetFocusInfo());
				}
			}

			_mapObject.FocusController.StartQueue();
		}

		private void CheckNeedUnlockChests(int neededSize) {
			var needChestTutor = false;
			for (var i = 0; i < _linkedChests.Count; ++i) {
				var chestPos      = _linkedChests[i].transform.localPosition;
				var nearestLength = (transform.localPosition - chestPos).magnitude;
				if (nearestLength <= neededSize) {
					_linkedChests[i].SetActiveSkin();

					if (_linkedChests[i].Type == MapChestType.RewardedAds)
						continue;

					var isChestUnlocked = _player.IsChestCollected(_linkedChests[i].ID);
					if (!isChestUnlocked) {
						_tutorialController.FirstChestObject = _linkedChests[i].gameObject;
						needChestTutor                       = true;
					}
				}
			}

			if (_linkedGrandChest) {
				var chestPos      = _linkedGrandChest.transform.localPosition;
				var nearestLength = (transform.localPosition - chestPos).magnitude;

				if (nearestLength <= neededSize)
					_linkedGrandChest.SetActiveAnim();
			}

			if (needChestTutor)
				CheckChestTutor();
		}

		private void CheckChestTutor() {
			if (!_tutorialController.IsTutorialComplete(TutorialId.FirstChest))
				_tutorialController.NeedFirstChest = true;
		}

		public bool IsObjectInRange(Vector3 objPos) {
			var length = (transform.localPosition - objPos).magnitude;
			if (length <= _range.size.x / 2f)
				return true;

			return false;
		}

		public bool IsObjectInViewRange(Vector3 objPos, int delta = 0, bool needCheckLocked = false) {
			var range = _viewRange;
			if (needCheckLocked && !_unlocked)
				range = _lockedViewRange;

			var currentZoom = _mapObject.CurrentMapZoom;
			var length      = (transform.position - objPos).magnitude;
			if (length <= (range + delta) * currentZoom)
				return true;

			return false;
		}

		public void LinkFogObject(GameObject fogObject) {
			_linkedFog.Add(fogObject);
		}

		public bool IsTowerAvailable() {
			if (_unlockedTowerIds.Count == 0)
				return true;

			var player = GameController.Instance.Player;

			var isAvailable = false;
			for (var i = 0; i < _unlockedTowerIds.Count; ++i) {
				//var towerInfo = player.GetTowerInfo(_unlockedTowerIds[i]);
				if (player.IsTowerUnlocked(_unlockedTowerIds[i]) /*&& towerInfo._playedCount > 0*/) {
					isAvailable = true;
					break;
				}
			}

			return isAvailable;
		}
	}
}
