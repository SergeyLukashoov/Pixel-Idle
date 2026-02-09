using UnityEngine;

namespace Tools.Editor.Builder {
	[CreateAssetMenu(fileName = "BuildConfig", menuName = "Builder/Create Build Config")]
	public class BuilderConfig : ScriptableObject {
		[SerializeField] private string _globalVersion;
		[SerializeField] private int    _bundleVersion;

		[Header("Android Build")]
		[SerializeField] private bool _needBuildAppBundle;
		[SerializeField] private string _keystoreName;
		[SerializeField] private string _keystorePass;
		[SerializeField] private string _aliasName;
		[SerializeField] private string _aliasPass;

		[Header("iOS Build")]
		[SerializeField] private string _iosBuildVersion;
		[SerializeField] private int _iosBundleVersion;

		public string GlobalVersion      => _globalVersion;
		public int    BundleVersion      => _bundleVersion;
		public bool   NeedBuildAppBundle => _needBuildAppBundle;
		public string KeystoreName       => _keystoreName;
		public string KeystorePass       => _keystorePass;
		public string AliasName          => _aliasName;
		public string AliasPass          => _aliasPass;
		public string IosBuildVersion    => _iosBuildVersion;
		public int    IosBundleVersion   => _iosBundleVersion;

		public void IncBundleVersion() {
			_bundleVersion++;
		}
	}
}
