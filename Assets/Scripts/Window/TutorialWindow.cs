using System;
using Core.Config.Tutorial;
using Core.Controllers;
using Core.Controllers.Tutorial;
using Core.Controllers.Windows;
using Game.Map;
using TMPro;
using Ui.Screens.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace Window {
	public class TutorialWindow : BaseWindow<TutorialWS> {
		public override WindowType Type => WindowType.TutorialWindow;

		[SerializeField] private CanvasGroup _fadeCG;
		[SerializeField] private CanvasGroup _infoCG;
		[SerializeField] private CanvasGroup _handCG;

		[SerializeField] private GameObject _targetPrefab;
		[SerializeField] private GameObject _fadePrefab;

		[SerializeField] private GameObject      _tutorialInfoRoot;
		[SerializeField] private GameObject      _tutorialHand;
		[SerializeField] private TextMeshProUGUI _tutorialInfoLabel;

		[SerializeField] private Transform _fadeRoot;

		private TutorialController _tutorialController;
		private TutorialStepInfo   _currentTutorialStepInfo;

		private GameObject _target;
		private GameObject _clickTarget;

		private LTDescr _updateStepTween;

		private bool _onUpdateTarget = false;

		private void Awake() {
			_tutorialController = GameController.Instance.TutorialController;
		}

		public override void BeforeShow() {
			GameController.Instance.BlockInput(false);
			
			StartCurrentStep();
		}

		private void StartCurrentStep() {
			_onUpdateTarget = true;
				
			var currentStepId = _tutorialController.StepIdx;
			_currentTutorialStepInfo = _settings.Info.TutorialStepInfo[currentStepId];

			if (_currentTutorialStepInfo.NeedRebuild) {
				_fadeCG.alpha = 0;
				_infoCG.alpha = 0;
				_handCG.alpha = 0;

				_updateStepTween = LeanTween.delayedCall(0.05f, UpdateCurrentStep);
			}
			else {
				_onUpdateTarget = false;
				InitTextInfo();
			}
		}

		private void UpdateCurrentStep() {
			_updateStepTween = null;

			_target = _tutorialController.OnFindTarget.Invoke();
			if (_tutorialController.OnFindClickTarget == null)
				_clickTarget = _target;
			else
				_clickTarget = _tutorialController.OnFindClickTarget.Invoke();

			if (_currentTutorialStepInfo.NeedFade)
				ShowFade();

			InitTextInfo();
			InitHand();

			_onUpdateTarget = false;
		}

		private void InitHand() {
			_handCG.alpha = 0f;
			if (_currentTutorialStepInfo.NeedHideHand)
				return;

			_tutorialHand.transform.localScale = Vector3.one;

			var targetRt  = _target.GetComponent<RectTransform>();
			var targetPos = targetRt.TransformPoint(targetRt.rect.center);

			if (_currentTutorialStepInfo.NeedHandCustomPos) {
				targetPos.x += _currentTutorialStepInfo.CustomHandShift.x;
				targetPos.y += _currentTutorialStepInfo.CustomHandShift.y;

				if (_currentTutorialStepInfo.CustomHandShift.y < 0) {
					var handScale = _tutorialHand.transform.localScale;
					handScale.y                        *= -1;
					_tutorialHand.transform.localScale =  handScale;
				}
			}
			else {
				targetPos.x -= 50;
				targetPos.y += 260;
			}

			_tutorialHand.transform.position = targetPos;
			LeanTween.alphaCanvas(_handCG, 1f, 0.5f);
		}

		private void InitTextInfo() {
			var infoPos = _currentTutorialStepInfo.InfoTextPos;
			if (_currentTutorialStepInfo.NeedDynamicPos) {
				var targetPos = _target.transform.position;
				if ((targetPos.y > 0 && infoPos.y > 0) || (targetPos.y < 0 && infoPos.y < 0))
					infoPos.y *= -1;
			}

			_tutorialInfoRoot.transform.position = infoPos;
			_tutorialInfoLabel.text              = GameController.Instance.GetGameText(_currentTutorialStepInfo.InfoTextId);

			if (_currentTutorialStepInfo.NeedRebuild)
				LeanTween.alphaCanvas(_infoCG, 1f, 0.5f);
		}

		private void ShowFade() {
			if (_target) {
				ClearFade();
				BuildFadeWithTarget();
				LeanTween.alphaCanvas(_fadeCG, 1f, 0.2f).setDelay(0.2f);
			}
		}

		private void ClearFade() {
			for (var i = 0; i < _fadeRoot.childCount; ++i)
				Destroy(_fadeRoot.GetChild(i).gameObject);
		}

		private void BuildFadeWithTarget() {
			var color      = new Color(95 / 255f, 107 / 255f, 117 / 255f, 0.9f);
			var mainCanvas = GameController.Instance.Board.MainCanvasRT;

			var targetRt  = _target.GetComponent<RectTransform>();
			var target    = Instantiate(_targetPrefab, _fadeRoot);
			var targetPos = targetRt.TransformPoint(targetRt.rect.center);
			targetPos.x               += _currentTutorialStepInfo.TargetPosShift.x;
			targetPos.y               += _currentTutorialStepInfo.TargetPosShift.y;
			target.transform.position =  targetPos;

			var targetSize = new Vector2(targetRt.rect.width, targetRt.rect.height);
			targetSize.x                                   += _currentTutorialStepInfo.TargetSizeShift.x;
			targetSize.y                                   += _currentTutorialStepInfo.TargetSizeShift.y;
			target.GetComponent<RectTransform>().sizeDelta =  targetSize;

			//Build left part
			var leftPart = Instantiate(_fadePrefab, _fadeRoot);
			leftPart.GetComponent<Image>().color = color;

			var leftPartRt = leftPart.GetComponent<RectTransform>();
			leftPartRt.offsetMin = new Vector2(0, 0);
			leftPartRt.offsetMax = new Vector2(-mainCanvas.sizeDelta.x / 2f + target.transform.localPosition.x - targetSize.x / 2f, 0);

			//Build right part
			var rightPart = Instantiate(_fadePrefab, _fadeRoot);
			rightPart.GetComponent<Image>().color = color;

			var rightPartRt = rightPart.GetComponent<RectTransform>();
			rightPartRt.offsetMin = new Vector2(mainCanvas.sizeDelta.x / 2f + target.transform.localPosition.x + targetSize.x / 2f, 0);
			rightPartRt.offsetMax = new Vector2(0, 0);

			//Build top part
			var topPart = Instantiate(_fadePrefab, _fadeRoot);
			topPart.GetComponent<Image>().color = color;

			var topPartRt = topPart.GetComponent<RectTransform>();
			topPartRt.offsetMin = new Vector2(mainCanvas.sizeDelta.x / 2f + target.transform.localPosition.x - targetSize.x / 2f,
				mainCanvas.sizeDelta.y / 2f + target.transform.localPosition.y + targetSize.y / 2f);

			topPartRt.offsetMax = new Vector2(-mainCanvas.sizeDelta.x / 2f + target.transform.localPosition.x + targetSize.x / 2f, 0);

			//Build bottom part
			var bottomPart = Instantiate(_fadePrefab, _fadeRoot);
			bottomPart.GetComponent<Image>().color = color;

			var bottomPartRt = bottomPart.GetComponent<RectTransform>();
			bottomPartRt.offsetMin = new Vector2(mainCanvas.sizeDelta.x / 2f + target.transform.localPosition.x - targetSize.x / 2f, 0);
			bottomPartRt.offsetMax = new Vector2(-mainCanvas.sizeDelta.x / 2f + target.transform.localPosition.x + targetSize.x / 2f,
				-mainCanvas.sizeDelta.y / 2f + target.transform.localPosition.y - targetSize.y / 2f);
		}

		private void Update() {
			CheckClickOnTarget();
		}

		private void CheckClickOnTarget() {
			if (OnShow || _onUpdateTarget)
				return;

			if (Input.GetMouseButtonDown(0) && IsPointIn()) {
				if (_currentTutorialStepInfo.SkipByClick) {
					ChangeStep();
					return;
				}

				if (_clickTarget) {
					var button = _clickTarget.GetComponent<Button>();
					if (button)
						button.onClick?.Invoke();
					else
						ClickOnCustomTarget();
				}

				ChangeStep();
			}
		}

		private void ChangeStep() {
			if (_updateStepTween != null) {
				LeanTween.cancel(_updateStepTween.uniqueId);
				_updateStepTween = null;
			}

			GameController.Instance.TutorialController.ChangeStep();
			if (_tutorialController.StepIdx <= _settings.Info.TutorialStepInfo.Count - 1)
				StartCurrentStep();
		}

		private void ClickOnCustomTarget() {
			if (_currentTutorialStepInfo.TargetId == "Tower1" || _currentTutorialStepInfo.TargetId == "Tower2") {
				_clickTarget.GetComponent<TowerObject>().OnTowerClick();
			}
			else if (_currentTutorialStepInfo.TargetId == "FirstActiveSlot") {
				_clickTarget.GetComponent<CardSlotView>().OnUnlockClick();
			}
			else if (_currentTutorialStepInfo.TargetId == "FirstCollectionSlot") {
				_clickTarget.GetComponent<CardSlotView>().OnApplayClick();
			}
			else if (_currentTutorialStepInfo.TargetId == "Chest") {
				_clickTarget.GetComponent<MapChestObject>().CollectChest();
			}
			else if (_currentTutorialStepInfo.TargetId == "MineInRange") {
				_clickTarget.GetComponent<MineObject>().OpenMineWindow(false);
			}
		}

		private bool IsPointIn() {
			if (!_clickTarget || _currentTutorialStepInfo.SkipByClick)
				return true;

			var mousePos   = Input.mousePosition;
			var targetPos  = Camera.main.WorldToScreenPoint(_clickTarget.transform.position);
			var targetSize = _clickTarget.GetComponent<RectTransform>().sizeDelta;
			targetSize *= _clickTarget.transform.lossyScale;

			if (mousePos.x > targetPos.x - targetSize.x / 2f && mousePos.x < targetPos.x + targetSize.x / 2f)
				if (mousePos.y > targetPos.y - targetSize.y / 2f && mousePos.y < targetPos.y + targetSize.y / 2f)
					return true;

			return false;
		}
	}

	public class TutorialWS : BaseWindowSettings {
		public TutorialInfo Info;
	}
}
