using System.Globalization;
using UnityEngine;

namespace Core.Utils {
	public static class FormatNumHelper {
		private static string[] _postfix = new[] {"", "K", "M", "B"};

		public static string GetNumStr(float num) {
			if (num == 0) {
				return "0";
			}

			num = Mathf.Round(num);
			var i = 0;
			while (i + 1 < _postfix.Length && num >= 1000) {
				num /= 1000;
				i++;
			}

			return num.ToString(format: "#.##", CultureInfo.InvariantCulture) + _postfix[i];
		}
	}
}
