using System;
using Core;
using Core.Controllers;
using Game.Boards;
using Game.Player;
using Spine;
using Spine.Unity;
using TMPro;
using Ui;
using UnityEngine;
using Event = Spine.Event;

namespace Game.Enemy {
	internal enum EnemyState {
		Stay       = 1,
		Move       = 2,
		Attack     = 3,
		Die        = 4,
		Spawn      = 5,
		WaitAttack = 6,
		Idle       = 7,
		Pause      = 8,
	}
	public class EnemyView : MonoBehaviour {
		[SerializeField] private ProgressBar      _healthBar;
		[SerializeField] private Transform        _objAnim;
		[SerializeField] private Transform        _targetForDamage;
		[SerializeField] private GameObject       _damageViewPrefab;
		[SerializeField] private GameObject       _deathFxPrefab;
		[SerializeField] private SkeletonGraphic  _animation;
		[SerializeField] private Canvas           _healthCanvas;
		[SerializeField] private TextMeshProUGUI  _statsLabel;
		[SerializeField] private CircleCollider2D _circleCollider2D;

		private LTDescr _delayDeathAnimDescr;

		private EnemyState _state     = EnemyState.Stay;
		private EnemyState _prevState = EnemyState.Stay;

		private PlayerView  _target;
		private EnemyInfo   _info;
		private EventData   _attackEventData;
		private CanvasGroup _healthCG;

		private float _currentHealth;
		private float _maxHealth;
		private float _speed;
		private float _damage;
		private float _coinsDrop;
		private float _expDrop;
		private float _timeToAttack = 0f;
		private int   _waveIdx;
		private bool  _isSetDestroyed = false;

		public Action<EnemyView, float, float> OnDieAction;

		public Transform  TargetForBullet => _targetForDamage;
		public bool       IsDead          => _currentHealth <= 0f;
		public float      CurrentHealth   => _currentHealth;
		public float      Damage          => _damage;
		public int        WaveIdx         => _waveIdx;
		public EnemyType  Type            => _info.Type;
		public GameObject DeathFx         => _deathFxPrefab;

		public GameBoard GameBoard      { get; set; }
		public bool      SkipFromSearch { get; set; } = false;

		public void SetPauseState(bool isPause) {
			if (isPause) {
				_prevState           = _state;
				_state               = EnemyState.Pause;
				_animation.timeScale = 0f;
			}
			else {
				_state               = _prevState;
				_animation.timeScale = MyTime.acceleration;
			}
		}

		public void SetDirection() {
			var currPos   = transform.position;
			var targetPos = _target.transform.position;

			var moveDir = (targetPos - currPos).normalized;
			var angle   = Vector2.Angle(new Vector2(-1, 0), moveDir);

			if (currPos.y < targetPos.y)
				angle = -angle;

			var angles = _objAnim.transform.eulerAngles;
			angles.z                       = angle;
			_objAnim.transform.eulerAngles = angles;
		}

		public void SetAnimationCurrentSpeed() {
			_animation.timeScale = MyTime.acceleration;
		}

		public void Initialize(EnemyInfo info, PlayerView target, int waveIdx) {
			_info            = info;
			_target          = target;
			_waveIdx         = waveIdx;
			_attackEventData = _animation.Skeleton.Data.FindEvent("attack");

			InitStats();
			InitHealthBar();
			InitDebugStatsLabel();
			SetAnimationCurrentSpeed();
		}

		private void InitHealthBar() {
			_healthCG                      = _healthBar.GetComponent<CanvasGroup>();
			_healthCG.alpha                = 0f;
			_healthCanvas                  = _healthBar.GetComponent<Canvas>();
			_healthCanvas.overrideSorting  = true;
			_healthCanvas.sortingLayerName = "OverGameLayer";

			UpdateHealthBar(false);
		}

		private void InitDebugStatsLabel() {
			_statsLabel.text = $"Atk:{_damage}\nCns:{_coinsDrop}\nExp:{_expDrop}";

			_statsLabel.gameObject.SetActive(CheaterController.NeedShowEnemyStats);
			//_healthLabel.gameObject.SetActive(CheaterController.NeedShowEnemyStats);
		}

