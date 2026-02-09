using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Core.Config.Store {
	[Serializable]
	public class ProductData {
		public ProductType _type;
		public string      _name;
		public string      _localizedPriceStr;

		public string SKU => _name.ToString();
	}

	[Serializable, CreateAssetMenu(fileName = "StoreDB", menuName = "DB/Create StoreDB", order = 61)]
	public class StoreDB : ScriptableObject {
		[SerializeField] private List<ProductData> _productsData = new List<ProductData>();

		public List<ProductData> AllProducts => _productsData;

		public ProductData GetProductBySKU(string sku) {
			return _productsData.FirstOrDefault(x => x.SKU == sku);
		}
	}
}
