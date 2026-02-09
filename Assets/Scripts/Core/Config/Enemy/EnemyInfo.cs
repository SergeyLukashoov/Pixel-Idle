using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config;
using UnityEngine;

namespace Game.Enemy {
	public class EnemyInfo {
		private EnemyType  _type;
		private GameObject _prefab;

		private float _health;
		private int   _attackRange;

#if HARDCORE
		private List<UpgradePriceInfo> _upgradeHealthStepInfos;
		private List<UpgradePriceInfo> _upgradeDamageStepInfos;
#else
		private List<float> _upgradeHealthStepInfos;
		private List<float> _upgradeDamageStepInfos;
		private List<float> _upgradeSpeedStepInfos;
		private List<float> _baseCoreCurrency;
		private List<float> _baseMetaCurrency;
#endif
		private float _speed;
		private float _speedAddByWave;
		private float _speedMax;
		private float _damage;
		private float _exp;
		private float _expAddByWave;
		private float _metaCurrency;
		private float _metaCurrencyAdd;
		private float _timeToAttack;

		public EnemyInfo(NPCStatsData data) {
			if (!Enum.TryParse(data.NPCID, out _type)) {
				Debug.LogError($"Wrong NPC ID = {data.NPCID}!");
				return;
			}

			_prefab = Resources.Load<GameObject>($"Enemy/{_type}/Prefabs/{_type}");
			if (!_prefab) {
				Debug.LogError($"Can't find prefab = {_type}!");
				return;
			}
#if HARDCORE
			_health = data.BaseHealth;
			_upgradeHealthStepInfos = data.UpgradeHealthInfo;
			_damage = data.BaseDamage;
			_upgradeDamageStepInfos = data.UpgradeDamageInfo;
			_speed = data.BaseSpeed;
			_speedAddByWave = data.SpeedInc;
			_speedMax = data.SpeedMax;
			_exp = data.BaseCoreCurrency;
			_expAddByWave = data.CoreCurrencyInc;
			_metaCurrency = data.BaseMetaCurrency;
			_metaCurrencyAdd = data.MetaCurrencyInc;
#else
			_upgradeHealthStepInfos = data.UpgradeHealthInfo;
			_upgradeDamageStepInfos = data.UpgradeDamageInfo;
			_upgradeSpeedStepInfos  = data.UpgradeSpeedInfo;
			_baseCoreCurrency       = data.BaseCoreCurrency;
			_baseMetaCurrency       = data.BaseMetaCurrency;
#endif
			_timeToAttack = data.TimeToAttack;
			_attackRange  = data.AttackRange;
		}

		public EnemyType  Type           => _type;
		public GameObject Prefab         => _prefab;
		public float      Health         => _health;
		public float      Speed          => _speed;
		public float      SpeedAddByWave => _speedAddByWave;
		public float      SpeedMax       => _speedMax;
		public float      Damage         => _damage;
		public float      Exp            => _exp;
		public float      ExpAddByWave   => _expAddByWave;
		public float      Coins          => _metaCurrency;
		public float      CoinsAddByWave => _metaCurrencyAdd;
		public float      TimeToAttack   => _timeToAttack;
		public int        AttackRange    => _attackRange;

#if HARDCORE
		public float AddHealthByWave(int waveIdx) {
			var addHealth = 0f;
			for (var i = 1; i <= waveIdx; ++i) {
				var healthStep = _upgradeHealthStepInfos.FirstOrDefault(x => waveIdx <= x._level);
				if (healthStep == null)
					return addHealth;

				addHealth += healthStep._price;
			}

			return addHealth;
		}
		
		public float AddDamageByWave(int waveIdx) {
			var addDamage = 0f;
			for (var i = 1; i <= waveIdx; ++i) {
				var damageStep = _upgradeDamageStepInfos.FirstOrDefault(x => waveIdx <= x._level);
				if (damageStep == null)
					return addDamage;

				addDamage += damageStep._price;
			}

			return addDamage;
		}
#else
		public float GetEnemyHealth(int waveIdx) {
			if (waveIdx > _upgradeHealthStepInfos.Count - 1) {
				var baseVal = 20;
				if (_type == EnemyType.Durable)
					baseVal = 100;
				else if (_type == EnemyType.Boss)
					baseVal = 400;

				var health     = _upgradeHealthStepInfos[_upgradeHealthStepInfos.Count - 1];
				var stepsCount = waveIdx - (_upgradeHealthStepInfos.Count - 1);
				for (var i = 0; i < stepsCount; ++i)
					health += baseVal * (waveIdx / 10 - 2);

				return health;
			}

			return _upgradeHealthStepInfos[waveIdx];
		}

		public float GetEnemyDamage(int waveIdx) {
			if (waveIdx > _upgradeDamageStepInfos.Count - 1) {
				var damage     = _upgradeDamageStepInfos[_upgradeDamageStepInfos.Count - 1];
				var stepsCount = waveIdx - (_upgradeDamageStepInfos.Count - 1);
				for (var i = 0; i < stepsCount; ++i)
					damage += 3 * (waveIdx / 10 - 2);

				return damage;
			}

			return _upgradeDamageStepInfos[waveIdx];
		}

		public float GetEnemySpeed(int waveIdx) {
			if (waveIdx > _upgradeSpeedStepInfos.Count - 1) {
				var steps   = (waveIdx - (_upgradeSpeedStepInfos.Count - 1)) / 10;
				var baseVal = 0.08f;
				if (_type == EnemyType.Quick)
					baseVal = 0.2f;
				else if (_type == EnemyType.Durable)
					baseVal = 0.05f;
				else if (_type == EnemyType.Boss)
					baseVal = 0.04f;

				var speed = _upgradeSpeedStepInfos[_upgradeSpeedStepInfos.Count - 1];
				speed += baseVal * (steps + 1);

				return speed;
			}

			return _upgradeSpeedStepInfos[waveIdx];
		}

		public float GetEnemyCoreCurrency(int waveIdx) {
			if (waveIdx > _baseCoreCurrency.Count - 1) {
				var steps   = (waveIdx - (_baseCoreCurrency.Count - 1)) / 10;
				var baseVal = 1;
				if (_type == EnemyType.Quick)
					baseVal = 2;
				else if (_type == EnemyType.Durable)
					baseVal = 3;
				else if (_type == EnemyType.Boss)
					baseVal = 5;

				var currency = _baseCoreCurrency[_baseCoreCurrency.Count - 1];
				currency += baseVal * (steps + 1);

				return currency;
			}

			return _baseCoreCurrency[waveIdx];
		}

		public float GetEnemyMetaCurrency(int waveIdx) {
			if (waveIdx > _baseMetaCurrency.Count - 1) {
				var steps   = (waveIdx - (_baseMetaCurrency.Count - 1)) / 10;
				var baseVal = 1;
				if (_type == EnemyType.Quick)
					baseVal = 2;
				else if (_type == EnemyType.Durable)
					baseVal = 3;
				else if (_type == EnemyType.Boss)
					baseVal = 5;

				var currency = _baseMetaCurrency[_baseMetaCurrency.Count - 1];
				currency += baseVal * (steps + 1);

				return currency;
			}

			return _baseMetaCurrency[waveIdx];
		}
#endif
	}
}
