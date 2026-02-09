using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config.Chests;
using Game.Player;
using UnityEngine;

namespace Core.Config {
	[Serializable]
	public class UpgradeStatsCrystalInfo {
		public PlayerCurrencyType Type;
		public int                StartLevelUp;
	}

	[Serializable, CreateAssetMenu(fileName = "MainConfig", menuName = "Config/Create Main Config", order = 52)]
	public class MainConfig : ScriptableObject {
		[SerializeField] private int _gameWidth;
		[SerializeField] private int _gameHeight;
		[SerializeField] private int _pixelsInMeter;
		[SerializeField] private int _baseSpeed;
		[SerializeField] private int _bulletSpeed;
		[SerializeField] private int _startCoins;
		[SerializeField] private int _startCoreExp;
		[SerializeField] private int _spawnPointsCooldown;
		[SerializeField] private int _unlockWorkshopReward;
		[SerializeField] private int _oneCrystalCoinsPrice;
		[SerializeField] private int _explodeRange;

		[Header("Towers")]
		[SerializeField] private int _healthModify;
		[SerializeField] private int _towerRangeStep;
		[SerializeField] private int _minTowerRange;
		[SerializeField] private int _maxTowerRange;
		[SerializeField] private int _towerViewRange;
		[SerializeField] private int _towerLockedViewRange;
		[SerializeField] private int _viewRangeDeltaForFog;

		[Header("Grand Chest")]
		[SerializeField] private int _grandChestMaxResources;
		[SerializeField] private int   _grandChestIncomeCount;
		[SerializeField] private float _grandChestStartProduceTime;
		[SerializeField] private float _grandChestProduceTimeStep;

		[Header("Cards")]
		[SerializeField] private int _activeSlotsCount;

		[Header("Resources")]
		[SerializeField] private DataContainer _gameData;

		[Header("UpgradePrice")]
		[SerializeField] private List<UpgradeStatsCrystalInfo> _upgradeStatsCrystalInfos;

		[Header("Ads")]
		[SerializeField] private float _interstitialAdCooldown;
		[SerializeField] private int              _interstitialAdMapCount;
		[SerializeField] private float            _adsAccelerateReward;
		[SerializeField] private float            _adsAccelerateTime;
		[SerializeField] private float            _adsEndRunBonus;
		[SerializeField] private List<RewardInfo> _noAdsReward;

		[Header("Cheater")]
		[SerializeField] private int _startWaveId = 0;

		public int              GameWidth                  => _gameWidth;
		public int              GameHeight                 => _gameHeight;
		public int              PixelsInMeter              => _pixelsInMeter;
		public int              BaseSpeed                  => _baseSpeed;
		public int              BulletSpeed                => _bulletSpeed;
		public int              StartCoins                 => _startCoins;
		public DataContainer    GameData                   => _gameData;
		public int              StartWaveId                => _startWaveId;
		public int              SpawnPointsCooldown        => _spawnPointsCooldown;
		public int              ActiveSlotsCount           => _activeSlotsCount;
		public int              StartCoreExp               => _startCoreExp;
		public int              UnlockWorkshopReward       => _unlockWorkshopReward;
		public int              HealthModify               => _healthModify;
		public int              MinTowerRange              => _minTowerRange;
		public int              MaxTowerRange              => _maxTowerRange;
		public int              TowerViewRange             => _towerViewRange;
		public int              TowerLockedViewRange       => _towerLockedViewRange;
		public int              TowerRangeStep             => _towerRangeStep;
		public int              ViewRangeDeltaForFog       => _viewRangeDeltaForFog;
		public int              OneCrystalCoinsPrice       => _oneCrystalCoinsPrice;
		public float            InterstitialAdCooldown     => _interstitialAdCooldown;
		public int              InterstitialAdMapCount     => _interstitialAdMapCount;
		public float            AdsAccelerateReward        => _adsAccelerateReward;
		public float            AdsAccelerateTime          => _adsAccelerateTime;
		public float            AdsEndRunBonus             => _adsEndRunBonus;
		public float            ExplodeRange               => _explodeRange;
		public List<RewardInfo> NoAdsReward                => _noAdsReward;
		public int              GrandChestMaxResources     => _grandChestMaxResources;
		public int              GrandChestIncomeCount      => _grandChestIncomeCount;
		public float            GrandChestStartProduceTime => _grandChestStartProduceTime;
		public float            GrandChestProduceTimeStep => _grandChestProduceTimeStep;


		public int GetCrystalPrice(PlayerCurrencyType crystalType, int levelUp) {
			var info = _upgradeStatsCrystalInfos.FirstOrDefault(x => x.Type == crystalType);
			if (info != null) {
				if (levelUp >= info.StartLevelUp) {
					var crystalLvlUp = info.StartLevelUp - levelUp;
					var count        = crystalLvlUp * ((crystalLvlUp + 1) / 4f);
					var roundedCount = Mathf.Ceil(count);

					return (int) roundedCount;
				}
			}

			return 0;
		}

		public int GetCrystalPriceForNewStats(int levelUp) {
			return 500 + levelUp * (levelUp + 100);
		}
	}
}
