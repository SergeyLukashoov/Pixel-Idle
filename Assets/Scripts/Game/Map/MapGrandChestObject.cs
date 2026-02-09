using System;
using System.Collections.Generic;
using Core.Config.Chests;
using Core.Config.Tutorial;
using Core.Controllers;
using Core.Controllers.Windows;
using Game.Cards;
using Game.Player;
using Spine.Unity;
using Ui;
using UnityEngine;
using UnityEngine.EventSystems;
using Window;

namespace Game.Map {
	public class MapGrandChestObject : MonoBehaviour {
		[SerializeField] private string            _id;
		[SerializeField] private SkeletonAnimation _animation;
		[SerializeField] private List<TowerObject> _linkedTowers = new List<TowerObject>();
		[SerializeField] private GameObject        _progressRoot;
		[SerializeField] private List<GameObject>  _progressFill = new List<GameObject>();
		[SerializeField] private MineProgress      _progressBar;
		[SerializeField] private MineCountLabel    _resCountObject;
		[SerializeField] private Animator          _bubbleAnim;
		[SerializeField] private Sprite            _cardSp;
		[SerializeField] private GameObject        _collectResObject;

		private WindowsController _windowController;
		private ChestData         _chestData;
		private IPlayer           _player;
		private GrandChestInfo    _grandChestInfo;

		private bool  _isActive           = false;
		private bool  _isChestUnlocked    = false;
		private float _currentProduceTime = 0f;
		private float _produceTime        = 0f;
		private int   _resourcesLeft      = 0;
		private float _timeToCallToAction = 4f;

		public string    ID                 => _id;
		public ChestData ChestData          => _chestData;
		public bool      IsActive           => _isActive;
		public float     CurrentProduceTime => _currentProduceTime;
		public float     ProduceTime        => _produceTime;
		public int       ResourcesLeft      => _resourcesLeft;

		private void OnDestroy() {
			_resCountObject.OnClickAction -= CollectRes;
		}

		private void Awake() {
			_windowController = GameController.Instance.WindowsController;
			_chestData        = GameController.Instance.DB.ChestsDB.GetChestData(_id);
			_player           = GameController.Instance.Player;
			_grandChestInfo   = GameController.Instance.Player.GetGrandChestInfo(_id);

			_progressBar.Initialize(PlayerCurrencyType.GreenCrystal);

			_resCountObject.OnClickAction += CollectRes;
			_resCountObject.gameObject.SetActive(false);
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

			AddCards(GameController.Instance.Config.GrandChestIncomeCount);
			CheckNeedShowCards();
			PlayCollectResAnim();

			_currentProduceTime = 0f;
			_resourcesLeft--;

			if (_resourcesLeft <= 0) {
				gameObject.SetActive(false);
				return;
			}

			var timeStep = GameController.Instance.Config.GrandChestMaxResources - _resourcesLeft;
			_produceTime = GameController.Instance.Config.GrandChestStartProduceTime +
			               GameController.Instance.Config.GrandChestProduceTimeStep * timeStep;

			_resCountObject.gameObject.SetActive(false);
			_progressBar.gameObject.SetActive(true);

			SetProgress();
		}

		private void PlayCollectResAnim() {
			var resObj = Instantiate(_collectResObject, transform);
			resObj.transform.localPosition = new Vector3(0, 200, 0);
			resObj.GetComponent<CollectResObject>().Initialize(_cardSp, GameController.Instance.Config.GrandChestIncomeCount);

			LeanTween.moveLocalY(resObj, 400, 0.6f);
			LeanTween.scale(resObj, Vector3.zero, 0.2f).setDelay(0.5f).setOnComplete(
				() => { Destroy(resObj); });
		}

		private void CheckNeedShowCards() {
			var showCardType = GameController.Instance.Player.GetCardForShow();
			if (showCardType != CardType.Empty && GameController.Instance.TutorialController.IsTutorialComplete(TutorialId.NewCard)) {
				var cardInfo = GameController.Instance.Player.GetCard(showCardType);
				var settings = new UpgradeCardWS {
					Info   = cardInfo,
					OnHide = CheckNeedShowCards
				};

				GameController.Instance.WindowsController.Show(WindowType.UpgradeCardWindow, settings, true, 0.3f);
			}
			else {
				GameController.Instance.NeedShowRateUsWindow = true;
			}
		}

		public void InitUnlockedState(int deltaFromLastSave) {
			if (_grandChestInfo == null) {
				_resCountObject.gameObject.SetActive(false);
				return;
			}

			_currentProduceTime =  _grandChestInfo._currentProduceTime;
			_produceTime        =  _grandChestInfo._produceTime;
			_resourcesLeft      =  _grandChestInfo._resourceLeft;
			_currentProduceTime += deltaFromLastSave;

			if (_currentProduceTime >= _produceTime) {
				_currentProduceTime = _produceTime;
				_animation.AnimationState.SetAnimation(0, "stay_active", true);
				_resCountObject.gameObject.SetActive(true);
				_progressBar.gameObject.SetActive(false);
			}
			else {
				_resCountObject.gameObject.SetActive(false);
			}
		}

