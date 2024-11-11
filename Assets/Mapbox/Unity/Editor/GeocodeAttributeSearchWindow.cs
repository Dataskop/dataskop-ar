namespace Mapbox.Editor {

	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections.Generic;
	using Geocoding;
	using Unity;
	using System.Globalization;
	using Mapbox.Unity.Map;
	using Editor;

	public class GeocodeAttributeSearchWindow : EditorWindow {

		private SerializedProperty _coordinateProperty;
		private object _objectToUpdate;

		private bool _updateAbstractMap;

		private string _searchInput = "";

		private ForwardGeocodeResource _resource;

		private List<Feature> _features;

		private Action<string> _callback;

		private const string searchFieldName = "searchField";
		private const float width = 320f;
		private const float height = 300f;

		private bool _isSearching = false;

		private void OnEnable() {
			_resource = new ForwardGeocodeResource("");
			EditorApplication.playModeStateChanged += OnModeChanged;
		}

		private void OnDisable() {
			EditorApplication.playModeStateChanged -= OnModeChanged;
		}

		private bool hasSetFocus = false;

		public static void Open(SerializedProperty property, object objectToUpdate = null) {
			GeocodeAttributeSearchWindow window = GetWindow<GeocodeAttributeSearchWindow>(true, "Search for location");

			window._coordinateProperty = property;

			if (objectToUpdate != null) {
				window._objectToUpdate = objectToUpdate;
			}

			Event e = Event.current;
			Vector2 mousePos = GUIUtility.GUIToScreenPoint(e.mousePosition);

			window.position = new Rect(mousePos.x - width, mousePos.y, width, height);
		}

		private void OnModeChanged(PlayModeStateChange state) {
			Close();
		}

		private void OnGUI() {
			GUILayout.Label("Search for a location");

			string oldSearchInput = _searchInput;

			GUI.SetNextControlName(searchFieldName);
			_searchInput = GUILayout.TextField(_searchInput);

			if (_searchInput.Length == 0) {
				GUILayout.Label("Type in a location to find it's latitude and longtitude");
			}
			else {
				bool changed = oldSearchInput != _searchInput;

				if (changed) {
					HandleUserInput(_searchInput);
				}

				if (_features.Count > 0) {
					GUILayout.Label("Results:");

					for (int i = 0; i < _features.Count; i++) {
						Feature feature = _features[i];
						string coordinates = feature.Center.x.ToString(CultureInfo.InvariantCulture) + ", " +
						                     feature.Center.y.ToString(CultureInfo.InvariantCulture);

						//abreviated coords for display in the UI
						string truncatedCoordinates =
							feature.Center.x.ToString("F2", CultureInfo.InvariantCulture) + ", " +
							feature.Center.y.ToString("F2", CultureInfo.InvariantCulture);

						//split feature name and add elements until the maxButtonContentLenght is exceeded
						string[] featureNameSplit = feature.PlaceName.Split(',');
						string buttonContent = "";
						int maxButtonContentLength = 30;

						for (int j = 0; j < featureNameSplit.Length; j++) {
							if (buttonContent.Length + featureNameSplit[j].Length < maxButtonContentLength) {
								if (string.IsNullOrEmpty(buttonContent)) {
									buttonContent = featureNameSplit[j];
								}
								else {
									buttonContent = buttonContent + "," + featureNameSplit[j];
								}
							}
						}

						if (buttonContent.Length < maxButtonContentLength + 15) {
							buttonContent = buttonContent + "," + " (" + truncatedCoordinates + ")";
						}

						if (GUILayout.Button(buttonContent)) {
							_coordinateProperty.stringValue = coordinates;
							_coordinateProperty.serializedObject.ApplyModifiedProperties();
							EditorUtility.SetDirty(_coordinateProperty.serializedObject.targetObject);

							if (_objectToUpdate != null) {
								EditorHelper.CheckForModifiedProperty(_coordinateProperty, _objectToUpdate, true);
							}

							Close();
						}
					}
				}
				else {
					if (_isSearching) {
						GUILayout.Label("Searching...");
					}
					else {
						GUILayout.Label("No search results");
					}
				}
			}

			if (!hasSetFocus) {
				GUI.FocusControl(searchFieldName);
				hasSetFocus = true;
			}
		}

		private void HandleUserInput(string searchString) {
			_features = new List<Feature>();
			_isSearching = true;

			if (!string.IsNullOrEmpty(searchString)) {
				_resource.Query = searchString;
				MapboxAccess.Instance.Geocoder.Geocode(_resource, HandleGeocoderResponse);
			}
		}

		private void HandleGeocoderResponse(ForwardGeocodeResponse res) {
			//null if no internet connection
			if (res != null) {
				//null if invalid token
				if (res.Features != null) {
					_features = res.Features;
				}
			}

			_isSearching = false;
			Repaint();
		}

	}

}