		private void InitStats() {
#if HARDCORE
			var addHealth = _info.AddHealthByWave(_waveIdx);
			_currentHealth =  _info.Health + addHealth;
			_currentHealth += _currentHealth * GameController.Instance.PlayedTowerInfo.EnemyHealthMultiplier;

			var addDamage = _info.AddDamageByWave(_waveIdx);
			_damage =  _info.Damage + addDamage;
			_damage += _damage * GameController.Instance.PlayedTowerInfo.EnemyDamageMultiplier;

			_speed = _info.Speed + _waveIdx * _info.SpeedAddByWave;
			if (_speed > _info.SpeedMax)
				_speed = _info.SpeedMax;

			_expDrop   = _info.Exp + _waveIdx * _info.ExpAddByWave;
			_coinsDrop = _info.Coins + _waveIdx * _info.CoinsAddByWave;
#else
			_currentHealth = _info.GetEnemyHealth(_waveIdx);
			_damage = _info.GetEnemyDamage(_waveIdx);
			_speed = _info.GetEnemySpeed(_waveIdx);
			_expDrop = _info.GetEnemyCoreCurrency(_waveIdx);
			_coinsDrop = _info.GetEnemyMetaCurrency(_waveIdx);
#endif
			_maxHealth = _currentHealth;
		}

		private void UpdateHealthBar(bool needTween) {
			//if (_healthLabel.gameObject.activeSelf)
			//_healthLabel.text = _currentHealth.ToString();

			var progress = _currentHealth / _maxHealth;

			if (_healthBar)
				_healthBar.SetProgress(progress, needTween);
		}

		public void Spawn() {
			_state = EnemyState.Spawn;
			_animation.AnimationState.SetAnimation(0, "spawn", false);
		}

		private void MoveToTarget() {
			_state = EnemyState.Move;

			var animName = "run";
			if (_info.Type == EnemyType.Range || _info.Type == EnemyType.Explode)
				animName = "walk";

			_animation.AnimationState.SetAnimation(0, animName, true);
		}

		public void TakeDamage(float dmg, bool isCrit, bool isVampireBite) {
			if (IsDead)
				return;

			_currentHealth -= dmg;
#if !REC_VIDEO
			if (_currentHealth > 0 && _healthCG.alpha == 0f) {
				LeanTween.alphaCanvas(_healthCG, 1f, 0.1f);
			}
#endif

			if (isVampireBite) {
				ShowVampireBiteAnim();
				GameBoard.PlayerView.VampireBiteAction();
			}

			UpdateHealthBar(true);

			if (_currentHealth <= 0f) {
				_state = EnemyState.Die;
				_animation.AnimationState.SetAnimation(0, "death", false);
				ShowDropAnim(_coinsDrop, _expDrop);
				OnDieAction?.Invoke(this, _coinsDrop, _expDrop);
			}
			else {
				ShowDamageView((int) dmg, isCrit);
			}

			PlayDamageAnim();
		}

		private void ShowVampireBiteAnim() {
			var biteObj = Instantiate(GameBoard.VampireBiteFxPrefab, GameBoard.OverGameLayer);
			biteObj.transform.position = transform.position;
			Destroy(biteObj, 3);
		}

		private void PlayDamageAnim() {
			_animation.AnimationState.SetAnimation(1, "damage", false);
			LeanTween.delayedCall(0.3f / MyTime.acceleration, () => {
				if (this) {
					_animation.AnimationState.SetEmptyAnimation(1, 0);
				}
			});
		}

		private void ShowDropAnim(float coinsCount, float expCount) {
			var delay = 0f;
			if (coinsCount > 0) {
				var coinsDropObj = Instantiate(GameBoard.DropViewPrefab, GameBoard.OverGameLayer);
				coinsDropObj.transform.position = _healthBar.transform.position;

				var coinsDropView = coinsDropObj.GetComponent<DropView>();
				coinsDropView.Init((int) coinsCount, PlayerCurrencyType.Coins);
				coinsDropView.StartAnim(delay);
				delay += 0.4f;
			}

			if (expCount > 0) {
				var expDropObj = Instantiate(GameBoard.DropViewPrefab, GameBoard.OverGameLayer);
				expDropObj.transform.position = _healthBar.transform.position;

				var expDropView = expDropObj.GetComponent<DropView>();
				expDropView.Init((int) expCount, PlayerCurrencyType.Exp);
				expDropView.StartAnim(delay);
			}
		}

		private void ShowDamageView(int dmg, bool isCrit) {
			var dmgViewObj = Instantiate(_damageViewPrefab, GameBoard.OverGameLayer);
			dmgViewObj.transform.position = _healthBar.transform.position;

			var color = Color.white;
			if (isCrit)
				color = new Color(255 / 255f, 81 / 255f, 85 / 255f);

			var view = dmgViewObj.GetComponent<DamageView>();
			view.Init($"-{dmg}", color);
			view.StartAnim();
		}

