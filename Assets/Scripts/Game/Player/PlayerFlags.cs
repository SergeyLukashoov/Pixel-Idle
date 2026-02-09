using System;
using System.Collections.Generic;
using System.Linq;
using Core.Controllers;
using UnityEngine;

namespace Game.Player {
	[Serializable]
	public class KilledBossInfo {
		public int TowerId;
		public int WaveId;
	}

	[Serializable]
	public class PlayerFlags {
		public bool                 NeedHideAbilityOnStart = true;
		public int                  MaxCompleteWaveId      = 0;
		public int                  MaxPlayedWaveId        = 0;
		public int                  PlayedCount            = 0;
		public float                GameAcceleration       = 1.5f;
		public List<int>            UnlockedCardSlotsId    = new List<int>();
		public List<KilledBossInfo> KilledBosses           = new List<KilledBossInfo>();
		public List<string>         CompleteTutorials      = new List<string>();
		public long                 MineSaveTime           = 0;
		public long                 GrandChestSaveTime     = 0;
		public List<string>         CollectedChests        = new List<string>();
		public bool                 NeedMusic              = true;
		public bool                 NeedSound              = true;
		public int                  GamePlayTime           = 0;
		public bool                 NeedTutorialWave       = true;
		public float                AdsAccelerationTime    = 0f;
		public List<string>         NonConsumableItems     = new List<string>();
		public int                  RateUsShowCount        = 0;
		public long                 RateUsShowTime         = 0;
		public bool                 IsWasRate              = false;

		public PlayerFlags() {
			NeedHideAbilityOnStart = true;
			MaxCompleteWaveId      = 0;
			MaxPlayedWaveId        = 0;
			PlayedCount            = 0;
		}

		public void SetBossKilled(KilledBossInfo info) {
			if (!IsBossKilled(info.TowerId, info.WaveId)) {
				KilledBosses.Add(info);
			}
		}

		public bool IsBossKilled(int towerId, int waveId) {
			var savedInfo = KilledBosses.FirstOrDefault(x => x.TowerId == towerId && x.WaveId == waveId);
			if (savedInfo == null)
				return false;

			return true;
		}

		public void UnlockSlot(int slotId) {
			if (UnlockedCardSlotsId.Contains(slotId)) {
				Debug.Log("Slot already unlocked!");
				return;
			}

			UnlockedCardSlotsId.Add(slotId);
		}

		public void SetTutorialComplete(string id) {
			if (!CompleteTutorials.Contains(id))
				CompleteTutorials.Add(id);

			GameController.Instance.Save();
		}

		public bool IsTutorialComplete(string id) {
			return CompleteTutorials.Contains(id);
		}
	}
}
