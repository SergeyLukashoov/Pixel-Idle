using System.Collections.Generic;
using System.Linq;
using Game.Player;

namespace Game.Ability {
	public class AbilityController : IAbilityController {
		private AbilityDB                 _abilityDB;
		private List<ActiveAbilityInfo>   _activeAbilityInfos   = new List<ActiveAbilityInfo>();
		private List<CooldownAbilityInfo> _cooldownAbilityInfos = new List<CooldownAbilityInfo>();
		private PlayerView                _playerView;

		public void Initialize(PlayerView playerView, AbilityDB abilityDB) {
			_playerView = playerView;
			_abilityDB  = abilityDB;
		}


		public float GetCooldown(AbilityType type) {
			var cooldownAbility = _cooldownAbilityInfos.FirstOrDefault(x => x.Type == type);
			if (cooldownAbility == null)
				return -1f;

			return cooldownAbility.CooldownTime;
		}
		
		public void TryActivateAbility(AbilityType type) {
			var cooldownAbility = _cooldownAbilityInfos.FirstOrDefault(x => x.Type == type);
			if (cooldownAbility != null)
				return;
			
			var abilityInfo = _abilityDB.GetAbilityInfo(type);
			if (abilityInfo == null) {
				return;
			}

			var activeAbility = new ActiveAbilityInfo(
				abilityInfo.Type,
				abilityInfo.Value,
				abilityInfo.Time
			);

			_activeAbilityInfos.Add(activeAbility);
			_playerView.ApplayAbility(abilityInfo.Type, abilityInfo.Value);

			var cooldownInfo = new CooldownAbilityInfo(
				abilityInfo.Type,
				abilityInfo.Cooldown
			);
			
			_cooldownAbilityInfos.Add(cooldownInfo);
		}

		public void Update() {
			UpdateActiveAbilities();
			UpdateAbilitiesOnCooldown();
		}

		private void UpdateAbilitiesOnCooldown() {
			if (_cooldownAbilityInfos.Count == 0)
				return;
			
			for (var i = 0; i < _cooldownAbilityInfos.Count; ++i)
				_cooldownAbilityInfos[i].UpdateCooldown();

			CheckCooldownEnd();
		}

		private void CheckCooldownEnd() {
			var completCooldown = _cooldownAbilityInfos.FindAll(x => x.IsEnd);
			if (completCooldown.Count == 0)
				return;

			for (var i = 0; i < completCooldown.Count; ++i) {
				_cooldownAbilityInfos.Remove(completCooldown[i]);
			}
		}
		
		private void UpdateActiveAbilities() {
			if (_activeAbilityInfos.Count == 0)
				return;

			for (var i = 0; i < _activeAbilityInfos.Count; ++i)
				_activeAbilityInfos[i].UpdateTime();

			CheckAbilitiesComplete();
		}

		private void CheckAbilitiesComplete() {
			var completAbilities = _activeAbilityInfos.FindAll(x => x.IsComplete);
			if (completAbilities.Count == 0)
				return;

			for (var i = 0; i < completAbilities.Count; ++i) {
				_playerView.ResetAbility(completAbilities[i].Type);
				_activeAbilityInfos.Remove(completAbilities[i]);
			}
		}
	}
}