		private void Update() {
			if (_state == EnemyState.Pause)
				return;

			//if (_state != EnemyState.Stay) {
			//if (_target && _target.State == PlayerState.Die) {
			//_state = EnemyState.Stay;
			//}
			//}

			if (_state == EnemyState.Spawn)
				UpdateSpawnState();
			else if (_state == EnemyState.Move)
				UpdateMoveState();
			else if (_state == EnemyState.Attack)
				UpdateAttackState();
			else if (_state == EnemyState.WaitAttack)
				UpdateWaitAttackState();
			else if (_state == EnemyState.Die)
				UpdateDieState();
		}

		private void UpdateDieState() {
			if (_isSetDestroyed)
				return;

			if (_animation.AnimationState.GetCurrent(0).IsComplete) {
				_healthBar.gameObject.SetActive(false);
				_isSetDestroyed = true;
				Destroy(gameObject);
			}
		}

		private void UpdateSpawnState() {
			if (_animation.AnimationState.GetCurrent(0).IsComplete)
				MoveToTarget();
		}

		private void UpdateMoveState() {
			if (!_target)
				return;

			if (IsDead)
				return;

			var targetPos = _target.GetPosition();
			var dir       = (targetPos - transform.localPosition).normalized;
			var pos       = transform.localPosition;

			pos.x += _speed * GameController.Instance.Config.BaseSpeed * dir.x * MyTime.deltaTime;
			pos.y += _speed * GameController.Instance.Config.BaseSpeed * dir.y * MyTime.deltaTime;

			transform.localPosition = pos;

			if (IsInAttackRange(targetPos)) {
				SetAttackState();
			}
		}

		public void SetIdleState(float delay) {
			_prevState = _state;
			_state     = EnemyState.Idle;
			_delayDeathAnimDescr = LeanTween.delayedCall(delay, () => {
				_delayDeathAnimDescr = null;

				if (_animation)
					_animation.AnimationState.SetAnimation(0, "win", true);
			});

			LeanTween.alphaCanvas(_healthCG, 0f, 0.1f);
		}

		public void ResumeState() {
			_state = EnemyState.Move;

			if (_delayDeathAnimDescr != null) {
				LeanTween.cancel(_delayDeathAnimDescr.uniqueId);
				_delayDeathAnimDescr = null;
			}

			if (_state == EnemyState.Attack)
				SetAttackState();
			else if (_state == EnemyState.Move)
				MoveToTarget();
			else if (_state == EnemyState.Spawn)
				Spawn();
		}

		private void SetAttackState() {
			_state = EnemyState.Attack;

			_animation.AnimationState.SetAnimation(0, "attack", false);
			_animation.AnimationState.GetCurrent(0).Event += OnAttackeEvent;
		}

		private void OnAttackeEvent(TrackEntry trackEntry, Event e) {
			if (_attackEventData == e.Data) {
				if (_info.Type == EnemyType.Range) {
					CreateBullet();
				}
				else {
					_target.TakeDamage(_damage, _info.Type);
				}
			}
		}

		private void CreateBullet() {
			GameBoard.CreateBulletForPlayer(this, _target, _damage);
		}

		private bool IsInAttackRange(Vector3 targetPos) {
			if (_info.Type == EnemyType.Range)
				return IsInRangeRadius();
			else
				return IsInMeeleRadius(targetPos);
		}

		private bool IsInMeeleRadius(Vector3 targetPos) {
			Vector2 targetV2Pos = targetPos;
			Vector2 thisV2Pos   = transform.localPosition;

			var dir          = (targetV2Pos - thisV2Pos).normalized;
			var collisionPos = thisV2Pos + dir * _circleCollider2D.radius;

			var dist        = (targetV2Pos - collisionPos).magnitude;
			var attackRange = _target.GetRadius() + _info.AttackRange;
			if (dist <= attackRange)
				return true;

			return false;
		}

		private bool IsInRangeRadius() {
			Vector2 targetV2Pos = _target.transform.position;
			Vector2 thisV2Pos   = transform.position;

			var dist = (targetV2Pos - thisV2Pos).magnitude;

			var rangeInMeters      = GameBoard.PlayerView.Stats.GetStatByType(PlayerStatType.Range);
			var attackRadius       = GameBoard.GetRangeInPixels();
			var playerAttackRadius = attackRadius - (attackRadius / rangeInMeters._value) * 0.5f;
			if (dist <= playerAttackRadius)
				return true;

			return false;
		}

		private void UpdateWaitAttackState() {
			_timeToAttack -= MyTime.deltaTime;
			if (_timeToAttack <= 0f) {
				_timeToAttack = _info.TimeToAttack;

				SetAttackState();
			}
		}

		private void UpdateAttackState() {
			if (_animation.AnimationState.GetCurrent(0).IsComplete) {
				_state = EnemyState.WaitAttack;

				_animation.AnimationState.GetCurrent(0).Event -= OnAttackeEvent;
			}
		}
	}
}
