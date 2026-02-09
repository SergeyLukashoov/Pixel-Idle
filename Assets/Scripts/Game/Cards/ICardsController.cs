using System.Collections.Generic;
using Game.Player;

namespace Game.Cards {
	public interface ICardsController {
		void Initialize(IPlayer player);
		public CardType CheckNeedDropCard(int towerId, int bossWaveId);
		public CardType DropCard(List<CardType> forceCardTypes = null);
		public void ApplayCard(CardType card);
		public void RemoveCard(CardType card);
		public float GetCoinsBoostVal();
	}
}
