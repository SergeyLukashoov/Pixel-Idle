using System;
using System.Collections.Generic;
using System.Linq;
using Core.Controllers;
using Core.Utils;
using Game.Boards;
using Game.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Map {
	public class MapObject : MonoBehaviour {
		[SerializeField] private MenuBoard     _menuBoard;
		[SerializeField] private GameObject    _hudBottomObject;
		[SerializeField] private MainBase      _mainBase;
		[SerializeField] private RectTransform _mapLayer;
		[SerializeField] private GameObject    _handPrefab;
		[SerializeField] private GameObject    _fogPrefab;
		[SerializeField] private Transform     _fogLayer;
		[SerializeField] private Transform     _overMapLayer;

		private List<TowerObject>         _towersList      = new List<TowerObject>();
		private List<MineObject>          _mineList        = new List<MineObject>();
		private List<MapGrandChestObject> _grandChestsList = new List<MapGrandChestObject>();
		private List<BezierPathObject>    _pathList        = new List<BezierPathObject>();

		private TowerObject     _targetForHand;
		private GameObject      _handObject;
		private FocusController _focusController;
		private IPlayer         _player;
		private Vector2         _mapSize          = Vector2.zero;
		private Vector3         _clickOffset      = Vector2.zero;
		private Vector3         _mouseDownPos     = Vector2.zero;
		private float           _mapBottomShift   = 150f;
		private float           _currentMapZoom   = 1f;
		private bool            _inMouseDownState = false;
		private bool            _inMapDragState   = false;
		private bool            _isFocus          = true;

		public FocusController FocusController => _focusController;
		public MenuBoard       MenuBoard       => _menuBoard;
		public bool            IsZoomOut       => transform.localScale.x < 1f;
		public float           CurrentMapZoom  => _currentMapZoom;
		public int             NeedOpenTowerId { get; set; } = -1;

		private void OnDestroy() {
			OnDestroySave();
		}

		private void OnDestroySave() {
			SaveMineInfo();
			SaveGrandChestsInfo();
		}

		private void OnApplicationFocus(bool hasFocus) {
			if (_isFocus != hasFocus) {
				if (!hasFocus)
					OnDestroySave();
				else
					OnGetFocus();

				_isFocus = hasFocus;
			}
		}

		private void OnGetFocus() {
			ReinitMines();
			ReinitGrandChests();
		}

		private void ReinitMines() {
			var unlockedMinesInfo = GameController.Instance.Player.Mines._minesInfos;
			if (unlockedMinesInfo.Count == 0)
				return;

			var nowSeconds        = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
			var deltaFromLastSave = (int) (nowSeconds - _player.Flags.MineSaveTime);
			if (_player.Flags.MineSaveTime <= 0)
				deltaFromLastSave = 0;

			for (var i = 0; i < unlockedMinesInfo.Count; ++i) {
				var mineObj = _mineList.FirstOrDefault(x => x.ID == unlockedMinesInfo[i]._id);
				if (mineObj == null) {
					Debug.LogError($"Mine {unlockedMinesInfo[i]._id} not found in scene!");
					continue;
				}

				mineObj.Initialize(this, deltaFromLastSave);
			}
		}

		private void ReinitGrandChests() {
			var unlockedGrandChestsInfo = GameController.Instance.Player.GrandChests._grandChestInfos;
			if (unlockedGrandChestsInfo.Count == 0)
				return;

			var nowSeconds        = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
			var deltaFromLastSave = (int) (nowSeconds - _player.Flags.GrandChestSaveTime);
			if (_player.Flags.GrandChestSaveTime <= 0)
				deltaFromLastSave = 0;

			for (var i = 0; i < unlockedGrandChestsInfo.Count; ++i) {
				if (unlockedGrandChestsInfo[i]._resourceLeft <= 0)
					continue;

				var grandChestObj = _grandChestsList.FirstOrDefault(x => x.ID == unlockedGrandChestsInfo[i]._id);
				if (grandChestObj == null) {
					Debug.LogError($"Mine {unlockedGrandChestsInfo[i]._id} not found in scene!");
					continue;
				}

				grandChestObj.InitUnlockedState(deltaFromLastSave);
			}
		}

		private void SaveMineInfo() {
			var unlockedMinesInfo = GameController.Instance.Player.Mines._minesInfos;
			if (unlockedMinesInfo.Count == 0)
				return;

			for (var i = 0; i < unlockedMinesInfo.Count; ++i) {
				var info    = unlockedMinesInfo[i];
				var mineObj = _mineList.FirstOrDefault(x => x.ID == info._id);
				if (mineObj == null) {
					Debug.LogError($"Mine {info._id} not found in scene!");
					continue;
				}

				info._producedCount = mineObj.CurrentProducedCount;
				info._produceTime   = mineObj.CurrentProduceTime;
			}

			GameController.Instance.Player.Flags.MineSaveTime = (long) TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
			GameController.Instance.Save();
		}

		private void SaveGrandChestsInfo() {
			var unlockedGrandChestInfos = GameController.Instance.Player.GrandChests._grandChestInfos;
			if (unlockedGrandChestInfos.Count == 0)
				return;

			for (var i = 0; i < unlockedGrandChestInfos.Count; ++i) {
				var info = unlockedGrandChestInfos[i];
				if (info._resourceLeft <= 0)
					continue;

				var grandChestObject = _grandChestsList.FirstOrDefault(x => x.ID == info._id);
				if (grandChestObject == null) {
					Debug.LogError($"Mine {info._id} not found in scene!");
					continue;
				}

				info._currentProduceTime = grandChestObject.CurrentProduceTime;
				info._produceTime        = grandChestObject.ProduceTime;
				info._resourceLeft       = grandChestObject.ResourcesLeft;
			}

			GameController.Instance.Player.Flags.GrandChestSaveTime = (long) TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
			GameController.Instance.Save();
		}

		public void Initialize() {
			_player          = GameController.Instance.Player;
			_mapSize         = GetComponent<RectTransform>().sizeDelta;
			_focusController = new FocusController(this);

			InitMapLayer();
			InitPath();
			InitTowers();
			InitMines();
			InitGrandChest();
			InitFog();
			SetMapPos();
			RemoveFog();

			//_targetForHand = GetTowerForHand();
		}

		private TowerObject GetTowerForHand() {
			var availableTowers = new List<TowerObject>();
			for (var i = 0; i < _towersList.Count; ++i) {
				if (_towersList[i].IsTowerAvailable() &&
				    !_towersList[i].Unlocked &&
				    _player.IsHaveCoins(_towersList[i].UnlockedPrice))
					availableTowers.Add(_towersList[i]);
			}

			TowerObject target;
			if (availableTowers.Count > 0) {
				var randIdx = Random.Range(0, availableTowers.Count);
				target = availableTowers[randIdx];
			}
			else {
				target = _towersList[0];
				for (var i = 0; i < _towersList.Count; ++i) {
					if (_towersList[i].Unlocked && _towersList[i].ID > target.ID)
						target = _towersList[i];
				}
			}

			return target;
		}

		private void InitPath() {
			var pathArr = GameObject.FindGameObjectsWithTag("Path");
			for (var i = 0; i < pathArr.Length; ++i) {
				var pathObj = pathArr[i].GetComponent<BezierPathObject>();
				_pathList.Add(pathObj);
			}
		}

		private void RemoveFog() {
			_mainBase.RemoveFog();

			for (var i = 0; i < _towersList.Count; ++i) {
				_towersList[i].RemoveFog(false);
			}
		}

		private void InitFog() {
			var fogLayerSize = _fogLayer.GetComponent<RectTransform>().sizeDelta;
			var startPos     = new Vector2(-fogLayerSize.x / 2f, -fogLayerSize.y / 2f);

			var stepX       = 70f;
			var stepY       = 63f;
			var startShiftX = 35f;

			var xCount = fogLayerSize.x / stepX + 1;
			var yCount = fogLayerSize.y / stepY + 1;

			for (var i = 0; i < yCount; ++i) {
				var posY = startPos.y + i * stepY;

				var needShiftX = i % 2 > 0;

				for (var j = 0; j < xCount; ++j) {
					var posX = startPos.x + j * stepX;
					if (needShiftX)
						posX -= startShiftX;

					var fogObj = Instantiate(_fogPrefab, _fogLayer);
					fogObj.transform.localPosition = new Vector3(posX, posY);
					fogObj.GetComponent<FogObject>().SetAlpha(190 / 255f);

					LinkFogToTowerOrMine(fogObj);
				}
			}
		}

		private void LinkFogToTowerOrMine(GameObject fogObj) {
			var delta = GameController.Instance.Config.ViewRangeDeltaForFog;
			if (_mainBase.IsObjectInViewRange(fogObj.transform.position, delta)) {
				_mainBase.LinkFogObject(fogObj);
			}

			for (var i = 0; i < _towersList.Count; ++i) {
				if (_towersList[i].IsObjectInViewRange(fogObj.transform.position, delta)) {
					_towersList[i].LinkFogObject(fogObj);
				}
			}

			fogObj.GetComponent<FogObject>().SetAlpha(0.8f);
		}

		public GameObject GetTowerById(int id) {
			var towerObject = _towersList.FirstOrDefault(x => x.ID == id);
			if (towerObject == null) {
				Debug.LogError($"Wrong tower ID = {id}");
				return null;
			}

			return towerObject.gameObject;
		}

		public void SetMapZoom(float neededZoom) {
			if (_mapLayer.sizeDelta.x / _mapSize.x > neededZoom)
				neededZoom = _mapLayer.sizeDelta.x / _mapSize.x;

			var currentMapZoom = transform.localScale.x;
			_currentMapZoom = neededZoom;

			GameController.Instance.BlockInput(true);
			LeanTween.value(gameObject, currentMapZoom, neededZoom, 0.5f).setOnUpdate(OnMapZoomUpdate).setOnComplete(
				() => { GameController.Instance.BlockInput(false); });

			ScalePath(neededZoom);
		}

		private void ScalePath(float neededZoom) {
			for (var i = 0; i < _pathList.Count; ++i) {
				_pathList[i].SetLineSize(neededZoom);
			}
		}

		private void ScaleAround(GameObject target, Vector3 pivot, Vector3 newScale) {
			var a  = target.transform.localPosition;
			var b  = pivot;
			var c  = a - b; // diff from object pivot to desired pivot/origin
			var rs = newScale.x / target.transform.localScale.x; // relative scale factor
			var fp = b + c * rs; // calc final position post-scale

			// finally, actually perform the scale/translation
			target.transform.localScale    = newScale;
			target.transform.localPosition = fp;
		}

		private void OnMapZoomUpdate(float val) {
			var newScale = new Vector2(val, val);
			ScaleAround(gameObject, _mapLayer.transform.position, newScale);

			var mapPos = transform.position;
			mapPos             = ClampPos(mapPos);
			transform.position = mapPos;
		}

		private void SetMapPos() {
			transform.position = Vector3.zero;

			var neededObjTransform = _mainBase.transform;
			var unlockedTowers     = GameController.Instance.Player.UnlockedTowers;
			var lastPlayedTowerId  = GameController.Instance.PlayedTowerInfo.Id;
			if (lastPlayedTowerId != -1) {
				neededObjTransform = _towersList.Find(x => x.ID == lastPlayedTowerId).transform;
			}
			else if (unlockedTowers.Count > 0) {
				var maxTowerId = unlockedTowers.Max(x => x._towerId);
				neededObjTransform = _towersList.Find(x => x.ID == maxTowerId).transform;
			}

			var neededMapPos = Vector3.zero;
			neededMapPos.x -= neededObjTransform.transform.position.x;
			neededMapPos.y -= neededObjTransform.transform.position.y;

			transform.position = ClampPos(neededMapPos);
		}

		private void InitMapLayer() {
			var mainCanvas = GameController.Instance.Board.MainCanvasRT;
			_mapLayer.sizeDelta = mainCanvas.sizeDelta * mainCanvas.transform.localScale.x;
		}

		private void InitTowers() {
			var towersArr = GameObject.FindGameObjectsWithTag("MapTower");
			for (var i = 0; i < towersArr.Length; ++i) {
				var towerObj = towersArr[i].GetComponent<TowerObject>();
				towerObj.Initialize(this);

				_towersList.Add(towerObj);
			}
		}

		private void InitMines() {
			var nowSeconds        = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
			var deltaFromLastSave = (int) (nowSeconds - _player.Flags.MineSaveTime);
			if (_player.Flags.MineSaveTime <= 0)
				deltaFromLastSave = 0;

			var minesArr = GameObject.FindGameObjectsWithTag("Mine");
			for (var i = 0; i < minesArr.Length; ++i) {
				var mineObj = minesArr[i].GetComponent<MineObject>();
				mineObj.Initialize(this, deltaFromLastSave);

				_mineList.Add(mineObj);
			}
		}

		private void InitGrandChest() {
			var nowSeconds        = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
			var deltaFromLastSave = (int) (nowSeconds - _player.Flags.GrandChestSaveTime);
			if (_player.Flags.GrandChestSaveTime <= 0)
				deltaFromLastSave = 0;

			var minesArr = GameObject.FindGameObjectsWithTag("GrandChest");
			for (var i = 0; i < minesArr.Length; ++i) {
				var grangChestObj = minesArr[i].GetComponent<MapGrandChestObject>();
				grangChestObj.InitUnlockedState(deltaFromLastSave);
				_grandChestsList.Add(grangChestObj);
			}
		}

		private void Update() {
			UpdateFocusController();
			UpdateMines();
			UpdateGrandChests();
			UpdateInput();
		}

		private void UpdateGrandChests() {
			for (var i = 0; i < _grandChestsList.Count; ++i)
				_grandChestsList[i].UpdateProduce();
		}

		private void UpdateInput() {
			if (!GameController.Instance.IsCanClick())
				return;

			UpdateMouseDown();
			UpdateMouseUp();
			UpdateMouseDownState();
			UpdateMapDragState();
		}

		private void UpdateFocusController() {
			_focusController.UpdateQueue();
		}

		private void UpdateMines() {
			for (var i = 0; i < _mineList.Count; ++i)
				_mineList[i].UpdateProduce();
		}

		private void UpdateMouseDown() {
			if (Input.GetMouseButtonDown(0)) {
				_inMouseDownState = true;
				_mouseDownPos     = GameController.Instance.Board.MainCamera.ScreenToWorldPoint(Input.mousePosition);
				_clickOffset      = transform.position - _mouseDownPos;
			}
		}

		private bool IsMouseOnHud() {
			var mousePos = GameController.Instance.Board.MainCamera.ScreenToWorldPoint(Input.mousePosition);
			var hudPos   = _hudBottomObject.transform.position;
			var hudSize  = _hudBottomObject.GetComponent<RectTransform>().sizeDelta * _hudBottomObject.transform.lossyScale;
			hudSize.x = Screen.width;

			if (mousePos.x > hudPos.x - hudSize.x / 2f && mousePos.x < hudPos.x + hudSize.x / 2f)
				if (mousePos.y > hudPos.y && mousePos.y < hudPos.y + hudSize.y)
					return true;

			return false;
		}

		private void UpdateMouseUp() {
			if (Input.GetMouseButtonUp(0)) {
				_inMouseDownState = false;
				_inMapDragState   = false;
				_clickOffset      = Vector2.zero;
				_mouseDownPos     = Vector3.zero;
			}
		}

		private void UpdateMouseDownState() {
			if (!_inMouseDownState)
				return;

			var mousePos       = GameController.Instance.Board.MainCamera.ScreenToWorldPoint(Input.mousePosition);
			var mouseDragDelta = (mousePos - _mouseDownPos).magnitude;
			if (mouseDragDelta >= 50f)
				_inMapDragState = true;
		}

		private void UpdateMapDragState() {
			if (!_inMapDragState)
				return;

			var currPos = GameController.Instance.Board.MainCamera.ScreenToWorldPoint(Input.mousePosition);
			currPos.x          += _clickOffset.x;
			currPos.y          += _clickOffset.y;
			currPos.z          =  0;
			transform.position =  ClampPos(currPos);
		}

		public Vector3 ClampPos(Vector3 mapPos) {
			var clampedPos = mapPos;

			var currentScale = transform.localScale.x;
			var leftPos      = mapPos.x - _mapSize.x * currentScale / 2f;
			var rightPos     = mapPos.x + _mapSize.x * currentScale / 2f;
			var topPos       = mapPos.y + _mapSize.y * currentScale / 2f;
			var bottomPos    = mapPos.y - _mapSize.y * currentScale / 2f;

			if (leftPos > -_mapLayer.sizeDelta.x / 2f) {
				clampedPos.x -= leftPos + _mapLayer.sizeDelta.x / 2f;
			}

			if (rightPos < _mapLayer.sizeDelta.x / 2f) {
				clampedPos.x -= rightPos - _mapLayer.sizeDelta.x / 2f;
			}

			if (topPos < _mapLayer.sizeDelta.y / 2f) {
				clampedPos.y -= topPos - _mapLayer.sizeDelta.y / 2f;
			}

			if (bottomPos > -_mapLayer.sizeDelta.y / 2f + _mapBottomShift) {
				clampedPos.y -= bottomPos + _mapLayer.sizeDelta.y / 2f - _mapBottomShift;
			}

			return clampedPos;
		}

		public void ShowHand() {
			_handObject = Instantiate(_handPrefab, _overMapLayer);

			var target  = GetTowerForHand();
			var handPos = target.transform.position;
			handPos.x -= 20;
			handPos.y += 140;

			_handObject.transform.position = handPos;
		}

		public void HideHand() {
			Destroy(_handObject);
		}

		public void TryOpenNeededTowerWindow() {
			if (NeedOpenTowerId == -1)
				return;

			var tower = GetTowerById(NeedOpenTowerId);
			tower.GetComponent<TowerObject>().StartTowerWindow(true);

			NeedOpenTowerId = -1;
		}
	}
}
