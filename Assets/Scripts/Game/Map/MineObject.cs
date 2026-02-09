using System.Collections.Generic;
using Core.Config.Mine;
using Core.Config.Tutorial;
using Core.Controllers;
using Core.Controllers.Tutorial;
using Core.Controllers.Windows;
using Game.Player;
using Spine.Unity;
using Ui;
using UnityEngine;
using Window;
using UnityEngine.EventSystems;

namespace Game.Map {
	internal static class MineState {
		public static string Closed = "closed";
		public static string Opened = "opened";
		public static string Work   = "work";
	}

	internal static class SkinType {
		public static string RED   = "red";
		public static string GREEN = "green";
	}

	public class MineObject : MonoBehaviour {
		[SerializeField] private int                _id;
		[SerializeField] private int                _startLevelUp = 1;
		[SerializeField] private int                _terrainId    = 1;
		[SerializeField] private PlayerCurrencyType _producedResType;
		[SerializeField] private SkeletonAnimation  _animation;
		[SerializeField] private GameObject         _overLayer;
		[SerializeField] private MineProgress       _mineProgress;
		[SerializeField] private SpriteRenderer     _crystalIcon;
		[SerializeField] private Sprite             _redCrystalSp;
		[SerializeField] private Sprite             _greenCrystalSp;
		[SerializeField] private MineCountLabel     _resCountObject;
		[SerializeField] private GameObject         _collectResObject;
		[SerializeField] private Animator           _bubbleAnim;

		private List<TowerObject> _towerObjects = new List<TowerObject>();

		private TutorialController _tutorialController;
		private MineProduceInfo    _mineProduceInfo;
		private MineInfo           _mineInfo;
		private MapObject          _mapObject;
		
		private string _state                = MineState.Closed;
		private bool   _unlocked             = false;
		private bool   _available            = false;
		private int    _currentProducedCount = 0;
		private float  _currentProduceTime   = 0f;
		private float  _timeToCallToAction   = 4f;

		public int   ID                   => _id;
		public int   StartLevelUp         => _startLevelUp;
		public int   CurrentProducedCount => _currentProducedCount;
		public float CurrentProduceTime   => _currentProduceTime;

		private void OnDestroy() {
			_resCountObject.OnClickAction -= CollectRes;
		}

		public FocusInfo GetFocusInfo() {
			var info = new FocusInfo {
				Object          = transform,
				OnExecuteAction = OpenMineWindowAfterUnlock,
			};

			return info;
		}

		private void CollectRes() {
			if (GameController.Instance.IsBlockInput)
				return;

			if (!gameObject.activeSelf)
				return;

			if (EventSystem.current.IsPointerOverGameObject())
				return;

			if (GameController.Instance.WindowsController.OpenedWindowCount > 0)
					return;

			if (_currentProducedCount <= 0)
				return;

			SendCollect();
			
			GameController.Instance.Player.ChangeCurrency(_producedResType, _currentProducedCount, "mine");
			PlayCollectResAnim();

			_currentProducedCount = 0;

			UpdateView();
		}

		private void PlayCollectResAnim() {
			var resObj = Instantiate(_collectResObject, transform);
			resObj.transform.localPosition = new Vector3(0, 200, 0);

			var resIconSp = _redCrystalSp;
			if (_producedResType == PlayerCurrencyType.GreenCrystal)
				resIconSp = _greenCrystalSp;

			resObj.GetComponent<CollectResObject>().Initialize(resIconSp, _currentProducedCount);

			LeanTween.moveLocalY(resObj, 400, 0.6f);
			LeanTween.scale(resObj, Vector3.zero, 0.2f).setDelay(0.5f).setOnComplete(
				() => { Destroy(resObj); });
		}

		public void Initialize(MapObject mapObject, int deltaFromLastSave) {
			_mapObject          = mapObject;
			_tutorialController = GameController.Instance.TutorialController;
			_available          = IsMineAvailable();
			_mineProgress.Initialize(_producedResType);
			_resCountObject.OnClickAction += CollectRes;

			_mineInfo = GameController.Instance.Player.GetMineInfo(_id);
			var currentLevelUp = _startLevelUp;
			if (_mineInfo != null)
				currentLevelUp = _mineInfo._levelUp;

			_mineProduceInfo = GameController.Instance.DB.MineDB.GetMineProduceInfo(currentLevelUp);

			if (_mineInfo != null) {
				_unlocked = true;
				InitUnlockedMine(deltaFromLastSave);
			}
			else {
				_unlocked = false;
				_state    = _available ? MineState.Opened : MineState.Closed;
			}

			SetSkin();
			UpdateView();
		}

