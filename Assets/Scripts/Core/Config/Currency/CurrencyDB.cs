using System;
using System.Collections.Generic;
using System.Linq;
using Game.Player;
using UnityEngine;

namespace Core.Config.Currency {
	[Serializable]
	public class CurrencyInfo {
		public PlayerCurrencyType Type;
		public Sprite             Icon;
		public Sprite             IconBig;
		public Sprite             IconSmall;
		public Sprite             DisableIcon;
	}
	
	[Serializable, CreateAssetMenu(fileName = "CurrencyDB", menuName = "DB/Create CurrencyDB", order = 55)]
	public class CurrencyDB : ScriptableObject {
		[SerializeField] private List<CurrencyInfo> _currencyInfos = new List<CurrencyInfo>();

		public CurrencyInfo GetCurrencyInfo(PlayerCurrencyType type) {
			return _currencyInfos.FirstOrDefault(x => x.Type == type);
		}
	}
}
