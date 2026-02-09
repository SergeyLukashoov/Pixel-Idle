using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Back {
	public class BackContainer : MonoBehaviour {
		[SerializeField] private Transform _root;
		[SerializeField] private Image     _backImage;

		[SerializeField] private List<Color> _terrainsColorList = new List<Color>();

		private List<Transform> _backObjects = new List<Transform>();

		public Transform BackRoot => _root;

		public void BuildBack(int terrainId) {
			var terrainColor = _terrainsColorList[terrainId - 1];
			_backImage.color = terrainColor;

			var stepX = 1080f;
			var stepY = 1920f;

			_backImage.GetComponent<RectTransform>().sizeDelta = new Vector2(stepX * 4, stepY * 4);

			var leftCornerPos = Vector3.zero;
			leftCornerPos.x =  -stepX * 2;
			leftCornerPos.y =  stepY * 2;
			leftCornerPos.y -= 354;

			var terrainNameStr = $"Back/Prefabs/Terrain_{terrainId}/";
			var backPartCenter = Resources.Load<GameObject>(terrainNameStr + "ObjectsPart_center");

			for (var i = 0; i < 5; ++i) {
				for (var j = 0; j < 5; ++j) {
					var pos = leftCornerPos;
					pos.x += stepX * i;
					pos.y -= stepY * j;

					if (i == 2 && j == 2) {
						var backObjects = Instantiate(backPartCenter, _root);
						backObjects.transform.localScale    = Vector3.one;
						backObjects.transform.localPosition = pos;

						_backObjects.Add(backObjects.transform);
					}
					else {
						var randPartId     = Random.Range(1, 5);
						var backPartPrefab = Resources.Load<GameObject>(terrainNameStr + $"ObjectsPart_{randPartId}");
						var backObjects    = Instantiate(backPartPrefab, _root);
						backObjects.transform.localScale    = Vector3.one;
						backObjects.transform.localPosition = pos;

						_backObjects.Add(backObjects.transform);
					}
				}
			}
		}

		public void CheckObjectsVisible() {
			for (var i = 0; i < _backObjects.Count; ++i)
				_backObjects[i].GetComponent<BackPartsContainer>().CheckPartVisible();
		}
	}
}
