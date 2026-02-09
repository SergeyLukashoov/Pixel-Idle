using Core.Controllers;
using Core.Utils;
using Game.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui {
	public class RewardObject : MonoBehaviour {
		[SerializeField] private Image           _icon;
		[SerializeField] private TextMeshProUGUI _count;
		[SerializeField] private GameObject      _adsRewardRoot;
		[SerializeField] private TextMeshProUGUI _adsRewardCount;

		public void Initialize(PlayerCurrencyType type, int count, bool needAdsReward = false) {
			var info = GameController.Instance.DB.CurrencyDB.GetCurrencyInfo(type);
			if (info != null) {
				if (info.IconBig)
					_icon.sprite = info.IconBig;
				else
					_icon.sprite = info.Icon;
			}

			if (_adsRewardRoot)
				_adsRewardRoot.SetActive(needAdsReward);

			SetCountStr(count);
		}

		private void SetCountStr(int count) {
			_count.text = FormatNumHelper.GetNumStr(count);
		}

		public void SetAdsReward(int count) {
			_adsRewardCount.text = FormatNumHelper.GetNumStr(count);
		}
	}
}
