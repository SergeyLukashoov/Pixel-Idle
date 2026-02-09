using Core.Config.Chests;
using Core.Config.Currency;
using Core.Config.Mine;
using Core.Config.Store;
using Core.Config.Towers;
using Game.Ability;
using Game.Cards;
using UnityEngine;

namespace Core.Config {
	public class DB : MonoBehaviour {
		[SerializeField] private CardsDB    _cardsDB;
		[SerializeField] private AbilityDB  _abilityDB;
		[SerializeField] private CurrencyDB _currencyDB;
		[SerializeField] private MineDB     _mineDB;
		[SerializeField] private TowersDB   _towersDB;
		[SerializeField] private ChestsDB   _chestsDB;
		[SerializeField] private StoreDB    _storeDB;

		public CardsDB    CardsDB    => _cardsDB;
		public AbilityDB  AbilityDB  => _abilityDB;
		public CurrencyDB CurrencyDB => _currencyDB;
		public MineDB     MineDB     => _mineDB;
		public TowersDB   TowersDB   => _towersDB;
		public ChestsDB   ChestsDB   => _chestsDB;
		public StoreDB    StoreDB    => _storeDB;
	}
}
