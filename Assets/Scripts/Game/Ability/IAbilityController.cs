using Game.Player;

namespace Game.Ability {
	public interface IAbilityController {
		void Initialize(PlayerView playerView, AbilityDB abilityDB);
		void TryActivateAbility(AbilityType type);
		void Update();
		float GetCooldown(AbilityType type);
	}
}
