using System.Collections;
using System.Collections.Generic;
using Core.Config.Wave;
using Game.Boards;
using UnityEngine;

namespace Game.Wave {
	public interface ISpawnWaveController {
		public int  WaveIdx              { get; }
		public int  WaveIdxForDraw       { get; }
		public int  WaveTime             { get; }
		public int  WaveProgress         { get; }
		public bool IsWaveEnd            { get; }
		public bool NextSpawnAfterDefeat { get; }
		public void Initialize(WaveDB waveDB, GameBoard board);
		public void StartSpawn();
		public void Update();
		public void CheckNeedDelayState();
		public float GetCurrentWaveProgress();
	}
}
