using UnityEngine;

namespace Core {
	public class MyTime {
		private static float _acceleration = 1.5f;
		public static  float deltaTime              => Time.deltaTime * _acceleration;
		public static  float acceleration           => _acceleration;
		public static  float maxAcceleration        { get; set; } = 2.5f;
		public static  float maxDefaultAcceleration { get; set; } = 2.5f;

		public static void InitAcceleration(float val) {
			_acceleration = val;
		}

		public static void SetAcceleration(float delta) {
			_acceleration += delta;
		}

		public static void SetMaxAcceleration() {
			_acceleration = maxAcceleration;
		}
	}
}
