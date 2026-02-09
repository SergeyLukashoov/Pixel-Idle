using System;
using Config.Player;
using Core.Controllers;
using Core.Controllers.Windows;
using Core.Utils;
using Game.Boards;
using Game.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Window;

namespace Ui.Screens.Upgrade {
	public class UpgradeStatView : MonoBehaviour {
		[SerializeField] private Image           _back;
		[SerializeField] private Image           _priceBack;
		[SerializeField] private Image           _currencyIcon;
		[SerializeField] private TextMeshProUGUI _labelName;
		[SerializeField] private TextMeshProUGUI _labelValue;
		[SerializeField] private TextMeshProUGUI _labelPrice;
		[SerializeField] private TextMeshProUGUI _labelMax;
		[SerializeField] private Button          _buttonUpgrade;
		[SerializeField] private Button          _buttonHint;
		[SerializeField] private Button          _buttonUnlock;

		[SerializeField] private Color  _priceContainerColor;
		[SerializeField] private Color  _priceContainerEmptyColor;
		[SerializeField] private Color  _activeBackColor;
		[SerializeField] private Color  _activeBackPressedColor;
		[SerializeField] private Color  _disbaledBackColor;
		[SerializeField] private Color  _maxBackColor;
		[SerializeField] private Color  _defaultTextColor;
		[SerializeField] private Color  _lockedTextColor;
		[SerializeField] private Sprite _defaultBackSp;
		[SerializeField] private Sprite _unlockBackSp;
		[SerializeField] private Sprite _priceBackSpMenu;
		[SerializeField] private Sprite _priceBackSpMenuPressed;
		[SerializeField] private Sprite _disbaledPriceBackSp;

		[SerializeField] private GameObject      _redCrystalPriceObj;
		[SerializeField] private GameObject      _greenCrystalPriceObj;
		[SerializeField] private TextMeshProUGUI _redCrystalPrice;
		[SerializeField] private TextMeshProUGUI _greenCrystalPrice;
		
		[SerializeField] private RectTransform _priceLayout;

		public Action<PlayerStatType, PlayerStatConfig> OnUpgradeAction;
		public GameBoard                                GameBoard     { private get; set; }
		public UpgradeScreen                            UpgradeScreen { private get; set; }
		public PlayerStatType                           Type          => _statConfig.Type;
		public Button                                   ButtonUpgrade => _buttonUpgrade;

		private IPlayer            _player;
		private PlayerStatConfig   _statConfig;
		private StatInfo           _statInfo;
		private PlayerCurrencyType _currencyType;
		private int                _upgradePrice;
		private int                _upgradeRedCrystalPrice;
		private int                _upgradeGreenCrystalPrice;
		private Vector3            _buttonPricePos;
		private bool               _isPressed = false;

		private void Awake() {
			_buttonUpgrade.onClick.AddListener(OnUpgradeClick);
			_buttonHint.onClick.AddListener(OnHintClick);
			_buttonUnlock.onClick.AddListener(OnUnlockClick);
		}

		private void OnHintClick() {
			var settings = new StatHintWS {
				StatInfo   = _statInfo,
				StatConfig = _statConfig
			};

			GameController.Instance.WindowsController.Show(WindowType.StatHintWindow, settings);
		}

		private bool IsMaxValue() {
			if (_statConfig.MaxValue == 0)
				return false;

			return _statInfo._value >= _statConfig.MaxValue;
		}

		private void OnUnlockClick() {
			if (!IsHaveCurrency())
				return;

			ChangeCurrency();

			_player.UnlockStat(_statInfo._type);
			_statInfo._unlocked = true;

			UpdateView();
		}

		private void OnUpgradeClick() {
			if (IsMaxValue())
				return;

			if (!IsHaveCurrency())
				return;

			ClickAnimation();

			var needNotEnough = false;
			if (_currencyType == PlayerCurrencyType.Coins) {
				needNotEnough = !_player.IsHaveCurrency(PlayerCurrencyType.RedCrystal, _upgradeRedCrystalPrice) ||
				                !_player.IsHaveCurrency(PlayerCurrencyType.GreenCrystal, _upgradeGreenCrystalPrice);
			}

			if (needNotEnough)
				ShowNotEnoughWindow();
			else
				UpgradeStat();
		}

		private void UpgradeStat() {
			ChangeCurrency();
			OnUpgradeAction?.Invoke(_statInfo._type, _statConfig);
		}