		private void SetSkin() {
			var skinName = SkinType.RED;
			if (_producedResType == PlayerCurrencyType.GreenCrystal)
				skinName = SkinType.GREEN;

			_animation.skeleton.SetSkin(skinName);

			var attachmentName = "mine/desert_BODY";
			if (_terrainId == 2)
				attachmentName = "mine/green_BODY";
			else if (_terrainId == 3)
				attachmentName = "mine/lava_BODY";

			_animation.skeleton.SetAttachment("BODY", attachmentName);
		}

		private void UpdateView() {
			_overLayer.SetActive(_unlocked);
			_resCountObject.gameObject.SetActive(_currentProducedCount > 0);
			_animation.AnimationState.SetAnimation(0, _state, true);

			var neededResSp = _redCrystalSp;
			if (_producedResType == PlayerCurrencyType.GreenCrystal) {
				neededResSp = _greenCrystalSp;
			}

			_crystalIcon.sprite = neededResSp;
			_mineProgress.gameObject.SetActive(_currentProducedCount < _mineProduceInfo.MaxCapacity);
		}

		private void InitUnlockedMine(int deltaFromLastSave) {
			_currentProduceTime   = _mineInfo._produceTime;
			_currentProducedCount = _mineInfo._producedCount;

			if (_mineInfo._producedCount < _mineProduceInfo.MaxCapacity) {
				_currentProduceTime += deltaFromLastSave;
				var producedCount = (int) (_currentProduceTime / _mineProduceInfo.ProduceTime);

				_currentProduceTime   -= _mineProduceInfo.ProduceTime * producedCount;
				_currentProducedCount += _mineProduceInfo.ProduceCount * producedCount;
				if (_currentProducedCount > _mineProduceInfo.MaxCapacity)
					_currentProducedCount = _mineProduceInfo.MaxCapacity;
			}

			if (_currentProducedCount < _mineProduceInfo.MaxCapacity)
				_state = MineState.Work;
			else
				_state = MineState.Opened;
		}

		private void Update() {
			UpdateCallToAction();
		}

		private void UpdateCallToAction() {
			if (!_resCountObject.gameObject.activeSelf)
				return;

			_timeToCallToAction -= Time.deltaTime;
			if (_timeToCallToAction <= 0f) {
				_timeToCallToAction = 4f;
				_bubbleAnim.SetTrigger("CallToAction");
			}
		}

		public void OnMouseDown() {
			if (!GameController.Instance.IsCanClick())
				return;

			OpenMineWindow(false);
		}

		private void OpenMineWindowAfterUnlock() {
#if REC_VIDEO
			return;
#endif

			if (!_tutorialController.IsTutorialComplete(TutorialId.FirstMine) &&
			    _tutorialController.IsTutorialComplete(TutorialId.FirstInfoRange)) {
				_tutorialController.StartTutorial(TutorialId.FirstMine, FindTargetForTutor);
			}
			else {
				OpenMineWindow(true);
			}
		}

		private GameObject FindTargetForTutor() {
			var currentStepTargetId = _tutorialController.CurrentStepTargetId;
			if (currentStepTargetId == "MineInRange")
				return gameObject;

			return null;
		}

		public void OpenMineWindow(bool needDoneConditions) {
			var settings = new MineWS {
				IsUnlocked         = _unlocked,
				IsAvailable        = _available,
				IncomeCount        = _mineProduceInfo.ProduceCount,
				CurrentCapacity    = _currentProducedCount,
				MaxCapacity        = _mineProduceInfo.MaxCapacity,
				CrystalType        = _producedResType,
				LevelUp            = _mineInfo?._levelUp ?? 1,
				CurrentTime        = _currentProduceTime,
				ProduceTime        = _mineProduceInfo.ProduceTime,
				NeedDoneConditions = needDoneConditions,
			};

			GameController.Instance.WindowsController.Show(WindowType.MineWindow, settings);
		}

		public void UpdateProduce() {
			if (_state != MineState.Work)
				return;

			_currentProduceTime += Time.deltaTime;
			if (_currentProduceTime >= _mineProduceInfo.ProduceTime) {
				_currentProduceTime   =  0f;
				_currentProducedCount += _mineProduceInfo.ProduceCount;
				if (_currentProducedCount > _mineProduceInfo.MaxCapacity) {
					_currentProducedCount = _mineProduceInfo.MaxCapacity;
					_state                = MineState.Opened;
				}

				UpdateView();
			}

			SetMineProgress();
		}

		private void SetMineProgress() {
			var progress = _currentProduceTime / _mineProduceInfo.ProduceTime;
			_mineProgress.SetProgress(progress);
		}

		public void AddTower(TowerObject towerObj) {
			_towerObjects.Add(towerObj);
		}

		private bool IsMineAvailable() {
			var isAvailable = false;
			for (var i = 0; i < _towerObjects.Count; ++i) {
				if (_towerObjects[i].Unlocked) {
					isAvailable = true;
					break;
				}
			}

			return isAvailable;
		}

		private void SendCollect() {
			
		}
	}
}
