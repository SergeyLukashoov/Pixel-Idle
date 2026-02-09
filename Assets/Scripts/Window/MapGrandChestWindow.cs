using System.Collections.Generic;
using Core.Config.Chests;
using Core.Controllers.Windows;
using Game.Map;
using Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class MapGrandChestWindow : BaseWindow<MapGrandChestWS> {
		public override WindowType Type => WindowType.MapGrandChestWindow;

		[SerializeField] private Button     _buttonClose;
		[SerializeField] private Button     _buttonCloseIcon;
		[SerializeField] private Button     _buttonTakeReward;
		[SerializeField] private Transform  _rewardsRoot;
		[SerializeField] private GameObject _rewardObj;
		
		private void Start() {
			InitButtons();
			InitRewards();
		}
		
		private void InitButtons() {
			_buttonClose.onClick.AddListener(OnCloseClick);
			_buttonCloseIcon.onClick.AddListener(OnCloseClick);
			_buttonTakeReward.onClick.AddListener(OnTakeRewardClick);
		}
		
		private void OnCloseClick() {
			Hide();
		}
		
		private void OnTakeRewardClick() {
			_settings.Chest.SetCollected();
			Hide();
		}
		
		private void InitRewards() {
			for (var i = 0; i < _settings.Reward.Count; ++i) {
				var reward = _settings.Reward[i];

				var rewardObj = Instantiate(_rewardObj, _rewardsRoot);
				rewardObj.GetComponent<RewardObject>().Initialize(reward.Type, reward.Count);
			}
		}
	}

	public class MapGrandChestWS : BaseWindowSettings {
		public string              ID;
		public List<RewardInfo>    Reward;
		public MapGrandChestObject Chest;
	}
}