		private void ShowNotEnoughWindow() {
			var redCrystalsDelta = _upgradeRedCrystalPrice - _player.Currency.RedCrystal;
			if (redCrystalsDelta < 0)
				redCrystalsDelta = 0;

			var greenCrystalsDelta = _upgradeGreenCrystalPrice - _player.Currency.GreenCrystal;
			if (greenCrystalsDelta < 0)
				greenCrystalsDelta = 0;

			var settings = new NotEnoughCrystalWS {
				RedCrystalsCount   = redCrystalsDelta,
				GreenCrystalsCount = greenCrystalsDelta,
				OnBuyClick         = () => UpgradeScreen.UpdateStatsView(),
			};

			GameController.Instance.WindowsController.Show(WindowType.NotEnoughCrystalWindow, settings);
		}

		private void ClickAnimation() {
			_priceBack.sprite = _priceBackSpMenuPressed;
			_isPressed        = true;
			LeanTween.delayedCall(0.1f, () => {
				_priceBack.sprite = _priceBackSpMenu;
				_isPressed        = false;
				UpdateView();
			});
		}

		private bool IsHaveCurrency() {
			if (CheaterController.NeedFreeUpgrades)
				return true;

			if (!_statInfo._unlocked)
				return _player.IsHaveCoins(_statConfig.UnlockPrice);

			if (_currencyType == PlayerCurrencyType.Coins) {
				var redCrystalsDelta = _upgradeRedCrystalPrice - _player.Currency.RedCrystal;
				if (redCrystalsDelta < 0)
					redCrystalsDelta = 0;

				var greenCrystalsDelta = _upgradeGreenCrystalPrice - _player.Currency.GreenCrystal;
				if (greenCrystalsDelta < 0)
					greenCrystalsDelta = 0;

				var neededPriceWithCrystals = _upgradePrice +
				                              redCrystalsDelta * GameController.Instance.Config.OneCrystalCoinsPrice +
				                              greenCrystalsDelta * GameController.Instance.Config.OneCrystalCoinsPrice;

				return _player.IsHaveCoins(neededPriceWithCrystals);
			}
			else {
				return GameBoard.IsHaveExp(_upgradePrice);
			}
		}

		private void ChangeCurrency() {
			if (CheaterController.NeedFreeUpgrades)
				return;

			if (!_statInfo._unlocked)
				_player.ChangeCoinsCount(-_statConfig.UnlockPrice);
			else if (_currencyType == PlayerCurrencyType.Coins) {
				_player.ChangeCoinsCount(-_upgradePrice);
				_player.ChangeCurrency(PlayerCurrencyType.RedCrystal, -_upgradeRedCrystalPrice);
				_player.ChangeCurrency(PlayerCurrencyType.GreenCrystal, -_upgradeGreenCrystalPrice);
			}
			else {
				GameBoard.ChangeExp(-_upgradePrice);
			}
		}

		public void Initialize(StatInfo info, PlayerCurrencyType currencyType) {
			_player         = GameController.Instance.Player;
			_statInfo       = info;
			_statConfig     = _player.Config.GetStatInfoByType(_statInfo._type);
			_currencyType   = currencyType;
			_buttonPricePos = _priceBack.transform.localPosition;

			UpdateView();
		}

		public void UpdateView() {
			_upgradePrice = _statConfig.GetUpgradePrice(_statInfo._levelUp);

			if (IsNewStats()) {
				_upgradeRedCrystalPrice   = GameController.Instance.Config.GetCrystalPriceForNewStats(_statInfo._levelUp + 1);
				_upgradeGreenCrystalPrice = GameController.Instance.Config.GetCrystalPriceForNewStats(_statInfo._levelUp + 1);
			}
			else {
				_upgradeRedCrystalPrice   = GameController.Instance.Config.GetCrystalPrice(PlayerCurrencyType.RedCrystal, _statInfo._levelUp + 1);
				_upgradeGreenCrystalPrice = GameController.Instance.Config.GetCrystalPrice(PlayerCurrencyType.GreenCrystal, _statInfo._levelUp + 1);
			}

			UpdateBackAndColors();
			SetCurrencyIcon();
			SetLabels();
			UpdateButton();
			UpdateCrystalPrice();
		}

