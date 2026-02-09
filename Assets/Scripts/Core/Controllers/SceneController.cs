using UnityEngine.SceneManagement;

namespace Core.Controllers {
	internal static class SceneName {
		public static string Loader = "Loader";
		public static string Menu   = "Menu";
		public static string Game   = "Game";
	}

	public static class SceneController {
		public static void LoadMenu() {
			GameController.Instance.BlockInput(true);
			GameController.Instance.GlobalFade.StartFadeToAlpha(1f,
				() => {
					GameController.Instance.BlockInput(false);
					SceneManager.LoadScene(SceneName.Menu);
				});
		}

		public static void LoadGame() {
			GameController.Instance.BlockInput(true);
			GameController.Instance.GlobalFade.StartFadeToAlpha(1f,
				() => {
					GameController.Instance.BlockInput(false);
					SceneManager.LoadScene(SceneName.Game);
				});
		}
	}
}
