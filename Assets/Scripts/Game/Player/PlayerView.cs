using System.Collections.Generic;
using Core;
using Core.Controllers;
using Core.Utils;
using Game.Ability;
using Game.Boards;
using Game.Enemy;
using Game.Cards;
using Spine;
using TMPro;
using Ui;
using UnityEngine;

namespace Game.Player {
	public enum PlayerState {
		Wait   = 1,
		Attack = 2,
		Die    = 3,
		Shoot  = 4,
		Pause  = 5,
	}

	public class PlayerView : MonoBehaviour {
		[SerializeField] private Transform        _root;
		[SerializeField] private Animator         _anim;
		[SerializeField] private Transform        _targetForShoot;
		[SerializeField] private Transform        _overGameLayer;
		[SerializeField] private GameObject       _damageViewPrefab;
		[SerializeField] private GameObject       _revivePrefab;
		[SerializeField] private ProgressBar      _healthProgress;
		[SerializeField] private TextMeshProUGUI  _healthLabel;
		[SerializeField] private CircleCollider2D _rangeCollider;

		private IPlayer _player;

		private PlayerState _state     = PlayerState.Wait;
		private PlayerState _prevState = PlayerState.Wait;

		private PlayerStats _inGameStats;
		private EnemyView   _target;
		private EventData   _shootEventData;
		private GameObject  _reviveObj;

		private float _attackSpeedAdd = 0f;
		private float _timeToShot;
		private float _timeToRegen = 1f;
		private float _currentTimeToShot;
		private float _currentHealth;
		private float _maxHealth;
		private float _invincebleTime;

		private bool _canRevive    = true;
		private bool _isInvincible = false;

		public float       CurrentHealth => _currentHealth;
		public EnemyType   KilledByType  { get;         set; } = EnemyType.Common;
		public GameBoard   Board         { private get; set; }
		public bool        WasDead       { get;         set; } = false;
		public PlayerStats Stats         => _inGameStats;
		public PlayerState State         => _state;
		public bool        IsDead        => _state == PlayerState.Die;

		public void SetInvincibleAfterAds(float time) {
			_state         = PlayerState.Attack;
			_currentHealth = _maxHealth / 2f;

			_isInvincible   = true;
			_invincebleTime = time;

			_anim.SetTrigger("Idle");
		}

		public Dictionary<string, int> GetInGameStatsLevelUp() {
			var statslevelUpInfo = new Dictionary<string, int>();

			for (var i = 0; i < _inGameStats._statInfos.Count; ++i) {
				var type    = _inGameStats._statInfos[i]._type.ToString();
				var levelUp = _inGameStats._statInfos[i]._levelUp;

				statslevelUpInfo.Add(type, levelUp);
			}

			return statslevelUpInfo;
		}

		public void VampireBiteAction() {
			var addHealth = _maxHealth / 2f;
			var delta     = _maxHealth - _currentHealth;

			if (addHealth > delta)
				addHealth = delta;

			var needSpawnHealth = _currentHealth < _maxHealth;
			_currentHealth += addHealth;

			SetHealthProgress(true);
			
			if (needSpawnHealth)
				ShowAddHealth((int) addHealth);
		}

		private void ShowAddHealth(int addCount) {
			var dmgViewObj = Instantiate(_damageViewPrefab, _overGameLayer);
			var size       = gameObject.GetComponent<RectTransform>().sizeDelta;
			var pos        = transform.position;
			pos.y += size.y / 2f;

			dmgViewObj.transform.position = pos;

			var view = dmgViewObj.GetComponent<DamageView>();
			view.Init($"+{addCount}", Color.green);
			view.StartAnim();
		}

		public void PlayWaveSkipAnim() {
			_anim.SetTrigger(CardType.WaveSkip.ToString());
		}

		private void Awake() {
			_player = GameController.Instance.Player;

			SetAnimationCurrentSpeed();
			InitStats();
			CalcTimeToShot();
			InitHealth();
			CheckNeedCardsFX();
		}

		public void SetAnimationCurrentSpeed() {
			_anim.speed = MyTime.acceleration;
		}

		private void CheckNeedCardsFX() {
			var seq         = LeanTween.sequence();
			var activeCards = _player.GetActiveCards();
			for (var i = 0; i < activeCards.Count; ++i) {
				if (activeCards[i] == CardType.Fortification) {
					seq.append(() => { _anim.SetTrigger(CardType.Fortification.ToString()); });

					var animLength = Utility.GetAnimLength(_anim, "PlayerFortification");
					animLength /= MyTime.acceleration;
					seq.append(animLength);
				}
				else if (activeCards[i] == CardType.HealthUp) {
					seq.append(() => { _anim.SetTrigger(CardType.HealthUp.ToString()); });

					var animLength = Utility.GetAnimLength(_anim, "PlayerHelthUp");
					animLength /= MyTime.acceleration;
					seq.append(animLength);
				}
			}
		}

		public void CalcTimeToShot() {
			var shotPerSec    = _inGameStats.GetStatByType(PlayerStatType.ShotsPerSec);
			var shotPerSecVal = shotPerSec._value;

			_timeToShot = 1f / shotPerSecVal;
		}