		private bool IsNewStats() {
			if (_statInfo._type == PlayerStatType.SuperCritChance ||
			    _statInfo._type == PlayerStatType.SuperCritMult ||
			    _statInfo._type == PlayerStatType.AbsoluteResist) {
				return true;
			}

			return false;
		}

		private void UpdateBackAndColors() {
			var textColor = _lockedTextColor;
			_priceBack.transform.localPosition = _buttonPricePos;
			_back.sprite                       = _defaultBackSp;
			_labelMax.gameObject.SetActive(false);

			if (IsMaxValue()) {
				_back.color       = _disbaledBackColor;
				_priceBack.sprite = _disbaledPriceBackSp;

				_labelMax.gameObject.SetActive(true);
				_labelPrice.gameObject.SetActive(false);
				_currencyIcon.gameObject.SetActive(false);
			}
			else if (!_statInfo._unlocked) {
				_back.sprite                       = _unlockBackSp;
				_priceBack.sprite                  = _isPressed ? _priceBackSpMenuPressed : _priceBackSpMenu;
				_priceBack.transform.localPosition = Vector3.zero;
			}
			else if (IsHaveCurrency()) {
				_back.color       = _activeBackColor;
				_priceBack.sprite = _isPressed ? _priceBackSpMenuPressed : _priceBackSpMenu;
				textColor         = _defaultTextColor;
			}
			else {
				_back.color       = _disbaledBackColor;
				_priceBack.sprite = _disbaledPriceBackSp;
			}

			_labelName.color  = textColor;
			_labelValue.color = textColor;
		}

		private void UpdateCrystalPrice() {
			if (_redCrystalPriceObj == null || _greenCrystalPriceObj == null)
				return;
			
			var needRedCrystalPrice   = !IsMaxValue() && _statInfo._unlocked && _upgradeRedCrystalPrice > 0;
			var needGreenCrystalPrice = !IsMaxValue() && _statInfo._unlocked && _upgradeGreenCrystalPrice > 0;

			_redCrystalPriceObj.SetActive(needRedCrystalPrice);
			_greenCrystalPriceObj.SetActive(needGreenCrystalPrice);

			if (needRedCrystalPrice)
				_redCrystalPrice.text = FormatNumHelper.GetNumStr(_upgradeRedCrystalPrice);

			if (needGreenCrystalPrice)
				_greenCrystalPrice.text = FormatNumHelper.GetNumStr(_upgradeGreenCrystalPrice);
			
			LayoutRebuilder.ForceRebuildLayoutImmediate(_priceLayout);
		}

		private void UpdateButton() {
			_buttonUnlock.gameObject.SetActive(!_statInfo._unlocked);
			_buttonUpgrade.gameObject.SetActive(_statInfo._unlocked);

			if (_statInfo._unlocked) {
				_buttonUpgrade.interactable = !IsMaxValue() && IsHaveCurrency();

				if (_currencyType == PlayerCurrencyType.Exp) {
					var btnUpgradeRt = _buttonUpgrade.GetComponent<RectTransform>();
					btnUpgradeRt.anchoredPosition = Vector2.zero;
					btnUpgradeRt.sizeDelta        = GetComponent<RectTransform>().sizeDelta;

					_buttonHint.gameObject.SetActive(false);
				}
			}
		}

		private void SetCurrencyIcon() {
			var neededCurrency = _currencyType;
			if (!_statInfo._unlocked)
				neededCurrency = PlayerCurrencyType.Coins;

			var currInfo = GameController.Instance.DB.CurrencyDB.GetCurrencyInfo(neededCurrency);
			if (currInfo == null)
				return;

			if (IsHaveCurrency())
				_currencyIcon.sprite = currInfo.Icon;
			else
				_currencyIcon.sprite = currInfo.DisableIcon;
		}

		private void SetLabels() {
			var isUnlocked = _statInfo._unlocked;
			var roundVal   = Math.Round(_statInfo._value, 3);

			_labelName.text  = GameController.Instance.GetGameText($"{_statInfo._type}_name");
			_labelValue.text = isUnlocked ? Utility.GetStringForStats(_statInfo._type, (float) roundVal) : "Unlock";

			var upgradePriceStr = FormatNumHelper.GetNumStr(_upgradePrice);
			var unlockPriceStr  = FormatNumHelper.GetNumStr(_statConfig.UnlockPrice);
			_labelPrice.text = isUnlocked ? upgradePriceStr : unlockPriceStr;
		}
	}
}
