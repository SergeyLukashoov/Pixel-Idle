using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Config.Tutorial {
	[Serializable]
	public class TutorialInfo {
		public TutorialId             TutorialId;
		public List<TutorialStepInfo> TutorialStepInfo;
	}

	[Serializable]
	public class TutorialStepInfo {
		public TutorialStepId TutorialStepId;
		public bool           NeedFade;
		public bool           NeedRebuild       = true;
		public bool           NeedDynamicPos    = false;
		public bool           NeedHandCustomPos = false;
		public bool           SkipByClick       = false;
		public bool           NeedHideHand      = false;
		public string         InfoTextId;
		public string         TargetId;
		public Vector2        InfoTextPos;
		public Vector2        TargetSizeShift = new Vector2(80, 80);
		public Vector2        TargetPosShift  = new Vector2(0, 0);
		public Vector2        CustomHandShift = new Vector2(0, 0);
	}

	[Serializable, CreateAssetMenu(fileName = "TutorialDB", menuName = "DB/Create TutorialDB", order = 70)]
	public class TutorialDB : ScriptableObject {
		[SerializeField] private List<TutorialInfo> _tutorialInfos = new List<TutorialInfo>();

		public TutorialInfo GetTutorialById(TutorialId id) {
			return _tutorialInfos.FirstOrDefault(x => x.TutorialId == id);
		}
	}
}
