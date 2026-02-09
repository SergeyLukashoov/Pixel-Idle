using Core.Controllers;
using TMPro;
using UnityEngine;

namespace Core.Utils {
	public class TextInitializer : MonoBehaviour {
		[SerializeField] private string _textId;
		[SerializeField] private bool   _isUpper = false;
		private void Start() {
			var str = GameController.Instance.GetGameText(_textId);
			
			if (_isUpper)
				GetComponent<TextMeshProUGUI>().text = str.ToUpper();
			else
				GetComponent<TextMeshProUGUI>().text = str;
		}
	}
}
