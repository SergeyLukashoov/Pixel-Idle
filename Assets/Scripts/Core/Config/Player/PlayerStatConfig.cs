using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Game.Player;
using UnityEngine;

namespace Config.Player {
	public class PlayerStatConfig {
		private PlayerStatType _type;
		private float          _value;
		private float          _maxValue = -1;
		private float          _upgradeStep;
#if HARDCORE
		private List<UpgradePriceInfo> _upgradePriceInfos;
#else
		private List<int> _upgradePriceInfos;
#endif
		private bool _lockedOnStart;
		private int  _showAfterTower;
		private int  _unlockPrice;

		public PlayerStatType Type           => _type;
		public float          Value          => _value;
		public float          MaxValue       => _maxValue;
		public float          UpgradeStep    => _upgradeStep;
		public bool           LockedOnStart  => _lockedOnStart;
		public int            ShowAfterTower => _showAfterTower;
		public int            UnlockPrice    => _unlockPrice;

		public PlayerStatConfig(PlayerStatsData data) {
			if (!Enum.TryParse(data.SkillId, out _type)) {
				Debug.LogError($"Wrong skill ID: {data.SkillId}");
				return;
			}

			_value             = data.BaseValue;
			_maxValue          = data.MaxValue;
			_upgradeStep       = data.ValueStep;
			_upgradePriceInfos = data.UpgradeInfos;
			_lockedOnStart     = data.LockedOnStart;
			_showAfterTower    = data.ShowAfterTower;
			_unlockPrice       = data.UnlockPrice;
		}

#if HARDCORE
		public int GetUpgradePrice(int currentLvlUp) {
			var nextLvlUp = currentLvlUp + 1;
			var price = 0f;
			for (var i = 1; i <= currentLvlUp + 1; ++i) {
				var priceInfo = _upgradePriceInfos.FirstOrDefault(x => i <= x._level);
				if (priceInfo == null) {
					Debug.LogError($"LevelUp to big! {nextLvlUp}");
					break;
				}

				price += priceInfo._price;
			}

			return (int) price;
		}
#else
		public int GetUpgradePrice(int currentLvlUp) {
			var nextLvlUp = currentLvlUp;
			if (nextLvlUp > _upgradePriceInfos.Count - 1) {
				var lastPrice     = _upgradePriceInfos[_upgradePriceInfos.Count - 1];
				var prevLastPrice = _upgradePriceInfos[_upgradePriceInfos.Count - 2];
				var step          = lastPrice - prevLastPrice;
				var stepIdx       = nextLvlUp - (_upgradePriceInfos.Count - 1);

				var price = lastPrice;
				for (var i = 0; i < stepIdx; ++i) {
					step++;
					price += step;
				}

				return price;
			}

			return _upgradePriceInfos[nextLvlUp];
		}
#endif
	}
}