		public void UpdateView() {
			var animName = "stay_unactive";

			_isChestUnlocked = _player.IsGrandChestUnlocked(_id);
			if (_isChestUnlocked) {
				animName = "action";
			}
			else {
				for (var i = 0; i < _linkedTowers.Count; ++i) {
					if (_linkedTowers[i].IsObjectInRange(transform.localPosition)) {
						animName = "stay_active";
						break;
					}
				}
			}

			_progressBar.gameObject.SetActive(_isChestUnlocked);
			_animation.AnimationState.SetAnimation(0, animName, true);

			UpdateProgressView(_isActive);
		}

		public void SetActive(bool isActive) {
			if (_isActive)
				return;

			_isActive = isActive;
			gameObject.SetActive(isActive);

			if (_grandChestInfo != null && _grandChestInfo._resourceLeft <= 0) {
				gameObject.SetActive(false);
			}
		}

		public void SetActiveAnim() {
			_animation.AnimationState.SetAnimation(0, "stay_active", true);
			UpdateProgressViewCount();
		}

		public void UpdateProgressView(bool needShow) {
			_progressRoot.SetActive(needShow);
			UpdateProgressViewCount();
		}

		private void UpdateProgressViewCount() {
			var progressCount = 0;
			for (var i = 0; i < _linkedTowers.Count; ++i) {
				var isActive = _linkedTowers[i].IsObjectInRange(transform.localPosition);
				if (isActive)
					progressCount++;

				_progressFill[i].SetActive(isActive);
			}

			if (progressCount == _linkedTowers.Count) {
				_progressRoot.SetActive(false);

				if (!_isChestUnlocked)
					_animation.AnimationState.SetAnimation(0, "call", true);
			}
		}

		private void OnMouseDown() {
			if (!GameController.Instance.IsCanClick())
				return;

			ClickOnChest();
		}

		private void ClickOnChest() {
			if (IsAvailable()) {
				if (_isChestUnlocked) {
					OpenActiveChestWindow();
				}
				else {
					OpenChestWindow();
				}
			}
			else {
				OpenChestInfoWindow();
			}
		}

		private bool IsAvailable() {
			var isAvailable = true;
			for (var i = 0; i < _linkedTowers.Count; ++i) {
				if (!_linkedTowers[i].IsObjectInRange(transform.localPosition)) {
					isAvailable = false;
					break;
				}
			}

			return isAvailable;
		}

		public void SetCollected() {
			CollectReward();

			_grandChestInfo = GameController.Instance.Player.GetGrandChestInfo(_id);
			UpdateView();
			InitUnlockedState(0);
		}

		private void CollectReward() {
			_player.UnlockGrandChest(_id);

			for (var i = 0; i < _chestData.Rewards.Count; ++i) {
				var rewardInfo = _chestData.Rewards[i];
				if (rewardInfo.Type == PlayerCurrencyType.Coins)
					_player.ChangeCoinsCount(rewardInfo.Count, "map_grand_chest");
				else if (rewardInfo.Type is
				         PlayerCurrencyType.CardCommon or
				         PlayerCurrencyType.CardRare or
				         PlayerCurrencyType.CardEpic)
					AddCards(rewardInfo.Count);
				else
					_player.ChangeCurrency(rewardInfo.Type, rewardInfo.Count, "map_grand_chest");
			}
		}

		private void AddCards(int count) {
			var cardController = GameController.Instance.CardsController;
			for (var i = 0; i < count; ++i) {
				var cardType = cardController.DropCard(_chestData.CardsList);
				_player.AddCard(cardType);
			}
		}

		private void OpenChestWindow() {
			var settings = new MapGrandChestWS {
				ID     = _id,
				Reward = _chestData.Rewards,
				Chest  = this,
				OnHide = CheckNeedShowCards,
			};

			_windowController.Show(WindowType.MapGrandChestWindow, settings);
		}

		private void OpenActiveChestWindow() {
			var settings = new GrandChestActiveWS {
				CurrentTime   = _currentProduceTime,
				ProduceTime   = _produceTime,
				ResourcesLeft = _resourcesLeft,
			};

			_windowController.Show(WindowType.GrandChestActiveWindow, settings);
		}

		public void UpdateProduce() {
			if (!gameObject.activeSelf)
				return;

			if (!_isChestUnlocked)
				return;

			if (_currentProduceTime >= _produceTime)
				return;

			_currentProduceTime += Time.deltaTime;
			if (_currentProduceTime >= _produceTime) {
				_currentProduceTime = _produceTime;
				_animation.AnimationState.SetAnimation(0, "stay_active", true);
				_resCountObject.gameObject.SetActive(true);
				_progressBar.gameObject.SetActive(false);
			}

			SetProgress();
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

		private void SetProgress() {
			var progress = _currentProduceTime / _produceTime;
			_progressBar.SetProgress(progress);
		}

		private void OpenChestInfoWindow() {
			var settings = new InfoHeaderWS {
				HeaderStr    = GameController.Instance.GetGameText("chest_info_window_header"),
				InfoStr      = GameController.Instance.GetGameText("chest_info_window_info"),
				NeedHeader   = true,
				NeedContent  = true,
				ChestContent = _chestData.Rewards,
			};

			_windowController.Show(WindowType.InfoHeaderWindow, settings);
		}
	}
}