		private void InitStats() {
			_inGameStats = new PlayerStats();
			_inGameStats.CreateInGameStats(GameController.Instance.Player.Stats);
		}

		private void InitHealth() {
			_maxHealth = _inGameStats.GetStatByType(PlayerStatType.Health)._value;
			TryApplayHealthUpCard();

			_currentHealth = _maxHealth;
			SetHealthProgress(false);
		}

		private void TryApplayHealthUpCard() {
			var activeCard = GameController.Instance.Player.GetActiveCardByType(CardType.HealthUp);
			if (activeCard != null) {
				var cardConfig = GameController.Instance.DB.CardsDB.GetCardConfig(activeCard._type);
				var currLevel  = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(activeCard._count);

				var healthMult = cardConfig.GetCardValueByLvlUp(currLevel - 1) / 100f;
				_maxHealth += (_maxHealth * healthMult);

				Debug.Log($"[Cards]: {activeCard._type} applayed!");
			}
		}

		private void SetHealthProgress(bool needTween) {
			_healthLabel.text = $"{(int) _currentHealth}/{_maxHealth}";

			var healthProgress = (int) _currentHealth / _maxHealth;
			_healthProgress.SetProgress(healthProgress, needTween);
		}

		public void UpdateHealth() {
			var needUpdateCurrHealth = _currentHealth >= _maxHealth;

			_maxHealth = _inGameStats.GetStatByType(PlayerStatType.Health)._value;
			TryApplayHealthUpCard();

			if (needUpdateCurrHealth)
				_currentHealth = _maxHealth;

			SetHealthProgress(false);
		}

		public void SetAttackState() {
			_state             = PlayerState.Attack;
			_currentTimeToShot = 0;
		}

		public void SetPauseState(bool isPause) {
			if (isPause) {
				_prevState = _state;
				_state     = PlayerState.Pause;
			}
			else {
				_state = _prevState;
			}
		}

		private void Update() {
			if (_state == PlayerState.Pause || _state == PlayerState.Die)
				return;

			UpdateHealthRegen();
			UpdateInvincibleTime();

			if (_state == PlayerState.Attack)
				UpdateAttackState();
		}

		private void UpdateInvincibleTime() {
			if (!_isInvincible)
				return;

			_invincebleTime -= MyTime.deltaTime;
			if (_invincebleTime <= 0f) {
				_isInvincible = false;

				Destroy(_reviveObj);
				_reviveObj = null;
			}
		}

		private void UpdateHealthRegen() {
			if (_state == PlayerState.Die)
				return;

			if (_currentHealth < _maxHealth) {
				_timeToRegen -= MyTime.deltaTime;
				if (_timeToRegen <= 0f) {
					_timeToRegen = 1f;

					var healthRegen    = _inGameStats.GetStatByType(PlayerStatType.HealthRegen);
					var healthRegenVal = healthRegen._value;
					_currentHealth += healthRegenVal;

					if (_currentHealth > _maxHealth)
						_currentHealth = _maxHealth;

					SetHealthProgress(true);
				}
			}
		}

		private void UpdateAttackState() {
			var target = Board.GetNearestEnemy(transform.position);

			_currentTimeToShot -= (0.8f + _attackSpeedAdd) * MyTime.deltaTime;
			if (target && _currentTimeToShot <= 0f) {
				_currentTimeToShot = _timeToShot;

				SetDirection(target.transform.localPosition);
				Attack(target);
			}
		}

		private void SetDirection(Vector3 targetPos) {
			var dir   = (targetPos - transform.localPosition).normalized;
			var angle = Vector2.Angle(new Vector2(1, 0), dir);

			if (targetPos.y < transform.localPosition.y)
				angle = -angle;

			var angles = _root.transform.eulerAngles;
			angles.z                    = angle;
			_root.transform.eulerAngles = angles;
		}

		private void Attack(EnemyView target) {
			_target = target;
			_anim.SetTrigger("GunFire");

			CreateBullet();
		}

		private void CreateBullet() {
			var damage = CalculateDamage(_target.gameObject, out var isCrit);
			Board.AttackTarget(_targetForShoot, _target, damage, isCrit);
		}

		public void TakeDamage(float damage, EnemyType type) {
			if (_isInvincible || CheaterController.IsGodMode)
				return;

			var resistInfo = _inGameStats.GetStatByType(PlayerStatType.PhysicalResist);
			if (resistInfo != null) {
				damage -= resistInfo._value;
				if (damage < 1)
					damage = 1;
			}

			var absoluteResistInfo = _inGameStats.GetStatByType(PlayerStatType.AbsoluteResist);
			if (absoluteResistInfo != null) {
				var resistVal = (int) (damage * absoluteResistInfo._value / 100f);
				damage -= resistVal;
				if (damage < 1)
					damage = 1;
			}

			TryApplayFortificationCard(ref damage);

			_currentHealth -= damage;
			if (_currentHealth <= 0) {
				_currentHealth = 0f;
				TryApplayReviveCard();
			}

			ShowDamageView((int) damage);
			SetHealthProgress(true);

			if (_currentHealth <= 0) {
				KilledByType = type;
				_state       = PlayerState.Die;
				_anim.SetTrigger("Death");
				Board.SetEnemiesIdleState();
				Board.SetEndRunState();
			}
		}

