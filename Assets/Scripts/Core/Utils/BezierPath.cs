using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils {
	public class BezierPath {
		private Transform _p0;
		private Transform _p1;
		private Transform _p2;
		private Transform _p3;
		
		private List<Vector2> _path = new List<Vector2>();

		private const int PointsCount = 15;

		public List<Vector2> Path => _path;

		public BezierPath(Transform p0, Transform p1, Transform p2, Transform p3) {
			_p0 = p0;
			_p1 = p1;
			_p2 = p2;
			_p3 = p3;
			
			UpdatePath();
		}

		private void UpdatePath() {
			var step = 1 / (float) PointsCount;
			for (var i = 0f; i < 1f; i += step) {
				var point = Bezier.GetPoint(_p0.localPosition, _p1.localPosition, _p2.localPosition, _p3.localPosition, i);
				_path.Add(point);
			}
		}
	}
}
