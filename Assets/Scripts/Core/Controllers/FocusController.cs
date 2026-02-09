using System;
using System.Collections.Generic;
using Game.Map;
using UnityEngine;

namespace Core.Controllers {
	public class FocusInfo {
		public Transform Object;
		public Action    OnExecuteAction;
	}

	public class FocusController {
		private List<FocusInfo> _queueList = new List<FocusInfo>();

		private MapObject _mapObject;
		private bool      _needUpdateQueue = false;
		private bool      _onExecuteAction = false;

		public FocusController(MapObject mapObject) {
			_mapObject = mapObject;
		}

		public void AddQueue(FocusInfo info) {
			_queueList.Add(info);
		}

		public void StartQueue() {
			_needUpdateQueue = true;
		}

		private void Execute() {
			if (_queueList.Count == 0)
				return;

			_onExecuteAction = true;

			var info = _queueList[0];
			_queueList.RemoveAt(0);

			var neededPos = _mapObject.transform.position;
			neededPos.x -= info.Object.position.x;
			neededPos.y -= info.Object.position.y;
			neededPos   =  _mapObject.ClampPos(neededPos);
			
			LeanTween.move(_mapObject.gameObject, neededPos, 0.5f).setOnComplete(
				() => {
					info.OnExecuteAction?.Invoke();
					_onExecuteAction = false;
				});

			if (_queueList.Count == 0)
				_needUpdateQueue = false;
		}

		public void UpdateQueue() {
			if (GameController.Instance.WindowsController.OpenedWindowCount > 0)
				return;

			if (!_needUpdateQueue)
				return;

			if (_onExecuteAction)
				return;

			Execute();
		}
	}
}
