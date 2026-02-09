using Core;

namespace Game.Ability {
	public class ActiveAbilityInfo {
		private AbilityType _type;
		private float       _value;
		private float       _time;

		public AbilityType Type       => _type;
		public bool        IsComplete => _time <= 0f;
		
		public ActiveAbilityInfo(AbilityType type, float value, float time) {
			_type  = type;
			_value = value;
			_time  = time;
		}

		public void UpdateTime() {
			_time -= MyTime.deltaTime;
			if (_time <= 0f)
				_time = 0f;
		}
	}
}