		private void TryApplayFortificationCard(ref float damage) {
			var activeCard = GameController.Instance.Player.GetActiveCardByType(CardType.Fortification);
			if (activeCard != null) {
				var cardConfig = GameController.Instance.DB.CardsDB.GetCardConfig(activeCard._type);
				var currLevel  = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(activeCard._count);

				var fortVal = cardConfig.GetCardValueByLvlUp(currLevel - 1);
				damage -= fortVal;
				if (damage < 1f)
					damage = 1f;

				Debug.Log($"[Cards] {activeCard._type} applayed!");
			}
		}

		private void TryApplayReviveCard() {
			if (!_canRevive)
				return;

			var activeCard = GameController.Instance.Player.GetActiveCardByType(CardType.Revive);
			if (activeCard != null) {
				var cardConfig = GameController.Instance.DB.CardsDB.GetCardConfig(activeCard._type);
				var currLevel  = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(activeCard._count);

				_canRevive      = false;
				_invincebleTime = cardConfig.GetCardValueByLvlUp(currLevel - 1);
				_isInvincible   = true;
				_currentHealth  = _maxHealth / 2f;

				AddReviveFx(_invincebleTime);
				Board.SetReviveIconActive(false);
			}
		}

		private void AddReviveFx(float lifeTime) {
			var reviveObj = Instantiate(_revivePrefab, transform.parent);
			reviveObj.transform.position = transform.position;

			_reviveObj = reviveObj;
			Utility.SetParticleLifetime(reviveObj, lifeTime);
		}

		private void ShowDamageView(int dmg) {
			if (dmg <= 0)
				return;

			var dmgViewObj = Instantiate(_damageViewPrefab, _overGameLayer);
			var size       = gameObject.GetComponent<RectTransform>().sizeDelta;
			var pos        = transform.position;
			pos.y += size.y / 2f;

			dmgViewObj.transform.position = pos;

			var view = dmgViewObj.GetComponent<DamageView>();
			view.Init($"-{dmg}", Color.white);
			view.StartAnim();
		}

		private float CalculateDamage(GameObject target, out bool isCrit) {
			var damage          = _inGameStats.GetStatByType(PlayerStatType.Damage)._value;
			var addDamage       = _inGameStats.GetStatByType(PlayerStatType.AddDmgPerRange)._value;
			var critChance      = _inGameStats.GetStatByType(PlayerStatType.CritChance)._value;
			var critMult        = _inGameStats.GetStatByType(PlayerStatType.CritMult)._value;
			var superCritChance = _inGameStats.GetStatByType(PlayerStatType.SuperCritChance)._value;
			var superCritMult   = _inGameStats.GetStatByType(PlayerStatType.SuperCritMult)._value;

			var distForTarget = (target.transform.localPosition - transform.localPosition).magnitude;
			var addDmgPercent = (int) (distForTarget / GameController.Instance.Config.PixelsInMeter) * addDamage;
			damage += damage * addDmgPercent / 100f;

			var critProb = new RandomProb();
			critProb.AddValue("yes", critChance);
			critProb.AddValue("no", 100 - critChance);

			var randVal = critProb.GetRandomValue();
			isCrit = false;

			if (randVal == "yes") {
				isCrit =  true;
				damage *= critMult;

				var superCritProb = new RandomProb();
				superCritProb.AddValue("yes", superCritChance);
				superCritProb.AddValue("no", 100 - superCritChance);

				randVal = critProb.GetRandomValue();
				if (randVal == "yes") {
					damage *= superCritMult;
				}
			}

			if (CheaterController.IsGodMode)
				return 100000;

			var activeCard = GameController.Instance.Player.GetActiveCardByType(CardType.AttackUp);
			if (activeCard != null) {
				var cardConfig = GameController.Instance.DB.CardsDB.GetCardConfig(activeCard._type);
				var currLevel  = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(activeCard._count);

				var dmgMult = cardConfig.GetCardValueByLvlUp(currLevel - 1) / 100f;
				damage += (damage * dmgMult);
				Debug.Log($"[Cards]: {activeCard._type} applayed!");
			}

			return damage;
		}

		public void ApplayAbility(AbilityType type, float value) {
			if (type == AbilityType.AddAttackSpeed) {
				SetAttackSpeedMult(value);
			}
		}

		public void ResetAbility(AbilityType type) {
			if (type == AbilityType.AddAttackSpeed) {
				SetAttackSpeedMult(0);
			}
		}

		private void SetAttackSpeedMult(float value) {
			_attackSpeedAdd = value;
		}

		public Vector3 GetPosition() {
			var localPos = transform.localPosition;
			var offset   = _rangeCollider.offset;

			localPos.x += offset.x;
			localPos.y += offset.y;

			return localPos;
		}

		public float GetRadius() {
			return _rangeCollider.radius;
		}

		public float GetAttackRange() {
			var range = _inGameStats.GetStatByType(PlayerStatType.Range);
			return range._value;
		}
	}
}
