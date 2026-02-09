using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Game.Ability {
	public class CooldownAbilityInfo {
		private AbilityType _type;
		private float       _coolDownTime;

		public AbilityType Type         => _type;
		public bool        IsEnd        => _coolDownTime <= 0f;
		public float       CooldownTime => _coolDownTime;
		
		public CooldownAbilityInfo(AbilityType type, float coolDownTime) {
			_type         = type;
			_coolDownTime = coolDownTime;
		}

		public void UpdateCooldown() {
			_coolDownTime -= MyTime.deltaTime;
		}
	}
}
