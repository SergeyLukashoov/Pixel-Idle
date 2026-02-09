using Core.Controllers;
using TMPro;
using Tools.Editor.Builder;
using UnityEngine;

namespace Game.Boards {
	public class LoaderBoard : BaseBoard {
		[SerializeField] private GameController  _controller;
		[SerializeField] private float           _splashTime;
		[SerializeField] private TextMeshProUGUI _versionLabel;

		private bool _canLoad = true;

		private void Start() {
			_controller.GlobalFade.SetAlpha(0f);
			
			InitVersionLabel();
		}

		private void InitVersionLabel() {
			var buildConfig = Resources.Load<BuilderConfig>("Config/BuildConfig");
			if (!buildConfig) {
				Debug.LogError("Can't load build config!");
				return;
			}

#if UNITY_ANDROID			
			_versionLabel.text = $"ver. {buildConfig.GlobalVersion}.{buildConfig.BundleVersion - 1}";
#elif UNITY_IOS
			_versionLabel.text = $"ver. {buildConfig.IosBuildVersion}.{buildConfig.IosBundleVersion}";
#endif
		}
		
		private void Update() {
			if (!_controller.IsInitialized)
				return;

			_splashTime -= Time.deltaTime;
			if (_splashTime <= 0f && _canLoad) {
				_canLoad = false;
				_controller.GlobalFade.StartFadeToAlpha(1f, SceneController.LoadMenu);
			}
		}
	}
}
