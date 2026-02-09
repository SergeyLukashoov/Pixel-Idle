using TMPro;
using UnityEngine;

namespace Ui {
	public class WaveCompleteCount : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI _expCount;
		[SerializeField] private TextMeshProUGUI _coinsCount;

		public void Initialize(int expCount, int coinsCount) {
			_expCount.text   = $"+{expCount}";
			_coinsCount.text = $"+{coinsCount}";
			
			_expCount.transform.parent.gameObject.SetActive(expCount > 0);
			_coinsCount.transform.parent.gameObject.SetActive(coinsCount > 0);
		}
	}
}
