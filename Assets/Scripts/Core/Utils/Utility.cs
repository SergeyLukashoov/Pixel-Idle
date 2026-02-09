using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Core.Config;
using Core.Controllers;
using Game.Player;
using UnityEngine;

namespace Core.Utils {
	public static class Utility {
		private static readonly System.Random Random = new System.Random();

		public static List<string> GetDefaultDependencies(bool needCheater = false) {
			var depList = new List<string> {
				"HARDCORE",
				"APPSFLYER",
				"GAMEANALYTICS"
			};

			if (needCheater)
				depList.Add("CHEATER");
			else
				depList.Add("DEVTODEV");

			return depList;
		}

		public static float GetAnimLength(Animator anim, string animName) {
			if (anim) {
				var clips = anim.runtimeAnimatorController.animationClips;
				foreach (var ac in clips) {
					if (ac.name == animName)
						return ac.length;
				}
			}

			Debug.Log($"Animation {animName} not found!");
			return 0;
		}

		public static List<T> ShuffleList<T>(List<T> inputList) {
			var cloneList = new List<T>();
			foreach (var t in inputList)
				cloneList.Add(t);

			var randomList = new List<T>();

			while (cloneList.Count != 0) {
				var randomIndex = Random.Next(0, cloneList.Count);
				randomList.Add(cloneList[randomIndex]);
				cloneList.RemoveAt(randomIndex);
			}

			return randomList;
		}

		public static List<string> FixData(string dataStr) {
			dataStr = dataStr.Substring(1, dataStr.Length - 2);
			var splitData = Regex.Split(dataStr, "\",\"");
			var fixedData = new List<string>();

			for (var i = 0; i < splitData.Length; ++i) {
				if (string.IsNullOrEmpty(splitData[i]))
					continue;

				fixedData.Add(splitData[i]);
			}

			return fixedData;
		}

		public static void SetParticleSorting(GameObject obj, string layer, int order) {
			var renderer = obj.GetComponent<Renderer>();
			renderer.sortingLayerName = layer;
			renderer.sortingOrder     = order;

			for (var i = 0; i < obj.transform.childCount; ++i) {
				var child = obj.transform.GetChild(i);
				SetParticleSorting(child.gameObject, layer, order);
			}
		}

		public static void PlayPS(GameObject obj) {
			var ps = obj.GetComponent<ParticleSystem>();
			ps.Play();

			for (var i = 0; i < obj.transform.childCount; ++i) {
				var child = obj.transform.GetChild(i);
				PlayPS(child.gameObject);
			}
		}

		public static void SetParticlesScale(GameObject obj, float scale) {
			var ps   = obj.GetComponent<ParticleSystem>();
			var main = ps.main;
			main.startSizeMultiplier *= scale;

			for (var i = 0; i < obj.transform.childCount; ++i) {
				var child = obj.transform.GetChild(i);
				SetParticlesScale(child.gameObject, scale);
			}
		}

		public static void SetParticleLifetime(GameObject obj, float lifeTime) {
			var psMain = obj.GetComponent<ParticleSystem>().main;
			psMain.startLifetime = lifeTime;

			for (var i = 0; i < obj.transform.childCount; ++i) {
				var child = obj.transform.GetChild(i);
				SetParticleLifetime(child.gameObject, lifeTime);
			}
		}

		public static string GetStringForStats(PlayerStatType type, float val) {
			var str = string.Format(CultureInfo.InvariantCulture, "{0}", val);
			if (type == PlayerStatType.ShotsPerSec)
				str = string.Format(CultureInfo.InvariantCulture, "{0:0.00}/sec", val);
			else if (type == PlayerStatType.Range)
				str = string.Format(CultureInfo.InvariantCulture, "{0}m", val);
			else if (type == PlayerStatType.AddDmgPerRange)
				str = string.Format(CultureInfo.InvariantCulture, "{0}%/m", val);
			else if (type == PlayerStatType.CritChance || type == PlayerStatType.SuperCritChance)
				str = string.Format(CultureInfo.InvariantCulture, "{0}%", val);
			else if (type == PlayerStatType.CritMult || type == PlayerStatType.SuperCritMult)
				str = string.Format(CultureInfo.InvariantCulture, "x{0:0.00}", val);
			else if (type == PlayerStatType.HealthRegen)
				str = string.Format(CultureInfo.InvariantCulture, "{0}/sec", val);

			return str;
		}

		public static Vector3[] BuildFlyCurve(Vector3 startPoint, Vector3 endPoint, float curveAngle, float curveProcentsFromLength,
			float centerPointDivider = 2.0f) {
			Vector3 moveDir      = endPoint - startPoint;
			float   moveDistance = moveDir.magnitude;
			moveDir = moveDir.normalized;

			Vector3 centerPoint = startPoint + moveDir * moveDistance / centerPointDivider;
			moveDir     =  Quaternion.Euler(0, 0, curveAngle) * moveDir;
			centerPoint += moveDir * moveDistance * curveProcentsFromLength;

			Vector3[] positions = {startPoint, centerPoint, centerPoint, endPoint};
			return positions;
		}

		public static string GetTimeStr(int timeSec) {
			var hours   = timeSec / 60 / 60;
			var minutes = timeSec / 60 - hours * 60;
			var seconds = timeSec - minutes * 60 - hours * 60 * 60;

			return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
		}

		public static bool IsNextDay(long prevDayTimeStamp) {
			var prevShowTime = new DateTime(prevDayTimeStamp);
			var currentDay   = DateTime.Now;

			if (currentDay.Year > prevShowTime.Year)
				return true;

			if (currentDay.DayOfYear > prevShowTime.DayOfYear)
				return true;

			return false;
		}
	}
}
