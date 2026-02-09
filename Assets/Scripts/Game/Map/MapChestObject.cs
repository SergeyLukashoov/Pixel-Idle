using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config.Chests;
using Core.Config.Tutorial;
using Core.Controllers;
using Core.Controllers.Windows;
using Game.Cards;
using Game.Player;
using Spine.Unity;
using UnityEngine;
using Window;
using Random = UnityEngine.Random;

namespace Game.Map {
	public enum MapChestType {
		Default,
		RewardedAds
	}

	public class MapChestObject : MonoBehaviour {
		[SerializeField] private MapChestType      _chestType;
		[SerializeField] private string            _id;
		[SerializeField] private SkeletonAnimation _animation;
		[SerializeField] private GameObject        _openFx;
		[SerializeField] private GameObject        _callToActionAnim;
		[SerializeField] private GameObject        _effectAnim;
		[SerializeField] private Color             _defaultColor;
		[SerializeField] private Color             _rewardedAdsColor;

		private IPlayer           _player;
		private WindowsController _windowController;
		private TowerObject       _tower;
		private ChestData         _chestData;

		public string       ID         => _id;
		public MapChestType Type       => _chestType;
		public ChestData    ChestData => _chestData;
		
		private void Awake() {
			_player           = GameController.Instance.Player;
			_windowController = GameController.Instance.WindowsController;

			_callToActionAnim.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
		}

		public void Initialize(TowerObject tower) {
			_tower     = tower;
			_chestData = GameController.Instance.DB.ChestsDB.GetChestData(_id);

			UpdateView();
		}

		private void UpdateView() {
			var skinName         = "unactive";
			var animName         = "closed";
			var needCallToAction = false;
			if (_tower.IsObjectInRange(transform.localPosition)) {
				if (_chestType == MapChestType.Default)
					skinName = "active";
				else
					skinName = "violet";

				animName         = "active";
				needCallToAction = true;
			}
			
			_callToActionAnim.SetActive(needCallToAction);
			_effectAnim.SetActive(needCallToAction);
			_animation.skeleton.SetSkin(skinName);
			_animation.AnimationState.SetAnimation(0, animName, true);

			StartPS(needCallToAction);
			SetColors();
		}

		private void StartPS(bool needCallToAction) {
			if (!needCallToAction)
				return;

			var ps = _callToActionAnim.GetComponent<ParticleSystem>();
			ps.Stop();

			var randDelay = Random.Range(0f, 0.4f);
			LeanTween.delayedCall(randDelay, () => { ps.Play(); });
		}
		
		private void SetColors() {
			var color = _chestType == MapChestType.Default ? _defaultColor : _rewardedAdsColor;

			var mainCallToActionPS = _callToActionAnim.GetComponent<ParticleSystem>().main;
			mainCallToActionPS.startColor = color;

			var effectAnimPS = _effectAnim.GetComponent<ParticleSystem>().main;
			effectAnimPS.startColor = color;
		}

		public void SetActiveSkin() {
			UpdateView();
		}

		public void SetDisableSkin() {
			_animation.skeleton.SetSkin("unactive");
			_animation.AnimationState.SetAnimation(0, "closed", true);
		}

		private void ClickOnChest() {
			if (_tower.IsObjectInRange(transform.localPosition)) {
				OpenChestWindow();
			}
			else {
				OpenChestInfoWindow();
			}
		}

		public void CollectChest(int adsMult = 1) {
			CollectReward(adsMult);
			SendCollectChest();
		}

		public void CollectAdChest() {
			CollectReward();
			SendCollectChest();
		}

		private void PlayOpenFx() {
			gameObject.SetActive(false);
			Destroy(gameObject, 4f);
			
			var fxObj = Instantiate(_openFx, transform.parent);
			fxObj.transform.position = transform.position;
			Destroy(fxObj, 4f);
		}

		private void CollectReward(int adsMult = 1) {
			_player.CollectChest(ID);

			for (var i = 0; i < _chestData.Rewards.Count; ++i) {
				var rewardInfo = _chestData.Rewards[i];
				if (rewardInfo.Type == PlayerCurrencyType.Coins)
					_player.ChangeCoinsCount(rewardInfo.Count * adsMult, "map_chest");
				else if (rewardInfo.Type is
				         PlayerCurrencyType.CardCommon or
				         PlayerCurrencyType.CardRare or
				         PlayerCurrencyType.CardEpic)
					AddCards(rewardInfo.Count * adsMult);
				else
					_player.ChangeCurrency(rewardInfo.Type, rewardInfo.Count * adsMult, "chest");
			}
		}

		private void AddCards(int count) {
			var cardController = GameController.Instance.CardsController;
			for (var i = 0; i < count; ++i) {
				var cardType = cardController.DropCard(_chestData.CardsList);
				_player.AddCard(cardType);
			}
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

		private void OpenChestWindow() {
			var settings = new MapChestWS {
				ID     = _id,
				Reward = _chestData.Rewards,
				OnHide = OnHide,
				Chest  = this,
			};

			GameController.Instance.BlockInput(true);
			LeanTween.delayedCall(0.4f,
				() => {
					GameController.Instance.BlockInput(false);
					_windowController.Show(WindowType.MapChestWindow, settings);
				});
		}

		private void OnHide() {
			if (_player.IsChestCollected(ID)) {
				PlayOpenFx();
				CheckNeedShowCards();	
			}
			
			GameController.Instance.NeedShowRateUsWindow = true;
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
		}

		private void OnMouseDown() {
			if (!GameController.Instance.IsCanClick())
				return;

			if (GameController.Instance.TutorialController.NeedFirstChest)
				return;

			ClickOnChest();
		}

		private void SendCollectChest() {
			var dict             = new Dictionary<string, int>();
			var droppedCardCount = 0;
			for (var i = 0; i < _chestData.Rewards.Count; ++i) {
				if (_chestData.Rewards[i].Type is
				    PlayerCurrencyType.CardCommon or
				    PlayerCurrencyType.CardRare or
				    PlayerCurrencyType.CardEpic)
					droppedCardCount = _chestData.Rewards[i].Count;
			}

			for (var i = 0; i < droppedCardCount; ++i) {
				var cardType = GameController.Instance.Player.GetCardForShow(false);
				var key      = cardType.ToString();
				if (dict.ContainsKey(key))
					dict[key] += 1;
				else
					dict.Add(key, 1);
			}
			
		}

		private int GetCurrencyCount(PlayerCurrencyType type) {
			var info = _chestData.Rewards.FirstOrDefault(x => x.Type == type);
			if (info != null)
				return info.Count;

			return 0;
		}
	}
}
