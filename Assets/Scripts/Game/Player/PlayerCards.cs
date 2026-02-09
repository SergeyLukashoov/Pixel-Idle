using System;
using System.Collections.Generic;
using Game.Cards;

namespace Game.Player {
	[Serializable]
	public class CardInfo {
		public CardType _type;
		public int      _count;
	}

	[Serializable]
	public class PlayerCards {
		public List<CardType> _activeCards    = new List<CardType>();
		public List<CardInfo> _collectedCards = new List<CardInfo>();
	}
}
