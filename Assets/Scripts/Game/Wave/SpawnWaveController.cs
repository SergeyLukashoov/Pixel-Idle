using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Config.Wave;
using Core.Controllers;
using Core.Utils;
using Game.Boards;
using Game.Cards;
using Game.Enemy;
using Game.Player;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Game.Wave {
	public enum SpawnState {
		Pause = 1,
		Spawn = 2,
		Delay = 3,
	}

	public class SpawnWaveController : ISpawnWaveController {
		private SpawnState _state = SpawnState.Pause;
		private WaveDB     _waveDB;
		private WaveInfo   _currentWaveInfo;
		private GameBoard  _gameBoard;

		private List<EnemyType> _spawnEnemies = new List<EnemyType>();

		private int _waveIdxForDraw     = 0;
		private int _maxWaveIdx         = 39;
		private int _afterGameWaveIdx   = 0;
		private int _waveIdx            = 0;
		private int _enemiesCountInWave = 0;

		private float _timeToSpawn;
		private float _timeToNextWave;
		private float _fullWaveTime;

		private IPlayer  _player;
		private DateTime _startSpawnTime;

		public int  WaveIdxForDraw       => _waveIdxForDraw;
		public int  WaveIdx              => _waveIdx;
		public int  WaveTime             => (int) (DateTime.Now - _startSpawnTime).TotalSeconds;
		public int  WaveProgress         => (int) ((1f - _spawnEnemies.Count / (float) _enemiesCountInWave) * 100);
		public bool IsWaveEnd            => _spawnEnemies.Count == 0;
		public bool NextSpawnAfterDefeat => _currentWaveInfo._nextWaveAfterDefeat;

		public void Initialize(WaveDB waveDB, GameBoard board) {
			_player    = GameController.Instance.Player;
			_waveDB    = waveDB;
			_gameBoard = board;

			_gameBoard.SetStateLabel(_state);

			_maxWaveIdx = 29;
			if (GameController.Instance.PlayedTowerInfo.Id > 2)
				_maxWaveIdx = 39;

			_waveIdx = GameController.Instance.PlayedTowerInfo.StartWaveId;
			if (_waveIdx > _maxWaveIdx)
				_afterGameWaveIdx = 1;
		}

		private float CalcWaveTime() {
			var timeToSpawnEnemy = _currentWaveInfo._timeToSpawnEnemy;
			var time             = timeToSpawnEnemy * (_spawnEnemies.Count - 1);
			if (_state != SpawnState.Delay)
				time += _timeToSpawn;

			return time;
		}

		public float GetCurrentWaveProgress() {
			return CalcWaveTime() / _fullWaveTime;
		}

		public void StartSpawn() {
			_gameBoard.SendStageStartEvent();

			_state          = SpawnState.Spawn;
			_startSpawnTime = DateTime.Now;

			if (_player.Flags.NeedTutorialWave) {
				BuildSpecialWave();
			}
			else {
				BuildWave();
			}

			_fullWaveTime       = CalcWaveTime();
			_enemiesCountInWave = _currentWaveInfo._spawnEnemyInfos.Count;

			_gameBoard.SetStateLabel(_state);
			_gameBoard.UpdateEnemyBaseStatsLabels();
			_gameBoard.UpdateWaveIdLabel(_waveIdxForDraw);
		}

		private void BuildSpecialWave() {
			_spawnEnemies.Clear();

			_currentWaveInfo                      = new WaveInfo();
			_currentWaveInfo._spawnEnemyInfos     = new List<SpawnEnemyInfo>();
			_currentWaveInfo._nextWaveAfterDefeat = true;

			var enemyInfo = new SpawnEnemyInfo();
			enemyInfo._type  = EnemyType.Common;
			enemyInfo._count = 1;
			_currentWaveInfo._spawnEnemyInfos.Add(enemyInfo);

			_currentWaveInfo._timeToSpawnEnemy = 1;
			_currentWaveInfo._pauseToNextWave  = 1;

			for (var i = 0; i < _currentWaveInfo._spawnEnemyInfos.Count; ++i) {
				var info = _currentWaveInfo._spawnEnemyInfos[i];
				AddEnemies(info._type, info._count);
			}

			_timeToSpawn    = _currentWaveInfo._timeToSpawnEnemy;
			_timeToNextWave = _currentWaveInfo._pauseToNextWave;
		}

		private void BuildWave() {
			var waveIdx = _waveIdx;
			if (_waveIdx > _maxWaveIdx)
				waveIdx = _maxWaveIdx + _afterGameWaveIdx;

			_currentWaveInfo = _waveDB.GetWaveById(waveIdx);
			_spawnEnemies.Clear();

			var isHaveBoss     = false;
			var neededBossType = EnemyType.Boss;
			for (var i = 0; i < _currentWaveInfo._spawnEnemyInfos.Count; ++i) {
				var info = _currentWaveInfo._spawnEnemyInfos[i];
				if (info._type is EnemyType.Boss or EnemyType.BossSimple) {
					neededBossType = info._type;
					isHaveBoss     = true;
					continue;
				}

				AddEnemies(info._type, info._count);
			}

			_spawnEnemies = Utility.ShuffleList(_spawnEnemies);
			if (isHaveBoss)
				_spawnEnemies.Add(neededBossType);

			_timeToSpawn    = _currentWaveInfo._timeToSpawnEnemy;
			_timeToNextWave = _currentWaveInfo._pauseToNextWave;
		}

		private void AddEnemies(EnemyType type, int count) {
			for (var i = 0; i < count; ++i)
				_spawnEnemies.Add(type);
		}

		public void Update() {
			if (_state == SpawnState.Spawn)
				UpdateSpawnState();
			else if (_state == SpawnState.Delay)
				UpdateDelayState();
		}

		private void UpdateSpawnState() {
			_timeToSpawn -= MyTime.deltaTime;
			if (_timeToSpawn <= 0f) {
				_timeToSpawn = _currentWaveInfo._timeToSpawnEnemy;
				SpawnEnemy();
			}
		}

		public void SpawnEnemy() {
			if (_spawnEnemies.Count == 0)
				return;

			var enemyType = _spawnEnemies[0];
			_spawnEnemies.RemoveAt(0);

			var spawnPointId = GetSpawnPointId(enemyType);
			_gameBoard.SpawnEnemy(enemyType, spawnPointId, _waveIdx);

			if (_spawnEnemies.Count == 0 && !_currentWaveInfo._nextWaveAfterDefeat) {
				_gameBoard.AddCurrencyForWave();
				CheckNeedDelayState();
			}
		}

		private int GetSpawnPointId(EnemyType type) {
			//Проверяем, если конкретный тип врагов должкен спавниться в конкретной точке
			for (var i = 0; i < _currentWaveInfo._spawnPointInfos.Count; ++i) {
				if (_currentWaveInfo._spawnPointInfos[i]._type == type) {
					var randVal = Random.Range(0, _currentWaveInfo._spawnPointInfos[i]._pointId.Count);
					return _currentWaveInfo._spawnPointInfos[i]._pointId[randVal];
				}
			}

			//Берем точки спавна прописанные для волны
			if (_currentWaveInfo._spawnPointsIds.Count > 0) {
				var randIdx = Random.Range(0, _currentWaveInfo._spawnPointsIds.Count);
				return _currentWaveInfo._spawnPointsIds[randIdx];
			}

			//Если -1, то берем любую радомную точку
			return -1;
		}

		private void UpdateDelayState() {
			_timeToNextWave -= MyTime.deltaTime;
			if (_timeToNextWave <= 0) {
				StartNextWave();
			}
		}

		private void TryApplayWaveSkipCard() {
			var activeCard = _player.GetActiveCardByType(CardType.WaveSkip);
			if (activeCard != null) {
				if (IsBossWave())
					return;

				var cardConfig = GameController.Instance.DB.CardsDB.GetCardConfig(activeCard._type);
				var currLevel  = GameController.Instance.DB.CardsDB.GetCurrentLevelUp(activeCard._count);
				var currVal    = cardConfig.GetCardValueByLvlUp(currLevel - 1);

				var skipProb = new RandomProb();
				skipProb.AddValue("yes", currVal);
				skipProb.AddValue("no", 100 - currVal);

				var randVal = skipProb.GetRandomValue();
				if (randVal == "yes") {
					Debug.Log($"[SKIP WAVE]: {_waveIdxForDraw}");
					AddWaveReward();
					_gameBoard.PlayWaveSkipAnim();

					_waveIdx++;
					_waveIdxForDraw++;

					IncAfterGameWaveIdx();
				}
			}
		}

		private void AddWaveReward() {
			var expCount   = 0f;
			var coinsCount = 0f;

			var waveIdx = _waveIdx;
			if (_waveIdx > _maxWaveIdx)
				waveIdx = _maxWaveIdx + _afterGameWaveIdx;

			var waveInfo = _waveDB.GetWaveById(waveIdx);
			for (var i = 0; i < waveInfo._spawnEnemyInfos.Count; ++i) {
				var enemyInfo = _gameBoard.EnemyDB.GetEnemyByType(waveInfo._spawnEnemyInfos[i]._type);
				for (var j = 0; j < waveInfo._spawnEnemyInfos[i]._count; ++j) {
#if HARDCORE
					expCount   += enemyInfo.Exp + _waveIdx * enemyInfo.ExpAddByWave;
					coinsCount += enemyInfo.Coins + _waveIdx * enemyInfo.CoinsAddByWave;
#else
					expCount += enemyInfo.GetEnemyCoreCurrency(_waveIdx);
					coinsCount += enemyInfo.GetEnemyMetaCurrency(_waveIdx);
#endif
				}
			}

			_gameBoard.AddRewardForSkippedWave(coinsCount, expCount);
		}

		private bool IsBossWave() {
			var waveIdx = _waveIdx;
			if (_waveIdx > _maxWaveIdx)
				waveIdx = _maxWaveIdx + _afterGameWaveIdx;

			var waveInfo   = _waveDB.GetWaveById(waveIdx);
			var isHaveBoss = waveInfo._spawnEnemyInfos.FirstOrDefault(x => x._type == EnemyType.Boss);
			if (isHaveBoss == null)
				return false;

			return true;
		}

		private void StartNextWave() {
			_player.SetTowerMaxCompleteWave(GameController.Instance.PlayedTowerInfo.Id, _waveIdxForDraw + 1);

			_waveIdx++;
			_waveIdxForDraw++;
			
			if (_waveIdxForDraw >= 20 && !GameController.Instance.NeedShowRateUsWindow)
				GameController.Instance.NeedShowRateUsWindow = true;

			if (_waveIdxForDraw > _player.Flags.MaxCompleteWaveId) {
				_player.Flags.MaxCompleteWaveId = _waveIdxForDraw;
				_player.Flags.MaxPlayedWaveId   = _waveIdxForDraw + 1;
			}

			IncAfterGameWaveIdx();
			TryApplayWaveSkipCard();

			var waveIdx = _waveIdx;
			if (_waveIdx > _maxWaveIdx)
				waveIdx = _maxWaveIdx + _afterGameWaveIdx;

			if (_waveDB.IsHaveWave(waveIdx)) {
				StartSpawn();
			}
			else {
				_state = SpawnState.Pause;
				_gameBoard.SetStateLabel(_state);
			}
		}

		private void IncAfterGameWaveIdx() {
			if (_waveIdx > _maxWaveIdx) {
				_afterGameWaveIdx++;
				if (_afterGameWaveIdx > 10)
					_afterGameWaveIdx = 1;
			}
		}

		public void CheckNeedDelayState() {
			if (_spawnEnemies.Count > 0)
				return;

			_state = SpawnState.Delay;
			_gameBoard.SetStateLabel(_state);
			_gameBoard.SendStageFinishEvent("win", "null");
		}
	}
}
