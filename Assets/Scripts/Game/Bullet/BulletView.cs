using System;
using System.Collections.Generic;
using Core;
using Core.Controllers;
using Game.Enemy;
using Game.Player;
using UnityEngine;

namespace Game.Bullet {
	public class BulletView : MonoBehaviour {
		[SerializeField] private bool _needTrail;

		public bool       IsCrit        { get; set; }
		public bool       IsVampireBite { get; set; }
		public float      Damage        { get; set; }
		public EnemyView  Target        { get; set; }
		public PlayerView Player        { get; set; }

		private List<GameObject> _trailObjs = new List<GameObject>();
		private float            _speed;
		private LTDescr          _moveTween;

		private void Awake() {
			_speed = GameController.Instance.Config.BulletSpeed;
		}

		private void InitTrailObjs() {
			if (!_needTrail)
				return;

			for (var i = 0; i < 2; ++i) {
				var trailObj = Instantiate(gameObject, transform);
				trailObj.transform.SetAsFirstSibling();
				trailObj.transform.localScale              = Vector3.one;
				trailObj.transform.localPosition           = Vector3.zero;
				trailObj.transform.eulerAngles             = transform.eulerAngles;
				trailObj.GetComponent<CanvasGroup>().alpha = GetAlpha(i);

				_trailObjs.Add(trailObj);
			}
		}

		private float GetAlpha(int idx) {
			if (idx == 0)
				return 0.5f;
			else
				return 0.2f;
		}

		public void Move(Action<BulletView> onDestroy) {
			InitTrailObjs();

			Vector3 targetPos;
			if (Player)
				targetPos = Player.transform.position;
			else
				targetPos = Target.TargetForBullet.position;

			var dir           = (targetPos - transform.position).normalized;
			var distForTarget = (targetPos - transform.position).magnitude;
			var flyTime       = distForTarget / _speed;

			_moveTween = LeanTween.move(gameObject, targetPos, flyTime / MyTime.acceleration).setOnUpdate(
				(float val) => { ApplayPosToTrail(dir); }).setOnComplete(
				() => {
					onDestroy?.Invoke(this);

					if (Target)
						Target.TakeDamage(Damage, IsCrit, IsVampireBite);
					else if (Player)
						Player.TakeDamage(Damage, EnemyType.Range);

					Destroy(gameObject);
					_moveTween = null;
				});
		}

		private void ApplayPosToTrail(Vector3 dir) {
			if (!_needTrail)
				return;

			var shiftDir = dir * -1;
			for (var i = 0; i < _trailObjs.Count; ++i) {
				var pos = transform.position;
				pos.x += shiftDir.x * ((i + 1) * 30);
				pos.y += shiftDir.y * ((i + 1) * 30);

				_trailObjs[i].transform.position = pos;
			}
		}

		public void SetPauseState(bool isPause) {
			if (_moveTween == null)
				return;

			if (isPause)
				_moveTween.pause();
			else {
				_moveTween.resume();
			}
		}
	}
}
