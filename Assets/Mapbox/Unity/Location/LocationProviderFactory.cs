#if !UNITY_EDITOR
#define NOT_UNITY_EDITOR
#endif

namespace Mapbox.Unity.Location {

	using UnityEngine;
	using Map;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Singleton factory to allow easy access to various LocationProviders.
	/// This is meant to be attached to a game object.
	/// </summary>
	public class LocationProviderFactory : MonoBehaviour {

		[SerializeField]
		public AbstractMap mapManager;

		[SerializeField]
		[Tooltip("Provider using Unity's builtin 'Input.Location' service")]
		private AbstractLocationProvider _deviceLocationProviderUnity;

		[SerializeField]
		[Tooltip("Custom native Android location provider. If this is not set above provider is used")]
		private DeviceLocationProviderAndroidNative _deviceLocationProviderAndroid;

		[SerializeField] private AbstractLocationProvider _editorLocationProvider;

		[SerializeField] private AbstractLocationProvider _transformLocationProvider;

		[SerializeField] private bool _dontDestroyOnLoad;

		/// <summary>
		/// The singleton instance of this factory.
		/// </summary>
		private static LocationProviderFactory _instance;

		public static LocationProviderFactory Instance
		{
			get => _instance;

			private set => _instance = value;
		}

		private ILocationProvider _defaultLocationProvider;

		/// <summary>
		/// The default location provider. 
		/// Outside of the editor, this will be a <see cref="T:Mapbox.Unity.Location.DeviceLocationProvider"/>.
		/// In the Unity editor, this will be an <see cref="T:Mapbox.Unity.Location.EditorLocationProvider"/>
		/// </summary>
		/// <example>
		/// Fetch location to set a transform's position:
		/// <code>
		/// void Update()
		/// {
		///     var locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
		///     transform.position = Conversions.GeoToWorldPosition(locationProvider.Location,
		///                                                         MapController.ReferenceTileRect.Center,
		///                                                         MapController.WorldScaleFactor).ToVector3xz();
		/// }
		/// </code>
		/// </example>
		public ILocationProvider DefaultLocationProvider
		{
			get => _defaultLocationProvider;
			set => _defaultLocationProvider = value;
		}

		/// <summary>
		/// Returns the serialized <see cref="T:Mapbox.Unity.Location.TransformLocationProvider"/>.
		/// </summary>
		public ILocationProvider TransformLocationProvider => _transformLocationProvider;

		/// <summary>
		/// Returns the serialized <see cref="T:Mapbox.Unity.Location.EditorLocationProvider"/>.
		/// </summary>
		public ILocationProvider EditorLocationProvider => _editorLocationProvider;

		/// <summary>
		/// Returns the serialized <see cref="T:Mapbox.Unity.Location.DeviceLocationProvider"/>
		/// </summary>
		public ILocationProvider DeviceLocationProvider => _deviceLocationProviderUnity;

		/// <summary>
		/// Create singleton instance and inject the DefaultLocationProvider upon initialization of this component. 
		/// </summary>
		protected virtual void Awake() {
			if (Instance != null) {
				DestroyImmediate(gameObject);
				return;
			}

			Instance = this;

			if (_dontDestroyOnLoad) {
				DontDestroyOnLoad(gameObject);
			}

			InjectEditorLocationProvider();
			InjectDeviceLocationProvider();
		}

		/// <summary>
		/// Injects the editor location provider.
		/// Depending on the platform, this method and calls to it will be stripped during compile.
		/// </summary>
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void InjectEditorLocationProvider() {
			DefaultLocationProvider = _editorLocationProvider;
		}

		/// <summary>
		/// Injects the device location provider.
		/// Depending on the platform, this method and calls to it will be stripped during compile.
		/// </summary>
		[System.Diagnostics.Conditional("NOT_UNITY_EDITOR")]
		private void InjectDeviceLocationProvider() {
			int AndroidApiVersion = 0;
			Regex regex = new Regex(@"(?<=API-)-?\d+");
			Match match =
				regex.Match(SystemInfo.operatingSystem); // eg 'Android OS 8.1.0 / API-27 (OPM2.171019.029/4657601)'

			if (match.Success) {
				int.TryParse(match.Groups[0].Value, out AndroidApiVersion);
			}

			Debug.LogFormat("{0} => API version: {1}", SystemInfo.operatingSystem, AndroidApiVersion);

			// only inject native provider if platform requirement is met
			// and script itself as well as parent game object are active
			if (Application.platform == RuntimePlatform.Android
			    && null != _deviceLocationProviderAndroid
			    && _deviceLocationProviderAndroid.enabled
			    && _deviceLocationProviderAndroid.transform.gameObject.activeInHierarchy
			    // API version 24 => Android 7 (Nougat): we are using GnssStatus 'https://developer.android.com/reference/android/location/GnssStatus.html'
			    // in the native plugin.
			    // GnssStatus is not available with versions lower than 24
			    && AndroidApiVersion >= 24
			   ) {
				DefaultLocationProvider = _deviceLocationProviderAndroid;
			}
			else {
				DefaultLocationProvider = _deviceLocationProviderUnity;
			}
		}

	}

}