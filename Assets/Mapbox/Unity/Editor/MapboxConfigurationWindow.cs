namespace Mapbox.Editor {

	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor.SceneManagement;
	using UnityEditor;
	using System.IO;
	using System.Collections;
	using Unity;
	using Tokens;
	using Json;
	using Unity.Utilities;
	using Unity.Utilities.DebugTools;
	using UnityEditor.Callbacks;
	using System;

	public class MapboxConfigurationWindow : EditorWindow {

		public static MapboxConfigurationWindow instance;
		private static MapboxConfiguration _mapboxConfig;
		private static MapboxTokenStatus _currentTokenStatus = MapboxTokenStatus.StatusNotYetSet;
		private static MapboxAccess _mapboxAccess;
		private static bool _waitingToLoad = false;

		//default mapboxconfig
		private static string _configurationFile;
		private static string _accessToken = "";
		[Range(0, 1000)] private static int _memoryCacheSize = 500;
		[Range(0, 3000)] private static int _fileCacheSize = 25000;
		private static int _webRequestTimeout = 30;
		private static bool _autoRefreshCache = false;

		//mapbox access callbacks
		private static bool _listeningForTokenValidation = false;
		private static bool _validating = false;
		private static string _lastValidatedToken;

		//gui flags
		private bool _showConfigurationFoldout;
		private bool _showChangelogFoldout;
		private Vector2 _scrollPosition;

		//samples
		private static int _selectedSample;
		private static ScenesList _sceneList;
		private static GUIContent[] _sampleContent;
		private string _sceneToOpen;

		//prefabs
		private static int _selectedPrefab;
		private static ScenesList _prefabList;
		private static GUIContent[] _prefabContent;


		//styles
		private GUISkin _skin;
		private Color _defaultContentColor;
		private Color _defaultBackgroundColor;
		private GUIStyle _titleStyle;
		private GUIStyle _bodyStyle;
		private GUIStyle _linkStyle;

		private GUIStyle _textFieldStyle;
		private GUIStyle _submitButtonStyle;

		private GUIStyle _validButtonStyle;
		private Color _validContentColor;
		private Color _validBackgroundColor;

		private GUIStyle _invalidFieldStyle;
		private GUIStyle _invalidButtonStyle;
		private Color _invalidContentColor;
		private Color _invalidBackgroundColor;
		private GUIStyle _errorStyle;

		private GUIStyle _verticalGroup;
		private GUIStyle _horizontalGroup;
		private GUIStyle _scrollViewStyle;

		private GUIStyle _sampleButtonStyle;


		[DidReloadScripts]
		public static void ShowWindowOnImport() {
			if (ShouldShowConfigurationWindow()) {
				PlayerPrefs.SetInt(Constants.Path.DID_PROMPT_CONFIGURATION, 1);
				PlayerPrefs.Save();
				InitWhenLoaded();
			}
		}

		[MenuItem("Mapbox/Setup")]
		private static void InitWhenLoaded() {
			if (EditorApplication.isCompiling && !_waitingToLoad) {
				//subscribe to updates
				_waitingToLoad = true;
				EditorApplication.update += InitWhenLoaded;
				return;
			}

			if (!EditorApplication.isCompiling) {
				//unsubscribe from updates if waiting
				if (_waitingToLoad) {
					EditorApplication.update -= InitWhenLoaded;
					_waitingToLoad = false;
				}

				Init();
			}
		}

		private static void Init() {
			Runnable.EnableRunnableInEditor();

			//verify that the config file exists
			_configurationFile = Path.Combine(Constants.Path.MAPBOX_RESOURCES_ABSOLUTE, Constants.Path.CONFIG_FILE);

			if (!Directory.Exists(Constants.Path.MAPBOX_RESOURCES_ABSOLUTE)) {
				Directory.CreateDirectory(Constants.Path.MAPBOX_RESOURCES_ABSOLUTE);
			}

			if (!File.Exists(_configurationFile)) {
				_mapboxConfig = new MapboxConfiguration {
					AccessToken = _accessToken,
					MemoryCacheSize = (uint)_memoryCacheSize,
					FileCacheSize = (uint)_fileCacheSize,
					AutoRefreshCache = _autoRefreshCache,
					DefaultTimeout = _webRequestTimeout
				};
				string json = JsonUtility.ToJson(_mapboxConfig);
				File.WriteAllText(_configurationFile, json);
				AssetDatabase.Refresh();
			}

			//finish opening the window after the assetdatabase is refreshed.
			EditorApplication.delayCall += OpenWindow;
		}

		private static void OpenWindow() {
			EditorApplication.delayCall -= OpenWindow;

			//setup mapboxaccess listener
			_mapboxAccess = MapboxAccess.Instance;

			if (!_listeningForTokenValidation) {
				_mapboxAccess.OnTokenValidation += HandleValidationResponse;
				_listeningForTokenValidation = true;
			}

			//setup local variables from mapbox config file
			_mapboxConfig = _mapboxAccess.Configuration;

			if (_mapboxConfig != null) {
				_accessToken = _mapboxConfig.AccessToken;
				_memoryCacheSize = (int)_mapboxConfig.MemoryCacheSize;
				_fileCacheSize = (int)_mapboxConfig.FileCacheSize;
				_autoRefreshCache = _mapboxConfig.AutoRefreshCache;
				_webRequestTimeout = (int)_mapboxConfig.DefaultTimeout;

			}

			//validate current config
			if (!string.IsNullOrEmpty(_accessToken)) {
				SubmitConfiguration();
			}

			//cache sample scene gui content
			GetSceneList();
			_selectedSample = -1;

			//instantiate the config window
			instance = GetWindow(typeof(MapboxConfigurationWindow)) as MapboxConfigurationWindow;
			instance.minSize = new Vector2(800, 350);
			instance.titleContent = new GUIContent("Mapbox Setup");
			instance.Show();

		}

		private static void GetSceneList() {
			_prefabList = Resources.Load<ScenesList>("Mapbox/PrefabList");
			_sceneList = Resources.Load<ScenesList>("Mapbox/ScenesList");

			_prefabContent = LoadContent(_prefabList);
			_sampleContent = LoadContent(_sceneList);

		}

		private static GUIContent[] LoadContent(ScenesList list) {

			//exclude scenes with no image data
			List<SceneData> content = new List<SceneData>();

			if (list != null) {
				for (int i = 0; i < list.SceneList.Length; i++) {
					if (list.SceneList[i] != null) {
						if (File.Exists(list.SceneList[i].ScenePath)) {
							if (list.SceneList[i].Image != null) {
								content.Add(list.SceneList[i]);
							}
						}
					}
				}
			}

			GUIContent[] outputContent = new GUIContent[content.Count];

			for (int i = 0; i < outputContent.Length; i++) {
				outputContent[i] = new GUIContent(content[i].Name, content[i].Image, content[i].ScenePath);
			}

			return outputContent;

		}


		/// <summary>
		/// Unity Events
		/// </summary>
		private void OnDisable() {
			AssetDatabase.Refresh();
		}

		private void OnDestroy() {
			AssetDatabase.Refresh();
		}

		private void OnLostFocus() {
			AssetDatabase.Refresh();
		}


		/// <summary>
		/// Mapbox access
		/// </summary>
		private static void SubmitConfiguration() {
			MapboxConfiguration mapboxConfiguration = new MapboxConfiguration {
				AccessToken = _accessToken,
				MemoryCacheSize = (uint)_memoryCacheSize,
				FileCacheSize = (uint)_fileCacheSize,
				AutoRefreshCache = _autoRefreshCache,
				DefaultTimeout = _webRequestTimeout
			};
			_mapboxAccess.SetConfiguration(mapboxConfiguration, false);
			_validating = true;
		}

		private static void HandleValidationResponse(MapboxTokenStatus status) {
			_currentTokenStatus = status;
			_validating = false;
			_lastValidatedToken = _accessToken;

			//save the config
			_configurationFile = Path.Combine(Constants.Path.MAPBOX_RESOURCES_ABSOLUTE, Constants.Path.CONFIG_FILE);
			string json = JsonUtility.ToJson(_mapboxAccess.Configuration);
			File.WriteAllText(_configurationFile, json);
		}


		private void OnGUI() {
			//only run after init
			if (instance == null) {
				//TODO: loading message?
				InitWhenLoaded();
				return;
			}

			InitStyles();

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, _scrollViewStyle);
			EditorGUILayout.BeginVertical();
			// Access token link.
			DrawAccessTokenLink();

			// Access token entry and validation.
			DrawAccessTokenField();

			// Draw the validation error, if one exists
			DrawError();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(_verticalGroup);

			// Configuration
			DrawConfigurationSettings();
			GUILayout.Space(8);

			// Changelog
			DrawChangelog();

			EditorGUILayout.EndVertical();

			// Draw Prefab Examples
			if (_prefabContent.Length > 0) {
				EditorGUILayout.BeginVertical(_verticalGroup);
				DrawPrefabLinks();
				EditorGUILayout.EndVertical();

			}

			// Draw Example links if the scenelist asset is where it should be.
			if (_sampleContent.Length > 0) {
				EditorGUILayout.BeginVertical(_verticalGroup);
				DrawExampleLinks();
				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.EndScrollView();
		}

		private void InitStyles() {
			_defaultContentColor = GUI.contentColor;
			_defaultBackgroundColor = GUI.backgroundColor;

			_titleStyle = new GUIStyle(GUI.skin.FindStyle("IN TitleText"));
			_titleStyle.padding.left = 3;
			_bodyStyle = new GUIStyle(GUI.skin.FindStyle("WordWrapLabel"));
			_linkStyle = new GUIStyle(GUI.skin.FindStyle("PR PrefabLabel"));
			_linkStyle.padding.left = 0;
			_linkStyle.padding.top = -1;

			_textFieldStyle = new GUIStyle(GUI.skin.FindStyle("TextField"));
			_textFieldStyle.margin.right = 0;
			_textFieldStyle.margin.top = 0;

			_submitButtonStyle = new GUIStyle(GUI.skin.FindStyle("ButtonRight"));
			_submitButtonStyle.padding.top = 0;
			_submitButtonStyle.margin.top = 0;
			_submitButtonStyle.fixedWidth = 200;

			_validButtonStyle = new GUIStyle(GUI.skin.FindStyle("LODSliderRange"));
			_validButtonStyle.alignment = TextAnchor.MiddleCenter;
			_validButtonStyle.padding = new RectOffset(0, 0, 0, 0);
			_validButtonStyle.border = new RectOffset(0, 0, 5, -2);
			_validButtonStyle.fixedWidth = 60;

			_validContentColor = new Color(1, 1, 1, .7f);
			_validBackgroundColor = new Color(.2f, .8f, .2f, 1);
			_invalidContentColor = new Color(1, 1, 1, .7f);
			_invalidBackgroundColor = new Color(.8f, .2f, .2f, 1);

			_errorStyle = new GUIStyle(GUI.skin.FindStyle("ErrorLabel"));
			_errorStyle.padding.left = 5;

			_verticalGroup = new GUIStyle();
			_verticalGroup.margin = new RectOffset(0, 0, 0, 35);
			_horizontalGroup = new GUIStyle();
			_horizontalGroup.padding = new RectOffset(0, 0, 4, 4);
			_scrollViewStyle = new GUIStyle(GUI.skin.FindStyle("scrollview"));
			_scrollViewStyle.padding = new RectOffset(20, 20, 40, 0);

			_sampleButtonStyle = new GUIStyle(GUI.skin.FindStyle("button"));
			_sampleButtonStyle.imagePosition = ImagePosition.ImageAbove;
			_sampleButtonStyle.padding = new RectOffset(0, 0, 5, 5);
			_sampleButtonStyle.fontStyle = FontStyle.Bold;
		}

		private void DrawAccessTokenLink() {

			EditorGUILayout.LabelField("Access Token", _titleStyle);

			EditorGUILayout.BeginHorizontal(_horizontalGroup);

			if (string.IsNullOrEmpty(_accessToken)) {
				//fit box to text to create an 'inline link'
				GUIContent labelContent = new("Copy your free token from");
				GUIContent linkContent = new("mapbox.com");

				EditorGUILayout.LabelField(
					labelContent, _bodyStyle, GUILayout.Width(_bodyStyle.CalcSize(labelContent).x)
				);

				if (GUILayout.Button(linkContent, _linkStyle)) {
					Application.OpenURL("https://www.mapbox.com/studio/account/tokens/");
				}

				//create link cursor
				Rect rect = GUILayoutUtility.GetLastRect();
				rect.width = _linkStyle.CalcSize(new GUIContent(linkContent)).x;
				EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

				GUILayout.FlexibleSpace();

			}
			else {

				GUIContent labelContent = new("Manage your tokens at");
				GUIContent linkContent = new("mapbox.com/studio/accounts/tokens/");

				EditorGUILayout.LabelField(
					labelContent, _bodyStyle, GUILayout.Width(_bodyStyle.CalcSize(labelContent).x)
				);

				if (GUILayout.Button(linkContent, _linkStyle)) {
					Application.OpenURL("https://www.mapbox.com/studio/account/tokens/");
				}

				//create link cursor
				Rect rect = GUILayoutUtility.GetLastRect();
				rect.width = _linkStyle.CalcSize(new GUIContent(linkContent)).x;
				EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

				GUILayout.FlexibleSpace();

			}

			EditorGUILayout.EndHorizontal();

		}

		private void DrawAccessTokenField() {
			EditorGUILayout.BeginHorizontal(_horizontalGroup);

			//_accessToken is empty
			if (string.IsNullOrEmpty(_accessToken)) {
				_accessToken = EditorGUILayout.TextField("", _accessToken, _textFieldStyle);
				EditorGUI.BeginDisabledGroup(true);
				GUILayout.Button("Submit", _submitButtonStyle);
				EditorGUI.EndDisabledGroup();
			}
			else {
				//_accessToken is being validated
				if (_validating) {
					EditorGUI.BeginDisabledGroup(true);
					_accessToken = EditorGUILayout.TextField("", _accessToken, _textFieldStyle);
					GUILayout.Button("Checking", _submitButtonStyle);
					EditorGUI.EndDisabledGroup();
				}
				//_accessToken is the same as the already submitted token
				else if (string.Equals(_lastValidatedToken, _accessToken)) {
					//valid token
					if (_currentTokenStatus == MapboxTokenStatus.TokenValid) {
						GUI.backgroundColor = _validBackgroundColor;
						GUI.contentColor = _validContentColor;

						_accessToken = EditorGUILayout.TextField("", _accessToken, _textFieldStyle);
						GUILayout.Button("Valid", _validButtonStyle);

						GUI.contentColor = _defaultContentColor;
						GUI.backgroundColor = _defaultBackgroundColor;

					}
					//invalid token
					else {

						GUI.contentColor = _invalidContentColor;
						GUI.backgroundColor = _invalidBackgroundColor;

						_accessToken = EditorGUILayout.TextField("", _accessToken, _textFieldStyle);
						GUILayout.Button("Invalid", _validButtonStyle);

						GUI.contentColor = _defaultContentColor;
						GUI.backgroundColor = _defaultBackgroundColor;

					}

					//Submit button
					if (GUILayout.Button("Submit", _submitButtonStyle)) {
						SubmitConfiguration();
					}

				}
				//_accessToken is a new, unsubmitted token.
				else {
					_accessToken = EditorGUILayout.TextField("", _accessToken, _textFieldStyle);

					if (GUILayout.Button("Submit", _submitButtonStyle)) {
						SubmitConfiguration();
					}
				}
			}

			EditorGUILayout.EndHorizontal();

		}

		private void DrawError() {
			//draw the error message, if one exists
			EditorGUILayout.BeginHorizontal(_horizontalGroup);

			if (_currentTokenStatus != MapboxTokenStatus.TokenValid
			    && _currentTokenStatus != MapboxTokenStatus.StatusNotYetSet
			    && string.Equals(_lastValidatedToken, _accessToken)
			    && !_validating) {
				EditorGUILayout.LabelField(_currentTokenStatus.ToString(), _errorStyle);
			}

			EditorGUILayout.EndHorizontal();

		}

		private void DrawChangelog() {
			EditorGUI.indentLevel = 2;

			GUIContent linkContent = new("v" + Constants.SDK_VERSION + " changelog");

			if (GUILayout.Button(linkContent, _linkStyle)) {
				Application.OpenURL(
					"https://github.com/mapbox/mapbox-unity-sdk/blob/develop/documentation/docs/05-changelog.md"
				);
			}

			EditorGUI.indentLevel = 0;
		}

		private void DrawConfigurationSettings() {
			_showConfigurationFoldout = EditorGUILayout.Foldout(_showConfigurationFoldout, "Configuration", true);

			if (_showConfigurationFoldout) {
				EditorGUIUtility.labelWidth = 240f;
				EditorGUI.indentLevel = 2;
				_memoryCacheSize = EditorGUILayout.IntSlider("Mem Cache Size (# of tiles)", _memoryCacheSize, 0, 1000);
				_fileCacheSize = EditorGUILayout.IntSlider("File Cache Size (# of tiles)", _fileCacheSize, 0, 3000);
				_autoRefreshCache = EditorGUILayout.Toggle(
					new GUIContent(
						"Auto refresh cache",
						"Automatically update tiles in the local ambient cache if there is a newer version available online. ATTENTION: for every tile displayed (even a cached one) a webrequest needs to be made to check for updates."
					), _autoRefreshCache
				);
				_webRequestTimeout = EditorGUILayout.IntField("Default Web Request Timeout (s)", _webRequestTimeout);

				EditorGUILayout.BeginHorizontal(_horizontalGroup);
				GUILayout.Space(35f);

				if (GUILayout.Button("Save")) {
					SubmitConfiguration();
				}

				EditorGUI.indentLevel = 0;
				EditorGUIUtility.labelWidth = 0f;
				EditorGUILayout.EndHorizontal();
			}

		}

		private void DrawPrefabLinks() {
			EditorGUI.BeginDisabledGroup(
				_currentTokenStatus != MapboxTokenStatus.TokenValid
				|| _validating
			);

			if (_currentTokenStatus == MapboxTokenStatus.TokenValid) {
				EditorGUILayout.LabelField("Map Prefabs", _titleStyle);
			}
			else {
				EditorGUILayout.LabelField("Map Prefabs", "Paste your mapbox access token to get started", _titleStyle);
			}

			EditorGUILayout.BeginHorizontal(_horizontalGroup);
			EditorGUILayout.LabelField(
				"Choose a starting scene to see each location-based prefab in action, or go to the prefabs folder and add them to your existing scene.",
				_bodyStyle
			);
			EditorGUILayout.EndHorizontal();

			int rowCount = 4;
			EditorGUILayout.BeginHorizontal();

			_selectedPrefab = GUILayout.SelectionGrid(-1, _prefabContent, rowCount, _sampleButtonStyle);

			if (_selectedPrefab != -1) {
				EditorApplication.isPaused = false;
				EditorApplication.isPlaying = false;

				_sceneToOpen = _prefabContent[_selectedPrefab].tooltip;
				EditorApplication.update += OpenAndPlayScene;
			}

			EditorGUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();

		}

		private void DrawExampleLinks() {
			EditorGUI.BeginDisabledGroup(
				_currentTokenStatus != MapboxTokenStatus.TokenValid
				|| _validating
			);

			if (_currentTokenStatus == MapboxTokenStatus.TokenValid) {
				EditorGUILayout.LabelField("Example Scenes", _titleStyle);
			}
			else {
				EditorGUILayout.LabelField("Example Scenes", _titleStyle);
			}

			int rowCount = 4;
			EditorGUILayout.BeginHorizontal(_horizontalGroup);

			_selectedSample = GUILayout.SelectionGrid(-1, _sampleContent, rowCount, _sampleButtonStyle);

			if (_selectedSample != -1) {
				EditorApplication.isPaused = false;
				EditorApplication.isPlaying = false;

				_sceneToOpen = _sampleContent[_selectedSample].tooltip;
				EditorApplication.update += OpenAndPlayScene;
			}

			EditorGUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();
		}

		private void OpenAndPlayScene() {
			if (EditorApplication.isPlaying) {
				return;
			}

			EditorApplication.update -= OpenAndPlayScene;

			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
				EditorSceneManager.OpenScene(_sceneToOpen);
				EditorApplication.isPlaying = true;

				EditorWindow editorWindow = GetWindow(typeof(MapboxConfigurationWindow));
				editorWindow.Close();

			}
			else {
				_sceneToOpen = null;
				_selectedPrefab = -1;
				_selectedSample = -1;
			}
		}

		private static bool ShouldShowConfigurationWindow() {
			if (!PlayerPrefs.HasKey(Constants.Path.DID_PROMPT_CONFIGURATION)) {
				PlayerPrefs.SetInt(Constants.Path.DID_PROMPT_CONFIGURATION, 0);
			}

			return PlayerPrefs.GetInt(Constants.Path.DID_PROMPT_CONFIGURATION) == 0;
		}

	}

}