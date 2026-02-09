using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Ability {
	[Serializable]
	public class AbilityInfo {
		public AbilityType Type;
		public float       Value;
		public float       Time;
		public int         Price;
		public float       Cooldown;
	}

	[CreateAssetMenu(fileName = "AbilityDB", menuName = "DB/Create AbilityDB", order = 56)]
	public class AbilityDB : ScriptableObject {
		[SerializeField] private List<AbilityInfo> _abilitiesInfo = new List<AbilityInfo>();

		public AbilityInfo GetAbilityInfo(AbilityType type) {
			return _abilitiesInfo.FirstOrDefault(x => x.Type == type);
		}
	}
}
