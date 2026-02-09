using UnityEngine;

namespace Game {
	public class BackPartsContainer : MonoBehaviour {
		public void CheckPartVisible() {
			var screenWidth  = Screen.width;
			var screenHeight = Screen.height;

			for (var i = 0; i < transform.childCount; ++i) {
				var child = transform.GetChild(i);

				var childPos   = child.position;
				var childScale = child.lossyScale;
				var childSize  = child.GetComponent<RectTransform>().sizeDelta;
				childSize *= childScale;

				var leftUpCorner    = new Vector2(childPos.x - childSize.x / 2f, childPos.y + childSize.y);
				var rightUpPoint    = new Vector2(childPos.x + childSize.x / 2f, childPos.y + childSize.y);
				var leftDownCorner  = new Vector2(childPos.x - childSize.x / 2f, childPos.y);
				var rightDownCorner = new Vector2(childPos.x + childSize.x / 2f, childPos.y);

				if (IsPointInScreen(leftUpCorner, screenWidth, screenHeight) ||
				    IsPointInScreen(rightUpPoint, screenWidth, screenHeight) ||
				    IsPointInScreen(leftDownCorner, screenWidth, screenHeight) ||
				    IsPointInScreen(rightDownCorner, screenWidth, screenHeight)) {
					child.gameObject.SetActive(true);
				}
				else {
					child.gameObject.SetActive(false);
				}
			}
		}

		private bool IsPointInScreen(Vector2 point, int screenWidth, int screenHeight) {
			if (point.x > -screenWidth / 2f && point.x < screenWidth / 2f)
				if (point.y > -screenHeight / 2f && point.y < screenHeight / 2f)
					return true;

			return false;
		}
	}
}
