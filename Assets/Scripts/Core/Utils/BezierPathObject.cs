using System.Collections.Generic;
using Core.Controllers;
using Game.Map;
using UnityEngine;

namespace Core.Utils {
	public class BezierPathObject : MonoBehaviour {
		[SerializeField] private Transform    _p0;
		[SerializeField] private Transform    _p1;
		[SerializeField] private Transform    _p2;
		[SerializeField] private Transform    _p3;
		[SerializeField] private LineRenderer _lineRenderer;

		[SerializeField] private Material _activePathMat;
		[SerializeField] private Material _disablePathMatPink;
		[SerializeField] private Material _disablePathMatPurple;

		[SerializeField] private TowerObject _firstTower;
		[SerializeField] private TowerObject _lastTower;

		private       BezierPath _path;
		private const int        SegmentsCount    = 15;
		private const int        DefaultLineSize = 20;

		public void Start() {
			InitPath();
		}

		public void InitPath() {
			var lineMaterial = _activePathMat;

			var lastTowerUnlocked = GameController.Instance.Player.IsTowerUnlocked(_lastTower.ID);

			if (!_firstTower) {
				if (!lastTowerUnlocked)
					lineMaterial = _disablePathMatPurple;
			}
			else {
				var firstTowerAvailable = _firstTower.IsTowerAvailable();
				var firstTowerUnlocked  = GameController.Instance.Player.IsTowerUnlocked(_firstTower.ID);
				if (!firstTowerAvailable || !firstTowerUnlocked) {
					gameObject.SetActive(false);
					return;
				}

				if (!lastTowerUnlocked)
					lineMaterial = _disablePathMatPurple;
			}

			_path                       = new BezierPath(_p0, _p1, _p2, _p3);
			_lineRenderer.positionCount = _path.Path.Count + 1;
			_lineRenderer.material      = lineMaterial;

			for (var i = 0; i < _path.Path.Count; ++i)
				_lineRenderer.SetPosition(i, _path.Path[i]);

			_lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _p3.localPosition);
		}

		public void ShowPath() {
			var startColor = _lineRenderer.startColor;
			var endColor = _lineRenderer.endColor;

			startColor.a = 0f;
			endColor.a = 0f;

			_lineRenderer.startColor = startColor;
			_lineRenderer.endColor = endColor;

			LeanTween.value(_lineRenderer.gameObject, 0f, 1f, 0.4f).setOnUpdate(
				(alpha) => {
					startColor.a = alpha;
					endColor.a   = alpha;
					
					_lineRenderer.startColor = startColor;
					_lineRenderer.endColor   = endColor;
				});
		}
		
		public void UpdateNextTower() {
			_lastTower.UpdateView();
			_lastTower.RemoveFog(true);
		}

		public void SetLineSize(float scale) {
			var width      = DefaultLineSize;
			var startWidth = DefaultLineSize * 0.5f;
			if (scale == 0.5f) {
				width      = (int) (DefaultLineSize * scale);
				startWidth = DefaultLineSize;
			}
			
			LeanTween.value(gameObject, startWidth, width, 0.5f).setOnUpdate(
				(val) => {
					_lineRenderer.startWidth = val;
					_lineRenderer.endWidth   = val;
				});
		}

		private void OnDrawGizmos() {
#if UNITY_EDITOR
			var prevPoint = _p0.position;

			Gizmos.color = Color.red;
			Gizmos.DrawSphere(_p0.position, 20);
			Gizmos.DrawSphere(_p1.position, 20);
			Gizmos.DrawSphere(_p2.position, 20);
			Gizmos.DrawSphere(_p3.position, 20);

			Gizmos.color = Color.blue;
			for (var i = 0; i < SegmentsCount + 1; ++i) {
				var param     = i / (float) SegmentsCount;
				var currPoint = Bezier.GetPoint(_p0.position, _p1.position, _p2.position, _p3.position, param);
				Gizmos.DrawLine(prevPoint, currPoint);
				prevPoint = currPoint;
			}
#endif
		}
	}
}
